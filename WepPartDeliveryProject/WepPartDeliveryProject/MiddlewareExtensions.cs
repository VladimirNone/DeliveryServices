namespace WepPartDeliveryProject
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestLogger(this IApplicationBuilder builder)
            => builder.UseMiddleware<RequestLoggerMiddleware>();
    }
}
