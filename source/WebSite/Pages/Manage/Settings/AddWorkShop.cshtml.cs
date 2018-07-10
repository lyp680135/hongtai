namespace WarrantyManage.Pages.Manage.Settings
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Models;
    using Util.Helpers;
    using static DataLibrary.EnumList;

    public class AddWorkShopModel : AuthorizeModel
    {
        public AddWorkShopModel(DataLibrary.DataContext db)
          : base(db)
        {
        }

        public bool IsSuccess { get; set; }

        public string Msg { get; set; }

        public void OnGet()
        {
            this.IsSuccess = false;
            this.Msg = string.Empty;
        }

        public void OnPostFirst(string workShopName, string code, string[] rukuName, string[] rukuTel, string[] chukuName, string[] chukuTel, string[] luruName, string[] ruluTel)
        {
            List<int> warehousingoperatorids = new List<int>();
            List<int> outboundoperatorids = new List<int>();
            List<int> qualityentryclerkids = new List<int>();

            List<DataLibrary.MngAdmin> list_MngAdmin = new List<DataLibrary.MngAdmin>();
            var tempClass = this.Db.PdWorkshop.FirstOrDefault(c => c.Name == workShopName);
            var tempClass2 = this.Db.PdWorkshop.FirstOrDefault(c => c.Code == code);
            if (tempClass != null)
            {
                this.Msg = "您已添加该车间，请勿重复添加";
            }
            else if (tempClass2 != null)
            {
                this.Msg = "该车间代码已被添加，请重新设置！";
            }

            // 如果数据库中不存在重复数据
            else
            {
                // 循环入库操作员
                for (var i = 0; i < rukuName.Length; i++)
                {
                    // 首先查询Admin表中是否存在以该手机号为用户名的用户，如果有，则不新增到Admin表中，直接加入到入库操作员Id数组中
                    DataLibrary.MngAdmin rukuperson = this.Db.MngAdmin.FirstOrDefault(c => c.UserName == rukuTel[i]);
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
                        this.Db.SaveChanges();
                    }
                    else
                    {
                        list_MngAdmin.Add(new DataLibrary.MngAdmin()
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
                    }
                }

                for (var i = 0; i < chukuName.Length; i++)
                {
                    DataLibrary.MngAdmin chukuperson = this.Db.MngAdmin.FirstOrDefault(c => c.UserName == chukuTel[i]);
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
                        this.Db.SaveChanges();
                    }
                    else
                    {
                        list_MngAdmin.Add(new DataLibrary.MngAdmin()
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
                    }
                }

                for (var i = 0; i < luruName.Length; i++)
                {
                    DataLibrary.MngAdmin ruluperson = this.Db.MngAdmin.FirstOrDefault(c => c.UserName == ruluTel[i]);
                    if (ruluperson != null)
                    {
                        // 如果原有用户没有出库操作员角色，则多赋予出库操作员角色权限
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

                        ruluperson.RealName = luruName[i];
                        qualityentryclerkids.Add(ruluperson.Id);
                        this.Db.SaveChanges();
                    }
                    else
                    {
                        list_MngAdmin.Add(new DataLibrary.MngAdmin()
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
                    }
                }

                foreach (var li in list_MngAdmin)
                {
                    this.Db.MngAdmin.Add(li);
                    this.Db.SaveChanges();
                }

                foreach (var li in list_MngAdmin.Where(p => p.GroupManage.Object.ToList().Contains((int)GroupManage.入库操作员)))
                {
                    warehousingoperatorids.Add(li.Id);
                }

                foreach (var li2 in list_MngAdmin.Where(p => p.GroupManage.Object.ToList().Contains((int)GroupManage.出库操作员)))
                {
                    outboundoperatorids.Add(li2.Id);
                }

                foreach (var li3 in list_MngAdmin.Where(p => p.GroupManage.Object.ToList().Contains((int)GroupManage.质量员)))
                {
                    qualityentryclerkids.Add(li3.Id);
                }

                var wsclass = new DataLibrary.PdWorkshop()
                {
                    Name = workShopName,
                    Code = code.ToUpper(),
                    Inputer = string.Join(",", warehousingoperatorids),
                    Outputer = string.Join(",", outboundoperatorids),
                    QAInputer = string.Join(",", qualityentryclerkids)
                };
                this.Db.PdWorkshop.Add(wsclass);
                this.Db.SaveChanges();
                this.IsSuccess = true;
            }
        }
    }
}