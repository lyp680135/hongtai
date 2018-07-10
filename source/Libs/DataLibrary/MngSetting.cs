namespace DataLibrary
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text;

    /// <summary>
    /// 系统配配置表
    /// </summary>
    [Table("mngsetting")]
    public partial class MngSetting
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets 客户中文名称
        /// </summary>
        [MaxLength(255)]
        public string Client { get; set; }

        /// <summary>
        /// Gets or sets 客户英文名称
        /// </summary>
        [MaxLength(255)]
        public string ClientEn { get; set; }

        /// <summary>
        /// Gets or sets 系统名称中文
        /// </summary>
        [MaxLength(255)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets 系统名称英文
        /// </summary>
        [MaxLength(255)]
        public string NameEn { get; set; }

        /// <summary>
        /// Gets or sets 商交宝广告ID
        /// </summary>
        public int SJBAdvertiseId { get; set; }

        /// <summary>
        /// Gets or sets 炉批号初始配置
        /// </summary>
        [MaxLength(50)]
        public string BatCode { get; set; }

        /// <summary>
        /// Gets or sets 网站主域名
        /// </summary>
        [MaxLength(150)]
        public string Domain { get; set; }

        /// <summary>
        /// Gets or sets 网站PC端域名
        /// </summary>
        [MaxLength(150)]
        public string Domain_PC { get; set; }

        /// <summary>
        /// Gets or sets 网站WAP端域名
        /// </summary>
        [MaxLength(150)]
        public string Domain_WAP { get; set; }

        /// <summary>
        /// Gets or sets 网站WAP管理端域名
        /// </summary>
        [MaxLength(150)]
        public string Domain_WAPManage { get; set; }

        /// <summary>
        /// Gets or sets 网站WAP管理端域名
        /// </summary>
        [MaxLength(150)]
        public string Domain_WebApi { get; set; }

        /// <summary>
        /// Gets or sets 二维码短链接域名
        /// </summary>
        [MaxLength(150)]
        public string Domain_QRCode { get; set; }

        /// <summary>
        /// Gets or sets 图片路径
        /// </summary>
        [MaxLength(255)]
        public string ImgPath { get; set; }

        /// <summary>
        /// Gets or sets 销售网络请求API路径
        /// </summary>
        [MaxLength(255)]
        public string APIRoot { get; set; }

        /// <summary>
        /// Gets or sets 商交宝短信类别ID
        /// </summary>
        public int SJBSMSTypeId { get; set; }

        /// <summary>
        /// Gets or sets 商交宝短信发送接口密钥
        /// </summary>
        [MaxLength(100)]
        public string SJBSMSCode { get; set; }

        /// <summary>
        /// Gets or sets 企业网站模板ID
        /// </summary>
        public int SiteTemplateId { get; set; }

        /// <summary>
        /// Gets or sets 系统版本
        /// </summary>
        public EnumList.SystemVersion SystemVersion { get; set; }
    }
}
