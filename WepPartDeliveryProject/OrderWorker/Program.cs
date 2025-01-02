using DbManager;
using DbManager.AppSettings;
using DbManager.Services;
using NLog;
using NLog.Web;
using System.Text.Json.Serialization;

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

    services.AddCustomOpenTelemetry(configuration, out var metricsExporter);

    var jsonSerializer = configuration.GetSection("JsonSerializer").Value;
    if (!string.IsNullOrEmpty(jsonSerializer) && jsonSerializer == "Newtonsoft")
    {
        logger.Info("Newtonsoft JsonSerializer used for controllers");
        services.AddControllers().AddNewtonsoftJson(options =>
        {
            options.SerializerSettings.MaxDepth = 3;
            options.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
        });
    }
    else
    {
        logger.Info("System.Text JsonSerializer used for controllers");
        services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            options.JsonSerializerOptions.WriteIndented = true;
        });
    }

    //services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();

    // Register application setting
    services.AddOptions<Neo4jSettings>().Bind(configuration.GetSection("Neo4jSettings"));
    services.AddOptions<KafkaSettings>().Bind(configuration.GetSection("KafkaSettings"));

    services.AddDbInfrastructure();
    services.AddSingleton<DeliveryHealthCheck>();
    services.AddHealthChecks()
        .AddCheck<GraphHealthCheck>(nameof(GraphHealthCheck), tags: ["live"])
        .AddCheck<DeliveryHealthCheck>(nameof(DeliveryHealthCheck), tags: ["ready"]);

    services.AddSingleton<QueryKafkaWorker, ObjectCasheQueryKafkaWorker>();
    services.AddHostedService<KafkaConsumerBackgroundService>();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }
    else
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
            options.RoutePrefix = string.Empty;
        });
    }

    app.UseHealthChecks("/health");

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