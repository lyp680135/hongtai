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
    using static DataLibrary.EnumList;

    public class CertService : ICertService
    {
        private DataLibrary.DataContext db;
        private IQualityService qualityService;
        private IStockOutService stockoutService;

        public CertService(DataLibrary.DataContext dataContext, IQualityService quality, IStockOutService stockout)
        {
            this.db = dataContext;
            this.qualityService = quality;
            this.stockoutService = stockout;
        }

        public CommonResult AddCert(JArray list, int sellerid, string lpn, string consignor, int userid)
        {
            if (list == null || list.Count == 0)
            {
                return new CommonResult(CommonResultStatus.Failed, "打印记录保存失败", "产品物资不能为空！");
            }

            try
            {
                var mngsetInfo = this.db.MngSetting.FirstOrDefault();

                // 流程版必需输了质量数据且通过了审核才能出库
                if (mngsetInfo.SystemVersion == EnumList.SystemVersion.流程版本)
                {
                    foreach (var item in list)
                    {
                        // 判断该炉号是否已经做好质量录入
                        var quality = this.db.PdQuality.Where(p => p.Batcode == item["Batcode"].ToString() && p.CheckStatus == CheckStatus_PdQuality.审核通过).FirstOrDefault();
                        if (quality == null)
                        {
                            return new CommonResult(CommonResultStatus.Failed, "出库失败了，所选批号[" + item["batcode"].ToString() + "]还没有通过质量检测。", string.Empty);
                        }
                    }
                }
                else if (mngsetInfo.SystemVersion == EnumList.SystemVersion.简单版本)
                {
                    foreach (var item in list)
                    {
                        int.TryParse(item["Materialid"].ToString(), out int materialid);

                        // 判断系统是否有预置数据
                        var presetdata = this.db.PdQuality.Where(p => p.MaterialId == materialid && p.CreateFlag == 1).Count();
                        if (presetdata > 0)
                        {
                            // 判断该炉号是否已经匹配到预置质量数据
                            var quality = this.db.PdQualityProductPreset.Where(p => p.Batcode == item["Batcode"].ToString()).FirstOrDefault();
                            if (quality == null)
                            {
                                return new CommonResult(CommonResultStatus.Failed, "出库失败了，所选批号[" + item["Batcode"].ToString() + "]还没有质量检测。", string.Empty);
                            }
                        }
                        else
                        {
                            return new CommonResult(CommonResultStatus.Failed, "出库失败了，所选批号[" + item["Batcode"].ToString() + "]找不到质量检测。", string.Empty);
                        }
                    }
                }

                List<SaleSellerAuth> authlist = new List<SaleSellerAuth>();

                // 先出库生成授权信息
                foreach (var item in list)
                {
                    var batcode = item["Batcode"].ToString();
                    var lengthtypestr = item["Lengthtype"].ToString();

                    int.TryParse(lengthtypestr, out int lengthtype);
                    int.TryParse(item["Number"].ToString(), out int number);

                    CommonResult result = this.stockoutService.Stockout(batcode, lpn, sellerid, lengthtype, number, userid);
                    if (result.Status == (int)CommonResultStatus.Failed)
                    {
                        return new CommonResult(CommonResultStatus.Failed, "打印记录保存失败", result.Reason);
                    }
                    else
                    {
                        var datalist = (List<SaleSellerAuth>)result.Data;
                        if (datalist != null)
                        {
                            foreach (var auth in datalist)
                            {
                                authlist.Add(auth);
                            }
                        }
                    }
                }

                if (authlist.Count <= 0)
                {
                    // TODO： 判断售达方是否有还没打印过质保书的产品授权，如果有满足条件且数量大于当前打印件数的，可以用原来的资源进行打印
                    return new CommonResult(CommonResultStatus.Failed, "打印记录保存失败", "没有产品出库成功！");
                }

                List<DataLibrary.SalePrintLogDetail> productinfos = new List<DataLibrary.SalePrintLogDetail>();

                int findedprintid = 0;
                int findsamecount = 0;

                // 生成一张质保书下的产品分组列表
                foreach (SaleSellerAuth jt in authlist)
                {
                    int authid = jt.Id;
                    int number = jt.Number.Value;

                    // 查找打印明细表中同批货是否已经开过质保书(但没有下载过）
                    var logdetail = (from l in this.db.SalePrintLogDetail
                                     where l.Authid == authid && l.Number == number
                                     select l).FirstOrDefault();
                    if (logdetail != null)
                    {
                        if (findedprintid == 0)
                        {
                            findedprintid = logdetail.PrintId;
                            findsamecount++;
                        }
                        else
                        {
                            if (findedprintid == logdetail.PrintId)
                            {
                                findsamecount++;
                            }
                        }
                    }

                    productinfos.Add(new SalePrintLogDetail
                    {
                        Authid = authid,
                        Number = number,
                    });
                }

                // 如果找到的记录完全一样，则应该直接调用原来的质保书即可
                if (findsamecount == list.Count)
                {
                    var log = this.db.SalePrintlog.Where(s => s.Id == findedprintid && (s.Status == null || s.Status == 0)).FirstOrDefault();
                    if (log != null)
                    {
                        if (log.Consignor != consignor)
                        {
                            // 更新收货单位
                            log.Consignor = consignor;
                            this.db.SalePrintlog.Update(log);

                            if (this.db.SaveChanges() > 0)
                            {
                                return new CommonResult(CommonResultStatus.Success, "打印记录保存成功", null, log.Printno);
                            }
                            else
                            {
                                return new CommonResult(CommonResultStatus.Failed, "打印记录保存失败", "更新收货单位失败！");
                            }
                        }

                        return new CommonResult(CommonResultStatus.Success, "沿用上次没有生成的质保书号", null, log.Printno);
                    }
                }

                string pno = this.db.SalePrintlog.Max(x => x.Printno);
                if (pno == null || string.IsNullOrEmpty(pno))
                {
                    pno = "10000001";
                }
                else
                {
                    int no = 0;
                    int.TryParse(pno, out no);
                    if (no < 10000000)
                    {
                        int realno = 10000000 + no;
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

                int time = (int)Util.Extensions.GetCurrentUnixTime();
                SalePrintlog entity = new SalePrintlog
                {
                    Createtime = time,
                    Consignor = consignor,
                    Printno = pno,
                    Signetangle = new Random().Next(-45, 45),
                    Checkcode = checkcode,
                    Adder = userid,
                };

                using (var tran = this.db.Database.BeginTransaction())
                {
                    this.db.SalePrintlog.Add(entity);

                    if (this.db.SaveChanges() > 0)
                    {
                        // 增加打印明细记录
                        foreach (var item in productinfos)
                        {
                            item.PrintId = entity.Id;
                            this.db.SalePrintLogDetail.Add(item);
                        }

                        if (this.db.SaveChanges() > 0)
                        {
                            tran.Commit();

                            return new CommonResult(CommonResultStatus.Success, "打印记录保存成功", null, pno);
                        }
                        else
                        {
                            // 回滚操作
                            tran.Rollback();

                            return new CommonResult(CommonResultStatus.Failed, "打印记录保存失败", "数据处理有误，已作回滚操作！");
                        }
                    }
                }

                return new CommonResult(CommonResultStatus.Failed, "打印记录保存失败", "预览模版失败，数据处理有误！");
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
            var prints = this.db.SalePrintlog.Where(x => x.Printno == printno);
            if (prints != null && prints.Count() > 0)
            {
                var mngsetting = this.db.MngSetting.FirstOrDefault();
                var sitebasic = this.db.SiteBasic.FirstOrDefault();
                var printFirst = prints.FirstOrDefault();
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

                    // 生产许可证号
                    info.Add("PermitNo", "XK05-001-00055");

                    // 地址
                    info.Add("Address", "中国·湖南·娄底");
                    info.Add("AddressEn", "HuNan.LouDi P.R.C");

                    // 产品品牌小图标
                    info.Add("Brand", "brand.jpg");
                }

                info.Add("Id", printFirst.Id);
                info.Add("CheckCode", printFirst.Checkcode);

                // 收货单位
                info.Add("DeliveryCompany", printFirst.Consignor);

                // 印章旋转角度
                info.Add("SignetAngle", printFirst.Signetangle);

                // 经销商授权表
                int printId = prints.FirstOrDefault().Id;
                List<int> list_AuthId = this.db.SalePrintLogDetail.Where(c => c.PrintId == printId).Select(c => c.Authid).ToList();

                var sellerauth = this.db.SaleSellerAuth.FirstOrDefault(x => x.Id == list_AuthId.FirstOrDefault());
                if (sellerauth != null)
                {
                    // 产品表
                    var pdproduct = this.db.PdProduct.FirstOrDefault(x => x.Batcode == sellerauth.Batcode);

                    // 质量标准
                    var basequality = this.db.BaseQualityStandard.FirstOrDefault(x => x.Classid == sellerauth.Classid && x.Materialid == sellerauth.Materialid);
                    if (basequality != null)
                    {
                        // 执行标准
                        var standard = this.db.BaseGbProduction.FirstOrDefault(x => x.Id == basequality.Standardid);
                        info.Add("Standard", standard.Name);
                    }

                    // 流程版必需输了质量数据且通过了审核才能出库
                    if (mngsetting.SystemVersion == EnumList.SystemVersion.流程版本)
                    {
                        // 质量审核
                        var pdquality = this.db.PdQuality.FirstOrDefault(x => x.Batcode == pdproduct.Batcode);
                        if (pdquality != null)
                        {
                            string checkPerson = string.Empty;
                            if (pdquality.CheckPerson.HasValue)
                            {
                                checkPerson = this.db.MngAdmin.FirstOrDefault(c => c.Id == pdquality.CheckPerson).RealName;
                            }

                            info.Add("CheckPerson", checkPerson);  // 审核人
                            info.Add("EntryPerson", this.db.MngAdmin.FirstOrDefault(c => c.Id == pdquality.EntryPerson).RealName);  // 录入人
                            info.Add("OutDate", Util.Extensions.GetDateTimeFromUnixTime(pdquality.Createtime).ToString("yyyy-MM-dd")); // 出证日期
                        }
                    }
                    else
                    {
                        // 简单流程预置数据的在读完质量数据时再取得审核人和签发人
                    }

                    // 产品国标名称
                    var gbname = this.db.BaseProductClass.FirstOrDefault(x => x.Id == sellerauth.Classid);

                    // 产品材质名称
                    var material = this.db.BaseProductMaterial.FirstOrDefault(x => x.Id == sellerauth.Materialid);
                    info.Add("Gbname", gbname.Gbname);
                    info.Add("MaterilaName", material.Name);

                    // 交货状态
                    info.Add("DeliveryState", Enum.GetName(typeof(EnumList.DeliveryType), gbname.DeliveryType));

                    // 交货状态英文
                    if (gbname.DeliveryType == (int)EnumList.DeliveryType.直条定尺)
                    {
                        info.Add("DeliveryStateEn", "Fixed length straight strip");
                    }
                    else if (gbname.DeliveryType == (int)EnumList.DeliveryType.盘卷)
                    {
                        info.Add("DeliveryStateEn", "Steel coll");
                    }
                    else
                    {
                        info.Add("DeliveryStateEn", "Unfixed length straight strip");
                    }

                    List<Tuple<string, int?>> detaillist = new List<Tuple<string, int?>>();

                    // 从打印明细表中、获取一张质保书下的产品明细
                    var logdetails = (from s in this.db.SalePrintLogDetail
                                      where s.PrintId == printFirst.Id
                                      join sl in this.db.SaleSellerAuth
                                     on s.Authid equals sl.Id
                                      select new
                                      {
                                          s.Id,
                                          s.Authid,
                                          s.Number,
                                          s.PrintId,
                                          sl.Materialid,
                                          sl.Classid,
                                          sl.Batcode,
                                          Spec = (from spec in this.db.BaseSpecifications where spec.Id == sl.Specid select spec).Single(),
                                      }).ToList();
                    if (logdetails != null)
                    {
                        foreach (var detail in logdetails)
                        {
                            detaillist.Add(new Tuple<string, int?>(detail.Batcode, detail.Materialid.Value));
                        }
                    }

                    // 获取质量数据
                    var info_data = this.qualityService.GetQualityData(detaillist);
                    if (info_data.Count < detaillist.Count)
                    {
                        return new CommonResult(CommonResultStatus.Failed, "质量数据异常", "找不到所选产品的质量数据！");
                    }

                    if (mngsetting.SystemVersion == EnumList.SystemVersion.简单版本)
                    {
                        string checkPerson = string.Empty;
                        string entryPerson = string.Empty;
                        var firstquality = info_data.FirstOrDefault();
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

                        info.Add("OutDate", DateTime.Now.ToString("yyyy-MM-dd")); // 出证日期
                    }

                    // 组装输出结构休，补全产品信息、规格、长度、质量数据
                    JArray outputarr = new JArray();
                    int index = 0;
                    foreach (var detail in logdetails)
                    {
                        JObject outinfo = new JObject();
                        outinfo["Batcode"] = detail.Batcode;

                        JObject productinfo = new JObject();
                        productinfo["Specname"] = detail.Spec.Specname;
                        productinfo["Length"] = detail.Spec.Referlength;
                        productinfo["Piece"] = detail.Number;
                        productinfo["Weight"] = detail.Spec.Referpieceweight * detail.Number;

                        outinfo["PdProduct"] = productinfo;

                        // 获取规格直径
                        int.TryParse(detail.Spec.Specname.Replace("Ф", string.Empty).Replace("Φ", string.Empty), out int diameter);

                        if (index < info_data.Count)
                        {
                            var item = info_data[index];

                            bool ok = false;
                            if (mngsetting.SystemVersion == EnumList.SystemVersion.简单版本)
                            {
                                if (detail.Materialid == item.MaterialId)
                                {
                                    ok = true;
                                }
                            }
                            else if (detail.Batcode == item.Batcode && detail.Materialid == item.MaterialId)
                            {
                                ok = true;
                            }

                            if (ok)
                            {
                                double c = 0, mn = 0, cr = 0, v = 0, mo = 0, cu = 0, ni = 0;

                                // 化学成份相关数据
                                outinfo["Qualityinfo"] = (JObject)item.Qualityinfos.Object;

                                foreach (var d in (JObject)item.Qualityinfos.Object)
                                {
                                    if (d.Key.ToUpper() == "C")
                                    {
                                        double.TryParse(outinfo["Qualityinfo"][d.Key].ToString(), out c);
                                    }

                                    if (d.Key.ToUpper() == "MN")
                                    {
                                        double.TryParse(outinfo["Qualityinfo"][d.Key].ToString(), out mn);
                                    }

                                    if (d.Key.ToUpper() == "CR")
                                    {
                                        double.TryParse(outinfo["Qualityinfo"][d.Key].ToString(), out cr);
                                    }

                                    if (d.Key.ToUpper() == "V")
                                    {
                                        double.TryParse(outinfo["Qualityinfo"][d.Key].ToString(), out v);
                                    }

                                    if (d.Key.ToUpper() == "MO")
                                    {
                                        double.TryParse(outinfo["Qualityinfo"][d.Key].ToString(), out mo);
                                    }

                                    if (d.Key.ToUpper() == "CU")
                                    {
                                        double.TryParse(outinfo["Qualityinfo"][d.Key].ToString(), out cu);
                                    }

                                    if (d.Key.ToUpper() == "NI")
                                    {
                                        double.TryParse(outinfo["Qualityinfo"][d.Key].ToString(), out ni);
                                    }
                                }

                                // Ceq = C +  Mn/6  + (Cr + V +Mo) / 5 + (Cu + ni) / 15
                                outinfo["Qualityinfo"]["Ceq"] = (c + (mn / 6) + ((cr + v + mo) / 5) + ((cu + ni) / 15)).ToString("0.00");

                                outinfo["Qualityinfo"]["Surface"] = "合格/Pass";
                                outinfo["Qualityinfo"]["Metall"] = "－";
                                outinfo["Qualityinfo"]["Vickers"] = "－";
                                outinfo["Qualityinfo"]["Microstructures"] = "－";

                                if (mngsetting.SystemVersion == EnumList.SystemVersion.简单版本)
                                {
                                    // 质量偏差这里，如果是预置质量数据，12规格的偏差多负1%
                                    if (outinfo["Qualityinfo"]["重量偏差"] != null)
                                    {
                                        double.TryParse(outinfo["Qualityinfo"]["重量偏差"].ToString(), out double weightoffset);
                                        if (diameter <= 12)
                                        {
                                            outinfo["Qualityinfo"]["重量偏差"] = weightoffset - 1;
                                        }
                                    }
                                }

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
                                                        dynamicdata[d.Key] = dynamicdata[d.Key] + "/" + x.Value;
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
                                        dynamicdata["强屈比"] = dynamicdata["强屈比"].ToString() + (antistrength / lowstrength).ToString("0.0");
                                        dynamicdata["屈屈比"] = dynamicdata["屈屈比"].ToString() + (lowstrength / material.Standardstrength).ToString("0.0");
                                    }
                                    else
                                    {
                                        dynamicdata["强屈比"] = dynamicdata["强屈比"].ToString() + "－";
                                        dynamicdata["屈屈比"] = dynamicdata["屈屈比"].ToString() + "－";
                                    }

                                    pos++;
                                }

                                // 冷弯、反弯
                                dynamicdata["冷弯"] = "完好/Pass";
                                dynamicdata["反弯"] = "完好/Pass";

                                // 冷弯弯心、反弯弯心  (规格*系数)
                                dynamicdata["冷弯弯心"] = detail.Spec.Coldratio * diameter;
                                dynamicdata["反弯弯心"] = detail.Spec.Reverseratio * diameter;

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

                    info.Add("List", JsonConvert.SerializeObject(outputarr));
                }

                // 物资产品化学成份、力学指标
                return new CommonResult(CommonResultStatus.Success, "查询成功", null, JsonConvert.SerializeObject(info));
            }

            return new CommonResult(CommonResultStatus.Failed, "未找到相关记录", string.Empty);
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

                    var printlog = this.db.SalePrintlog.Where(s => s.Printno == printno).FirstOrDefault();
                    if (printlog != null)
                    {
                        // 获取产品牌号
                        List<int> list_AuthId = this.db.SalePrintLogDetail.Where(c => c.PrintId == printlog.Id).Select(c => c.Authid).ToList();

                        var auth = this.db.SaleSellerAuth.Where(s => s.Id == list_AuthId.First()).FirstOrDefault();
                        if (auth != null)
                        {
                            int materialid = auth.Materialid.Value;

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

                    // 底部
                    html = html.Replace("{{data.OutDate}}", jobj["OutDate"].ToString());
                    html = html.Replace("{{data.EntryPerson}}", jobj["EntryPerson"].ToString());
                    html = html.Replace("{{data.CheckPerson}}", jobj["CheckPerson"].ToString());
                    html = html.Replace("{{data.DomainPc}}", jobj["DomainPc"].ToString());

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

            var cert = this.db.SalePrintlog.Where(s => s.Printno == certparams["printno"].ToString()).FirstOrDefault();
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

            // string paramsvalues = "html=" + HttpUtility.UrlEncode(HttpUtility.UrlEncode(html))
            //    + "&filedomain=" + certparams["filedomain"].ToString()
            //    + "&scale=" + certparams["scale"].ToString()
            //    + "&rotate=" + certparams["rotate"].ToString()
            //    + "&width=" + certparams["width"].ToString()
            //    + "&height=" + certparams["height"].ToString()
            //    + "&qrcodeurl=" + HttpUtility.UrlEncode(HttpUtility.UrlEncode(qrcodeurl))
            //    + "&iswater=" + certparams["iswater"].ToString();

            var content = new FormUrlEncodedContent(values);
            //var content = new StringContent(JsonConvert.SerializeObject(jsonvalues), Encoding.UTF8, "application/json");
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

                this.db.SalePrintlog.Update(cert);
                this.db.SaveChanges();
            }

            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[fs.Length];
            fs.Read(buffer, 0, (int)fs.Length);

            return path;
        }
    }
}
