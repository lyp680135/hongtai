namespace DataLibrary
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text;

    /// <summary>
    /// 工作车间表
    /// </summary>
    [Table("pdworkshop")]
    public partial class PdWorkshop
    {
        [Key]
        public int Id { get; set; }

        // 车间名称
        [MaxLength(50)]
        [Required]
        public string Name { get; set; }

        // 车间生产线代码
        [MaxLength(50)]
        [Required]
        public string Code { get; set; }

        [MaxLength(255)]
        public string Address { get; set; }

        [MaxLength(50)]
        public string Manager { get; set; }

        // 入库操作员
        [MaxLength(50)]
        public string Inputer { get; set; }

        // 出库操作员
        [MaxLength(50)]
        public string Outputer { get; set; }

        // 质量录入员
        [MaxLength(50)]
        public string QAInputer { get; set; }

        // 炉号工
        [MaxLength(50)]
        public string Furnacer { get; set; }

        public int? CreateTime { get; set; }
    }
}
