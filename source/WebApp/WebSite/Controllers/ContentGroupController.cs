namespace WarrantyManage.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Common.IService;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Util.Helpers;

    [Authorize]
    public class ContentGroupController : Controller
    {
        private DataLibrary.DataContext db;
        private IContentGroupService contentGroupService;

        public ContentGroupController(DataLibrary.DataContext db, IContentGroupService contentGroupService)
        {
            this.db = db;
            this.contentGroupService = contentGroupService;
        }

        [HttpPost]
        public int Add(string contentTitle, int? modelId, int? parId)
        {
            if (string.IsNullOrEmpty(contentTitle) || !modelId.HasValue || !parId.HasValue)
            {
                return 0;
            }
            else
            {
                if (this.db.SiteCategory.FirstOrDefault(c => c.ContentTitle == contentTitle) != null)
                {
                    return -1;
                }

                DataLibrary.SiteCategory contentGroup = this.db.SiteCategory.FirstOrDefault(c => c.Id == parId);
                int? depTh = contentGroup == null ? 0 : contentGroup.Depth + 1;
                var list = this.db.SiteCategory.Where(c => c.Depth == depTh).ToList();
                var model = new DataLibrary.SiteCategory()
                {
                    ContentTitle = contentTitle,
                    ModelId = modelId,
                    HasModelContent = DataLibrary.EnumList.HasModelContent.否,
                    ParId = parId,
                    Depth = depTh,
                    Sequence = list.Count == 0 ? 1 : Util.Extensions.ToInt(this.db.SiteCategory.Where(c => c.Depth == depTh).Max(c => c.Sequence)) + 1,
                };
                this.db.SiteCategory.Add(model);
                return this.db.SaveChanges();
            }
        }

        [HttpPost]
        public int Edit(int id, int modelId, string contentTitle, int parId = 0)
        {
            if (string.IsNullOrEmpty(contentTitle))
            {
                return 0;
            }
            else
            {
                if (this.db.SiteCategory.FirstOrDefault(c => c.ContentTitle == contentTitle && c.Id != id) != null)
                {
                    return -1;
                }

                DataLibrary.SiteCategory contentGroup = this.db.SiteCategory.FirstOrDefault(c => c.Id == id);
                if (contentGroup.ParId != parId)
                {
                    DataLibrary.SiteCategory parcontentGroup = this.db.SiteCategory.FirstOrDefault(c => c.Id == parId);
                    int? depTh = parcontentGroup == null ? 0 : parcontentGroup.Depth + 1;
                    var list = this.db.SiteCategory.Where(c => c.Depth == depTh).ToList();
                    contentGroup.ParId = parId;
                    contentGroup.Depth = depTh;
                    contentGroup.Sequence = list.Count == 0 ? 1 : Util.Extensions.ToInt(this.db.SiteCategory.Where(c => c.Depth == depTh).Max(c => c.Sequence)) + 1;
                }

                contentGroup.ModelId = modelId;
                contentGroup.ContentTitle = contentTitle;
                this.db.SiteCategory.Update(contentGroup);
                return this.db.SaveChanges();
            }
        }

        [HttpPost]
        public int Delete(string ids)
        {
            if (string.IsNullOrEmpty(ids))
            {
                return 0;
            }

            List<object> idList = ids.Split(",").ToList<object>();
            string[] id_arr = ids.Split(",");

            List<DataLibrary.SiteCategory> contentGroups = (from c in this.db.SiteCategory select c).ToList();
            foreach (var id in id_arr)
            {
                if (!string.IsNullOrEmpty(id))
                {
                    var list = this.contentGroupService.GetChildIdByParId(new List<int>(), System.Convert.ToInt32(id));
                    foreach (var item in list)
                    {
                        if (!idList.Contains(item))
                        {
                            idList.Add(item);
                        }
                    }
                }
            }

            List<DataLibrary.SiteCategory> contentGroupList = this.db.SiteCategory.Where(c => idList.Contains(c.Id)).ToList();

            foreach (var item in contentGroupList)
            {
                if (item.HasModelContent == DataLibrary.EnumList.HasModelContent.是)
                {
                    return -1;
                }
            }

            this.db.SiteCategory.RemoveRange(contentGroupList);
            return this.db.SaveChanges();
        }

        public List<DataLibrary.SiteCategory> GetContentGroupByModelId(int modelId)
        {
            return this.db.SiteCategory.Where(c => c.ModelId == modelId).ToList();
        }

        [HttpPost]
        public int AddModelContent(int id)
        {
            try
            {
                DataLibrary.SiteCategory contentGroup = this.db.SiteCategory.FirstOrDefault(c => c.Id == id);
                DataLibrary.SiteModel manageModel = this.db.SiteModel.FirstOrDefault(c => c.Id == contentGroup.ModelId);
                List<object> id_list = manageModel.BaseManageId.Split(",").ToList<object>();
                List<DataLibrary.SiteModelColumn> baseManageModels = this.db.SiteModelColumn.Where(c => id_list.Contains(c.Id)).ToList();
                string database = "sitecontent_" + contentGroup.ModelId;
                string sql = "insert into " + database + "(";
                string str = string.Empty;
                foreach (var item in baseManageModels)
                {
                    sql += item.FildName + ",";
                    if (item.PageShowType == DataLibrary.EnumList.PageShowType.时间选择框)
                    {
                        // 转成时间戳
                        str += "'" + Util.Extensions.GetUnixTimeFromDateTime(Util.Extensions.ToDate(this.Request.Form[item.FildName])) + "',";
                    }
                    else
                    {
                        str += "'" + this.Request.Form[item.FildName].ToString() + "',";
                    }
                }

                sql += "cid) VALUES (";
                sql += str + id;
                sql += ")";
                if (this.db.Database.ExecuteSqlCommand(sql) == 1)
                {
                    contentGroup.HasModelContent = DataLibrary.EnumList.HasModelContent.是;
                    this.db.SiteCategory.Update(contentGroup);
                    this.db.SaveChanges();
                    return 1;
                }

                return 0;
            }
            catch
            {
                return 0;
            }
        }

        [HttpPost]
        public int EditModelContent(int id, int modelContentId)
        {
            try
            {
                DataLibrary.SiteCategory contentGroup = this.db.SiteCategory.FirstOrDefault(c => c.Id == id);
                DataLibrary.SiteModel manageModel = this.db.SiteModel.FirstOrDefault(c => c.Id == contentGroup.ModelId);
                List<object> id_list = manageModel.BaseManageId.Split(",").ToList<object>();
                List<DataLibrary.SiteModelColumn> baseManageModels = this.db.SiteModelColumn.Where(c => id_list.Contains(c.Id)).ToList();
                string database = "sitecontent_" + contentGroup.ModelId;
                string sql = "update " + database + " SET ";
                foreach (var item in baseManageModels)
                {
                    if (item.PageShowType == DataLibrary.EnumList.PageShowType.时间选择框)
                    {
                        // 转成时间戳
                        sql += item.FildName + " = '" + Util.Extensions.GetUnixTimeFromDateTime(Util.Extensions.ToDate(this.Request.Form[item.FildName])) + "',";
                    }
                    else
                    {
                        sql += item.FildName + " = '" + this.Request.Form[item.FildName].ToString() + "',";
                    }
                }

                sql = sql.Substring(0, sql.Length - 1);
                sql += " WHERE id=" + modelContentId;
                return this.db.Database.ExecuteSqlCommand(sql);
            }
            catch
            {
                return 0;
            }
        }

        [HttpPost]
        public int DeleteModelContent(string ids, int contentId)
        {
            try
            {
                DataLibrary.SiteCategory contentGroup = this.db.SiteCategory.FirstOrDefault(c => c.Id == contentId);
                string database = "sitecontent_" + contentGroup.ModelId;

                string sql2 = "delete from " + database + " where Id in (" + ids.Substring(0, ids.Length - 1) + ")";

                this.db.Database.ExecuteSqlCommand(sql2);
                this.db.SaveChanges();
                string sql1 = "select * from " + database + " where cid = " + contentId;
                List<Dictionary<string, object>> list = MySqlHelper.GetInstanct(Startup.ConnStr).ExecuteEntityToDicList(sql1, System.Data.CommandType.Text, null);
                if (list.Count == 0)
                {
                    contentGroup.HasModelContent = DataLibrary.EnumList.HasModelContent.否;
                    this.db.SiteCategory.Update(contentGroup);
                }

                this.db.SaveChanges();
                return 1;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 内容分组栏目上移、下移
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="type">up:上移 down:下移</param>
        /// <returns>数据改动条数</returns>
        public int MoveContentGroup(int id, string type)
        {
            var result = this.db.SiteCategory.FirstOrDefault(c => c.Id == id);
            if (result != null && result.Sequence > 0)
            {
                var temp = 0;
                DataLibrary.SiteCategory contentGroup = new DataLibrary.SiteCategory();
                if (type == "up")
                {
                    contentGroup = this.db.SiteCategory.Where(c => c.ParId == result.ParId && c.Depth == result.Depth && c.Sequence < result.Sequence).OrderByDescending(c => c.Sequence).FirstOrDefault();
                }
                else
                {
                    contentGroup = this.db.SiteCategory.Where(c => c.ParId == result.ParId && c.Depth == result.Depth && c.Sequence > result.Sequence).OrderBy(c => c.Sequence).FirstOrDefault();
                }

                if (contentGroup != null)
                {
                    temp = contentGroup.Sequence;
                    contentGroup.Sequence = result.Sequence;
                    result.Sequence = temp;
                    return this.db.SaveChanges();
                }

                return 0;
            }

            return 0;
        }
    }
}