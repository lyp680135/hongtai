namespace WarrantyManage.Pages.Manage.Quality
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Common.IService;
    using DataLibrary;
    using LinqKit;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Models;
    using Util;

    public class TemplateModel : AuthorizeModel
    {
        private IBaseService<PdQuality> pdQuality;

        public TemplateModel(DataContext db, IBaseService<PdQuality> pdQuality)
          : base(db)
        {
            this.pdQuality = pdQuality;
        }

        public override int PageSize
        {
            get { return 15; }
        }

        public List<DataLibrary.PdQuality> PdQualityList { get; set; }

        public int PageIndex { get; set; }

        public string CheckCode { get; set; }

        public string StartTime { get; set; }

        public string EndTime { get; set; }

        public int PageCount { get; set; }

        public int MaterialId { get; set; }

        public new int Page { get; set; }

        public void OnGet(string startTime = "", string endTime = "", int pg = 1, int materialId = 0)
        {
            this.PageIndex = pg;
            var predicate = PredicateBuilder.New<PdQuality>(true);
            int total = 0;
            predicate.Extend(w => w.CreateFlag == 1, PredicateOperator.And);

            this.MaterialId = materialId;
            predicate.Extend(w => w.MaterialId == materialId, PredicateOperator.And);

            if (!string.IsNullOrEmpty(startTime))
            {
                this.StartTime = startTime;
                DateTime.TryParse(startTime, out DateTime sTiem);
                // 添加查询条件,PredicateOperator.And查询谓词
                predicate.Extend(w => System.Convert.ToDateTime(w.Createtime.ToLong().GetDateTimeFromUnixTime().ToString("yyyy-MM-dd")) >= sTiem, PredicateOperator.And);
            }

            if (!string.IsNullOrEmpty(endTime))
            {
                this.EndTime = endTime;
                DateTime.TryParse(endTime, out DateTime eTime);

                // 添加查询条件,PredicateOperator.And查询谓词
                predicate.Extend(w => System.Convert.ToDateTime(w.Createtime.ToLong().GetDateTimeFromUnixTime().ToString("yyyy-MM-dd")) <= eTime, PredicateOperator.And);
            }

            this.PdQualityList = this.pdQuality.Page<int>(ref total, this.PageIndex, this.PageSize, p => p.Id, predicate, false);
            this.PageCount = total;
        }
    }
}