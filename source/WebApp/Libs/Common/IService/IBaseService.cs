﻿namespace Common.IService
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Text;

    public interface IBaseService<T>
         where T : class
    {
        /// <summary>
        /// 按条件返回单个对象
        /// </summary>
        /// <param name="exp">条件</param>
        /// <returns>T</returns>
        T FindSingle(Expression<Func<T, bool>> exp = null);

        /// <summary>
        /// 按条件判断是否存在对象
        /// </summary>
        /// <param name="exp">条件</param>
        /// <returns>bool</returns>
        bool IsExist(Expression<Func<T, bool>> exp);

        /// <summary>
        /// 按条件查询返回集合
        /// </summary>
        /// <param name="exp">条件</param>
        /// <returns>集合</returns>
        List<T> FindList(Expression<Func<T, bool>> exp = null);

        /// <summary>
        /// 分页
        /// </summary>
        /// <typeparam name="TKey">排序类型约束</typeparam>
        /// <param name="total">总数</param>
        /// <param name="pageindex">当前页索引</param>
        /// <param name="pagesize">显示页面大小</param>
        /// <param name="orderby">按照某个字段排序</param>
        /// <param name="wherelambda">条件</param>
        /// <param name="isorder">是否升序</param>
        /// <returns>list<T></returns>
        List<T> Page<TKey>(ref int total, int pageindex = 1, int pagesize = 10, Expression<Func<T, TKey>> orderby = null, Expression<Func<T, bool>> wherelambda = null, bool isorder = true);

        /// <summary>
        /// 按条件查询总条数
        /// </summary>
        /// <param name="exp">条件</param>
        /// <returns>集合</returns>
        int GetCount(Expression<Func<T, bool>> exp = null);

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="entity">实体</param>
        void Add(T entity);

        /// <summary>
        /// 批量添加
        /// </summary>
        /// <param name="entities">List<T></param>
        void BatchAdd(List<T> entities);

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="entity">实体</param>
        void Update(T entity);

        /// <summary>
        /// 批量修改
        /// </summary>
        /// <param name="entities">List<T></param>
        void BatchUpdate(List<T> entities);

        /// <summary>
        /// 删除实体
        /// </summary>
        /// <param name="entity">实体</param>
        void Delete(T entity);

        /// <summary>
        /// 批量删除
        /// </summary>
        /// <param name="entities">List<T></param>
        void BatchDelete(List<T> entities);
    }
}