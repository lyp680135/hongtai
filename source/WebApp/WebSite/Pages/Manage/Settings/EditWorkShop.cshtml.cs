namespace WarrantyManage.Pages.Manage.Settings
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Models;
    using Newtonsoft.Json;
    using static DataLibrary.EnumList;

    public class EditWorkShopModel : AuthorizeModel
    {
        public EditWorkShopModel(DataLibrary.DataContext db)
          : base(db)
        {
        }

        public DataLibrary.PdWorkshop Wsclass { get; set; }

        public List<DataLibrary.PdWorkshopTeam> Teamlist { get; set; }

        public void OnGet(int id)
        {
            var rs = this.Db.PdWorkshop.FirstOrDefault(p => p.Id == id);
            if (rs != null)
            {
                this.Wsclass = rs;
                int teamid = rs.Id;
                this.Teamlist = this.Db.PdWorkshopTeam.Where(c => c.WorkshopId == teamid).ToList();
            }
            else
            {
                this.RedirectToError();
            }
        }
    }
}