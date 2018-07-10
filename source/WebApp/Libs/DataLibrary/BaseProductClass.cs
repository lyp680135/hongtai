﻿namespace DataLibrary
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("baseproductclass")]
    public partial class BaseProductClass
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(50)]
        public string Name { get; set; }

        [MaxLength(50)]
        public string Gbname { get; set; }

        // 交货类型
        public int DeliveryType { get; set; }

        // 计量方式
        public int Measurement { get; set; }

        [MaxLength(255)]
        public string Note { get; set; }

        public int? Createtime { get; set; }
    }
}