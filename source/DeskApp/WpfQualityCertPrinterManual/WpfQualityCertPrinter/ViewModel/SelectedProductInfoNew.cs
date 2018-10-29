using DataLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace WpfQualityCertPrinter.ViewModel
{
    public class SelectedProductInfoNew
    {
        public string Batcode { get; set; }
        public int Materialid { get; set; }
        public string Spec { get; set; }

        public int? Length { get; set; }
        public float? SingleWeight { get; set; }
        public int? Printnumber { get; set; }
    }
}
