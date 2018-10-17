namespace WarrantyManage.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Common.IService;
    using Common.Service;
    using DataLibrary;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Models;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using OfficeOpenXml;
    using OfficeOpenXml.Style;
    using Util;
    using Util.Helpers;
    using static DataLibrary.EnumList;

    [Authorize]
    public class QualityController : Controller
    {
        private const int Count = 4;

        /// <summary>
        /// 图片存放根目录
        /// </summary>
        private readonly string rootPath = "/UploadFile/upLoadXlsx/";
        private DataContext db;

        private IUserService userService;

        private IBaseService<SalePrintlog> salePrintlog;

        private ILogService logService;

        private IBaseService<PdQuality> baseQuality;

        private IHostingEnvironment hostingEnvironment;

        public QualityController(DataContext db, IUserService userService, IBaseService<SalePrintlog> salePrintlog, ILogService logService, IBaseService<PdQuality> baseQuality, IHostingEnvironment hostingEnvironment)
        {
            this.db = db;
            this.userService = userService;
            this.salePrintlog = salePrintlog;
            this.logService = logService;
            this.baseQuality = baseQuality;
            this.hostingEnvironment = hostingEnvironment;
        }

        public PdBatcode SingleNextById(int id, string shopcode)
        {
            return this.db.PdBatcode.OrderBy(c => c.Id).FirstOrDefault(c => c.Id > id && c.Batcode.EndsWith(shopcode));
        }

        public PdBatcode SingleByPrefixCode(string prefixCode)
        {
            return this.db.PdBatcode.OrderByDescending(c => c.Id).Where(c => c.Batcode.Contains(prefixCode)).FirstOrDefault();
        }

        public PdBatcode SingleByBatcode(string batcode)
        {
            return this.db.PdBatcode.OrderByDescending(c => c.Id).FirstOrDefault(c => c.Batcode == batcode) ?? new PdBatcode();
        }

        public PdBatcode SingleLast(string shopcode)
        {
            return this.db.PdBatcode.OrderByDescending(c => c.Id).FirstOrDefault(c => c.Batcode.EndsWith(shopcode)) ?? new PdBatcode();
        }

        /// <summary>
        /// 获取炉批号
        /// </summary>
        /// <param name="curcode">当前炉批号</param>
        /// <param name="offset">偏移量</param>
        /// <param name="wid" >车间id</param>
        /// <returns>stirng</returns>
        public string GetPatCodeNew(string curcode, int offset, int wid)
        {
            var batcode = curcode;

            var current = this.db.PdBatcode.FirstOrDefault(f => f.Batcode == curcode);
            if (current != null)
            {
                var productInfo = new PdBatcode();
                var serialno = current.Serialno;
                if (offset > 0)
                {
                    productInfo = this.db.PdBatcode.Where(w => w.Workshopid == wid).OrderBy(o => o.Serialno).FirstOrDefault(f => f.Serialno > serialno);
                }
                else
                {
                    productInfo = this.db.PdBatcode.Where(w => w.Workshopid == wid).OrderByDescending(o => o.Serialno).FirstOrDefault(f => f.Serialno < serialno);
                }

                if (productInfo != null)
                {
                    batcode = productInfo.Batcode;
                }
            }

            return JsonConvert.SerializeObject(new ResponseModel(EnumList.ApiResponseStatus.Success, string.Empty, batcode));
        }

        /// <summary>
        /// 获取炉批号(炉批号生成算法)
        /// </summary>
        /// <param name="curcode">当前炉批号，无则传空</param>
        /// <param name="offset">上下相加的数（增加则为1，减少为-1）</param>
        /// <param name="workShopCode">workShopCode</param>
        /// <returns>GetPatCode</returns>
        [HttpPost]
        public string GetPatCode(string curcode, int offset, string workShopCode)
        {
            string batcode = string.Empty;

            // 第一位为生产线
            string fIRST_WORD = workShopCode; // myWorkshopProductLine;

            if (string.IsNullOrWhiteSpace(fIRST_WORD))
            {
                var userId = this.userService.ApplicationUser.Mng_admin.Id;

                var query = this.db.PdWorkshop.AsEnumerable().Where(c => c.QAInputer.Split(',').Contains(userId.ToString())).FirstOrDefault();
                if (query != null && !string.IsNullOrWhiteSpace(query.Code))
                {
                    fIRST_WORD = query.Code;
                }
                else
                {
                    return JsonConvert.SerializeObject(new ResponseModel(EnumList.ApiResponseStatus.Failed, "当前登录人员找不到所属车间", string.Empty));
                }
            }

            string dATE_CODE = DateTime.Now.ToString("yyMM");

            // 查询数据库里最后一条批号
            PdBatcode curritem = this.SingleLast(fIRST_WORD);

            // 如果是刚打开程序初始读取批号
            if (string.IsNullOrEmpty(batcode) && offset == 0)
            {
                if (curritem != null)
                {
                    batcode = curritem.Batcode;
                    return JsonConvert.SerializeObject(new ResponseModel(EnumList.ApiResponseStatus.Success, string.Empty, batcode));
                }
            }

            // 根据当前批号从数据库获取批号记录标识，以方便找上一条下一条
            if (!string.IsNullOrEmpty(curcode))
            {
                curritem = this.SingleByBatcode(curcode);
            }

            // 如果当前批号为空并且数据库里也没有
            if (curritem.Id <= 0)
            {
                batcode = string.Format("{0}{1}{2}", fIRST_WORD, dATE_CODE, 1.ToString().PadLeft(4, '0'));

                return JsonConvert.SerializeObject(new ResponseModel(EnumList.ApiResponseStatus.Success, string.Empty, batcode));
            }
            else
            {
                if (offset > 0)
                {
                    var pdcode = this.SingleNextById(curritem.Id, fIRST_WORD);

                    if (pdcode != null)
                    {
                        return JsonConvert.SerializeObject(new ResponseModel(EnumList.ApiResponseStatus.Success, string.Empty, pdcode.Batcode));
                    }
                }
            }

            // 分隔字符串
            string code = curritem.Batcode.Replace(fIRST_WORD, string.Empty);
            string datestr = code.Substring(0, 4);
            string numberstr = code.Replace(datestr, string.Empty);
            numberstr = numberstr.Substring(0, numberstr.Length - 1);

            // 如果是当月，则在批号后做运算
            if (datestr == dATE_CODE)
            {
                int number = int.Parse(numberstr);
                number += offset;

                if (number > 0)
                {
                    batcode = string.Format("{0}{1}{2}", fIRST_WORD, dATE_CODE, number.ToString().PadLeft(4, '0'));

                    // 如果是下一个，则为新生成的炉号，需要插入到数据库中
                    if (offset > 0)
                    {
                        if (curritem.Status == 1)
                        {
                            batcode = curritem.Batcode;
                        }
                        else
                        {
                            // 如果当前批号没有已使用，则仍然使用当前批号，不能跳过
                            batcode = curritem.Batcode;
                        }
                    }
                }

                // 如果炉号到0，则表示已经到了上月最后一个炉号
                else
                {
                    string preMonthCode = DateTime.Now.AddMonths(-1).ToString("yyMM");
                    string prefix = string.Format("{0}%", preMonthCode);

                    var pdcode = this.SingleByPrefixCode(prefix);

                    if (pdcode != null)
                    {
                        batcode = pdcode.Batcode;
                    }
                    else
                    {
                        batcode = curritem.Batcode;
                    }

                    // 如果上个月的批号没有，则继续保持当前批号，即返回空值
                }
            }

            // 如果不是当月，则为历史记录，直接在数据库里取上一个和下一个
            else
            {
                if (offset > 0)
                {
                    var pdcode = this.SingleNextById(curritem.Id, fIRST_WORD);

                    if (pdcode != null)
                    {
                        batcode = pdcode.Batcode;
                    }

                    // 如果数据库没有，则生成新的
                    // 如果当前批号为空并且数据库里也没有
                    if (string.IsNullOrEmpty(batcode))
                    {
                        batcode = string.Format("{0}{1}{2}", fIRST_WORD, dATE_CODE, 1.ToString().PadLeft(4, '0'));

                        if (curritem.Status == 1)
                        {
                            batcode = curritem.Batcode;
                        }
                        else
                        {
                            // 如果当前批号没有已使用，则仍然使用当前批号，不能跳过
                            batcode = curritem.Batcode;
                        }
                    }
                }
                else
                {
                    var pdcode = this.SinglePrevById(curritem.Id, fIRST_WORD);
                    if (pdcode != null)
                    {
                        return JsonConvert.SerializeObject(new ResponseModel(EnumList.ApiResponseStatus.Success, string.Empty, pdcode.Batcode));
                    }

                    // 如果上一条的批号没有，则继续保持当前批号，即返回空值
                }
            }

            return JsonConvert.SerializeObject(new ResponseModel(EnumList.ApiResponseStatus.Success, string.Empty, batcode));
        }

        [HttpPost]
        public ActionResult Exists(string batCode)
        {
            if (!string.IsNullOrEmpty(batCode))
            {
                int count = this.CheckBatRecored(batCode).Count;

                // 先判断在 pdProduct里面有没有 该炉批号，没有的话，
                if (count > 0)
                {
                    return this.AjaxResult(true, "提示：该炉批号已添加过 " + count + " 条质量纪录");
                }
                else if (this.GetMaterialSpecNameByBatCode(batCode) == null)
                {
                    return this.AjaxResult(false, "提示:该炉批号生产纪录中不存在");
                }
            }

            return this.AjaxResult(false, string.Empty);
        }

        /// <summary>
        /// 提交数据之前做校验
        /// </summary>
        /// <param name="pId">batCode</param
        /// <param name="materialid">materialid</param>
        /// <param name=batCode">炉批号</param>
        /// <param name=smeltCode">冶炼id</param>
        /// <returns>View</returns>
        public ActionResult Check(int pId = 0, int materialid = 0, string batCode = "", string smeltCode = "")
        {
            PdProduct productInfo = null;
            List<BaseQualityStandard> listQualityStandards = null;
            List<BaseQualityStandard> dxQualityStandards = null;
            if (materialid <= 0)
            {
                if (!string.IsNullOrEmpty(batCode))
                {
                    productInfo = this.db.PdProduct.FirstOrDefault(f => f.Batcode == batCode);
                }
                else
                {
                    var pdQuality = this.db.PdQuality.FirstOrDefault(f => f.Id == pId);
                    productInfo = this.db.PdProduct.FirstOrDefault(f => f.Batcode == pdQuality.Batcode);
                    if (pdQuality == null)
                    {
                        return this.AjaxResult(false, "数据不存在无法编辑");
                    }
                }

                if (productInfo == null)
                {
                    return this.Json(new { flag = "2", msg = "不存在该编号的钢炉" });
                }

                listQualityStandards = this.db.BaseQualityStandard.Where(w => w.TargetType == 0 && w.Status == 0 && w.Materialid == productInfo.Materialid).ToList();
                dxQualityStandards = this.db.BaseQualityStandard.Where(w => w.TargetType == 1 && w.Status == 0 && w.Materialid == productInfo.Materialid).ToList();
            }
            else
            {
                //if (smeltCode > 0)
                //{
                //    var smeltInfo = this.db.PdSmeltCode.FirstOrDefault(f => f.Id == smeltCode);
                //    if (smeltInfo != null)
                //    {
                //        var pdquality = this.db.PdQuality.FirstOrDefault(f => f.Id == smeltInfo.Qid);
                //        materialid = pdquality.MaterialId.ToInt();
                //    }
                //}

                listQualityStandards = this.db.BaseQualityStandard.Where(w => w.TargetType == 0 && w.Status == 0 && w.Materialid == materialid).ToList();
                dxQualityStandards = this.db.BaseQualityStandard.Where(w => w.TargetType == 1 && w.Status == 0 && w.Materialid == materialid).ToList();
            }

            List<string> targetNameList = new List<string>();
            if (listQualityStandards.Count > 0)
            {
                // 单行元素比较
                foreach (var item in listQualityStandards)
                {
                    var val = this.Request.Form[item.TargetName].ToString();
                    if ((item.TargetIsNull == 0 && !string.IsNullOrEmpty(val)) || item.TargetIsNull == 1)
                    {
                        // 如果满足条件,说明输入的值超标
                        if (item.TargetMin.ToDouble(2) > val.ToDouble(2)
                                || item.TargetMax.ToDouble(2) < val.ToDouble(2))
                        {
                            targetNameList.Add(item.TargetName);
                        }
                    }
                }
            }

            if (dxQualityStandards.Count > 0)
            {
                // 多行元素比较
                foreach (var item in dxQualityStandards)
                {
                    // 多行元素比较3次,
                    for (int i = 0; i < Count; i++)
                    {
                        var val = this.Request.Form[item.TargetName + i].ToString();
                        if ((item.TargetIsNull == 0 && !string.IsNullOrEmpty(val)) || item.TargetIsNull == 1)
                        {
                            if (item.TargetMin.ToDouble(2) > val.ToDouble(2)
                           || item.TargetMax.ToDouble(2) < val.ToDouble(2))
                            {
                                // 如果集合没有就添加
                                if (!targetNameList.Any(x => x == item.TargetName) && val.SafeString() != string.Empty)
                                {
                                    targetNameList.Add(item.TargetName + i.ToString());
                                }
                            }
                        }
                    }
                }
            }

            if (targetNameList != null && targetNameList.Count > 0)
            {
                return this.Json(new { flag = "false", msg = targetNameList });
            }

            return this.Json(new { flag = "true", msg = string.Empty });
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="id">id</param>
        /// <returns>ajax</returns>
        public ActionResult DeleteQuality(int id)
        {
            try
            {
                var pdQualityInfo = this.db.PdQuality.AsNoTracking().FirstOrDefault(f => f.Id == id);
                if (pdQualityInfo == null)
                {
                    return this.AjaxResult(false, "不存在该指标数据,无法删除");
                }

                this.db.Remove(pdQualityInfo);
                this.db.SaveChanges();
                return this.AjaxResult(true, "删除成功");
            }
            catch
            {
                return this.AjaxResult(false, "删除失败");
            }
        }

        /// <summary>
        /// 修改模板
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>string</returns>
        public string EditQuality(int id)
        {
            try
            {
                var pdQualityInfo = this.db.PdQuality.FirstOrDefault(f => f.Id == id);
                if (pdQualityInfo == null)
                {
                    return "不存在该模板,无法修改";
                }

                int materialid = pdQualityInfo.MaterialId.ToInt();
                Regex regNum = new Regex("\\d+");
                List<string> list_ma_spec = this.GetMaterialSpecName(materialid);
                int materialNameNum = regNum.Match(list_ma_spec[0]).Value.ToInt();
                if (materialid <= 0)
                {
                    return "请选择材质";
                }

                var listQualityStandards = this.db.BaseQualityStandard.Where(w => w.TargetType == 0 && w.Status == 0 && w.Materialid == materialid).ToList();
                if (listQualityStandards == null || listQualityStandards.Count <= 0)
                {
                    return "该批号下的产品没有设置好可以参考的单样本质量指标";
                }

                // 单行元素字典
                Dictionary<string, double> keyValuePairs = new Dictionary<string, double>();
                foreach (var item in listQualityStandards)
                {
                    keyValuePairs.Add(item.TargetName, this.Request.Form[item.TargetName].ToDouble());
                }

                List<string> targetList = new List<string>()
                    {
                        "C", "Mn", "V", "Mo", "Cu", "Ni",
                    };
                var keyPairs = keyValuePairs.Keys.ToList();
                List<string> targetName = new List<string>();
                foreach (var item in targetList)
                {
                    // 判断是否缺少元素
                    if (!keyPairs.Any(x => x == item))
                    {
                        targetName.Add(item);
                    }
                }

                double ceq = 0;

                // 如果少指标就返回
                if (targetName.Count > 0)
                {
                    keyValuePairs.Add("Ceq", ceq);
                }
                else
                {
                    ceq = (keyValuePairs["C"] + (keyValuePairs["Mn"] / 6) +
                    ((keyValuePairs["C"] + keyValuePairs["V"] + keyValuePairs["Mo"]) / 5) +
                    ((keyValuePairs["Cu"] = keyValuePairs["Ni"]) / 15)).ToDouble(2);
                    keyValuePairs.Add("Ceq", ceq);
                }

                // List<PdQualityInfo_Dynamics> list_PdQualityInfo_Dynamics = new List<PdQualityInfo_Dynamics>();
                // 多行元素集合
                List<object> keyValues = new List<object>();
                var dxQualityStandards = this.db.BaseQualityStandard.Where(w => w.TargetType == 1 && w.Status == 0 && w.Materialid == materialid).ToList();
                if (dxQualityStandards.Count > 0)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        if (dxQualityStandards == null || dxQualityStandards.Count <= 0)
                        {
                            return "该批号下的产品没有设置好可以参考的多样本质量指标";
                        }

                        Dictionary<string, object> keyValue = new Dictionary<string, object>();
                        foreach (var item in dxQualityStandards)
                        {
                            keyValue.Add(item.TargetName, this.Request.Form[item.TargetName + i].ToDouble(2));
                        }

                        // 多行元素必要字段校验
                        List<string> targetdxList = new List<string>()
                            {
                                "下屈服强度", "抗拉强度", "伸长率A", "伸长率Agt"
                            };
                        var keyS = keyValue.Keys.ToList();
                        List<string> targetdxName = new List<string>();
                        foreach (var item in targetdxList)
                        {
                            // 判断是否缺少元素
                            if (!keyS.Any(x => x == item))
                            {
                                targetdxName.Add(item);
                            }
                        }

                        double qqb = 0;
                        double qiangqb = 0;

                        // 如果少指标就返回
                        if (targetdxName.Count > 0)
                        {
                            if (list_ma_spec[0].ToUpper().EndsWith("E"))
                            {
                                if (!keyValue.Keys.Any(a => a == "强屈比"))
                                {
                                    keyValue.Add("强屈比", qiangqb);
                                }

                                if (!keyValue.Keys.Any(b => b == "屈屈比"))
                                {
                                    keyValue.Add("屈屈比", qqb);
                                }
                            }
                        }
                        else
                        {
                            if (list_ma_spec[0].ToUpper().EndsWith("E"))
                            {
                                if (!keyValue.Keys.Any(a => a == "屈屈比"))
                                {
                                    qqb = (keyValue["下屈服强度"].ToDouble() / materialNameNum).ToDouble(2);
                                    keyValue.Add("屈屈比", qqb);
                                }

                                // 强屈比计算公式
                                if (!keyValue.Keys.Any(b => b == "强屈比"))
                                {
                                    qiangqb = (keyValue["抗拉强度"].ToDouble() / keyValue["下屈服强度"].ToDouble()).ToDouble(2);
                                    keyValue.Add("强屈比", qiangqb);
                                }
                            }
                        }

                        // 如果材质带E结尾的，则计算SS
                        keyValue.Add("冷弯弯心", string.Empty);
                        keyValue.Add("冷弯", string.Empty);
                        keyValue.Add("反弯弯心", string.Empty);
                        keyValue.Add("反弯", string.Empty);
                        keyValues.Add(keyValue);
                    }
                }

                pdQualityInfo.Qualityinfos = keyValuePairs;
                pdQualityInfo.Qualityinfos_Dynamics = keyValues;
                this.db.Update(pdQualityInfo);
                this.db.SaveChanges();
                return "true";
            }
            catch
            {
                return "修改失败，请检查录入的数据";
            }
        }

        /// <summary>
        /// 添加模板
        /// </summary>
        /// <param name="materialid">材质ID</param>
        /// <returns>string</returns>
        public string AddQuality(int materialid)
        {
            try
            {
                Regex regNum = new Regex("\\d+");
                List<string> list_ma_spec = this.GetMaterialSpecName(materialid);
                int materialNameNum = regNum.Match(list_ma_spec[0]).Value.ToInt();
                if (materialid <= 0)
                {
                    return "请选择材质";
                }

                var listQualityStandards = this.db.BaseQualityStandard.Where(w => w.TargetType == 0 && w.Status == 0 && w.Materialid == materialid).ToList();
                if (listQualityStandards == null || listQualityStandards.Count <= 0)
                {
                    return "该批号下的产品没有设置好可以参考的单样本质量指标";
                }

                // 单行元素字典
                Dictionary<string, double> keyValuePairs = new Dictionary<string, double>();
                foreach (var item in listQualityStandards)
                {
                    keyValuePairs.Add(item.TargetName, this.Request.Form[item.TargetName].ToDouble());
                }

                List<string> targetList = new List<string>()
                    {
                        "C", "Mn", "V", "Mo", "Cu", "Ni",
                    };
                var keyPairs = keyValuePairs.Keys.ToList();
                List<string> targetName = new List<string>();
                foreach (var item in targetList)
                {
                    // 判断是否缺少元素
                    if (!keyPairs.Any(x => x == item))
                    {
                        targetName.Add(item);
                    }
                }

                double ceq = 0;

                // 如果少指标就返回
                if (targetName.Count > 0)
                {
                    keyValuePairs.Add("Ceq", ceq);
                }
                else
                {
                    ceq = (keyValuePairs["C"] + (keyValuePairs["Mn"] / 6) +
                    ((keyValuePairs["C"] + keyValuePairs["V"] + keyValuePairs["Mo"]) / 5) +
                    ((keyValuePairs["Cu"] = keyValuePairs["Ni"]) / 15)).ToDouble(2);
                    keyValuePairs.Add("Ceq", ceq);
                }

                // List<PdQualityInfo_Dynamics> list_PdQualityInfo_Dynamics = new List<PdQualityInfo_Dynamics>();
                // 多行元素集合
                List<object> keyValues = new List<object>();
                var dxQualityStandards = this.db.BaseQualityStandard.Where(w => w.TargetType == 1 && w.Status == 0 && w.Materialid == materialid).ToList();
                if (dxQualityStandards.Count > 0)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        if (dxQualityStandards == null || dxQualityStandards.Count <= 0)
                        {
                            return "该批号下的产品没有设置好可以参考的多样本质量指标";
                        }

                        Dictionary<string, object> keyValue = new Dictionary<string, object>();
                        foreach (var item in dxQualityStandards)
                        {
                            keyValue.Add(item.TargetName, this.Request.Form[item.TargetName + i].ToDouble(2));
                        }

                        // 多行元素必要字段校验
                        List<string> targetdxList = new List<string>()
                            {
                                "下屈服强度", "抗拉强度", "伸长率A", "伸长率Agt"
                            };
                        var keyS = keyValue.Keys.ToList();
                        List<string> targetdxName = new List<string>();
                        foreach (var item in targetdxList)
                        {
                            // 判断是否缺少元素
                            if (!keyS.Any(x => x == item))
                            {
                                targetdxName.Add(item);
                            }
                        }

                        double qqb = 0;
                        double qiangqb = 0;

                        // 如果少指标就返回
                        if (targetdxName.Count > 0)
                        {
                            if (list_ma_spec[0].ToUpper().EndsWith("E"))
                            {
                                if (!keyValue.Keys.Any(a => a == "强屈比"))
                                {
                                    keyValue.Add("强屈比", qiangqb);
                                }

                                if (!keyValue.Keys.Any(b => b == "屈屈比"))
                                {
                                    keyValue.Add("屈屈比", qqb);
                                }
                            }
                        }
                        else
                        {
                            if (list_ma_spec[0].ToUpper().EndsWith("E"))
                            {
                                if (!keyValue.Keys.Any(a => a == "屈屈比"))
                                {
                                    qqb = (keyValue["下屈服强度"].ToDouble() / materialNameNum).ToDouble(2);
                                    keyValue.Add("屈屈比", qqb);
                                }

                                // 屈屈比计算公式
                                if (!keyValue.Keys.Any(b => b == "强屈比"))
                                {
                                    qiangqb = (keyValue["抗拉强度"].ToDouble() / keyValue["下屈服强度"].ToDouble()).ToDouble(2);
                                    keyValue.Add("强屈比", qiangqb);
                                }
                            }
                        }

                        // 如果材质带E结尾的，则计算SS
                        keyValue.Add("冷弯弯心", string.Empty);
                        keyValue.Add("冷弯", string.Empty);
                        keyValue.Add("反弯弯心", string.Empty);
                        keyValue.Add("反弯", string.Empty);
                        keyValues.Add(keyValue);
                    }
                }

                var userInfo = this.db.MngAdmin.Where(w => w.GroupManage.Object.Contains(3)).FirstOrDefault();
                this.db.PdQuality.Add(new PdQuality()
                {
                    Batcode = materialid.ToString(),
                    MaterialId = materialid,
                    CheckStatus = EnumList.CheckStatus_PdQuality.审核通过,
                    Createtime = (int)Util.Extensions.GetCurrentUnixTime(),
                    EntryPerson = this.userService.ApplicationUser.Mng_admin.Id,
                    CheckPerson = userInfo != null ? userInfo.Id : 0,
                    CreateFlag = 1,
                    Qualityinfos = keyValuePairs,
                    Qualityinfos_Dynamics = keyValues
                });

                this.db.SaveChanges();
                return "true";
            }
            catch
            {
                return "添加失败，请检查录入的数据";
            }
        }

        /// <summary>
        /// 添加化学数据
        /// </summary>
        /// <param name="yLCode">冶炼炉号</param>
        /// <param name="mId">材质Id</param>
        /// <returns>string</returns>
        public string ChemistryAdd(string yLCode, int mId)
        {
            if (string.IsNullOrWhiteSpace(yLCode))
            {
                return "冶炼炉号不能为空";
            }

            var chemistryInfo = this.db.PdSmeltCode.FirstOrDefault(f => f.SmeltCode == yLCode && f.Status == 0);
            if (chemistryInfo != null)
            {
                return "已存在该冶炼炉号化学数据,无法重复添加";
            }

            var listQualityStandards = this.db.BaseQualityStandard.Where(w => w.TargetType == 0 && w.Status == 0 && w.Materialid == mId && w.TargetCategory == EnumList.TargetCategory.化学指标).ToList();
            if (listQualityStandards == null || listQualityStandards.Count <= 0)
            {
                return "该批号下的产品没有设置好可以参考的单样本质量指标";
            }

            Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
            if (listQualityStandards.Count > 0)
            {
                foreach (var item in listQualityStandards)
                {
                    keyValuePairs.Add(item.TargetName, this.Request.Form[item.TargetName].ToString());
                }

                this.db.PdSmeltCode.Add(new PdqualitySmeltCode
                {
                    SmeltCode = yLCode,
                    Chemistry = keyValuePairs,
                    Createtime = (int)Util.Extensions.GetCurrentUnixTime(),
                    EntryPerson = this.userService.ApplicationUser.Mng_admin.Id,
                    Status = 0
                });

                this.db.SaveChanges();
            }

            return "true";
        }

        /// <summary>
        /// 编辑状态
        /// </summary>
        /// <param name="id">主键</param>
        /// <param name="status">状态</param>
        /// <returns>string</returns>
        public string ChemistryEdit(int id, int status)
        {
            var chemistryInfo = this.db.PdSmeltCode.FirstOrDefault(f => f.Id == id);
            if (chemistryInfo == null)
            {
                return "不存在化学数据无法编辑";
            }

            var chemistry = this.db.PdSmeltCode.FirstOrDefault(f => f.SmeltCode == chemistryInfo.SmeltCode && f.Status == 0 && f.Id != id);
            if (chemistry != null)
            {
                return "已经存在该冶炼炉号化学数据,恢复失败";
            }

            chemistryInfo.Status = status;
            this.db.Update(chemistryInfo);
            this.db.SaveChanges();
            return "true";
        }

        /// <summary>
        /// 添加物理数据
        /// </summary>
        /// <param name="smeltCode">冶炼表Id</param>
        /// <param name="batCode">炉批号</param>
        /// <returns>string</returns>
        public string PhysicsAdd(string smeltCode, string batCode)
        {
            if (string.IsNullOrEmpty(smeltCode))
            {
                return "请选择冶炼炉号";
            }

            if (string.IsNullOrWhiteSpace(batCode))
            {
                return "请输入炉批号";
            }

            List<string> list_ma_spec = this.GetMaterialSpecNameByBatCode(batCode);
            if (list_ma_spec == null)
            {
                return "添加失败，该炉批号找不到生产纪录";
            }

            if (string.IsNullOrEmpty(list_ma_spec[0]))
            {
                return "添加失败，找不到该炉批号生产产品的材质基础数据";
            }

            var modelSpec = this.GetSpecById(System.Convert.ToInt32(list_ma_spec[1]));
            if (modelSpec == null)
            {
                return "添加失败，找不到该炉批号生产产品的规格基础数据";
            }

            if (!modelSpec.Coldratio.HasValue || modelSpec.Coldratio.Value == 0
                || !modelSpec.Reverseratio.HasValue || modelSpec.Reverseratio.Value == 0)
            {
                return "添加失败，该炉批号生产产品的规格基础数据有误 （冷弯或反弯无数据）";
            }

            Regex regNum = new Regex("\\d+");
            int materialNameNum = 0; // list_ma_spec
            int specnameNum = 0;
            if (regNum.IsMatch(list_ma_spec[0]) && regNum.IsMatch(modelSpec.Specname))
            {
                materialNameNum = regNum.Match(list_ma_spec[0]).Value.ToInt();
                specnameNum = regNum.Match(modelSpec.Specname).Value.ToInt();
            }
            else
            {
                return "添加失败，材质或规格提取数字失败";
            }

            var list_check = this.CheckBatRecored(batCode);

            // 先判断是否审核通过，审核通过的不能修改
            if (list_check.Where(c => c.CheckStatus == EnumList.CheckStatus_PdQuality.审核通过).Count() > 0)
            {
                return "已添加过相同炉批号的质量数据,并且审核已通过";
            }
            else if (list_check.Where(c => c.CheckStatus == EnumList.CheckStatus_PdQuality.等待审核).Count() > 0)
            {
                return "已添加过相同炉批号的质量数据,并且等待审核中";
            }

            var pdsmelcodeInfo = this.db.PdSmeltCode.FirstOrDefault(f => f.SmeltCode == smeltCode && f.Status == 0);
            if (pdsmelcodeInfo == null)
            {
                return "不存在该冶炼炉号化学数据,请先输入化学数据";
            }

            var pdquatulyInfo = this.db.PdProduct.FirstOrDefault(f => f.Batcode == batCode);
            if (pdquatulyInfo == null)
            {
                return "不存在该炉批号产品";
            }

            if (pdquatulyInfo.Materialid == 0 || !pdquatulyInfo.Materialid.HasValue)
            {
                return "该炉批号无材质ID";
            }

            PdQuality pdquatuly = new PdQuality();
            List<object> keyValues = new List<object>();
            var dxQualityStandards = this.db.BaseQualityStandard.Where(w => w.TargetType == 1 && w.Status == 0 && w.Materialid == pdquatulyInfo.Materialid && w.TargetCategory == EnumList.TargetCategory.物理指标).ToList();
            var listQualityStandards = this.db.BaseQualityStandard.Where(w => w.TargetType == 0 && w.Status == 0 && w.Materialid == pdquatulyInfo.Materialid && w.TargetCategory == EnumList.TargetCategory.物理指标).ToList();
            if (listQualityStandards.Count <= 0)
            {
                return "该材质下没有设置物理指标";
            }

            if (listQualityStandards.Count > 0)
            {
                Dictionary<string, object> keyValuePairs = new Dictionary<string, object>();

                foreach (var item in listQualityStandards)
                {
                    keyValuePairs.Add(item.TargetName, this.Request.Form[item.TargetName].ToString());
                }

                var dicNew = (JObject)pdsmelcodeInfo.Chemistry.Object;
                foreach (var item in dicNew)
                {
                    keyValuePairs.Add(item.Key, item.Value.ToDouble());
                }

                pdquatuly.Qualityinfos = keyValuePairs;
            }
            else
            {
                pdquatuly.Qualityinfos = pdsmelcodeInfo.Chemistry;
            }

            if (dxQualityStandards.Count > 0)
            {
                for (int i = 0; i < Count; i++)
                {
                    if (dxQualityStandards == null || dxQualityStandards.Count <= 0)
                    {
                        return "该批号下的产品没有设置好可以参考的多样本质量指标";
                    }

                    if (this.Request.Form["下屈服强度" + i].SafeString() == string.Empty)
                    {
                        break;
                    }

                    Dictionary<string, object> keyValue = new Dictionary<string, object>();
                    foreach (var item in dxQualityStandards)
                    {
                        if (item.TargetName.StartsWith("伸长率"))
                        {
                            keyValue.Add(item.TargetName, this.Request.Form[item.TargetName + i].ToDouble(1).ToString("f1"));
                        }
                        else
                        {
                            keyValue.Add(item.TargetName, this.Request.Form[item.TargetName + i].ToString());
                        }
                    }

                    // 多行元素必要字段校验
                    List<string> targetdxList = new List<string>()
                            {
                                "下屈服强度", "抗拉强度", "伸长率A", "伸长率Agt"
                            };
                    var keyS = keyValue.Keys.ToList();
                    List<string> targetdxName = new List<string>();
                    foreach (var item in targetdxList)
                    {
                        // 判断是否缺少元素
                        if (!keyS.Any(x => x == item))
                        {
                            targetdxName.Add(item);
                        }
                    }

                    double qqb = 0;
                    double qiangqb = 0;

                    // 如果少指标就返回
                    if (targetdxName.Count > 0)
                    {
                        if (list_ma_spec[0].ToUpper().EndsWith("E"))
                        {
                            if (!keyValue.Keys.Any(a => a == "强屈比"))
                            {
                                keyValue.Add("强屈比", qiangqb);
                            }

                            if (!keyValue.Keys.Any(b => b == "屈屈比"))
                            {
                                keyValue.Add("屈屈比", qqb);
                            }
                        }
                    }
                    else
                    {
                        if (list_ma_spec[0].ToUpper().EndsWith("E"))
                        {
                            if (!keyValue.Keys.Any(a => a == "屈屈比"))
                            {
                                qqb = (keyValue["下屈服强度"].ToDouble() / materialNameNum).ToDouble(2);
                                keyValue.Add("屈屈比", qqb);
                            }

                            // 强屈比计算公式
                            if (!keyValue.Keys.Any(b => b == "强屈比"))
                            {
                                qiangqb = (keyValue["抗拉强度"].ToDouble() / keyValue["下屈服强度"].ToDouble()).ToDouble(2);
                                keyValue.Add("强屈比", qiangqb);
                            }
                        }
                    }

                    keyValues.Add(keyValue);
                }

                pdquatuly.Qualityinfos_Dynamics = keyValues;
            }

            pdquatuly.CreateFlag = 0;
            pdquatuly.Batcode = batCode;
            pdquatuly.CheckStatus = EnumList.CheckStatus_PdQuality.等待审核;
            pdquatuly.MaterialId = pdquatulyInfo.Materialid;
            pdquatuly.Createtime = (int)Util.Extensions.GetCurrentUnixTime();
            pdquatuly.EntryPerson = this.userService.ApplicationUser.Mng_admin.Id;
            this.db.Add(pdquatuly);
            this.db.SaveChanges();
            return "true";
        }

        /// <summary>
        /// 编辑物理数据
        /// </summary>
        /// <param name="qId">质量数据id</param>
        /// <returns>string</returns>
        public string PhysicsEdit(int qId)
        {
            var pdquatulyInfo = this.db.PdQuality.FirstOrDefault(f => f.Id == qId);
            if (pdquatulyInfo == null)
            {
                return "不存在质量数据,无法编辑";
            }

            List<string> list_ma_spec = this.GetMaterialSpecNameByBatCode(pdquatulyInfo.Batcode);
            if (list_ma_spec == null)
            {
                return "修改失败，该炉批号找不到生产纪录";
            }

            if (string.IsNullOrEmpty(list_ma_spec[0]))
            {
                return "修改失败，找不到该炉批号生产产品的材质基础数据";
            }

            var modelSpec = this.GetSpecById(System.Convert.ToInt32(list_ma_spec[1]));
            if (modelSpec == null)
            {
                return "修改失败，找不到该炉批号生产产品的规格基础数据";
            }

            if (!modelSpec.Coldratio.HasValue || modelSpec.Coldratio.Value == 0
                || !modelSpec.Reverseratio.HasValue || modelSpec.Reverseratio.Value == 0)
            {
                return "修改失败，该炉批号生产产品的规格基础数据有误 （冷弯或反弯无数据）";
            }

            Regex regNum = new Regex("\\d+");
            int materialNameNum = 0; // list_ma_spec
            int specnameNum = 0;
            if (regNum.IsMatch(list_ma_spec[0]) && regNum.IsMatch(modelSpec.Specname))
            {
                materialNameNum = regNum.Match(list_ma_spec[0]).Value.ToInt();
                specnameNum = regNum.Match(modelSpec.Specname).Value.ToInt();
            }
            else
            {
                return "添加失败，材质或规格提取数字失败";
            }

            List<object> keyValues = new List<object>();
            var dxQualityStandards = this.db.BaseQualityStandard.Where(w => w.TargetType == 1 && w.Status == 0 && w.Materialid == pdquatulyInfo.MaterialId && w.TargetCategory == EnumList.TargetCategory.物理指标).ToList();
            var listQualityStandards = this.db.BaseQualityStandard.Where(w => w.TargetType == 0 && w.Status == 0 && w.Materialid == pdquatulyInfo.MaterialId && w.TargetCategory == EnumList.TargetCategory.物理指标).ToList();
            if (listQualityStandards.Count > 0)
            {
                Dictionary<string, double> keyValuePairs = new Dictionary<string, double>();

                foreach (var item in listQualityStandards)
                {
                    keyValuePairs.Add(item.TargetName, this.Request.Form[item.TargetName].ToDouble());
                }

                var dicNew = (JObject)pdquatulyInfo.Qualityinfos.Object;

                foreach (var item in dicNew)
                {
                    if (!keyValuePairs.Keys.ToList().Any(o => o == item.Key))
                    {
                        keyValuePairs.Add(item.Key, item.Value.ToDouble());
                    }
                }

                pdquatulyInfo.Qualityinfos = keyValuePairs;
            }

            if (dxQualityStandards.Count > 0)
            {
                for (int i = 0; i < Count; i++)
                {
                    if (dxQualityStandards == null || dxQualityStandards.Count <= 0)
                    {
                        return "该批号下的产品没有设置好可以参考的多样本质量指标";
                    }

                    if (this.Request.Form["下屈服强度" + i].SafeString() == string.Empty)
                    {
                        break;
                    }

                    Dictionary<string, object> keyValue = new Dictionary<string, object>();
                    foreach (var item in dxQualityStandards)
                    {
                        if (item.TargetName.StartsWith("伸长率"))
                        {
                            keyValue.Add(item.TargetName, this.Request.Form[item.TargetName + i].ToDouble(1).ToString("f1"));
                        }
                        else
                        {
                            keyValue.Add(item.TargetName, this.Request.Form[item.TargetName + i].ToString());
                        }
                    }

                    // 多行元素必要字段校验
                    List<string> targetdxList = new List<string>()
                            {
                                "下屈服强度", "抗拉强度", "伸长率A", "伸长率Agt"
                            };
                    var keyS = keyValue.Keys.ToList();
                    List<string> targetdxName = new List<string>();
                    foreach (var item in targetdxList)
                    {
                        // 判断是否缺少元素
                        if (!keyS.Any(x => x == item))
                        {
                            targetdxName.Add(item);
                        }
                    }

                    double qqb = 0;
                    double qiangqb = 0;

                    // 如果少指标就返回
                    if (targetdxName.Count > 0)
                    {
                        if (list_ma_spec[0].ToUpper().EndsWith("E"))
                        {
                            if (!keyValue.Keys.Any(a => a == "强屈比"))
                            {
                                keyValue.Add("强屈比", qiangqb);
                            }

                            if (!keyValue.Keys.Any(b => b == "屈屈比"))
                            {
                                keyValue.Add("屈屈比", qqb);
                            }
                        }
                    }
                    else
                    {
                        if (list_ma_spec[0].ToUpper().EndsWith("E"))
                        {
                            if (!keyValue.Keys.Any(a => a == "屈屈比"))
                            {
                                qqb = (keyValue["下屈服强度"].ToDouble() / materialNameNum).ToDouble(2);
                                keyValue.Add("屈屈比", qqb);
                            }

                            // 强屈比计算公式
                            if (!keyValue.Keys.Any(b => b == "强屈比"))
                            {
                                qiangqb = (keyValue["抗拉强度"].ToDouble() / keyValue["下屈服强度"].ToDouble()).ToDouble(2);
                                keyValue.Add("强屈比", qiangqb);
                            }
                        }
                    }

                    keyValues.Add(keyValue);
                }

                pdquatulyInfo.Qualityinfos_Dynamics = keyValues;
                this.db.SaveChanges();
            }

            return "true";
        }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="batCode">炉批号</param>
        /// <returns>string</returns>
        [HttpPost]
        public string Add(string batCode)
        {
            List<string> list_ma_spec = this.GetMaterialSpecNameByBatCode(batCode);
            if (list_ma_spec == null)
            {
                return "添加失败，该炉批号找不到生产纪录";
            }

            if (string.IsNullOrEmpty(list_ma_spec[0]))
            {
                return "添加失败，找不到该炉批号生产产品的材质基础数据";
            }

            var modelSpec = this.GetSpecById(System.Convert.ToInt32(list_ma_spec[1]));
            if (modelSpec == null)
            {
                return "添加失败，找不到该炉批号生产产品的规格基础数据";
            }

            if (!modelSpec.Coldratio.HasValue || modelSpec.Coldratio.Value == 0
                || !modelSpec.Reverseratio.HasValue || modelSpec.Reverseratio.Value == 0)
            {
                return "添加失败，该炉批号生产产品的规格基础数据有误 （冷弯或反弯无数据）";
            }

            Regex regNum = new Regex("\\d+");
            int materialNameNum = 0; // list_ma_spec
            int specnameNum = 0;
            if (regNum.IsMatch(list_ma_spec[0]) && regNum.IsMatch(modelSpec.Specname))
            {
                materialNameNum = regNum.Match(list_ma_spec[0]).Value.ToInt();
                specnameNum = regNum.Match(modelSpec.Specname).Value.ToInt();
            }
            else
            {
                return "添加失败，材质或规格提取数字失败";
            }

            var list_check = this.CheckBatRecored(batCode);

            // 先判断是否审核通过，审核通过的不能修改
            if (list_check.Where(c => c.CheckStatus == EnumList.CheckStatus_PdQuality.审核通过).Count() > 0)
            {
                return "已添加过相同炉批号的质量数据,并且审核已通过";
            }
            else if (list_check.Where(c => c.CheckStatus == EnumList.CheckStatus_PdQuality.等待审核).Count() > 0)
            {
                return "已添加过相同炉批号的质量数据,并且等待审核中";
            }
            else
            {
                try
                {
                    var productInfo = this.db.PdProduct.FirstOrDefault(f => f.Batcode == batCode);
                    if (productInfo == null)
                    {
                        return "不存在该编号的钢炉";
                    }

                    // PdQualityInfo _PdQualityInfo = PdQualityInfo;
                    // Ceq计算公式 Ceq = C + Mn / 6 + (Cr + V + Mo) / 5 + (Cu + Ni) / 15
                    // _PdQualityInfo.Ceq = (_PdQualityInfo.C.ToDouble() + (_PdQualityInfo.Mn.ToDouble() / 6) +
                    //    ((_PdQualityInfo.Cr.ToDouble() + _PdQualityInfo.V.ToDouble() + _PdQualityInfo.Mo.ToDouble()) / 5) +
                    //    ((_PdQualityInfo.Cu.ToDouble() + _PdQualityInfo.Ni.ToDouble()) / 15)).ToDouble(2);
                    // 单行
                    var listQualityStandards = this.db.BaseQualityStandard.Where(w => w.TargetType == 0 && w.Status == 0 && w.Materialid == productInfo.Materialid).ToList();
                    if (listQualityStandards == null || listQualityStandards.Count <= 0)
                    {
                        return "该批号下的产品没有设置好可以参考的单样本质量指标";
                    }

                    // 单行元素字典
                    Dictionary<string, double> keyValuePairs = new Dictionary<string, double>();
                    if (listQualityStandards.Count > 0)
                    {
                        foreach (var item in listQualityStandards)
                        {
                            keyValuePairs.Add(item.TargetName, this.Request.Form[item.TargetName].ToDouble());
                        }

                        List<string> targetList = new List<string>()
                    {
                        "C", "Mn", "V", "Mo", "Cu", "Ni",
                    };
                        var keyPairs = keyValuePairs.Keys.ToList();
                        List<string> targetName = new List<string>();
                        foreach (var item in targetList)
                        {
                            // 判断是否缺少元素
                            if (!keyPairs.Any(x => x == item))
                            {
                                targetName.Add(item);
                            }
                        }

                        double ceq = 0;

                        // 如果少指标就返回
                        if (targetName.Count > 0)
                        {
                            keyValuePairs.Add("Ceq", ceq);
                        }
                        else
                        {
                            ceq = (keyValuePairs["C"] + (keyValuePairs["Mn"] / 6) +
                            ((keyValuePairs["C"] + keyValuePairs["V"] + keyValuePairs["Mo"]) / 5) +
                            ((keyValuePairs["Cu"] = keyValuePairs["Ni"]) / 15)).ToDouble(2);
                            keyValuePairs.Add("Ceq", ceq);
                        }
                    }

                    // List<PdQualityInfo_Dynamics> list_PdQualityInfo_Dynamics = new List<PdQualityInfo_Dynamics>();
                    // 多行元素集合
                    List<object> keyValues = new List<object>();
                    var dxQualityStandards = this.db.BaseQualityStandard.Where(w => w.TargetType == 1 && w.Status == 0 && w.Materialid == productInfo.Materialid).ToList();
                    if (dxQualityStandards.Count > 0)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            if (dxQualityStandards == null || dxQualityStandards.Count <= 0)
                            {
                                return "该批号下的产品没有设置好可以参考的多样本质量指标";
                            }

                            Dictionary<string, object> keyValue = new Dictionary<string, object>();
                            foreach (var item in dxQualityStandards)
                            {
                                keyValue.Add(item.TargetName, this.Request.Form[item.TargetName + i].ToDouble(2));
                            }

                            // 多行元素必要字段校验
                            List<string> targetdxList = new List<string>()
                            {
                                "下屈服强度", "抗拉强度", "伸长率A", "伸长率Agt"
                            };
                            var keyS = keyValue.Keys.ToList();
                            List<string> targetdxName = new List<string>();
                            foreach (var item in targetdxList)
                            {
                                // 判断是否缺少元素
                                if (!keyS.Any(x => x == item))
                                {
                                    targetdxName.Add(item);
                                }
                            }

                            double qqb = 0;
                            double qiangqb = 0;

                            // 如果少指标就返回
                            if (targetdxName.Count > 0)
                            {
                                if (list_ma_spec[0].ToUpper().EndsWith("E"))
                                {
                                    if (!keyValue.Keys.Any(a => a == "强屈比"))
                                    {
                                        keyValue.Add("强屈比", qiangqb);
                                    }

                                    if (!keyValue.Keys.Any(b => b == "屈屈比"))
                                    {
                                        keyValue.Add("屈屈比", qqb);
                                    }
                                }
                            }
                            else
                            {
                                if (list_ma_spec[0].ToUpper().EndsWith("E"))
                                {
                                    if (!keyValue.Keys.Any(a => a == "屈屈比"))
                                    {
                                        qqb = (keyValue["下屈服强度"].ToDouble() / materialNameNum).ToDouble(2);
                                        keyValue.Add("屈屈比", qqb);
                                    }

                                    // 强屈比计算公式
                                    if (!keyValue.Keys.Any(b => b == "强屈比"))
                                    {
                                        qiangqb = (keyValue["抗拉强度"].ToDouble() / keyValue["下屈服强度"].ToDouble()).ToDouble(2);
                                        keyValue.Add("强屈比", qiangqb);
                                    }
                                }
                            }

                            keyValues.Add(keyValue);
                        }
                    }

                    this.db.PdQuality.Add(new PdQuality()
                    {
                        Batcode = batCode,
                        CheckStatus = EnumList.CheckStatus_PdQuality.等待审核,
                        MaterialId = productInfo.Materialid,
                        Createtime = (int)Util.Extensions.GetCurrentUnixTime(),
                        EntryPerson = this.userService.ApplicationUser.Mng_admin.Id,
                        CreateFlag = 0,
                        Qualityinfos = keyValuePairs,
                        Qualityinfos_Dynamics = keyValues
                    });

                    this.db.SaveChanges();
                }
                catch
                {
                    return "添加失败，请检查录入的数据";
                }
            }

            return "true";
        }

        /// <summary>
        /// 修改数据
        /// </summary>
        /// <param name="pId">炉批号</param>
        /// <returns>string</returns>
        [HttpPost]
        public string Edit(int pId)
        {
            var qualityInfo = this.db.PdQuality.FirstOrDefault(f => f.Id == pId);
            if (qualityInfo == null)
            {
                return "该编号的炉炉批无录入数据,无法进行编辑";
            }

            var batCode = qualityInfo.Batcode;
            List<string> list_ma_spec = this.GetMaterialSpecNameByBatCode(batCode);
            if (list_ma_spec == null)
            {
                return "修改失败，该炉批号找不到生产纪录";
            }

            if (string.IsNullOrEmpty(list_ma_spec[0]))
            {
                return "修改失败，找不到该炉批号生产产品的材质基础数据";
            }

            var modelSpec = this.GetSpecById(System.Convert.ToInt32(list_ma_spec[1]));
            if (modelSpec == null)
            {
                return "修改失败，找不到该炉批号生产产品的规格基础数据";
            }

            if (!modelSpec.Coldratio.HasValue || modelSpec.Coldratio.Value == 0
                || !modelSpec.Reverseratio.HasValue || modelSpec.Reverseratio.Value == 0)
            {
                return "修改失败，该炉批号生产产品的规格基础数据有误 （冷弯或反弯无数据）";
            }

            Regex regNum = new Regex("\\d+");
            int materialNameNum = 0; // list_ma_spec
            int specnameNum = 0;
            if (regNum.IsMatch(list_ma_spec[0]) && regNum.IsMatch(modelSpec.Specname))
            {
                materialNameNum = regNum.Match(list_ma_spec[0]).Value.ToInt();
                specnameNum = regNum.Match(modelSpec.Specname).Value.ToInt();
            }
            else
            {
                return "修改失败，材质或规格提取数字失败";
            }

            // 先判断是否审核通过，审核通过的不能修改
            var productInfo = this.db.PdProduct.FirstOrDefault(f => f.Batcode == batCode);
            if (productInfo == null)
            {
                return "无该编号的炉批";
            }

            if (qualityInfo.CheckStatus == EnumList.CheckStatus_PdQuality.审核通过)
            {
                return "审核通过,无法修改";
            }

            // 单行指标
            var listQualityStandards = this.db.BaseQualityStandard.Where(w => w.TargetType == 0 && w.Status == 0 && w.Materialid == productInfo.Materialid).ToList();
            if (listQualityStandards == null || listQualityStandards.Count <= 0)
            {
                return "单行质量指标,无字段";
            }

            // 单行元素字典
            Dictionary<string, double> keyValuePairs = new Dictionary<string, double>();
            if (listQualityStandards.Count > 0)
            {
                foreach (var item in listQualityStandards)
                {
                    keyValuePairs.Add(item.TargetName, this.Request.Form[item.TargetName].ToDouble());
                }

                List<string> targetList = new List<string>()
                    {
                        "C", "Mn", "V", "Mo", "Cu", "Ni",
                    };
                var keyPairs = keyValuePairs.Keys.ToList();
                List<string> targetName = new List<string>();
                foreach (var item in targetList)
                {
                    // 判断是否缺少元素
                    if (!keyPairs.Any(x => x == item))
                    {
                        targetName.Add(item);
                    }
                }

                double ceq = 0;

                // 如果少指标就返回
                if (targetName.Count > 0)
                {
                    keyValuePairs.Add("Ceq", ceq);
                }
                else
                {
                    ceq = (keyValuePairs["C"] + (keyValuePairs["Mn"] / 6) +
                    ((keyValuePairs["C"] + keyValuePairs["V"] + keyValuePairs["Mo"]) / 5) +
                    ((keyValuePairs["Cu"] = keyValuePairs["Ni"]) / 15)).ToDouble(2);
                    keyValuePairs.Add("Ceq", ceq);
                }
            }

            // List<PdQualityInfo_Dynamics> list_PdQualityInfo_Dynamics = new List<PdQualityInfo_Dynamics>();
            // 多行元素集合
            List<object> keyValues = new List<object>();
            var dxQualityStandards = this.db.BaseQualityStandard.Where(w => w.TargetType == 1 && w.Status == 0 && w.Materialid == productInfo.Materialid).ToList();
            if (dxQualityStandards.Count > 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (dxQualityStandards == null || dxQualityStandards.Count <= 0)
                    {
                        return "该批号下的产品没有设置好可以参考的多样本质量指标";
                    }

                    Dictionary<string, object> keyValue = new Dictionary<string, object>();
                    foreach (var item in dxQualityStandards)
                    {
                        keyValue.Add(item.TargetName, this.Request.Form[item.TargetName + i].ToDouble(2));
                    }

                    // 多行元素必要字段校验
                    List<string> targetdxList = new List<string>()
                            {
                                "下屈服强度", "抗拉强度", "伸长率A", "伸长率Agt"
                            };
                    var keyS = keyValue.Keys.ToList();
                    List<string> targetdxName = new List<string>();
                    foreach (var item in targetdxList)
                    {
                        // 判断是否缺少元素
                        if (!keyS.Any(x => x == item))
                        {
                            targetdxName.Add(item);
                        }
                    }

                    double qqb = 0;
                    double qiangqb = 0;

                    // 如果少指标就返回
                    if (targetdxName.Count > 0)
                    {
                        if (list_ma_spec[0].ToUpper().EndsWith("E"))
                        {
                            if (!keyValue.Keys.Any(a => a == "强屈比"))
                            {
                                keyValue.Add("强屈比", qiangqb);
                            }

                            if (!keyValue.Keys.Any(b => b == "屈屈比"))
                            {
                                keyValue.Add("屈屈比", qqb);
                            }
                        }
                    }
                    else
                    {
                        if (list_ma_spec[0].ToUpper().EndsWith("E"))
                        {
                            if (!keyValue.Keys.Any(a => a == "屈屈比"))
                            {
                                qqb = (keyValue["下屈服强度"].ToDouble() / materialNameNum).ToDouble(2);
                                keyValue.Add("屈屈比", qqb);
                            }

                            // 强屈比计算公式
                            if (!keyValue.Keys.Any(b => b == "强屈比"))
                            {
                                qiangqb = (keyValue["抗拉强度"].ToDouble() / keyValue["下屈服强度"].ToDouble()).ToDouble(2);
                                keyValue.Add("强屈比", qiangqb);
                            }
                        }
                    }

                    keyValues.Add(keyValue);
                }
            }

            qualityInfo.Qualityinfos = keyValuePairs;
            qualityInfo.Qualityinfos_Dynamics = keyValues;
            this.db.PdQuality.Update(qualityInfo);
            this.db.SaveChanges();

            // 实体为空则添加，否则则修改
            return "true";
        }

        /// <summary>
        /// 审核数据
        /// </summary>
        /// <param name="chkId">审核Id</param>
        /// <param name="checkStatus">状态</param>
        /// <returns>string</returns>
        [HttpPost]
        public string CheckQuality(string chkId, EnumList.CheckStatus_PdQuality checkStatus)
        {
            chkId = chkId.Trim(',');

            var chkList = Array.ConvertAll(chkId.Split(','), new Converter<string, int>(x => int.Parse(x)));

            if (chkList.Length > 0)
            {
                var query = this.db.PdQuality.Where(c => chkList.Contains(c.Id));
                if (query != null)
                {
                    foreach (var q in query)
                    {
                        q.CheckPerson = this.userService.ApplicationUser.Mng_admin.Id;
                        q.CheckStatus = checkStatus;

                        var tempObject = q.Qualityinfos_Dynamics.Object;

                        if (checkStatus == EnumList.CheckStatus_PdQuality.审核通过)
                        {
                            for (int i = 0; i < tempObject.Count; i++)
                            {
                                // tempObject[i].冷弯 = "完好/Pass";
                                // tempObject[i].反弯 = "完好/Pass";
                                JObject qualityinfos_Dynamics = (JObject)q.Qualityinfos_Dynamics.Object[i];
                                qualityinfos_Dynamics["冷弯"] = "完好/Pass";
                                qualityinfos_Dynamics["反弯"] = "完好/Pass";
                            }
                        }
                        else if (checkStatus == EnumList.CheckStatus_PdQuality.审核不通过)
                        {
                            for (int i = 0; i < tempObject.Count; i++)
                            {
                                JObject qualityinfos_Dynamics = (JObject)q.Qualityinfos_Dynamics.Object[i];
                                qualityinfos_Dynamics["冷弯"] = string.Empty;
                                qualityinfos_Dynamics["反弯"] = string.Empty;
                            }
                        }

                        q.Qualityinfos_Dynamics = tempObject;
                    }

                    this.db.SaveChanges();
                    return "true";
                }
                else
                {
                    return "找不到您要的质量数据";
                }
            }
            else
            {
                return "请选择要审核的数据";
            }
        }

        /// <summary>
        /// 检查图片是否存在
        /// </summary>
        /// <param name="id">id</param>
        /// <returns>ActionResult</returns>
        public ActionResult CheckDownLoad(int id)
        {
            try
            {
                var salePrintlogInfo = this.salePrintlog.FindSingle(w => w.Id == id);
                if (salePrintlogInfo == null)
                {
                    return this.AjaxResult(false, "不存在该图像记录");
                }

                // 预览图片的名称
                var ylimg = string.Format("{0}{1}p", salePrintlogInfo.Id.ToString(), salePrintlogInfo.Checkcode);

                // 请求的url
                var reqUrl = this.db.MngSetting.FirstOrDefault().Domain_WebApi + "qualitypics/" + ylimg + ".jpg";
                var resStr = HttpHelper.Get(reqUrl, "image/jpeg");
                return this.AjaxResult(true, string.Format("{0}{1}p", salePrintlogInfo.Id.ToString(), salePrintlogInfo.Checkcode));
            }
            catch (Exception ex)
            {
                this.logService.LogError(this.logService.GetLogStr(this.HttpContext, ex));
                return this.AjaxResult(false, "不存在图片,无法下载");
            }
        }

        /// <summary>
        /// 下载图片
        /// </summary>
        /// <param name="id">id</param>
        /// <returns>ActionResult</returns>
        public ActionResult Download(int id)
        {
            try
            {
                var salePrintlogInfo = this.salePrintlog.FindSingle(w => w.Id == id);
                if (salePrintlogInfo == null)
                {
                    return this.AjaxResult(false, "不存在该图像记录");
                }

                // 下载图片的名称
                var ylimg = string.Format("{0}{1}", salePrintlogInfo.Id.ToString(), salePrintlogInfo.Checkcode);
                var fileType = ".jpg";

                // 请求的url
                var reqUrl = this.db.MngSetting.FirstOrDefault().Domain_WebApi + "qualitypics/" + ylimg + fileType;
                var contextType = "image/jpg";

                // 修改状态
                salePrintlogInfo.Status = 1;
                this.salePrintlog.Update(salePrintlogInfo);

                return this.File(HttpHelper.GetByteArray(reqUrl, contextType), "image/jpg", ylimg + fileType);
            }
            catch (Exception ex)
            {
                this.logService.LogError(this.logService.GetLogStr(this.HttpContext, ex));
                return this.AjaxResult(false, "下载失败");
            }
        }

        /// <summary>
        /// 下载模板前做数据校验
        /// </summary>
        /// <param name="materialid">材质id</param>
        /// <returns>ActionResult</returns>
        public ActionResult CheckMaterialid(int materialid)
        {
            try
            {
                if (materialid <= 0)
                {
                    return this.AjaxResult(false, "请选择材质");
                }

                var bqsList = this.db.BaseQualityStandard.Where(w => w.Materialid == materialid).ToList();
                if (bqsList == null || bqsList.Count <= 0)
                {
                    return this.AjaxResult(false, "该材质下没有质量指标,请先录入质量指标");
                }

                return this.AjaxResult(true, string.Empty);
            }
            catch (Exception ex)
            {
                this.logService.LogError(this.logService.GetLogStr(this.HttpContext, ex));
                return this.AjaxResult(false, ex.Message);
            }
        }

        /// <summary>
        /// 下载模板根据材质Id
        /// </summary>
        /// <param name="materialid">材质Id</param>
        /// <returns>ActionResult</returns>
        public ActionResult DownTemplate(int materialid)
        {
            try
            {
                if (materialid <= 0)
                {
                    return this.AjaxResult(false, "请选择材质");
                }

                var bqsList = this.db.BaseQualityStandard.Where(w => w.Materialid == materialid).ToList();
                if (bqsList == null || bqsList.Count <= 0)
                {
                    return this.AjaxResult(false, "该材质下没有质量指标,请先录入质量指标");
                }

                string sFileName = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}.xlsx";

                using (ExcelPackage package = new ExcelPackage())
                {
                    // 添加worksheet
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("DownTemplate");

                    // 单行元素
                    var danxList = this.db.BaseQualityStandard.Where(w => w.Materialid == materialid && w.TargetType == 0 && w.Status == 0).ToList();

                    for (int i = 0; i < danxList.Count; i++)
                    {
                        // 表头值
                        worksheet.Cells[1, (i + 1).ToInt()].Value = danxList[i].TargetName;
                    }

                    // 多行元素
                    var duoxList = this.db.BaseQualityStandard.Where(w => w.Materialid == materialid && w.TargetType == 1 && w.Status == 0).ToList();

                    // 多行元素索引开始值
                    var colsDanx = worksheet.Dimension.Columns + 1;
                    for (int j = 0; j < duoxList.Count; j++)
                    {
                        // 表头值
                        worksheet.Cells[1, colsDanx.ToInt(), 1, (colsDanx + 2).ToInt()].Value = duoxList[j].TargetName;

                        // 单元格合并
                        worksheet.Cells[1, colsDanx.ToInt(), 1, (colsDanx + 2).ToInt()].Merge = true;

                        colsDanx += 3;
                    }

                    // 水平居中
                    worksheet.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    // 垂直居中
                    worksheet.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    return this.File(package.GetAsByteArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", sFileName);
                }
            }
            catch (Exception ex)
            {
                this.logService.LogError(this.logService.GetLogStr(this.HttpContext, ex));
                return this.AjaxResult(false, ex.Message);
            }
        }

        /// <summary>
        /// 撤回质保书
        /// </summary>
        /// <param name="id">质保书ID</param>
        /// <returns>撤回操作状态</returns>
        [HttpPost]
        public ActionResult Withdraw(int id)
        {
            var modelPrint = this.db.SalePrintlog.FirstOrDefault(c => c.Id == id);
            if (modelPrint != null)
            {
                if (modelPrint.Status != (int)SalePrintlogStatus.已下载)
                {
                    return this.AjaxResult(false, "该质保书未生成，不可撤回");
                }

                var createTime = modelPrint.Createtime.ToLong().GetDateTimeFromUnixTime().Date;
                if (DateTime.Now.Date > createTime.AddMonths(1))
                {
                    return this.AjaxResult(false, "只能撤回30天内的质保书");
                }

                using (var tran = this.db.Database.BeginTransaction())
                {
                    // 标识质保书状态
                    modelPrint.Status = (int)SalePrintlogStatus.已撤回;

                    // 查找打印详情纪录并纪录 相关的授权纪录ID列表
                    var list_saleSellerAuth = this.db.SalePrintLogDetail.Where(c => c.PrintId == modelPrint.Id);

                    foreach (var auth in list_saleSellerAuth)
                    {
                        // 查找授权详情纪录 并纪录 产品ID列表
                        var list_SaleSellerAuthDetail = this.db.SaleSellerAuthDetail.Where(c => auth.Authid == c.AuthId).Take(auth.Number);
                        List<int> list_productId = list_SaleSellerAuthDetail.Select(c => c.Productid).ToList();

                        // 删除授权详情纪录
                        this.db.SaleSellerAuthDetail.RemoveRange(list_SaleSellerAuthDetail);

                        // 删除授权纪录
                        var model_SaleSellerAuth = this.db.SaleSellerAuth.FirstOrDefault(c => c.Id == auth.Authid);
                        if (model_SaleSellerAuth != null)
                        {
                            int authNum = model_SaleSellerAuth.Number.Value;
                            if (authNum > auth.Number)
                            {
                                model_SaleSellerAuth.Number = model_SaleSellerAuth.Number - auth.Number;
                            }
                            else
                            {
                                this.db.SaleSellerAuth.Remove(model_SaleSellerAuth);
                            }
                        }

                        // 7、根据产品ID删除出库纪录
                        this.db.PdStockOut.RemoveRange(this.db.PdStockOut.Where(c => list_productId.Contains(c.Productid.Value)).Take(auth.Number));
                    }

                    // 删除打印详情纪录
                    this.db.SalePrintLogDetail.RemoveRange(list_saleSellerAuth);

                    this.db.SaveChanges();

                    tran.Commit();
                }

                return this.AjaxResult(true, "撤回成功");
            }
            else
            {
                return this.AjaxResult(false, "该质保书不存在");
            }
        }

        /// <summary>
        /// 上传数据
        /// </summary>
        /// <returns>ActionResult</returns>
        public ActionResult UpLoadTemplate()
        {
            try
            {
                var excelfile = this.Request.Form.Files[0];
                if (excelfile == null || excelfile.Length <= 0)
                {
                    return this.AjaxResult(false, "上传文件为空");
                }

                string sFileName = excelfile.FileName;
                var path = this.hostingEnvironment.ContentRootPath + this.rootPath + "/";

                // 获取上传文件的扩展名
                string fileEx = System.IO.Path.GetExtension(sFileName);

                // 获取无扩展名的文件名
                string noFileName = System.IO.Path.GetFileNameWithoutExtension(sFileName);

                // 定义上传文件的类型字符串
                string fileType = ".xlsx";

                if (!fileType.Contains(fileEx))
                {
                    return this.AjaxResult(false, "上传文件类型错误");
                }

                // 保存上传的数据
                using (FileStream fs = System.IO.File.Create(Path.Combine(path, sFileName)))
                {
                    // 复制文件
                    excelfile.CopyTo(fs);

                    // 清空缓冲区数据
                    fs.Flush();
                }

                FileInfo file = new FileInfo(path + sFileName);
                using (ExcelPackage package = new ExcelPackage(file))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[1];

                    // 总行数
                    int rowCount = worksheet.Dimension.Rows;

                    // 总列数
                    int colCount = worksheet.Dimension.Columns;

                    var materialid = 0;

                    // 装头部的字典
                    Dictionary<string, int> dicHead = new Dictionary<string, int>();

                    var bsqList = this.db.BaseQualityStandard.Where(w => w.Status == 0).ToList();

                    // 插入如数据集合
                    List<PdQuality> baseQualityList = new List<PdQuality>();
                    for (int row = 1; row <= rowCount; row++)
                    {
                        // 读取头部
                        if (row == 1)
                        {
                            // 填充表头
                            for (int col = 1; col <= colCount; col++)
                            {
                                var value = worksheet.Cells[row, col].Value;

                                // 添加头部,不为空的就添加
                                if (value != null && !dicHead.Any(a => a.Key == value.ToString()))
                                {
                                    dicHead.Add(value.ToString(), col);
                                }
                            }

                            if (bsqList == null || bsqList.Count <= 0)
                            {
                                return this.AjaxResult(false, "元素指标不存在");
                            }

                            // 根据材质Id分组查询
                            foreach (var item in bsqList.Select(s => s.Materialid).Distinct())
                            {
                                var bsqListByMid = bsqList.Where(w => w.Materialid == item).ToList();
                                if (bsqListByMid == null || bsqListByMid.Count <= 0)
                                {
                                    return this.AjaxResult(false, "该材质id:" + item + "下,元素指标不存在");
                                }

                                // 根据表头搜索出对应的材质Id
                                foreach (var headItem in dicHead)
                                {
                                    var bsqInfoByMid = bsqListByMid.FirstOrDefault(f => f.TargetName == headItem.Key);

                                    if (bsqInfoByMid == null)
                                    {
                                        materialid = 0;
                                        break;
                                    }
                                    else
                                    {
                                        materialid = bsqInfoByMid.Materialid.ToInt();
                                    }
                                }

                                // 如果匹配到就跳出循环
                                if (materialid > 0)
                                {
                                    break;
                                }
                            }

                            if (materialid == 0)
                            {
                                return this.AjaxResult(false, "没找到匹配的材质Id");
                            }

                            if (dicHead.Count <= 0)
                            {
                                return this.AjaxResult(false, "不存在表头,无法导入");
                            }
                        }

                        // 读取数据
                        else
                        {
                            PdQuality baseQuality = new PdQuality();

                            // 单行元素字典
                            Dictionary<string, object> keyValuePairs = new Dictionary<string, object>();

                            List<object> keyValues = new List<object>();

                            var danxList = bsqList.Where(w => w.Materialid == materialid && w.TargetType == 0).ToList();
                            var duoxList = bsqList.Where(w => w.Materialid == materialid && w.TargetType == 1).ToList();
                            if (danxList.Count > 0)
                            {
                                danxList.ForEach(o =>
                                {
                                    if (dicHead.Keys.Any(s => s == o.TargetName))
                                    {
                                        var value = worksheet.Cells[row, dicHead[o.TargetName]].Value;
                                        double val = 0;
                                        if (value != null)
                                        {
                                            val = value.ToDouble();
                                        }
                                        keyValuePairs.Add(o.TargetName, val);
                                    }
                                    else
                                    {
                                        keyValuePairs.Add(o.TargetName, 0);
                                    }
                                });

                                if (keyValuePairs.Count <= 0)
                                {
                                    return this.AjaxResult(false, "导入单行指标数据存在问题");
                                }
                            }

                            if (duoxList.Count > 0)
                            {
                                // 多行指标添加三次
                                for (int i = 0; i < 3; i++)
                                {
                                    // 多行元素字典
                                    Dictionary<string, object> keyValue = new Dictionary<string, object>();
                                    duoxList.ForEach(s =>
                                    {
                                        if (dicHead.Keys.Any(w => w == s.TargetName))
                                        {
                                            var value = worksheet.Cells[row, dicHead[s.TargetName] + i].Value;
                                            double val = 0;
                                            if (value != null)
                                            {
                                                val = value.ToDouble();
                                            }
                                            keyValue.Add(s.TargetName, val);
                                        }
                                        else
                                        {
                                            keyValue.Add(s.TargetName, string.Empty);
                                        }
                                    });
                                    keyValue.Add("冷弯弯心", string.Empty);
                                    keyValue.Add("冷弯", string.Empty);
                                    keyValue.Add("反弯弯心", string.Empty);
                                    keyValue.Add("反弯", string.Empty);
                                    keyValues.Add(keyValue);
                                }

                                if (keyValues.Count <= 0)
                                {
                                    return this.AjaxResult(false, "导入多行指标数据存在问题");
                                }
                            }

                            var userInfo = this.db.MngAdmin.Where(w => w.GroupManage.Object.Contains(3)).FirstOrDefault();

                            // 赋值
                            baseQuality.MaterialId = materialid;
                            baseQuality.Batcode = materialid.ToString();
                            baseQuality.CheckStatus = EnumList.CheckStatus_PdQuality.审核通过;
                            baseQuality.CreateFlag = 1;
                            baseQuality.Createtime = (int)Util.Extensions.GetCurrentUnixTime();
                            baseQuality.EntryPerson = this.userService.ApplicationUser.Mng_admin.Id;
                            baseQuality.CheckPerson = userInfo != null ? userInfo.Id : 0;
                            baseQuality.Qualityinfos = keyValuePairs;
                            baseQuality.Qualityinfos_Dynamics = keyValues;

                            // 添充数据
                            baseQualityList.Add(baseQuality);
                        }
                    }

                    if (baseQualityList != null && baseQualityList.Count > 0)
                    {
                        this.baseQuality.BatchAdd(baseQualityList);
                    }
                }

                return this.AjaxResult(true, "上传成功");
            }
            catch (Exception ex)
            {
                this.logService.LogError(this.logService.GetLogStr(this.HttpContext, ex));
                return this.AjaxResult(false, ex.Message);
            }
        }

        /// <summary>
        /// 手动同步产品的质量数据
        /// </summary>
        /// <returns>同步结果</returns>
        public ResponseModel BatSync()
        {
            string errorMessage = string.Empty;

            var setting = this.db.MngSetting.FirstOrDefault();
            if (setting.SystemVersion == SystemVersion.简单版本)
            {
                // 首先判断预置数据是否存在
                var qualitys = this.db.PdQuality.Where(p => p.CreateFlag == 1).ToList();
                if (qualitys.Count > 0)
                {
                    // 获取到没有预置数据关联的产品，按产品材质以及批号分组
                    var groups = (from product in this.db.PdProduct
                                  group product by new { product.Batcode, product.Materialid } into pgroup
                                  select new
                                  {
                                      Batcode = pgroup.Key.Batcode,
                                      Materialid = pgroup.Key.Materialid,
                                      Presetdata = (from pp in this.db.PdQualityProductPreset
                                                    where pp.Materialid == pgroup.Key.Materialid && pp.Batcode == pgroup.Key.Batcode
                                                    select pp).FirstOrDefault()
                                  }).Where(p => p.Presetdata == null).ToList();
                    if (groups.Count > 0)
                    {
                        int result = 0;
                        foreach (var g in groups)
                        {
                            int id = 0;
                            var lastrs = this.db.PdQualityProductPreset.Where(p => p.Materialid == g.Materialid).OrderByDescending(p => p.Id).FirstOrDefault();
                            if (lastrs != null)
                            {
                                id = lastrs.Qid.Value;

                                var nextrs = this.db.PdQuality.Where(p => p.MaterialId == g.Materialid && p.Id > id).OrderBy(p => p.Id).FirstOrDefault();
                                if (nextrs != null)
                                {
                                    var preset = new PdQualityProductPreset()
                                    {
                                        Batcode = g.Batcode,
                                        Materialid = g.Materialid,
                                        Qid = nextrs.Id,
                                        Createtime = (int)Util.Extensions.GetCurrentUnixTime(),
                                    };

                                    this.db.PdQualityProductPreset.Add(preset);

                                    if (this.db.SaveChanges() > 0)
                                    {
                                        result++;
                                        continue;
                                    }
                                }
                            }

                            // 如果已经读到最后一条或者数据还是空的，则重新读第一条
                            var firstrs = this.db.PdQuality.Where(p => p.MaterialId == g.Materialid).OrderBy(p => p.Id).FirstOrDefault();
                            if (firstrs != null)
                            {
                                var preset = new PdQualityProductPreset()
                                {
                                    Batcode = g.Batcode,
                                    Materialid = g.Materialid,
                                    Qid = firstrs.Id,
                                    Createtime = (int)Util.Extensions.GetCurrentUnixTime(),
                                };

                                this.db.PdQualityProductPreset.Add(preset);

                                if (this.db.SaveChanges() > 0)
                                {
                                    result++;
                                }
                            }
                        }

                        if (result > 0)
                        {
                            return new ResponseModel(ApiResponseStatus.Success, "同步成功", result.ToString());
                        }
                        else
                        {
                            errorMessage = "同步后但没有影响行数！！！";
                        }
                    }
                    else
                    {
                        errorMessage = "没有需要同步的产品！！！";
                    }
                }
                else
                {
                    errorMessage = "请先添加好预置质量数据！！！";
                }
            }
            else
            {
                errorMessage = "流程版本不允许使用该功能！！！";
            }

            return new ResponseModel(ApiResponseStatus.Failed, "同步失败", errorMessage);
        }

        /// <summary>
        /// 流转数组
        /// </summary>
        /// <param name="stream">流文件</param>
        /// <returns>byte数组</returns>
        private byte[] StreamToBytes(Stream stream)
        {
            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);

            // 设置当前流的位置为流的开始
            stream.Seek(0, SeekOrigin.Begin);
            return bytes;
        }

        /// <summary>
        /// 根据规格ID获取实体
        /// </summary>
        /// <param name="specId">specId</param>
        /// <returns>实体</returns>
        private BaseSpecifications GetSpecById(int specId)
        {
            return this.db.BaseSpecifications.FirstOrDefault(c => c.Id == specId);
        }

        private PdBatcode SinglePrevById(int id, string shopcode)
        {
            return this.db.PdBatcode.OrderByDescending(c => c.Id).FirstOrDefault(c => c.Id < id && c.Batcode.StartsWith(shopcode));
        }

        /// <summary>
        /// 查找该炉批号是否有录入过的纪录
        /// </summary>
        /// <param name="batCode">BatCode</param>
        /// <returns>录入过的所有实体</returns>
        private List<PdQuality> CheckBatRecored(string batCode)
        {
            if (!string.IsNullOrEmpty(batCode))
            {
                return this.db.PdQuality.Where(c => c.Batcode == batCode).ToList();
            }
            else
            {
                return new List<PdQuality>();
            }
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

        /// <summary>
        /// 根据炉批号获得该材质名称与规格ID
        /// </summary>
        /// <param name="batCode">BatCode</param>
        /// <returns>材质名称与规格ID</returns>
        private List<string> GetMaterialSpecNameByBatCode(string batCode)
        {
            var query = this.db.PdProduct.FirstOrDefault(c => c.Batcode == batCode);

            if (query == null)
            {
                return null;
            }
            else
            {
                List<string> list_result = new List<string>();
                int materialId = query.Materialid.Value;
                var query_material = this.db.BaseProductMaterial.FirstOrDefault(c => c.Id == materialId);

                list_result.Add(query_material != null ? query_material.Name : string.Empty);
                list_result.Add(query.Specid.Value.ToString());

                return list_result;
            }
        }

        private List<string> GetMaterialSpecName(int mid)
        {
            var query_material = this.db.BaseProductMaterial.FirstOrDefault(c => c.Id == mid);
            List<string> list_result = new List<string>();
            list_result.Add(query_material != null ? query_material.Name : string.Empty);
            return list_result;
        }
    }
}