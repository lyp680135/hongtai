namespace WarrantyApiCenter.Controllers.V1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Common.IService;
    using Common.Service;
    using DataLibrary;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Util;
    using WarrantyApiCenter.Models;
    using static DataLibrary.EnumList;

    /// <summary>
    /// 产品
    /// </summary>
    [Produces("application/json")]
    public class ProductController : BaseController
    {
        private DataLibrary.DataContext db;
        private IUserService userService;
        private IStockOutService stockoutService;

        public ProductController(DataLibrary.DataContext dataContext, IUserService user, IStockOutService stock)
        {
            this.db = dataContext;
            this.userService = user;
            this.stockoutService = stock;
        }

        [HttpGet]
        public ResponseModel Get()
        {
            // 只获取当前登录人员所属车间的轧制批号
            if (this.userService.IsManageMember)
            {
                var userid = string.Format("{0}", this.userService.ApplicationUser.Mng_admin.Id);

                // 获取当前出库员用户所在的车间代码
                var workshop = this.db.PdWorkshop.Where(p => p.Outputer.Contains(userid)).FirstOrDefault();
                if (workshop != null)
                {
                    var batcodes = this.db.PdBatcode.Where(p => p.Workshopid == workshop.Id).OrderByDescending(c => c.Id).ToList();
                    if (batcodes != null)
                    {
                        return new ResponseModel(ApiResponseStatus.Success, string.Empty, JsonConvert.SerializeObject(batcodes));
                    }
                    else
                    {
                        return new ResponseModel(ApiResponseStatus.Failed, "轧制批号记录为空", string.Empty);
                    }
                }
            }

            return new ResponseModel(ApiResponseStatus.Failed, "没有权限", string.Empty);
        }

        [HttpGet("{batcode}")]
        public ResponseModel Get(string batcode)
        {
            if (this.userService.IsManageMember)
            {
                var userid = string.Format("{0}", this.userService.ApplicationUser.Mng_admin.Id);

                // 获取当前出库员用户所在的车间代码
                var workshop = this.db.PdWorkshop.Where(p => p.Outputer.Contains(userid)).FirstOrDefault();
                if (workshop != null)
                {
                    var product = this.db.PdProduct.Where(p => p.Batcode == batcode).Join(
                        this.db.BaseProductMaterial, p => p.Materialid, m => m.Id, (p, m) => new
                        {
                            batcode = p.Batcode,
                            classid = p.Classid,
                            materialid = p.Materialid,
                            materialname = m.Name,
                            specid = p.Specid,
                        }).Join(this.db.BaseSpecifications, p => p.specid, s => s.Id, (p, s) => new
                        {
                            batcode = p.batcode,
                            classid = p.classid,
                            materialid = p.materialid,
                            materialname = p.materialname,
                            specid = p.specid,
                            specname = s.Specname,
                            pieceweight = s.Referpieceweight,
                        }).FirstOrDefault();

                    if (product != null)
                    {
                        return new ResponseModel(ApiResponseStatus.Success, string.Empty, JsonConvert.SerializeObject(product));
                    }
                    else
                    {
                        return new ResponseModel(ApiResponseStatus.Failed, "该轧制批号下产品记录为空", string.Empty);
                    }
                }
            }

            return new ResponseModel(ApiResponseStatus.Failed, "没有权限", string.Empty);
        }

        [HttpPost]
        public ResponseModel Post([FromForm]string value)
        {
            var requestmodel = new { lpn = string.Empty, sellerid = string.Empty, deliveryType = 0, list = string.Empty };
            var data = JsonConvert.DeserializeAnonymousType(value.Base64ToString(), requestmodel);

            int.TryParse(data.sellerid, out int sellerid);

            if (string.IsNullOrEmpty(data.lpn) || sellerid <= 0)
            {
                return new ResponseModel(ApiResponseStatus.Failed, "请输入车牌和售达方和尺码", string.Empty);
            }

            int success_nums = 0;
            if (data != null)
            {
                JArray list = (JArray)JsonConvert.DeserializeObject(data.list);
                if (list.Count <= 0)
                {
                    return new ResponseModel(ApiResponseStatus.Failed, "请输入出库产品", string.Empty);
                }

                var mngsetInfo = this.db.MngSetting.FirstOrDefault();

                // 流程版必需输了质量数据且通过了审核才能出库
                if (mngsetInfo.SystemVersion == EnumList.SystemVersion.流程版本)
                {
                    foreach (var item in list)
                    {
                        // 判断该炉号是否已经做好质量录入
                        var quality = this.db.PdQuality.Where(p => p.Batcode == item["batcode"].ToString() && p.CheckStatus == CheckStatus_PdQuality.审核通过).FirstOrDefault();
                        if (quality == null)
                        {
                            return new ResponseModel(ApiResponseStatus.Failed, "出库失败了，所选批号[" + item["batcode"].ToString() + "]还没有通过质量检测。", string.Empty);
                        }
                    }
                }
                else if (mngsetInfo.SystemVersion == EnumList.SystemVersion.简单版本)
                {
                    foreach (var item in list)
                    {
                        var product = this.db.PdProduct.Where(p => p.Batcode == item["batcode"].ToString()).FirstOrDefault();
                        if (product != null)
                        {
                            int materialid = product.Materialid.Value;

                            // 判断系统是否有预置数据
                            var presetdata = this.db.PdQuality.Where(p => p.MaterialId == materialid && p.CreateFlag == 1).Count();
                            if (presetdata > 0)
                            {
                                // 判断该炉号是否已经匹配到预置质量数据
                                var quality = this.db.PdQualityProductPreset.Where(p => p.Batcode == item["batcode"].ToString()).FirstOrDefault();
                                if (quality == null)
                                {
                                    return new ResponseModel(ApiResponseStatus.Failed, "出库失败了，所选批号[" + item["batcode"].ToString() + "]还没有质量检测。", string.Empty);
                                }
                            }
                            else
                            {
                                return new ResponseModel(ApiResponseStatus.Failed, "出库失败了，所选批号[" + item["batcode"].ToString() + "]找不到质量检测。", string.Empty);
                            }
                        }
                        else
                        {
                            return new ResponseModel(ApiResponseStatus.Failed, "出库失败了，所选批号[" + item["batcode"].ToString() + "]中没有找到产品。", string.Empty);
                        }
                    }
                }

                foreach (var item in list)
                {
                    var batcode = item["batcode"].ToString();
                    var startbundle = item["startbundle"].ToString();
                    var endbundle = item["endbundle"].ToString();

                    // 如果是非尺出库
                    if (data.deliveryType != (int)ProductQualityLevel.定尺)
                    {
                        startbundle = startbundle.Replace("F", string.Empty);
                        endbundle = endbundle.Replace("F", string.Empty);
                    }

                    int.TryParse(startbundle, out int start);
                    int.TryParse(endbundle, out int end);

                    if (end - start >= 0)
                    {
                        // TODO 改成公共的服务
                        // CommonResult result = this.stockoutService.Stockout(batcode, data.lpn, sellerid, data.deliveryType, end - start);
                        var mdl = new StockoutModel();
                        int result = mdl.Stockout(this.db, batcode, data.lpn, sellerid, start, end, (DataLibrary.EnumList.ProductQualityLevel)data.deliveryType);
                        success_nums += result;
                    }
                }
            }

            if (success_nums > 0)
            {
                return new ResponseModel(ApiResponseStatus.Success, string.Empty, success_nums.ToString());
            }
            else
            {
                return new ResponseModel(ApiResponseStatus.Failed, "出库失败了，产品不存在或已出库", string.Empty);
            }
        }

        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
