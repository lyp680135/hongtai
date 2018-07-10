namespace DataLibrary
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text;
    using static DataLibrary.EnumList;

    [Table("sitecategory")]
    public partial class SiteCategory
    {
        [Key]
        public int Id { get; set; }

        // 内容标题
        [MaxLength(50)]
        [Required]
        public string ContentTitle { get; set; }

        // 模板Id
        public int? ModelId { get; set; }

        // 是否已经添加模型内容
        public HasModelContent HasModelContent { get; set; }

        // 父级Id
        public int? ParId { get; set; }

        // 排序
        public int Sequence { get; set; }

        // 层级深度
        public int? Depth { get; set; }
    }
}
