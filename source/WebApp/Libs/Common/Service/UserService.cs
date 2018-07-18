namespace Common.Service
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using Common.IService;
    using DataLibrary;
    using Microsoft.AspNetCore.Http;
    using Util;
    using static DataLibrary.EnumList;

    /// <summary>
    /// 当前登录用户实体服务
    /// </summary>
    public class UserService : IUserService
    {
        private DataContext db;
        private IHttpContextAccessor accessor;

        public UserService(DataLibrary.DataContext dataContext, IHttpContextAccessor httpContextAccessor)
        {
            this.db = dataContext;
            this.accessor = httpContextAccessor;

            this.ApplicationUser = this.GetCurrentUser();
            this.SaleSellerUser = this.GetCurrentSaleSeller();
        }

        /// <summary>
        /// Gets or sets a value indicating whether 是否已登录
        /// </summary>
        public bool IsAuth { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether 是否钢厂人员（true 是,false为经销商）
        /// </summary>
        public bool IsManageMember { get; set; }

        /// <summary>
        /// Gets or sets 当前钢厂人员登录的用户及权限
        /// </summary>
        public ApplicationUser ApplicationUser { get; set; }

        /// <summary>
        /// Gets or sets 经销商登录人员用户
        /// </summary>
        public SaleSeller SaleSellerUser { get; set; }

        /// <summary>
        /// 根据当前用户判断是否拥有某个权限(只针对于钢厂人员，经销商权限都是写死的)
        /// </summary>
        /// <param name="permissionId">权限ID</param>
        /// <returns>有权限 true,无权限 false</returns>
        public bool IshavePermission(int permissionId)
        {
            if (this.ApplicationUser != null && this.ApplicationUser.Mng_admin != null)
            {
                int groupId = Convert.ToInt32(this.ApplicationUser.Mng_admin.GroupManage);
                if (groupId == (int)GroupManage.管理员)
                {
                    return true;
                }
                else
                {
                    bool flag = false;

                    if (this.db.MngPermissiongroupset.Where(c => c.GroupId == groupId && c.PermissionId == permissionId).Count() > 0)
                    {
                        flag = true;
                    }

                    if (this.db.MngPermissionpersonset.Where(c => c.AdminId == this.ApplicationUser.Mng_admin.Id && c.PermissionId == permissionId).Count() > 0)
                    {
                        flag = true;
                    }

                    return flag;
                }
            }

            return false;
        }

        /// <summary>
        /// 根据角色名称判断是否有该角色
        /// </summary>
        /// <param name="roleName">角色名称</param>
        /// <returns>是否</returns>
        public bool IsHaveRole(string roleName)
        {
            var appUser = this.GetCurrentUser();
            if (appUser.Mng_admin != null)
            {
                var model_pmg = this.db.MngPermissiongroup.FirstOrDefault(c => c.GroupName == roleName);

                if (model_pmg != null)
                {
                    if (appUser.Mng_admin.GroupManage.Object.Contains(model_pmg.Id))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// 根据角色名称和Id判断是否有该角色
        /// </summary>
        /// <param name="roleName">角色名称</param>
        /// <param name="userId">用户Id</param>
        /// <returns>是否</returns>
        public bool IsHaveRole(string roleName, int userId)
        {
            var mng_admin = this.db.MngAdmin.FirstOrDefault(c => c.Id == userId);
            if (mng_admin != null)
            {
                var model_pmg = this.db.MngPermissiongroup.FirstOrDefault(c => c.GroupName == roleName);

                if (model_pmg != null)
                {
                    if (mng_admin.GroupManage.Object.Contains(model_pmg.Id))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            return false;
        }

            /// <summary>
            /// 获得当前钢厂人员登录的用户及权限
            /// </summary>
            /// <returns>当前钢厂人员实体</returns>
            private ApplicationUser GetCurrentUser()
        {
            var user1 = this.accessor.HttpContext.User;
            this.IsAuth = user1.Identity.IsAuthenticated;

            if (this.IsAuth && user1.Identity.AuthenticationType == SystemMemberType.Manage.ToString())
            {
                this.IsManageMember = true;
                this.IsAuth = true;

                if (user1 != null && user1.Claims.Count() > 0)
                {
                    var user = new ApplicationUser
                    {
                        Mng_admin = new DataLibrary.MngAdmin
                        {
                            UserName = user1.Identity.Name
                        }
                    };

                    var userid = Util.Extensions.ToInt(user1.Claims.First(p => p.Type == ClaimTypes.NameIdentifier).Value);

                    user.Mng_admin = this.db.MngAdmin.FirstOrDefault(c => c.Id == userid);

                    List<int> list = new List<int>();

                    if (user.Mng_admin != null)
                    {
                        var result = this.db.MngPermissionpersonset.Where(p => p.AdminId == userid);
                        foreach (var re in result)
                        {
                            list.Add(re.PermissionId);
                        }

                        if (user.Mng_admin.GroupManage.SafeString() != string.Empty)
                        {
                            var groupIds = user.Mng_admin.GroupManage.Object;
                            var result2 = this.db.MngPermissiongroupset.Where(d => groupIds.Contains(d.GroupId));
                            foreach (var re in result2)
                            {
                                list.Add(re.PermissionId);
                            }
                        }
                    }

                    user.PermissionIds = list;

                    return user;
                }
            }

            this.IsAuth = false;
            return null;
        }

        /// <summary>
        /// 获得当前经销商登录人员帐号
        /// </summary>
        /// <returns>经销商登录人员实体</returns>
        private SaleSeller GetCurrentSaleSeller()
        {
            var user = this.accessor.HttpContext.User;
            this.IsAuth = user.Identity.IsAuthenticated;

            if (this.IsAuth && user.Identity.AuthenticationType == SystemMemberType.Seller.ToString())
            {
                this.IsManageMember = false;

                if (user != null && user.Claims.Count() > 0)
                {
                    var userid = Util.Extensions.ToInt(user.Claims.First(p => p.Type == ClaimTypes.NameIdentifier).Value);

                    var saleSeller = this.db.SaleSeller.FirstOrDefault(c => c.Id == userid);

                    return saleSeller;
                }
            }

            return null;
        }
    }
}
