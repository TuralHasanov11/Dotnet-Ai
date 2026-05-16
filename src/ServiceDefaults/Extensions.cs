using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Compliance.Classification;
using Microsoft.Extensions.Compliance.Redaction;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using ServiceDefaults.Errors;
using ServiceDefaults.Identity;
using ServiceDefaults.Monitoring;
using ServiceDefaults.Performance;
using ServiceDefaults.Security;
using SharedKernel.Compliance;
using SharedKernel.Identity;

namespace ServiceDefaults;

[ExcludeFromCodeCoverage]
public static class Extensions
{
    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.ConfigureOpenTelemetry();

        ConfigureResiliency(builder);

        builder.Services.AddOutputCache(
            configureOptions: static caching =>
                caching.AddPolicy("HealthChecks",
                build: static policy => policy.Expire(TimeSpan.FromSeconds(10))));

        builder.AddDefaultHealthChecks();

        // TODO: Add service discovery

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // Turn on resilience by default
            http.AddStandardResilienceHandler();

            // Turn on service discovery by default
        });

        builder.Services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";

                context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);

                var activity = context.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity;
                context.ProblemDetails.Extensions.TryAdd("traceId", activity?.Id);
            };
        });

        builder.Services.AddExceptionHandler<ProblemExceptionHandler>();

        builder.Services.Configure<HostOptions>(builder.Configuration.GetSection("Host"));

        builder.Services.AddHsts(options =>
        {
            options.Preload = true;
            options.IncludeSubDomains = true;
        });

        builder.Services.AddScoped<ContentTypeOptionsMiddleware>();

        builder.Services.AddRedaction(options =>
        {
            // EUP: HMAC redactor
            options.SetHmacRedactor(r =>
            {
                r.KeyId = 123456789;
                r.Key = Convert.ToBase64String(Encoding.UTF8.GetBytes("uVtXrJ3k5g5p7+Xl5f8uVtXrJ3k5g5p7+Xl5f8uVtXrJ3k5g5p7+Xl5f8="));
            }, LoggingTaxonomyDefinitions.EUPDataClassification);

            // EUII: Secret redactor
            options.SetRedactor<SecretRedactor>(new DataClassificationSet(LoggingTaxonomyDefinitions.EUIIDataClassification));

            // CustomerData: Erasing redactor
            options.SetRedactor<ErasingRedactor>(new DataClassificationSet(LoggingTaxonomyDefinitions.CustomerDataClassification));

            // AdministratorData: Erasing redactor
            options.SetRedactor<ErasingRedactor>(new DataClassificationSet(LoggingTaxonomyDefinitions.AdministratorDataClassification));

            // FeedbackData: Erasing redactor
            options.SetRedactor<ErasingRedactor>(new DataClassificationSet(LoggingTaxonomyDefinitions.FeedbackDataClassification));
        });

        ConfigureIdentity(builder);

        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;
            options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
            options.SerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower;
            options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.SerializerOptions.WriteIndented = false;
            options.SerializerOptions.Encoder = JavaScriptEncoder.Default;
            options.SerializerOptions.AllowTrailingCommas = true;
            options.SerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;
        });

        builder.Services.AddApiVersioning(options =>
        {
            options.ApiVersionReader = new HeaderApiVersionReader("X-API-Version");
            options.ReportApiVersions = true;
        }).AddApiExplorer(options =>
        {
            // Calling "AddApiExplorer" is required for OpenAPI versioning to work correctly.
            // Without this, the generated OpenAPI documents will not be versioned.

            // GroupNameFormat specifies the format of the API version.
            // Without this, versioning will use the literal group names. In our case, that would be 1.0.
            // For compatibility with the "default" /openapi/v1.json behavior from Microsoft.AspNetCore.OpenApi, we use v'VVV' so we can retrieve it using v1.json.
            // See https://github.com/dotnet/aspnet-api-versioning/wiki/Version-Format#custom-api-version-format-strings for more information about formatting API versions.
            options.GroupNameFormat = "'v'VVV";
        });

        return builder;
    }

    private static void ConfigureResiliency<TBuilder>(TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddRequestTimeouts(
            configure: static timeouts =>
            {
                timeouts.DefaultPolicy = new RequestTimeoutPolicy
                {
                    Timeout = TimeSpan.FromMilliseconds(2000),
                    TimeoutStatusCode = 503,
                };

                timeouts.AddPolicy("HealthChecks", TimeSpan.FromSeconds(5));
            });

        builder.Services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter(
                policyName: "FixedRateLimitingPolicy",
                fixedWindowOptions =>
                {
                    fixedWindowOptions.PermitLimit = 4;
                    fixedWindowOptions.Window = TimeSpan.FromSeconds(12);
                    fixedWindowOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    fixedWindowOptions.QueueLimit = 2;
                });
        });

        builder.Services.AddRequestDecompression();

        builder.Services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
        });

        builder.Services.Configure<BrotliCompressionProviderOptions>(options => options.Level = CompressionLevel.Fastest);

        builder.Services.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.SmallestSize);
    }

    private static void ConfigureIdentity<TBuilder>(TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddOptions<KeycloakOptions>()
                .BindConfiguration(KeycloakOptions.SectionName)
                .ValidateDataAnnotations()
                .ValidateOnStart();

        builder.Services.AddScoped<IIdentityService, IdentityService>();
        builder.Services.AddSingleton<IAuthorizationHandler, GroupAuthorizationHandler>();
    }

    public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddMeter(
                        "Microsoft.AspNetCore.Hosting",
                        "System.Net.Http",
                        "Microsoft.AspNetCore.Server.Kestrel",
                        builder.Environment.ApplicationName);
            })
            .WithTracing(tracing =>
            {
                if (builder.Environment.IsDevelopment())
                {
                    // We want to view all traces in development
                    tracing.SetSampler(new AlwaysOnSampler());
                }

                tracing.AddSource(builder.Environment.ApplicationName)
                    .AddAspNetCoreInstrumentation()
                    .AddGrpcClientInstrumentation()
                    .AddHttpClientInstrumentation();
            });

        builder.AddOpenTelemetryExporters();

        builder.Logging.EnableEnrichment();
        builder.Logging.EnableRedaction();

        builder.Services.AddApplicationLogEnricher(options =>
        {
            options.ApplicationName = true;
            options.BuildVersion = true;
            options.DeploymentRing = true;
            options.EnvironmentName = true;
        });

        builder.Services.AddStaticLogEnricher<MachineNameEnricher>();

        builder.Services.AddScoped<RequestTimeMiddleware>();

        return builder;
    }

    private static TBuilder AddOpenTelemetryExporters<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

        // Azure Monitor exporter setup is available. See documentation for details.

        return builder;
    }

    public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddHealthChecks()
            // Add a default liveness check to ensure app is responsive
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        // Adding health checks endpoints to applications in non-development environments has security implications.
        if (app.Environment.IsDevelopment())
        {
            app.UseMiddleware<RequestTimeMiddleware>();

            var healthChecks = app.MapGroup("");

            healthChecks
                .CacheOutput("HealthChecks")
                .WithRequestTimeout("HealthChecks");

            // All health checks must pass for app to be considered ready to accept traffic after starting
            healthChecks.MapHealthChecks("/health");

            // Only health checks tagged with the "live" tag must pass for app to be considered alive
            healthChecks.MapHealthChecks("/alive", new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains("live")
            });
        }

        app.MapFallback(() => "Fallback").AllowAnonymous();

        return app;
    }
}