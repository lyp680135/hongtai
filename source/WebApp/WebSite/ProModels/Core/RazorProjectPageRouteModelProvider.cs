namespace WarrantyManage.ProModels.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc.ApplicationModels;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
    using Microsoft.AspNetCore.Mvc.RazorPages.Internal;
    using Microsoft.AspNetCore.Razor.Language;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    public class RazorProjectPageRouteModelProvider : IPageRouteModelProvider
    {
        private readonly RazorProject project;
        private readonly RazorPagesOptions pagesOptions;
        private readonly PageRouteModelFactory routeModelFactory;
        private readonly ILogger<RazorProjectPageRouteModelProvider> logger;

        public RazorProjectPageRouteModelProvider(
            RazorProject razorProject,
            IOptions<RazorPagesOptions> pagesOptionsAccessor,
            ILoggerFactory loggerFactory)
        {
            this.project = razorProject;
            this.pagesOptions = pagesOptionsAccessor.Value;
            this.logger = loggerFactory.CreateLogger<RazorProjectPageRouteModelProvider>();
            this.routeModelFactory = new PageRouteModelFactory(this.pagesOptions, this.logger);
        }

        private static bool IsRouteable(RazorProjectItem item)
        {
            // Pages like _ViewImports should not be routable.
            return !item.FileName.StartsWith("_", StringComparison.OrdinalIgnoreCase);
        }

        /// <remarks>
        /// Ordered to execute after <see cref="CompiledPageRouteModelProvider"/>.
        /// </remarks>
        public int Order => -1000 + 10;

        public void OnProvidersExecuted(PageRouteModelProviderContext context)
        {
        }

        public void OnProvidersExecuting(PageRouteModelProviderContext context)
        {
            // When RootDirectory and AreaRootDirectory overlap, e.g. RootDirectory = /, AreaRootDirectoryy = /Areas;
            // we need to ensure that the page is only route-able via the area route. By adding area routes first,
            // we'll ensure non area routes get skipped when it encounters an IsAlreadyRegistered check.
            if (this.pagesOptions.AllowAreas)
            {
                this.AddAreaPageModels(context);
            }

            this.AddPageModels(context);
        }

        private void AddPageModels(PageRouteModelProviderContext context)
        {
            var normalizedAreaRootDirectory = Startup.SiteTemplateRootDirectory;
            if (!normalizedAreaRootDirectory.EndsWith("/", StringComparison.Ordinal))
            {
                normalizedAreaRootDirectory += "/";
            }

            foreach (var item in this.project.EnumerateItems(Startup.SiteTemplateRootDirectory))
            {
                if (!IsRouteable(item))
                {
                    continue;
                }

                var relativePath = item.CombinedPath;
                if (context.RouteModels.Any(m => string.Equals(relativePath, m.RelativePath, StringComparison.OrdinalIgnoreCase)))
                {
                    // A route for this file was already registered either by the CompiledPageRouteModel or as an area route.
                    // by this provider. Skip registering an additional entry.

                    // Note: We're comparing duplicates based on root-relative paths. This eliminates a page from being discovered
                    // by overlapping area and non-area routes where ViewEnginePath would be different.
                    continue;
                }

                if (!PageDirectiveFeature.TryGetPageDirective(this.logger, item, out var routeTemplate))
                {
                    // .cshtml pages without @page are not RazorPages.
                    continue;
                }

                var routeModel = this.routeModelFactory.CreateRouteModel(relativePath, routeTemplate);
                if (routeModel != null && context.RouteModels.Any(m => string.Equals(routeModel.ViewEnginePath, m.ViewEnginePath, StringComparison.OrdinalIgnoreCase)) == false)
                {
                    context.RouteModels.Add(routeModel);
                }
            }
        }

        private void AddAreaPageModels(PageRouteModelProviderContext context)
        {
            foreach (var item in this.project.EnumerateItems(this.pagesOptions.AreaRootDirectory))
            {
                if (!IsRouteable(item))
                {
                    continue;
                }

                var relativePath = item.CombinedPath;
                if (context.RouteModels.Any(m => string.Equals(relativePath, m.RelativePath, StringComparison.OrdinalIgnoreCase)))
                {
                    // A route for this file was already registered either by the CompiledPageRouteModel.
                    // Skip registering an additional entry.
                    continue;
                }

                if (!PageDirectiveFeature.TryGetPageDirective(this.logger, item, out var routeTemplate))
                {
                    // .cshtml pages without @page are not RazorPages.
                    continue;
                }

                var routeModel = this.routeModelFactory.CreateAreaRouteModel(relativePath, routeTemplate);
                if (routeModel != null)
                {
                    context.RouteModels.Add(routeModel);
                }
            }
        }
    }
}
