namespace WarrantyManage.Pages.Manage.Quality
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Models;
    using Util;
    using static DataLibrary.EnumList;

    public class ListModel : AuthorizeModel
    {
        public ListModel(DataLibrary.DataContext db)
             : base(db)
        {
        }

        public List<DataLibrary.PdQuality> List_Quality { get; set; }

        public int? Status { get; set; }

        public string BatCodeNumber { get; set; }

        public int PageIndex { get; set; } = 1;

        public int PSize { get; set; } = 5;

        public int Total { get; set; }

        public void OnGet(string batCode, int? chkStatus)
        {
            int page = this.Request.Query["page"].ToInt();

            this.PageIndex = page > 0 ? page : 1;

            this.BatCodeNumber = batCode;
            this.Status = chkStatus;

            var query = this.Db.PdQuality.Where(w => w.CreateFlag == 0);
            if (!string.IsNullOrEmpty(batCode))
            {
                query = query.Where(c => c.Batcode.Contains(batCode));
            }

            if (chkStatus.HasValue)
            {
                if (chkStatus != -2)
                {
                    query = query.Where(c => (int)c.CheckStatus == chkStatus.Value);
                }
            }

            this.Total = query.Count();
            this.List_Quality = query.OrderByDescending(c => c.Id).Skip((this.PageIndex - 1) * this.PSize).Take(this.PSize).ToList();
        }
    }
}