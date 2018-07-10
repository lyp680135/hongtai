namespace WarrantyManage.Pages.Manage.Quality
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Common.IService;
    using DataLibrary;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Models;

    public class TemplateInfoModel : AuthorizeModel
    {
        private IBaseService<BaseQualityStandard> baseQualityStandard;

        public TemplateInfoModel(DataLibrary.DataContext db, IBaseService<BaseQualityStandard> baseQualityStandard)
           : base(db)
        {
            this.baseQualityStandard = baseQualityStandard;
        }

        public int MaterialId { get; set; }

        public List<DataLibrary.BaseQualityStandard> ListQualityStandards { get; set; }

        public DataLibrary.PdQuality PdQuality { get; set; } = new DataLibrary.PdQuality();

        public void OnGet(int? id, int mid)
        {
            var bqsInfo = this.Db.BaseQualityStandard.First();
            if (bqsInfo == null)
            {
                this.RedirectToError();
            }
            if (mid > 0)
            {
                this.MaterialId = mid;
            }
            else
            {
                this.MaterialId = Convert.ToInt32(bqsInfo.Materialid);
                mid = Convert.ToInt32(bqsInfo.Materialid);
            }

            this.ListQualityStandards = this.Db.BaseQualityStandard.Where(w => w.Materialid == mid && w.Status == 0).ToList();
            if (id.HasValue && id.Value > 0)
            {
                this.PdQuality = this.Db.PdQuality.FirstOrDefault(c => c.Id == id);
                if (this.PdQuality == null)
                {
                    this.RedirectToError();
                }
            }
        }
    }
}