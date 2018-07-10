namespace Common.IService
{
    using System.Collections.Generic;
    using Newtonsoft.Json.Linq;

    public interface IContentGroupService
    {
        /// <summary>
        /// 根据ParId获取所有数据集合
        /// </summary>
        /// <param name="parId">父级Id</param>
        /// <returns>内容栏目集合</returns>
        List<DataLibrary.SiteCategory> GetContentGroupByParId(int parId);

        /// <summary>
        /// 根据父级ParId获取他的子级数
        /// </summary>
        /// <param name="parId">父级Id</param>
        /// <returns>子级数</returns>
        int GetChildNumByParId(int parId);

        /// <summary>
        /// 根据ParId递归获取他以下的所有数据集合
        /// </summary>
        /// <param name="parId">父级Id</param>
        /// <param name="cId">内容栏目Id</param>
        /// <returns>子级集合</returns>
        JArray GetAllDataForRecursion(int parId, int cId);

        /// <summary>
        /// 根据ParId获取所有子级Id
        /// </summary>
        /// <param name="list">子集Id集合</param>
        /// <param name="parId">父级Id</param>
        /// <returns>Id集合</returns>
        List<int> GetChildIdByParId(List<int> list, int parId);

        /// <summary>
        /// 根据模型Id获取模型名称
        /// </summary>
        /// <param name="modelId">模型Id</param>
        /// <returns>模型名称</returns>
        string GetModelNameById(int modelId);

        /// <summary>
        /// 根据栏目id获取栏目
        /// </summary>
        /// <param name="id">栏目id</param>
        /// <returns>栏目</returns>
        DataLibrary.SiteCategory GetSiteCategoryById(int id);

        /// <summary>
        /// 根据栏目名称获取栏目
        /// </summary>
        /// <param name="name">栏目名</param>
        /// <returns>栏目</returns>
        DataLibrary.SiteCategory GetSiteCategoryByName(string name);
    }
}
