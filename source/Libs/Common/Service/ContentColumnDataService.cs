namespace Common.Service
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Common.IService;
    using DataLibrary;
    using Util;
    using Util.Helpers;

    public class ContentColumnDataService : IContentColumnDataService
    {
        private DataLibrary.DataContext db;
        private Util.Helpers.MySqlHelper mySqlHelper;

        public ContentColumnDataService(DataLibrary.DataContext db, MySqlHelper mySqlHelper)
        {
            this.db = db;
            this.mySqlHelper = mySqlHelper;
        }

        public Dictionary<string, object> GetContentColumnData(int id, int cid)
        {
            var cg = this.db.SiteCategory.FirstOrDefault(c => c.Id == cid);
            if (cg != null && cg.ModelId.HasValue)
            {
                var manageModel = this.db.SiteModel.FirstOrDefault(c => c.Id == cg.ModelId);
                if (manageModel != null)
                {
                    string database = "sitecontent_" + manageModel.Id;
                    string sql = "select * from " + database + " where id = " + id;
                    return this.mySqlHelper.ExecuteEntityToDic(sql, System.Data.CommandType.Text, null);
                }
            }

            return null;
        }

        /// <summary>
        /// 根据栏目ID获取当前栏目的内容总数
        /// </summary>
        /// <param name="cid">栏目ID</param>
        /// <param name="where">条件</param>
        /// <returns>Count</returns>
        public int GetContentCount(int cid, string where = null)
        {
            var cg = this.db.SiteCategory.FirstOrDefault(c => c.Id == cid);
            if (cg != null && cg.ModelId.HasValue)
            {
                var manageModel = this.db.SiteModel.FirstOrDefault(c => c.Id == cg.ModelId);
                if (manageModel != null)
                {
                    string database = "sitecontent_" + manageModel.Id;
                    string sql = $"select count(0) from {database} where cid = {cid} {(!string.IsNullOrWhiteSpace(where) ? " and " + where : string.Empty)}";
                    return this.mySqlHelper.ExecuteEntity(sql, System.Data.CommandType.Text, null).ToInt();
                }
            }

            return 0;
        }

        /// <summary>
        /// 根据栏目名称获取当前栏目的内容总数
        /// </summary>
        /// <param name="contentName">栏目名称</param>
        /// <param name="where">条件</param>
        /// <returns>Count</returns>
        public int GetContentCount(string contentName, string where = null)
        {
            var cg = this.db.SiteCategory.FirstOrDefault(c => c.ContentTitle == contentName);
            if (cg != null && cg.ModelId.HasValue)
            {
                var manageModel = this.db.SiteModel.FirstOrDefault(c => c.Id == cg.ModelId);
                if (manageModel != null)
                {
                    string database = "sitecontent_" + manageModel.Id;
                    string sql = $"select count(0) from {database} where cid = {cg.Id} {(!string.IsNullOrWhiteSpace(where) ? " and " + where : string.Empty)}";
                    return this.mySqlHelper.ExecuteEntity(sql, System.Data.CommandType.Text, null).ToInt();
                }
            }

            return 0;
        }


        public List<Dictionary<string, object>> GetContentColumnDataList(int cid, int? page, int? pageSize, string where = null)
        {
            pageSize = pageSize.HasValue ? pageSize : 15;
            var pageIndex = (page.HasValue && page < 1) ? 0 : (page - 1) * pageSize;
            var cg = this.db.SiteCategory.FirstOrDefault(c => c.Id == cid);
            if (cg != null && cg.ModelId.HasValue)
            {
                var manageModel = this.db.SiteModel.FirstOrDefault(c => c.Id == cg.ModelId);
                if (manageModel != null)
                {
                    string database = "sitecontent_" + manageModel.Id;
                    string sql = $"select * from {database} where cid = {cid} {(!string.IsNullOrWhiteSpace(where) ? " and " + where : string.Empty)} order by Id desc limit {pageIndex},{pageSize}";
                    return this.mySqlHelper.ExecuteEntityToDicList(sql, System.Data.CommandType.Text, null);
                }
            }

            return null;
        }

        public List<Dictionary<string, object>> GetContentColumnDataList(string contentName, int? page, int? pageSize, string where = null)
        {
            pageSize = pageSize.HasValue ? pageSize : 15;
            var pageIndex = (page.HasValue && page < 1) ? 0 : (page - 1) * pageSize;
            var cg = this.db.SiteCategory.FirstOrDefault(c => c.ContentTitle == contentName);
            if (cg != null && cg.ModelId.HasValue)
            {
                var manageModel = this.db.SiteModel.FirstOrDefault(c => c.Id == cg.ModelId);
                if (manageModel != null)
                {
                    string database = "sitecontent_" + manageModel.Id;
                    string sql = $"select * from {database} where cid = {cg.Id} {(!string.IsNullOrWhiteSpace(where) ? " and " + where : string.Empty)} order by Id desc limit {pageIndex},{pageSize}";
                    return this.mySqlHelper.ExecuteEntityToDicList(sql, System.Data.CommandType.Text, null);
                }
            }

            return null;
        }

        public List<SiteModelColumn> GetSiteModelColumnByCid(int cid, string description)
        {
            int? modelId = this.db.SiteCategory.FirstOrDefault(c => c.Id == cid).ModelId;
            if (modelId.HasValue)
            {
                string[] baseManageIds = this.db.SiteModel.FirstOrDefault(c => c.Id == modelId).BaseManageId.Split(new char[1] { ',' });
                List<DataLibrary.SiteModelColumn> list = this.db.SiteModelColumn.Where(c => baseManageIds.ToList<object>().Contains(c.Id)).ToList();
                if (!string.IsNullOrEmpty(description))
                {
                    return list.Where(c => c.FildDescription == description).ToList();
                }

                return list;
            }

            return null;
        }
    }
}
