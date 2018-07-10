namespace Common.Service
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Common.IService;

    /// <summary>
    /// SiteBasic表数据读取服务
    /// </summary>
    public class SiteBasicService : ISiteBasicService
    {
        private DataLibrary.DataContext db;

        public SiteBasicService(DataLibrary.DataContext dataContext)
        {
            this.db = dataContext;
            this.SiteBasic = this.GetSiteBasic();
        }

        public DataLibrary.SiteBasic SiteBasic { get; private set; }

        private DataLibrary.SiteBasic GetSiteBasic()
        {
            DataLibrary.SiteBasic siteBasic = this.db.SiteBasic.FirstOrDefault();
            return siteBasic;
        }
    }
}
