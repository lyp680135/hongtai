namespace WarrantyManage.Pages.Manage.Site.ManageModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Models;

    public class EditModelModel : AuthorizeModel
    {
        public EditModelModel(DataLibrary.DataContext db)
          : base(db)
        {
        }

        public List<DataLibrary.SiteModelColumn> BaseManageModels { get; set; }

        public string ModelName { get; set; }

        public int ManageModelId { get; set; }

        public void OnGet(int id)
        {
            this.ManageModelId = id;
            var manageModel = this.Db.SiteModel.FirstOrDefault(f => f.Id == id);
            if (manageModel == null)
            {
                return;
            }

            this.ModelName = manageModel.Description;
            this.BaseManageModels = new List<DataLibrary.SiteModelColumn>();
            var bmmId = manageModel.BaseManageId.Split(',').ToList();
            bmmId.ForEach(o =>
            {
                var baseManageModelInfo = this.Db.SiteModelColumn.FirstOrDefault(f => f.Id == Convert.ToInt32(o));
                if (baseManageModelInfo != null)
                {
                    this.BaseManageModels.Add(baseManageModelInfo);
                }
            });
            this.BaseManageModels = this.BaseManageModels.OrderBy(o => o.Id).ToList();
        }
    }
}