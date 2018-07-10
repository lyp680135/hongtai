using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataLibrary
{
    public partial class PdProduct
    {
        [Key]
        public int Id { get; set; }
        public string Batcode { get; set; }
        public int? Classid { get; set; }
        public int? Materialid { get; set; }
        public int? Specid { get; set; }
        public int? Lengthtype { get; set; }
        public double? Length { get; set; }
        public string Bundlecode { get; set; }
        public int? Piececount { get; set; }
        public double? Meterweight { get; set; }
        public double? Weight { get; set; }
        public int? Createtime { get; set; }
        public int? Adder { get; set; }

        public int? WorkShift { get; set; }

        public string Randomcode { get; set; }

        /** UI控件使用 **/
        public string Classname { get; set; }
        public string Materialname { get; set; }
        public string Specname { get; set; }
        public string Shiftname { get; set; }

        public double? ReferWeight { get; set; }

        public int BundlecodeValue { get; set; }
    }
}
