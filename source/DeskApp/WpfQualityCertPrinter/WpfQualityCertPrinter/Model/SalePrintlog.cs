namespace DataLibrary
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class SalePrintlog
    {
        [Key]
        public int Id { get; set; }

        public int? Authid { get; set; }

        public int? Number { get; set; }

        /// <summary>
        /// Gets or sets 收货单位
        /// </summary>
        public string Consignor { get; set; }

        /// <summary>
        /// Gets or sets 质保书打印编号
        /// </summary>
        public string Printno { get; set; }

        /// <summary>
        /// Gets or sets 质保书印章角度
        /// </summary>
        public int? Signetangle { get; set; }

        public int? Createtime { get; set; }

        /// <summary>
        /// Gets or sets 状态：0-预览未下载  1-已下载
        /// </summary>
        public int? Status { get; set; }

        /// <summary>
        /// Gets or sets 质量证明书随机校验码
        /// </summary>
        public string Checkcode { get; set; }


        /// <summary>
        /// UI用的售达方字段
        /// </summary>
        public int Sellerid { get; set; }

        /// <summary>
        /// UI用的售达方名称字段
        /// </summary>
        public string Sellername { get; set; }

        /// <summary>
        /// UI用的车牌号字段
        /// </summary>
        public string Lpn { get; set; }
    }
}
