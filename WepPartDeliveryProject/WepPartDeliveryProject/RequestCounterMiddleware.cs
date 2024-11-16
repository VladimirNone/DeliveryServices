using DbManager;
using System.Diagnostics.Metrics;

namespace WepPartDeliveryProject
{
    public class RequestCounterMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly Instrumentation _instrumentation;
        private const string Prefix = "request_";

        private Dictionary<string, Counter<long>> _counters = new Dictionary<string, Counter<long>>();

        public RequestCounterMiddleware(RequestDelegate next, Instrumentation instrumentation)
        {
            this._next = next;
            this._instrumentation = instrumentation;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var methodName = context.Request.Path.Value?.Split('/')[2].ToLower();
            if(methodName != null)
            {
                methodName = Prefix + methodName;
                if (this._counters.TryGetValue(methodName, out var counter))
                {
                    counter.Add(1);
                }
                else
                {
                    var newCounter = this._instrumentation.Meter.CreateCounter<long>(methodName);
                    this._counters.Add(methodName, newCounter);
                    newCounter.Add(1);
                }
            }

            await this._next(context);
        }
    }
}
