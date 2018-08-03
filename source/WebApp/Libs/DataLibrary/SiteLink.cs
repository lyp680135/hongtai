namespace DataLibrary
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text;

    [Table("sitelink")]
    public partial class SiteLink
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(50)]
        public string Name { get; set; }

        [MaxLength(255)]
        public string Description { get; set; }

        [MaxLength(255)]
        public string Url { get; set; }

        [MaxLength(255)]
        public string PicLink { get; set; }

        public int? PicWidth { get; set; }

        public int? PicHeight { get; set; }

        public int? IsShow { get; set; }

        public int Sequence { get; set; }

        [Required]
        public int? Position { get; set; }

        public int? CreateTime { get; set; }

        public int? AdminId { get; set; }

        public int? LinkType { get; set; }
    }
}
