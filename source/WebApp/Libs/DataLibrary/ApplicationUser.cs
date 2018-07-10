namespace DataLibrary
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text;

    /// <summary>
    /// 钢厂人员登录实体
    /// </summary>
    public class ApplicationUser
    {
        /// <summary>
        /// Gets or sets 当前登录用户实体
        /// </summary>
        public DataLibrary.MngAdmin Mng_admin { get; set; }

        /// <summary>
        /// Gets or sets 当前登录用户的功能权限集合
        /// </summary>
        public List<int> PermissionIds { get; set; }
    }
}
