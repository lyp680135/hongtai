namespace WarrantyEnterpriseMobileStation
{
    using System.IO;
    using Common.Middleware;
    using Common.Service;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.FileProviders;
    using System.Text.Encodings.Web;
    using System.Text.Unicode;

    public class Startup
    {
        private readonly IHostingEnvironment hostingEnvironments;

        public Startup(IHostingEnvironment hostingEnvironment)
        {
            this.hostingEnvironments = hostingEnvironment;
            var builder = new ConfigurationBuilder()
                .SetBasePath(hostingEnvironment.ContentRootPath)
                .AddJsonFile("config.json", true, true)
                .AddEnvironmentVariables();

            this.Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; private set; }

        private int SiteTemplateId { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {
            // 添加DataContext
            services.AddDbContext<DataLibrary.DataContext>(x => x.UseMySql(this.Configuration["Database:ConnectionString"]).EnableSensitiveDataLogging());

            services.AddSettingService();

            this.SiteTemplateId = this.GetSiteTemplateId(services);

            services.AddMvc().AddRazorPagesOptions(options =>
            {
                options.RootDirectory = $"/Template/{this.SiteTemplateId}";
            });

            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                {
                    options.AccessDeniedPath = new PathString("/Login");
                    options.LoginPath = new PathString("/Login");
                    options.LogoutPath = new PathString("/LogOff");
                });

            services.AddSingleton(HtmlEncoder.Create(UnicodeRanges.All));
            services.AddUserService();
            services.AddLoginService();
            services.AddSinglePageService();
            services.AddMySqlServices(this.Configuration["Database:ConnectionString"]);
            services.AddContentGroupService();
            services.AddContentColumnDataService();
            services.AddLogService();
            services.AddBaseService();
            services.AddCMSService();
            services.AddSiteBasicService();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            // 日志查询中间件
            app.UseMiddleware<LogMiddleware>();

            // 添加日志静态文件
            app.AddLogStaticFiles();
            app.UseStaticFiles(
                new StaticFileOptions()
                {
                    FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), $"Template/{this.SiteTemplateId}/Content")),
                    RequestPath = new PathString("/Content")
                });

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });
        }

        private int GetSiteTemplateId(IServiceCollection services)
        {
            var setting = services.BuildServiceProvider().GetService<Common.IService.ISettingService>();
            return setting.MngSetting.SiteTemplateId;
        }
    }
}
