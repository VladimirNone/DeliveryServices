using DbManager.Dal;
using DbManager.Data;
using DbManager.Data.Nodes;
using DbManager.Neo4j.DataGenerator;
using DbManager.Neo4j.Implementations;
using DbManager.Neo4j.Interfaces;
using DbManager.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics.Metrics;

namespace DbManager
{
    public static class ServiceRegistration
    {
        public static void AddDbInfrastructure(this IServiceCollection services)
        {
            // This is to register Neo4j Client Object as a singleton
            services.AddSingleton<BoltGraphClientFactory>();

            services.AddSingleton<KafkaClientHandle>();
            services.AddSingleton<KafkaDependentProducer<string, string>>();
            services.AddSingleton<KafkaEventProducer>();

            services.AddSingleton<IRepositoryFactory, RepositoryFactory>();

            services.AddTransient<IPasswordService, PasswordService>();

            services.AddTransient<DataGenerator>();
            services.AddSingleton<GeneratorService>();

            // This is the registration for custom repository class
            services.AddTransient<IGeneralRepository<Order>, OrderRepository>();
            services.AddTransient<IGeneralRepository<Dish>, DishRepository>();
            services.AddTransient<IGeneralRepository<User>, UserRepository>();
            services.AddTransient<IGeneralRepository<Client>, ClientRepository>();
            services.AddTransient<IGeneralRepository<DeliveryMan>, DeliveryManRepository>();
        }

        public static void AddCustomOpenTelemetry(this IServiceCollection services, IConfiguration configuration, out string metricsExporter)
        {
            // Note: Switch between OTLP/Console by setting UseTracingExporter in appsettings.json.
            var tracingExporter = configuration.GetValue("UseTracingExporter", defaultValue: "console")!.ToLowerInvariant();

            // Note: Switch between Prometheus/OTLP/Console by setting UseMetricsExporter in appsettings.json.
            var localMetricsExporter = metricsExporter = configuration.GetValue("UseMetricsExporter", defaultValue: "console")!.ToLowerInvariant();

            // Note: Switch between Explicit/Exponential by setting HistogramAggregation in appsettings.json
            var histogramAggregation = configuration.GetValue("HistogramAggregation", defaultValue: "explicit")!.ToLowerInvariant();

            // Create a service to expose ActivitySource, and Metric Instruments
            // for manual instrumentation
            services.AddSingleton<Instrumentation>();

            // Configure OpenTelemetry logging, metrics, & tracing with auto-start using the
            // AddOpenTelemetry extension from OpenTelemetry.Extensions.Hosting.
            services.AddOpenTelemetry()
                .ConfigureResource(r => r
                    .AddService(
                        serviceName: configuration.GetValue("ServiceName", defaultValue: "otel-test")!,
                        serviceInstanceId: Environment.MachineName))
                .WithTracing(builder =>
                {
                    // Ensure the TracerProvider subscribes to any custom ActivitySources.
                    builder
                        .AddSource(Instrumentation.ActivitySourceName)
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
                            builder.AddConsoleExporter();
                            break;
                    }
                })
                .WithMetrics(builder =>
                {
                    // Ensure the MeterProvider subscribes to any custom Meters.
                    builder
                        .AddMeter(Instrumentation.MeterName)
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

                    switch (localMetricsExporter)
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
        }
    }
}
