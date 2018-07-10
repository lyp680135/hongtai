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
        [Required]
        public string Name { get; set; }

        [MaxLength(255)]
        public string Description { get; set; }

        [MaxLength(255)]
        [Required]
        public string Url { get; set; }

        [MaxLength(255)]
        [Required]
        public string PicLink { get; set; }

        [Required]
        public int? PicWidth { get; set; }

        [Required]
        public int? PicHeight { get; set; }

        [Required]
        public int? IsShow { get; set; }

        [Required]
        public int Sequence { get; set; }

        [Required]
        public int? Position { get; set; }

        [Required]
        public int? CreateTime { get; set; }

        [Required]
        public int? AdminId { get; set; }

        [Required]
        public int? LinkType { get; set; }
    }
}
