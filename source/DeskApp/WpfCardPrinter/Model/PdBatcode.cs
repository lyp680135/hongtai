using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataLibrary
{
    public partial class PdBatcode
    {
        [Key]
        public int Id { get; set; }
        public string Batcode { get; set; }

        public int Serialno { get; set; }
     
        public int? Createtime { get; set; }
        public string Adder { get; set; }
        public int? Status { get; set; }

        /// <summary>
        /// Gets or sets 车间号
        /// </summary>
        public int Workshopid { get; set; }

        /// <summary>
        /// Gets or sets 钢坯支数
        /// </summary>
        public int? Billetnumber { get; set; }

        /// <summary>
        /// Gets or sets 钢坯单重
        /// </summary>
        public double? Billetpieceweight { get; set; }

        /// <summary>
        /// Gets or sets 成材率
        /// </summary>
        public double? Productrate { get; set; }
    }
}
