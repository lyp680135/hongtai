namespace DataLibrary
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("saleseller")]
    public partial class SaleSeller
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(255)]
        [Required]
        public string Name { get; set; }

        [MaxLength(50)]
        [Required]
        public string Mobile { get; set; }

        public int? Createtime { get; set; }

        public int? Parent { get; set; }
    }
}
