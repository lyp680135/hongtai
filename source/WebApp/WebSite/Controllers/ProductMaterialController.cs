namespace WarrantyManage.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Authorize]
    public class ProductMaterialController : Controller
    {
        private DataLibrary.DataContext db;

        public ProductMaterialController(DataLibrary.DataContext db)
        {
            this.db = db;
        }

        [HttpPost]
        public string Create(string name, int classid, string note, int standardstrength)
        {
            using (var tran = this.db.Database.BeginTransaction())
            {
                try
                {
                    var checkClass = this.db.BaseProductMaterial.FirstOrDefault(c => c.Name == name && c.Classid == classid);
                    if (checkClass != null)
                    {
                        return "-1";
                    }
                    else
                    {
                        var wsclass = new DataLibrary.BaseProductMaterial()
                        {
                            Name = name,
                            Classid = classid,
                            Note = note,
                            Standardstrength = standardstrength
                        };

                        this.db.BaseProductMaterial.Add(wsclass);
                        this.db.SaveChanges();
                        tran.Commit();
                        return wsclass.Id.ToString();
                    }
                }
                catch (Exception)
                {
                    tran.Rollback();
                    return "0";
                }
            }
        }

        [HttpGet]
        public ObjectResult Edit(int id)
        {
            var rs = this.db.BaseProductMaterial.FirstOrDefault(p => p.Id == id);
            if (rs != null)
            {
                return new ObjectResult(rs);
            }

            return null;
        }

        [HttpPost]
        public int Edit(int hiddId, string name, int classid, string note, int standardstrength)
        {
            using (var tran = this.db.Database.BeginTransaction())
            {
                try
                {
                    var rs = this.db.BaseProductMaterial.FirstOrDefault(p => p.Id == hiddId);

                    if (rs == null)
                    {
                        return -1;
                    }

                    if (this.db.BaseProductMaterial.FirstOrDefault(p => p.Id != hiddId && p.Name == name) != null)
                    {
                        return -1;
                    }
                    rs.Name = name;
                    rs.Classid = classid;
                    rs.Note = note;
                    rs.Standardstrength = standardstrength;
                    this.db.BaseProductMaterial.Update(rs);
                    this.db.SaveChanges();
                    tran.Commit();
                    return 1;
                }
                catch (Exception)
                {
                    tran.Rollback();
                    return 0;
                }
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
                    int.TryParse(idarr[i], out int id);
                    var temp = this.db.BaseSpecifications.FirstOrDefault(c => c.Materialid == id);
                    var temp2 = this.db.PdProduct.FirstOrDefault(c => c.Materialid == id);
                    if (temp != null || temp2 != null)
                    {
                        tempFlag = false;
                        break;
                    }
                    else
                    {
                        if (id > 0)
                        {
                            var bq = this.db.BaseQualityStandard.FirstOrDefault(p => p.Materialid == id);
                            if (bq != null)
                            {
                                this.db.BaseQualityStandard.Remove(bq);
                            }

                            idlist.Add(id);
                        }
                    }
                }

                if (tempFlag)
                {
                    this.db.SaveChanges();

                    var list = this.db.BaseProductMaterial.Where(p => idlist.Contains(p.Id)).ToList();
                    var sql = this.db.BaseProductMaterial.AsQueryable();

                    this.db.BaseProductMaterial.RemoveRange(list);

                    return this.db.SaveChanges();
                }
            }

            return 0;
        }
    }
}