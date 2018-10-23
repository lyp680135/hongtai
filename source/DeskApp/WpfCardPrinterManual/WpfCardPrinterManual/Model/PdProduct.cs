using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfCardPrinterManual.Model
{
    public class PdProduct
    {
        public int Id { get; set; }
        public string Batcode { get; set; }
        public string ClassName { get; set; }
        public string MaterialName{ get; set; }
        public string SpecName { get; set; }       
        public double? Length { get; set; }
        public string Bundlecode { get; set; }
        public int? Piececount { get; set; }
        public double? Meterweight { get; set; }
        public double? Weight { get; set; }
        public int? Createtime { get; set; }             
        public string Randomcode { get; set; }
        public string GBStandard { get; set; }
    }
}
