namespace DataLibrary
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using static DataLibrary.EnumList;

    /// <summary>
    /// 质量检测数据（JsonObect存的数据）
    /// </summary>
    public class PdQualityInfo
    {
        public double? 重量偏差 { get; set; }

        public double? C { get; set; }

        public double? Si { get; set; }

        public double? Mn { get; set; }

        public double? P { get; set; }

        public double? S { get; set; }

        public double? Cr { get; set; }

        public double? V { get; set; }

        public double? Mo { get; set; }

        public double? Cu { get; set; }

        public double? Ni { get; set; }

        public double? Nb { get; set; }

        public double? As { get; set; }

        public double? Ceq { get; set; }
    }
}
