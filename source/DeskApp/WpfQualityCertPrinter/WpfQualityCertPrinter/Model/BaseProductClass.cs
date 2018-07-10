using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataLibrary
{
    public partial class BaseProductClass
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Gbname { get; set; }
        public string Note { get; set; }
        public int? Createtime { get; set; }
    }
}
