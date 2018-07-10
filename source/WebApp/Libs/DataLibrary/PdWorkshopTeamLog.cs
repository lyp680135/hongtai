namespace DataLibrary
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("pdworkshopteamlog")]
    public partial class PdWorkshopTeamLog
    {
        [Key]
        public int Id { get; set; }

        public int WorkshopId { get; set; }

        public int TeamId { get; set; }

        [MaxLength(50)]
        public string Batcode { get; set; }

        [MaxLength(50)]
        public string Adder { get; set; }

        public int CreateTime { get; set; }
    }
}
