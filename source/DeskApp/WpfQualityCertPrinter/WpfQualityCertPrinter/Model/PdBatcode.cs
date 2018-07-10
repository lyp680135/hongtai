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
     
        public int? Createtime { get; set; }
        public string Adder { get; set; }
        public int? Status { get; set; }
    }
}
