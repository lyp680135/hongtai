namespace WarrantyManage.Pages.Manage.Site.ManageModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Models;

    public class InfoModelModel : AuthorizeModel
    {
        public InfoModelModel(DataLibrary.DataContext db)
          : base(db)
        {
        }

        public List<DataLibrary.SiteModelColumn> BaseManageModel { get; set; }

        public void OnGet(int id)
        {
            this.BaseManageModel = new List<DataLibrary.SiteModelColumn>();
            var manageModelInfo = this.Db.SiteModel.FirstOrDefault(f => f.Id == id);
            if (manageModelInfo == null)
            {
                return;
            }

            var bmmId = manageModelInfo.BaseManageId.Split(',').ToList();
            if (bmmId.Count <= 0)
            {
                return;
            }

            bmmId.ForEach(o =>
            {
                var baseManageInfo = this.Db.SiteModelColumn.FirstOrDefault(f => f.Id == Convert.ToInt32(o));
                if (baseManageInfo != null)
                {
                    this.BaseManageModel.Add(baseManageInfo);
                }
            });
            this.BaseManageModel = this.BaseManageModel.OrderBy(o => o.Id).ToList();
        }
    }
}