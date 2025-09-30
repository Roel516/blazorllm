using AspNetCoreRateLimit;
using MyBlazorApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configure API controllers with JSON options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressModelStateInvalidFilter = false;
    });

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient",
        policy =>
        {
            var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
                ?? new[] { "http://localhost:5000" };
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
});

// Configure HttpClients with proper timeouts and headers
builder.Services.AddHttpClient("HuggingFaceClient", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("User-Agent", "MyBlazorApp/1.0");
})
.ConfigureHttpClient((serviceProvider, client) =>
{
    var config = serviceProvider.GetRequiredService<IConfiguration>();
    var apiKey = config["HuggingFace:ApiKey"];
    if (!string.IsNullOrEmpty(apiKey))
    {
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
    }
})
.SetHandlerLifetime(TimeSpan.FromMinutes(5));

// Separate client for image generation with longer timeout
builder.Services.AddHttpClient("ImageGenerationClient", client =>
{
    client.Timeout = TimeSpan.FromMinutes(2);
    client.DefaultRequestHeaders.Add("User-Agent", "MyBlazorApp/1.0");
})
.ConfigureHttpClient((serviceProvider, client) =>
{
    var config = serviceProvider.GetRequiredService<IConfiguration>();
    var apiKey = config["HuggingFace:ApiKey"];
    if (!string.IsNullOrEmpty(apiKey))
    {
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
    }
})
.SetHandlerLifetime(TimeSpan.FromMinutes(5));

// Configure rate limiting
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// Configure ApiConfiguration
builder.Services.Configure<ApiConfiguration>(
    builder.Configuration.GetSection("ApiSettings"));

// Add health checks
builder.Services.AddHealthChecks();

// Add response compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Add request logging middleware
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

    if (context.Request.Path.StartsWithSegments("/api"))
    {
        logger.LogInformation("API Request: {Method} {Path} from {IpAddress}",
            context.Request.Method,
            context.Request.Path,
            context.Connection.RemoteIpAddress);
    }

    await next();
});

app.UseResponseCompression();
// app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.UseCors("AllowBlazorClient");
app.UseIpRateLimiting();

app.MapRazorComponents<MyBlazorApp.Components.App>()
    .AddInteractiveServerRenderMode();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
