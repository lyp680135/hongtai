namespace WarrantyManage.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Authorize]
    public class GBProductionController : Controller
    {
        private DataLibrary.DataContext db;

        public GBProductionController(DataLibrary.DataContext db)
        {
            this.db = db;
        }

        [HttpPost]
        public string Create(string name, string note)
        {
            var gbname = new DataLibrary.BaseGbProduction()
            {
                Name = name,
                Note = note,
                Createtime = (int)Util.Extensions.GetCurrentUnixTime()
            };

            this.db.BaseGbProduction.Add(gbname);
            this.db.SaveChanges();

            return gbname.Id.ToString();
        }

        [HttpGet]
        public ObjectResult Edit(int id)
        {
            var rs = this.db.BaseGbProduction.FirstOrDefault(p => p.Id == id);
            if (rs != null)
            {
                return new ObjectResult(rs);
            }

            return null;
        }

        [HttpPost]
        public int Edit(int hiddId, string name, string note)
        {
            var rs = this.db.BaseGbProduction.FirstOrDefault(p => p.Id == hiddId);

            if (rs != null)
            {
                rs.Name = name;
                rs.Note = note;
            }

            this.db.BaseGbProduction.Update(rs);

            return this.db.SaveChanges();
        }

        [HttpPost]
        public int Delete(string ids)
        {
            if (string.IsNullOrEmpty(ids))
            {
                return 0;
            }

            ids = ids.TrimEnd(',');
            string[] idarr = ids.Split(',');
            var tempFlag = true;
            if (idarr.Length > 0)
            {
                List<int> idlist = new List<int>();
                for (int i = 0; i < idarr.Length; i++)
                {
                    int id = 0;
                    int.TryParse(idarr[i], out id);
                    var temp2 = this.db.BaseQualityStandard.FirstOrDefault(c => c.Standardid == id);
                    if (temp2 != null)
                    {
                        tempFlag = false;
                        break;
                    }
                    else
                    {
                        if (id > 0)
                        {
                            idlist.Add(id);
                            var qs = this.db.BaseQualityStandard.FirstOrDefault(p => p.Standardid == id);
                            if (qs != null)
                            {
                                qs.Standardid = null;
                                this.db.BaseQualityStandard.Update(qs);
                                this.db.SaveChanges();
                            }
                        }
                    }
                }

                if (tempFlag)
                {
                    var list = this.db.BaseGbProduction.Where(p => idlist.Contains(p.Id)).ToList();
                    var sql = this.db.BaseGbProduction.AsQueryable();
                    this.db.BaseGbProduction.RemoveRange(list);
                    return this.db.SaveChanges();
                }
            }

            return 0;
        }
    }
}