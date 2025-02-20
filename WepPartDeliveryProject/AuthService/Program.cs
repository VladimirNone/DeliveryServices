using DbManager;
using DbManager.AppSettings;
using DbManager.Services;
using NLog;
using NLog.Web;
using System.Text.Json.Serialization;
using WepPartDeliveryProject;

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

    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(
            policy =>
            {
                policy
                    .WithOrigins(configuration.GetSection("ClientAppSettings:ClientAppApi").Value)
                    .WithOrigins("http://localhost:3000")
                    .WithOrigins("http://localhost:3001")
                    //.WithHeaders(HeaderNames.ContentType, HeaderNames.Cookie)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
    });

    services.AddOptions<ApplicationSettings>().Bind(configuration.GetSection("ApplicationSettings"));
    services.AddOptions<Neo4jSettings>().Bind(configuration.GetSection("Neo4jSettings"));

    services.AddDbInfrastructure(ServiceRegistration.DatabaseProvider.Neo4j);
    services.AddSingleton<DeliveryHealthCheck>();
    services.AddHealthChecks()
        .AddCheck<GraphHealthCheck>(nameof(GraphHealthCheck), tags: ["live"])
        .AddCheck<DeliveryHealthCheck>(nameof(DeliveryHealthCheck), tags: ["ready"]);

    services.AddSingleton<JwtService>();

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

    services.AddCustomOpenTelemetry(configuration, out var metricsExporter);

    // Add services to the container.
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHealthChecks("/health");

    app.MapControllers();

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