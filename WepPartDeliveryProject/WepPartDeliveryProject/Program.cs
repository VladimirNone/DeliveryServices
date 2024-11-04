using DbManager;
using DbManager.Mapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using NLog;
using NLog.Web;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics.Metrics;
using System.Text;
using WepPartDeliveryProject;
using WepPartDeliveryProject.BackgroundServices;

var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
try
{
    var builder = WebApplication.CreateBuilder(args);

    // NLog: Setup NLog for Dependency injection
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    // Add services to the container.
    var services = builder.Services;
    var configuration = builder.Configuration;

    // Note: Switch between OTLP/Console by setting UseTracingExporter in appsettings.json.
    var tracingExporter = configuration.GetValue("UseTracingExporter", defaultValue: "console")!.ToLowerInvariant();

    // Note: Switch between Prometheus/OTLP/Console by setting UseMetricsExporter in appsettings.json.
    var metricsExporter = configuration.GetValue("UseMetricsExporter", defaultValue: "console")!.ToLowerInvariant();

    // Note: Switch between Explicit/Exponential by setting HistogramAggregation in appsettings.json
    var histogramAggregation = configuration.GetValue("HistogramAggregation", defaultValue: "explicit")!.ToLowerInvariant();

    // Configure OpenTelemetry logging, metrics, & tracing with auto-start using the
    // AddOpenTelemetry extension from OpenTelemetry.Extensions.Hosting.
    builder.Services.AddOpenTelemetry()
        .ConfigureResource(r => r
            .AddService(
                serviceName: configuration.GetValue("ServiceName", defaultValue: "otel-test")!,
                serviceInstanceId: Environment.MachineName))
        .WithTracing(builder =>
        {
            // Ensure the TracerProvider subscribes to any custom ActivitySources.
            builder
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation();

            // Use IConfiguration binding for AspNetCore instrumentation options.
            services.Configure<AspNetCoreTraceInstrumentationOptions>(configuration.GetSection("AspNetCoreInstrumentation"));

            switch (tracingExporter)
            {
                case "otlp":
                    builder.AddOtlpExporter(otlpOptions =>
                    {
                        // Use IConfiguration directly for Otlp exporter endpoint option.
                        otlpOptions.Endpoint = new Uri(configuration.GetValue("Otlp:Endpoint", defaultValue: "http://localhost:4317")!);
                    });
                    break;

                default:
                    builder.AddConsoleExporter("someName", options => { });
                    break;
            }
        })
        .WithMetrics(builder =>
        {
            // Ensure the MeterProvider subscribes to any custom Meters.
            builder
                //.AddMeter()
                .SetExemplarFilter(ExemplarFilterType.TraceBased)
                .AddRuntimeInstrumentation()
                .AddProcessInstrumentation()
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation();

            switch (histogramAggregation)
            {
                case "exponential":
                    builder.AddView(instrument =>
                    {
                        return instrument.GetType().GetGenericTypeDefinition() == typeof(Histogram<>)
                            ? new Base2ExponentialBucketHistogramConfiguration()
                            : null;
                    });
                    break;
                default:
                    // Explicit bounds histogram is the default.
                    // No additional configuration necessary.
                    break;
            }

            switch (metricsExporter)
            {
                case "prometheus":
                    builder.AddPrometheusExporter();
                    break;
                case "otlp":
                    builder.AddOtlpExporter(otlpOptions =>
                    {
                        // Use IConfiguration directly for Otlp exporter endpoint option.
                        otlpOptions.Endpoint = new Uri(configuration.GetValue("Otlp:Endpoint", defaultValue: "http://localhost:4317")!);
                    });
                    break;
                default:
                    builder.AddConsoleExporter();
                    break;
            }
        });

    services.AddAutoMapper(typeof(MapperProfile));
    services.AddSingleton<JwtService>();

    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(
            policy =>
            {
                policy
                    .WithOrigins(configuration.GetSection("ClientAppSettings:ClientAppApi").Value)
                    .WithOrigins("http://localhost:3000")
                    //.WithHeaders(HeaderNames.ContentType, HeaderNames.Cookie)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
    });

    services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.MaxDepth = 3;
                options.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
            }
        );
    //services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();

    // Register application setting
    services.AddOptions<Neo4jSettings>().Bind(configuration.GetSection("Neo4jSettings"));
    services.AddOptions<ApplicationSettings>().Bind(configuration.GetSection("ApplicationSettings"));

    services.AddDbInfrastructure(configuration);
    services.AddSingleton<DeliveryHealthCheck>();
    services.AddHealthChecks()
        .AddCheck<GraphHealthCheck>(nameof(GraphHealthCheck), tags: ["live"])
        .AddCheck<DeliveryHealthCheck>(nameof(DeliveryHealthCheck), tags: ["ready"]);

    services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

    })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetSection("ApplicationSettings:JwtSecretKey").Value)),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
            };
        });

    services.AddHostedService<StartupBackgroundService>();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty;
    });

    app.UseCors();

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseHealthChecks("/health");

    app.UseAuthentication();
    app.UseRouting();
    app.UseAuthorization();

    app.MapControllers();

    // Configure OpenTelemetry Prometheus AspNetCore middleware scrape endpoint if enabled.
    if (metricsExporter.Equals("prometheus", StringComparison.OrdinalIgnoreCase))
    {
        app.UseOpenTelemetryPrometheusScrapingEndpoint();
    }

    app.Run();
}
catch (Exception exception)
{
    // NLog: catch setup errors
    logger.Fatal(exception, "Stopped program because of exception");
    throw;
}
finally
{
    // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
    LogManager.Shutdown();
}