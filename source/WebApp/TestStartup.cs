using System;

public class TestStartup
{
    public TestStartup()
    {
        private static IServiceProvider service;

    private readonly IHostingEnvironment hostingEnvironments;

    public TestStartup(IHostingEnvironment hostingEnvironment, IServiceProvider service)
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

    public IConfiguration Configuration { get; }

    public static TService GetService<TService>()
    {
        return service.GetService<TService>();
    }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
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
