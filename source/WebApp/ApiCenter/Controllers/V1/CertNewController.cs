namespace WarrantyApiCenter.Controllers.V1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Common.IService;
    using Common.Service;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Util;
    using WarrantyApiCenter.Models;
    using static DataLibrary.EnumList;

    /// <summary>
    /// WPF请求备用
    /// </summary>
    [Produces("application/json")]
    public class CertNewController : BaseController
    {
        private readonly IHostingEnvironment hostingEnvironment;
        private DataLibrary.DataContext db;
        private IUserService userService;
        private ICertNewService certNewService;
        private ILogService logService;

        public CertNewController(DataLibrary.DataContext dataContext, IUserService user, ICertNewService cert, IHostingEnvironment hostingEnvironment, ILogService logService)
        {
            this.db = dataContext;
            this.userService = user;
            this.certNewService = cert;
            this.hostingEnvironment = hostingEnvironment;
            this.logService = logService;
        }

        [HttpGet]
        [AllowAnonymous]
        public ResponseModel Get(int id)
        {
            if (id > 0)
            {
                var printlog = this.db.SalePrintlogNew.Where(s => s.Id == id).FirstOrDefault();

                return new ResponseModel(ApiResponseStatus.Success, string.Empty, JsonConvert.SerializeObject(printlog));
            }
            else
            {
                var printlog = this.db.SalePrintlogNew.Where(s => s.Status == 1).OrderByDescending(s => s.Createtime).FirstOrDefault();

                return new ResponseModel(ApiResponseStatus.Success, string.Empty, JsonConvert.SerializeObject(printlog));
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public ResponseModel Get(string printno)
        {
            if (!string.IsNullOrEmpty(printno))
            {
                var printlog = this.db.SalePrintlogNew.Where(s => s.Printno == printno).FirstOrDefault();

                return new ResponseModel(ApiResponseStatus.Success, string.Empty, JsonConvert.SerializeObject(printlog));
            }

            return null;
        }

        [HttpPost]
        [AllowAnonymous]
        public ResponseModel Post(string requestData)
        {
            JArray array = new JArray();
            var obj = (JObject)JsonConvert.DeserializeObject(requestData);

            if (obj != null)
            {
                var list = (obj["list"] != null) ? obj["list"].ToString() : null;
                var lpn = (obj["lpn"] != null) ? obj["lpn"].ToString() : null;
                var sellerid = (obj["sellerid"] != null) ? int.Parse(obj["sellerid"].ToString()) : 0;
                var consignor = (obj["consignor"] != null) ? obj["consignor"].ToString() : null;
                var printidstr = (obj["printid"] != null) ? obj["printid"].ToString() : null;
                var userid = (obj["userid"] != null) ? int.Parse(obj["userid"].ToString()) : 0;
                var outDate = (obj["outdate"] != null) ? obj["outdate"].SafeString().ToDate() : DateTime.Now;

                // 闽源手动版时间需要自定义， 如果时间是服务器当天，则取服务器时间
                if (outDate.Year == DateTime.Now.Year && outDate.DayOfYear == DateTime.Now.DayOfYear)
                {
                    outDate = DateTime.Now;
                }

                var materialId = obj["materialid"].ToInt(); // 材质ID

                int.TryParse(printidstr, out int printid);

                string printno = string.Empty;
                int iswater = 0;
                if (list == null && obj["printno"] != null && obj["iswater"] != null)
                {
                    printno = obj["printno"].ToString();
                    iswater = int.Parse(obj["iswater"].ToString());
                }
                else
                {
                    array = (JArray)JsonConvert.DeserializeObject(list);

                    if (printid <= 0)
                    {
                        CommonResult result = this.certNewService.AddCert(array, sellerid, lpn, consignor, userid, outDate);
                        if (result.Status == (int)CommonResultStatus.Failed)
                        {
                            return new ResponseModel(ApiResponseStatus.Failed, result.Message, result.Reason);
                        }

                        // 获取打印序号
                        printno = result.Data.ToString();
                    }
                    else
                    {
                        var printlog = this.db.SalePrintlogNew.FirstOrDefault(s => s.Id == printid);
                        if (printlog != null)
                        {
                            printno = printlog.Printno;

                            if (printlog.Status == 0)
                            {
                                // 更新出库的车牌号和收货单位、还有打印的数量
                                printlog.Consignor = consignor;
                                printlog.Adder = userid;
                                this.db.SalePrintlogNew.Update(printlog);

                                this.db.SaveChanges();
                            }
                            else if (printlog.Status == 1)
                            {
                                // 已经打印过的不允许更新，直接返回原照片
                                JObject retdata = new JObject();
                                retdata["printid"] = printid;
                                retdata["printno"] = printlog.Printno;
                                retdata["printcode"] = printlog.Checkcode;

                                return new ResponseModel(ApiResponseStatus.Success, "生成成功", JsonConvert.SerializeObject(retdata));
                            }
                            else if (printlog.Status == 10)
                            {
                                // 已经撤销过的不允许更新，直接返回原照片
                                JObject retdata = new JObject();
                                retdata["printid"] = printid;
                                retdata["printno"] = printlog.Printno;
                                retdata["printcode"] = printlog.Checkcode;

                                return new ResponseModel(ApiResponseStatus.Success, "生成成功", JsonConvert.SerializeObject(retdata));
                            }
                        }
                        else
                        {
                            return new ResponseModel(ApiResponseStatus.Failed, "数据错误", "打印记录不存在！");
                        }
                    }
                }

                if (!string.IsNullOrEmpty(printno))
                {
                    string certsavepath = this.hostingEnvironment.ContentRootPath + "/qualitypics/";

                    // 生成质保书
                    CommonResult retresult = this.certNewService.GenerateCert(printno, certsavepath, (iswater == 0) ? false : true);
                    if (retresult.Status == (int)CommonResultStatus.Failed)
                    {
                        return new ResponseModel(ApiResponseStatus.Failed, retresult.Message, retresult.Reason);
                    }
                    else
                    {
                        return new ResponseModel(ApiResponseStatus.Success, retresult.Message, JsonConvert.SerializeObject(retresult.Data));
                    }
                }

                return new ResponseModel(ApiResponseStatus.Failed, "数据错误", "生成的序列号有误");
            }

            return new ResponseModel(ApiResponseStatus.Failed, "生成失败", "参数有误");
        }


        /// <summary>
        /// 撤回质保书
        /// </summary>
        /// <param name="id">质保书ID</param>
        /// <returns>撤回操作状态</returns>
        [HttpPost]
        [Route("Revokecert")]
        [AllowAnonymous]
        public ResponseModel Revokecert(int id)
        {
            var modelPrint = this.db.SalePrintlogNew.FirstOrDefault(c => c.Id == id);
            if (modelPrint != null)
            {
                if (modelPrint.Status != (int)SalePrintlogStatus.已下载)
                {
                    return new ResponseModel(ApiResponseStatus.Failed, "该质保书未生成，不可撤回", "该质保书未生成，不可撤回");
                }

                var createTime = modelPrint.Createtime.ToLong().GetDateTimeFromUnixTime().Date;
                if (DateTime.Now.Date > createTime.AddMonths(1))
                {
                    return new ResponseModel(ApiResponseStatus.Failed, "只能撤回30天内的质保书", "只能撤回30天内的质保书");
                }

                using (var tran = this.db.Database.BeginTransaction())
                {
                    // 标识质保书状态
                    modelPrint.Status = (int)SalePrintlogStatus.已撤回;

                    // 查找打印详情纪录并纪录 相关的授权纪录ID列表
                    var list_saleSellerAuth = this.db.SalePrintLogDetailNew.Where(c => c.PrintId == modelPrint.Id);

                    // 删除打印详情纪录
                    this.db.SalePrintLogDetailNew.RemoveRange(list_saleSellerAuth);

                    this.db.SaveChanges();

                    tran.Commit();
                }

                return new ResponseModel(ApiResponseStatus.Success, "撤回成功", "撤回成功");
            }
            else
            {
                return new ResponseModel(ApiResponseStatus.Failed, "该质保书不存在", "该质保书不存在");
            }
        }


        [HttpPost]
        [Route("Purchasecert")]
        [AllowAnonymous]
        public ResponseModel Purchasecert(string requestData)
        {
            try
            {
                JArray array = new JArray();
                var obj = (JObject)JsonConvert.DeserializeObject(requestData);

                if (obj != null)
                {
                    var list = (obj["list"] != null) ? obj["list"].ToString() : null;
                    var lpn = (obj["lpn"] != null) ? obj["lpn"].ToString() : null;
                    var sellerid = (obj["sellerid"] != null) ? int.Parse(obj["sellerid"].ToString()) : 0;
                    var consignor = (obj["consignor"] != null) ? obj["consignor"].ToString() : null;
                    var printidstr = (obj["printid"] != null) ? obj["printid"].ToString() : null;
                    var userid = (obj["userid"] != null) ? int.Parse(obj["userid"].ToString()) : 0;
                    var outDate = (obj["outdate"] != null) ? obj["outdate"].SafeString().ToDate() : DateTime.Now;
                    var purchaseno = (obj["purchaseorderno"] != null) ? obj["purchaseorderno"].ToString() : null;

                    // 闽源手动版时间需要自定义， 如果时间是服务器当天，则取服务器时间
                    if (outDate.Year == DateTime.Now.Year && outDate.DayOfYear == DateTime.Now.DayOfYear)
                    {
                        outDate = DateTime.Now;
                    }

                    int.TryParse(printidstr, out int printid);

                    if (string.IsNullOrEmpty(lpn))
                    {
                        return new ResponseModel(ApiResponseStatus.Failed, "生成失败", "必须填写随车车牌号才能出库！");
                    }

                    if (sellerid <= 0)
                    {
                        return new ResponseModel(ApiResponseStatus.Failed, "生成失败", "必须选择售达方才能出库！");
                    }
                    else
                    {
                        var seller = this.db.SaleSeller.Where(s => s.Id == sellerid).FirstOrDefault();
                        if (seller == null)
                        {
                            return new ResponseModel(ApiResponseStatus.Failed, "生成失败", "无效的售达方数据");
                        }
                        consignor = seller.Name;
                    }

                    int materialId = 0;

                    if (string.IsNullOrEmpty(list) && obj["printno"] == null)
                    {
                        return new ResponseModel(ApiResponseStatus.Failed, "生成失败", "必须输入好产品才能生成质保书！");
                    }
                    else
                    {
                        array = (JArray)JsonConvert.DeserializeObject(list);
                        if (array == null || array.Count == 0)
                        {
                            return new ResponseModel(ApiResponseStatus.Failed, "生成失败", "必须输入好产品才能生成质保书！");
                        }

                        // 检查牌号是否一致
                        bool finderr = false;
                        int index = 0;
                        List<string> errors = new List<string>();
                        List<string> batcodes = new List<string>();
                        foreach (var item in array)
                        {
                            index++;
                            var batcode = (item["Batcode"] != null) ? item["Batcode"].ToString() : null;
                            var lengthstr = (item["Length"] != null) ? item["Length"].ToString() : null;
                            var spec = (item["Specname"] != null) ? item["Specname"].ToString() : null;
                            var printnumberstr = (item["Printnumber"] != null) ? item["Printnumber"].ToString() : null;
                            var length = lengthstr.ToInt();
                            var printnumber = printnumberstr.ToInt();

                            if (string.IsNullOrEmpty(batcode))
                            {
                                errors.Add($"第{index}行, 请输入批号");
                                continue;
                            }

                            if (length <= 0)
                            {
                                errors.Add($"第{index}行, 请输入定尺长度");
                                continue;
                            }

                            if (string.IsNullOrEmpty(spec))
                            {
                                errors.Add($"第{index}行, 请输入正确的规格");
                                continue;
                            }

                            if (printnumber <= 0)
                            {
                                errors.Add($"第{index}行, 请输入件数");
                                continue;
                            }

                            batcodes.Add($"{batcode} {spec}");

                            var p = this.db.PdProduct.Where(s => s.Batcode == batcode && s.Length == length).FirstOrDefault();
                            if (p == null)
                            {
                                errors.Add($"批号: {batcode} 定尺: {length}未找到产品数据");
                                continue;
                            }

                            int tmpMaterialid = p.Materialid ?? 0;
                            if (tmpMaterialid <= 0)
                            {
                                errors.Add($"批号: {batcode} 定尺: {length}未找到材质信息");
                                continue;
                            }

                            if (materialId == 0 && tmpMaterialid > 0)
                            {
                                materialId = tmpMaterialid;
                            }
                            else if (materialId != tmpMaterialid && !finderr)
                            {
                                finderr = true;
                                break;
                            }

                            item["Materialid"] = p.Materialid;
                            item["Spec"] = spec.Replace("Φ","");
                            item["SingleWeight"] = p.Weight;
                        }

                        if (errors.Count > 0)
                        {
                            return new ResponseModel(ApiResponseStatus.Failed, "生成失败", string.Join(",", errors));
                        }

                        if (finderr)
                        {
                            return new ResponseModel(ApiResponseStatus.Failed, "生成失败", "不同牌号的产品不能打印在一张质保书中，请按要求选择！");
                        }

                        var loopBatcode = batcodes.GroupBy(x => x).Select(g => new { g.Key, Total = g.Count() }).Where(s => s.Total > 1);
                        if (loopBatcode.Count() > 0)
                        {
                            return new ResponseModel(ApiResponseStatus.Failed, "生成失败", string.Join(",", loopBatcode.Select(x => x.Key).ToArray()) + " 请写在同一行");
                        }
                    }

                    // var materialId = obj["materialid"].ToInt(); // 材质ID
                    string printno = string.Empty;
                    int iswater = 0;
                    if (list == null && obj["printno"] != null && obj["iswater"] != null)
                    {
                        printno = obj["printno"].ToString();
                        iswater = int.Parse(obj["iswater"].ToString());
                    }
                    else
                    {
                        if (printid <= 0)
                        {
                            CommonResult result = this.certNewService.AddCert(array, sellerid, lpn, consignor, userid, outDate, purchaseno);
                            if (result.Status == (int)CommonResultStatus.Failed)
                            {
                                return new ResponseModel(ApiResponseStatus.Failed, result.Message, result.Reason);
                            }

                            // 获取打印序号
                            printno = result.Data.ToString();
                        }
                        else
                        {
                            var printlog = this.db.SalePrintlogNew.FirstOrDefault(s => s.Id == printid);
                            if (printlog != null)
                            {
                                printno = printlog.Printno;

                                if (printlog.Status == 0)
                                {
                                    // 更新出库的车牌号和收货单位、还有打印的数量
                                    printlog.Consignor = consignor;
                                    printlog.Adder = userid;
                                    this.db.SalePrintlogNew.Update(printlog);

                                    this.db.SaveChanges();
                                }
                                else if (printlog.Status == 1)
                                {
                                    // 已经打印过的不允许更新，直接返回原照片
                                    JObject retdata = new JObject();
                                    retdata["printid"] = printid;
                                    retdata["printno"] = printlog.Printno;
                                    retdata["printcode"] = printlog.Checkcode;

                                    return new ResponseModel(ApiResponseStatus.Success, "生成成功", JsonConvert.SerializeObject(retdata));
                                }
                                else if (printlog.Status == 10)
                                {
                                    // 已经撤销过的不允许更新，直接返回原照片
                                    JObject retdata = new JObject();
                                    retdata["printid"] = printid;
                                    retdata["printno"] = printlog.Printno;
                                    retdata["printcode"] = printlog.Checkcode;

                                    return new ResponseModel(ApiResponseStatus.Success, "生成成功", JsonConvert.SerializeObject(retdata));
                                }
                            }
                            else
                            {
                                return new ResponseModel(ApiResponseStatus.Failed, "数据错误", "打印记录不存在！");
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(printno))
                    {
                        string certsavepath = this.hostingEnvironment.ContentRootPath + "/qualitypics/";

                        // 生成质保书
                        CommonResult retresult = this.certNewService.GenerateCert(printno, certsavepath, (iswater == 0) ? false : true);
                        if (retresult.Status == (int)CommonResultStatus.Failed)
                        {
                            return new ResponseModel(ApiResponseStatus.Failed, retresult.Message, retresult.Reason);
                        }
                        else
                        {
                            return new ResponseModel(ApiResponseStatus.Success, retresult.Message, JsonConvert.SerializeObject(retresult.Data));
                        }
                    }

                    return new ResponseModel(ApiResponseStatus.Failed, "数据错误", "生成的序列号有误");
                }

                return new ResponseModel(ApiResponseStatus.Failed, "生成失败", "参数有误");
            }
            catch (Exception ex)
            {
                this.logService.LogError(ex.Message, ex);
                return new ResponseModel(ApiResponseStatus.Failed, "生成失败", ex.Message);
            }

        }
    }
}