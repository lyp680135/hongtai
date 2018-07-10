namespace DataLibrary
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text;
    using static DataLibrary.EnumList;

    /// <summary>
    /// 手机验证码-刘钟
    /// </summary>
    [Table("basemobilecode")]
    public partial class BaseMobileCode
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets 手机号
        /// </summary>
        [MaxLength(20)]
        [Required]
        public string Mobile { get; set; }

        /// <summary>
        /// Gets or sets 手机短信验证码发送类别
        /// </summary>
        [Required]
        public MobileCodeType CodeType { get; set; }

        /// <summary>
        /// Gets or sets 验证码
        /// </summary>
        [MaxLength(10)]
        [Required]
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets 系统会员类别
        /// </summary>
        [Required]
        public SystemMemberType MemberType { get; set; }

        /// <summary>
        /// Gets or sets 发送时间
        /// </summary>
        public DateTime Sendtime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether 是否已验证
        /// </summary>
        public bool IsValaid { get; set; }
    }
}
