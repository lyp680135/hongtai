namespace WarrantyApiCenter.Controllers.V1
{
    using System.Linq;
    using Common.IService;
    using Common.Service;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using WarrantyApiCenter.Models;
    using static DataLibrary.EnumList;

    /// <summary>
    /// WPF请求备用
    /// </summary>
    [Produces("application/json")]
    public class CertController : BaseController
    {
        private readonly IHostingEnvironment hostingEnvironment;
        private DataLibrary.DataContext db;
        private IUserService userService;
        private ICertService certService;

        public CertController(DataLibrary.DataContext dataContext, IUserService user, ICertService cert, IHostingEnvironment hostingEnvironment)
        {
            this.db = dataContext;
            this.userService = user;
            this.certService = cert;
            this.hostingEnvironment = hostingEnvironment;
        }

        [HttpGet]
        [AllowAnonymous]
        public ResponseModel Get(int id)
        {
            if (id > 0)
            {
                var printlog = this.db.SalePrintlog.Where(s => s.Id == id).FirstOrDefault();

                return new ResponseModel(ApiResponseStatus.Success, string.Empty, JsonConvert.SerializeObject(printlog));
            }
            else
            {
                var printlog = this.db.SalePrintlog.Where(s => s.Status == 1).OrderByDescending(s => s.Createtime).FirstOrDefault();

                return new ResponseModel(ApiResponseStatus.Success, string.Empty, JsonConvert.SerializeObject(printlog));
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public ResponseModel Get(string printno)
        {
            if (!string.IsNullOrEmpty(printno))
            {
                var printlog = this.db.SalePrintlog.Where(s => s.Printno == printno).FirstOrDefault();

                return new ResponseModel(ApiResponseStatus.Success, string.Empty, JsonConvert.SerializeObject(printlog));
            }

            return null;
        }

        [HttpPost]
        [AllowAnonymous]
        public ResponseModel Post(string requestData)
        {
            // {"list":"[{\"Batcode\":\"A18050011\",\"Classid\":1,\"Classname\":null,\"Materialid\":1,\"Materialname\":\"HRB400\",\"Specid\":1,\"Length\":\"L\",\"Lengthtype\":1,\"Lengthnote\":\"非尺\",\"Specfullname\":\"Ф12x9\",\"Number\":2,\"Printnumber\":2}]","lpn":"江XXX","sellerid":1,"consignor":""}
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
                        CommonResult result = this.certService.AddCert(array, sellerid, lpn, consignor, userid);
                        if (result.Status == (int)CommonResultStatus.Failed)
                        {
                            return new ResponseModel(ApiResponseStatus.Failed, result.Message, result.Reason);
                        }

                        // 获取打印序号
                        printno = result.Data.ToString();
                    }
                    else
                    {
                        var printlog = this.db.SalePrintlog.FirstOrDefault(s => s.Id == printid);
                        if (printlog != null)
                        {
                            printno = printlog.Printno;

                            if (printlog.Status == 0)
                            {
                                // 更新出库的车牌号和收货单位、还有打印的数量
                                printlog.Consignor = consignor;
                                printlog.Adder = userid;
                                this.db.SalePrintlog.Update(printlog);

                                if (this.db.SaveChanges() > 0)
                                {
                                    var detail = this.db.SalePrintLogDetail.FirstOrDefault(s => s.PrintId == printlog.Id);
                                    if (detail != null)
                                    {
                                        var auth = this.db.SaleSellerAuth.FirstOrDefault(s => s.Id == detail.Authid);
                                        if (auth != null)
                                        {
                                            auth.Lpn = lpn;

                                            this.db.SaleSellerAuth.Update(auth);
                                            if (this.db.SaveChanges() <= 0)
                                            {
                                                return new ResponseModel(ApiResponseStatus.Failed, "数据错误", "更新出库车牌号时发生错误！");
                                            }
                                        }
                                        else
                                        {
                                            return new ResponseModel(ApiResponseStatus.Failed, "数据错误", "经销商授权信息有误！");
                                        }

                                        // 更新打印数量
                                        for (var i = 0; i < array.Count; i++)
                                        {
                                            if (auth.Batcode == array[i]["Batcode"].ToString())
                                            {
                                                int.TryParse(array[i]["Printnumber"].ToString(), out int printnumber);
                                                detail.Printnumber = printnumber;

                                                this.db.SalePrintLogDetail.Update(detail);
                                                if (this.db.SaveChanges() <= 0)
                                                {
                                                    return new ResponseModel(ApiResponseStatus.Failed, "数据错误", "更新打印数量时发生错误！");
                                                }

                                                // 节约时间，找到了就不跳出
                                                break;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    return new ResponseModel(ApiResponseStatus.Failed, "数据错误", "更新收货单位时发生错误！");
                                }
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
                    CommonResult retresult = this.certService.GenerateCert(printno, certsavepath, (iswater == 0) ? false : true);
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
    }
}