WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Configure PORT
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://*:{port}");

// Ensure directories BEFORE configuring database
EnsureDirectoriesExist();
Console.WriteLine("Directories created");

// Configure database BEFORE Umbraco builder
string dbPath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "Umbraco.sqlite.db");
builder.Configuration["ConnectionStrings:umbracoDbDSN"] = $"Data Source={dbPath};Cache=Shared;Foreign Keys=True;Pooling=True";
Console.WriteLine($"Using database at: {dbPath}");

// Create Umbraco builder
builder.CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()
    .AddComposers()
    .Build();

// Build app
var app = builder.Build();

// Error handling first
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
}

// CORS
app.UseCors("AllowStaticSite");

// Health endpoint
app.MapGet("/health", () => {
    return "Application is running!";
});

// Diagnostic middleware
app.Use(async (context, next) => {
    Console.WriteLine($"Request received: {context.Request.Method} {context.Request.Path}");
    try {
        await next();
    }
    catch (Exception ex) {
        Console.WriteLine($"ERROR: {ex.Message}");
        throw;
    }
    Console.WriteLine($"Response status: {context.Response.StatusCode}");
});

// Boot Umbraco
await app.BootUmbracoAsync();

// Configure Umbraco
app.UseUmbraco()
    .WithMiddleware(u => {
        u.UseBackOffice();
        u.UseWebsite();
    })
    .WithEndpoints(u => {
        u.UseBackOfficeEndpoints();
        u.UseWebsiteEndpoints();
    });

await app.RunAsync();
// Helper method to ensure directories exist
static void EnsureDirectoriesExist()
{
    var requiredDirectories = new[]
    {
        "wwwroot/media",
        "wwwroot/css",
        "wwwroot/js",
        "App_Data/TEMP",
        "App_Plugins"
    };

    foreach (var dir in requiredDirectories)
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), dir);
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            Console.WriteLine($"Created directory: {path}");
        }
    }
}