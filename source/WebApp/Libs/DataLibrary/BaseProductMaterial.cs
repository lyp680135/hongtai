namespace DataLibrary
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using static DataLibrary.EnumList;

    [Table("baseproductmaterial")]
    public partial class BaseProductMaterial
    {
        [Key]
        public int Id { get; set; }

        public int? Classid { get; set; }

        [MaxLength(50)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets 材质的标准强度
        /// </summary>
        public int Standardstrength { get; set; }

        [MaxLength(255)]
        public string Note { get; set; }

        [MaxLength(50)]
        public string Templatename { get; set; }

        /// <summary>
        /// Gets or sets  0:未作废 ：1作废
        /// </summary>
        public MaterialIsCancel MaterialIsCancel { get; set; }
    }
}
