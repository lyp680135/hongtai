namespace WarrantyManage.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Authorize]
    public class ProductNameController : Controller
    {
        private DataLibrary.DataContext db;

        public ProductNameController(DataLibrary.DataContext db)
        {
            this.db = db;
        }

        [HttpPost]
        public string Create(string productName, string normName, int meteringMode, int deliveryType, string remark)
        {
            var temp = this.db.BaseProductClass.FirstOrDefault(c => c.Name == productName);
            if (temp != null)
            {
                return "您已添加过该品名，请勿重复添加";
            }
            else
            {
                var wsclass = new DataLibrary.BaseProductClass()
                {
                    Name = productName,
                    Gbname = normName,
                    Measurement = meteringMode,
                    DeliveryType = deliveryType,
                    Note = remark
                };

                this.db.BaseProductClass.Add(wsclass);
                this.db.SaveChanges();
                return wsclass.Id.ToString();
            }
        }

        [HttpGet]
        public ObjectResult Edit(int id)
        {
            var rs = this.db.BaseProductClass.FirstOrDefault(p => p.Id == id);
            if (rs != null)
            {
                return new ObjectResult(rs);
            }

            return null;
        }

        [HttpPost]
        public int Edit(int id, string productName, string normName, int measurement, int deliveryType, string remark)
        {
            var rs = this.db.BaseProductClass.FirstOrDefault(p => p.Id == id);
            var temp = this.db.BaseProductClass.FirstOrDefault(c => c.Name == productName && c.Id != id);
            if (temp != null)
            {
                return -1;
            }

            if (rs != null)
            {
                rs.Name = productName;
                rs.Gbname = normName;
                rs.Measurement = measurement;
                rs.DeliveryType = deliveryType;
                rs.Note = remark;
            }

            this.db.BaseProductClass.Update(rs);

            return this.db.SaveChanges();
        }

        [HttpPost]
        public int Delete(string ids)
        {
            if (string.IsNullOrEmpty(ids))
            {
                return 0;
            }

            string[] idarr = ids.Split(',');
            var tempFlag = true;
            if (idarr.Length > 0)
            {
                List<int> idlist = new List<int>();
                for (int i = 0; i < idarr.Length; i++)
                {
                    int id = 0;
                    int.TryParse(idarr[i], out id);
                    var temp = this.db.BaseProductMaterial.FirstOrDefault(c => c.Classid == id);
                    var temp2 = this.db.BaseSpecifications.FirstOrDefault(c => c.Classid == id);
                    var temp3 = this.db.PdProduct.FirstOrDefault(c => c.Classid == id);
                    if (temp != null || temp2 != null || temp3 != null)
                    {
                        tempFlag = false;
                        break;
                    }
                    else
                    {
                        if (id > 0)
                        {
                            idlist.Add(id);
                        }
                    }
                }

                if (tempFlag)
                {
                    var list = this.db.BaseProductClass.Where(p => idlist.Contains(p.Id)).ToList();
                    this.db.BaseProductClass.RemoveRange(list);

                    return this.db.SaveChanges();
                }
            }

            return 0;
        }
    }
}