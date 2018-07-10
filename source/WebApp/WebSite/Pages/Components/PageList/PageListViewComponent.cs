namespace WarrantyManage.Components.PageList
{
    using System;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Util;

    [ViewComponent(Name = "PageList")]
    public class PageListViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(int pageSize, int pageIndex, int pageCount, string pageUrl, string pageText, int showCount, bool showAll = true)
        {
            string absUrl = this.Request.GetAbsoluteUri();

            if (showCount % 2 == 0)
            {
                showCount += 1;
            }

            int beginIndex = 0;
            int endIndex = 0;

            int halfCount = (int)Math.Floor(showCount * 0.5f);

            beginIndex = Math.Max(1, pageIndex - halfCount);
            endIndex = Math.Min(pageCount / pageSize > 0 ? (pageCount % pageSize == 0 ? pageCount / pageSize : (pageCount / pageSize) + 1) : 1, pageIndex + halfCount);

            if (endIndex < showCount)
            {
                endIndex = Math.Min(beginIndex - 1 + showCount, pageCount / pageSize > 0 ? (pageCount % pageSize == 0 ? pageCount / pageSize : (pageCount / pageSize) + 1) : 1);
            }

            if (beginIndex > (pageCount / pageSize > 0 ? pageCount / pageSize : 1) - showCount)
            {
                beginIndex = Math.Max(1, (pageCount / pageSize > 0 ? pageCount / pageSize : 1) - showCount + 1);
            }

            this.ViewData.Add("CurrentIndex", pageIndex);
            this.ViewData.Add("BeginIndex", beginIndex);
            this.ViewData.Add("PageCount", pageCount);
            this.ViewData.Add("PageUrl", pageUrl);
            this.ViewData.Add("PageText", pageText);
            this.ViewData.Add("EndIndex", endIndex);
            this.ViewData.Add("ShowAll", showAll);

            return this.View();
        }
    }
}
