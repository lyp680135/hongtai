using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataLibrary
{
    public partial class BaseProductMaterial
    {
        [Key]
        public int Id { get; set; }
        public int? Classid { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
    }
}
