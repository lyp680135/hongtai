using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataLibrary
{
    public partial class PdWorkshopTeam
    {
        [Key]
        public int Id { get; set; }
        public int WorkshopId { get; set; }
        public string TeamName { get; set; }
        public string Leader { get; set; }
        public string Adder { get; set; }
        public int? CreateTime { get; set; }
    }
}
