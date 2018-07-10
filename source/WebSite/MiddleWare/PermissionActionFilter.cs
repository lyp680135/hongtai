namespace WarrantyManage.MiddleWare
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Models;

    public class PermissionActionFilter : Attribute, IActionFilter
    {
        private DataLibrary.DataContext db;
        private Common.IService.IUserService userService;

        public PermissionActionFilter(DataLibrary.DataContext db, Common.IService.IUserService userService)
        {
            this.db = db;
            this.userService = userService;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var user1 = context.HttpContext.User;

            if (user1 != null && user1.Identity.IsAuthenticated)
            {
                string url = context.ActionDescriptor.DisplayName.ToLower();

                url = url.TrimEnd('/').ToLower();

                // 过滤不需要验证权限的
                List<string> list_NoPermission = new List<string>
                {
                    "/login",
                    "/logoff",
                    "/manage/index",
                    "/manage/left",
                    "/manage/error",
                    "/manage/nopermission"
                };

                if (string.IsNullOrEmpty(url) || list_NoPermission.Contains(url))
                {
                    return;
                }

                var model = this.db.MngMenuclass.FirstOrDefault(c => c.Url == url);
                if (model != null)
                {
                    var user = this.userService.ApplicationUser;

                    List<int> listPermission = user.PermissionIds;
                    if (listPermission != null && listPermission.Count > 0)
                    {
                        bool flag = false;
                        foreach (var va in listPermission)
                        {
                            if (va == model.Id)
                            {
                                flag = true;
                                break;
                            }
                        }

                        if (!flag)
                        {
                            context.Result = new Microsoft.AspNetCore.Mvc.RedirectResult("~/Manage/NoPermission");
                        }
                    }
                }
            }
        }
    }
}
