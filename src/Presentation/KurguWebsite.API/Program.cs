using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using KurguWebsite.API.Extensions;
using KurguWebsite.API.Filters;
using KurguWebsite.API.Helpers;
using KurguWebsite.API.Middleware;
using KurguWebsite.Application.Features.DependencyInjection;
using KurguWebsite.Infrastructure;
using KurguWebsite.Infrastructure.Identity;
using KurguWebsite.Infrastructure.Middleware;
using KurguWebsite.Persistence;
using KurguWebsite.Persistence.Context;
using KurguWebsite.Persistence.Seed;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using System.Reflection;
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
    builder.Services.AddRouting(options =>
    {
        options.LowercaseUrls = true;
    });

    builder.Host.UseSerilog();
    builder.WebHost.CaptureStartupErrors(true)
        .UseSetting("detailedErrors", "true");

    ConfigureServices(builder.Services, builder.Configuration);

    var app = builder.Build();

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
    // *** THE FIX IS HERE ***
    // We now register Persistence first, so that the Infrastructure layer's 
    // explicit JWT authentication setup can override the default Identity settings.
    services.AddPersistence(configuration);
    services.AddApplication();
    services.AddInfrastructure(configuration);
/*    services.AddPermissionPolicies(); */

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
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

    services.AddEndpointsApiExplorer();
    services.ConfigureSwagger();

    services.AddHealthChecks()
        .AddDbContextCheck<KurguWebsiteDbContext>("Database")
        .AddCheck<CustomHealthCheck>("Custom");

    services.AddResponseCompression(options => options.EnableForHttps = true);
    services.AddResponseCaching();

    services.AddHttpContextAccessor();
    services.AddDataProtection();
    services.AddAntiforgery(options => options.HeaderName = "X-CSRF-TOKEN");

    services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
}

// -------------------- Configure Pipeline --------------------
async Task ConfigureAsync(WebApplication app, IWebHostEnvironment env)
{
    app.UseMiddleware<GlobalExceptionMiddleware>();
    app.UseSecurityHeaders();
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
            options.RoutePrefix = "swagger";
        });
    }

    app.MapControllers();
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
    });

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

