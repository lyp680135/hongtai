namespace WarrantyManage.Pages.Manage.Site.ContentGroup
{
    using System;
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

        public int IsSuccess { get; set; }

        public List<DataLibrary.SiteModel> ModelList { get; set; }

        public DataLibrary.SiteCategory ContentGroup { get; set; }

        public string ContentListStr { get; set; }

        public bool Disable { get; set; }

        public bool ParDisable { get; set; }

        public int ParId { get; set; }

        public void OnGet(int? id)
        {
            this.ParId = -1;
            this.ContentListStr = Json.ToJson((from c in this.Db.SiteCategory select c).OrderByDescending(c => c.Sequence).ToList());
            this.Disable = false;
            this.ParDisable = false;
            this.ModelList = (from c in this.Db.SiteModel select c).ToList();
            if (id.HasValue)
            {
                this.ContentGroup = this.Db.SiteCategory.FirstOrDefault(c => c.Id == id);
                this.ParId = System.Convert.ToInt32(this.ContentGroup.ParId);
                this.ParDisable = true;
                if (this.ContentGroup.HasModelContent == DataLibrary.EnumList.HasModelContent.是)
                {
                    this.Disable = true;
                }
            }
        }
    }
}