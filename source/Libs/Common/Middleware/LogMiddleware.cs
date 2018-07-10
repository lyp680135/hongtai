namespace Common.Middleware
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json;
    using static DataLibrary.EnumList;

    /// <summary>
    /// 日志中间件
    /// </summary>
    public class LogMiddleware
    {
        private readonly RequestDelegate next;
        private readonly string headerStr = "logfile";
        private readonly string authKey = "xiaoyukeji";

        public LogMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public Task Invoke(HttpContext httpContext)
        {
            string requestPath = httpContext.Request.Path.Value.ToLower();

            if (requestPath.Contains(this.headerStr) && requestPath.EndsWith(".log"))
            {
                var authKey = httpContext.Request.Query["auth"];
                if (this.authKey == authKey)
                {
                    return this.next(httpContext);
                }
                else
                {
                    return this.AuthorizationFailed(httpContext);
                }
            }

            return this.next(httpContext);
        }

        private Task AuthorizationFailed(HttpContext httpContext)
        {
            httpContext.Response.ContentType = "application/json;charset=utf-8";
            httpContext.Response.StatusCode = 401;
            return httpContext.Response.WriteAsync(JsonConvert.SerializeObject(new { Status = 0, Msg = "请输入读取日志秘钥" }));
        }
    }
}
