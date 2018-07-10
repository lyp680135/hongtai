namespace DataLibrary
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// 部门表
    /// </summary>
    [Table("mngdepartmentclass")]
    public partial class MngDepartmentclass
    {
        [Key]
        public int Id { get; set; }

        public int? ParId { get; set; }

        [MaxLength(50)]
        [Required]
        public string ClassName { get; set; }

        public int? Sequence { get; set; }

        public int? Depth { get; set; }

        public int? ChildNum { get; set; }

        [MaxLength(255)]
        public string ParPath { get; set; }

        public bool? BeLock { get; set; }

        public bool IsCanDelete { get; set; }
    }
}
