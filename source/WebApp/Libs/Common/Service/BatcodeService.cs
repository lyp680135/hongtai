namespace Common.Service
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using Common.IService;
    using DataLibrary;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using static DataLibrary.EnumList;

    public class BatcodeService : IBatcodeService
    {
        private static string workshopcodeTag = "@";
        private static string workshiftcodeTag = "$";
        private static string yearTag = "yy";
        private static string monthTag = "MM";
        private static string dayTag = "dd";
        private static string numberTag = "0000";

        private string mDefaultFormat = workshopcodeTag + yearTag + monthTag + numberTag;

        private DataLibrary.DataContext db;

        public BatcodeService(DataLibrary.DataContext dataContext)
        {
            this.db = dataContext;
        }

        /// <summary>
        /// 生成车间的下一个批号
        /// </summary>
        /// <param name="workshopid">车间ID</param>
        /// <param name="shiftid">车间班组</param>
        /// <param name="must">强制生成(不管当前批号状态)</param>
        /// <returns>批号</returns>
        public CommonResult GenerateNextBatcode(int workshopid, int shiftid, bool must = false)
        {
            if (must && this.mDefaultFormat.IndexOf(workshiftcodeTag) != -1 && shiftid <= 0)
            {
                return new CommonResult(CommonResultStatus.Failed, "生成批号失败", "班组编号不正确,不满足生成规则");
            }

            var workshop = this.db.PdWorkshop.FirstOrDefault(p => p.Id == workshopid);
            if (workshop != null)
            {
                var shift = this.db.PdWorkshopTeam.FirstOrDefault(p => p.Id == shiftid);
                if (shift != null)
                {
                    return this.GetBatcode(string.Empty, 1, workshop.Code, shift.Code, must);
                }
                else
                {
                    return this.GetBatcode(string.Empty, 1, workshop.Code, null, must);
                }
            }

            return new CommonResult(CommonResultStatus.Failed, "生成批号失败", "车间号不正确！");
        }

        public string GetPrevBatcode(string batcode)
        {
            var shopcode_position = this.mDefaultFormat.IndexOf(BatcodeService.workshopcodeTag);
            if (shopcode_position != -1)
            {
                string code = batcode.Substring(shopcode_position, BatcodeService.workshopcodeTag.Length);
                var model = this.SingleByBatcode(batcode);
                if (model != null)
                {
                    return this.GetBatcode(batcode, -1, code, null).Data.ToString();
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// 获取当前车间的最后一个批号实体
        /// </summary>
        /// <param name="shopcode">车间代码</param>
        /// <returns>批号实体</returns>
        public PdBatcode SingleLast(string shopcode)
        {
            // 根据批号规则，获取符合条件的批号
            var position = this.mDefaultFormat.IndexOf(BatcodeService.workshopcodeTag);
            if (position != -1)
            {
                return this.db.PdBatcode.OrderByDescending(c => c.Id).FirstOrDefault(c => c.Batcode.Substring(position, 1) == shopcode);
            }

            return null;
        }

        /// <summary>
        /// 根据批号获取当前批号实体
        /// </summary>
        /// <param name="batcode">当前批号</param>
        /// <returns>批号实体</returns>
        private PdBatcode SingleByBatcode(string batcode)
        {
            return this.db.PdBatcode.OrderByDescending(c => c.Id).FirstOrDefault(c => c.Batcode == batcode);
        }

        /// <summary>
        /// 获取当前批号的下一条批号实体
        /// </summary>
        /// <param name="id">当前批号ID</param>
        /// <param name="shopcode">车间代码</param>
        /// <returns>批号实体</returns>
        private PdBatcode SingleNextById(int id, string shopcode)
        {
            // 根据批号规则，获取符合条件的批号
            var position = this.mDefaultFormat.IndexOf(BatcodeService.workshopcodeTag);
            if (position != -1)
            {
                return this.db.PdBatcode.OrderBy(c => c.Id).FirstOrDefault(c => c.Id > id && c.Batcode.Substring(position, 1) == shopcode);
            }

            return null;
        }

        /// <summary>
        /// 获取当前批号的上一条批号实体
        /// </summary>
        /// <param name="id">当前批号ID</param>
        /// <param name="shopcode">车间代码</param>
        /// <returns>批号实体</returns>
        private PdBatcode SinglePrevById(int id, string shopcode)
        {
            var position = this.mDefaultFormat.IndexOf(BatcodeService.workshopcodeTag);
            if (position != -1)
            {
                return this.db.PdBatcode.OrderByDescending(c => c.Id).Where(c => c.Id < id && c.Batcode.Substring(position, 1) == shopcode).FirstOrDefault();
            }

            return null;
        }

        /// <summary>
        /// 获取轧制批号(轧制批号生成算法)
        /// </summary>
        /// <param name="curcode">当前炉批号，无则传空</param>
        /// <param name="offset">上下相加的数（增加则为1，减少为-1）</param>
        /// <param name="workshopcode">生产车间代码</param>
        /// <param name="workshiftcode">车间班组代码</param>
        /// <param name="must">强制生成(不管当前批号状态)</param>
        /// <returns>生成的轧制批号</returns>
        private CommonResult GetBatcode(string curcode, int offset, string workshopcode, string workshiftcode, bool must = false)
        {
            string batcode = string.Empty;

            // 查询数据库里最后一条批号
            PdBatcode curritem = this.SingleLast(workshopcode);

            // 如果是刚打开程序初始读取批号
            if (string.IsNullOrEmpty(batcode) && offset == 0)
            {
                if (curritem != null)
                {
                    batcode = curritem.Batcode;

                    return new CommonResult(CommonResultStatus.Failed, "生成失败", "返回最后一条轧制批号！", batcode);
                }
            }

            // 根据当前批号从数据库获取批号记录标识，以方便找上一条下一条
            if (!string.IsNullOrEmpty(curcode))
            {
                curritem = this.SingleByBatcode(curcode);
            }

            // 如果当前批号为空并且数据库里也没有
            if (curritem == null || curritem.Id <= 0)
            {
                if (must == true)
                {
                    batcode = this.GenerateRealCode(workshopcode, workshiftcode, 1);

                    var model = new PdBatcode();
                    model.Batcode = batcode;
                    model.Createtime = (int)Util.Extensions.GetCurrentUnixTime();
                    model.Status = 0;
                    model.Adder = "BatcodeService";

                    this.db.PdBatcode.Add(model);

                    // 如果没有保存成功，则直接返回空
                    if (this.db.SaveChanges() <= 0)
                    {
                        return new CommonResult(CommonResultStatus.Failed, "生成失败", "新记录保存不成功！");
                    }

                    return new CommonResult(CommonResultStatus.Success, "生成成功", null, batcode);
                }
                else
                {
                    return new CommonResult(CommonResultStatus.Failed, "生成失败", "还没有生成任何批号！");
                }
            }
            else
            {
                if (offset > 0)
                {
                    var pdcode = this.SingleNextById(curritem.Id, workshiftcode);

                    if (pdcode != null)
                    {
                        return new CommonResult(CommonResultStatus.Failed, "生成失败", "返回当前下一条轧制批号！", pdcode.Batcode);
                    }
                }
            }

            // 分隔字符串，获取日期、以及序号
            string code = curritem.Batcode;

            int year_position = this.mDefaultFormat.IndexOf(BatcodeService.yearTag);
            int month_position = this.mDefaultFormat.IndexOf(BatcodeService.monthTag);
            int day_position = this.mDefaultFormat.IndexOf(BatcodeService.dayTag);
            int number_position = this.mDefaultFormat.IndexOf(BatcodeService.numberTag);

            string yearstr = string.Empty,
                monthstr = string.Empty,
                daystr = string.Empty,
                numberstr = string.Empty;
            string year_code = DateTime.Now.ToString(BatcodeService.yearTag);
            string month_code = DateTime.Now.ToString(BatcodeService.monthTag);
            string day_code = DateTime.Now.ToString(BatcodeService.dayTag);

            bool issamedate = true;
            if (year_position != -1)
            {
                yearstr = code.Substring(year_position, BatcodeService.yearTag.Length);

                if (yearstr != year_code)
                {
                    issamedate = false;
                }
            }

            if (month_position != -1)
            {
                monthstr = code.Substring(month_position, BatcodeService.monthTag.Length);

                if (monthstr != month_code)
                {
                    issamedate = false;
                }
            }

            if (day_position != -1)
            {
                daystr = code.Substring(day_position, BatcodeService.dayTag.Length);

                if (daystr != day_code)
                {
                    issamedate = false;
                }
            }

            if (number_position != -1)
            {
                numberstr = code.Substring(number_position, BatcodeService.numberTag.Length);
            }

            // 如果找不到序号，则直接返回
            if (string.IsNullOrEmpty(numberstr))
            {
                return new CommonResult(CommonResultStatus.Failed, "生成失败", "在批号中找不到序号段！");
            }

            // 如果是同一个时间周期，则在批号后做运算
            if (issamedate)
            {
                int.TryParse(numberstr, out int number);
                number += offset;

                if (number > 0)
                {
                    // 如果是下一个，则为新生成的炉号，需要插入到数据库中
                    if (offset > 0)
                    {
                        if (curritem.Status == 1 || must == true)
                        {
                            batcode = this.GenerateRealCode(workshopcode, workshiftcode, number);

                            var model = new PdBatcode();
                            model.Batcode = batcode;
                            model.Createtime = (int)Util.Extensions.GetCurrentUnixTime();
                            model.Status = 0;
                            model.Adder = "BatcodeService";

                            this.db.PdBatcode.Add(model);

                            // 如果没有保存成功，则直接返回空
                            if (this.db.SaveChanges() <= 0)
                            {
                                return new CommonResult(CommonResultStatus.Failed, "生成失败", "新记录保存不成功！");
                            }

                            // 生成成功后, 更新前一个批号的状态
                            if (curritem.Status == 0)
                            {
                                curritem.Status = 1;

                                this.db.PdBatcode.Update(curritem);
                            }

                            return new CommonResult(CommonResultStatus.Success, "生成成功", null, batcode);
                        }
                        else
                        {
                            // 如果当前批号没有已使用，则仍然使用当前批号，不能跳过
                            batcode = curritem.Batcode;
                        }
                    }
                }

                // 如果炉号到0，则表示已经到了上一个时间周期最后一个炉号
                else
                {
                    var pdcode = this.SinglePrevById(curritem.Id, workshopcode);
                    if (pdcode != null)
                    {
                        batcode = pdcode.Batcode;
                    }

                    // 如果上一个周期的批号没有，则继续保持当前批号，即返回空值
                }
            }

            // 如果不是当前时间周期的，则为历史记录，直接在数据库里取上一个和下一个
            else
            {
                if (offset > 0)
                {
                    var pdcode = this.SingleNextById(curritem.Id, workshopcode);

                    if (pdcode != null)
                    {
                        batcode = pdcode.Batcode;
                    }

                    // 如果数据库没有，则生成新的
                    // 如果当前批号为空并且数据库里也没有
                    if (string.IsNullOrEmpty(batcode))
                    {
                        batcode = this.GenerateRealCode(workshopcode, workshiftcode, 1);
                        if (must == true)
                        {
                            var model = new PdBatcode();
                            model.Batcode = batcode;
                            model.Createtime = (int)Util.Extensions.GetCurrentUnixTime();
                            model.Status = 0;
                            model.Adder = "BatcodeService";

                            this.db.PdBatcode.Add(model);

                            // 如果没有保存成功，则直接返回空
                            if (this.db.SaveChanges() <= 0)
                            {
                                return new CommonResult(CommonResultStatus.Failed, "生成失败", "新记录保存不成功！");
                            }

                            // 生成成功后, 更新前一个批号的状态
                            if (curritem.Status == 0)
                            {
                                curritem.Status = 1;

                                this.db.PdBatcode.Update(curritem);
                            }

                            return new CommonResult(CommonResultStatus.Success, "生成成功", null, batcode);
                        }

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
                    var pdcode = this.SinglePrevById(curritem.Id, workshopcode);
                    if (pdcode != null)
                    {
                        return new CommonResult(CommonResultStatus.Failed, "生成失败", "返回上一条记录", pdcode.Batcode);
                    }

                    // 如果上一条的批号没有，则继续保持当前批号，即返回空值
                }
            }

            return new CommonResult(CommonResultStatus.Failed, "生成失败", "返回指定条件批号", batcode);
        }

        /// <summary>
        /// 正式生成轧制批号
        /// </summary>
        /// <param name="workshopcode">车间代码</param>
        /// <param name="workshiftcode">车间班组代码</param>
        /// <param name="number">序号</param>
        /// <returns>批号</returns>
        private string GenerateRealCode(string workshopcode, string workshiftcode, int number)
        {
            // 一个组件一个组件进行替换
            StringBuilder realbatcode = new StringBuilder(this.mDefaultFormat);

            string year_code = DateTime.Now.ToString(BatcodeService.yearTag);
            string month_code = DateTime.Now.ToString(BatcodeService.monthTag);
            string day_code = DateTime.Now.ToString(BatcodeService.dayTag);

            bool hasshopcode = false, hasshiftcode = false;

            var shopcode_position = this.mDefaultFormat.IndexOf(BatcodeService.workshopcodeTag);
            if (shopcode_position != -1)
            {
                hasshopcode = true;
            }

            var shiftcode_position = this.mDefaultFormat.IndexOf(BatcodeService.workshiftcodeTag);
            if (shiftcode_position != -1)
            {
                hasshiftcode = true;
            }

            if (hasshopcode)
            {
                realbatcode = realbatcode.Replace(BatcodeService.workshopcodeTag, workshopcode);
            }

            if (hasshiftcode)
            {
                realbatcode = realbatcode.Replace(BatcodeService.workshiftcodeTag, workshiftcode);
            }

            realbatcode = realbatcode.Replace(BatcodeService.yearTag, year_code);
            realbatcode = realbatcode.Replace(BatcodeService.monthTag, month_code);
            realbatcode = realbatcode.Replace(BatcodeService.dayTag, day_code);

            // 生成序号
            int number_len = BatcodeService.numberTag.Length;
            string realnumber = string.Format("{0}", number.ToString().PadLeft(number_len, '0'));

            realbatcode = realbatcode.Replace(BatcodeService.numberTag, realnumber);

            return realbatcode.ToString();
        }
    }
}
