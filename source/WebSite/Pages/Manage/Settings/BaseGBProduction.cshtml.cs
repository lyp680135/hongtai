namespace WarrantyManage.Pages.Manage.Settings
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Models;

    public class BaseGBProductionModel : AuthorizeModel
    {
        public BaseGBProductionModel(DataLibrary.DataContext db)
          : base(db)
        {
        }

        public string Keyword { get; set; }

        public List<DataLibrary.BaseGbProduction> List { get; set; }

        public void OnGet()
        {
            this.Keyword = this.Request.Query["keyword"];

            var query = from c in this.Db.BaseGbProduction select c;

            if (!string.IsNullOrEmpty(this.Keyword))
            {
                query = query.Where(p => p.Name.Contains(this.Keyword));
            }

            var queryResult = query.OrderBy(p => p.Id).ToList();

            this.List = queryResult;
            this.ViewData["keyword"] = this.Keyword;
        }
    }
}