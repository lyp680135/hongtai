using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfCardPrinter.Model
{
    public class PdStockOut
    {
        public int Id { get; set; }
        public int Adder { get; set; }
        public int Createtime { get; set; }
        public string Lpn { get; set; }
        public int Productid { get; set; }
        public int Sellerid { get; set; }
    }
}
