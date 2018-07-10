namespace WarrantyManage.Pages.Manage.Settings
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Models;

    public class BaseQuanlityModel : AuthorizeModel
    {
        public BaseQuanlityModel(DataLibrary.DataContext db)
         : base(db)
        {
        }

        public List<DataLibrary.BaseProductClass> PMlist { get; set; }

        public List<DataLibrary.BaseProductMaterial> Czlist { get; set; }

        public List<DataLibrary.BaseQualityStandard> ZlList { get; set; }

        public List<DataLibrary.BaseGbProduction> Qslist { get; set; }

        public string TargetName { get; set; }

        public int Mid { get; set; }

        public int PageIndex { get; set; }

        public int PageCount { get; set; }

        public new int PageSize { get; set; }

        public new int Page { get; set; }

        public void OnGet(int? pg, string targetName, int mid)
        {
            this.Page = pg ?? 1;
            this.PageSize = 10;
            this.PageIndex = (pg.HasValue && pg < 1) ? 0 : (this.Page - 1) * this.PageSize;
            var queryqs = from q in this.Db.BaseGbProduction select q;
            this.Qslist = queryqs.OrderBy(p => p.Id).ToList();
            var query = from p in this.Db.BaseProductClass select p;
            this.PMlist = query.OrderBy(p => p.Id).ToList();
            var czquery = from c in this.Db.BaseProductMaterial select c;
            this.Czlist = czquery.OrderBy(c => c.Id).ToList();
            this.ZlList = this.Db.BaseQualityStandard.ToList();

            if (!string.IsNullOrEmpty(targetName))
            {
                this.ZlList = this.ZlList.Where(w => w.TargetName.ToUpper() == targetName.ToUpper()).ToList();
                this.TargetName = targetName;
            }

            if (mid > 0)
            {
                this.ZlList = this.ZlList.Where(w => w.Materialid == mid).ToList();
                this.Mid = mid;
            }

            this.PageCount = this.ZlList.Count();
            var queryList = this.ZlList.OrderBy(z => z.Id).Skip(this.PageIndex).Take(this.PageSize).ToList();
            this.ZlList = queryList;
        }
    }
}