namespace WarrantyManage.Pages.Manage
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DataLibrary;
    using Models;

    public class LeftModel : AuthorizeModel
    {
        private Common.IService.IUserService userService;

        public LeftModel(DataContext db, Common.IService.IUserService userService)
           : base(db)
        {
            this.userService = userService;
        }

        public int MyId { get; set; }

        public new ApplicationUser User { get; set; }

        public List<DataLibrary.MngMenuclass> List_menu { get; set; }

        public void OnGet(int parId)
        {
            this.User = this.userService.ApplicationUser;

            this.List_menu = this.Db.MngMenuclass.Where(c => c.ParId == parId
            && c.IsPermission == false
            && c.PermissionType == DataLibrary.EnumList.PermissionType.PC && this.User.PermissionIds.ToArray().Contains(c.Id)).OrderBy(c => c.Sequence).ToList();

            this.MyId = parId;
        }
    }
}