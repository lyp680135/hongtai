namespace WarrantyManage.ProModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using Quartz;
    using Quartz.Impl;
    using Quartz.Logging;
    using Util;

    public class QuartzNet
    {
        /// <summary>
        /// 定时服务程序启动入口配置
        /// </summary>
        /// <returns>任务</returns>
        public static async Task RunQuartzJob()
        {
            try
            {
                // 1.命名StdScheduler并创建factory
                NameValueCollection props = new NameValueCollection
                {
                    { "quartz.serializer.type", "binary" }
                };

                ISchedulerFactory schedulerFactory = new StdSchedulerFactory(props);
                var scheduler = await schedulerFactory.GetScheduler();

                // 启动
                await scheduler.Start();

                // 任务作业
                var job1 = JobBuilder.Create<Jobs.UpdateQrCodeAuthJob>().WithIdentity("job1", "group1").Build();

                // 作业触发器
                var job1_trigger = TriggerBuilder.Create().WithIdentity("trigger1", "group1").StartNow()
                                    .WithSimpleSchedule(x => x
                                        .WithIntervalInMinutes(10) // 10分钟检测一次
                                        .RepeatForever()) // 永远循环
                                    .Build();

                // await Task.Delay(TimeSpan.FromSeconds(10)); // 休眠10秒

                // 告诉Quartz 使用触发器在执行作业上
                await scheduler.ScheduleJob(job1, job1_trigger);

                // await scheduler.Shutdown(); // 关闭调度程序
            }
            catch (SchedulerException se)
            {
                Console.WriteLine(se);
            }
        }

        /// <summary>
        ///  QuarzNet的日志输出配置类
        /// </summary>
        public class ConsoleLogProvider : ILogProvider
        {
            public Logger GetLogger(string name)
            {
                return (level, func, exception, parameters) =>
                {
                    if (level >= LogLevel.Info && func != null)
                    {
                        Console.WriteLine("[" + DateTime.Now.ToLongTimeString() + "] [" + level + "] " + func(), parameters);
                    }

                    return true;
                };
            }

            public IDisposable OpenNestedContext(string message)
            {
                throw new NotImplementedException();
            }

            public IDisposable OpenMappedContext(string key, string value)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// 任务列表
        /// </summary>
        public class Jobs
        {
            [DisallowConcurrentExecution] // 不允许并发执行
            public class UpdateQrCodeAuthJob : IJob
            {
                public async Task Execute(IJobExecutionContext context)
                {
                    Common.IService.IQrCodeAuthService qrCodeAuthService = Startup.ServiceLocator.Instance.GetService<Common.IService.IQrCodeAuthService>();

                    try
                    {
                        // 如果本月没有设置过授权数据
                        if (!qrCodeAuthService.IsExistsDataForThisMonty())
                        {
                            int authDate = (int)new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).GetUnixTimeFromDateTime();
                            var dataForPrev = qrCodeAuthService.GetDataForPrevMonth();

                            var listDataForPrev = dataForPrev.Item1; // 上月数据

                            var listAdd = new List<DataLibrary.PdQRCodeAuth>(); // 要添加的数据

                            if (listDataForPrev != null && listDataForPrev.Count > 0)
                            {
                                int maxId = dataForPrev.Item2;

                                listDataForPrev.ForEach(v =>
                               {
                                   maxId++;

                                   listAdd.Add(new DataLibrary.PdQRCodeAuth()
                                   {
                                       Adder = 0,
                                       AuthDate = authDate,
                                       Classid = v.Classid,
                                       Createtime = (int)DateTime.Now.GetUnixTimeFromDateTime(),
                                       Id = maxId,
                                       Materialid = v.Materialid,
                                       Number = v.Number,
                                       Specid = v.Specid,
                                       WorkshopId = v.WorkshopId
                                   });
                               });

                                int addCount = qrCodeAuthService.AddRange(listAdd);

                                await Console.Out.WriteLineAsync($"生产二维码授权定时复制程序已运行完成，本次新增{addCount}条");
                            }
                            else
                            {
                                await Console.Out.WriteLineAsync($"生产二维码授权定时复制程序已运行完成，上月无历史授权数据，复制失败");
                            }
                        }
                        else
                        {
                            await Console.Out.WriteLineAsync($"生产二维码授权定时复制程序已运行完成，本月已有授权数据");
                        }
                    }
                    catch (Exception ex)
                    {
                        await Console.Out.WriteLineAsync($"生产二维码授权定时复制程序运行出错：" + ex.ToString());
                    }
                }
            }
        }
    }
}
