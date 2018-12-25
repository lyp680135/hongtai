namespace WarrantyManage.Pages.Manage.Quality
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using DataLibrary;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Models;

    public class InfoModel : AuthorizeModel
    {
        private Common.IService.IUserService userService;

        public InfoModel(DataLibrary.DataContext db, Common.IService.IUserService userService)
             : base(db)
        {
            this.userService = userService;
        }
        public int Wid { get; set; }
        public string Batcode { get; set; }

        public string HtmlStr { get; set; }

        public List<DataLibrary.PdWorkshop> List_workShop { get; set; }

        public List<DataLibrary.BaseQualityStandard> ListQualityStandards { get; set; }

        public DataLibrary.PdQuality PdQuality { get; set; } = new DataLibrary.PdQuality();

        public void OnGet(int? id, string batcode, string htmlStr, int wid)
        {
            var userId = this.userService.ApplicationUser.Mng_admin.Id;
            this.List_workShop = this.Db.PdWorkshop.AsEnumerable().Where(c => c.QAInputer.Split(',').Contains(userId.ToString())).ToList();
            if (!string.IsNullOrEmpty(htmlStr))
            {
                this.HtmlStr = htmlStr;
            }

            if (!string.IsNullOrEmpty(batcode))
            {
                this.Batcode = batcode;
                var productInfo = this.Db.PdProduct.FirstOrDefault(f => f.Batcode == batcode);
                if (productInfo != null)
                {
                    this.ListQualityStandards = this.Db.BaseQualityStandard.Where(w => w.Materialid == productInfo.Materialid && w.Status == 0).ToList();
                }
            }
            else
            {
                if (this.List_workShop.Count > 0)
                {
                    var workInfo = new PdWorkshop();

                    if (!id.HasValue || id.Value <= 0)
                    {
                        if (wid > 0)
                        {
                            workInfo = this.List_workShop.FirstOrDefault(f => f.Id == wid);
                            this.Wid = wid;
                        }
                        else
                        {
                            workInfo = this.List_workShop.FirstOrDefault();
                        }

                        PdBatcode currentInfo = this.Db.PdBatcode.OrderByDescending(c => c.Id).FirstOrDefault(c => c.Workshopid == workInfo.Id) ?? new PdBatcode();
                        if (currentInfo != null)
                        {
                            this.Batcode = currentInfo.Batcode;
                            var productInfo = this.Db.PdProduct.FirstOrDefault(f => f.Batcode == currentInfo.Batcode);
                            if (productInfo != null)
                            {
                                this.ListQualityStandards = this.Db.BaseQualityStandard.Where(w => w.Materialid == productInfo.Materialid && w.Status == 0).ToList();
                            }
                        }
                    }
                    else
                    {
                        var currentInfo = this.Db.PdQuality.FirstOrDefault(f => f.Id == id);
                        if (this.PdQuality == null)
                        {
                            this.RedirectToError();
                        }

                        this.ListQualityStandards = this.Db.BaseQualityStandard.Where(w => w.Materialid == currentInfo.MaterialId && w.Status == 0).ToList();
                    }
                }
            }

            if (id.HasValue && id.Value > 0)
            {
                this.PdQuality = this.Db.PdQuality.FirstOrDefault(c => c.Id == id);
                if (this.PdQuality == null)
                {
                    this.RedirectToError();
                }
                else if (this.PdQuality.CheckStatus != DataLibrary.EnumList.CheckStatus_PdQuality.等待审核)
                {
                    this.RedirectToError();
                }
            }
        }
    }
}