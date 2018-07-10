namespace Common.Service
{
    using System.Collections.Generic;
    using System.Linq;
    using Common.IService;
    using DataLibrary;

    public class CMSService : ICMSService
    {
        private readonly DataLibrary.DataContext db;

        public CMSService(DataLibrary.DataContext db, IUserService userService, ISinglePageService singlePageService, ISettingService settingService, ISiteBasicService siteBasicService, IContentColumnDataService contentColumnDataService)
        {
            this.db = db;
            this.IsinglepageService = singlePageService;
            this.ISettingService = settingService;
            this.IsiteBasicService = siteBasicService;
            this.IcontentColumnDataService = contentColumnDataService;
            this.IuserService = userService;
        }

        public IContentColumnDataService IcontentColumnDataService { get; set; }

        public IUserService IuserService { get; set; }

        private ISinglePageService IsinglepageService { get; set; }

        private ISettingService ISettingService { get; set; }

        private ISiteBasicService IsiteBasicService { get; set; }

        /// <summary>
        /// 获取单页数据
        /// </summary>
        /// <param name="title">标题</param>
        /// <returns>单页数据</returns>
        public DataLibrary.SiteSinglePage GetSinglePage(string title)
        {
            return this.IsinglepageService.GetSinglePageByTitle(title);
        }

        /// <summary>
        /// 获取单页数据
        /// </summary>
        /// <param name="id">标题</param>
        /// <returns>单页数据</returns>
        public DataLibrary.SiteSinglePage GetSinglePage(int id)
        {
            return this.db.SiteSinglePage.FirstOrDefault(c => c.Id == id);
        }

        /// <summary>
        /// 获取配置表数据
        /// </summary>
        /// <returns>配置表数据</returns>
        public DataLibrary.MngSetting GetSetting()
        {
            return this.ISettingService.MngSetting;
        }

        /// <summary>
        /// 获取基础数据表
        /// </summary>
        /// <returns>基础数据表</returns>
        public DataLibrary.SiteBasic GetBasic()
        {
            return this.IsiteBasicService.SiteBasic;
        }

        /// <summary>
        /// 获取友情链接
        /// </summary>
        /// <returns>友情链接集合</returns>
        public List<SiteLink> GetSiteLink(int position)
        {
            return this.db.SiteLink.Where(c => c.IsShow == (int)EnumList.SiteIsShow.是 && c.Position == position).OrderByDescending(c => c.Sequence).ToList();
        }
    }
}
