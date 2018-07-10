namespace WarrantyManage.Pages.Authority
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Models;

    public class MenuModel : AuthorizeModel
    {
        public MenuModel(DataLibrary.DataContext db)
             : base(db)
        {
        }

        public int ParId { get; set; }

        public string ClassPath { get; set; }

        public List<DataLibrary.MngMenuclass> List_menu { get; set; }

        public void OnGet()
        {
            this.ParId = Util.Extensions.ToInt(this.Request.Query["parId"]);
            this.List_menu = this.Db.MngMenuclass.Where(c => c.ParId == this.ParId).OrderBy(c => c.Sequence).ToList();
            this.ClassPath = this.ShowClassPath(this.ParId, " >> <a href=\"?ParId={0}\" class=\"tablelink\">{1}</a>");
        }

        /// <summary>
        /// 根据底层的ClassID 获取菜单 Class路径
        /// </summary>
        /// <param name="classId">classId</param>
        /// <param name="urlFormat">urlFormat</param>
        /// <returns>菜单 Class路径</returns>
        private string ShowClassPath(int classId, string urlFormat)
        {
            string path = string.Empty;

            DataLibrary.MngMenuclass model = this.Db.MngMenuclass.FirstOrDefault(c => c.Id == classId);
            if (model == null)
            {
                return string.Empty;
            }
            else
            {
                path = string.Format(urlFormat, model.Id.ToString(), model.ClassName);
                if (model.ParId > 0)
                {
                    path = this.ShowClassPath(Convert.ToInt32(model.ParId), urlFormat) + path;
                }
            }

            return path;
        }
    }
}