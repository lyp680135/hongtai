namespace WarrantyManage.Pages.Manage.Settings
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using DataLibrary;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Models;

    public class BaseSpecModel : AuthorizeModel
    {
        public BaseSpecModel(DataLibrary.DataContext db)
        : base(db)
        {
        }

        public List<GroupPM> List_group { get; set; }

        public List<BaseProductClass> List_pm { get; set; }

        public List<BaseProductMaterial> List_cz { get; set; }

        public int MaterialId { get; set; }

        public void OnGet()
        {
            this.MaterialId = Convert.ToInt32(this.Request.Query["materialId"]);
            this.List_group = new List<GroupPM>();
            var query = from c in this.Db.BaseSpecifications
                        group c by new { c.Classid, c.Materialid } into g
                        select g;
            if (this.MaterialId > 0)
            {
                query = query.Where(c => c.Key.Materialid == this.MaterialId);
            }
            else
            {
                query = query.Where(c => c.Key.Materialid == -1);
            }

            List<int> pmId_array = new List<int>();
            List<int> czId_array = new List<int>();

            foreach (var q in query)
            {
                pmId_array.Add(q.Key.Classid.Value);

                czId_array.Add(q.Key.Materialid.Value);

                this.List_group.Add(new GroupPM()
                {
                    Pm = q.Key.Classid.Value,
                    Cz = q.Key.Materialid.Value
                });
            }

            this.List_pm = this.Db.BaseProductClass.Where(c => pmId_array.ToArray().Contains(c.Id)).ToList();
            this.List_cz = this.Db.BaseProductMaterial.Where(c => czId_array.ToArray().Contains(c.Id)).ToList();
            this.ViewData["MaterialId"] = this.MaterialId;
        }

        public List<BaseSpecifications> GetSpecByPmAndCz(int pm, int cz)
        {
            return this.Db.BaseSpecifications.Where(c => c.Classid == pm && c.Materialid == cz).ToList();
        }

        /// <summary>
        /// 分组之后的所有品名和材质
        /// </summary>
        public class GroupPM
        {
            public int Pm { get; set; }

            public int Cz { get; set; }
        }
    }
}