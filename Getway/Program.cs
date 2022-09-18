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

builder.Services.AddOcelot(builder.Configuration).AddPolly();

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

app.UseCors("CorsPolicy");

app.Run();
