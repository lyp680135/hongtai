namespace DataLibrary
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("pdworkshopteam")]
    public partial class PdWorkshopTeam
    {
        [Key]
        public int Id { get; set; }

        public int WorkshopId { get; set; }

        [MaxLength(50)]
        public string TeamName { get; set; }

        [MaxLength(50)]
        public string Code { get; set; }

        [MaxLength(50)]
        public string Leader { get; set; }

        [MaxLength(50)]
        public string Adder { get; set; }

        public int? CreateTime { get; set; }
    }
}
