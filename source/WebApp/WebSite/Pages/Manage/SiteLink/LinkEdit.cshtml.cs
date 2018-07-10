namespace WarrantyManage.Pages.Manage.SiteLink
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Models;

    public class LinkEditModel : AuthorizeModel
    {
        public LinkEditModel(DataLibrary.DataContext db)
            : base(db)
        {
        }

        public string Msg { get; set; }

        public DataLibrary.SiteLink SiteLink { get; set; }

        public void OnGet(int id)
        {
            this.Msg = string.Empty;
            var siteLink = this.Db.SiteLink.FirstOrDefault(c => c.Id == id);
            if (siteLink != null)
            {
                this.SiteLink = siteLink;
            }
            else
            {
                this.RedirectToError();
            }
        }

        public void OnPost(int id, string name)
        {
            this.Msg = string.Empty;
            var temp = this.Db.SiteLink.FirstOrDefault(c => c.Name == name && c.Id != id);
            if (temp != null)
            {
            }
            else
            {
                var lin = this.Db.SiteLink.FirstOrDefault(c => c.Id == id);
                this.SiteLink = lin;
            }
        }
    }
}