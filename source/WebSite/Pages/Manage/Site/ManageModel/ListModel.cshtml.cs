namespace WarrantyManage.Pages.Manage.Site.ManageModel
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using Common.IService;
    using DataLibrary;
    using LinqKit;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Models;
    using Util.Helpers;

    public class ListModelModel : AuthorizeModel
    {
        private IBaseService<SiteModel> manageModel;

        public ListModelModel(DataContext db, IBaseService<SiteModel> manageModel)
          : base(db)
        {
            this.manageModel = manageModel;
        }

        public override int PageSize
        {
            get { return 10; }
        }

        public List<DataLibrary.SiteModel> ManageModels { get; set; }

        public List<DataLibrary.SiteCategory> ContentGroups { get; set; }

        public int PageIndex { get; set; }

        public string Description { get; set; }

        public int PageCount { get; set; }

        public new int Page { get; set; }

        public void OnGet(string description, int pg = 1)
        {
            this.PageIndex = pg;
            var predicate = PredicateBuilder.New<SiteModel>(true);
            int total = 0;
            if (!string.IsNullOrEmpty(description))
            {
                predicate.Extend(w => w.Description.Contains(description), PredicateOperator.And);
                this.Description = description;
            }

            this.ManageModels = this.manageModel.Page<int>(ref total, this.PageIndex, this.PageSize, p => p.Id, predicate, false);
            this.PageCount = total;

            this.ContentGroups = this.Db.SiteCategory.ToList();
        }
    }
}