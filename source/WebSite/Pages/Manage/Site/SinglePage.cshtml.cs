namespace WarrantyManage.Pages.Manage.Site
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Models;

    public class SinglePageModel : AuthorizeModel
    {
        public SinglePageModel(DataLibrary.DataContext db)
          : base(db)
        {
        }

        public List<DataLibrary.SiteSinglePage> List { get; set; }

        public void OnGet()
        {
            var keyword = this.Request.Query["keyword"];
            var query = from n in this.Db.SiteSinglePage select n;
            if (!string.IsNullOrEmpty(keyword))
            {
               query = query.Where(n => n.Title.Contains(keyword));
            }

            foreach (var page in query.ToList())
            {
                page.Descrption = Util.Extensions.RemoveHtml(page.Descrption);
                if (page.Descrption.Length > 30)
                {
                    page.Descrption = page.Descrption.Substring(0, 30) + "...";
                }
            }

            this.List = query.ToList();

            this.ViewData["keyword"] = keyword;
        }
    }
}