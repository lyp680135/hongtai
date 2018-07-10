﻿namespace WarrantyManage.ProModels.Core
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using Microsoft.AspNetCore.Mvc.ApplicationModels;
    using Microsoft.AspNetCore.Mvc.Razor;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Primitives;

    internal class PageRouteModelFactory
    {
        private static readonly string IndexFileName = "Index" + RazorViewEngine.ViewExtension;
        private readonly RazorPagesOptions options;
        private readonly ILogger logger;
        private readonly string normalizedRootDirectory;
        private readonly string normalizedAreaRootDirectory;

        public PageRouteModelFactory(
            RazorPagesOptions options,
            ILogger logger)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            this.normalizedRootDirectory = NormalizeDirectory(Startup.SiteTemplateRootDirectory);
            this.normalizedAreaRootDirectory = NormalizeDirectory(options.AreaRootDirectory);
        }

        public PageRouteModel CreateRouteModel(string relativePath, string routeTemplate)
        {
            var viewEnginePath = this.GetViewEnginePath(this.normalizedRootDirectory, relativePath);
            var routeModel = new PageRouteModel(relativePath, viewEnginePath);

            PopulateRouteModel(routeModel, viewEnginePath, routeTemplate);

            return routeModel;
        }

        public PageRouteModel CreateAreaRouteModel(string relativePath, string routeTemplate)
        {
            if (!this.TryParseAreaPath(relativePath, out var areaResult))
            {
                return null;
            }

            var routeModel = new PageRouteModel(relativePath, areaResult.viewEnginePath, areaResult.areaName);

            var routePrefix = CreateAreaRoute(areaResult.areaName, areaResult.viewEnginePath);
            PopulateRouteModel(routeModel, routePrefix, routeTemplate);
            routeModel.RouteValues["area"] = areaResult.areaName;

            return routeModel;
        }

        // Internal for unit testing
        internal bool TryParseAreaPath(string relativePath, out(string areaName, string viewEnginePath) result)
        {
            // path = "/Areas/Products/Pages/Manage/Home.cshtml"
            // Result ("Products", "/Manage/Home")
            result = (string.Empty, string.Empty);
            Debug.Assert(relativePath.StartsWith("/", StringComparison.Ordinal));

            // Parse the area root directory.
            var areaRootEndIndex = relativePath.IndexOf('/', startIndex: 1);
            if (areaRootEndIndex == -1 ||
                areaRootEndIndex >= relativePath.Length - 1 || // There's at least one token after the area root.
                !relativePath.StartsWith(this.normalizedAreaRootDirectory, StringComparison.OrdinalIgnoreCase))
            {
                // _logger.UnsupportedAreaPath(_options, relativePath);
                return false;
            }

            // The first directory that follows the area root is the area name.
            var areaEndIndex = relativePath.IndexOf('/', startIndex: areaRootEndIndex + 1);
            if (areaEndIndex == -1 || areaEndIndex == relativePath.Length)
            {
                // _logger.UnsupportedAreaPath(_options, relativePath);
                return false;
            }

            var areaName = relativePath.Substring(areaRootEndIndex + 1, areaEndIndex - areaRootEndIndex - 1);

            string viewEnginePath;

            var optionsRootDirectory = Startup.SiteTemplateRootDirectory;

            if (optionsRootDirectory == "/")
            {
                // When RootDirectory is "/", every thing past the area name is the page path.
                Debug.Assert(relativePath.EndsWith(RazorViewEngine.ViewExtension), $"{relativePath} does not end in extension '{RazorViewEngine.ViewExtension}'.");
                viewEnginePath = relativePath.Substring(areaEndIndex, relativePath.Length - areaEndIndex - RazorViewEngine.ViewExtension.Length);
            }
            else
            {
                // Normalize the pages root directory so that it has a trailing slash. This ensures we're looking at a directory delimiter
                // and not just the area name occuring as part of a segment.
                Debug.Assert(optionsRootDirectory.StartsWith("/", StringComparison.Ordinal));

                // If the pages root has a value i.e. it's not the app root "/", ensure that the area path contains this value.
                if (string.Compare(relativePath, areaEndIndex, this.normalizedRootDirectory, 0, this.normalizedRootDirectory.Length, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    // _logger.UnsupportedAreaPath(_options, relativePath);
                    return false;
                }

                // Include the trailing slash of the root directory at the start of the viewEnginePath
                var pageNameIndex = areaEndIndex + this.normalizedRootDirectory.Length - 1;
                viewEnginePath = relativePath.Substring(pageNameIndex, relativePath.Length - pageNameIndex - RazorViewEngine.ViewExtension.Length);
            }

            result = (areaName, viewEnginePath);
            return true;
        }

        private static string CreateAreaRoute(string areaName, string viewEnginePath)
        {
            // AreaName = Products, ViewEnginePath = /List/Categories
            // Result = /Products/List/Categories
            Debug.Assert(!string.IsNullOrEmpty(areaName));
            Debug.Assert(!string.IsNullOrEmpty(viewEnginePath));
            Debug.Assert(viewEnginePath.StartsWith("/", StringComparison.Ordinal));

            var builder = new InplaceStringBuilder(1 + areaName.Length + viewEnginePath.Length);
            builder.Append('/');
            builder.Append(areaName);
            builder.Append(viewEnginePath);

            return builder.ToString();
        }

        private static SelectorModel CreateSelectorModel(string prefix, string routeTemplate)
        {
            return new SelectorModel
            {
                AttributeRouteModel = new AttributeRouteModel
                {
                    Template = AttributeRouteModel.CombineTemplates(prefix, routeTemplate),
                }
            };
        }

        private static string NormalizeDirectory(string directory)
        {
            Debug.Assert(directory.StartsWith("/", StringComparison.Ordinal));
            if (directory.Length > 1 && !directory.EndsWith("/", StringComparison.Ordinal))
            {
                return directory + "/";
            }

            return directory;
        }

        private static void PopulateRouteModel(PageRouteModel model, string pageRoute, string routeTemplate)
        {
            model.RouteValues.Add("page", model.ViewEnginePath);

            var selectorModel = CreateSelectorModel(pageRoute, routeTemplate);
            model.Selectors.Add(selectorModel);

            var fileName = Path.GetFileName(model.RelativePath);
            if (!AttributeRouteModel.IsOverridePattern(routeTemplate) &&
                string.Equals(IndexFileName, fileName, StringComparison.OrdinalIgnoreCase))
            {
                selectorModel.AttributeRouteModel.SuppressLinkGeneration = true;

                var index = pageRoute.LastIndexOf('/');
                var parentDirectoryPath = index == -1 ?
                    string.Empty :
                    pageRoute.Substring(0, index);
                model.Selectors.Add(CreateSelectorModel(parentDirectoryPath, routeTemplate));
            }
        }

        private string GetViewEnginePath(string rootDirectory, string path)
        {
            // rootDirectory = "/Pages/AllMyPages/"
            // path = "/Pages/AllMyPages/Home.cshtml"
            // Result = "/Home"
            Debug.Assert(path.StartsWith(rootDirectory, StringComparison.OrdinalIgnoreCase));
            Debug.Assert(path.EndsWith(RazorViewEngine.ViewExtension, StringComparison.OrdinalIgnoreCase));
            var startIndex = rootDirectory.Length - 1;
            var endIndex = path.Length - RazorViewEngine.ViewExtension.Length;
            return path.Substring(startIndex, endIndex - startIndex);
        }
    }
}
