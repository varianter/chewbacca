using ApplicationCore.Interfaces;
using ApplicationCore.Services;

using Azure.Identity;

using CronScheduler.Extensions.Scheduler;

using Infrastructure;
using Infrastructure.ApiClients;
using Infrastructure.Repositories;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using Refit;

using Shared;
using Shared.AzureIdentity;

using Swashbuckle.AspNetCore.Filters;

using Web;

var builder = WebApplication.CreateBuilder(args);

var isDevelopment = builder.Environment.EnvironmentName.Equals("Development");
builder.Services.AddCors(options =>
{
    if (isDevelopment)
    {
        options.AddPolicy("DashCorsPolicy",
            policy =>
            {
                policy.AllowAnyMethod().AllowAnyHeader()
                    .WithOrigins("https://dash.variant.no", "https://variantdash-dev.azurewebsites.net", "https://dash.dev.variant.dev",
                        "http://localhost:3000");
            });
    }
    else
    {
        {
            options.AddPolicy("DashCorsPolicy",
                policy =>
                {
                    policy.AllowAnyMethod().AllowAnyHeader().WithOrigins("https://dash.variant.no","https://dash.dev.variant.dev",
                        "https://variantdash-dev.azurewebsites.net");
                });
        }
    }
});

// Add services to the container.

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(
    c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Chewbacca", Version = "v1" });
        c.OperationFilter<AddResponseHeadersFilter>(); // [SwaggerResponseHeader]

        c.OperationFilter<AppendAuthorizeToSummaryOperationFilter>(); // Adds "(Auth)" to the summary so that you can see which endpoints have Authorization
        c.OperationFilter<SecurityRequirementsOperationFilter>();
        c.AddSecurityDefinition("oauth2",
            new OpenApiSecurityScheme
            {
                Description = "Standard Authorization header using the Bearer scheme. Example: \"bearer {token}\"",
                In = ParameterLocation.Header,
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey
            });
        var filePath = Path.Combine(AppContext.BaseDirectory, "Web.xml");
        c.IncludeXmlComments(filePath);
    });

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    // appsettings.Local.json is in the .gitignore. Using a local config instead of userSecrets to avoid references in the .csproj:
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddFeatureManagement();

builder.Services.AddScoped<BlobStorageService>();
builder.Services.AddScoped<IBlobStorageRepository, BlobStorageRepository>();

builder.Services.AddScoped<IEmergencyContactRepository, EmergencyContactRepository>();
builder.Services.AddScoped<EmergencyContactRepository>();

builder.Services
    .AddScoped<IEmployeeAllergiesAndDietaryPreferencesRepository, EmployeeAllergiesAndDietaryPreferencesRepository>();
builder.Services.AddScoped<EmployeeAllergiesAndDietaryPreferencesRepository>();

builder.Services.AddScoped<EmployeesService>();

builder.Services.AddScoped<IEmployeesRepository, EmployeesRepository>();
builder.Services.AddScoped<EmployeesRepository>();

builder.Services.AddScoped<FilteredUids>();
builder.Services.AddScoped<CvPartnerRepository>();
builder.Services.AddScoped<IVibesRepository, VibesRepository>();

// Bind configuration "TestApp:Settings" section to the Settings object
var initialAppSettings = builder.Configuration.GetSection("AppSettings").Get<AppSettings>();
if (initialAppSettings == null) throw new Exception("Unable to load app settings");

// ApplicationCore
builder.Services.AddScoped<OrchestratorService>();
if (initialAppSettings.UseAzureAppConfig)
{
    /*
     * Free Tier of Azure App Configuration has a limit of 1,000 requests per day, so be careful with the number of requests.
     * Feature Flags does not have a Sentinel feature, so it results in one request per feature flag.
     */
    builder.Services.AddAzureAppConfiguration();
    // Load configuration from Azure App Configuration
    builder.Configuration.AddAzureAppConfiguration(options => options
        .ConfigureRefresh(refreshOptions =>
        {
            refreshOptions.Register("Sentinel", refreshAll: true)
                .SetCacheExpiration(TimeSpan.FromMinutes(10));
        })
        .UseFeatureFlags(options =>
        {
            // See comment above about the number of requests
            options.CacheExpirationInterval = TimeSpan.FromHours(12);
        })
        .Connect(initialAppSettings.AzureAppConfigUri, new DefaultAzureCredential()).ConfigureKeyVault(
            vaultOptions =>
            {
                vaultOptions.SetCredential(new DefaultAzureCredential());
            }));
}

