namespace WarrantyApiCenter.Extensions
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Http;

    public static class AnonymousCheck
    {
        /// <summary>
        /// 可访问匿名请求列表
        /// </summary>
        private static List<KeyValuePair<string, List<string>>> listAnonymousData =
            new List<KeyValuePair<string, List<string>>>()
        {
             new KeyValuePair<string, List<string>>("/api/token", new List<string>() { "get" }),
             new KeyValuePair<string, List<string>>("/api/systemconfig", new List<string>() { "get" }),
             new KeyValuePair<string, List<string>>("/api/tokenbyphone", new List<string>() { "get" }),
             new KeyValuePair<string, List<string>>("/api/tokenbysaler", new List<string>() { "get" }),
             new KeyValuePair<string, List<string>>("/api/v1/warranty", new List<string>() { "get" }),
             new KeyValuePair<string, List<string>>("/api/v1/quality", new List<string>() { "get" }),
             new KeyValuePair<string, List<string>>("/api/v1/cert", new List<string>() { "post" }),
        };

        public static List<KeyValuePair<string, List<string>>> ListAnonymousData { get => listAnonymousData; set => listAnonymousData = value; }

        /// <summary>
        /// 查看当前请求是否允许匿名
        /// </summary>
        /// <param name="httpContext">httpContext</param>
        /// <returns>bool</returns>
        public static bool Invoke(HttpContext httpContext)
        {
            string requestPath = httpContext.Request.Path.Value.ToLower();
            string method = httpContext.Request.Method.ToLower();
            foreach (var item in ListAnonymousData)
            {
                if (item.Key.ToLower() == requestPath || item.Key.ToLower() + "/" == requestPath)
                {
                    foreach (var itemMethod in item.Value)
                    {
                        if (itemMethod.ToLower() == method)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
