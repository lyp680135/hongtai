namespace Common.Service
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Common.IService;

    /// <summary>
    /// 单页数据读取服务
    /// </summary>
    public class SinglePageService : ISinglePageService
    {
        private DataLibrary.DataContext db;

        public SinglePageService(DataLibrary.DataContext dataContext)
        {
            this.db = dataContext;
        }

        /// <summary>
        /// 读取单页数据
        /// </summary>
        /// <param name="title">单面标题</param>
        /// <returns>单面数据实体</returns>
        public DataLibrary.SiteSinglePage GetSinglePageByTitle(string title)
        {
            DataLibrary.SiteSinglePage page = this.db.SiteSinglePage.FirstOrDefault(c => c.Title == title);
            if (page != null)
            {
                return page;
            }
            else
            {
                var pages = new DataLibrary.SiteSinglePage();
                return pages;
            }
        }
    }
}