initialAppSettings = builder.Configuration.GetSection("AppSettings").Get<AppSettings>();

// Refit
builder.Services.AddRefitClient<ICvPartnerApiClient>()
    .ConfigureHttpClient(c => c.BaseAddress = initialAppSettings.CvPartner.Uri);
builder.Services.AddRefitClient<IVibesApiClient>()
    .ConfigureHttpClient(c => c.BaseAddress = initialAppSettings.Vibes.BaseUri)
    .AddHttpMessageHandler(() =>
        new RefitBearerTokenHandler(new ClientSecretCredential(initialAppSettings.Identity.TenantId,
                initialAppSettings.Identity.ClientId,
                initialAppSettings.Identity.ClientSecret)
            , initialAppSettings.Vibes.Scope));

builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

builder.Services.AddDbContextPool<EmployeeContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("EmployeeDatabase"), sqlOptions =>
    {
        sqlOptions.CommandTimeout(45);
    });
    options.AddInterceptors(new AzureAdAuthenticationDbConnectionInterceptor());
});

builder.Services.AddScheduler(ctx =>
{
    const string jobName = nameof(OrchestratorJob);
    ctx.AddJob(
        sp =>
        {
            var options = sp.GetRequiredService<IOptionsMonitor<SchedulerOptions>>().Get(jobName);
            return new OrchestratorJob(sp, options);
        },
        options =>
        {
            options.CronSchedule = "0 4 * * *";
            options.RunImmediately = false;
        },
        jobName: jobName);
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var variantTenantId = initialAppSettings.Identity.TenantId;
        options.Authority = $"https://login.microsoftonline.com/{variantTenantId}/v2.0/";
        options.Audience = "api://chewbacca";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ClockSkew = TimeSpan.FromMinutes(5),
            RequireSignedTokens = true,
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidIssuers = new[]
            {
                $"https://login.microsoftonline.com/{variantTenantId}/v2.0",
                $"https://sts.windows.net/{variantTenantId}/"
            },
            ValidateAudience = true,
            ValidateLifetime = true
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseCors();

/*
 * Migrate the database.
 * Ideally the app shouldn't have access to alter the database schema, but we do it for simplicity's sake,
 * both here and in the bicep/infrastructure-as-code.
 */
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EmployeeContext>();
    var isInMemoryDatabase = db.Database.ProviderName?.StartsWith("Microsoft.EntityFrameworkCore.InMemory") ?? false;

    if (isInMemoryDatabase)
    {
        db.Database.EnsureCreated();
    }
    else
    {
        db.Database.Migrate();
    }
}

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

if (initialAppSettings.UseAzureAppConfig)
{
    app.UseAzureAppConfiguration();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "Hello World!");
app.MapControllers();

// Example of Minimal API instead of using Controllers
app.MapGet("/healthcheck",
    async ([FromServices] EmployeeContext db, [FromServices] IFeatureManager featureManager, [FromServices] IOptionsSnapshot<AppSettings> appSettings) =>
    {
        app.Logger.LogInformation("Getting employees from database");

        var dbCanConnect = await db.Database.CanConnectAsync();
        var healthcheck = appSettings.Value.Healthcheck;
        
        var featuresFromAppCfg = await featureManager.IsEnabledAsync("ReadsFeatureManagementFromAppConfig");
        var response = new HealthcheckResponse
        {
            Database = dbCanConnect, KeyVault = healthcheck.KeyVault, AppConfig = healthcheck.AppConfig, FeatureManagement = featuresFromAppCfg
        };

        return response;
    });

app.Run();

// Needed for testing:
namespace Web
{
    public partial class Program
    {
    }

}

