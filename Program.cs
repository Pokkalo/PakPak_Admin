WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

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
