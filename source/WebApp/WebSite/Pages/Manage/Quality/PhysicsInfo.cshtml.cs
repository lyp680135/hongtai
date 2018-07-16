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

        public List<DataLibrary.PdSmeltCode> PdSmeltCode { get; set; }

        public List<DataLibrary.BaseQualityStandard> ListQualityStandards { get; set; }

        public DataLibrary.PdQuality PdQuality { get; set; } = new DataLibrary.PdQuality();

        public string BatCode { get; set; }

        public void OnGet(int? id)
        {
            var userId = this.userService.ApplicationUser.Mng_admin.Id;
            var targetCategory = EnumList.TargetCategory.����ָ��;
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
                    BatCode = currentInfo.Batcode;
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
            }
            this.PdSmeltCode = this.Db.PdSmeltCode.OrderByDescending(o => o.Id).Take(100).ToList();
        }
    }
}