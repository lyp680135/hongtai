using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataLibrary
{
    public partial class PdWorkshopTeamLog
    {
        [Key]
        public int Id { get; set; }
        public int WorkshopId { get; set; }
        public int TeamId { get; set; }
        public string Batcode { get; set; }
        public string Adder { get; set; }
        public int CreateTime { get; set; }
    }
}
