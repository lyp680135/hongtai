namespace DataLibrary
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class SaleSeller
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Mobile { get; set; }

        public int? Createtime { get; set; }

        public int? Parent { get; set; }
    }
}
