using MMLib.SwaggerForOcelot.DependencyInjection;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Polly;
using Ubitex.Travel.Base;

var builder = WebApplication.CreateBuilder(args);

var routes = "Routes";

builder.Configuration.AddOcelotWithSwaggerSupport(options =>
{
    options.Folder = routes;
});

builder.Host.ConfigureAppConfiguration((hostingContext, config) =>
{
    config
    .SetBasePath(hostingContext.HostingEnvironment.ContentRootPath)
    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
});

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

//builder.Services.AddSwaggerGen();

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
    .AddOcelot(routes, builder.Environment)
    .AddEnvironmentVariables();

//builder.Services.AddOcelot();

builder.Services.AddOcelot(builder.Configuration).AddPolly();

//builder.Services.AddSwaggerForOcelot(builder.Configuration);

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

//app.UseSwagger();

//app.UseSwaggerUI(c =>
//{
//    c.SwaggerEndpoint("/client/swagger/v1/swagger.json", "Client");
//    c.SwaggerEndpoint("/hotel/swagger/v1/swagger.json", "Hotel");
//    c.SwaggerEndpoint("/flight/swagger/v1/swagger.json", "Flight");
//    //c.RoutePrefix = "docs";
//});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

//app.UseSwaggerForOcelotUI(options =>
//{
//    options.PathToSwaggerGenerator = "/swagger/docs";
//    options.ReConfigureUpstreamSwaggerJson = AlterUpstream.AlterUpstreamSwaggerJson;
//});

await app.UseOcelot();

//app.UseSwaggerForOcelotUI(options =>
//{
//    options.PathToSwaggerGenerator = "/swagger/docs";
//    options.ReConfigureUpstreamSwaggerJson = AlterUpstream.AlterUpstreamSwaggerJson;

//}).UseOcelot().Wait();

app.UseCors("CorsPolicy");

app.Run();
