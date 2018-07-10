namespace WarrantyApiCenter.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json;
    using static DataLibrary.EnumList;

    /// <summary>
    /// WebApi的权限验证（除 /api/token 外，其他所有的Controller 需进行 Jwt认证）
    /// </summary>
    public class TokenAuthMiddleware
    {
        private readonly RequestDelegate next;

        public TokenAuthMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public Task Invoke(HttpContext httpContext)
        {
            string requestPath = httpContext.Request.Path.Value.ToLower();
            if (requestPath == "/api/v1/quality" || requestPath == "/api/v1/quality/")
            {
            }
            else
            {
                httpContext.Response.ContentType = "application/json;charset=utf-8";
            }

            httpContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            httpContext.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
            httpContext.Response.Headers.Add("Access-Control-Allow-Headers", "Authorization");
            if (httpContext.Request.Method.ToLower() == "options")
            {
                var responseModel = new Models.ResponseModel(ApiResponseStatus.Success, "Options请求成功", string.Empty);
                httpContext.Response.StatusCode = 200;
                return httpContext.Response.WriteAsync(JsonConvert.SerializeObject(responseModel));
            }

            if (requestPath == "/api/v1/quality" || requestPath == "/api/v1/quality/")
            {
                return this.next(httpContext);
            }

            if (AnonymousCheck.Invoke(httpContext))
            {
                return this.next(httpContext);
            }
            else
            {
                // 检测是否包含'Authorization'请求头，如果不包含返回context进行下一个中间件，用于访问不需要认证的API
                if (!httpContext.Request.Headers.ContainsKey("Authorization"))
                {
                    return this.AuthorizationFailed(httpContext);
                }
                else
                {
                    var tokenHeader = httpContext.Request.Headers["Authorization"];

                    try
                    {
                        tokenHeader = tokenHeader.ToString().Substring("Bearer ".Length).Trim();
                        var result = Models.StoreSignedUser.ExistsSignedUser(tokenHeader);
                        if (result == null)
                        {
                            return this.AuthorizationFailed(httpContext);
                        }
                        else
                        {
                            ClaimsPrincipal principal = new ClaimsPrincipal(result.UserClaims);
                            httpContext.User = principal; // 构建authorize认证
                            return this.next(httpContext);
                        }
                    }
                    catch
                    {
                        return this.AuthorizationFailed(httpContext);
                    }
                }
            }
        }

        private Task AuthorizationFailed(HttpContext httpContext)
        {
            var responseModel = new Models.ResponseModel(ApiResponseStatus.Failed, "JWT验证失败", string.Empty);
            httpContext.Response.StatusCode = 401;
            return httpContext.Response.WriteAsync(JsonConvert.SerializeObject(responseModel));
        }
    }
}
