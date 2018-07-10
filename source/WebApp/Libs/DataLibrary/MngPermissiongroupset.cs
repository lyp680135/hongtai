namespace DataLibrary
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// 权限组与具体权限的 关系
    /// </summary>
    [Table("mngpermissiongroupset")]
    public partial class MngPermissiongroupset
    {
        [Key]
        public int Id { get; set; }

        public int GroupId { get; set; }

        public int PermissionId { get; set; }
    }
}
