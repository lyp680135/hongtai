namespace WarrantyManage.Pages.Manage.SiteLink
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Models;
    using Util;
    using static DataLibrary.EnumList;

    public class LinkListModel : AuthorizeModel
    {
        public LinkListModel(DataLibrary.DataContext db)
            : base(db)
        {
        }

        public List<DataLibrary.SiteLink> List_Q { get; set; }

        public int PageIndex { get; set; } = 1;

        public int Psize { get; set; } = 10;

        public int Total { get; set; }

        public void OnGet()
        {
            int page = this.Request.Query["page"].ToInt();

            this.PageIndex = page > 0 ? page : 1;
            string name = this.Request.Query["Name"];
            var query = from c in this.Db.SiteLink select c;
            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(p => p.Name.Contains(name) || p.Url.Contains(name) || p.Description.Contains(name));
            }

            this.List_Q = query.OrderByDescending(p => p.Sequence).Skip((this.PageIndex - 1) * this.Psize).Take(this.Psize).ToList();
            this.Total = query.Count();
            this.ViewData["Name"] = name;
        }
    }
}