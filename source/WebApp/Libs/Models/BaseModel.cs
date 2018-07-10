namespace Models
{
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// 所有Page页面Model的 父级
    /// </summary>
    /// <typeparam name="T">DbContext</typeparam>
    public class BaseModel<T> : PageModel
        where T : DbContext
    {
        public BaseModel(T db)
        {
            this.Db = db;
        }

        public BaseModel()
        {
        }

        /// <summary>
        /// Gets or sets 数据操作类
        /// </summary>
        public T Db { get; set; }

        /// <summary>
        /// Gets 统一分页的条数
        /// </summary>
        public virtual int PageSize
        {
            get { return 15; }
        }

        /// <summary>
        /// Gets 商交宝图片域名
        /// </summary>
        public virtual string IMGPath
        {
            get { return "http://img.31goods.com/uploadfile/siteadv/"; }
        }

        /// <summary>
        /// 跳转至错误页
        /// </summary>
        public virtual void RedirectToError()
        {
            this.Response.Redirect("/Error");
        }

        /// <summary>
        /// 跳转至错误页
        /// </summary>
        public virtual void RedirectToNoPermission()
        {
            this.Response.Redirect("/NoPermission");
        }
    }
}
