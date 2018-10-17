namespace WarrantyApiCenter.Controllers.V1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Common.IService;
    using Common.Service;
    using DataLibrary;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using WarrantyApiCenter.Models;
    using static DataLibrary.EnumList;

    [Produces("application/json")]
    public class WarrantyController : BaseController
    {
        private DataLibrary.DataContext db;
        private IUserService userService;
        private IQualityService qualityService;
        private ICertService certService;

        public WarrantyController(DataLibrary.DataContext dataContext, IUserService user, IQualityService quality, ICertService certService)
        {
            this.db = dataContext;
            this.userService = user;
            this.qualityService = quality;
            this.certService = certService;
        }

        // GET: api/Warranty/5
        [HttpGet]
        [AllowAnonymous]
        public ResponseModel Get(string p)
        {
            if (string.IsNullOrEmpty(p))
            {
                return new ResponseModel(ApiResponseStatus.Failed, "查询质保书失败", "打印编号为空！");
            }

            var result = this.certService.GetCertData(p);
            if (result.Status == (int)CommonResultStatus.Success)
            {
                return new ResponseModel(ApiResponseStatus.Success, "查询成功", result.Data.ToString());
            }
            else
            {
                return new ResponseModel(ApiResponseStatus.Failed, "未找到相关记录", string.Empty);
            }
        }

        /// <summary>
        /// 生成质保书记录
        /// </summary>
        /// <param name="requestData">准备输出到质保书上的产品清单</param>
        /// <returns>返回生成的质保书编号</returns>
        [HttpPost]
        public ResponseModel Post(string requestData)
        {
            if (string.IsNullOrEmpty(requestData))
            {
                return new ResponseModel(ApiResponseStatus.Failed, "打印记录保存失败", "缺少参数！");
            }

            JObject jo = (JObject)JsonConvert.DeserializeObject(requestData);
            if (jo == null)
            {
                return new ResponseModel(ApiResponseStatus.Failed, "打印记录保存失败", "缺少参数！");
            }

            if (jo["list"] == null)
            {
                return new ResponseModel(ApiResponseStatus.Failed, "打印记录保存失败", "产品物资不能为空！");
            }

            JArray jarr = (JArray)JsonConvert.DeserializeObject(jo["list"].ToString());
            if (jarr == null || jarr.Count == 0)
            {
                return new ResponseModel(ApiResponseStatus.Failed, "打印记录保存失败", "产品物资不能为空！");
            }

            try
            {
                var user = this.userService.SaleSellerUser;

                List<DataLibrary.SalePrintLogDetail> productinfos = new List<DataLibrary.SalePrintLogDetail>();

                int findedprintid = 0;
                int findsamecount = 0;

                // 生成一张质保书下的产品分组列表
                foreach (JToken jt in jarr)
                {
                    int.TryParse(jt["Id"].ToString(), out int authid);
                    int.TryParse(jt["SelectedNumber"].ToString(), out int number);

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
                        Printnumber = number,
                    });
                }

                // 如果找到的记录完全一样，则应该直接调用原来的质保书即可
                if (findsamecount == jarr.Count)
                {
                    var log = this.db.SalePrintlog.Where(s => s.Id == findedprintid && (s.Status == null || s.Status == 0)).FirstOrDefault();
                    if (log != null)
                    {
                        if (log.Consignor != jo["companyname"].ToString())
                        {
                            // 更新收货单位
                            log.Consignor = jo["companyname"].ToString();
                            this.db.SalePrintlog.Update(log);

                            if (this.db.SaveChanges() > 0)
                            {
                                return new ResponseModel(ApiResponseStatus.Success, "打印记录保存成功", log.Printno);
                            }
                            else
                            {
                                return new ResponseModel(ApiResponseStatus.Failed, "打印记录保存失败", "更新收货单位失败！");
                            }
                        }

                        return new ResponseModel(ApiResponseStatus.Success, "沿用上次没有生成的质保书号", log.Printno);
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
                    Consignor = jo["companyname"].ToString(),
                    Printno = pno,
                    Signetangle = new Random().Next(-45, 45),
                    Checkcode = checkcode,
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
                            return new ResponseModel(ApiResponseStatus.Success, "打印记录保存成功", pno);
                        }
                        else
                        {
                            // 回滚操作
                            tran.Rollback();

                            return new ResponseModel(ApiResponseStatus.Failed, "打印记录保存失败", "数据处理有误，已作回滚操作！");
                        }
                    }
                }

                return new ResponseModel(ApiResponseStatus.Failed, "打印记录保存失败", "预览模版失败，数据处理有误！");
            }
            catch (Exception ex)
            {
                return new ResponseModel(ApiResponseStatus.Failed, "打印记录保存失败", ex.Message);
            }
        }

        // PUT: api/Warranty/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
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
    }
}
