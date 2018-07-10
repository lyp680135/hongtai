namespace Common.IService
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Common.Service;
    using DataLibrary;
    using Newtonsoft.Json.Linq;

    public interface IBatcodeService
    {
        /// <summary>
        /// 生成下一个轧制批号
        /// </summary>
        /// <param name="workshopid">车间号</param>
        /// <param name="shiftid">车间班组</param>
        /// <returns>返回生成的轧制批号</returns>
        CommonResult GenerateNextBatcode(int workshopid, int shiftid, bool must = false);

        /// <summary>
        /// 获得上一个批号
        /// </summary>
        /// <param name="batcode">当前批号</param>
        /// <returns>批号</returns>
        string GetPrevBatcode(string batcode);

        /// <summary>
        /// 获取当前车间的最近一个轧制批号
        /// </summary>
        /// <param name="shopcode">车间代码</param>
        /// <returns>批号</returns>
        PdBatcode SingleLast(string shopcode);
    }
}
