namespace WarrantyManage.Pages.Manage.Site
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Models;

    public class EditorSinglePageModel : AuthorizeModel
    {
        public EditorSinglePageModel(DataLibrary.DataContext db)
         : base(db)
        {
        }

        public new DataLibrary.SiteSinglePage Page { get; set; }

        public bool IsSuccess { get; set; }

        public string NewsTag { get; set; }

        public void OnGet(int id)
        {
            var query = from p in this.Db.SiteSinglePage select p;
            this.Page = query.FirstOrDefault(p => p.Id == id);
            this.IsSuccess = false;
        }

        public void OnPostFirst(int hiddId,  string title, string content, string createTime, string descrption)
        {
            this.IsSuccess = true;
            var pageclass = this.Db.SiteSinglePage.FirstOrDefault(n => n.Id == hiddId);
            pageclass.Title = title;
            pageclass.Content = content;
            pageclass.Descrption = descrption;
            pageclass.Createtime = (int)Util.Extensions.GetUnixTimeFromDateTime(Convert.ToDateTime(createTime));
            this.Db.SiteSinglePage.Update(pageclass);
            this.Page = pageclass;
            this.Db.SaveChanges();
        }
    }
}