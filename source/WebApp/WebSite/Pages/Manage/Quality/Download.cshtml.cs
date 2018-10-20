namespace WarrantyManage.Pages.Manage.Quality
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
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
        private Util.Helpers.MySqlHelper mySqlHelper;

        public DownloadModel(DataContext db, IBaseService<SalePrintlog> salePrintlog, Util.Helpers.MySqlHelper mySqlHelper)
          : base(db)
        {
            this.salePrintlog = salePrintlog;
            this.mySqlHelper = mySqlHelper;
        }

        public override int PageSize
        {
            get { return 10; }
        }

        public List<DataLibrary.SalePrintlog> SalePrintLogList { get; set; }

        public List<ViewModelLog> ViewModelLogList { get; set; }

        public int PageIndex { get; set; }

        public string Printno { get; set; }

        public string StartTime { get; set; }

        public string EndTime { get; set; }

        public int PageCount { get; set; }

        public new int Page { get; set; }

        public string BatCode { get; set; }

        public string Lpn { get; set; }

        public void OnGet(string batCode = "", string lpn = "", string printno = "", string startTime = "", string endTime = "", int pg = 1)
        {
            this.PageIndex = pg;

            // var predicate = PredicateBuilder.New<SalePrintlog>(true);
            // predicate.Extend(
            //    w => w.Status == (int)SalePrintlogStatus.已下载 ||
            //    w.Status == (int)SalePrintlogStatus.已撤回, PredicateOperator.And);
            var sqlStr = new StringBuilder();
            sqlStr.Append(" select a.*,c.Batcode,c.Lpn from saleprintlog  a ");
            sqlStr.Append(" inner JOIN saleprintlogdetail b on a.id=b.printid ");
            sqlStr.Append(" inner JOIN salesellerauth c on b.authid=c.Id ");
            sqlStr.AppendFormat(" where (a.status ={0} or a.status={1}) ", (int)SalePrintlogStatus.已下载, (int)SalePrintlogStatus.已撤回);

            // int total = 0;
            if (!string.IsNullOrEmpty(printno))
            {
                this.Printno = printno;
                sqlStr.AppendFormat(" and  a.printno ='{0}' ", printno);

                // 添加查询条件,PredicateOperator.And查询谓词
                // predicate.Extend(w => w.Printno == printno, PredicateOperator.And);
            }

            if (!DateTime.TryParse(startTime, out DateTime sTiem))
            {
                startTime = DateTime.Today.ToString("yyyy-MM-dd");
            }
            else
            {
                this.StartTime = startTime;
                sqlStr.AppendFormat(" and a.createtime >= {0} ", sTiem.GetUnixTimeFromDateTime());

                // 添加查询条件,PredicateOperator.And查询谓词
                // predicate.Extend(w => System.Convert.ToDateTime(w.Createtime.ToLong().GetDateTimeFromUnixTime().ToString("yyyy-MM-dd")) >= sTiem, PredicateOperator.And);
            }

            if (!DateTime.TryParse(endTime, out DateTime eTime))
            {
                endTime = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");
            }
            else
            {
                this.EndTime = endTime;
                sqlStr.AppendFormat(" and  a.createtime <= {0} ", eTime.GetUnixTimeFromDateTime());

                // 添加查询条件,PredicateOperator.And查询谓词
                // predicate.Extend(w => System.Convert.ToDateTime(w.Createtime.ToLong().GetDateTimeFromUnixTime().ToString("yyyy-MM-dd")) <= eTime, PredicateOperator.And);
            }

            if (!string.IsNullOrEmpty(batCode))
            {
                this.BatCode = batCode;
                sqlStr.AppendFormat(" and  c.Batcode ='{0}' ", batCode);
            }

            if (!string.IsNullOrEmpty(lpn))
            {
                this.Lpn = lpn;
                sqlStr.AppendFormat(" and  c.lpn like '%{0}%' ", lpn);
            }

            // this.SalePrintLogList = this.salePrintlog.Page<int>(ref total, this.PageIndex, this.PageSize, p => p.Id, predicate, false);
            sqlStr.Append(" GROUP BY a.printno,b.number,a.Createtime,a.consignor, a.id,c.Lpn,c.Batcode  ORDER BY  a.id desc ");
            this.PageCount = this.mySqlHelper.ExecuteList<ViewModelLog>(sqlStr.ToString(), null, System.Data.CommandType.Text).Count;
            sqlStr.AppendFormat(" LIMIT {0},{1} ", (this.PageIndex - 1) * this.PageSize, this.PageSize);
            this.ViewModelLogList = this.mySqlHelper.ExecuteList<ViewModelLog>(sqlStr.ToString(), null, System.Data.CommandType.Text);
        }

        public class ViewModelLog : SalePrintlog
        {
            public string BatCode { get; set; }

            public string Lpn { get; set; }
        }
    }
}