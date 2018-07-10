namespace WarrantyManage.Pages.Manage.Site.ContentGroup
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Models;
    using Util.Helpers;

    public class ListModel : AuthorizeModel
    {
        public ListModel(DataLibrary.DataContext db)
           : base(db)
        {
        }

        public string ListStr { get; set; }

        public string ModelStr { get; set; }

        public int PageIndex { get; set; }

        public int PageCount { get; set; }

        public new int Page { get; set; }

        public new int PageSize { get; set; }

        public void OnGet(int? pg)
        {
            this.PageSize = 15;
            this.Page = pg ?? 1;
            this.PageIndex = (pg.HasValue && pg < 1) ? 0 : (this.Page - 1) * this.PageSize;
            List<DataLibrary.SiteCategory> list = (from c in this.Db.SiteCategory select c).OrderByDescending(c => c.Sequence).ToList();
            this.PageCount = list.Count;
            this.ListStr = Json.ToJson(list);
            this.ModelStr = Json.ToJson((from c in this.Db.SiteModel select c).ToList());
        }
    }
}