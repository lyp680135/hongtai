namespace DataLibrary
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("pdproduct")]
    public partial class PdProduct
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(50)]
        [Required]
        public string Batcode { get; set; }

        public int? Classid { get; set; }

        public int? Materialid { get; set; }

        public int? Specid { get; set; }

        public int? Lengthtype { get; set; }

        public double? Length { get; set; }

        [MaxLength(50)]
        public string Bundlecode { get; set; }

        public int? Piececount { get; set; }

        public double? Meterweight { get; set; }

        public double? Weight { get; set; }

        public int? Createtime { get; set; }

        public int? Adder { get; set; }

        public int? WorkShift { get; set; }

        [MaxLength(50)]
        public string Randomcode { get; set; }
    }
}
