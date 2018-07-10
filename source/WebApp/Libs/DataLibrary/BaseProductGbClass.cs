namespace DataLibrary
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("baseproductgbclass")]
    public partial class BaseProductGbClass
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(50)]
        public string Gbname { get; set; }

        [MaxLength(255)]
        public string Note { get; set; }

        public int? Createtime { get; set; }
    }
}
