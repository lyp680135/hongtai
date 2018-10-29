using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataLibrary
{
    public partial class BaseSpecifications
    {
        [Key]
        public int Id { get; set; }
        public int? Classid { get; set; }
        public int? Materialid { get; set; }
        public string Callname { get; set; }
        public string Specname { get; set; }
        public double? Referlength { get; set; }
        public double? Refermeterweight { get; set; }
        public double? Referpieceweight { get; set; }
        public int? Referpiececount { get; set; }

        //供界面使用
        public string FullSpecname { get; set; }
    }
}
