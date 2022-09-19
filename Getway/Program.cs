using Ocelot.Cache.CacheManager;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Polly;

var builder = WebApplication.CreateBuilder(args);

var routes = "Routes";
builder.Host.ConfigureAppConfiguration((hostingContext, config) =>
{
    config
    .SetBasePath(hostingContext.HostingEnvironment.ContentRootPath)
    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
});

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
    .AddOcelot(routes, builder.Environment)
    .AddEnvironmentVariables();

//builder.Services.AddOcelot();

builder
    .Services
    .AddOcelot(builder.Configuration)
    .AddPolly()
    .AddCacheManager(m =>
    {
        m.WithDictionaryHandle();
    });

builder.Services.AddCors(options => options.AddPolicy("CorsPolicy",
     builder =>
     {
         builder
         .AllowAnyOrigin()
         .AllowAnyMethod()
         .AllowAnyHeader()
         .SetIsOriginAllowed((host) => true);
     }));

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await app.UseOcelot();

app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Codepedia-Custom-Header-Response", "Satinder singh");

    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");

    context.Response.Headers.Add("X-Frame-Options", "DENY");

    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");

    context.Response.Headers.Remove("server");

    context.Response.Headers.Remove("X-AspNet-Version");

    context.Response.Headers.Remove("X-Powered-By");

    await next();
});


app.UseCors("CorsPolicy");

app.Run();
