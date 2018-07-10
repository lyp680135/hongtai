namespace WarrantyApiCenter.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Common.IService;
    using Common.Service;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json;

    public class ExceptionHandlerMiddleWare
    {
        private readonly RequestDelegate next;
        private readonly ILogService logService;

        public ExceptionHandlerMiddleWare(RequestDelegate next)
        {
            this.next = next;
            this.logService = Startup.GetService<ILogService>();
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await this.next(context);
            }
            catch (Exception ex)
            {
                this.logService.LogError(this.logService.GetLogStr(context, ex));
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            if (exception == null)
            {
                return;
            }

            await WriteExceptionAsync(context, exception).ConfigureAwait(false);
        }

        private static async Task WriteExceptionAsync(HttpContext context, Exception exception)
        {
            // 返回友好的提示
            var response = context.Response;

            // 状态码
            if (exception is UnauthorizedAccessException)
            {
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
            }
            else if (exception is Exception)
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;
            }

            await response.WriteAsync(
               JsonConvert.SerializeObject(new Models.ResponseModel(DataLibrary.EnumList.ApiResponseStatus.Failed, "请求失败", exception.GetBaseException().Message))).ConfigureAwait(false);
        }
    }
}
