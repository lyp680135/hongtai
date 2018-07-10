namespace WarrantyManage.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using MySql.Data.MySqlClient;

    [Authorize]
    public class GBNameController : Controller
    {
        private DataLibrary.DataContext db;

        public GBNameController(DataLibrary.DataContext db)
        {
            this.db = db;
        }

        [HttpPost]
        public string Create(string gBName, string note)
        {
            var temp = this.db.BaseProductGbClass.FirstOrDefault(c => c.Gbname == gBName);
            if (temp != null)
            {
                return "您已添加该国标名，请勿重复添加";
            }
            else
            {
                var gbname = new DataLibrary.BaseProductGbClass()
                {
                    Gbname = gBName,
                    Note = note,
                    Createtime = (int)Util.Extensions.GetCurrentUnixTime()
                };

                this.db.BaseProductGbClass.Add(gbname);
                this.db.SaveChanges();

                return gbname.Id.ToString();
            }
        }

        [HttpGet]
        public ObjectResult Edit(int id)
        {
            var rs = this.db.BaseProductGbClass.FirstOrDefault(p => p.Id == id);

            if (rs != null)
            {
                return new ObjectResult(rs);
            }

            return null;
        }

        [HttpPost]
        public int Edit(int hiddId, string gBName, string note)
        {
            var rs = this.db.BaseProductGbClass.FirstOrDefault(p => p.Id == hiddId);
            var temp = this.db.BaseProductGbClass.FirstOrDefault(c => c.Gbname == gBName && c.Id != hiddId);
            if (temp != null)
            {
                return -1;
            }
            else
            {
                if (rs != null)
                {
                    rs.Gbname = gBName;
                    rs.Note = note;
                }

                this.db.BaseProductGbClass.Update(rs);

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
                    var product = this.db.BaseProductGbClass.FirstOrDefault(c => c.Id == id);
                    if (product != null)
                    {
                        var temp2 = this.db.BaseProductClass.FirstOrDefault(c => c.Gbname == product.Gbname);
                        if (temp2 != null)
                        {
                            tempFlag = false;
                            break;
                        }
                        else
                        {
                            idlist.Add(id);
                        }
                    }
                }

                if (tempFlag)
                {
                    var list = this.db.BaseProductGbClass.Where(p => idlist.Contains(p.Id)).ToList();
                    var sql = this.db.BaseProductClass.AsQueryable();

                    this.db.BaseProductGbClass.RemoveRange(list);

                    return this.db.SaveChanges();
                }
            }

            return 0;
        }
    }
}