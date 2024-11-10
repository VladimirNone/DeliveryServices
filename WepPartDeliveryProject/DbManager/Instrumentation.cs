using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace DbManager
{
    public class Instrumentation : IDisposable
    {
        public const string MeterName = "DbManager";
        public const string ActivitySourceName = "DbManager";
        private readonly Meter _meter;

        public Instrumentation()
        {
            string? version = typeof(Instrumentation).Assembly.GetName().Version?.ToString();
            this.ActivitySource = new ActivitySource(ActivitySourceName, version);
            this._meter = new Meter(MeterName, version);
            this.CacheEventCounter = this._meter.CreateCounter<long>("cache.events", description: "The number of events for changing object cache on cluster.");
        }

        public ActivitySource ActivitySource { get; }

        public Counter<long> CacheEventCounter { get; }

        public void Dispose()
        {
            this.ActivitySource.Dispose();
            this._meter.Dispose();
        }
    }
}
