using System.Net;
using System.Text;

namespace WepPartDeliveryProject
{
    public class RequestLoggerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggerMiddleware> _logger;

        public RequestLoggerMiddleware(RequestDelegate next, ILogger<RequestLoggerMiddleware> logger)
        {
            this._logger = logger;
            this._next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            this._logger.LogDebug(await FormatRequest(context.Request));

            await this._next(context);
        }

        private async Task<string> FormatRequest(HttpRequest request)
        {
            //This line allows us to set the reader for the request back at the beginning of its stream.
            request.EnableBuffering();

            using var reader = new StreamReader(request.Body,
                Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                leaveOpen: true /* important! */);

            //We convert the byte[] into a string using UTF8 encoding...
            var bodyAsText = await reader.ReadToEndAsync();

            if (request.Body.CanSeek)
                request.Body.Position = 0;

            return $"{request.Scheme}://{request.Host}{request.Path}{WebUtility.UrlDecode(request.QueryString.ToString())} {bodyAsText}";
        }

    }
}
