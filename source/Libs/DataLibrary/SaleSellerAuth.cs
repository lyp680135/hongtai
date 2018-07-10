namespace DataLibrary
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("salesellerauth")]
    public partial class SaleSellerAuth
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int? Classid { get; set; }

        [Required]
        public int? Materialid { get; set; }

        [Required]
        public int? Specid { get; set; }

        [MaxLength(50)]
        [Required]
        public string Batcode { get; set; }

        public int? Number { get; set; }

        [MaxLength(50)]
        public string Lpn { get; set; }

        public int Sellerid { get; set; }

        public int? Parentseller { get; set; }

        public int? Createtime { get; set; }

        /// <summary>
        /// Gets or sets 0.定尺1.非尺
        /// </summary>
        public int? Lengthtype { get; set; }
    }
}
