namespace WarrantyManage.Pages.Manage.Settings
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.EntityFrameworkCore;
    using Models;

    public class WorkShopModel : AuthorizeModel
    {
        public WorkShopModel(DataLibrary.DataContext db)
         : base(db)
        {
        }

        public string Keyword { get; set; }

        public void OnGet()
        {
            this.Keyword = this.Request.Query["keyword"];

            var query = from c in this.Db.PdWorkshop select c;

            if (!string.IsNullOrEmpty(this.Keyword))
            {
                query = query.Where(p => p.Name.Contains(this.Keyword));
            }

            var queryResult = query.OrderBy(p => p.Id).ToList();

            this.ViewData["keyword"] = this.Keyword;
            this.ViewData["list"] = queryResult;
        }
    }
}