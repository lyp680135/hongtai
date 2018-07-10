namespace WarrantyManage.Pages.Manage.Site.ModelContent
{
    using System.Collections.Generic;
    using System.Linq;
    using Models;
    using Util.Helpers;

    public class AddModel : AuthorizeModel
    {
        public AddModel(DataLibrary.DataContext db)
           : base(db)
        {
        }

        public DataLibrary.SiteCategory Category { get; set; }

        public DataLibrary.SiteModel Model { get; set; }

        public List<DataLibrary.SiteModelColumn> Columns { get; set; }

        public Dictionary<string, object> KeyValuePairs { get; set; }

        public int? ModelContentId { get; set; }

        public void OnGet(int? id, int contentId)
        {
            this.KeyValuePairs = null;
            this.ModelContentId = 0;
            this.Category = this.Db.SiteCategory.FirstOrDefault(c => c.Id == contentId);
            this.Model = this.Db.SiteModel.FirstOrDefault(c => c.Id == this.Category.ModelId);
            List<object> id_list = this.Model.BaseManageId.Split(",").ToList<object>();
            this.Columns = this.Db.SiteModelColumn.Where(c => id_list.Contains(c.Id)).OrderBy(c => c.FildWeight).ToList();
            string database = "sitecontent_" + this.Category.ModelId;
            if (id.HasValue)
            {
                this.ModelContentId = id;
                string sql = "select * from " + database + " where Id=" + id;
                this.KeyValuePairs = MySqlHelper.GetInstanct(Startup.Configuration["Database:ConnectionString"]).ExecuteEntityToDic(sql, System.Data.CommandType.Text, null);
            }
        }
    }
}