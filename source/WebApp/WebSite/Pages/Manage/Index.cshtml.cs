namespace WarrantyManage.Pages.Manage
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DataLibrary;
    using Microsoft.Extensions.Options;
    using Models;

    public class IndexModel : AuthorizeModel
    {
        private Common.IService.IUserService userService;

        public IndexModel(DataLibrary.DataContext db, Common.IService.IUserService userService)
          : base(db)
        {
            this.userService = userService;
        }

        public DataLibrary.MngSetting Config { get; set; }

        /// <summary>
        /// Gets or sets 消息条数
        /// </summary>
        public int MsgCount { get; set; }

        /// <summary>
        /// Gets or sets 当前登录用户
        /// </summary>
        public new ApplicationUser User { get; set; }

        public List<DataLibrary.MngMenuclass> List_menu { get; set; }

        public int FirstId { get; set; }

        public void OnGet()
        {
            this.User = this.userService.ApplicationUser;

            int[] permissions = this.User.PermissionIds.ToArray();

            var menus = this.Db.MngMenuclass.Where(c => c.ParId == 0 && c.PermissionType == DataLibrary.EnumList.PermissionType.PC
            && c.IsPermission == false
            && permissions.Contains(c.Id)).OrderBy(c => c.Sequence).ToList();
            if (menus != null && menus.Count > 0)
            {
                this.FirstId = menus[0].Id;
            }

            this.List_menu = menus;

            this.Config = this.Db.MngSetting.FirstOrDefault();
            if (this.Config == null)
            {
                this.Config = new DataLibrary.MngSetting();
            }
        }
    }
}