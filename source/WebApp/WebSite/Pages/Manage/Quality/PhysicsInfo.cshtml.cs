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

        public List<DataLibrary.PdqualitySmeltCode> PdSmeltCodeList { get; set; }

        public List<DataLibrary.BaseQualityStandard> ListQualityStandards { get; set; }

        public DataLibrary.PdQuality PdQuality { get; set; } = new DataLibrary.PdQuality();

        public DataLibrary.PdqualitySmeltCode PdSmeltCode { get; set; } = new DataLibrary.PdqualitySmeltCode();

        public string BatCode { get; set; }

        public string SmeltCode { get; set; }

        public int Wid { get; set; }

        public string Msg { get; set; }

        public void OnGet(int? id, string batCode, string smeltCode, int wid = 0)
        {
            var userId = this.userService.ApplicationUser.Mng_admin.Id;
            var targetCategory = EnumList.TargetCategory.物理指标;

            this.List_workShop = this.Db.PdWorkshop.AsEnumerable().Where(c => c.QAInputer.Split(',').Contains(userId.ToString())).ToList();
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

                    var productInfo = new PdProduct();

                    if (!string.IsNullOrEmpty(batCode))
                    {
                        this.BatCode = batCode;
                        productInfo = this.Db.PdProduct.FirstOrDefault(f => f.Batcode == batCode);
                        if (productInfo != null)
                        {
                            this.ListQualityStandards = this.Db.BaseQualityStandard.Where(w => w.Materialid == productInfo.Materialid && w.Status == 0 && w.TargetCategory == targetCategory).ToList();
                        }

                        PdBatcode currBatCode = this.Db.PdBatcode.FirstOrDefault(f => f.Batcode == batCode);
                        if (currBatCode == null)
                        {
                            this.ListQualityStandards = new List<BaseQualityStandard>();
                            this.Msg = "不存在该炉批号";
                        }
                    }
                    else
                    {
                        PdBatcode currentInfo = this.Db.PdBatcode.OrderByDescending(c => c.Serialno).FirstOrDefault(c => c.Workshopid == workInfo.Id) ?? new PdBatcode();
                        if (currentInfo != null)
                        {
                            this.BatCode = currentInfo.Batcode;
                            productInfo = this.Db.PdProduct.FirstOrDefault(f => f.Batcode == currentInfo.Batcode);
                            if (productInfo != null)
                            {
                                this.ListQualityStandards = this.Db.BaseQualityStandard.Where(w => w.Materialid == productInfo.Materialid && w.Status == 0 && w.TargetCategory == targetCategory).ToList();
                            }                            
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

                    this.ListQualityStandards = this.Db.BaseQualityStandard.Where(w => w.Materialid == currentInfo.MaterialId && w.Status == 0 && w.TargetCategory == targetCategory).ToList();
                }

                if (!string.IsNullOrEmpty(smeltCode))
                {
                    this.PdSmeltCode = this.Db.PdSmeltCode.FirstOrDefault(f => f.SmeltCode == smeltCode);
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