FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

ENV UMBRACO__CMS__GLOBAL__DATABASEFACTORY__CONFIGUREDATABASETYPE=SQLite
ENV CONNECTIONSTRINGS__UMBRACODBDSN="Data Source=/app/App_Data/Umbraco.sqlite.db;Cache=Shared;Foreign Keys=True;Pooling=True"
ENV CONNECTIONSTRINGS__UMBRACODBDSN_PROVIDERNAME="Microsoft.Data.Sqlite"

# Add to your environment variables
ENV UMBRACO__CMS__HOSTING__DEBUG=true
ENV ASPNETCORE_LOGGING__CONSOLE__DISABLECOLORS=true
ENV DOTNET_gcServer=0
ENV DOTNET_gcConcurrent=0

# Add to end of Dockerfile before ENTRYPOINT:
RUN echo "Content of App_Data:" && ls -la /app/App_Data

# Copy project files for better layer caching
COPY ["PakPak_Admin.csproj", "./"]
RUN dotnet restore "PakPak_Admin.csproj"

# Copy remaining files
COPY . .
RUN dotnet build "PakPak_Admin.csproj" -c Release -o /app/build
RUN dotnet publish "PakPak_Admin.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Install curl for healthchecks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Create required directories
RUN mkdir -p /app/wwwroot/media /app/wwwroot/css /app/wwwroot/js /app/App_Data/TEMP /app/App_Plugins
COPY --from=build /app/publish .
RUN chmod -R 777 /app/wwwroot/media /app/App_Data

# Railway specific configuration
ENV PORT=8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV UMBRACO_CMS_GLOBAL__INSTALLMISSINGDATABASE=true
ENV DataDirectory=/app/App_Data
ENV UMBRACO__CMS__HOSTING__DEBUG=false
ENV UMBRACO__CMS__HOSTING__LOCALHOSTREDIRECTENABLED=false

EXPOSE 8080

# Add simple healthcheck
HEALTHCHECK --interval=30s --timeout=30s --start-period=60s --retries=3 \
  CMD curl -f http://localhost:8080/healthz || exit 1

ENTRYPOINT ["dotnet", "PakPak_Admin.dll"]