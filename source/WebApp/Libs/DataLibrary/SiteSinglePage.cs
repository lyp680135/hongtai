namespace DataLibrary
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text;

    /// <summary>
    /// 单页管理表
    /// </summary>
    [Table("sitesinglepage")]
    public partial class SiteSinglePage
    {
        [Key]
        public int Id { get; set; }

        // 标题
        [MaxLength(255)]
        public string Title { get; set; }

        // 内容
        public string Content { get; set; }

        // 页面描述
        [MaxLength(255)]
        public string Descrption { get; set; }

        // 创建时间
        public int Createtime { get; set; }
    }
}
