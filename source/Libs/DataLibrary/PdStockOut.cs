namespace DataLibrary
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("pdstockout")]
    public partial class PdStockOut
    {
        [Key]
        public int Id { get; set; }

        public int? Productid { get; set; }

        public int? Sellerid { get; set; }

        [MaxLength(50)]
        public string Lpn { get; set; }

        public int? Createtime { get; set; }

        public int? Adder { get; set; }
    }
}
