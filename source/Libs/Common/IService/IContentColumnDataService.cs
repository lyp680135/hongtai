namespace Common.IService
{
    using System.Collections.Generic;

    public interface IContentColumnDataService
    {
        /// <summary>
        /// 根据栏目id获取该栏目下所有数据
        /// </summary>
        /// <param name="cid">内容栏目id</param>
        /// <param name="page">页码数</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="where">查询条件，可以不传</param>
        /// <returns>该栏目所有数据</returns>
        List<Dictionary<string, object>> GetContentColumnDataList(int cid, int? page, int? pageSize, string where = null);

        /// <summary>
        /// 根据栏目名称获取该栏目下所有数据
        /// </summary>
        /// <param name="contentName">栏目名称</param>
        /// <param name="page">页码数</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="where">查询条件，可以不传</param>
        /// <returns>该栏目所有数据</returns>
        List<Dictionary<string, object>> GetContentColumnDataList(string contentName, int? page, int? pageSize, string where = null);

        /// <summary>
        /// 根据栏目ID获取当前栏目的内容总数
        /// </summary>
        /// <param name="cid">栏目ID</param>
        /// <param name="where">条件</param>
        /// <returns>Count</returns>
        int GetContentCount(int cid, string where = null);

        /// <summary>
        /// 根据栏目名称获取当前栏目的内容总数
        /// </summary>
        /// <param name="contentName">栏目名称</param>
        /// <param name="where">条件</param>
        /// <returns>Count</returns>
        int GetContentCount(string contentName, string where = null);

        /// <summary>
        /// 根据栏目内容id和所属栏目id获取栏目内容
        /// </summary>
        /// <param name="id">栏目内容id</param>
        /// <param name="cid">栏目id</param>
        /// <returns>栏目内容</returns>
        Dictionary<string, object> GetContentColumnData(int id, int cid);

        /// <summary>
        /// 根据栏目id和字段描述查询该栏目绑定的字段模型
        /// </summary>
        /// <param name="cid">栏目id</param>
        /// <param name="description">字段描述</param>
        /// <returns>字段模型</returns>
        List<DataLibrary.SiteModelColumn> GetSiteModelColumnByCid(int cid, string description);
    }
}
