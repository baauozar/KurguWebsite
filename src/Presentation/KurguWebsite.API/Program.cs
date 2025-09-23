using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using KurguWebsite.API.Extensions;
using KurguWebsite.Application.Features.DependencyInjection;
using KurguWebsite.Infrastructure;
using KurguWebsite.Infrastructure.Middleware;
using KurguWebsite.Persistence;
using KurguWebsite.Persistence.Context;
using KurguWebsite.Persistence.Seed;
using KurguWebsite.WebAPI.Filters;
using KurguWebsite.WebAPI.Helpers;
using KurguWebsite.WebAPI.Middleware;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using System.Text.Json.Serialization;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/kurguwebsite-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        fileSizeLimitBytes: 10_485_760,
        rollOnFileSizeLimit: true)
    .CreateLogger();

try
{
    Log.Information("Starting KurguWebsite API");

    var builder = WebApplication.CreateBuilder(args);
  /*  builder.Services.AddRouting(options =>
    {
        options.LowercaseUrls = true;
    });*/
    // Use Serilog
    builder.Host.UseSerilog();
    builder.WebHost.CaptureStartupErrors(true)
        .UseSetting("detailedErrors", "true");

    // Add services
    ConfigureServices(builder.Services, builder.Configuration);

    var app = builder.Build();

    // Configure pipeline
    await ConfigureAsync(app, app.Environment);

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// -------------------- Services --------------------
void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    // Add Application, Infrastructure, Persistence layers
    services.AddApplication();
    services.AddInfrastructure(configuration);
    services.AddPersistence(configuration);

    // Controllers with filters
    services.AddControllers(options =>
    {
        options.Filters.Add(new ProducesAttribute("application/json"));
        options.Filters.Add<ValidationFilter>();
        options.Filters.Add<ApiExceptionFilter>();
        options.ReturnHttpNotAcceptable = true;
        options.RespectBrowserAcceptHeader = true;
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .SelectMany(e => e.Value.Errors.Select(x => new
                {
                    Field = e.Key,
                    Message = x.ErrorMessage
                }));

            return new BadRequestObjectResult(new
            {
                Message = "Validation failed",
                Errors = errors
            });
        };
    });

    // API Versioning + Swagger
    // API Versioning + Swagger
    services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader = ApiVersionReader.Combine(
            new UrlSegmentApiVersionReader(),
            new HeaderApiVersionReader("X-Api-Version"));
    })
    .AddMvc().AddApiExplorer(options =>
    {
        // This is where the options from AddVersionedApiExplorer moved to
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

    services.AddEndpointsApiExplorer();
    services.ConfigureSwagger();

    // Health Checks
    services.AddHealthChecks()
        .AddDbContextCheck<KurguWebsiteDbContext>("Database")
        .AddCheck<CustomHealthCheck>("Custom");

    // Response compression & caching
    services.AddResponseCompression(options => options.EnableForHttps = true);
    services.AddResponseCaching();

    // Other essentials
    services.AddHttpContextAccessor();
    services.AddDataProtection();
    services.AddAntiforgery(options => options.HeaderName = "X-CSRF-TOKEN");

    // AutoMapper
    services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
}

// -------------------- Configure Pipeline --------------------
async Task ConfigureAsync(WebApplication app, IWebHostEnvironment env)
{
    app.UseMiddleware<GlobalExceptionMiddleware>();
    app.UseSecurityHeaders(); // customize your options
    app.UseAntiXss();
    app.UseSerilogRequestLogging();

    if (!env.IsDevelopment())
    {
        app.UseHsts();
        app.UseHttpsRedirection();
    }

    app.UseResponseCompression();
    app.UseResponseCaching();

    app.UseStaticFiles();

    app.UseCors(env.IsDevelopment() ? "Development" : "Production");

    app.UseRouting();

    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();

    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    if (env.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            foreach (var desc in provider.ApiVersionDescriptions)
            {
                options.SwaggerEndpoint($"/swagger/{desc.GroupName}/swagger.json",
                    $"Kurgu Website API {desc.GroupName}");
            }
            options.RoutePrefix = string.Empty;
        });
    }

    app.MapControllers();
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
    });

    // --------- Seed database in Development ---------
    if (env.IsDevelopment())
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<KurguWebsiteDbContext>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        var connString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connString))
            throw new InvalidOperationException("DefaultConnection is not set in appsettings.json");

        await context.Database.EnsureCreatedAsync();
        await context.Database.MigrateAsync();

        await DatabaseSeeder.SeedAsync(scope.ServiceProvider);
    }

    Log.Information("Application started successfully");
}

public partial class Program { }
