using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace WpfQualityCertPrinter.Model
{
    public class LoginLog
    {
        [Key]
        public int Id { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        [Required]
        public string UserName { get; set; }
        public long LoginTime { get; set; }
        /// <summary>
        /// Gets or sets 真实姓名
        /// </summary>
        public string RealName { get; set; }
        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; }
    }
}
