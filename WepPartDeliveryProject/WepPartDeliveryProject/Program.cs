using DbManager;
using DbManager.Neo4j.DataGenerator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Neo4jClient;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var services = builder.Services;
var configuration = builder.Configuration;

services.AddLogging(loggingBuilder => {
    var loggingSection = configuration.GetSection("Logging");
    loggingBuilder.AddFile(loggingSection);
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

// Fetch settings object from configuration
var settings = new Neo4jSettings();
configuration.GetSection("Neo4jSettings").Bind(settings);

services.AddDbInfrastructure(settings);
services.AddHealthChecks().AddCheck<GraphHealthCheck>("GraphHealthCheck");

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
graphClient.OperationCompleted += (sender, e) => 
app.Logger.LogInformation(e.QueryText.Replace("\r\n", ""));

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseHealthChecks("/healthcheck");

app.UseRouting();

app.MapControllers();

app.Run();
