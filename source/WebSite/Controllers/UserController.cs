namespace WarrantyManage.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;

    public class UserController : Controller
    {
        private DataLibrary.DataContext db;

        public UserController(DataLibrary.DataContext db)
        {
            this.db = db;
        }

        public IActionResult Index()
        {
            return this.View();
        }

        /// <summary>
        /// 更改经销商帐号可用状态
        /// </summary>
        /// <param name="sType">sType</param>
        /// <param name="id">id</param>
        /// <returns>是否成功</returns>
        [HttpPost]
        public string UserAdminSetUse(string sType, int id)
        {
            var result = this.db.MngAdmin.First(c => c.Id == id);
            if (result != null)
            {
                if (sType == "no")
                {
                    result.InJob = false;
                }

                if (sType == "yes")
                {
                    result.InJob = true;
                }

                if (this.db.SaveChanges() > 0)
                {
                    return "True";
                }
            }
            else
            {
                return "该操作员不存在或者己删除！";
            }

            return "False";
        }

        [HttpPost]
        public ActionResult UserAdminAdd(string userName, string password, string realName, int sex, int groupManage)
        {
            string msg = string.Empty;

            password = Util.Helpers.Encrypt.Md5By32(password);

            // List<DataCenter.Mng_PermissionGroup> ListPermission = new List<Mng_PermissionGroup>();

                // ListPermission = (from c in _db.Mng_PermissionGroup select c).ToList();
                userName = userName.TrimEnd();

                if (this.db.MngAdmin.FirstOrDefault(c => c.UserName == userName) != null)
                {
                    msg = "该用户名己存在！";
                }
                else
                {
                this.db.MngAdmin.Add(new DataLibrary.MngAdmin()
                    {
                        DepartId = 1,
                        GroupManage = groupManage.ToString(),
                        InJob = true,
                        Password = password,
                        RealName = realName,
                        Sex = Convert.ToBoolean(sex),
                        UserName = userName
                    });
                    if (this.db.SaveChanges() > 0)
                    {
                        msg = "添加成功！";
                    }
                }

            this.ViewBag.Msg = msg;

            // ViewBag.ListPermission = ListPermission;
            return this.View();
        }
    }
}