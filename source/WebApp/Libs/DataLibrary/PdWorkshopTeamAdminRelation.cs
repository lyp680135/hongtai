namespace DataLibrary
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text;

    [Table("pdworkshopteamadminrelation")]
    /// <summary>
    /// 入库员班组关系表
    /// </summary>
    public partial class PdWorkshopTeamAdminRelation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AdminId { get; set; }

        [Required]
        public int WorkShopTeamId { get; set; }

        public int WorkShopId { get; set; }
    }
}
