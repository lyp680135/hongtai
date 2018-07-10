namespace WarrantyManage.Pages.Manage.Settings
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Models;

    public class WorkShopTeamModel : AuthorizeModel
    {
        public WorkShopTeamModel(DataLibrary.DataContext db)
          : base(db)
        {
        }

        public List<DataLibrary.PdWorkshop> Workshoplist { get; set; }

        public void OnGet()
        {
            this.Workshoplist = this.Db.PdWorkshop.OrderByDescending(c => c.CreateTime).ToList();
            string keyword = this.Request.Query["keyword"];
            string workshopId = this.Request.Query["WorkshopId"];
            string leader = this.Request.Query["Leader"];
            var query = this.Db.PdWorkshopTeam.OrderByDescending(p => p.CreateTime).ToList();
            if (!string.IsNullOrEmpty(keyword))
            {
                 query = query.Where(p => p.TeamName.Contains(keyword)).ToList();
            }

            if (!string.IsNullOrEmpty(workshopId))
            {
                 query = query.Where(p => p.WorkshopId == int.Parse(workshopId)).ToList();
            }

            if (!string.IsNullOrEmpty(leader))
            {
                query = query.Where(p => p.Leader.Contains(leader)).ToList();
            }

            this.ViewData["list"] = query;
            this.ViewData["keyword"] = keyword;
            this.ViewData["WorkshopId"] = workshopId;
            this.ViewData["Leader"] = leader;
        }
    }
}