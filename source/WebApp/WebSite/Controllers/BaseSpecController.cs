namespace WarrantyManage.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Authorize]
    public class BaseSpecController : Controller
    {
        private DataLibrary.DataContext db;

        public BaseSpecController(DataLibrary.DataContext db)
        {
            this.db = db;
        }

        [HttpPost]
        public ActionResult Add(int materialid, string[] specname, string[] callname, double[] referpieceweight, int[] referpiececount, double[] refermeterweight, double[] referlength, double[] coldratio, double[] reverseratio)
        {
            var classid = this.db.BaseProductMaterial.FirstOrDefault(f => f.Id == materialid).Classid;
            for (var i = 0; i < specname.Length; i++)
            {
                var tempClass = this.db.BaseSpecifications.FirstOrDefault(c => c.Classid == classid
                                                                        && c.Materialid == materialid
                                                                        && c.Callname == callname[i]);
                if (tempClass != null)
                {
                    return this.AjaxResult(false, "该规格别名已经存在，请勿重复添加!");
                }
                else
                {
                    var spec = new DataLibrary.BaseSpecifications()
                    {
                        Classid = classid,
                        Materialid = materialid,
                        Specname = specname[i],
                        Callname = callname[i],
                        Referpieceweight = referpieceweight[i],
                        Referpiececount = referpiececount[i],
                        Referlength = referlength[i],
                        Refermeterweight = refermeterweight[i],
                        Coldratio = coldratio[i],
                        Reverseratio = reverseratio[i]
                    };

                    this.db.BaseSpecifications.Add(spec);
                    this.db.SaveChanges();
                }
            }

            return this.AjaxResult(true, "添加成功!");
        }

        [HttpPost]
        public ActionResult Edit(int[] hiddenId, int classid, int materialid, string[] specname, string[] callname, double[] referpieceweight, int[] referpiececount, double[] referlength, double[] refermeterweight, double[] coldratio, double[] reverseratio)
        {
            for (var i = 0; i < hiddenId.Length; i++)
            {
                if (hiddenId[i] > 0)
                {
                    var tempClass = this.db.BaseSpecifications.FirstOrDefault(c => c.Classid == classid
                                                       && c.Materialid == materialid
                                                       && c.Callname == callname[i] && c.Id != hiddenId[i]);
                    if (tempClass != null)
                    {
                        return this.AjaxResult(false, "该规格别名已经存在，请勿重复添加!");
                    }

                    var rs = this.db.BaseSpecifications.FirstOrDefault(p => p.Id == hiddenId[i]);
                    if (rs != null)
                    {
                        rs.Classid = classid;
                        rs.Materialid = materialid;
                        rs.Callname = callname[i];
                        rs.Specname = specname[i];
                        rs.Referpieceweight = referpieceweight[i];
                        rs.Referpiececount = referpiececount[i];
                        rs.Referlength = referlength[i];
                        rs.Refermeterweight = refermeterweight[i];
                        rs.Coldratio = coldratio[i];
                        rs.Reverseratio = reverseratio[i];
                        this.db.BaseSpecifications.Update(rs);
                        this.db.SaveChanges();
                    }
                }
                else
                {
                    var tempClass = this.db.BaseSpecifications.FirstOrDefault(c => c.Classid == classid
                                                       && c.Materialid == materialid
                                                       && c.Callname == callname[i]);
                    if (tempClass != null)
                    {
                        return this.AjaxResult(false, "该规格别名已经存在，请勿重复添加!");
                    }

                    var spec = new DataLibrary.BaseSpecifications()
                    {
                        Classid = classid,
                        Materialid = materialid,
                        Specname = specname[i],
                        Callname = callname[i],
                        Referpieceweight = referpieceweight[i],
                        Referpiececount = referpiececount[i],
                        Referlength = referlength[i],
                        Refermeterweight = refermeterweight[i],
                        Coldratio = coldratio[i],
                        Reverseratio = reverseratio[i],
                    };
                    this.db.BaseSpecifications.Add(spec);
                    this.db.SaveChanges();
                }
            }

            return this.AjaxResult(true, "修改成功!");
        }

        [HttpPost]
        public int Delete(int id)
        {
            if (id > 0)
            {
                var bsclass = this.db.BaseSpecifications.FirstOrDefault(p => p.Id == id);
                var temp3 = this.db.PdProduct.FirstOrDefault(c => c.Specid == id);
                if (temp3 != null)
                {
                    return -1;
                }
                else
                {
                    if (bsclass != null)
                    {
                        this.db.BaseSpecifications.Remove(bsclass);
                        return this.db.SaveChanges();
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Ajax返回格式
        /// </summary>
        /// <param name="res">返回值</param>
        /// <param name="content">返回数据</param>
        /// <returns>ActionResult</returns>
        private ActionResult AjaxResult(bool res, object content)
        {
            return this.Json(new { state = res, content = content });
        }
    }
}