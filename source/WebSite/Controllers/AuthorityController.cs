namespace WarrantyManage.Controllers
{
    using System;
    using System.Linq;
    using DataLibrary;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Authorize]
    public class AuthorityController : Controller
    {
        private DataContext db;
        private Common.IService.IUserService userService;

        public AuthorityController(DataContext db, Common.IService.IUserService userService)
        {
            this.db = db;
            this.userService = userService;
        }

        [HttpPost]
        public bool MenuCreate(string menuName, string menuUrl, int hiddParId, bool isPermission, int permissionType)
        {
            using (var tran = this.db.Database.BeginTransaction())
            {
                try
                {
                    var parMenu = this.db.MngMenuclass.FirstOrDefault(c => c.Id == hiddParId);

                    int depth = parMenu == null ? 0 : parMenu.Depth + 1;

                    MngMenuclass modelMenu = new MngMenuclass()
                    {
                        ChildNum = 0,
                        ClassName = menuName,
                        Depth = depth,
                        IsCanDelete = true, // 手动新增的默认都可删除
                        IsPermission = isPermission,
                        ParId = hiddParId,
                        ParPath = string.Empty,
                        PermissionType = (EnumList.PermissionType)permissionType,
                        Sequence = Util.Extensions.ToInt(this.db.MngMenuclass.Where(c => c.Depth == depth).Max(c => c.Sequence)) + 1,
                        Url = menuUrl,
                    };
                    this.db.MngMenuclass.Add(modelMenu);

                    if (this.db.SaveChanges() > 0)
                    {
                        modelMenu.ParPath = parMenu == null ? (modelMenu.Id + ",") : (parMenu.ParPath + (modelMenu.Id + ","));
                        this.db.MngMenuclass.Update(modelMenu); // 更改路径标识

                        if (parMenu != null)
                        {
                            parMenu.ChildNum++;
                        } // 更新父级的 ChildNum

                        this.db.SaveChanges();
                        tran.Commit();
                        return true;
                    }
                    else
                    {
                        tran.Rollback();
                        return false;
                    }
                }
                catch
                {
                    tran.Rollback();
                    return false;
                }
            }
        }

        [HttpPost]
        public bool MenuEdit(string menuName, string menuUrl, bool isPermission, int hiddParId, int hiddId, int permissionType)
        {
            var result = this.db.MngMenuclass.FirstOrDefault(c => c.Id == hiddId);
            if (result != null)
            {
                result.ClassName = menuName;
                result.Url = menuUrl;
                result.IsPermission = isPermission;
                result.PermissionType = (EnumList.PermissionType)permissionType;

                this.db.SaveChanges();
                return true;
            }

            return false;
        }

        [HttpPost]
        public bool MenuDelete(int id)
        {
            using (var tran = this.db.Database.BeginTransaction())
            {
                try
                {
                    // 要删除的当前菜单
                    var result = this.db.MngMenuclass.FirstOrDefault(c => c.Id == id);
                    if (result != null && result.IsCanDelete)
                    {
                        // 更改当前 层次的 排序
                        var query = this.db.MngMenuclass.Where(c => c.Depth == result.Depth && c.Sequence > result.Sequence);
                        foreach (var q in query)
                        {
                            q.Sequence--;
                        }

                        // 删除自己及其子类
                        var del = this.db.MngMenuclass.Where(c => c.ParPath.Contains(result.ParPath)).ToList();
                        this.db.RemoveRange(del);

                        // 更改父级类 ChildNum
                        var parent = this.db.MngMenuclass.FirstOrDefault(c => c.Id == result.ParId);
                        if (parent != null)
                        {
                            parent.ChildNum = parent.ChildNum - 1;
                        }

                        this.db.SaveChanges();
                        tran.Commit();
                        return true;
                    }
                }
                catch
                {
                    tran.Rollback();
                    return false;
                }
            }

            return false;
        }

        [HttpPost]
        public bool MenuOrderUp(int id)
        {
            var result = this.db.MngMenuclass.FirstOrDefault(c => c.Id == id);
            if (result != null && result.Sequence > 0)
            {
                // 获取前一条
                var pre = this.db.MngMenuclass.Where(c => c.Depth == result.Depth && c.ParId == result.ParId && c.Sequence < result.Sequence)
                    .OrderByDescending(c => c.Sequence).FirstOrDefault();
                if (pre != null)
                {
                    int tempSequence = result.Sequence; // 临时记录
                    // 改变自己
                    result.Sequence = pre.Sequence;
                    pre.Sequence = tempSequence;
                }

                // 改变前一条
                this.db.SaveChanges();

                return true;
            }

            return false;
        }

        [HttpPost]
        public bool MenuOrderDown(int id)
        {
            var result = this.db.MngMenuclass.FirstOrDefault(c => c.Id == id);
            if (result != null && result.Sequence > 0)
            {
                // 获取后一条
                var next = this.db.MngMenuclass.Where(c => c.Depth == result.Depth && c.ParId == result.ParId && c.Sequence > result.Sequence)
                    .OrderBy(c => c.Sequence).FirstOrDefault();
                if (next != null)
                {
                    int tempSequence = result.Sequence; // 临时记录
                    // 改变自己
                    result.Sequence = next.Sequence;
                    next.Sequence = tempSequence;
                }

                // 改变前一条
                this.db.SaveChanges();

                return true;
            }

            return false;
        }

        /// <summary>
        /// 角色编辑
        /// </summary>
        /// <param name="groupName">组名 </param>
        /// <param name="title">标题</param>
        /// <param name="hiddId">ID</param>
        /// <returns>返回信息</returns>
        [HttpPost]
        public string PermissionGroupEdit(string groupName, string title, int hiddId)
        {
            var model = this.db.MngPermissiongroup.FirstOrDefault(c => c.Id == hiddId);
            if (model != null)
            {
                if (this.db.MngPermissiongroup.Count(c => c.GroupName == groupName && c.Id != hiddId) > 0)
                {
                    return "己存在相同的角色名！";
                }

                model.Description = !string.IsNullOrEmpty(title) ? title : string.Empty;
                model.GroupName = groupName;

                this.db.SaveChanges();
                return "true";
            }

            return "false";
        }

        /// <summary>
        /// 角色添加
        /// </summary>
        /// <param name="groupName">组名</param>
        /// <param name="title">标题</param>
        /// <returns>返回信息</returns>
        [HttpPost]
        public string PermissionGroupAdd(string groupName, string title)
        {
            if (!string.IsNullOrEmpty(groupName))
            {
                if (this.db.MngPermissiongroup.FirstOrDefault(c => c.GroupName == groupName) != null)
                {
                    return "己存在相同的角色名！";
                }
                else
                {
                    this.db.MngPermissiongroup.Add(new MngPermissiongroup()
                    {
                        GroupName = groupName,
                        Description = !string.IsNullOrEmpty(title) ? title : string.Empty,
                        BeLock = false,
                        IsCanDelete = true,
                        Sequence = Util.Extensions.ToInt(this.db.MngPermissiongroup.Max(c => c.Sequence)) + 1
                    });
                    if (this.db.SaveChanges() > 0)
                    {
                        return "true";
                    }
                }
            }

            return "false";
        }

        /// <summary>
        /// 角色删除
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>是否成功删除</returns>
        [HttpPost]
        public bool PermissionGroupDelete(int id)
        {
            using (var tran = this.db.Database.BeginTransaction())
            {
                try
                {
                    var model = this.db.MngPermissiongroup.FirstOrDefault(c => c.Id == id);

                    if (model != null && model.IsCanDelete)
                    {
                        // 更新排序
                        var list = this.db.MngPermissiongroup.Where(e => e.Sequence > model.Sequence);
                        foreach (var item in list)
                        {
                            item.Sequence--;
                        }

                        // 移除 角色与权限的关系表数据
                        var setList = this.db.MngPermissiongroupset.Where(c => c.GroupId == model.Id).ToList();
                        this.db.MngPermissiongroupset.RemoveRange(setList);

                        // 移除自己
                        this.db.MngPermissiongroup.Remove(model);
                        this.db.SaveChanges();
                        tran.Commit();
                        return true;
                    }
                }
                catch
                {
                    tran.Rollback();
                    return false;
                }
            }

            return false;
        }

        [HttpPost]
        public ActionResult SetFirstPwd(string newPassWord, string newPassWord2)
        {
            // 设置密码
            if (string.IsNullOrEmpty(newPassWord) || string.IsNullOrEmpty(newPassWord2) || newPassWord != newPassWord2)
            {
                return this.AjaxResult(false, "请正确输入您的密码，且两次必须一致");
            }
            else
            {
                var user = this.db.MngAdmin.FirstOrDefault(c => c.UserName == this.userService.ApplicationUser.Mng_admin.UserName);
                if (user != null)
                {
                    user.Password = Util.Helpers.Encrypt.Md5By32(newPassWord);
                    this.db.SaveChanges();
                    return this.AjaxResult(true, "设置成功");
                }
                else
                {
                    return this.AjaxResult(false, "用户不存在");
                }
            }
        }

        /// <summary>
        /// Ajax返回格式
        /// </summary>
        /// <param name="res">返回值</param>
        /// <param name="content">返回数据</param>
        /// <returns>ActionResult</returns>
        private ActionResult AjaxResult(bool res, object content)
        {
            return this.Json(new { state = res, content = content });
        }
    }
}