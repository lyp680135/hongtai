namespace WarrantyManage.Pages.Manage.Site.ManageModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Models;

    public class AddModelModel : AuthorizeModel
    {
        public AddModelModel(DataLibrary.DataContext db)
           : base(db)
        {
        }

        public void OnGet()
        {
        }
    }
}