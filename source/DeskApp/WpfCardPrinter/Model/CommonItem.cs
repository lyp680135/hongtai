using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataLibrary
{
    public partial class CommonItem
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
