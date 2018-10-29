namespace DataLibrary
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class SaleSellerAuth
    {
        [Key]
        public int Id { get; set; }

        public int? Classid { get; set; }

        public int? Materialid { get; set; }

        public int? Specid { get; set; }

        public string Batcode { get; set; }

        public int? Number { get; set; }

        public string Lpn { get; set; }

        public int Sellerid { get; set; }

        public int? Parentseller { get; set; }

        public int? Createtime { get; set; }

        /// <summary>
        /// Gets or sets 0.定尺1.非尺
        /// </summary>
        public int? Lengthtype { get; set; }

        /// <summary>
        /// UI用的售达方名称
        /// </summary>
        public string Sellername { get; set; }
    }
}
