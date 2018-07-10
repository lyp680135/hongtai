namespace Common.Service
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Common.IService;
    using Microsoft.Extensions.DependencyInjection;
    using Util.Helpers;

    public static partial class Extension
    {
        public static IServiceCollection AddUserService(this IServiceCollection service)
        {
            return service.AddScoped<IUserService, UserService>();
        }

        public static IServiceCollection AddLoginService(this IServiceCollection service)
        {
            return service.AddScoped<ILoginService, LoginService>();
        }

        public static IServiceCollection AddSinglePageService(this IServiceCollection service)
        {
            return service.AddScoped<ISinglePageService, SinglePageService>();
        }

        public static IServiceCollection AddSettingService(this IServiceCollection service)
        {
            return service.AddScoped<ISettingService, SettingService>();
        }

        public static IServiceCollection AddQrCodeAuthService(this IServiceCollection service)
        {
            return service.AddScoped<IQrCodeAuthService, QrCodeAuthService>();
        }

        public static IServiceCollection AddLogService(this IServiceCollection service)
        {
            return service.AddScoped<ILogService, LogSercive>();
        }

        public static IServiceCollection AddBaseService(this IServiceCollection service)
        {
            return service.AddScoped(typeof(IBaseService<>), typeof(BaseService<>));
        }

        public static IServiceCollection AddBatcodeService(this IServiceCollection service)
        {
            return service.AddScoped<IBatcodeService, BatcodeService>();
        }

        public static IServiceCollection AddContentGroupService(this IServiceCollection service)
        {
            return service.AddScoped<IContentGroupService, ContentGroupService>();
        }

        public static IServiceCollection AddSiteBasicService(this IServiceCollection service)
        {
            return service.AddScoped<ISiteBasicService, SiteBasicService>();
        }

        public static IServiceCollection AddContentColumnDataService(this IServiceCollection service)
        {
            return service.AddScoped<IContentColumnDataService, ContentColumnDataService>();
        }

        public static IServiceCollection AddQualityService(this IServiceCollection service)
        {
            return service.AddScoped<IQualityService, QualityService>();
        }

        public static IServiceCollection AddStockoutService(this IServiceCollection service)
        {
            return service.AddScoped<IStockOutService, StockOutService>();
        }

        public static IServiceCollection AddCertService(this IServiceCollection service)
        {
            return service.AddScoped<ICertService, CertService>();
        }

        public static IServiceCollection AddMySqlServices(this IServiceCollection service, string connStr)
        {
            return service.AddScoped<MySqlHelper>(o => new MySqlHelper(connStr));
        }

        public static IServiceCollection AddCMSService(this IServiceCollection service)
        {
            return service.AddScoped<ICMSService, CMSService>();
        }
    }
}
