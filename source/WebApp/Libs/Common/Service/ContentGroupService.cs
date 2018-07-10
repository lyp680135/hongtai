namespace Common.Service
{
    using System.Collections.Generic;
    using System.Linq;
    using Common.IService;
    using DataLibrary;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// 栏目分组数据服务
    /// </summary>
    public class ContentGroupService : IContentGroupService
    {
        private DataLibrary.DataContext db;
        private List<DataLibrary.SiteCategory> contentGroups;

        public ContentGroupService(DataLibrary.DataContext db)
        {
            this.db = db;
            this.contentGroups = (from c in this.db.SiteCategory select c).ToList();
        }

        /// <summary>
        /// 根据ParId获取所有数据集合
        /// </summary>
        /// <param name="parId">父级Id</param>
        /// <returns>内容栏目集合</returns>
        public List<DataLibrary.SiteCategory> GetContentGroupByParId(int parId)
        {
            return this.contentGroups.Where(c => c.ParId == parId).ToList();
        }

        /// <summary>
        /// 根据父级ParId获取他的子级数
        /// </summary>
        /// <param name="parId">父级Id</param>
        /// <returns>子级数</returns>
        public int GetChildNumByParId(int parId)
        {
            return this.db.SiteCategory.Count(c => c.ParId == parId);
        }

        /// <summary>
        /// 根据ParId递归获取他以下的所有数据集合
        /// </summary>
        /// <param name="parId">父级Id</param>
        /// <param name="cId">内容栏目Id</param>
        /// <returns>子级集合</returns>
        public JArray GetAllDataForRecursion(int parId, int cId)
        {
            JArray ja = new JArray();

            var firstData = this.GetContentGroupByParId(parId); // 如果传入0，则是根级

            foreach (var fd in firstData)
            {
                JObject jo = new JObject();
                jo["name"] = fd.ContentTitle;
                jo["id"] = fd.Id;
                jo["spread"] = true;
                if (cId == fd.Id)
                {
                    jo["skin"] = "tree_select";
                }

                if (this.GetChildNumByParId(parId) > 0)
                {
                    var ja2 = this.GetAllDataForRecursion(fd.Id, cId);
                    if (ja2.Count > 0)
                    {
                        jo["children"] = ja2;
                    }
                }

                ja.Add(jo);
            }

            return ja;
        }

        /// <summary>
        /// 根据ParId获取所有子级Id
        /// </summary>
        /// <param name="list">子集Id集合</param>
        /// <param name="parId">父级Id</param>
        /// <returns>Id集合</returns>
        public List<int> GetChildIdByParId(List<int> list, int parId)
        {
            var firstData = this.GetContentGroupByParId(parId); // 如果传入0，则是根级
            foreach (var fd in firstData)
            {
                if (this.GetChildNumByParId(parId) > 0)
                {
                    list.Add(fd.Id);
                    this.GetChildIdByParId(list, fd.Id);
                }
            }

            return list;
        }

        /// <summary>
        /// 根据模型Id获取模型名称
        /// </summary>
        /// <param name="modelId">模型Id</param>
        /// <returns>模型名称</returns>
        public string GetModelNameById(int modelId)
        {
            return this.db.SiteModel.FirstOrDefault(c => c.Id == modelId).Description;
        }

        public SiteCategory GetSiteCategoryById(int id)
        {
            return this.db.SiteCategory.FirstOrDefault(c => c.Id == id);
        }

        public SiteCategory GetSiteCategoryByName(string name)
        {
            return this.db.SiteCategory.FirstOrDefault(c => c.ContentTitle == name);
        }
    }
}
