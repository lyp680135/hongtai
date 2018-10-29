namespace Common.IService
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public interface IQualityService
    {
        /// <summary>
        /// 获取质量数据
        /// </summary>
        /// <param name="batcodeMidList">炉批号和材质id集合</param>
        /// <returns>list</returns>
        List<DataLibrary.PdQuality> GetQualityData(List<Tuple<string, int?>> batcodeMidList = null, bool isSimpleVer = false);
    }
}
