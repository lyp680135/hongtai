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

    public class GBNameModel : AuthorizeModel
    {
        public GBNameModel(DataLibrary.DataContext db)
          : base(db)
        {
        }

        public string Keyword { get; set; }

        public void OnGet()
        {
            this.Keyword = this.Request.Query["keyword"];

            var query = from c in this.Db.BaseProductGbClass select c;

            if (!string.IsNullOrEmpty(this.Keyword))
            {
                query = query.Where(p => p.Gbname.Contains(this.Keyword));
            }

            var queryResult = query.OrderBy(p => p.Id).ToList();

            this.ViewData["list"] = queryResult;
            this.ViewData["keyword"] = this.Keyword;
        }
    }
}