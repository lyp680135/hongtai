namespace DataLibrary
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using static DataLibrary.EnumList;

    /// <summary>
    /// 质量检测数据力学（JsonObect存的数据）
    /// </summary>
    public class PdQualityInfo_Dynamics
    {
        public double? 下屈服强度 { get; set; }

        public double? 抗拉强度 { get; set; }

        public double? 伸长率A { get; set; }

        public double? 伸长率Agt { get; set; }

        public double? 强屈比 { get; set; }

        public double? 屈屈比 { get; set; }

        public string 冷弯 { get; set; }

        public double? 冷弯弯心 { get; set; }

        public string 反弯 { get; set; }

        public double? 反弯弯心 { get; set; }
    }
}
