﻿namespace DataLibrary
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text;

    [Table("pdqualitysmeltcode")]
    public partial class PdqualitySmeltCode
    {
        [Key]
        public int Id { get; set; }

        public string SmeltCode { get; set; }

        public int EntryPerson { get; set; }

        public int Createtime { get; set; }

        /// <summary>
        ///  Gets or sets 0:未作废,1:已作废
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// Gets or sets 产品质量数据- 化学
        /// </summary>
        public JsonObject<object> Chemistry { get; set; }
    }
}
