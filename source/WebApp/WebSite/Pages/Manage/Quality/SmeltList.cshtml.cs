namespace WarrantyManage.Pages.Manage.Quality { 
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
        private IBaseService<PdSmeltCode> pdSmeltCode;

        public SmeltListModel(DataContext db, IBaseService<PdSmeltCode> pdSmeltCode)
          : base(db)
        {
            this.pdSmeltCode = pdSmeltCode;
        }

        public override int PageSize
        {
            get { return 10; }
        }

        public List<DataLibrary.PdSmeltCode> ManageModels { get; set; }

        public int PageIndex { get; set; }

        public string Description { get; set; }

        public int PageCount { get; set; }

        public new int Page { get; set; }

        public void OnGet(int pg = 1)
        {
            this.PageIndex = pg;
            var predicate = PredicateBuilder.New<PdSmeltCode>(true);
            int total = 0;
            this.ManageModels = this.pdSmeltCode.Page<int>(ref total, this.PageIndex, this.PageSize, p => p.Id, predicate, false);
            this.PageCount = total;
        }
    }
}