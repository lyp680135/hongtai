namespace DataLibrary
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// 菜单与权限
    /// </summary>
    [Table("mngmenuclass")]
    public partial class MngMenuclass
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets 父级ID
        /// </summary>
        public int? ParId { get; set; }

        [MaxLength(50)]
        [Required]
        public string ClassName { get; set; }

        /// <summary>
        /// Gets or sets 排序
        /// </summary>
        public int Sequence { get; set; }

        /// <summary>
        /// Gets or sets 层级深度
        /// </summary>
        public int Depth { get; set; }

        /// <summary>
        /// Gets or sets 子菜单数
        /// </summary>
        public int ChildNum { get; set; }

        /// <summary>
        /// Gets or sets 父级路径
        /// </summary>
        [MaxLength(255)]
        public string ParPath { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether 是否权限
        /// </summary>
        public bool IsPermission { get; set; }

        /// <summary>
        /// Gets or sets 权限类别（0：PC端，1：移动端，2两者）
        /// </summary>
        public EnumList.PermissionType PermissionType { get; set; }

        /// <summary>
        /// Gets or sets 链接地址
        /// </summary>
        [MaxLength(255)]
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether 是否可以删除
        /// </summary>
        public bool IsCanDelete { get; set; }
    }
}
