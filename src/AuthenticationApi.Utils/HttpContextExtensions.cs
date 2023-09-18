using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace AuthenticationApi.Utils
{
    public static class HttpContextExtensions
    {
        public static bool TryGetApiKey(this HttpContext httpContext, out string apiKey)
        {
            var headers = httpContext.Request.Headers;

            if (!headers.TryGetValue(Constants.ApiKeyHeaderName, out StringValues value)
               || string.IsNullOrWhiteSpace(value))
            {
                apiKey = "";
                return false;
            }

            apiKey = value!;
            return true;
        }
    }
}
