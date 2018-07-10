namespace DataLibrary
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;


    /// <summary>
    /// 管理员用户表
    /// </summary>
    public partial class MngAdmin
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets 部门ID
        /// </summary>
        public int? DepartId { get; set; }

        public DateTime? LoginTime { get; set; }

        public int? LoginTimes { get; set; }

        public string LoginIp { get; set; }

        public string FirstChar { get; set; }

        /// <summary>
        /// Gets or sets 真实姓名
        /// </summary>
        public string RealName { get; set; }

        public bool? Sex { get; set; }

        public string Pic { get; set; }

        /// <summary>
        /// Gets or sets 权限组
        /// </summary>
        public object GroupManage { get; set; }

        /// <summary>
        /// Gets or sets 移动电话
        /// </summary>
        public object MobilePhone { get; set; }

        public string QQ { get; set; }

        public string Address { get; set; }

        public string Mail { get; set; }

        /// <summary>
        /// Gets or sets 是否在职
        /// </summary>
        public bool? InJob { get; set; }

        public bool IsCanDelete { get; set; }
    }
}
