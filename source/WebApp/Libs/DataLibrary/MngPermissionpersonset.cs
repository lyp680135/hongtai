namespace DataLibrary
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// 个人的额外权限
    /// </summary>
    [Table("mngpermissionpersonset")]
    public partial class MngPermissionpersonset
    {
        [Key]
        public int Id { get; set; }

        public int AdminId { get; set; }

        public int PermissionId { get; set; }
    }
}
