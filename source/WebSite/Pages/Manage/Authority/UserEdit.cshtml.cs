namespace WarrantyManage.Pages.Manage.Authority
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Models;

    public class UserEditModel : AuthorizeModel
    {
        public UserEditModel(DataLibrary.DataContext db)
            : base(db)
        {
        }

        public string Msg { get; set; }

        public DataLibrary.MngAdmin Admin { get; set; }

        public void OnGet(int id)
        {
            var mng = this.Db.MngAdmin.FirstOrDefault(c => c.Id == id);
            if (mng != null)
            {
                this.Admin = mng;
                this.Msg = string.Empty;
            }
            else
            {
                this.RedirectToError();
            }
        }

        public void OnPost(int id, string userName, string password, string realName, int sex, int[] groupManage)
        {
            this.Msg = string.Empty;

            userName = userName.TrimEnd();
            var temp = this.Db.MngAdmin.FirstOrDefault(c => c.UserName == userName && c.Id != id);
            if (temp != null)
            {
                this.Msg = "该登录手机号己存在！";
                this.Admin = new DataLibrary.MngAdmin()
                {
                    GroupManage = groupManage.ToList(),
                    Password = password,
                    RealName = realName,
                    Sex = Convert.ToBoolean(sex),
                    UserName = userName,
                };
            }
            else if (this.Db.MngAdmin.FirstOrDefault(c => c.RealName == realName && c.Id != id) != null)
            {
                this.Msg = "该姓名己存在，建议加以区分！";
                this.Admin = new DataLibrary.MngAdmin()
                {
                    GroupManage = groupManage.ToList(),
                    Password = password,
                    RealName = realName,
                    Sex = Convert.ToBoolean(sex),
                    UserName = userName,
                };
            }
            else
            {
                var adminClass = this.Db.MngAdmin.FirstOrDefault(c => c.Id == id);
                if (adminClass != null)
                {
                    adminClass.UserName = userName;
                    adminClass.RealName = realName;
                    adminClass.GroupManage = groupManage.ToList();
                    adminClass.Sex = Convert.ToBoolean(sex);
                    if (!string.IsNullOrEmpty(password))
                    {
                        adminClass.Password = Util.Helpers.Encrypt.Md5By32(password);
                    }

                    this.Db.SaveChanges();
                    this.Msg = "修改成功！";
                }
                else
                {
                    this.RedirectToError();
                }

                this.Admin = adminClass;
            }
        }
    }
}