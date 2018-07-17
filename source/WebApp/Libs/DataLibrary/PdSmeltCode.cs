namespace DataLibrary
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text;

    [Table("pdsmeltcode")]
    public partial class PdSmeltCode
    {
        [Key]
        public int Id { get; set; }

        public string SmeltCode { get; set; }

        public int Qid { get; set; }
    }
}
