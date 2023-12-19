var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

builder.Services.AddResponseCaching();

builder.Services.AddMemoryCache();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// Use HTTPS Strict Transport Security (HSTS)
app.UseHsts();

// Use Content Security Policy (CSP)
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'");

    context.Response.OnStarting(() =>
    {
        context.Response.Headers.Remove("server");
        context.Response.Headers.Remove("x-powered-by");
        return Task.CompletedTask;
    });

    context.Request.Scheme = "https"; // Set the scheme to https

    await next();
});

// Use CORS
app.UseCors(policy =>
{
    policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.UseResponseCaching();
app.MapControllers();
app.Run();