using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace DbManager
{
    public class Instrumentation : IDisposable
    {
        public const string MeterName = "DbManager";
        public const string ActivitySourceName = "DbManager";
        public readonly Meter Meter;

        public Instrumentation()
        {
            string? version = typeof(Instrumentation).Assembly.GetName().Version?.ToString();
            this.ActivitySource = new ActivitySource(ActivitySourceName, version);
            this.Meter = new Meter(MeterName, version);
        }

        public ActivitySource ActivitySource { get; }

        public void Dispose()
        {
            this.ActivitySource.Dispose();
            this.Meter.Dispose();
        }
    }
}
