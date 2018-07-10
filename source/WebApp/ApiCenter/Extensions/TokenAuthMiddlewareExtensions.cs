namespace WarrantyApiCenter.Extensions
{
    using Microsoft.AspNetCore.Builder;

    public static class TokenAuthMiddlewareExtensions
    {
        public static IApplicationBuilder UseTokenAuthMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TokenAuthMiddleware>();
        }
    }
}
