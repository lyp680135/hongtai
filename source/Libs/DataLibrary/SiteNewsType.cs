namespace DataLibrary
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text;

    [Table("sitenewstype")]
    public partial class SiteNewsType
    {
        [Key]
        public int Id { get; set; }

        // 类型名称
        [MaxLength(255)]
        [Required]
        public string TypeName { get; set; }

       // 创建时间
        public int? Createtime { get; set; }

        [MaxLength(255)]
        [Required]
        public string TagName { get; set; }
    }
}
