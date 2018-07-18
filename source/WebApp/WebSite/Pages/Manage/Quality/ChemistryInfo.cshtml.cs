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

    public class ChemistryInfoModel : AuthorizeModel
    {
        private Common.IService.IUserService userService;

        public ChemistryInfoModel(DataLibrary.DataContext db, Common.IService.IUserService userService)
            : base(db)
        {
            this.userService = userService;
        }

        public List<DataLibrary.PdWorkshop> List_workShop { get; set; }

        public List<DataLibrary.BaseQualityStandard> ListQualityStandards { get; set; }

        public DataLibrary.PdQuality PdQuality { get; set; } = new DataLibrary.PdQuality();

        public int Mid { get; set; }

        public void OnGet(int? id)
        {
            var userId = this.userService.ApplicationUser.Mng_admin.Id;
            var targetCategory = EnumList.TargetCategory.化学指标;
            this.List_workShop = this.Db.PdWorkshop.AsEnumerable().Where(c => c.QAInputer.Split(',').Contains(userId.ToString())).ToList();
            if (this.List_workShop.Count > 0)
            {
                var workInfo = this.List_workShop.FirstOrDefault();
                if (!id.HasValue || id.Value <= 0)
                {
                    BaseProductMaterial currentInfo = this.Db.BaseProductMaterial.FirstOrDefault();
                    if (currentInfo != null)
                    {
                        this.ListQualityStandards = this.Db.BaseQualityStandard.Where(w => w.Materialid == currentInfo.Id && w.Status == 0 && w.TargetCategory == targetCategory).ToList();
                        this.Mid = currentInfo.Id;
                    }
                }
                else
                {
                    var currentInfo = this.Db.PdQuality.FirstOrDefault(f => f.Id == id);
                    if (this.PdQuality == null)
                    {
                        this.RedirectToError();
                    }

                    this.Mid = Convert.ToInt32(currentInfo.MaterialId);
                    this.ListQualityStandards = this.Db.BaseQualityStandard.Where(w => w.Materialid == currentInfo.MaterialId && w.Status == 0 && w.TargetCategory == targetCategory).ToList();
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