namespace WarrantyApiCenter
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Common.Middleware;
    using Common.Service;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.FileProviders;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.IdentityModel.Tokens;
    using WarrantyApiCenter.Extensions;

    public class Startup
    {
        private static IServiceProvider service;

        private readonly IHostingEnvironment hostingEnvironments;

        public Startup(IHostingEnvironment hostingEnvironment, IServiceProvider service)
        {
            this.hostingEnvironments = hostingEnvironment;
            var builder = new ConfigurationBuilder()
                .SetBasePath(hostingEnvironment.ContentRootPath)
                .AddJsonFile("config.json", true, true)
                .AddEnvironmentVariables();

            this.Configuration = builder.Build();

            service = new ServiceCollection()
                            .AddLogService()
                            .BuildServiceProvider();
            Startup.service = service;
        }

        /// <summary>
        /// Gets 数据库连接字符串
        /// </summary>
        public static string ConnStr { get; private set; }

        public IConfiguration Configuration { get; }

        public static TService GetService<TService>()
        {
            return service.GetService<TService>();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            ConnStr = this.Configuration["Database:ConnectionString"];
            services.AddMemoryCache();

            // 添加DataContext
            services.AddDbContext<DataLibrary.DataContext>(
                x => x.UseMySql(this.Configuration["Database:ConnectionString"]).EnableSensitiveDataLogging());

            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddUserService();
            services.AddLoginService();
            services.AddSettingService();
            services.AddQrCodeAuthService();
            services.AddBaseService();
            services.AddQualityService();
            services.AddStockoutService();
            services.AddCertService();
            services.AddMySqlServices(ConnStr);
            services.AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
            });
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // 日志查询中间件
            app.UseMiddleware<LogMiddleware>();

            // 添加日志静态文件
            app.AddLogStaticFiles();
            app.UseStaticFiles();

            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), @"qualitypics")),
                RequestPath = new PathString("/qualitypics")
            });

            // 异常处理中间件
            app.UseMiddleware<ExceptionHandlerMiddleWare>();

            // JWT认证中间件
            app.UseMiddleware<TokenAuthMiddleware>();

            app.UseMvc();
        }
    }
}
