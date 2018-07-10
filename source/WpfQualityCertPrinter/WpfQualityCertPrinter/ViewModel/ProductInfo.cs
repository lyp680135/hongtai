using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfQualityCertPrinter.ViewModel
{
    public class ProductInfo
    {
        public int Id { get; set; }
        public string Batcode { get; set; }
        public int Classid { get; set; }
        public String Classname { get; set; }
        public int Materialid { get; set; }
        public string Materialname { get; set; }
        public int Specid { get; set; }
        public string Specfullname { get; set; }
        public string Length { get; set; }
        public int Lengthtype { get; set; }
        public string Lengthnote { get; set; }
        public int Piececount { get; set; }
        public string Bundlecode { get; set; }
        public int Workshiftid { get; set; }
        public string Workshiftname { get; set; }
        public bool Checked { get; set; }
    }
}
