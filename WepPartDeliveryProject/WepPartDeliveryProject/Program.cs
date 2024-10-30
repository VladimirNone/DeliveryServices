using DbManager;
using DbManager.AppSettings;
using DbManager.Mapper;
using DbManager.Neo4j.DataGenerator;
using DbManager.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Neo4jClient;
using Neo4jClient.Execution;
using Newtonsoft.Json;
using System.Text;
using WepPartDeliveryProject;
using WepPartDeliveryProject.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var services = builder.Services;
var configuration = builder.Configuration;

/*services.AddLogging(loggingBuilder => {
    var loggingSection = configuration.GetSection("Logging");
    loggingBuilder.AddFile(loggingSection);
});
*/
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
                .WithHeaders(HeaderNames.ContentType, HeaderNames.Cookie)
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
services.Configure<Neo4jSettings>(configuration.GetSection("Neo4jSettings"));
services.Configure<ApplicationSettings>(configuration.GetSection("ApplicationSettings"));

services.AddDbInfrastructure(configuration);
services.AddHealthChecks().AddCheck<GraphHealthCheck>("GraphHealthCheck");

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

services.AddHostedService<KafkaConsumerBackgroundService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

var appSetting = new ApplicationSettings();
configuration.GetSection("ApplicationSettings").Bind(appSetting);

if (appSetting.GenerateData)
    await app.Services.GetService<GeneratorService>().GenerateAll();

//Отчасти костыль
var graphClient = app.Services.GetService<IGraphClient>();
graphClient.OperationCompleted += (sender, e) => app.Logger.LogInformation(e.QueryText.Replace("\r\n", " "));

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});

app.UseCors();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseHealthChecks("/healthcheck");

app.UseAuthentication();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();

app.Run();