namespace WarrantyManage.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using Common.IService;
    using Common.Service;
    using DataLibrary;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Models;
    using MySql.Data.MySqlClient;
    using Newtonsoft.Json;

    [Authorize]
    public class BatcodeController : Controller
    {
        private DataLibrary.DataContext db;
        private IBatcodeService batcodeService;
        private IUserService userService;

        public BatcodeController(DataLibrary.DataContext db, IUserService userService, IBatcodeService codeservice)
        {
            this.db = db;
            this.userService = userService;
            this.batcodeService = codeservice;
        }

        /// <summary>
        /// 生成批号
        /// </summary>
        /// <param name="hiddId">车间ID</param>
        /// <param name="number">钢坯支数</param>
        /// <param name="pieceweight">单重</param>
        /// <param name="totalweight">总重</param>
        /// <param name="rate">成材率</param
        /// <param name="workshopid">班组id</param>
        /// <returns>生成成功的批号ID</returns>
        [HttpPost]
        public int Create(int hiddId, int number, double pieceweight, double totalweight, double rate, int workshopid)
        {
            var rs = this.db.PdWorkshop.FirstOrDefault(p => p.Id == hiddId);

            if (rs != null)
            {
                CommonResult result = this.batcodeService.GenerateNextBatcode(rs.Id, workshopid, true);
                if (result.Status == (int)CommonResultStatus.Success)
                {
                    var code = this.db.PdBatcode.FirstOrDefault(p => p.Batcode == result.Data.ToString());
                    if (code != null)
                    {
                        code.Adder = this.userService.ApplicationUser.Mng_admin.RealName;
                        code.Workshopid = hiddId;
                        code.Billetnumber = number;
                        code.Billetpieceweight = pieceweight;
                        code.Productrate = rate;

                        this.db.PdBatcode.Update(code);
                    }
                }
            }

            return this.db.SaveChanges();
        }

        public ActionResult GetWorkShop(int id)
        {
            var wsTMList = this.db.PdWorkshopTeam.Where(w => w.WorkshopId == id).ToList();
            return this.Json(wsTMList);
        }
    }
}