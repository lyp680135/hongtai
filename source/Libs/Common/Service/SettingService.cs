namespace Common.Service
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Common.IService;

    /// <summary>
    /// MngSetting表数据读取服务
    /// </summary>
    public class SettingService : ISettingService
    {
        private DataLibrary.DataContext db;

        public SettingService(DataLibrary.DataContext dataContext)
        {
            this.db = dataContext;
            this.MngSetting = this.GetSetting();
        }

        public DataLibrary.MngSetting MngSetting { get; private set; }

        private DataLibrary.MngSetting GetSetting()
        {
            DataLibrary.MngSetting setting = this.db.MngSetting.FirstOrDefault();
            if (setting != null)
            {
                return setting;
            }
            else
            {
                throw new Exception("系统配置表未初始化");
            }
        }
    }
}
