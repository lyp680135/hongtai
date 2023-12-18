namespace Common.Service
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using Common.IService;
    using DataLibrary;
    // using ImageSharp;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using SixLabors.ImageSharp;
    using Util;
    using static DataLibrary.EnumList;

    /// <summary>
    /// 质量证明书手动输入版
    /// </summary>
    public class CertNewService : ICertNewService
    {
        private DataLibrary.DataContext db;
        private IQualityService qualityService;
        private IStockOutService stockoutService;

        public CertNewService(DataLibrary.DataContext dataContext, IQualityService quality, IStockOutService stockout)
        {
            this.db = dataContext;
            this.qualityService = quality;
            this.stockoutService = stockout;
        }

        public CommonResult AddCert(JArray list, int sellerid, string lpn, string consignor, int userid, DateTime outDate, string purchaseno = "")
        {
            if (list == null || list.Count == 0)
            {
                return new CommonResult(CommonResultStatus.Failed, "打印记录保存失败", "产品物资不能为空！");
            }

            try
            {
                int materialid = 0; // 材质ID

                var mngsetInfo = this.db.MngSetting.FirstOrDefault();

                foreach (var item in list)
                {
                    materialid = item["Materialid"].ToInt();
                    string batCode = item["Batcode"].SafeString().Trim();

                    // 判断系统是否有预置数据
                    var presetdata = this.db.PdQuality.Where(p => p.MaterialId == materialid && p.CreateFlag == 1).Count();
                    if (presetdata > 0)
                    {
                        // 判断该炉号以前是否打过 质保书
                        var quality = this.db.PdQualityProductPreset.Where(p => p.Batcode == batCode && p.Materialid == materialid).FirstOrDefault();
                        if (quality == null)
                        {
                            // 临时给这个批号匹配预置数据 , 先获取上次使用的最后一条预制数据
                            var prev = this.db.PdQualityProductPreset.Where(c => c.Materialid == materialid).OrderByDescending(c => c.Id).FirstOrDefault();

                            var qid = prev != null ? prev.Qid : 0;
                            if (qid > 0)
                            {
                                // 取下一条
                                var nextQuality = this.db.PdQuality.Where(p => p.MaterialId == materialid && p.CreateFlag == 1 && p.Id > qid).OrderBy(c => c.Id).FirstOrDefault();
                                if (nextQuality != null)
                                {
                                    this.db.PdQualityProductPreset.Add(new PdQualityProductPreset()
                                    {
                                        Batcode = batCode,
                                        Createtime = DateTime.Now.GetUnixTimeFromDateTime().ToInt(),
                                        Materialid = materialid,
                                        Qid = nextQuality.Id
                                    });
                                    this.db.SaveChanges();
                                }
                                else
                                {
                                    this.AddFirstPdQuality(materialid, batCode);
                                }
                            }
                            else
                            {
                                this.AddFirstPdQuality(materialid, batCode);
                            }
                        }
                    }
                    else
                    {
                        return new CommonResult(CommonResultStatus.Failed, "出库失败了，所选批号[" + item["Batcode"].ToString() + "]找不到质量检测数据。", string.Empty);
                    }
                }

                string pno = this.db.SalePrintlogNew.Max(x => x.Printno);
                if (pno == null || string.IsNullOrEmpty(pno))
                {
                    pno = "1000001";
                }
                else
                {
                    int no = 0;
                    int.TryParse(pno, out no);
                    if (no < 1000000)
                    {
                        int realno = 1000000 + no;
                        realno += 1;
                        pno = string.Format("{0}", realno);
                    }
                    else
                    {
                        pno = string.Format("{0}", no + 1);
                    }
                }

                // 生成随机校验码
                string checkcode = this.GenerateRandomCode();

                int time = (int)outDate.GetUnixTimeFromDateTime();
                SalePrintlogNew entity = new SalePrintlogNew
                {
                    Createtime = time,
                    Status = 0,
                    MaterialId = materialid,
                    Consignor = consignor,
                    Printno = pno,
                    Lpn = lpn,
                    Signetangle = new Random().Next(-45, 45),
                    Checkcode = "n" + checkcode, // 手动输入版的 质保书验证码，前面加n
                    Adder = userid,
                    Purchaseno = purchaseno,
                };

                this.db.SalePrintlogNew.Add(entity);

                if (this.db.SaveChanges() > 0)
                {
                    foreach (var item in list)
                    {
                        this.db.SalePrintLogDetailNew.Add(new SalePrintLogDetailNew()
                        {
                            BatCode = item["Batcode"].SafeString().Trim(),
                            PrintId = entity.Id,
                            Printnumber = item["Printnumber"].ToInt(),
                            SingleWeight = (float)item["SingleWeight"].ToDouble(),
                            Spec = item["Spec"].SafeString(),
                            Length = item["Length"].ToInt(),
                            SellerId = sellerid,
                            MaterialId = materialid
                        });
                    }

                    this.db.SaveChanges();

                    return new CommonResult(CommonResultStatus.Success, "打印记录保存成功", null, pno);
                }
                else
                {
                    return new CommonResult(CommonResultStatus.Failed, "打印记录保存失败", null, pno);
                }
            }
            catch (Exception ex)
            {
                return new CommonResult(CommonResultStatus.Failed, "打印记录保存失败", ex.Message);
            }
        }


        /// <summary>
        /// 取真实的质量数据
        /// </summary>
        /// <param name="list"></param>
        /// <param name="sellerid"></param>
        /// <param name="lpn"></param>
        /// <param name="consignor"></param>
        /// <param name="userid"></param>
        /// <param name="outDate"></param>
        /// <param name="purchaseno"></param>
        /// <returns></returns>
        public CommonResult AddCert2(JArray list, int sellerid, string lpn, string consignor, int userid, DateTime outDate, string purchaseno = "")
        {
            if (list == null || list.Count == 0)
            {
                return new CommonResult(CommonResultStatus.Failed, "打印记录保存失败", "产品物资不能为空！");
            }

            try
            {
                int materialid = 0; // 材质ID

                var mngsetInfo = this.db.MngSetting.FirstOrDefault();

                foreach (var item in list)
                {
                    materialid = item["Materialid"].ToInt();
                    string batCode = item["Batcode"].SafeString().Trim();

                    // 判断系统是否有录入质量数据
                    var data = this.db.PdQuality.Where(p => p.MaterialId == materialid && p.CreateFlag == 0 && p.CheckStatus == CheckStatus_PdQuality.审核通过).Count();
                    if (data <= 0)
                    {
                        return new CommonResult(CommonResultStatus.Failed, "出库失败了，所选批号[" + item["Batcode"].ToString() + "]找不到质量检测数据。", string.Empty);
                    }
 
                }

                string pno = this.db.SalePrintlogNew.Max(x => x.Printno);
                if (pno == null || string.IsNullOrEmpty(pno))
                {
                    pno = "1000001";
                }
                else
                {
                    int no = 0;
                    int.TryParse(pno, out no);
                    if (no < 1000000)
                    {
                        int realno = 1000000 + no;
                        realno += 1;
                        pno = string.Format("{0}", realno);
                    }
                    else
                    {
                        pno = string.Format("{0}", no + 1);
                    }
                }

                // 生成随机校验码
                string checkcode = this.GenerateRandomCode();

                int time = (int)outDate.GetUnixTimeFromDateTime();
                SalePrintlogNew entity = new SalePrintlogNew
                {
                    Createtime = time,
                    Status = 0,
                    MaterialId = materialid,
                    Consignor = consignor,
                    Printno = pno,
                    Lpn = lpn,
                    Signetangle = new Random().Next(-45, 45),
                    Checkcode = "n" + checkcode, // 手动输入版的 质保书验证码，前面加n
                    Adder = userid,
                    Purchaseno = purchaseno,
                };

                this.db.SalePrintlogNew.Add(entity);

                if (this.db.SaveChanges() > 0)
                {
                    foreach (var item in list)
                    {
                        this.db.SalePrintLogDetailNew.Add(new SalePrintLogDetailNew()
                        {
                            BatCode = item["Batcode"].SafeString().Trim(),
                            PrintId = entity.Id,
                            Printnumber = item["Printnumber"].ToInt(),
                            SingleWeight = (float)item["SingleWeight"].ToDouble(),
                            Spec = item["Spec"].SafeString(),
                            Length = item["Length"].ToInt(),
                            SellerId = sellerid,
                            MaterialId = materialid
                        });
                    }

                    this.db.SaveChanges();

                    return new CommonResult(CommonResultStatus.Success, "打印记录保存成功", null, pno);
                }
                else
                {
                    return new CommonResult(CommonResultStatus.Failed, "打印记录保存失败", null, pno);
                }
            }
            catch (Exception ex)
            {
                return new CommonResult(CommonResultStatus.Failed, "打印记录保存失败", ex.Message);
            }
        }


        public CommonResult GetCertData(string printno)
        {
            if (string.IsNullOrEmpty(printno))
            {
                return new CommonResult(CommonResultStatus.Failed, "查询质保书失败", "打印编号为空！");
            }

            // 根据打印编号查出打印的授权产品
            var printFirst = this.db.SalePrintlogNew.FirstOrDefault(x => x.Printno == printno);
            if (printFirst != null)
            {
                var mngsetting = this.db.MngSetting.FirstOrDefault();
                var sitebasic = this.db.SiteBasic.FirstOrDefault();
                JObject info = new JObject();
                if (mngsetting != null)
                {
                    info.Add("Client", mngsetting.Client);          // 企业名称
                    info.Add("ClientEn", mngsetting.ClientEn);     // 企业英文名称
                    info.Add("DomainPc", mngsetting.Domain_PC);    // PC网站域名
                                                                   // 企业LOGO
                    info.Add("Logo", "logo.png");

                    info.Add("TitleIcon", "title.png");

                    // 质量图标
                    info.Add("QualityIcon", "qs.png");

                    // 附加的质量图标（鸿泰特有）
                    info.Add("QualityIcon2", "qs2.png");

                    // 生产许可证号
                    info.Add("PermitNo", "XK05-001-00075");

                    // 地址
                    info.Add("Address", "中国·江苏·镇江");
                    info.Add("AddressEn", "ZhenJiang.JiangSu P.R.C");

                    // 产品品牌小图标
                    info.Add("Brand", "brand.png");
                }

                info.Add("Id", printFirst.Id);
                info.Add("CheckCode", printFirst.Checkcode);

                // 收货单位
                info.Add("DeliveryCompany", printFirst.Consignor);

                // 印章旋转角度
                info.Add("SignetAngle", printFirst.Signetangle);

                // 打印的车牌号
                info.Add("LPN", printFirst.Lpn);

                // 采购单号
                info.Add("Purchaseno", printFirst.Purchaseno);

                // 经销商授权表
                int printId = printFirst.Id;

                var material = this.db.BaseProductMaterial.FirstOrDefault(c => c.Id == printFirst.MaterialId);
                var productClass = this.db.BaseProductClass.FirstOrDefault(c => c.Id == material.Classid);
                var printDetail = this.db.SalePrintLogDetailNew.Where(c => c.PrintId == printFirst.Id).ToList();

                if (printDetail != null && printDetail.Count > 0)
                {
                    // 质量标准
                    var basequality = this.db.BaseQualityStandard.FirstOrDefault(x => x.Classid == productClass.Id && x.Materialid == material.Id);
                    if (basequality != null)
                    {
                        // 执行标准
                        var standard = this.db.BaseGbProduction.FirstOrDefault(x => x.Id == basequality.Standardid);
                        info.Add("Standard", standard.Name);
                    }

                    info.Add("Gbname", productClass.Gbname.Replace("钢筋混泥土用", string.Empty));
                    info.Add("MaterilaName", material.Name);

                    // 交货状态
                    info.Add("DeliveryState", Enum.GetName(typeof(EnumList.DeliveryType), productClass.DeliveryType));

                    // 交货状态英文
                    if (productClass.DeliveryType == (int)EnumList.DeliveryType.直条定尺)
                    {
                        info.Add("DeliveryStateEn", "Fixed length straight strip");
                    }
                    else if (productClass.DeliveryType == (int)EnumList.DeliveryType.盘卷)
                    {
                        info.Add("DeliveryStateEn", "Steel coll");
                    }
                    else
                    {
                        info.Add("DeliveryStateEn", "Unfixed length straight strip");
                    }

                    string checkPerson = string.Empty;
                    string entryPerson = string.Empty;

                    int? qid = this.db.PdQualityProductPreset.FirstOrDefault(c => c.Batcode == printDetail.First().BatCode).Qid;
                    var firstquality = this.db.PdQuality.FirstOrDefault(c => c.Id == qid.Value);
                    if (firstquality != null)
                    {
                        checkPerson = this.db.MngAdmin.FirstOrDefault(c => c.Id == firstquality.CheckPerson).RealName;
                        entryPerson = this.db.MngAdmin.FirstOrDefault(c => c.Id == firstquality.EntryPerson).RealName;
                    }

                    info.Add("CheckPerson", checkPerson);  // 审核人

                    var person = this.db.MngAdmin.Where(m => m.Id == printFirst.Adder).FirstOrDefault();
                    if (person != null)
                    {
                        info.Add("EntryPerson", person.RealName);  // 打印人
                    }
                    else
                    {
                        // 调不到就取预置数据人员信息
                        info.Add("EntryPerson", entryPerson);  // 录入人
                    }

                    // 质保书打印日期可自定义
                    var createtime = (long)printFirst.Createtime.Value;
                    info.Add("OutDate", createtime.GetDateTimeFromUnixTime().ToString("yyyy-MM-dd")); // 出证日期

                    // 合计数据
                    int total = 0;
                    double totalweight = 0;

                    // 组装输出结构休，补全产品信息、规格、长度、质量数据
                    JArray outputarr = new JArray();
                    int index = 0;
                    foreach (var detail in printDetail)
                    {
                        JObject outinfo = new JObject();
                        outinfo["Batcode"] = detail.BatCode;

                        JObject productinfo = new JObject();
                        productinfo["Specname"] = "Ф" + detail.Spec;
                        productinfo["Length"] = detail.Length;
                        productinfo["Piece"] = detail.Printnumber;
                        productinfo["Weight"] = detail.SingleWeight * detail.Printnumber;

                        outinfo["PdProduct"] = productinfo;

                        total += detail.Printnumber.Value;
                        totalweight += detail.SingleWeight.Value * detail.Printnumber.Value;

                        // 获取规格直径
                        int.TryParse(detail.Spec.ToString(), out int diameter);

                        List<Tuple<string, int?>> detaillist = new List<Tuple<string, int?>>();
                        if (printDetail != null)
                        {
                            foreach (var de in printDetail)
                            {
                                detaillist.Add(new Tuple<string, int?>(de.BatCode, printFirst.MaterialId.Value));
                            }
                        }

                        var info_data = this.qualityService.GetQualityData(detaillist,true);
                        if (index < info_data.Count)
                        {
                            var item = info_data[index];

                            bool ok = false;
                            if (printFirst.MaterialId == item.MaterialId)
                            {
                                ok = true;
                            }

                            if (ok)
                            {
                                double si = 0, p = 0, s = 0, nb = 0;
                                double c = 0, mn = 0, cr = 0, v = 0, mo = 0, cu = 0, ni = 0, ti = 0, ceq = 0;

                                // 化学成份相关数据
                                outinfo["Qualityinfo"] = (JObject)item.Qualityinfos.Object;

                                foreach (var d in (JObject)item.Qualityinfos.Object)
                                {
                                    if (d.Key.ToUpper() == "C")
                                    {
                                        double.TryParse(outinfo["Qualityinfo"][d.Key].ToString(), out c);
                                        outinfo["Qualityinfo"][d.Key] = c.ToString("0.00");
                                    }

                                    if (d.Key.ToUpper() == "SI")
                                    {
                                        double.TryParse(outinfo["Qualityinfo"][d.Key].ToString(), out si);
                                        outinfo["Qualityinfo"][d.Key] = si.ToString("0.00");
                                    }

                                    if (d.Key.ToUpper() == "MN")
                                    {
                                        double.TryParse(outinfo["Qualityinfo"][d.Key].ToString(), out mn);
                                        outinfo["Qualityinfo"][d.Key] = mn.ToString("0.00");
                                    }

                                    if (d.Key.ToUpper() == "P")
                                    {
                                        double.TryParse(outinfo["Qualityinfo"][d.Key].ToString(), out p);
                                        outinfo["Qualityinfo"][d.Key] = p.ToString("0.000");
                                    }

                                    if (d.Key.ToUpper() == "S")
                                    {
                                        double.TryParse(outinfo["Qualityinfo"][d.Key].ToString(), out s);
                                        outinfo["Qualityinfo"][d.Key] = s.ToString("0.000");
                                    }

                                    if (d.Key.ToUpper() == "V")
                                    {
                                        double.TryParse(outinfo["Qualityinfo"][d.Key].ToString(), out v);
                                        outinfo["Qualityinfo"][d.Key] = v.ToString("0.000");
                                    }

                                    if (d.Key.ToUpper() == "NB")
                                    {
                                        double.TryParse(outinfo["Qualityinfo"][d.Key].ToString(), out nb);
                                        outinfo["Qualityinfo"][d.Key] = nb.ToString("0.000");
                                    }

                                    if (d.Key.ToUpper() == "CU")
                                    {
                                        double.TryParse(outinfo["Qualityinfo"][d.Key].ToString(), out cu);
                                        outinfo["Qualityinfo"][d.Key] = cu.ToString("0.000");
                                    }

                                    if (d.Key.ToUpper() == "NI")
                                    {
                                        double.TryParse(outinfo["Qualityinfo"][d.Key].ToString(), out ni);
                                        outinfo["Qualityinfo"][d.Key] = ni.ToString("0.000");
                                    }

                                    if (d.Key.ToUpper() == "MO")
                                    {
                                        double.TryParse(outinfo["Qualityinfo"][d.Key].ToString(), out mo);
                                        outinfo["Qualityinfo"][d.Key] = mo.ToString("0.000");
                                    }

                                    if (d.Key.ToUpper() == "CR")
                                    {
                                        double.TryParse(outinfo["Qualityinfo"][d.Key].ToString(), out cr);
                                        outinfo["Qualityinfo"][d.Key] = cr.ToString("0.000");
                                    }

                                    if (d.Key.ToUpper() == "TI")
                                    {
                                        double.TryParse(outinfo["Qualityinfo"][d.Key].ToString(), out ti);
                                        outinfo["Qualityinfo"][d.Key] = ti.ToString("0.000");
                                    }

                                    // Ceq 要读手动输入的
                                    if (d.Key.ToUpper() == "CEQ")
                                    {
                                        double.TryParse(outinfo["Qualityinfo"][d.Key].ToString(), out ceq);
                                    }
                                }

                                if (ceq != 0)
                                {
                                    outinfo["Qualityinfo"]["Ceq"] = ceq.ToString("0.00");
                                }
                                else
                                {
                                    // Ceq = C +  Mn/6  + (Cr + V +Mo) / 5 + (Cu + ni) / 15
                                    outinfo["Qualityinfo"]["Ceq"] = (c + (mn / 6) + ((cr + v + mo) / 5) + ((cu + ni) / 15)).ToString("0.00");
                                }

                                outinfo["Qualityinfo"]["Surface"] = "合格/Pass";
                                outinfo["Qualityinfo"]["Metall"] = "合格/Pass";
                                outinfo["Qualityinfo"]["Vickers"] = "合格/Pass";
                                outinfo["Qualityinfo"]["Microstructures"] = "合格/Pass";

                                // 质量偏差这里，如果是预置质量数据，12规格的偏差多负1%
                                if (outinfo["Qualityinfo"]["重量偏差"] != null)
                                {
                                    double.TryParse(outinfo["Qualityinfo"]["重量偏差"].ToString(), out double weightoffset);
                                    if (diameter <= 12)
                                    {
                                        outinfo["Qualityinfo"]["重量偏差"] = weightoffset - 1;
                                    }
                                }

                                // 重量偏差只保留1位小数点
                                double.TryParse(outinfo["Qualityinfo"]["重量偏差"].ToString(), out double realweightoffset);
                                outinfo["Qualityinfo"]["重量偏差"] = realweightoffset.ToString("0.0");

                                // 合并多行数据为一层数据
                                var dynamicdata = new JObject();
                                dynamicdata["强屈比"] = string.Empty;
                                dynamicdata["屈屈比"] = string.Empty;

                                int pos = 0;
                                var arr = JArray.FromObject(item.Qualityinfos_Dynamics.Object);
                                foreach (JObject arritem in arr)
                                {
                                    foreach (var x in arritem)
                                    {
                                        var find = false;
                                        foreach (var d in dynamicdata)
                                        {
                                            if (d.Key == x.Key)
                                            {
                                                find = true;
                                                if (d.Key != "强屈比" && d.Key != "屈屈比")
                                                {
                                                    if (d.Key == "伸长率Agt")
                                                    {
                                                        double.TryParse(x.Value.ToString(), out double val);
                                                        dynamicdata[d.Key] = dynamicdata[d.Key] + "/" + val.ToString("0.0");
                                                    }
                                                    else
                                                    {
														if (d.Key == "伸长率A")
                                                        {
                                                            double.TryParse(x.Value.ToString(), out double val);
                                                            dynamicdata[d.Key] = dynamicdata[d.Key] + "/" + val.ToString("0");
                                                        }
														else
														{
                                                            dynamicdata[d.Key] = dynamicdata[d.Key] + "/" + x.Value;
														}
                                                    }
                                                }

                                                break;
                                            }
                                        }

                                        if (!find)
                                        {
                                            dynamicdata[x.Key] = x.Value;
                                            if (x.Key == "伸长率Agt")
                                            {
                                                double.TryParse(x.Value.ToString(), out double val);
                                                dynamicdata[x.Key] = val.ToString("0.0");
                                            }

											if (x.Key == "伸长率A")
                                            {
                                                double.TryParse(x.Value.ToString(), out double val);
                                                dynamicdata[x.Key] = val.ToString("0");
                                            }
                                        }
                                    }

                                    double.TryParse(arritem["抗拉强度"].ToString(), out double antistrength);
                                    double.TryParse(arritem["下屈服强度"].ToString(), out double lowstrength);

                                    if (pos > 0)
                                    {
                                        // 计算强屈比、屈屈比
                                        if (!string.IsNullOrEmpty(dynamicdata["强屈比"].ToString()))
                                        {
                                            dynamicdata["强屈比"] = dynamicdata["强屈比"].ToString() + "/";
                                            dynamicdata["屈屈比"] = dynamicdata["屈屈比"].ToString() + "/";
                                        }
                                    }
                                    else
                                    {
                                        dynamicdata["强屈比"] = string.Empty;
                                        dynamicdata["屈屈比"] = string.Empty;
                                    }

                                    if (lowstrength > 0)
                                    {
                                        dynamicdata["强屈比"] = dynamicdata["强屈比"].ToString() + (antistrength / lowstrength).ToString("0.00");
                                        dynamicdata["屈屈比"] = dynamicdata["屈屈比"].ToString() + (lowstrength / material.Standardstrength).ToString("0.00");
                                    }
                                    else
                                    {
                                        dynamicdata["强屈比"] = dynamicdata["强屈比"].ToString() + "－";
                                        dynamicdata["屈屈比"] = dynamicdata["屈屈比"].ToString() + "－";
                                    }

                                    pos++;
                                }

                                // 判断是否是抗震，抗震的才显示屈屈比和强屈比、伸长率Agt
                                if (!material.Name.EndsWith("E"))
                                {
                                    dynamicdata["强屈比"] = string.Empty;
                                    dynamicdata["屈屈比"] = string.Empty;

                                    dynamicdata["伸长率Agt"] = string.Empty;
                                }

                                // 获取类似规格，这里只是用来计算 弯心，有可能不是当前自己的规格
                                var tempspec = this.db.BaseSpecifications.FirstOrDefault(d => d.Materialid == item.MaterialId && (d.Specname == ("Φ" + detail.Spec) || d.Specname == ("Ф" + detail.Spec)));
                                if (tempspec != null && tempspec.Coldratio > 0)
                                {
                                    // 冷弯弯心、反弯弯心  (规格*系数)
                                    dynamicdata["冷弯弯心"] = tempspec.Coldratio * diameter;
                                    dynamicdata["反弯弯心"] = tempspec.Reverseratio * diameter;

                                    // 冷弯、反弯
                                    dynamicdata["冷弯"] = "完好/Pass";
                                    dynamicdata["反弯"] = "完好/Pass";
                                }
                                else
                                {
                                    dynamicdata["冷弯弯心"] = string.Empty;
                                    dynamicdata["反弯弯心"] = string.Empty;

                                    // 冷弯、反弯
                                    dynamicdata["冷弯"] = "-";
                                    dynamicdata["反弯"] = "-";
                                }

                                outinfo["QualityinfoDynamic"] = dynamicdata;
                            }
                        }

                        index++;

                        outputarr.Add(outinfo);
                    }

                    // 行数如果少于五行，则补全、单元格都用 "－" 填充
                    if (outputarr.Count < 5)
                    {
                        for (int i = outputarr.Count; i < 5; i++)
                        {
                            JObject outinfo = new JObject();
                            outinfo["Batcode"] = "－";

                            JObject productinfo = new JObject();
                            productinfo["Specname"] = "－";
                            productinfo["Length"] = "－";
                            productinfo["Piece"] = "－";
                            productinfo["Weight"] = "－";

                            outinfo["PdProduct"] = productinfo;

                            outinfo["Qualityinfo"] = null;
                            outinfo["QualityinfoDynamic"] = null;

                            outputarr.Add(outinfo);
                        }
                    }

                    info.Add("Total", string.Format("{0}件，{1}Kg", total, totalweight));

                    info.Add("List", JsonConvert.SerializeObject(outputarr));
                }

                // 物资产品化学成份、力学指标
                return new CommonResult(CommonResultStatus.Success, "查询成功", null, JsonConvert.SerializeObject(info));
            }

            return new CommonResult(CommonResultStatus.Failed, "未找到相关记录", string.Empty);
        }

        public CommonResult GetCertData2(string printno)
        {
            if (string.IsNullOrEmpty(printno))
            {
                return new CommonResult(CommonResultStatus.Failed, "查询质保书失败", "打印编号为空！");
            }

            // 根据打印编号查出打印的授权产品
            var printFirst = this.db.SalePrintlogNew.FirstOrDefault(x => x.Printno == printno);
            if (printFirst != null)
            {
                var mngsetting = this.db.MngSetting.FirstOrDefault();
                var sitebasic = this.db.SiteBasic.FirstOrDefault();
                JObject info = new JObject();
                if (mngsetting != null)
                {
                    info.Add("Client", mngsetting.Client);          // 企业名称
                    info.Add("ClientEn", mngsetting.ClientEn);     // 企业英文名称
                    info.Add("DomainPc", mngsetting.Domain_PC);    // PC网站域名
                                                                   // 企业LOGO
                    info.Add("Logo", "logo.png");

                    info.Add("TitleIcon", "title.png");

                    // 质量图标
                    info.Add("QualityIcon", "qs.png");

                    // 附加的质量图标（鸿泰特有）
                    info.Add("QualityIcon2", "qs2.png");

                    // 生产许可证号
                    info.Add("PermitNo", "XK05-001-00075");

                    // 地址
                    info.Add("Address", "中国·江苏·镇江");
                    info.Add("AddressEn", "ZhenJiang.JiangSu P.R.C");

                    // 产品品牌小图标
                    info.Add("Brand", "brand.png");
                }

                info.Add("Id", printFirst.Id);
                info.Add("CheckCode", printFirst.Checkcode);

                // 收货单位
                info.Add("DeliveryCompany", printFirst.Consignor);

                // 印章旋转角度
                info.Add("SignetAngle", printFirst.Signetangle);

                // 打印的车牌号
                info.Add("LPN", printFirst.Lpn);

                // 采购单号
                info.Add("Purchaseno", printFirst.Purchaseno);

                // 经销商授权表
                int printId = printFirst.Id;

                var material = this.db.BaseProductMaterial.FirstOrDefault(c => c.Id == printFirst.MaterialId);
                var productClass = this.db.BaseProductClass.FirstOrDefault(c => c.Id == material.Classid);
                var printDetail = this.db.SalePrintLogDetailNew.Where(c => c.PrintId == printFirst.Id).ToList();

                if (printDetail != null && printDetail.Count > 0)
                {
                    // 质量标准
                    var basequality = this.db.BaseQualityStandard.FirstOrDefault(x => x.Classid == productClass.Id && x.Materialid == material.Id);
                    if (basequality != null)
                    {
                        // 执行标准
                        var standard = this.db.BaseGbProduction.FirstOrDefault(x => x.Id == basequality.Standardid);
                        info.Add("Standard", standard.Name);
                    }

                    info.Add("Gbname", productClass.Gbname.Replace("钢筋混泥土用", string.Empty));
                    info.Add("MaterilaName", material.Name);

                    // 交货状态
                    info.Add("DeliveryState", Enum.GetName(typeof(EnumList.DeliveryType), productClass.DeliveryType));

                    // 交货状态英文
                    if (productClass.DeliveryType == (int)EnumList.DeliveryType.直条定尺)
                    {
                        info.Add("DeliveryStateEn", "Fixed length straight strip");
                    }
                    else if (productClass.DeliveryType == (int)EnumList.DeliveryType.盘卷)
                    {
                        info.Add("DeliveryStateEn", "Steel coll");
                    }
                    else
                    {
                        info.Add("DeliveryStateEn", "Unfixed length straight strip");
                    }

                    // 质量审核
                    var pdquality = this.db.PdQuality.FirstOrDefault(x => x.Batcode == printDetail.FirstOrDefault().BatCode);
                    if (pdquality != null)
                    {
                        string checkPerson = string.Empty;
                        if (pdquality.CheckPerson.HasValue)
                        {
                            checkPerson = this.db.MngAdmin.FirstOrDefault(c => c.Id == pdquality.CheckPerson).RealName;
                        }

                        string entryPerson = string.Empty;
                        if (printFirst.Adder.HasValue && printFirst.Adder.Value > 0)
                        {
                            entryPerson = this.db.MngAdmin.FirstOrDefault(c => c.Id == printFirst.Adder).RealName;
                        }
                        else
                        {
                            entryPerson = this.db.MngAdmin.FirstOrDefault(c => c.Id == pdquality.EntryPerson).RealName;
                        }

                        info.Add("CheckPerson", checkPerson);  // 审核人
                        info.Add("EntryPerson", entryPerson);  // 录入人
                    }

                    // 质保书打印日期可自定义
                    var createtime = (long)printFirst.Createtime.Value;
                    info.Add("OutDate", createtime.GetDateTimeFromUnixTime().ToString("yyyy-MM-dd")); // 出证日期

                    // 合计数据
                    int total = 0;
                    double totalweight = 0;

                    // 组装输出结构休，补全产品信息、规格、长度、质量数据
                    JArray outputarr = new JArray();
                    int index = 0;
                    foreach (var detail in printDetail)
                    {
                        JObject outinfo = new JObject();
                        outinfo["Batcode"] = detail.BatCode;

                        JObject productinfo = new JObject();
                        productinfo["Specname"] = "Ф" + detail.Spec;
                        productinfo["Length"] = detail.Length;
                        productinfo["Piece"] = detail.Printnumber;
                        productinfo["Weight"] = detail.SingleWeight * detail.Printnumber;

                        outinfo["PdProduct"] = productinfo;

                        total += detail.Printnumber.Value;
                        totalweight += detail.SingleWeight.Value * detail.Printnumber.Value;

                        // 获取规格直径
                        int.TryParse(detail.Spec.ToString(), out int diameter);

                        List<Tuple<string, int?>> detaillist = new List<Tuple<string, int?>>();
                        if (printDetail != null)
                        {
                            foreach (var de in printDetail)
                            {
                                detaillist.Add(new Tuple<string, int?>(de.BatCode, printFirst.MaterialId.Value));
                            }
                        }

                        var info_data = this.qualityService.GetQualityData(detaillist);
                        if (index < info_data.Count)
                        {
                            var item = info_data[index];

                            bool ok = false;
                            if (printFirst.MaterialId == item.MaterialId)
                            {
                                ok = true;
                            }

                            if (ok)
                            {
                                double si = 0, p = 0, s = 0, nb = 0;
                                double c = 0, mn = 0, cr = 0, v = 0, mo = 0, cu = 0, ni = 0, ti = 0, ceq = 0;

                                // 化学成份相关数据
                                outinfo["Qualityinfo"] = (JObject)item.Qualityinfos.Object;

                                foreach (var d in (JObject)item.Qualityinfos.Object)
                                {
                                    if (d.Key.ToUpper() == "C")
                                    {
                                        double.TryParse(outinfo["Qualityinfo"][d.Key].ToString(), out c);
                                        outinfo["Qualityinfo"][d.Key] = c.ToString("0.00");
                                    }

                                    if (d.Key.ToUpper() == "SI")
                                    {
                                        double.TryParse(outinfo["Qualityinfo"][d.Key].ToString(), out si);
                                        outinfo["Qualityinfo"][d.Key] = si.ToString("0.00");
                                    }

                                    if (d.Key.ToUpper() == "MN")
                                    {
                                        double.TryParse(outinfo["Qualityinfo"][d.Key].ToString(), out mn);
                                        outinfo["Qualityinfo"][d.Key] = mn.ToString("0.00");
                                    }

                                    if (d.Key.ToUpper() == "P")
                                    {
                                        double.TryParse(outinfo["Qualityinfo"][d.Key].ToString(), out p);
                                        outinfo["Qualityinfo"][d.Key] = p.ToString("0.000");
                                    }

                                    if (d.Key.ToUpper() == "S")
                                    {
                                        double.TryParse(outinfo["Qualityinfo"][d.Key].ToString(), out s);
                                        outinfo["Qualityinfo"][d.Key] = s.ToString("0.000");
                                    }

                                    if (d.Key.ToUpper() == "V")
                                    {
                                        double.TryParse(outinfo["Qualityinfo"][d.Key].ToString(), out v);
                                        outinfo["Qualityinfo"][d.Key] = v.ToString("0.000");
                                    }

                                    if (d.Key.ToUpper() == "NB")
                                    {
                                        double.TryParse(outinfo["Qualityinfo"][d.Key].ToString(), out nb);
                                        outinfo["Qualityinfo"][d.Key] = nb.ToString("0.000");
                                    }

                                    if (d.Key.ToUpper() == "CU")
                                    {
                                        double.TryParse(outinfo["Qualityinfo"][d.Key].ToString(), out cu);
                                        outinfo["Qualityinfo"][d.Key] = cu.ToString("0.000");
                                    }

                                    if (d.Key.ToUpper() == "NI")
                                    {
                                        double.TryParse(outinfo["Qualityinfo"][d.Key].ToString(), out ni);
                                        outinfo["Qualityinfo"][d.Key] = ni.ToString("0.000");
                                    }

                                    if (d.Key.ToUpper() == "MO")
                                    {
                                        double.TryParse(outinfo["Qualityinfo"][d.Key].ToString(), out mo);
                                        outinfo["Qualityinfo"][d.Key] = mo.ToString("0.000");
                                    }

                                    if (d.Key.ToUpper() == "CR")
                                    {
                                        double.TryParse(outinfo["Qualityinfo"][d.Key].ToString(), out cr);
                                        outinfo["Qualityinfo"][d.Key] = cr.ToString("0.000");
                                    }

                                    if (d.Key.ToUpper() == "TI")
                                    {
                                        double.TryParse(outinfo["Qualityinfo"][d.Key].ToString(), out ti);
                                        outinfo["Qualityinfo"][d.Key] = ti.ToString("0.000");
                                    }

                                    // Ceq 要读手动输入的
                                    if (d.Key.ToUpper() == "CEQ")
                                    {
                                        double.TryParse(outinfo["Qualityinfo"][d.Key].ToString(), out ceq);
                                    }
                                }

                                if (ceq != 0)
                                {
                                    outinfo["Qualityinfo"]["Ceq"] = ceq.ToString("0.00");
                                }
                                else
                                {
                                    // Ceq = C +  Mn/6  + (Cr + V +Mo) / 5 + (Cu + ni) / 15
                                    outinfo["Qualityinfo"]["Ceq"] = (c + (mn / 6) + ((cr + v + mo) / 5) + ((cu + ni) / 15)).ToString("0.00");
                                }

                                outinfo["Qualityinfo"]["Surface"] = "合格/Pass";
                                outinfo["Qualityinfo"]["Metall"] = "合格/Pass";
                                outinfo["Qualityinfo"]["Vickers"] = "合格/Pass";
                                outinfo["Qualityinfo"]["Microstructures"] = "合格/Pass";

                                // 质量偏差这里，如果是预置质量数据，12规格的偏差多负1%
                                if (outinfo["Qualityinfo"]["重量偏差"] != null)
                                {
                                    double.TryParse(outinfo["Qualityinfo"]["重量偏差"].ToString(), out double weightoffset);
                                    if (diameter <= 12)
                                    {
                                        outinfo["Qualityinfo"]["重量偏差"] = weightoffset - 1;
                                    }
                                }

                                // 重量偏差只保留1位小数点
                                double.TryParse(outinfo["Qualityinfo"]["重量偏差"].ToString(), out double realweightoffset);
                                outinfo["Qualityinfo"]["重量偏差"] = realweightoffset.ToString("0.0");

                                // 合并多行数据为一层数据
                                var dynamicdata = new JObject();
                                dynamicdata["强屈比"] = string.Empty;
                                dynamicdata["屈屈比"] = string.Empty;

                                int pos = 0;
                                var arr = JArray.FromObject(item.Qualityinfos_Dynamics.Object);
                                foreach (JObject arritem in arr)
                                {
                                    foreach (var x in arritem)
                                    {
                                        var find = false;
                                        foreach (var d in dynamicdata)
                                        {
                                            if (d.Key == x.Key)
                                            {
                                                find = true;
                                                if (d.Key != "强屈比" && d.Key != "屈屈比")
                                                {
                                                    if (d.Key == "伸长率Agt")
                                                    {
                                                        double.TryParse(x.Value.ToString(), out double val);
                                                        dynamicdata[d.Key] = dynamicdata[d.Key] + "/" + val.ToString("0.0");
                                                    }
                                                    else
                                                    {
                                                        if (d.Key == "伸长率A")
                                                        {
                                                            double.TryParse(x.Value.ToString(), out double val);
                                                            dynamicdata[d.Key] = dynamicdata[d.Key] + "/" + val.ToString("0");
                                                        }
                                                        else
                                                        {
                                                            dynamicdata[d.Key] = dynamicdata[d.Key] + "/" + x.Value;
                                                        }
                                                    }
                                                }

                                                break;
                                            }
                                        }

                                        if (!find)
                                        {
                                            dynamicdata[x.Key] = x.Value;
                                            if (x.Key == "伸长率Agt")
                                            {
                                                double.TryParse(x.Value.ToString(), out double val);
                                                dynamicdata[x.Key] = val.ToString("0.0");
                                            }

                                            if (x.Key == "伸长率A")
                                            {
                                                double.TryParse(x.Value.ToString(), out double val);
                                                dynamicdata[x.Key] = val.ToString("0");
                                            }
                                        }
                                    }

                                    double.TryParse(arritem["抗拉强度"].ToString(), out double antistrength);
                                    double.TryParse(arritem["下屈服强度"].ToString(), out double lowstrength);

                                    if (pos > 0)
                                    {
                                        // 计算强屈比、屈屈比
                                        if (!string.IsNullOrEmpty(dynamicdata["强屈比"].ToString()))
                                        {
                                            dynamicdata["强屈比"] = dynamicdata["强屈比"].ToString() + "/";
                                            dynamicdata["屈屈比"] = dynamicdata["屈屈比"].ToString() + "/";
                                        }
                                    }
                                    else
                                    {
                                        dynamicdata["强屈比"] = string.Empty;
                                        dynamicdata["屈屈比"] = string.Empty;
                                    }

                                    if (lowstrength > 0)
                                    {
                                        dynamicdata["强屈比"] = dynamicdata["强屈比"].ToString() + (antistrength / lowstrength).ToString("0.00");
                                        dynamicdata["屈屈比"] = dynamicdata["屈屈比"].ToString() + (lowstrength / material.Standardstrength).ToString("0.00");
                                    }
                                    else
                                    {
                                        dynamicdata["强屈比"] = dynamicdata["强屈比"].ToString() + "－";
                                        dynamicdata["屈屈比"] = dynamicdata["屈屈比"].ToString() + "－";
                                    }

                                    pos++;
                                }

                                // 判断是否是抗震，抗震的才显示屈屈比和强屈比、伸长率Agt
                                if (!material.Name.EndsWith("E"))
                                {
                                    dynamicdata["强屈比"] = string.Empty;
                                    dynamicdata["屈屈比"] = string.Empty;

                                    dynamicdata["伸长率Agt"] = string.Empty;
                                }

                                // 获取类似规格，这里只是用来计算 弯心，有可能不是当前自己的规格
                                var tempspec = this.db.BaseSpecifications.FirstOrDefault(d => d.Materialid == item.MaterialId && (d.Specname == ("Φ" + detail.Spec) || d.Specname == ("Ф" + detail.Spec)));
                                if (tempspec != null && tempspec.Coldratio > 0)
                                {
                                    // 冷弯弯心、反弯弯心  (规格*系数)
                                    dynamicdata["冷弯弯心"] = tempspec.Coldratio * diameter;
                                    dynamicdata["反弯弯心"] = tempspec.Reverseratio * diameter;

                                    // 冷弯、反弯
                                    dynamicdata["冷弯"] = "完好/Pass";
                                    dynamicdata["反弯"] = "完好/Pass";
                                }
                                else
                                {
                                    dynamicdata["冷弯弯心"] = string.Empty;
                                    dynamicdata["反弯弯心"] = string.Empty;

                                    // 冷弯、反弯
                                    dynamicdata["冷弯"] = "-";
                                    dynamicdata["反弯"] = "-";
                                }

                                outinfo["QualityinfoDynamic"] = dynamicdata;
                            }
                        }

                        index++;

                        outputarr.Add(outinfo);
                    }

                    // 行数如果少于五行，则补全、单元格都用 "－" 填充
                    if (outputarr.Count < 5)
                    {
                        for (int i = outputarr.Count; i < 5; i++)
                        {
                            JObject outinfo = new JObject();
                            outinfo["Batcode"] = "－";

                            JObject productinfo = new JObject();
                            productinfo["Specname"] = "－";
                            productinfo["Length"] = "－";
                            productinfo["Piece"] = "－";
                            productinfo["Weight"] = "－";

                            outinfo["PdProduct"] = productinfo;

                            outinfo["Qualityinfo"] = null;
                            outinfo["QualityinfoDynamic"] = null;

                            outputarr.Add(outinfo);
                        }
                    }

                    info.Add("Total", string.Format("{0}件，{1}Kg", total, totalweight));

                    info.Add("List", JsonConvert.SerializeObject(outputarr));
                }

                // 物资产品化学成份、力学指标
                return new CommonResult(CommonResultStatus.Success, "查询成功", null, JsonConvert.SerializeObject(info));
            }

            return new CommonResult(CommonResultStatus.Failed, "未找到相关记录", string.Empty);
        }

        /// <summary>
        /// 生成证书，质量数据从真实数据获取
        /// </summary>
        /// <param name="printno"></param>
        /// <param name="savepath"></param>
        /// <param name="iswater"></param>
        /// <returns></returns>
        public CommonResult GenerateCert2(string printno, string savepath, bool iswater)
        {
            CommonResult result = this.GetCertData2(printno);
            if (result.Status == (int)CommonResultStatus.Success)
            {
                JObject jobj = (JObject)JsonConvert.DeserializeObject(result.Data.ToString());

                if (jobj != null)
                {
                    JObject certparams = new JObject();

                    // 从本地文件中找到模板
                    string path = savepath;

                    var settings = this.db.MngSetting.FirstOrDefault();

                    var printlog = this.db.SalePrintlogNew.Where(s => s.Printno == printno).FirstOrDefault();
                    if (printlog != null)
                    {
                        int materialid = printlog.MaterialId.Value;

                        path = savepath + "/template/template" + materialid + ".html";

                        // 准备生成质保书相关参数
                        certparams["printno"] = printno;

                        // 1.二维码扫码后链接地址
                        certparams["qrcodeurl"] = settings.Domain_QRCode;
                        certparams["qrcodeurl"] = string.Format(settings.Domain_QRCode, printlog.Id, printlog.Checkcode);

                        // 2.质保书印章的角度
                        certparams["rotate"] = printlog.Signetangle;

                        // 3.质保书印章的角度
                        certparams["rotate"] = printlog.Signetangle;

                        // 4.质保书的高宽
                        certparams["width"] = 1583;
                        certparams["height"] = 1125;

                        // 5.质保书上的图片文件所在域名
                        certparams["filedomain"] = settings.ImgPath;

                        // 6.是否打上水印
                        certparams["iswater"] = iswater ? 1 : 0;

                        // 7.二维码生成的缩放比
                        certparams["scale"] = 4;
                    }

                    FileStream fs = new FileStream(path, FileMode.Open);
                    byte[] template_buffer = new byte[fs.Length];
                    fs.Read(template_buffer, 0, template_buffer.Length);
                    fs.Close();

                    string templatestring = Encoding.UTF8.GetString(template_buffer);

                    // 获取本地模板
                    StringBuilder html = new StringBuilder(templatestring);

                    string template_image_path = settings.Domain_WebApi;
                    if (settings.Domain_WebApi.EndsWith("/"))
                    {
                        template_image_path = template_image_path.Remove(template_image_path.Length - 1);
                    }

                    // 头部
                    html = html.Replace("{{data.Client}}", jobj["Client"].ToString());
                    html = html.Replace("{{data.ClientEn}}", jobj["ClientEn"].ToString());
                    html = html.Replace("{{data.Logo}}", template_image_path + "/qualitypics/template/images/" + jobj["Logo"].ToString());
                    html = html.Replace("{{data.TitleIcon}}", template_image_path + "/qualitypics/template/images/" + jobj["TitleIcon"].ToString());
                    html = html.Replace("{{data.QualityIcon}}", template_image_path + "/qualitypics/template/images/" + jobj["QualityIcon"].ToString());

                    // 附加的质量图标（鸿泰特有）
                    html = html.Replace("{{data.QualityIcon2}}", template_image_path + "/qualitypics/template/images/" + jobj["QualityIcon2"].ToString());

                    html = html.Replace("{{data.Brand}}", template_image_path + "/qualitypics/template/images/" + jobj["Brand"].ToString());
                    html = html.Replace("{{data.PrintNo}}", printno);
                    html = html.Replace("{{data.DeliveryCompany}}", jobj["DeliveryCompany"].ToString());
                    html = html.Replace("{{data.Gbname}}", jobj["Gbname"].ToString());
                    html = html.Replace("{{data.Standard}}", jobj["Standard"].ToString());
                    html = html.Replace("{{data.PermitNo}}", jobj["PermitNo"].ToString());
                    html = html.Replace("{{data.MaterilaName}}", jobj["MaterilaName"].ToString());
                    html = html.Replace("{{data.DeliveryState}}", jobj["DeliveryState"].ToString());
                    html = html.Replace("{{data.DeliveryStateEn}}", jobj["DeliveryStateEn"].ToString());
                    html = html.Replace("{{data.Address}}", jobj["Address"].ToString());
                    html = html.Replace("{{data.AddressEn}}", jobj["AddressEn"].ToString());

                    // 车牌号
                    html = html.Replace("{{data.LPN}}", jobj["LPN"].ToString());

                    // 采购单号
                    html = html.Replace("{{data.Purchaseno}}", jobj["Purchaseno"].ToString());


                    // 底部
                    html = html.Replace("{{data.OutDate}}", jobj["OutDate"].ToString());
                    html = html.Replace("{{data.EntryPerson}}", jobj["EntryPerson"].ToString());
                    html = html.Replace("{{data.CheckPerson}}", jobj["CheckPerson"].ToString());
                    html = html.Replace("{{data.DomainPc}}", jobj["DomainPc"].ToString());
                    html = html.Replace("{{data.Total}}", jobj["Total"].ToString());

                    Regex regex = new Regex(@"\{\{foreach\(([^\}]*)\)\}\}([\s\S]*?)\{\{/foreach\}\}", RegexOptions.IgnoreCase);
                    var match = regex.Match(html.ToString());
                    while (match.Success)
                    {
                        string matchcontent = match.Groups[0].ToString();

                        string item_name = string.Empty, item_index = string.Empty, list_name = string.Empty;

                        // 头部
                        var header = match.Groups[1].ToString();
                        if (!string.IsNullOrEmpty(header))
                        {
                            // 从头部获取遍历体，以及遍历的实体、索引
                            Regex headregex = new Regex(@"\(([a-zA-Z]*?),([a-zA-Z]*?)\)[\s]*?in[\s]*?data\.([a-zA-Z]+)$", RegexOptions.IgnoreCase);
                            var headmatch = headregex.Match(header);
                            if (headmatch.Success)
                            {
                                item_name = headmatch.Groups[1].ToString();
                                item_index = headmatch.Groups[2].ToString();
                                list_name = headmatch.Groups[3].ToString();
                            }
                        }

                        var body = match.Groups[2].ToString();

                        StringBuilder sb = new StringBuilder();
                        if (jobj[list_name] != null)
                        {
                            var datalist = (JArray)JsonConvert.DeserializeObject(jobj[list_name].ToString());
                            for (var i = 0; i < datalist.Count; i++)
                            {
                                var item = datalist[i];

                                var newbody = body;

                                Regex bodyregex = new Regex(@"\{\{[a-zA-Z+\d]*(\.[^\.]*)?(\.[^\.\}]*)?\}\}", RegexOptions.IgnoreCase);
                                var bodymatch = bodyregex.Match(newbody);
                                while (bodymatch.Success)
                                {
                                    var bodymatchcontent = bodymatch.Groups[0].ToString();

                                    string firstprop = bodymatch.Groups[1].ToString().Replace(".", string.Empty);
                                    string secondprop = bodymatch.Groups[2].ToString().Replace(".", string.Empty);

                                    // 直接当成表达式来转换，如（{{index+1}}）
                                    if (string.IsNullOrEmpty(firstprop) && string.IsNullOrEmpty(secondprop))
                                    {
                                        var mstr = bodymatch.Groups[0].ToString();

                                        // 支持简单的+、－运算
                                        Regex mregex = new Regex(@"\{\{([a-zA-Z]*)[\s*]?([\+-])?[\s*]?([\d]*)?\}\}");
                                        var mmatch = mregex.Match(mstr);
                                        if (mmatch.Success)
                                        {
                                            var varitem = mmatch.Groups[1].ToString();
                                            var varoperator = mmatch.Groups[2].ToString();
                                            var varnumber = mmatch.Groups[3].ToString();

                                            if (!string.IsNullOrEmpty(varoperator) && !string.IsNullOrEmpty(varnumber))
                                            {
                                                int.TryParse(varnumber, out int number);

                                                string replacestr = bodymatchcontent;

                                                if (varoperator == "+")
                                                {
                                                    if (varitem == item_index)
                                                    {
                                                        replacestr = string.Format("{0}", i + number);
                                                    }
                                                    else if (varitem == item_name)
                                                    {
                                                        // 暂不支持属性运算
                                                    }

                                                    newbody = newbody.Replace(bodymatchcontent, replacestr);
                                                }
                                            }
                                            else
                                            {
                                                string replacestr = string.Empty;

                                                if (varitem == item_index)
                                                {
                                                    replacestr = string.Format("{0}", i);
                                                }
                                                else if (varitem == item_name)
                                                {
                                                    replacestr = string.Format("{0}", item.ToString());
                                                }

                                                newbody = newbody.Replace(bodymatchcontent, replacestr);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // 判断是否是遍历体开头
                                        if (bodymatchcontent.StartsWith("{{" + item_name + "."))
                                        {
                                            // 获取一层属性名
                                            JToken newcontent = item[firstprop];

                                            if (!string.IsNullOrEmpty(secondprop))
                                            {
                                                if (newcontent != null && !string.IsNullOrEmpty(newcontent.ToString()))
                                                {
                                                    newcontent = newcontent[secondprop];
                                                    if (!string.IsNullOrWhiteSpace(string.Format("{0}", newcontent)))
                                                    {
                                                        if (Regex.IsMatch(string.Format("{0}", newcontent), "^([0-9]{1,})$")
                                                            || Regex.IsMatch(string.Format("{0}", newcontent), "^([0-9]{1,}[.][0-9]*)$"))
                                                        {
                                                            if (secondprop != "C"
                                                                && secondprop != "Si"
                                                                && secondprop != "Mn"
                                                                && secondprop != "P"
                                                                && secondprop != "S")
                                                            {
                                                                double.TryParse(string.Format("{0}", newcontent), out double val);
                                                                if (val <= 0)
                                                                {
                                                                    newcontent = "－";
                                                                }
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        newcontent = "－";
                                                    }
                                                }
                                                else
                                                {
                                                    newcontent = "－";
                                                }
                                            }

                                            newbody = newbody.Replace(bodymatchcontent, string.Format("{0}", newcontent));
                                        }
                                    }

                                    bodymatch = bodymatch.NextMatch();
                                }

                                sb.Append(newbody);
                            }

                            html = html.Replace(matchcontent, sb.ToString());
                        }

                        match = match.NextMatch();
                    }

                    try
                    {
                        string imgpath = this.HtmlToBitmap(html.ToString(), savepath, certparams);

                        var retobj = new JObject();
                        retobj["printno"] = printno;
                        retobj["printid"] = printlog.Id;
                        retobj["printcode"] = printlog.Checkcode;

                        return new CommonResult(CommonResultStatus.Success, "生成成功", null, retobj);
                    }
                    catch (Exception e)
                    {
                        return new CommonResult(CommonResultStatus.Success, "生成失败", e.Message);
                    }
                }
            }

            return new CommonResult(CommonResultStatus.Failed, "生成失败", result.Reason);
        }

        public CommonResult GenerateCert(string printno, string savepath, bool iswater)
        {
            CommonResult result = this.GetCertData(printno);
            if (result.Status == (int)CommonResultStatus.Success)
            {
                JObject jobj = (JObject)JsonConvert.DeserializeObject(result.Data.ToString());

                if (jobj != null)
                {
                    JObject certparams = new JObject();

                    // 从本地文件中找到模板
                    string path = savepath;

                    var settings = this.db.MngSetting.FirstOrDefault();

                    var printlog = this.db.SalePrintlogNew.Where(s => s.Printno == printno).FirstOrDefault();
                    if (printlog != null)
                    {
                        int materialid = printlog.MaterialId.Value;

                        path = savepath + "/template/template" + materialid + ".html";

                        // 准备生成质保书相关参数
                        certparams["printno"] = printno;

                        // 1.二维码扫码后链接地址
                        certparams["qrcodeurl"] = settings.Domain_QRCode;
                        certparams["qrcodeurl"] = string.Format(settings.Domain_QRCode, printlog.Id, printlog.Checkcode);

                        // 2.质保书印章的角度
                        certparams["rotate"] = printlog.Signetangle;

                        // 3.质保书印章的角度
                        certparams["rotate"] = printlog.Signetangle;

                        // 4.质保书的高宽
                        certparams["width"] = 1583;
                        certparams["height"] = 1125;

                        // 5.质保书上的图片文件所在域名
                        certparams["filedomain"] = settings.ImgPath;

                        // 6.是否打上水印
                        certparams["iswater"] = iswater ? 1 : 0;

                        // 7.二维码生成的缩放比
                        certparams["scale"] = 4;
                    }

                    FileStream fs = new FileStream(path, FileMode.Open);
                    byte[] template_buffer = new byte[fs.Length];
                    fs.Read(template_buffer, 0, template_buffer.Length);
                    fs.Close();

                    string templatestring = Encoding.UTF8.GetString(template_buffer);

                    // 获取本地模板
                    StringBuilder html = new StringBuilder(templatestring);

                    string template_image_path = settings.Domain_WebApi;
                    if (settings.Domain_WebApi.EndsWith("/"))
                    {
                        template_image_path = template_image_path.Remove(template_image_path.Length - 1);
                    }

                    // 头部
                    html = html.Replace("{{data.Client}}", jobj["Client"].ToString());
                    html = html.Replace("{{data.ClientEn}}", jobj["ClientEn"].ToString());
                    html = html.Replace("{{data.Logo}}", template_image_path + "/qualitypics/template/images/" + jobj["Logo"].ToString());
                    html = html.Replace("{{data.TitleIcon}}", template_image_path + "/qualitypics/template/images/" + jobj["TitleIcon"].ToString());
                    html = html.Replace("{{data.QualityIcon}}", template_image_path + "/qualitypics/template/images/" + jobj["QualityIcon"].ToString());

                    // 附加的质量图标（鸿泰特有）
                    html = html.Replace("{{data.QualityIcon2}}", template_image_path + "/qualitypics/template/images/" + jobj["QualityIcon2"].ToString());

                    html = html.Replace("{{data.Brand}}", template_image_path + "/qualitypics/template/images/" + jobj["Brand"].ToString());
                    html = html.Replace("{{data.PrintNo}}", printno);
                    html = html.Replace("{{data.DeliveryCompany}}", jobj["DeliveryCompany"].ToString());
                    html = html.Replace("{{data.Gbname}}", jobj["Gbname"].ToString());
                    html = html.Replace("{{data.Standard}}", jobj["Standard"].ToString());
                    html = html.Replace("{{data.PermitNo}}", jobj["PermitNo"].ToString());
                    html = html.Replace("{{data.MaterilaName}}", jobj["MaterilaName"].ToString());
                    html = html.Replace("{{data.DeliveryState}}", jobj["DeliveryState"].ToString());
                    html = html.Replace("{{data.DeliveryStateEn}}", jobj["DeliveryStateEn"].ToString());
                    html = html.Replace("{{data.Address}}", jobj["Address"].ToString());
                    html = html.Replace("{{data.AddressEn}}", jobj["AddressEn"].ToString());

                    // 车牌号
                    html = html.Replace("{{data.LPN}}", jobj["LPN"].ToString());

                    // 采购单号
                    html = html.Replace("{{data.Purchaseno}}", jobj["Purchaseno"].ToString());


                    // 底部
                    html = html.Replace("{{data.OutDate}}", jobj["OutDate"].ToString());
                    html = html.Replace("{{data.EntryPerson}}", jobj["EntryPerson"].ToString());
                    html = html.Replace("{{data.CheckPerson}}", jobj["CheckPerson"].ToString());
                    html = html.Replace("{{data.DomainPc}}", jobj["DomainPc"].ToString());
                    html = html.Replace("{{data.Total}}", jobj["Total"].ToString());

                    Regex regex = new Regex(@"\{\{foreach\(([^\}]*)\)\}\}([\s\S]*?)\{\{/foreach\}\}", RegexOptions.IgnoreCase);
                    var match = regex.Match(html.ToString());
                    while (match.Success)
                    {
                        string matchcontent = match.Groups[0].ToString();

                        string item_name = string.Empty, item_index = string.Empty, list_name = string.Empty;

                        // 头部
                        var header = match.Groups[1].ToString();
                        if (!string.IsNullOrEmpty(header))
                        {
                            // 从头部获取遍历体，以及遍历的实体、索引
                            Regex headregex = new Regex(@"\(([a-zA-Z]*?),([a-zA-Z]*?)\)[\s]*?in[\s]*?data\.([a-zA-Z]+)$", RegexOptions.IgnoreCase);
                            var headmatch = headregex.Match(header);
                            if (headmatch.Success)
                            {
                                item_name = headmatch.Groups[1].ToString();
                                item_index = headmatch.Groups[2].ToString();
                                list_name = headmatch.Groups[3].ToString();
                            }
                        }

                        var body = match.Groups[2].ToString();

                        StringBuilder sb = new StringBuilder();
                        if (jobj[list_name] != null)
                        {
                            var datalist = (JArray)JsonConvert.DeserializeObject(jobj[list_name].ToString());
                            for (var i = 0; i < datalist.Count; i++)
                            {
                                var item = datalist[i];

                                var newbody = body;

                                Regex bodyregex = new Regex(@"\{\{[a-zA-Z+\d]*(\.[^\.]*)?(\.[^\.\}]*)?\}\}", RegexOptions.IgnoreCase);
                                var bodymatch = bodyregex.Match(newbody);
                                while (bodymatch.Success)
                                {
                                    var bodymatchcontent = bodymatch.Groups[0].ToString();

                                    string firstprop = bodymatch.Groups[1].ToString().Replace(".", string.Empty);
                                    string secondprop = bodymatch.Groups[2].ToString().Replace(".", string.Empty);

                                    // 直接当成表达式来转换，如（{{index+1}}）
                                    if (string.IsNullOrEmpty(firstprop) && string.IsNullOrEmpty(secondprop))
                                    {
                                        var mstr = bodymatch.Groups[0].ToString();

                                        // 支持简单的+、－运算
                                        Regex mregex = new Regex(@"\{\{([a-zA-Z]*)[\s*]?([\+-])?[\s*]?([\d]*)?\}\}");
                                        var mmatch = mregex.Match(mstr);
                                        if (mmatch.Success)
                                        {
                                            var varitem = mmatch.Groups[1].ToString();
                                            var varoperator = mmatch.Groups[2].ToString();
                                            var varnumber = mmatch.Groups[3].ToString();

                                            if (!string.IsNullOrEmpty(varoperator) && !string.IsNullOrEmpty(varnumber))
                                            {
                                                int.TryParse(varnumber, out int number);

                                                string replacestr = bodymatchcontent;

                                                if (varoperator == "+")
                                                {
                                                    if (varitem == item_index)
                                                    {
                                                        replacestr = string.Format("{0}", i + number);
                                                    }
                                                    else if (varitem == item_name)
                                                    {
                                                        // 暂不支持属性运算
                                                    }

                                                    newbody = newbody.Replace(bodymatchcontent, replacestr);
                                                }
                                            }
                                            else
                                            {
                                                string replacestr = string.Empty;

                                                if (varitem == item_index)
                                                {
                                                    replacestr = string.Format("{0}", i);
                                                }
                                                else if (varitem == item_name)
                                                {
                                                    replacestr = string.Format("{0}", item.ToString());
                                                }

                                                newbody = newbody.Replace(bodymatchcontent, replacestr);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // 判断是否是遍历体开头
                                        if (bodymatchcontent.StartsWith("{{" + item_name + "."))
                                        {
                                            // 获取一层属性名
                                            JToken newcontent = item[firstprop];

                                            if (!string.IsNullOrEmpty(secondprop))
                                            {
                                                if (newcontent != null && !string.IsNullOrEmpty(newcontent.ToString()))
                                                {
                                                    newcontent = newcontent[secondprop];
                                                    if (!string.IsNullOrWhiteSpace(string.Format("{0}", newcontent)))
                                                    {
                                                        if (Regex.IsMatch(string.Format("{0}", newcontent), "^([0-9]{1,})$")
                                                            || Regex.IsMatch(string.Format("{0}", newcontent), "^([0-9]{1,}[.][0-9]*)$"))
                                                        {
                                                            if (secondprop != "C"
                                                                && secondprop != "Si"
                                                                && secondprop != "Mn"
                                                                && secondprop != "P"
                                                                && secondprop != "S")
                                                            {
                                                                double.TryParse(string.Format("{0}", newcontent), out double val);
                                                                if (val <= 0)
                                                                {
                                                                    newcontent = "－";
                                                                }
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        newcontent = "－";
                                                    }
                                                }
                                                else
                                                {
                                                    newcontent = "－";
                                                }
                                            }

                                            newbody = newbody.Replace(bodymatchcontent, string.Format("{0}", newcontent));
                                        }
                                    }

                                    bodymatch = bodymatch.NextMatch();
                                }

                                sb.Append(newbody);
                            }

                            html = html.Replace(matchcontent, sb.ToString());
                        }

                        match = match.NextMatch();
                    }

                    try
                    {
                        string imgpath = this.HtmlToBitmap(html.ToString(), savepath, certparams);

                        var retobj = new JObject();
                        retobj["printno"] = printno;
                        retobj["printid"] = printlog.Id;
                        retobj["printcode"] = printlog.Checkcode;

                        return new CommonResult(CommonResultStatus.Success, "生成成功", null, retobj);
                    }
                    catch (Exception e)
                    {
                        return new CommonResult(CommonResultStatus.Success, "生成失败", e.Message);
                    }
                }
            }

            return new CommonResult(CommonResultStatus.Failed, "生成失败", result.Reason);
        }

        /// <summary>
        /// 添加首条 预置数据给当前炉批号
        /// </summary>
        /// <param name="materialid">材质ID</param>
        /// <param name="batCode">批号</param>
        private void AddFirstPdQuality(int materialid, string batCode)
        {
            // 如果该材质没有使用过预置数据，则当前炉批号给他使用第一条。
            int firstQid = this.db.PdQuality.OrderBy(c => c.Id).Where(c => c.MaterialId == materialid && c.CreateFlag == 1).First().Id;
            this.db.PdQualityProductPreset.Add(new PdQualityProductPreset()
            {
                Batcode = batCode,
                Createtime = DateTime.Now.GetUnixTimeFromDateTime().ToInt(),
                Materialid = materialid,
                Qid = firstQid
            });
            this.db.SaveChanges();
        }

        /// <summary>
        /// 生成四位数校验码
        /// </summary>
        /// <returns>校验码</returns>
        private string GenerateRandomCode()
        {
            StringBuilder codebuilder = new StringBuilder();

            long tick = DateTime.Now.Ticks;
            Random ran = new Random((int)(tick & 0xffffffffL) | (int)(tick >> 32));

            for (int i = 0; i < 4; i++)
            {
                int r = ran.Next(0, 10);
                codebuilder.Append(r);
            }

            return codebuilder.ToString();
        }

        private string HtmlToBitmap(string html, string savepath, JObject certparams)
        {
            if (string.IsNullOrEmpty(html))
            {
                return null;
            }

            if (certparams == null)
            {
                return null;
            }

            if (certparams["printno"] == null)
            {
                return null;
            }

            if (certparams["filedomain"] == null)
            {
                return null;
            }

            if (certparams["width"] == null)
            {
                return null;
            }

            if (certparams["height"] == null)
            {
                return null;
            }

            if (certparams["rotate"] == null)
            {
                return null;
            }

            if (certparams["qrcodeurl"] == null)
            {
                return null;
            }

            var cert = this.db.SalePrintlogNew.Where(s => s.Printno == certparams["printno"].ToString()).FirstOrDefault();
            if (cert == null)
            {
                return null;
            }

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xhtml+xml"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml", 0.9));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("image/webp"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*", 0.8));
            client.DefaultRequestHeaders.Add("accept", "application/json, text/javascript, */*; q=0.01");

            var values = new List<KeyValuePair<string, string>>();

            string qrcodeurl = certparams["qrcodeurl"].ToString();

            // 如果是不是正式证书而是预览图，则将序号后4位用****代替，并且扫码参数后多加一个参数
            if (certparams["iswater"].ToString() != "1")
            {
                var pno = certparams["printno"].ToString();
                string cryptpno = Regex.Replace(pno, @"(\d{4})(\d{4})", "$1****");
                html = html.Replace(pno, cryptpno);

                qrcodeurl = qrcodeurl + "&t=1";
            }

            values.Add(new KeyValuePair<string, string>("html", HttpUtility.UrlEncode(html)));
            values.Add(new KeyValuePair<string, string>("filedomain", certparams["filedomain"].ToString()));
            values.Add(new KeyValuePair<string, string>("scale", certparams["scale"].ToString()));
            values.Add(new KeyValuePair<string, string>("rotate", certparams["rotate"].ToString()));
            values.Add(new KeyValuePair<string, string>("width", certparams["width"].ToString()));
            values.Add(new KeyValuePair<string, string>("height", certparams["height"].ToString()));
            values.Add(new KeyValuePair<string, string>("qrcodeurl", HttpUtility.UrlEncode(HttpUtility.UrlEncode(qrcodeurl))));
            values.Add(new KeyValuePair<string, string>("iswater", certparams["iswater"].ToString()));

            JObject jsonvalues = new JObject
            {
                { "html", HttpUtility.UrlEncode(HttpUtility.UrlEncode(html)) },
                { "filedomain", certparams["filedomain"].ToString() },
                { "scale", certparams["scale"].ToString() },
                { "rotate", certparams["rotate"].ToString() },
                { "width", certparams["width"].ToString() },
                { "height", certparams["height"].ToString() },
                { "qrcodeurl", HttpUtility.UrlEncode(HttpUtility.UrlEncode(qrcodeurl)) },
                { "iswater", certparams["iswater"].ToString() },
            };

            var content = new FormUrlEncodedContent(values);
            content.Headers.Add("UserAgent", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/31.0.1650.57 Safari/537.36");
            content.Headers.Add("Timeout", "10000");
            content.Headers.Add("KeepAlive", "true");

            var response = client.PostAsync(certparams["filedomain"].ToString() + "/qrcode/getqualityimg.ashx", content);
            response.Wait();
            var responsedata = response.Result.Content.ReadAsStreamAsync();
            responsedata.Wait();

            string path = string.Empty;
            using (var img = Image.Load(responsedata.Result))
            {
                var encoder = new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder();
                encoder.Quality = 90;

                if (certparams["iswater"].ToString() == "1")
                {
                    path = savepath + "/" + cert.Id + cert.Checkcode + ".jpg";
                    img.Save(path, encoder);

                    cert.Status = 1;
                }
                else
                {
                    path = savepath + "/" + cert.Id + cert.Checkcode + "p.jpg";
                    img.Save(path, encoder);

                    cert.Status = 0;
                }

                this.db.SalePrintlogNew.Update(cert);
                this.db.SaveChanges();
            }

            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[fs.Length];
            fs.Read(buffer, 0, (int)fs.Length);

            return path;
        }
    }
}
