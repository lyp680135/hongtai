namespace WarrantyManage
{
    using System;
    using System.IO;
    using System.Text.Encodings.Web;
    using System.Text.Unicode;
    using Common.Middleware;
    using Common.Service;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ApplicationModels;
    using Microsoft.AspNetCore.Mvc.Razor;
    using Microsoft.AspNetCore.StaticFiles;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.FileProviders;
    using Microsoft.Extensions.Options;
    using UEditorNetCore;
    using WarrantyManage.MiddleWare;

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

            Configuration = builder.Build();
        }

        public static IConfiguration Configuration { get; private set; }

        /// <summary>
        /// Gets ���ݿ������ַ���
        /// </summary>
        public static string ConnStr { get; private set; }

        /// <summary>
        /// Gets CMSģ��·��
        /// </summary>
        public static string SiteTemplateRootDirectory { get; private set; }

        public void ConfigureServices(IServiceCollection services)
        {
            ConnStr = Configuration["Database:ConnectionString"];
            services.AddDbContext<DataLibrary.DataContext>(x => x.UseMySql(ConnStr).EnableSensitiveDataLogging());

            services.AddSettingService();
            SiteTemplateRootDirectory = $"/Template/{this.GetSiteTemplateId(services)}";

            var mvcBuilder = services.AddMvc(options =>
            {
                options.Filters.Add(typeof(PermissionPageFilter));
                options.Filters.Add(typeof(PermissionActionFilter));
            }).AddRazorPagesOptions(options =>
            {
                options.Conventions.AddPageRoute("/News/Detail", "/News");
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            ServiceDescriptor serviceDescriptor_razorViewEngineOptions = ServiceDescriptor.Transient<IConfigureOptions<RazorViewEngineOptions>, ProModels.Core.RazorPagesRazorViewEngineOptionsSetup>();
            ServiceDescriptor serviceDescriptor_routeModel = ServiceDescriptor.Singleton<IPageRouteModelProvider, ProModels.Core.RazorProjectPageRouteModelProvider>();

            mvcBuilder.Services.TryAddEnumerable(serviceDescriptor_razorViewEngineOptions);
            mvcBuilder.Services.TryAddEnumerable(serviceDescriptor_routeModel);

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
            services.TryAddScoped<IHttpContextAccessor, HttpContextAccessor>();
            services.AddMemoryCache(); // ʹ�û���
            services.AddUserService();
            services.AddLoginService();
            services.AddCMSService();
            services.AddSinglePageService();
            services.AddSettingService();
            services.AddQrCodeAuthService();
            services.AddSiteBasicService();
            services.AddLogService();
            services.AddBaseService();
            services.AddBatcodeService();
            services.AddContentGroupService();
            services.AddContentColumnDataService();
            services.AddMySqlServices(ConnStr);
            services.AddUEditorService(this.hostingEnvironments.ContentRootPath + "/wwwroot/ueditor/net/config.json");
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
                app.UseStatusCodePagesWithReExecute("/Manage/NotFound");
                app.UseExceptionHandler("/Manage/Error");
            }

            // ��־��ѯ�м��
            app.UseMiddleware<LogMiddleware>();

            // �����־��̬�ļ�
            app.AddLogStaticFiles();
            app.UseStaticFiles();
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), @"UploadFile")),
                RequestPath = new PathString("/UploadFile")
            });

            app.UseStaticFiles(
                new StaticFileOptions()
                {
                    FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), SiteTemplateRootDirectory.Trim('/') + "/Content")),
                    RequestPath = new PathString("/Content")
                });
            app.UseAuthentication();

            this.InitializeDatabase(app);

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });

            ServiceLocator.Instance = app.ApplicationServices;
        }

        /// <summary>
        /// ��ʼ�����ݿ�
        /// </summary>
        /// <param name="app">app</param>
        private void InitializeDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var db = serviceScope.ServiceProvider.GetService<DataLibrary.DataContext>();

                if (db.Database.EnsureCreated())
                {
                    try
                    {
                        var path = System.IO.Path.GetFullPath("initData.sql");
                        if (System.IO.File.Exists(path))
                        {
                            string initsql = System.IO.File.ReadAllText(path);

                            // db.Database.ExecuteSqlCommand(initsql);
                            // db.SaveChanges();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("ϵͳ��ʼ������ʧ��!" + ex.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// ��ø�ϵͳ��CMSģ��ID
        /// </summary>
        /// <param name="services">ServiceCollection</param>
        /// <returns>ģ��ID</returns>
        private int GetSiteTemplateId(IServiceCollection services)
        {
            var setting = services.BuildServiceProvider().GetService<Common.IService.ISettingService>();
            return setting.MngSetting.SiteTemplateId;
        }

        /// <summary>
        /// ���е�ע����󼯺�
        /// </summary>
        public class ServiceLocator
        {
            public static IServiceProvider Instance { get; set; }
        }
    }
}
