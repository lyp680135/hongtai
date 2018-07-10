namespace DataLibrary
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("pdqrcodeprintedlog")]
    public partial class PdQRCodePrintedLog
    {
        [Key]
        public int Id { get; set; }

        public int WorkshopId { get; set; }

        public int ProductId { get; set; }

        public int SpecId { get; set; }

        public int Number { get; set; }

        // 锁定状态：0-临时放开 1-锁定
        public int Status { get; set; }

        public int? Createtime { get; set; }

        public int? Adder { get; set; }
    }
}
