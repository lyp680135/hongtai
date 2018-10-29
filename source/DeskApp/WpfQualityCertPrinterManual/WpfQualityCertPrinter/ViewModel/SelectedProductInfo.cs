using DataLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace WpfQualityCertPrinter.ViewModel
{
    public class SelectedProductInfo
    {
        public string Batcode { get; set; }
        public int Classid { get; set; }
        public String Classname { get; set; }
        public int Materialid { get; set; }
        public string Materialname { get; set; }
        public int Specid { get; set; }
        public string Length { get; set; }
        public int Lengthtype { get; set; }
        public string Lengthnote { get; set; }
        public string Specfullname { get; set; }
        public int Startbundle { get; set; }
        public int Endbundle { get; set; }
        public int Number { get; set; }
        public int Printnumber { get; set; }

        public ObservableCollection<BaseSpecifications> BaseSpec { get; set; }
    }
}
