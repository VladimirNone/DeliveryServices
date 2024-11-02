using DbManager;
using DbManager.AppSettings;
using DbManager.Mapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using NLog;
using NLog.Web;
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

    services.AddAutoMapper(typeof(MapperProfile));
    services.AddSingleton<JwtService>();

    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(
            policy =>
            {
                policy
                    .WithOrigins(configuration.GetSection("ClientAppSettings:ClientAppApi").Value)
                    //.WithOrigins("http://localhost:3000")
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
    services.AddOptions<KafkaSettings>().Bind(configuration.GetSection("KafkaSettings"));

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
    services.AddHostedService<KafkaConsumerBackgroundService>();

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