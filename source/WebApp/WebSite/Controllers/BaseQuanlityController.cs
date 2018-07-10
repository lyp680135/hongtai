namespace WarrantyManage.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using DataLibrary;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    [Authorize]
    public class BaseQuanlityController : Controller
    {
        private DataLibrary.DataContext db;

        public BaseQuanlityController(DataLibrary.DataContext db)
        {
            this.db = db;
        }

        public IActionResult Index()
        {
            return this.View();
        }

        [HttpPost]
        public void SetQuanlity(int id, string field, string value)
        {
            if (id > 0)
            {
                string sql = "Update BaseQualityStandard set " + field + " = '" + value + "' where Id=" + id;
                this.db.Database.ExecuteSqlCommand(sql);
            }
            else
            {
                string sql = "insert into BaseQualityStandard(" + field + ")  values('" + value + "')";
                this.db.Database.ExecuteSqlCommand(sql);
            }
        }

        [HttpPost]
        public ActionResult AddBaseQuanlity(BaseQualityStandard baseQualityStandard)
        {
            var baseQualityStandard1 = this.db.BaseQualityStandard.FirstOrDefault(f => f.TargetName == baseQualityStandard.TargetName && f.Status != 1 && f.Materialid == baseQualityStandard.Materialid);
            if (baseQualityStandard1 != null)
            {
                return this.AjaxResult(false, "存在该指标数据");
            }

            baseQualityStandard.TargetName = baseQualityStandard.TargetName.Trim();
            baseQualityStandard.Status = 0;
            baseQualityStandard.Classid = this.db.BaseProductMaterial.FirstOrDefault(w => w.Id == baseQualityStandard.Materialid).Classid;
            this.db.BaseQualityStandard.Add(baseQualityStandard);
            this.db.SaveChanges();
            return this.AjaxResult(true, "添加成功");
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var rs = this.db.BaseQualityStandard.FirstOrDefault(p => p.Id == id);
            if (rs != null)
            {
                return new ObjectResult(rs);
            }

            return null;
        }

        public ActionResult Edit(BaseQualityStandard baseQualityStandard)
        {
            var baseQualityStandard1 = this.db.BaseQualityStandard.AsNoTracking().FirstOrDefault(p => p.Id == baseQualityStandard.Id);
            if (baseQualityStandard1 == null)
            {
                return this.AjaxResult(false, "不存在该记录");
            }

            var baseQualityStandardInfo = this.db.BaseQualityStandard.FirstOrDefault(f =>
            f.TargetName == baseQualityStandard.TargetName
            && f.Status != 1
            && f.Materialid == baseQualityStandard.Materialid
            && f.Id != baseQualityStandard.Id);

            if (baseQualityStandardInfo != null)
            {
                return this.AjaxResult(false, "指定了重复的指标名,无法修改");
            }
            baseQualityStandard.TargetName = baseQualityStandard.TargetName.Trim();
            baseQualityStandard.Classid = this.db.BaseProductMaterial.FirstOrDefault(w => w.Id == baseQualityStandard.Materialid).Classid;
            this.db.BaseQualityStandard.Update(baseQualityStandard);
            this.db.SaveChanges();
            return this.AjaxResult(true, "修改成功");
        }

        public ActionResult DelQuanlity(int id)
        {
            var quanlityInfo = this.db.BaseQualityStandard.AsNoTracking().FirstOrDefault(f => f.Id == id);
            if (quanlityInfo == null)
            {
                return this.AjaxResult(false, "不存在该元素");
            }

            var pdProduct = this.db.PdProduct.FirstOrDefault(f => f.Materialid == quanlityInfo.Materialid);
            if (pdProduct != null)
            {
                return this.AjaxResult(false, "该元素已经在产品,无法删除");
            }

            this.db.BaseQualityStandard.Remove(quanlityInfo);
            this.db.SaveChanges();
            return this.AjaxResult(true, "删除成功");
        }

        /// <summary>
        /// 可以放在BaseController
        /// </summary>
        /// <param name="res">res</param>
        /// <param name="content">content</param>
        /// <returns>View</returns>
        protected ActionResult AjaxResult(bool res, string content)
        {
            return this.Json(new { state = res, content = content });
        }
    }
}