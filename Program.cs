WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Ensure directories exist
EnsureDirectoriesExist();

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

// And in the Configure section
app.UseCors("AllowStaticSite");
await app.BootUmbracoAsync();

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