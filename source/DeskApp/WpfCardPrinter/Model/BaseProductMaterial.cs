using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataLibrary
{
    public partial class BaseProductMaterial
    {
        [Key]
        public int Id { get; set; }
        public int Classid { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }

        //UI使用
        public string Classname { get; set; }
        public string GbClassname { get; set; }
        public int? Deliverytype { get; set; }
        public int? Measurement { get; set; }

        public string Gbdocument { get; set; }

        public int MaterialIsCancel { get; set; }
    }
}
