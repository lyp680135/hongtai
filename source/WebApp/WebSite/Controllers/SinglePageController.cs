namespace WarrantyManage.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Authorize]
    public class SinglePageController : Controller
    {
        private DataLibrary.DataContext db;

        public SinglePageController(DataLibrary.DataContext db)
        {
            this.db = db;
        }

        public IActionResult Index()
        {
            return this.View();
        }

        [HttpPost]
        public int Delete(string ids)
        {
            if (string.IsNullOrEmpty(ids))
            {
                return 0;
            }

            string[] idarr = ids.Split(',');

            if (idarr.Length > 0)
            {
                List<int> idlist = new List<int>();
                for (int i = 0; i < idarr.Length; i++)
                {
                    int id = 0;
                    int.TryParse(idarr[i], out id);

                    if (id > 0)
                    {
                        idlist.Add(id);
                    }
                }

                var list = this.db.SiteSinglePage.Where(p => idlist.Contains(p.Id)).ToList();
                var sql = this.db.SiteSinglePage.AsQueryable();

                this.db.SiteSinglePage.RemoveRange(list);

                return this.db.SaveChanges();
            }

            return 0;
        }
    }
}