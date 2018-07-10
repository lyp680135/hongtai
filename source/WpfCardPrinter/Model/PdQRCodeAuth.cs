using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataLibrary
{
    public partial class PdQRCodeAuth
    {
        [Key]
        public int Id { get; set; }
        public int WorkshopId { get; set; }
        public int Classid { get; set; }
        public int Materialid { get; set; }
        public int Specid { get; set; }
        public int Number { get; set; }
        public int AuthDate { get; set; }
        public int? Adder { get; set; }
        public int? Createtime { get; set; }
    }
}
