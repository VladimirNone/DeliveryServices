namespace WepPartDeliveryProject
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestCounter(this IApplicationBuilder builder)
            => builder.UseMiddleware<RequestCounterMiddleware>();
    }
}
