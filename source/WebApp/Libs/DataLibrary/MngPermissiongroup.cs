namespace DataLibrary
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("mngpermissiongroup")]
    public partial class MngPermissiongroup
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(50)]
        [Required]
        public string GroupName { get; set; }

        [MaxLength(100)]
        [Required]
        public string Description { get; set; }

        public bool? BeLock { get; set; }

        public int Sequence { get; set; }

        public bool IsCanDelete { get; set; }
    }
}
