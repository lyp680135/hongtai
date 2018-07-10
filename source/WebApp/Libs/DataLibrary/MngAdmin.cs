namespace DataLibrary
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("mngadmin")]

    /// <summary>
    /// 管理员用户表
    /// </summary>
    public partial class MngAdmin
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(50)]
        [Required]
        public string UserName { get; set; }

        [MaxLength(50)]
        [Required]
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets 部门ID
        /// </summary>
        public int? DepartId { get; set; }

        public DateTime? LoginTime { get; set; }

        public int? LoginTimes { get; set; }

        [MaxLength(20)]
        public string LoginIp { get; set; }

        [MaxLength(20)]
        public string FirstChar { get; set; }

        /// <summary>
        /// Gets or sets 真实姓名
        /// </summary>
        [MaxLength(50)]
        public string RealName { get; set; }

        public bool? Sex { get; set; }

        [MaxLength(50)]
        public string Pic { get; set; }

        /// <summary>
        /// Gets or sets 权限组
        /// </summary>
        public JsonObject<List<int>> GroupManage { get; set; }

        /// <summary>
        /// Gets or sets 移动电话
        /// </summary>
        public JsonObject<List<string>> MobilePhone { get; set; }

        [MaxLength(50)]
        public string QQ { get; set; }

        [MaxLength(128)]
        public string Address { get; set; }

        [MaxLength(50)]
        public string Mail { get; set; }

        /// <summary>
        /// Gets or sets 是否在职
        /// </summary>
        public bool? InJob { get; set; }

        public bool IsCanDelete { get; set; }
    }
}
