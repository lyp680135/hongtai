using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DataLibrary
{
    /// <summary>
    /// 系统配配置表
    /// </summary>
    public partial class MngSetting
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 系统名称中文
        /// </summary>
        public string Client { get; set; }

        /// <summary>
        /// 系统名称英文
        /// </summary>
        public string ClientEn { get; set; }

        /// <summary>
        /// 系统名称中文
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 系统名称英文
        /// </summary>
        public string NameEn { get; set; }

        /// <summary>
        /// 商交宝广告ID
        /// </summary>
        public int SJBAdvertiseId { get; set; }

        /// <summary>
        /// 炉批号初始配置
        /// </summary>
        public string BatCode { get; set; }
        /// <summary>
        /// 网站主域名
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// 网站PC端域名
        /// </summary>
        public string Domain_PC { get; set; }

        /// <summary>
        /// 网站WAP端域名
        /// </summary>
        public string Domain_WAP { get; set; }

        /// <summary>
        /// 网站WAP管理端域名
        /// </summary>
        public string Domain_WAPManage { get; set; }

        /// <summary>
        /// 网站WAP管理端域名
        /// </summary>
        public string Domain_WebApi { get; set; }

        /// <summary>
        /// 图片路径
        /// </summary>
        public string ImgPath { get; set; }

        /// <summary>
        /// 销售网络请求API路径
        /// </summary>
        public string APIRoot { get; set; }

        /// <summary>
        /// Gets or sets 系统版本
        /// </summary>
        public EnumList.SystemVersion SystemVersion { get; set; }
    }
}

