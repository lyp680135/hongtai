namespace DataLibrary
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Text;

    /// <summary>
    /// 工作车间表
    /// </summary>
    public partial class PdWorkshop
    {
        [Key]
        public int Id { get; set; }

        // 车间名称
        [Required]
        public string Name { get; set; }

        // 车间生产线代码
        [Required]
        public string Code { get; set; }

        public string Address { get; set; }

        public string Manager { get; set; }

        // 入库操作员
        public string Inputer { get; set; }

        // 出库操作员
        public string Outputer { get; set; }

        // 质量录入员
        public string QAInputer { get; set; }

        public int? CreateTime { get; set; }
    }
}
