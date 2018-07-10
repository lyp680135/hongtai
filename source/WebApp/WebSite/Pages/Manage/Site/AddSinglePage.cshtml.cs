namespace WarrantyManage.Pages.Manage.Site
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Models;

    public class AddSinglePageModel : AuthorizeModel
    {
        public AddSinglePageModel(DataLibrary.DataContext db)
         : base(db)
        {
        }

        public bool IsSuccess { get; set; }

        public List<DataLibrary.SiteSinglePage> List { get; set; }

        public void OnGet()
        {
        }

        public void OnPostFirst(string title, string content, string descrption, string createTime)
        {
            var singlepage = new DataLibrary.SiteSinglePage()
            {
                Title = title,
                Content = content,
                Descrption = descrption,
                Createtime = (int)Util.Extensions.GetUnixTimeFromDateTime(Convert.ToDateTime(createTime))
            };

            this.Db.SiteSinglePage.Add(singlepage);
            this.Db.SaveChanges();
            if (singlepage.Id > 0)
            {
                this.List = (from c in this.Db.SiteSinglePage select c).ToList();
                this.IsSuccess = true;
            }
        }
    }
}