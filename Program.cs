WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()
    .AddComposers()
    .Build();

WebApplication app = builder.Build();

// Add before app.UseUmbraco()
app.MapGet("/healthz", () => {
    try {
        // Just basic functionality check, no Umbraco dependency
        Console.WriteLine("Health endpoint accessed");
        return Results.Ok("Healthy");
    }
    catch (Exception ex) {
        Console.WriteLine($"Health check failed: {ex.Message}");
        return Results.StatusCode(500);
    }
});

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
