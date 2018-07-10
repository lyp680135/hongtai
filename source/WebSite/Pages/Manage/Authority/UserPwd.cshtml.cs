namespace WarrantyManage.Pages.Manage.Authority
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Models;

    public class UserPwdModel : AuthorizeModel
    {
        private Common.IService.IUserService userService;

        public UserPwdModel(DataLibrary.DataContext db, Common.IService.IUserService userService)
             : base(db)
        {
            this.userService = userService;
        }

        public string Msg { get; set; }

        public void OnGet()
        {
        }

        public void OnPostFirst(string oldPassword, string newPassword, string passwordEnsure)
        {
            this.Msg = string.Empty;

            if (newPassword != passwordEnsure)
            {
                this.Msg = "两次输入的密码不一致";
                return;
            }

            oldPassword = Util.Helpers.Encrypt.Md5By32(oldPassword);
            newPassword = Util.Helpers.Encrypt.Md5By32(newPassword);

            var updUser = this.Db.MngAdmin.FirstOrDefault(c => c.Id == this.userService.ApplicationUser.Mng_admin.Id);
            if (updUser == null)
            {
                this.RedirectToError();
            }
            else
            {
                if (updUser.Password.ToUpper() != oldPassword.ToUpper())
                {
                    this.Msg = "原密码输入错误，请重新输入";
                }
                else
                {
                    updUser.Password = newPassword;

                    this.Db.Update(updUser);
                    this.Db.SaveChanges();
                    this.Msg = "操作成功";
                }
            }
        }
    }
}