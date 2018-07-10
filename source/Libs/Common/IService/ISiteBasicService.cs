namespace Common.IService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// SiteBasic表数据读取服务
    /// </summary>
    public interface ISiteBasicService
    {
        /// <summary>
        /// Gets 当前网站基础配置
        /// </summary>
        DataLibrary.SiteBasic SiteBasic { get; }
    }
}
