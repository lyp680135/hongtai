namespace WarrantyManage.Pages.Manage.Settings
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Models;

    public class ProductMaterialModel : AuthorizeModel
    {
        public ProductMaterialModel(DataLibrary.DataContext db)
         : base(db)
        {
        }

        public string Keyword { get; set; }

        public string Pname { get; set; }

        public void OnGet()
        {
            this.Keyword = this.Request.Query["keyword"];
            this.Pname = this.Request.Query["pname"];
            var query = from c in this.Db.BaseProductMaterial select c;
            var query2 = from p in this.Db.BaseProductClass select p;

            if (!string.IsNullOrEmpty(this.Keyword))
            {
                query = query.Where(p => p.Name.Contains(this.Keyword));
            }

            if (!string.IsNullOrEmpty(this.Pname))
            {
                query = query.Where(p => p.Classid == int.Parse(this.Pname));
            }

            var queryResult = query.OrderBy(p => p.Id).ToList();
            var queryResult2 = query2.OrderBy(p => p.Id).ToList();

            this.ViewData["keyword"] = this.Keyword;
            this.ViewData["pname"] = this.Pname;
            this.ViewData["list"] = queryResult;
            this.ViewData["plist"] = queryResult2;
        }
    }
}