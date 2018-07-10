namespace WarrantyManage.Pages.Manage
{
    using System.Linq;
    using DataLibrary;
    using Models;

    public class DefaultModel : AuthorizeModel
    {
        public DefaultModel(DataLibrary.DataContext db)
         : base(db)
        {
        }

        public void OnGet()
        {
        }
    }
}