using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfQualityCertPrinter.Model
{
   public class GJSelectModel
    {
        public string printno { get; set; }
        public long createtime { get; set; }
        public string consignor { get; set; }
        public string name { get; set; }
        public string lpn { get; set; }
       public int? Status { get; set; }
    }
}
