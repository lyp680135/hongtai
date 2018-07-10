namespace Common.Service
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.StaticFiles;
    using Microsoft.Extensions.FileProviders;

    public static class StaticFilesExtension
    {
        public static IApplicationBuilder AddLogStaticFiles(this IApplicationBuilder app)
        {
            // 添加.log文件映射
            var contentTypeProvider = new FileExtensionContentTypeProvider();
            contentTypeProvider.Mappings[".log"] = "text/plain";
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(
                   Path.Combine(Directory.GetCurrentDirectory(), @"logfile")),
                ContentTypeProvider = contentTypeProvider,
                RequestPath = new PathString("/logfile")
            });

            // 开启静态文件目录浏览
            app.UseDirectoryBrowser(new DirectoryBrowserOptions()
            {
                FileProvider = new PhysicalFileProvider(
              Path.Combine(Directory.GetCurrentDirectory(), @"logfile")),
                RequestPath = new PathString("/logfile")
            });
            return app;
        }
    }
}
