namespace WarrantyManage.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Common.IService;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Models;

    [Authorize]
    public class SiteLinkController : Controller
    {
        private readonly IUserService userService;
        private DataLibrary.DataContext db;

        public SiteLinkController(DataLibrary.DataContext db, IUserService userService)
        {
            this.db = db;
            this.userService = userService;
        }

        [HttpPost]
        public string AddLink(string name, string description, string url, string picLink, int? picHeight, int? picWidth, int? isShow, int? position, int? linkType)
        {
            if (this.db.SiteLink.FirstOrDefault(c => c.Name == name) != null)
            {
                return "Exit";
            }
            else
            {
                var maxSequence = this.db.SiteLink.OrderByDescending(c => c.Sequence).FirstOrDefault();
                var obj = new DataLibrary.SiteLink()
                {
                    Name = name,
                    Description = description,
                    Url = url,
                    PicLink = picLink,
                    PicHeight = picHeight,
                    PicWidth = picWidth,
                    IsShow = isShow,
                    Sequence = Util.Extensions.ToInt(maxSequence == null ? 0 : maxSequence.Sequence) + 1,
                    Position = position,
                    LinkType = linkType,
                    CreateTime = (int)Util.Extensions.GetCurrentUnixTime(),
                    AdminId = this.userService.ApplicationUser.Mng_admin.Id
                };
                this.db.SiteLink.Add(obj);
            }

            if (this.db.SaveChanges() > 0)
            {
                return "ok";
            }

            return "no";
        }

        [HttpPost]
        public string LinkEdit(int id, string name, string description, string url, string picLink, int picHeight, int picWidth, int isShow, int position, int linkType)
        {
            if (this.db.SiteLink.FirstOrDefault(c => c.Name == name && c.Id != id) != null)
            {
                return "Exist";
            }
            else
            {
                var lin = this.db.SiteLink.FirstOrDefault(c => c.Id == id);
                if (lin != null)
                {
                    lin.Name = name;
                    lin.Description = description;
                    lin.Url = url;
                    lin.PicLink = picLink;
                    lin.PicHeight = picHeight;
                    lin.PicWidth = picWidth;
                    lin.IsShow = isShow;
                    lin.Position = position;
                    lin.LinkType = linkType;
                    this.db.SaveChanges();
                }
            }

            return "ok";
        }

        [HttpPost]
        public string LinkDel(string ids)
        {
            if (string.IsNullOrEmpty(ids))
            {
                return "0";
            }

            ids = ids.TrimEnd(',');
            string[] idarr = ids.Split(',');
            if (idarr.Length > 0)
            {
                List<int> idlist = new List<int>();
                for (int i = 0; i < idarr.Length; i++)
                {
                    int id = 0;
                    int.TryParse(idarr[i], out id);
                    var link = this.db.SiteLink.FirstOrDefault(c => c.Id == id);
                    if (link != null)
                    {
                        var temp2 = this.db.SiteLink.FirstOrDefault(c => c.Name == link.Name);
                        if (temp2 != null)
                        {
                            idlist.Add(id);
                        }
                    }
                }

                var list = this.db.SiteLink.Where(p => idlist.Contains(p.Id)).ToList();
                var sql = this.db.SiteLink.AsQueryable();
                this.db.SiteLink.RemoveRange(list);
                this.db.SaveChanges();
            }

            return "ok";
        }

        [HttpPost]
        public bool LinkOrderDown(int id)
         {
            var result = this.db.SiteLink.FirstOrDefault(c => c.Id == id);
            if (result != null && result.Sequence > 0)
            {
                var pre = this.db.SiteLink.Where(c => c.Sequence < result.Sequence)
                    .OrderByDescending(c => c.Sequence).FirstOrDefault();
                if (pre != null)
                {
                    int tempSequence = pre.Sequence;
                    pre.Sequence = result.Sequence;
                    result.Sequence = tempSequence;
                }

                this.db.SaveChanges();

                return true;
            }

            return false;
        }

        [HttpPost]
        public bool LinkOrderUp(int id)
        {
            var result = this.db.SiteLink.FirstOrDefault(c => c.Id == id);
            if (result != null && result.Sequence > 0)
            {
                var next = this.db.SiteLink.Where(c => c.Sequence > result.Sequence)
                    .OrderBy(c => c.Sequence).FirstOrDefault();
                if (next != null)
                {
                    int tempSequence = result.Sequence;
                    result.Sequence = next.Sequence;
                    next.Sequence = tempSequence;
                }

                this.db.SaveChanges();

                return true;
            }

            return false;
        }
    }
}
