namespace Common.IService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// 单页数据读取服务
    /// </summary>
    public interface ISinglePageService
    {
        /// <summary>
        /// 根据Title读取单页数据
        /// </summary>
        /// <param name="title">单页标题</param>
        /// <returns>数据实体</returns>
        DataLibrary.SiteSinglePage GetSinglePageByTitle(string title);
    }
}
