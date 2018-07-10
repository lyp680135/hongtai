namespace WarrantyManage.Pages.Manage.SiteLink
{
    using Models;

    public class LinkAddModel : AuthorizeModel
    {
        private Common.IService.IUserService userService;

        public LinkAddModel(DataLibrary.DataContext db, Common.IService.IUserService userService)
            : base(db)
        {
            this.userService = userService;
        }

        public void OnGet()
        {
        }
    }
}
