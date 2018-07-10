namespace WarrantyManage.Pages.Authority
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Models;

    public class UserModel : AuthorizeModel
    {
        public UserModel(DataLibrary.DataContext db)
         : base(db)
        {
        }

        public List<DataLibrary.MngAdmin> AdminList { get; set; }

        public new int PageSize { get; set; }

        public int PageIndex { get; set; }

        public int PageCount { get; set; }

        public new int Page { get; set; }

        public void OnGet(int? pg)
        {
            this.Page = pg ?? 1;
            this.PageSize = 10;
            this.PageIndex = (pg.HasValue && pg < 1) ? 0 : (this.Page - 1) * this.PageSize;
            var realName = this.Request.Query["RealName"];
            var userName = this.Request.Query["UserName"];
            var groupManage = this.Request.Query["GroupManage"];
            var inJob = this.Request.Query["InJob"];
            var query = from c in this.Db.MngAdmin select c;
            if (!string.IsNullOrEmpty(realName))
            {
                query = query.Where(p => p.RealName.Contains(realName));
            }

            if (!string.IsNullOrEmpty(userName))
            {
                query = query.Where(p => p.UserName.Contains(userName));
            }

            if (!string.IsNullOrEmpty(groupManage))
            {
                query = query.Where(p => p.GroupManage.Object.Contains(int.Parse(groupManage)));
            }

            if (!string.IsNullOrEmpty(inJob))
            {
                var flag = int.Parse(inJob) == 1 ? true : false;
                query = query.Where(p => p.InJob == flag);
            }

            var queryResult = query.OrderByDescending(p => p.Id).Skip(this.PageIndex).Take(this.PageSize).ToList();
            this.PageCount = query.Count();

            this.AdminList = queryResult;
            this.ViewData["RealName"] = realName;
            this.ViewData["UserName"] = userName;
            this.ViewData["GroupManage"] = groupManage;
            this.ViewData["InJob"] = inJob;
        }
    }
}