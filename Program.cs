WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add this near the top of your file to use Railway's PORT environment variable
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://*:{port}");

Console.WriteLine("Application starting...");

// Ensure directories exist
EnsureDirectoriesExist();
Console.WriteLine("Directories created");

builder.CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()
    .AddComposers()
    .Build();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowStaticSite", builder =>
    {
        builder.WithOrigins("https://yourusername.github.io")
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

WebApplication app = builder.Build();

string dbPath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "Umbraco.sqlite.db");
Console.WriteLine($"Using database at: {dbPath}");
builder.Configuration["ConnectionStrings:umbracoDbDSN"] = $"Data Source={dbPath};Cache=Shared;Foreign Keys=True;Pooling=True";

// And in the Configure section
app.UseCors("AllowStaticSite");
await app.BootUmbracoAsync();

app.MapGet("/health", () => {
    Console.WriteLine("Health endpoint accessed!");
    return "Application is running!";
});

app.Use(async (context, next) => {
    Console.WriteLine($"Request received: {context.Request.Method} {context.Request.Path}");
    await next();
    Console.WriteLine($"Response status: {context.Response.StatusCode}");
});

app.UseUmbraco()
    .WithMiddleware(u =>
    {
        u.UseBackOffice();
        u.UseWebsite();
    })
    .WithEndpoints(u =>
    {
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