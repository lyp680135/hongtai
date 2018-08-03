namespace WarrantyManage.Pages.Manage.Quality
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

    public class SmeltListModel : AuthorizeModel
    {
        private IBaseService<PdqualitySmeltCode> pdSmeltCode;

        public SmeltListModel(DataContext db, IBaseService<PdqualitySmeltCode> pdSmeltCode)
          : base(db)
        {
            this.pdSmeltCode = pdSmeltCode;
        }

        public override int PageSize
        {
            get { return 10; }
        }

        public List<DataLibrary.PdqualitySmeltCode> ManageModels { get; set; }

        public int PageIndex { get; set; }

        public string Description { get; set; }

        public int PageCount { get; set; }

        public string SmeltCode { get; set; }

        public string Type { get; set; }

        public new int Page { get; set; }

        public void OnGet(int pg = 1, string smeltCode = "", string type = "")
        {
            this.PageIndex = pg;
            var predicate = PredicateBuilder.New<PdqualitySmeltCode>(true);
            if (!string.IsNullOrEmpty(type))
            {
                predicate.Extend(w => w.Status == 0, PredicateOperator.And);
            }
            if (!string.IsNullOrWhiteSpace(smeltCode))
            {
                predicate.Extend(w => w.SmeltCode == smeltCode.Trim(), PredicateOperator.And);
                this.SmeltCode = smeltCode;
            }

            if (!string.IsNullOrEmpty(type))
            {
                this.Type = type;
            }

            int total = 0;
            this.ManageModels = this.pdSmeltCode.Page<int>(ref total, this.PageIndex, this.PageSize, p => p.Id, predicate, false);
            this.PageCount = total;
        }
    }
}