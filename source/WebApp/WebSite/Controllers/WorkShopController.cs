namespace WarrantyManage.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using DataLibrary;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Models;
    using MySql.Data.MySqlClient;
    using Newtonsoft.Json;
    using static DataLibrary.EnumList;

    [Authorize]
    public class WorkShopController : Controller
    {
        private DataLibrary.DataContext db;

        private Common.IService.IUserService userService;

        public WorkShopController(DataLibrary.DataContext db, Common.IService.IUserService userService)
        {
            this.db = db;
            this.userService = userService;
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
                    var workshop = this.db.PdWorkshop.FirstOrDefault(c => c.Id == id);
                    var code = workshop.Code.ToUpper();
                    var temp = this.db.PdQRCodeAuth.FirstOrDefault(c => c.WorkshopId == id);
                    var temp2 = this.db.PdProduct.FirstOrDefault(c => c.Batcode.Substring(0, 1) == code);
                    if (temp2 != null || temp != null)
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
                    var list = this.db.PdWorkshop.Where(p => idlist.Contains(p.Id)).ToList();
                    var sql = this.db.PdWorkshop.AsQueryable();
                    this.db.PdWorkshop.RemoveRange(list);
                    return this.db.SaveChanges();
                }
            }

            return 0;
        }

        [HttpPost]
        public ActionResult Add(string workShopName, string code, string[] rukuName, string[] rukuTel, string[] chukuName, string[] chukuTel, string[] luruName, string[] ruluTel, int[] wuliorhuaxue)
        {
            List<int> warehousingoperatorids = new List<int>();
            List<int> outboundoperatorids = new List<int>();
            List<int> qualityentryclerkids = new List<int>();
            string sameName = string.Empty;

            // 炉号工ID集合
            // List<int> furnacemanids = new List<int>();
            var tempClass = this.db.PdWorkshop.FirstOrDefault(c => c.Name == workShopName);
            var tempClass2 = this.db.PdWorkshop.FirstOrDefault(c => c.Code == code);
            var quanlityhuaxue = this.db.MngPermissiongroup.FirstOrDefault(c => c.GroupName == "质量员-化学");
            if (tempClass != null)
            {
                return this.AjaxResult(false, "您已添加该车间，请勿重复添加");
            }
            else if (tempClass2 != null)
            {
                return this.AjaxResult(false, "该车间代码已被添加，请重新设置！");
            }

            // 如果数据库中不存在重复数据
            else
            {
                // 循环入库操作员
                for (var i = 0; i < rukuName.Length; i++)
                {
                    // 首先查询Admin表中是否存在以该手机号为用户名的用户，如果有，则不新增到Admin表中，直接加入到入库操作员Id数组中
                    DataLibrary.MngAdmin rukuperson = this.db.MngAdmin.FirstOrDefault(c => c.UserName == rukuTel[i]);

                    // 判断是否存在姓名重复
                    DataLibrary.MngAdmin same = this.db.MngAdmin.FirstOrDefault(c => c.RealName == rukuName[i] && c.UserName != rukuTel[i]);
                    if (same != null)
                    {
                        return this.AjaxResult(false, same.RealName + "已存在，请加上识别标识,如张三(A车间)");
                    }

                    // 不重复则添加到用户表
                    else
                    {
                        if (rukuperson != null)
                        {
                            // 如果原有用户没有入库操作员角色，则多赋予入库操作员角色权限
                            if (!rukuperson.GroupManage.Object.Contains((int)GroupManage.入库操作员))
                            {
                                List<int> gmanage = new List<int>();
                                foreach (var group in rukuperson.GroupManage.Object)
                                {
                                    gmanage.Add(group);
                                }

                                gmanage.Add((int)GroupManage.入库操作员);
                                rukuperson.GroupManage = gmanage;
                            }

                            rukuperson.RealName = rukuName[i];
                            warehousingoperatorids.Add(rukuperson.Id);
                            this.db.SaveChanges();
                        }
                        else
                        {
                            var tempadmin = this.db.MngAdmin.Add(
                                 new DataLibrary.MngAdmin()
                                 {
                                     DepartId = 1,
                                     FirstChar = string.Empty,
                                     GroupManage = (new int[] { (int)GroupManage.入库操作员 }).ToList(),
                                     InJob = true,
                                     Password = string.Empty,
                                     RealName = rukuName[i],
                                     Sex = true,
                                     UserName = rukuTel[i]
                                 });
                            this.db.SaveChanges();
                            warehousingoperatorids.Add(tempadmin.Entity.Id);
                        }
                    }
                }

                for (var i = 0; i < chukuName.Length; i++)
                {
                    // 判断是否存在姓名重复
                    DataLibrary.MngAdmin same = this.db.MngAdmin.FirstOrDefault(c => c.RealName == chukuName[i] && c.UserName != chukuTel[i]);
                    if (same != null)
                    {
                        return this.AjaxResult(false, same.RealName + "已存在，请加上识别标识,如张三(A车间)");
                    }

                    DataLibrary.MngAdmin chukuperson = this.db.MngAdmin.FirstOrDefault(c => c.UserName == chukuTel[i]);
                    if (chukuperson != null)
                    {
                        // 如果原有用户没有出库操作员角色，则多赋予出库操作员角色权限
                        if (!chukuperson.GroupManage.Object.Contains((int)GroupManage.出库操作员))
                        {
                            List<int> gmanage = new List<int>();
                            foreach (var group in chukuperson.GroupManage.Object)
                            {
                                gmanage.Add(group);
                            }

                            gmanage.Add((int)GroupManage.出库操作员);
                            chukuperson.GroupManage = gmanage;
                        }

                        chukuperson.RealName = chukuName[i];
                        outboundoperatorids.Add(chukuperson.Id);
                        this.db.SaveChanges();
                    }
                    else
                    {
                        var tempadmin = this.db.MngAdmin.Add(
                                new DataLibrary.MngAdmin()
                                {
                                    DepartId = 1,
                                    FirstChar = string.Empty,
                                    GroupManage = (new int[] { (int)GroupManage.出库操作员 }).ToList(),
                                    InJob = true,
                                    Password = string.Empty,
                                    RealName = chukuName[i],
                                    Sex = true,
                                    UserName = chukuTel[i]
                                });
                        this.db.SaveChanges();
                        outboundoperatorids.Add(tempadmin.Entity.Id);
                    }
                }

                for (var i = 0; i < luruName.Length; i++)
                {
                    // 判断是否存在姓名重复
                    DataLibrary.MngAdmin same = this.db.MngAdmin.FirstOrDefault(c => c.RealName == luruName[i] && c.UserName != ruluTel[i]);
                    if (same != null)
                    {
                        return this.AjaxResult(false, same.RealName + "已存在，请加上识别标识,如张三(A车间)");
                    }

                    DataLibrary.MngAdmin ruluperson = this.db.MngAdmin.FirstOrDefault(c => c.UserName == ruluTel[i]);
                    if (ruluperson != null)
                    {
                        // 首先移除掉所有质量员角色
                        ruluperson.GroupManage.Object.Remove((int)GroupManage.质量员);
                        ruluperson.GroupManage.Object.Remove(quanlityhuaxue.Id);

                        // 如果原有用户没有质量员角色，则多赋予质量员角色权限
                        if (wuliorhuaxue[i] == 1)
                        {
                            if (!ruluperson.GroupManage.Object.Contains((int)GroupManage.质量员))
                            {
                                List<int> gmanage = new List<int>();
                                foreach (var group in ruluperson.GroupManage.Object)
                                {
                                    gmanage.Add(group);
                                }

                                gmanage.Add((int)GroupManage.质量员);
                                ruluperson.GroupManage = gmanage;
                            }
                        }

                        if (wuliorhuaxue[i] == 2)
                        {
                            if (!this.userService.IsHaveRole("质量员-化学", ruluperson.Id))
                            {
                                List<int> gmanage = new List<int>();
                                foreach (var group in ruluperson.GroupManage.Object)
                                {
                                    gmanage.Add(group);
                                }

                                if (quanlityhuaxue != null)
                                {
                                    gmanage.Add(quanlityhuaxue.Id);
                                }

                                ruluperson.GroupManage = gmanage;
                            }
                        }

                        if (wuliorhuaxue[i] == 3)
                        {
                            List<int> gmanage = new List<int>();
                            foreach (var group in ruluperson.GroupManage.Object)
                            {
                                gmanage.Add(group);
                            }

                            if (!this.userService.IsHaveRole("质量员-化学", ruluperson.Id))
                            {
                                if (quanlityhuaxue != null)
                                {
                                    gmanage.Add(quanlityhuaxue.Id);
                                }
                            }

                            if (!ruluperson.GroupManage.Object.Contains((int)GroupManage.质量员))
                            {
                                gmanage.Add((int)GroupManage.质量员);
                            }

                            ruluperson.GroupManage = gmanage;
                        }

                        ruluperson.RealName = luruName[i];
                        qualityentryclerkids.Add(ruluperson.Id);
                        this.db.SaveChanges();
                    }
                    else
                    {
                        // 如果原有用户没有质量员角色，则多赋予质量员角色权限
                        if (wuliorhuaxue[i] == 1)
                        {
                            var tempadmin = this.db.MngAdmin.Add(
                               new DataLibrary.MngAdmin()
                               {
                                   DepartId = 1,
                                   FirstChar = string.Empty,
                                   GroupManage = (new int[] { (int)GroupManage.质量员 }).ToList(),
                                   InJob = true,
                                   Password = string.Empty,
                                   RealName = luruName[i],
                                   Sex = true,
                                   UserName = ruluTel[i]
                               });
                            this.db.SaveChanges();
                            qualityentryclerkids.Add(tempadmin.Entity.Id);
                        }

                        if (wuliorhuaxue[i] == 2)
                        {
                            var tempadmin = this.db.MngAdmin.Add(
                              new DataLibrary.MngAdmin()
                              {
                                  DepartId = 1,
                                  FirstChar = string.Empty,
                                  GroupManage = (new int[] { quanlityhuaxue != null ? quanlityhuaxue.Id : 0 }).ToList(),
                                  InJob = true,
                                  Password = string.Empty,
                                  RealName = luruName[i],
                                  Sex = true,
                                  UserName = ruluTel[i]
                              });
                            // var tempadmin = this.db.MngAdmin.FirstOrDefault();
                            this.db.SaveChanges();
                            qualityentryclerkids.Add(tempadmin.Entity.Id);
                        }

                        if (wuliorhuaxue[i] == 3)
                        {
                            var tempadmin = this.db.MngAdmin.Add(
                              new DataLibrary.MngAdmin()
                              {
                                  DepartId = 1,
                                  FirstChar = string.Empty,
                                  GroupManage = (new int[] { quanlityhuaxue != null ? quanlityhuaxue.Id : 0, (int)GroupManage.质量员 }).ToList(),
                                  InJob = true,
                                  Password = string.Empty,
                                  RealName = luruName[i],
                                  Sex = true,
                                  UserName = ruluTel[i]
                              });
                            // var tempadmin = this.db.MngAdmin.FirstOrDefault();
                            this.db.SaveChanges();
                            qualityentryclerkids.Add(tempadmin.Entity.Id);
                        }
                    }
                    #region 隐藏炉号工代码
                    //// 循环炉号工
                    // for (var i = 0; i < luhaoName.Length; i++)
                    // {
                    //    // 判断是否存在姓名重复
                    //    DataLibrary.MngAdmin same = this.db.MngAdmin.FirstOrDefault(c => c.RealName == luhaoName[i] && c.UserName != luhaoTel[i]);
                    //    if (same != null)
                    //    {
                    //        return this.AjaxResult(false, same.RealName + "已存在，请加上识别标识,如张三(A车间)");
                    //    }

                    // DataLibrary.MngAdmin luhaoperson = this.db.MngAdmin.FirstOrDefault(c => c.UserName == luhaoTel[i]);
                    //    if (luhaoperson != null)
                    //    {
                    //        // 如果原有用户没有炉号工角色，则多赋予炉号工角色权限
                    //        if (!luhaoperson.GroupManage.Object.Contains((int)GroupManage.炉号工))
                    //        {
                    //            List<int> gmanage = new List<int>();
                    //            foreach (var group in luhaoperson.GroupManage.Object)
                    //            {
                    //                gmanage.Add(group);
                    //            }

                    // gmanage.Add((int)GroupManage.炉号工);
                    //            luhaoperson.GroupManage = gmanage;
                    //        }

                    // luhaoperson.RealName = luruName[i];
                    //        furnacemanids.Add(luhaoperson.Id);
                    //        this.db.SaveChanges();
                    //    }
                    //    else
                    //    {
                    //        var tempadmin = this.db.MngAdmin.Add(
                    //               new DataLibrary.MngAdmin()
                    //               {
                    //                   DepartId = 1,
                    //                   FirstChar = string.Empty,
                    //                   GroupManage = (new int[] { (int)GroupManage.炉号工 }).ToList(),
                    //                   InJob = true,
                    //                   Password = string.Empty,
                    //                   RealName = luhaoName[i],
                    //                   Sex = true,
                    //                   UserName = luhaoTel[i]
                    //               });
                    //        this.db.SaveChanges();
                    //        furnacemanids.Add(tempadmin.Entity.Id);
                    //    }
                    // }
                    #endregion
                }

                var wsclass = new DataLibrary.PdWorkshop()
                {
                    Name = workShopName,
                    Code = code.ToUpper(),
                    Inputer = string.Join(",", warehousingoperatorids),
                    Outputer = string.Join(",", outboundoperatorids),
                    QAInputer = string.Join(",", qualityentryclerkids),

                    // Furnacer = string.Join(",", furnacemanids),
                };
                this.db.PdWorkshop.Add(wsclass);
                this.db.SaveChanges();
                return this.AjaxResult(true, "操作成功");
            }
        }

        [HttpPost]
        public ActionResult Edit(int hiddId, string workShopName, string code, string[] rukuName, string[] rukuTel, int[] workShopTeamId, string[] chukuName, string[] chukuTel, string[] luruName, string[] ruluTel, string[] luhaoName, string[] luhaoTel, int[] wuliorhuaxue)
        {
            List<int> warehousingoperatorids = new List<int>();
            List<int> outboundoperatorids = new List<int>();
            List<int> qualityentryclerkids = new List<int>();

            // 炉号工ID集合
            List<int> furnacemanids = new List<int>();
            var quanlityhuaxue = this.db.MngPermissiongroup.FirstOrDefault(c => c.GroupName == "质量员-化学");
            if (this.db.PdWorkshop.FirstOrDefault(c => c.Name == workShopName && c.Id != hiddId) != null)
            {
                return this.AjaxResult(false, "您已添加该车间，请勿重复添加");
            }
            else if (this.db.PdWorkshop.FirstOrDefault(c => c.Code == code && c.Id != hiddId) != null)
            {
                return this.AjaxResult(false, "该车间代码已被添加，请重新设置！");
            }
            else
            {
                var rs = this.db.PdWorkshop.FirstOrDefault(p => p.Id == hiddId);
                if (rs != null)
                {
                    rs.Name = workShopName;
                    rs.Code = code.ToUpper();
                    for (var i = 0; i < rukuName.Length; i++)
                    {
                        DataLibrary.MngAdmin rukuperson = this.db.MngAdmin.FirstOrDefault(c => c.UserName == rukuTel[i]);
                        DataLibrary.MngAdmin same = null;
                        DataLibrary.PdWorkshopTeamAdminRelation relationadmin = null;

                        // 如果为空则是新增
                        if (rukuperson == null)
                        {
                            same = this.db.MngAdmin.FirstOrDefault(c => c.RealName == rukuName[i] && c.UserName != rukuTel[i]);
                        }
                        else
                        {
                            // 判断是否存在姓名重复
                            same = this.db.MngAdmin.FirstOrDefault(c => c.RealName == rukuName[i] && c.UserName != rukuTel[i] && c.Id != rukuperson.Id);
                        }

                        if (same != null)
                        {
                            return this.AjaxResult(false, same.RealName + " 已存在，请加上识别标识,如张三(A车间)");
                        }

                        if (rukuperson != null)
                        {
                            relationadmin = this.db.PdWorkshopTeamAdminRelation.FirstOrDefault(c => c.AdminId == rukuperson.Id && c.WorkShopId == rs.Id);

                            if (!rukuperson.GroupManage.Object.Contains((int)GroupManage.入库操作员))
                            {
                                List<int> list_temp = rukuperson.GroupManage.Object;
                                list_temp.Add((int)GroupManage.入库操作员);

                                rukuperson.GroupManage = list_temp;
                            }

                            if (relationadmin == null)
                            {
                                var relation = this.db.PdWorkshopTeamAdminRelation.Add(
                                   new DataLibrary.PdWorkshopTeamAdminRelation()
                                   {
                                       AdminId = rukuperson.Id,
                                       WorkShopTeamId = workShopTeamId[i],
                                       WorkShopId = rs.Id
                                   });
                            }
                            else
                            {
                                relationadmin.AdminId = rukuperson.Id;
                                relationadmin.WorkShopTeamId = workShopTeamId[i];
                                relationadmin.WorkShopId = rs.Id;
                            }

                            rukuperson.RealName = rukuName[i];
                            warehousingoperatorids.Add(rukuperson.Id);
                            this.db.SaveChanges();
                        }
                        else
                        {
                            var tempadmin = this.db.MngAdmin.Add(
                                 new DataLibrary.MngAdmin()
                                 {
                                     DepartId = 1,
                                     FirstChar = string.Empty,
                                     GroupManage = (new int[] { (int)GroupManage.入库操作员 }).ToList(),
                                     InJob = true,
                                     Password = string.Empty,
                                     RealName = rukuName[i],
                                     Sex = true,
                                     UserName = rukuTel[i]
                                 });
                            this.db.SaveChanges();
                            relationadmin = this.db.PdWorkshopTeamAdminRelation.FirstOrDefault(c => c.AdminId == tempadmin.Entity.Id && c.WorkShopId == rs.Id);
                            if (relationadmin == null)
                            {
                                var relation = this.db.PdWorkshopTeamAdminRelation.Add(
                                   new DataLibrary.PdWorkshopTeamAdminRelation()
                                   {
                                       AdminId = tempadmin.Entity.Id,
                                       WorkShopTeamId = workShopTeamId[i],
                                       WorkShopId = rs.Id
                                   });
                            }
                            else
                            {
                                relationadmin.AdminId = tempadmin.Entity.Id;
                                relationadmin.WorkShopTeamId = workShopTeamId[i];
                                relationadmin.WorkShopId = rs.Id;
                            }

                            this.db.SaveChanges();
                            warehousingoperatorids.Add(tempadmin.Entity.Id);
                        }
                    }

                    for (var i = 0; i < chukuName.Length; i++)
                    {
                        DataLibrary.MngAdmin chukuperson = this.db.MngAdmin.FirstOrDefault(c => c.UserName == chukuTel[i]);
                        DataLibrary.MngAdmin same = null;

                        // 如果为空则是新增
                        if (chukuperson == null)
                        {
                            same = this.db.MngAdmin.FirstOrDefault(c => c.RealName == chukuName[i] && c.UserName != chukuTel[i]);
                        }
                        else
                        {
                            // 判断是否存在姓名重复
                            same = this.db.MngAdmin.FirstOrDefault(c => c.RealName == chukuName[i] && c.UserName != chukuTel[i] && c.Id != chukuperson.Id);
                        }

                        if (same != null)
                        {
                            return this.AjaxResult(false, same.RealName + " 已存在，请加上识别标识,如张三(A车间)");
                        }

                        if (chukuperson != null)
                        {
                            if (!chukuperson.GroupManage.Object.Contains((int)GroupManage.出库操作员))
                            {
                                List<int> list_temp = chukuperson.GroupManage.Object;
                                list_temp.Add((int)GroupManage.出库操作员);

                                chukuperson.GroupManage = list_temp;
                            }

                            chukuperson.RealName = chukuName[i];
                            this.db.SaveChanges();

                            outboundoperatorids.Add(chukuperson.Id);
                        }
                        else
                        {
                            var tempadmin = this.db.MngAdmin.Add(
                                new DataLibrary.MngAdmin()
                                {
                                    DepartId = 1,
                                    FirstChar = string.Empty,
                                    GroupManage = (new int[] { (int)GroupManage.出库操作员 }).ToList(),
                                    InJob = true,
                                    Password = string.Empty,
                                    RealName = chukuName[i],
                                    Sex = true,
                                    UserName = chukuTel[i]
                                });
                            this.db.SaveChanges();
                            outboundoperatorids.Add(tempadmin.Entity.Id);
                        }
                    }

                    for (var i = 0; i < luruName.Length; i++)
                    {
                        DataLibrary.MngAdmin ruluperson = this.db.MngAdmin.FirstOrDefault(c => c.UserName == ruluTel[i]);
                        DataLibrary.MngAdmin same = null;

                        // 如果为空则是新增
                        if (ruluperson == null)
                        {
                            same = this.db.MngAdmin.FirstOrDefault(c => c.RealName == luruName[i] && c.UserName != ruluTel[i]);
                        }
                        else
                        {
                            // 判断是否存在姓名重复
                            same = this.db.MngAdmin.FirstOrDefault(c => c.RealName == luruName[i] && c.UserName != ruluTel[i] && c.Id != ruluperson.Id);
                        }

                        if (same != null)
                        {
                            return this.AjaxResult(false, same.RealName + " 已存在，请加上识别标识,如张三(A车间)");
                        }

                        if (ruluperson != null)
                        {
                            // 首先移除掉所有质量员角色
                            ruluperson.GroupManage.Object.Remove((int)GroupManage.质量员);
                            ruluperson.GroupManage.Object.Remove(quanlityhuaxue.Id);

                            // 如果原有用户没有质量员角色，则多赋予质量员角色权限
                            if (wuliorhuaxue[i] == 1)
                            {
                                if (!ruluperson.GroupManage.Object.Contains((int)GroupManage.质量员))
                                {
                                    List<int> gmanage = new List<int>();
                                    foreach (var group in ruluperson.GroupManage.Object)
                                    {
                                        gmanage.Add(group);
                                    }

                                    gmanage.Add((int)GroupManage.质量员);
                                    ruluperson.GroupManage = gmanage;
                                }
                            }

                            if (wuliorhuaxue[i] == 2)
                            {
                                if (!this.userService.IsHaveRole("质量员-化学", ruluperson.Id))
                                {
                                    List<int> gmanage = new List<int>();
                                    foreach (var group in ruluperson.GroupManage.Object)
                                    {
                                        gmanage.Add(group);
                                    }

                                    if (quanlityhuaxue != null)
                                    {
                                        gmanage.Add(quanlityhuaxue.Id);
                                    }

                                    ruluperson.GroupManage = gmanage;
                                }
                            }

                            if (wuliorhuaxue[i] == 3)
                            {
                                List<int> gmanage = new List<int>();
                                foreach (var group in ruluperson.GroupManage.Object)
                                {
                                    gmanage.Add(group);
                                }

                                if (!this.userService.IsHaveRole("质量员-化学", ruluperson.Id))
                                {
                                    if (quanlityhuaxue != null)
                                    {
                                        gmanage.Add(quanlityhuaxue.Id);
                                    }
                                }

                                if (!ruluperson.GroupManage.Object.Contains((int)GroupManage.质量员))
                                {
                                    gmanage.Add((int)GroupManage.质量员);
                                }

                                ruluperson.GroupManage = gmanage;
                            }

                            ruluperson.RealName = luruName[i];
                            qualityentryclerkids.Add(ruluperson.Id);
                            this.db.SaveChanges();
                        }
                        else
                        {
                            // 如果原有用户没有质量员角色，则多赋予质量员角色权限
                            if (wuliorhuaxue[i] == 1)
                            {
                                var tempadmin = this.db.MngAdmin.Add(
                                   new DataLibrary.MngAdmin()
                                   {
                                       DepartId = 1,
                                       FirstChar = string.Empty,
                                       GroupManage = (new int[] { (int)GroupManage.质量员 }).ToList(),
                                       InJob = true,
                                       Password = string.Empty,
                                       RealName = luruName[i],
                                       Sex = true,
                                       UserName = ruluTel[i]
                                   });
                                this.db.SaveChanges();
                                qualityentryclerkids.Add(tempadmin.Entity.Id);
                            }

                            if (wuliorhuaxue[i] == 2)
                            {
                                var tempadmin = this.db.MngAdmin.Add(
                                  new DataLibrary.MngAdmin()
                                  {
                                      DepartId = 1,
                                      FirstChar = string.Empty,
                                      GroupManage = (new int[] { quanlityhuaxue != null ? quanlityhuaxue.Id : 0 }).ToList(),
                                      InJob = true,
                                      Password = string.Empty,
                                      RealName = luruName[i],
                                      Sex = true,
                                      UserName = ruluTel[i]
                                  });
                                this.db.SaveChanges();
                                qualityentryclerkids.Add(tempadmin.Entity.Id);
                            }

                            if (wuliorhuaxue[i] == 3)
                            {
                                var tempadmin = this.db.MngAdmin.Add(
                                  new DataLibrary.MngAdmin()
                                  {
                                      DepartId = 1,
                                      FirstChar = string.Empty,
                                      GroupManage = (new int[] { quanlityhuaxue != null ? quanlityhuaxue.Id : 0, (int)GroupManage.质量员 }).ToList(),
                                      InJob = true,
                                      Password = string.Empty,
                                      RealName = luruName[i],
                                      Sex = true,
                                      UserName = ruluTel[i]
                                  });
                                this.db.SaveChanges();
                                qualityentryclerkids.Add(tempadmin.Entity.Id);
                            }
                        }
                    }

                    for (var i = 0; i < luhaoName.Length; i++)
                    {
                        DataLibrary.MngAdmin luhaoperson = this.db.MngAdmin.FirstOrDefault(c => c.UserName == luhaoTel[i]);
                        DataLibrary.MngAdmin same = null;

                        // 如果为空则是新增
                        if (luhaoperson == null)
                        {
                            same = this.db.MngAdmin.FirstOrDefault(c => c.RealName == luhaoName[i] && c.UserName != luhaoTel[i]);
                        }
                        else
                        {
                            // 判断是否存在姓名重复
                            same = this.db.MngAdmin.FirstOrDefault(c => c.RealName == luhaoName[i] && c.UserName != luhaoTel[i] && c.Id != luhaoperson.Id);
                        }

                        if (same != null)
                        {
                            return this.AjaxResult(false, same.RealName + " 已存在，请加上识别标识,如张三(A车间)");
                        }

                        if (luhaoperson != null)
                        {
                            if (!luhaoperson.GroupManage.Object.Contains((int)GroupManage.炉号工))
                            {
                                List<int> list_temp = luhaoperson.GroupManage.Object;
                                list_temp.Add((int)GroupManage.炉号工);

                                luhaoperson.GroupManage = list_temp;
                            }

                            luhaoperson.RealName = luhaoName[i];
                            furnacemanids.Add(luhaoperson.Id);
                            this.db.SaveChanges();
                        }
                        else
                        {
                            var tempadmin = this.db.MngAdmin.Add(
                                new DataLibrary.MngAdmin()
                                {
                                    DepartId = 1,
                                    FirstChar = string.Empty,
                                    GroupManage = (new int[] { (int)GroupManage.炉号工 }).ToList(),
                                    InJob = true,
                                    Password = string.Empty,
                                    RealName = luhaoName[i],
                                    Sex = true,
                                    UserName = luhaoTel[i]
                                });
                            this.db.SaveChanges();
                            furnacemanids.Add(tempadmin.Entity.Id);
                        }
                    }

                    rs.Inputer = string.Join(",", warehousingoperatorids);
                    rs.Outputer = string.Join(",", outboundoperatorids);
                    rs.QAInputer = string.Join(",", qualityentryclerkids);
                    rs.Furnacer = string.Join(",", furnacemanids);
                    this.db.PdWorkshop.Update(rs);

                    this.db.SaveChanges();

                    return this.AjaxResult(true, "操作成功");
                }
                else
                {
                    return this.AjaxResult(false, "当前车间不存在");
                }
            }
        }

        // 从该车间移除掉该员工
        [HttpPost]
        public ActionResult DeleWorker(int hiddId, int id, string flag)
        {
            List<int> warehousingoperatorids = new List<int>();
            List<int> outboundoperatorids = new List<int>();
            List<int> qualityentryclerkids = new List<int>();

            // 炉号工ID集合
            List<int> furnacemanids = new List<int>();
            var workshop = this.db.PdWorkshop.FirstOrDefault(c => c.Id == hiddId);
            if (workshop != null)
            {
                if (flag == "luhao")
                {
                    if (workshop.Furnacer.Contains(id.ToString()))
                    {
                        string furnace = this.ReChange(workshop.Furnacer, id);
                        workshop.Furnacer = furnace;
                        this.db.SaveChanges();
                    }
                    else
                    {
                        return this.AjaxResult(false, "当前人员不属于当前车间！");
                    }
                }

                if (flag == "ruku")
                {
                    if (workshop.Inputer.Contains(id.ToString()))
                    {
                        string input = this.ReChange(workshop.Inputer, id);
                        workshop.Inputer = input;
                        this.db.SaveChanges();
                    }
                    else
                    {
                        return this.AjaxResult(false, "当前人员不属于当前车间！");
                    }
                }

                if (flag == "chuku")
                {
                    if (workshop.Outputer.Contains(id.ToString()))
                    {
                        string output = this.ReChange(workshop.Outputer, id);
                        workshop.Outputer = output;
                        this.db.SaveChanges();
                    }
                    else
                    {
                        return this.AjaxResult(false, "当前人员不属于当前车间！");
                    }
                }

                if (flag == "luru")
                {
                    if (workshop.QAInputer.Contains(id.ToString()))
                    {
                        string qaInput = this.ReChange(workshop.QAInputer, id);
                        workshop.QAInputer = qaInput;
                        this.db.SaveChanges();
                    }
                    else
                    {
                        return this.AjaxResult(false, "当前人员不属于当前车间！");
                    }
                }
            }

            return this.AjaxResult(true, "操作成功");
        }

        /// <summary>
        /// Ajax返回格式
        /// </summary>
        /// <param name="res">返回值</param>
        /// <param name="content">返回数据</param>
        /// <returns>ActionResult</returns>
        private ActionResult AjaxResult(bool res, object content)
        {
            return this.Json(new { state = res, content = content });
        }

        // 转化数据格式
        private string ReChange(string ids, int id)
        {
            List<int> temp = new List<int>();
            foreach (var fur in ids.Split(','))
            {
                temp.Add(Convert.ToInt32(fur));
            }

            if (temp.Contains(id))
            {
                temp.Remove(id);
            }

            string idsafter = string.Join(",", temp);

            return idsafter;
        }
    }
}