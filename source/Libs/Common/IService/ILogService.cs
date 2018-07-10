namespace Common.IService
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.AspNetCore.Http;

    public interface ILogService
    {
        /// <summary>
        /// 信息日志
        /// </summary>
        /// <param name="logstr">需要记录的数据</param>
        /// <param name="logger">日志类型</param>
        void LogInfo(string logstr, string logger = "");

        void LogInfo(string logstr, Exception ex, string logger = "");

        /// <summary>
        ///  异常日志
        /// </summary>
        /// <param name="logstr">需要记录的数据</param>
        /// <param name="logger">日志类型</param>
        void LogError(string logstr, string logger = "");

        void LogError(string logstr, Exception ex, string logger = "");

        /// <summary>
        /// 警告日志
        /// </summary>
        /// <param name="logstr">需要记录的数据</param>
        /// <param name="logger">日志类型</param>
        void LogWarm(string logstr, string logger = "");

        void LogWarm(string logstr, Exception ex, string logger = "");

        /// <summary>
        /// 调试日志
        /// </summary>
        /// <param name="logstr">需要记录的数据</param>
        /// <param name="logger">日志类型</param>
        void LogDebug(string logstr, string logger = "");

        void LogDebug(string logstr, Exception ex, string logger = "");

        string GetLogStr(HttpContext httpContext, Exception ex);

        /// <summary>
        /// 根据Http上下文记录信息
        /// </summary>
        /// <param name="httpContext">http上下文</param>
        /// <returns>字符串</returns>
        string GetLogStr(HttpContext httpContext);
    }
}
