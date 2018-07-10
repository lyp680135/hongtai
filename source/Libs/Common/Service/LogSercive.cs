namespace Common.Service
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Common.IService;
    using log4net;
    using log4net.Config;
    using log4net.Repository;
    using Microsoft.AspNetCore.Http;
    using Util.Helpers;

    public class LogSercive : ILogService
    {
        private static log4net.ILog logger = null;
        private static ILoggerRepository logRepository;
        private static object objlock = new object();

        public void LogError(string logstr, string logger = "ERROR")
        {
            if (InitLog4net())
            {
                LogSercive.logger = LogManager.GetLogger(logRepository.Name, logger);
                LogSercive.logger.Error(logstr);
            }
        }

        public void LogError(string logstr, Exception ex, string logger = "ERROR")
        {
            if (InitLog4net())
            {
                LogSercive.logger = LogManager.GetLogger(logRepository.Name, logger);
                LogSercive.logger.Error(logstr, ex);
            }
        }

        public void LogInfo(string logstr, string logger = "INFO")
        {
            if (InitLog4net())
            {
                LogSercive.logger = LogManager.GetLogger(logRepository.Name, logger);
                LogSercive.logger.Info(logstr);
            }
        }

        public void LogInfo(string logstr, Exception ex, string logger = "INFO")
        {
            if (InitLog4net())
            {
                LogSercive.logger = LogManager.GetLogger(logRepository.Name, logger);
                LogSercive.logger.Info(logstr, ex);
            }
        }

        public void LogWarm(string logstr, string logger = "WARN")
        {
            if (InitLog4net())
            {
                LogSercive.logger = LogManager.GetLogger(logRepository.Name, logger);
                LogSercive.logger.Warn(logstr);
            }
        }

        public void LogWarm(string logstr, Exception ex, string logger = "WARN")
        {
            if (InitLog4net())
            {
                LogSercive.logger = LogManager.GetLogger(logRepository.Name, logger);
                LogSercive.logger.Warn(logstr, ex);
            }
        }

        public void LogDebug(string logstr, string logger = "DEBUG")
        {
            Console.WriteLine(logstr);
            if (InitLog4net())
            {
                LogSercive.logger = LogManager.GetLogger(logRepository.Name, logger);
                LogSercive.logger.Debug(logstr);
            }
        }

        public void LogDebug(string logstr, Exception ex, string logger = "DEBUG")
        {
            Console.WriteLine(ex.Message);
            if (InitLog4net())
            {
                LogSercive.logger = LogManager.GetLogger(logRepository.Name, logger);
                LogSercive.logger.Debug(logstr, ex);
            }
        }

        public string GetLogStr(HttpContext httpContext)
        {
            string getAbsoluteUri = GetAbsoluteUri(httpContext.Request);
            string referer = httpContext.Request.Headers["Referer"].ToString();
            string cookie = httpContext.Request.Cookies["UserName"];
            string reqMethond = httpContext.Request.Method;
            return Json.ToJson(new { getAbsoluteUri, referer, cookie, reqMethond });
        }

        public string GetLogStr(HttpContext httpContext, Exception ex)
        {
            string getAbsoluteUri = GetAbsoluteUri(httpContext.Request);
            string exception = ex.ToString();
            string referer = httpContext.Request.Headers["Referer"].ToString();
            string cookie = httpContext.Request.Cookies["UserName"];
            string reqMethond = httpContext.Request.Method;
            return Json.ToJson(new { getAbsoluteUri, referer, cookie, reqMethond, exception });
        }

        /// <summary>
        /// 单列,初始化配置
        /// </summary>
        /// <returns>void</returns>
        private static bool InitLog4net()
        {
            if (logger != null)
            {
                return true;
            }

            lock (objlock)
            {
                if (logger == null)
                {
                     logRepository = LogManager.CreateRepository("LogRepository");
                    XmlConfigurator.Configure(logRepository, new FileInfo("log.config"));
                    return true;
                }
            }

            return false;
        }

        private static string GetAbsoluteUri(HttpRequest request)
        {
            return new StringBuilder()
                .Append(request.Scheme)
                .Append("://")
                .Append(request.Host)
                .Append(request.PathBase)
                .Append(request.Path)
                .Append(request.QueryString)
                .ToString();
        }
    }
}
