using DbManager;
using DbManager.Neo4j.DataGenerator;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Neo4jClient;
using Newtonsoft.Json;
using WepPartDeliveryProject;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var services = builder.Services;
var configuration = builder.Configuration;

services.AddLogging(loggingBuilder => {
    var loggingSection = configuration.GetSection("Logging");
    loggingBuilder.AddFile(loggingSection);
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy.WithOrigins(configuration.GetSection("ClientAppSettings:ClientAppApi").Value)
            //.WithHeaders(HeaderNames.ContentType, HeaderNames.Cookie)
            .AllowAnyHeader()
            .WithMethods("GET", "POST")
            .AllowCredentials();
/*            policy.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();*/
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

services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(cookieAuthOp => 
{
    cookieAuthOp.Cookie = new CookieBuilder() { SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None, SecurePolicy = CookieSecurePolicy.Always };
});

services.AddAuthorization(options =>
{
    options.AddPolicy("age-policy", x => { x.RequireClaim("age"); });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//await app.Services.GetService<GeneratorService>().GenerateAll();
//Отчасти костыль
var graphClient = app.Services.GetService<IGraphClient>();
graphClient.OperationCompleted += (sender, e) => app.Logger.LogInformation(e.QueryText.Replace("\r\n", ""));

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseHealthChecks("/healthcheck");

app.UseCors();

app.UseAuthentication();
app.UseRouting();
app.UseAuthorization();

/*app.Use(async (context, next) =>
{
    var req = context.User;
    await next.Invoke();
});*/

app.MapControllers();

app.Run();