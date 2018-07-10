namespace WarrantyManage.Pages.Manage.Site.ModelContent
{
    using System.Collections.Generic;
    using System.Linq;
    using Models;
    using Util.Helpers;

    public class ListModel : AuthorizeModel
    {
        public ListModel(DataLibrary.DataContext db)
          : base(db)
        {
        }

        public List<Dictionary<string, object>> DirList { get; set; }

        public List<DataLibrary.SiteModelColumn> BaseManageModels { get; set; }

        public DataLibrary.SiteModel ManageModel { get; set; }

        public DataLibrary.SiteCategory ContentGroup { get; set; }

        public int PageIndex { get; set; }

        public int PageCount { get; set; }

        public new int Page { get; set; }

        public new int PageSize { get; set; }

        public void OnGet(int? pg, int? cid)
        {
            this.PageSize = 15;
            if (!cid.HasValue)
            {
                this.ContentGroup = this.Db.SiteCategory.FirstOrDefault(c => c.ParId == 0);
                if (this.ContentGroup == null)
                {
                    return;
                }
                else
                {
                    cid = this.ContentGroup.Id;
                }
            }

            this.Page = pg ?? 1;
            this.PageIndex = (pg.HasValue && pg < 1) ? 0 : (this.Page - 1) * this.PageSize;
            this.ContentGroup = this.ContentGroup == null ? this.Db.SiteCategory.FirstOrDefault(c => c.Id == cid) : this.ContentGroup;
            this.ManageModel = this.Db.SiteModel.FirstOrDefault(c => c.Id == this.ContentGroup.ModelId);
            List<object> id_list = this.ManageModel.BaseManageId.Split(",").ToList<object>();
            this.BaseManageModels = this.Db.SiteModelColumn.Where(c => id_list.Contains(c.Id)).ToList();
            string database = "sitecontent_" + this.ContentGroup.ModelId;
            string sql = "select * from " + database + " where cid = " + this.ContentGroup.Id + " order by Id desc limit " + this.PageIndex + "," + this.PageSize;
            this.DirList = MySqlHelper.GetInstanct(Startup.Configuration["Database:ConnectionString"]).ExecuteEntityToDicList(sql, System.Data.CommandType.Text, null);
        }
    }
}