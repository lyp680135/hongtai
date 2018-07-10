namespace WarrantyManage.Pages.Authority
{
    using System;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc;
    using Models;

    public class UserAddModel : AuthorizeModel
    {
        public UserAddModel(DataLibrary.DataContext db)
             : base(db)
        {
        }

        public string Msg { get; set; }

        [BindProperty]
        public string UserName { get; set; }

        [BindProperty]
        public string Password { get; set; }

        [BindProperty]
        public string RealName { get; set; }

        [BindProperty]
        public int Sex { get; set; }

        [BindProperty]
        public int[] GroupManage { get; set; }

        public void OnGet()
        {
            this.Msg = string.Empty;
        }

        public void OnPostFirst()
        {
            this.Msg = string.Empty;

            this.UserName = this.UserName.TrimEnd();
            if (this.Db.MngAdmin.FirstOrDefault(c => c.UserName == this.UserName) != null)
            {
                this.Msg = "该登录手机号己存在！";
            }else if (this.Db.MngAdmin.FirstOrDefault(c => c.RealName == this.RealName) != null)
            {
                this.Msg = "该姓名己存在，建议加以区分！";
            }
            else
            {
                this.Password = Util.Helpers.Encrypt.Md5By32(this.Password);
                this.Db.MngAdmin.Add(new DataLibrary.MngAdmin()
                {
                    DepartId = 1,
                    GroupManage = this.GroupManage.ToList(),
                    InJob = true,
                    Password = this.Password,
                    RealName = this.RealName,
                    Sex = Convert.ToBoolean(this.Sex),
                    UserName = this.UserName,
                    FirstChar = string.Empty,
                });
                if (this.Db.SaveChanges() > 0)
                {
                    this.Msg = "添加成功！";
                }
            }
        }
    }
}