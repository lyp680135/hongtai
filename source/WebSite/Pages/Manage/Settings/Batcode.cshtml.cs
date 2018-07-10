namespace WarrantyManage.Pages.Manage.Settings
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Common.IService;
    using Common.Service;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.EntityFrameworkCore;
    using Models;
    using Newtonsoft.Json.Linq;

    public class BatcodeModel : AuthorizeModel
    {
        private readonly IUserService userService;

        private IBatcodeService mBatcodeService;

        public BatcodeModel(DataLibrary.DataContext db, IBatcodeService batcodeService, IUserService userService)
          : base(db)
        {
            this.mBatcodeService = batcodeService;
            this.userService = userService;
        }

        public void OnGet()
        {
            // 当前登陆人员
            DataLibrary.MngAdmin admin = this.userService.ApplicationUser.Mng_admin;
            var query = from c in this.Db.PdWorkshop select c;

            if (admin.GroupManage.Object.ToList().Contains(Convert.ToInt32(DataLibrary.EnumList.GroupManage.炉号工)))
            {
                query = query.Where(c => c.Furnacer.Contains(admin.Id.ToString()));
            }

            var queryResult = query.OrderBy(p => p.Id).ToList();

            JArray arr = new JArray();
            for (int i = 0; i < queryResult.Count; i++)
            {
                string batcode = string.Empty;
                var lastbatcode = this.mBatcodeService.SingleLast(queryResult[i].Code);
                if (lastbatcode != null)
                {
                    batcode = lastbatcode.Batcode;
                }

                var obj = new JObject()
                {
                    { "Id", queryResult[i].Id },
                    { "Name", queryResult[i].Name },
                    { "Code", queryResult[i].Code },
                    { "Manager", queryResult[i].Manager },
                    { "LastBatcode", batcode },
                };

                arr.Add(obj);
            }

            this.ViewData["list"] = arr;
        }
    }
}