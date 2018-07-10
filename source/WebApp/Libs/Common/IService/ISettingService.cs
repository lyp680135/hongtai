namespace Common.IService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// MngSetting表数据读取服务
    /// </summary>
    public interface ISettingService
    {
        /// <summary>
        /// Gets 当前系统配置数据
        /// </summary>
        DataLibrary.MngSetting MngSetting { get; }
    }
}
