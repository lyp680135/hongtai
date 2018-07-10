namespace WarrantyManage
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Quartz;
    using Quartz.Impl;
    using Quartz.Logging;

    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseIISIntegration()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .UseUrls("http://*:5001")
                .Build();

            // 配置Quartz的日志输出
            LogProvider.SetCurrentLogProvider(new ProModels.QuartzNet.ConsoleLogProvider());

            // 启动Quartz定时服务程序
            ProModels.QuartzNet.RunQuartzJob().GetAwaiter().GetResult();

            host.Run();
        }
    }
}
