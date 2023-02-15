using DbManager;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var services = builder.Services;
var configuration = builder.Configuration;

services.AddControllers();
//services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

// Register application setting
services.Configure<ApplicationSettings>(configuration.GetSection("ApplicationSettings"));

// Fetch settings object from configuration
var settings = new ApplicationSettings();
configuration.GetSection("ApplicationSettings").Bind(settings);

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
