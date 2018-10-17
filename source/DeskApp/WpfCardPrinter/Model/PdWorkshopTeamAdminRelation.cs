using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfCardPrinter.Model
{
    public class PdWorkshopTeamAdminRelation
    {
       
        public int Id { get; set; }
       
        public int AdminId { get; set; }

        public int WorkShopTeamId { get; set; }

        public int WorkShopId { get; set; }
    }
}
