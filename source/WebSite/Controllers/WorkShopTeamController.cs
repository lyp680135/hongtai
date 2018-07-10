namespace WarrantyManage.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Authorize]
    public class WorkShopTeamController : Controller
    {
        private Common.IService.IUserService userService;
        private DataLibrary.DataContext db;

        public WorkShopTeamController(DataLibrary.DataContext db, Common.IService.IUserService userService)
        {
            this.userService = userService;
            this.db = db;
        }

        public IActionResult Index()
        {
            return this.View();
        }

        [HttpPost]
        public string Create(string teamName, int workshopId, string leader, string code)
        {
            var temp = this.db.PdWorkshopTeam.FirstOrDefault(c => c.TeamName == teamName && c.WorkshopId == workshopId);
            var temp2 = this.db.PdWorkshopTeam.FirstOrDefault(c => c.Code == code && c.WorkshopId == workshopId);
            if (temp != null)
            {
                return "您已为该车间添加该生产班组，请勿重复添加";
            }

            if (temp2 != null)
            {
                return "该车间已存在该班组代码，请勿重复添加";
            }
            else
            {
                var shopteam = new DataLibrary.PdWorkshopTeam()
                {
                    TeamName = teamName,
                    Leader = leader,
                    WorkshopId = workshopId,
                    Adder = this.userService.ApplicationUser.Mng_admin.RealName,
                    Code = code,
                    CreateTime = (int)Util.Extensions.GetCurrentUnixTime()
                };

                this.db.PdWorkshopTeam.Add(shopteam);
                this.db.SaveChanges();
                return shopteam.Id.ToString();
            }
        }

        [HttpGet]
        public ObjectResult Edit(int id)
        {
            var rs = this.db.PdWorkshopTeam.FirstOrDefault(p => p.Id == id);

            if (rs != null)
            {
                return new ObjectResult(rs);
            }

            return null;
        }

        [HttpPost]
        public int Edit(int hiddId, string teamName, int workshopId, string leader, string code)
        {
            var rs = this.db.PdWorkshopTeam.FirstOrDefault(p => p.Id == hiddId);
            var temp = this.db.PdWorkshopTeam.FirstOrDefault(c => c.TeamName == teamName && c.Id != hiddId && c.WorkshopId == workshopId);
            var temp2 = this.db.PdWorkshopTeam.FirstOrDefault(c => c.Code == code && c.WorkshopId == workshopId && c.Id != hiddId);
            if (temp != null)
            {
                return -1;
            }

            if (temp2 != null)
            {
                return -2;
            }
            else
            {
                if (rs != null)
                {
                    rs.TeamName = teamName;
                    rs.Leader = leader;
                    rs.WorkshopId = workshopId;
                    rs.Code = code;
                }

                this.db.PdWorkshopTeam.Update(rs);

                return this.db.SaveChanges();
            }
        }

        [HttpPost]
        public int Delete(string ids)
        {
            if (string.IsNullOrEmpty(ids))
            {
                return 0;
            }

            ids = ids.TrimEnd(',');
            string[] idarr = ids.Split(',');
            var tempFlag = true;
            if (idarr.Length > 0)
            {
                List<int> idlist = new List<int>();
                for (int i = 0; i < idarr.Length; i++)
                {
                    int.TryParse(idarr[i], out int id);
                    var temp = this.db.PdProduct.FirstOrDefault(c => c.WorkShift == id);
                    if (temp != null)
                    {
                        tempFlag = false;
                        break;
                    }
                    else
                    {
                        if (id > 0)
                        {
                            idlist.Add(id);
                        }
                    }
                }

                if (tempFlag)
                {
                    var list = this.db.PdWorkshopTeam.Where(p => idlist.Contains(p.Id)).ToList();
                    var sql = this.db.PdWorkshopTeam.AsQueryable();
                    this.db.PdWorkshopTeam.RemoveRange(list);
                    return this.db.SaveChanges();
                }
            }

            return 0;
        }
    }
}