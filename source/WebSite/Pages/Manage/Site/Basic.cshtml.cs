namespace WarrantyManage.Pages.Manage.Site
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Models;

    public class BasicModel : AuthorizeModel
    {
        public BasicModel(DataLibrary.DataContext db)
        : base(db)
        {
        }

        public List<DataLibrary.SiteBasic> List { get; set; }

        public bool IsSuccess { get; set; }

        public void OnGet(string tag)
        {
            this.IsSuccess = false;
            this.List = (from c in this.Db.SiteBasic select c).ToList();
        }

        public void OnPostFirst(int id, string siteTitle, string siteTitleEn, List<string> carouselImage, string compName, string compAddress, string siteDesc, string icp, string copyright, string statsCode, string logo, string logoText, string mapLocation)
        {
            var sitebasic = new DataLibrary.SiteBasic()
            {
                Id = id,
                SiteTitle = siteTitle,
                SiteTitleEn = siteTitleEn,
                CompAddress = compAddress,
                CompName = compName,
                SiteDesc = siteDesc,
                CarouselImage = carouselImage,

                // CompIntroduce = CompIntroduce,
                // Hotline = Hotline,
                Icp = icp,
                Copyright = copyright,
                Logo = logo,
                LogoText = logoText,
                StatsCode = statsCode,
                MapLocation = mapLocation
            };
            if (id > 0)
            {
                this.Db.SiteBasic.Update(sitebasic);
            }
            else
            {
                this.Db.SiteBasic.Add(sitebasic);
            }

            this.Db.SaveChanges();
            if (sitebasic.Id > 0)
            {
                this.List = (from c in this.Db.SiteBasic select c).ToList();
                this.IsSuccess = true;
            }
        }
    }
}