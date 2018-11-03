using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfQualityCertPrinter.ViewModel
{
    public class MaterialList
    {
        public int Materialid { get; set; }

        /// <summary>
        /// 材质名称
        /// </summary>
        public string Materialname { get; set; }

        /// <summary>
        /// 所属产品名称
        /// </summary>
        public String Classname { get; set; }

        /// <summary>
        /// 界面展示
        /// </summary>
        public string ShowName { get; set; }

        /// <summary>
        /// 计量方式
        /// </summary>
        public DataLibrary.EnumList.MeteringMode Measurement { get; set; }

        /// <summary>
        /// Gets or sets  0:未作废 ：1作废
        /// </summary>
        public DataLibrary.EnumList.MaterialIsCancel MaterialIsCancel { get; set; }
      
    }
}
