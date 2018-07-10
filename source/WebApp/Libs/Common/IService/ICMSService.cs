namespace Common.IService
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public interface ICMSService
    {
        IContentColumnDataService IcontentColumnDataService { get; set; }

        IUserService IuserService { get; set; }

        DataLibrary.SiteSinglePage GetSinglePage(string title);

        DataLibrary.SiteSinglePage GetSinglePage(int id);

        DataLibrary.MngSetting GetSetting();

        DataLibrary.SiteBasic GetBasic();

        List<DataLibrary.SiteLink> GetSiteLink(int position);
    }
}
