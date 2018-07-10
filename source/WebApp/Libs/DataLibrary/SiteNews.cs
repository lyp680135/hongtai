namespace DataLibrary
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text;

    [Table("sitenews")]
    public partial class SiteNews
    {
        [Key]
        public int Id { get; set; }

        // 新闻标题
        [MaxLength(255)]
        [Required]
        public string NewsTitle { get; set; }

        // 新闻图片
        public JsonObject<List<string>> ImgUrl { get; set; }

        // 新闻内容
        [Required]
        public string NewsContent { get; set; }

        // 新闻类型ID
        [Required]
        public int NewsTypeId { get; set; }

        // 创建时间
        public int Createtime { get; set; }

        // 作者
        [MaxLength(255)]
        public string NewsAuthor { get; set; }

        // 浏览量
        public int ViewCount { get; set; }

        // 摘要
        [MaxLength(255)]
        public string Abstract { get; set; }
    }
}
