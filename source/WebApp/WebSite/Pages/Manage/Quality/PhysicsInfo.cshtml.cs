namespace WarrantyManage.Pages.Manage.Quality
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using DataLibrary;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Models;

    public class PhysicsInfoModel : AuthorizeModel
    {
        private Common.IService.IUserService userService;

        public PhysicsInfoModel(DataLibrary.DataContext db, Common.IService.IUserService userService)
            : base(db)
        {
            this.userService = userService;
        }

        public List<DataLibrary.PdWorkshop> List_workShop { get; set; }

        public List<DataLibrary.PdqualityPdSmeltCode> PdSmeltCodeList { get; set; }

        public List<DataLibrary.BaseQualityStandard> ListQualityStandards { get; set; }

        public DataLibrary.PdQuality PdQuality { get; set; } = new DataLibrary.PdQuality();

        public DataLibrary.PdqualityPdSmeltCode PdSmeltCode { get; set; } = new DataLibrary.PdqualityPdSmeltCode();

        public string BatCode { get; set; }

        public int SmeltCode { get; set; }

        public void OnGet(int? id, string batCode, int smeltCode = 0)
        {
            var userId = this.userService.ApplicationUser.Mng_admin.Id;
            var targetCategory = EnumList.TargetCategory.物理指标;

            this.List_workShop = this.Db.PdWorkshop.AsEnumerable().Where(c => c.QAInputer.Split(',').Contains(userId.ToString())).ToList();
            if (this.List_workShop.Count > 0)
            {
                var workInfo = this.List_workShop.FirstOrDefault();
                if (!id.HasValue || id.Value <= 0)
                {
                    PdBatcode currentInfo = this.Db.PdBatcode.OrderByDescending(c => c.Id).FirstOrDefault(c => c.Batcode.StartsWith(workInfo.Code)) ?? new PdBatcode();
                    if (currentInfo != null)
                    {
                        var productInfo = this.Db.PdProduct.FirstOrDefault(f => f.Batcode == currentInfo.Batcode);
                        if (productInfo != null)
                        {
                            this.ListQualityStandards = this.Db.BaseQualityStandard.Where(w => w.Materialid == productInfo.Materialid && w.Status == 0 && w.TargetCategory == targetCategory).ToList();
                        }
                    }

                    this.BatCode = currentInfo.Batcode;
                    if (!string.IsNullOrEmpty(batCode))
                    {
                        this.BatCode = batCode;
                    }

                }
                else
                {
                    var currentInfo = this.Db.PdQuality.FirstOrDefault(f => f.Id == id);
                    if (this.PdQuality == null)
                    {
                        this.RedirectToError();
                    }

                    this.ListQualityStandards = this.Db.BaseQualityStandard.Where(w => w.Materialid == currentInfo.MaterialId && w.Status == 0 && w.TargetCategory == targetCategory).ToList();
                }

                if (smeltCode <= 0)
                {
                    this.PdSmeltCode = this.Db.PdSmeltCode.LastOrDefault();
                }
                else
                {
                    this.PdSmeltCode = this.Db.PdSmeltCode.FirstOrDefault(f => f.Id == smeltCode);
                    this.SmeltCode = smeltCode;
                }
            }

            this.PdSmeltCodeList = this.Db.PdSmeltCode.OrderByDescending(o => o.Id).Take(100).ToList();

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