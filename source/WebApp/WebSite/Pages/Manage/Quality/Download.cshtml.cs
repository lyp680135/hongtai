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
    using Util.Helpers;
    using static DataLibrary.EnumList;

    public class DownloadModel : AuthorizeModel
    {
        private IBaseService<SalePrintlog> salePrintlog;

        public DownloadModel(DataContext db, IBaseService<SalePrintlog> salePrintlog)
          : base(db)
        {
            this.salePrintlog = salePrintlog;
        }

        public override int PageSize
        {
            get { return 10; }
        }

        public List<DataLibrary.SalePrintlog> SalePrintLogList { get; set; }

        public int PageIndex { get; set; }

        public string Printno { get; set; }

        public string StartTime { get; set; }

        public string EndTime { get; set; }

        public int PageCount { get; set; }

        public new int Page { get; set; }

        public void OnGet(string printno = "", string startTime = "", string endTime = "", int pg = 1)
        {
            this.PageIndex = pg;
            var predicate = PredicateBuilder.New<SalePrintlog>(true);
            predicate.Extend(
                w => w.Status == (int)SalePrintlogStatus.已下载 ||
                w.Status == (int)SalePrintlogStatus.已撤回, PredicateOperator.And);

            int total = 0;
            if (!string.IsNullOrEmpty(printno))
            {
                this.Printno = printno;

                // 添加查询条件,PredicateOperator.And查询谓词
                predicate.Extend(w => w.Printno == printno, PredicateOperator.And);
            }

            if (!DateTime.TryParse(startTime, out DateTime sTiem))
            {
                startTime = DateTime.Today.ToString("yyyy-MM-dd");
            }
            else
            {
                this.StartTime = startTime;

                // 添加查询条件,PredicateOperator.And查询谓词
                predicate.Extend(w => System.Convert.ToDateTime(w.Createtime.ToLong().GetDateTimeFromUnixTime().ToString("yyyy-MM-dd")) >= sTiem, PredicateOperator.And);
            }

            if (!DateTime.TryParse(endTime, out DateTime eTime))
            {
                endTime = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");
            }
            else
            {
                this.EndTime = endTime;

                // 添加查询条件,PredicateOperator.And查询谓词
                predicate.Extend(w => System.Convert.ToDateTime(w.Createtime.ToLong().GetDateTimeFromUnixTime().ToString("yyyy-MM-dd")) <= eTime, PredicateOperator.And);
            }

            this.SalePrintLogList = this.salePrintlog.Page<int>(ref total, this.PageIndex, this.PageSize, p => p.Id, predicate, false);
            this.PageCount = total;
        }
    }
}