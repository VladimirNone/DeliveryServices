using DbManager;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var services = builder.Services;
var configuration = builder.Configuration;

services.AddControllers();
services.AddSwaggerGen();

// Register application setting
services.Configure<ApplicationSettings>(configuration.GetSection("ApplicationSettings"));

// Fetch settings object from configuration
var settings = new ApplicationSettings();
configuration.GetSection("ApplicationSettings").Bind(settings);

services.AddDbInfrastructure(settings);


// In production, the React files will be served from this directory
/*services.AddSpaStaticFiles(configuration =>
{
    configuration.RootPath = "client_app/build";
});*/

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

/*app.UseSpa(spa =>
{
    spa.Options.SourcePath = "client_app";

    if (app.Environment.IsDevelopment())
    {
        spa.UseReactDevelopmentServer(npmScript: "start");
    }
});*/

app.Run();
