namespace DataLibrary
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text;

    [Table("sitebasic")]
    public partial class SiteBasic
    {
        [Key]
        public int Id { get; set; }

        // 网站中文标题
        [MaxLength(255)]
        public string SiteTitle { get; set; }

        // 网站英文标题
        [MaxLength(255)]
        public string SiteTitleEn { get; set; }

        // 公司名称
        [MaxLength(255)]
        public string CompName { get; set; }

        // 公司地址
        [MaxLength(255)]
        public string CompAddress { get; set; }

        // 网站描述
        [MaxLength(500)]
        public string SiteDesc { get; set; }

        // 首页轮播图
        public JsonObject<List<string>> CarouselImage { get; set; }

        // 网站logo
        [MaxLength(255)]
        public string Logo { get; set; }

        // 网站logo文字
        [MaxLength(255)]
        public string LogoText { get; set; }

        // 前台网站名称备案号
        [MaxLength(50)]
        public string Icp { get; set; }

        // 前台网站版权
        [MaxLength(255)]
        public string Copyright { get; set; }

        // 统计代码
        [MaxLength(1000)]
        public string StatsCode { get; set; }

        // 经纬度
        [MaxLength(255)]
        public string MapLocation { get; set; }
    }
}
