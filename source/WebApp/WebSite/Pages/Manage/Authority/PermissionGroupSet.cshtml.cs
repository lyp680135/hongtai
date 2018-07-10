namespace WarrantyManage.Pages.Manage.Authority
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DataLibrary;
    using Models;
    using Newtonsoft.Json.Linq;

    public class PermissionGroupSetModel : AuthorizeModel
    {
        public PermissionGroupSetModel(DataLibrary.DataContext db)
             : base(db)
        {
        }

        public int GroupId { get; set; }

        public string Msg { get; set; }

        public MngPermissiongroup ModelPermissionGroup { get; set; }

        public string TreeViewData { get; set; }

        public void OnGet(int groupId)
        {
            groupId = Util.Extensions.ToInt(groupId);

            if (groupId > 0)
            {
                this.ModelPermissionGroup = this.Db.MngPermissiongroup.FirstOrDefault(c => c.Id == groupId);
                if (this.ModelPermissionGroup != null)
                {
                    // 查询出己存在的 功能权限
                    var alreadyExists = from t in this.Db.MngPermissiongroupset
                                        where t.GroupId == groupId
                                        select new
                                        {
                                            PermissionId = t.PermissionId,
                                        };
                    JArray ja = new JArray();

                    // 生成树形的json (数据 权限)
                    var query = from c in this.Db.MngMenuclass orderby c.Sequence select c;
                    {
                        foreach (var re in query)
                        {
                            JObject jo = new JObject();

                            jo["name"] = re.ClassName;
                            jo["parid"] = re.ParId;
                            jo["parpath"] = re.ParPath;
                            jo["id"] = re.Id;
                            jo["childnum"] = re.ChildNum;
                            jo["type"] = re.ChildNum > 0 ? "folder" : (Convert.ToBoolean(re.IsPermission) ? "file1" : "file");
                            jo["showchk"] = true;
                            jo["chkvalue"] = false;
                            if (alreadyExists.Where(c => c.PermissionId == re.Id).Count() > 0)
                            {
                                jo["chkvalue"] = true;
                            }

                            ja.Add(jo);
                        }
                    }

                    this.TreeViewData = ja.ToString();
                }
                else
                {
                    this.RedirectToError();
                }
            }
            else
            {
                this.RedirectToError();
            }
        }

        public void OnPost(int groupId, string[] permissions)
        {
            this.Db.MngPermissiongroupset.RemoveRange(from c in this.Db.MngPermissiongroupset where c.GroupId == groupId select c);
            this.Db.SaveChanges();

            if (permissions != null)
            {
                List<MngPermissiongroupset> list = new List<MngPermissiongroupset>();

                foreach (var s in permissions)
                {
                    list.Add(new MngPermissiongroupset() { GroupId = groupId, PermissionId = Convert.ToInt32(s) });
                }

                this.Db.MngPermissiongroupset.AddRange(list);
                this.Db.SaveChanges();
            }

            this.Msg = "layer.msg('操作成功', { icon: 1 });setTimeout(\"window.location='/Manage/Authority/Permissiongroup';\", 2000);";
            this.ModelPermissionGroup = this.Db.MngPermissiongroup.FirstOrDefault(c => c.Id == groupId);
            this.TreeViewData = "[]";
        }
    }
}