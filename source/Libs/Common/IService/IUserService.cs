namespace Common.IService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using DataLibrary;
    using Microsoft.AspNetCore.Http;
    using Util;
    using static DataLibrary.EnumList;

    /// <summary>
    /// 获取站点初始化配置服务
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Gets or sets a value indicating whether 是否已登录
        /// </summary>
        bool IsAuth { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether 是否钢厂人员（true 是,false为经销商）
        /// </summary>
        bool IsManageMember { get; set; }

        /// <summary>
        /// Gets or sets 当前钢厂人员登录的用户及权限
        /// </summary>
        ApplicationUser ApplicationUser { get; set; }

        /// <summary>
        /// Gets or sets 经销商登录人员用户
        /// </summary>
        SaleSeller SaleSellerUser { get; set; }

        /// <summary>
        /// 根据当前用户判断是否拥有某个权限(只针对于钢厂人员，经销商权限都是写死的)
        /// </summary>
        /// <param name="permissionId">权限ID</param>
        /// <returns>有权限 true,无权限 false</returns>
        bool IshavePermission(int permissionId);
    }
}
