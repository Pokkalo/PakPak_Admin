# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["PakPak_Admin.csproj", "./"]
RUN dotnet restore "PakPak_Admin.csproj"

# Copy all source code
COPY . .

# Build the application
RUN dotnet build "PakPak_Admin.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "PakPak_Admin.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Set proper permissions for Umbraco
USER root

# Create required Umbraco directories with appropriate permissions
RUN mkdir -p /app/wwwroot/media /app/wwwroot/css /app/wwwroot/js /app/App_Data/TEMP /app/App_Plugins

# Copy published files from build stage
COPY --from=publish /app/publish .

# Ensure necessary directories have correct permissions
RUN chmod -R 777 /app/wwwroot/media /app/App_Data

# Configure environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV UMBRACO_CMS_GLOBAL__INSTALLMISSINGDATABASE=true

# Set DataDirectory path for SQLite
ENV DataDirectory=/app/App_Data

# Expose port
EXPOSE 80

# Set healthcheck
HEALTHCHECK --interval=30s --timeout=30s --start-period=30s --retries=3 \
  CMD curl -f http://localhost:80/ || exit 1

# Start the application
ENTRYPOINT ["dotnet", "PakPak_Admin.dll"]