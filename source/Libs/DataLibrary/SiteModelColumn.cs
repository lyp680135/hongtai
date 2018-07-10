namespace DataLibrary
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text;
    using static DataLibrary.EnumList;

    /// <summary>
    /// 模型字段管理基类
    /// </summary>
    [Table("sitemodelcolumn")]
    public class SiteModelColumn
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets 字段名
        /// </summary>
        public string FildName { get; set; }

        /// <summary>
        /// Gets or sets 字段描述
        /// </summary>
        public string FildDescription { get; set; }

        /// <summary>
        /// Gets or sets 字段类型
        /// </summary>
        public FileType FildType { get; set; }

        /// <summary>
        /// Gets or sets 字段长度
        /// </summary>
        public int? FildLength { get; set; }

        /// <summary>
        /// Gets or sets 字段是否为空
        /// 0.为空;1.不为空
        /// </summary>
        public int? FildIsNull { get; set; }

        /// <summary>
        /// Gets or sets 页面元素类型
        /// </summary>
        public PageShowType PageShowType { get; set; }

        /// <summary>
        /// Gets or sets 字段初始值
        /// </summary>
        public string FildValue { get; set; }

        /// <summary>
        /// Gets or sets 字段权重
        /// </summary>
        public int FildWeight { get; set; }
    }
}
