namespace Common.Service
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;
    using Common.IService;
    using DataLibrary;
    using Microsoft.EntityFrameworkCore;

    public class BaseService<T> : IBaseService<T>
        where T : class
    {
        private DataContext dbContext;

        public BaseService(DataContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public void Add(T entity)
        {
            try
            {
                this.dbContext.Set<T>().Add(entity);
                this.dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void BatchAdd(List<T> entities)
        {
            try
            {
                using (var beginTransaction = this.dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        this.dbContext.Set<T>().AddRange(entities);
                        this.dbContext.SaveChanges();
                        beginTransaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        beginTransaction.Rollback();
                        throw ex;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void BatchDelete(List<T> entities)
        {
            try
            {
                using (var beginTransaction = this.dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        this.dbContext.Set<T>().RemoveRange(entities);
                        this.dbContext.SaveChanges();

                        beginTransaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        beginTransaction.Rollback();
                        throw ex;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void BatchUpdate(List<T> entities)
        {
            try
            {
                using (var beginTransaction = this.dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        this.dbContext.Set<T>().UpdateRange(entities);
                        this.dbContext.SaveChanges();

                        beginTransaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        beginTransaction.Rollback();
                        throw ex;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Delete(T entity)
        {
            try
            {
                this.dbContext.Set<T>().Remove(entity);
                this.dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Update(T entity)
        {
            try
            {
                this.dbContext.Set<T>().Update(entity);
                this.dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<T> FindList(Expression<Func<T, bool>> exp = null)
        {
            return this.Filter(exp).ToList();
        }

        public List<T> Page<TKey>(ref int total, int pageIndex = 1, int pageSize = 10, Expression<Func<T, TKey>> orderBy = null, Expression<Func<T, bool>> exp = null, bool isOrder = true)
        {
            if (pageIndex < 1)
            {
                pageIndex = 1;
            }

            IQueryable<T> data = isOrder ?
               this.dbContext.Set<T>().OrderBy(orderBy) :
               this.dbContext.Set<T>().OrderByDescending(orderBy);
            if (exp != null)
            {
                data = data.Where(exp).AsNoTracking();
            }

            total = data.ToList().Count;
            return data.Skip(pageSize * (pageIndex - 1)).Take(pageSize).ToList();
        }

        /// <summary>
        /// 按条件获取单个实体
        /// </summary>
        /// <param name="exp">条件</param>
        /// <returns>T</returns>
        public T FindSingle(Expression<Func<T, bool>> exp = null)
        {
            try
            {
                if (exp != null)
                {
                    return this.dbContext.Set<T>().AsNoTracking().FirstOrDefault(exp);
                }

                return this.dbContext.Set<T>().AsNoTracking().FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int GetCount(Expression<Func<T, bool>> exp = null)
        {
            return this.Filter(exp).Count();
        }

        public bool IsExist(Expression<Func<T, bool>> exp)
        {
            throw new NotImplementedException();
        }

        private IQueryable<T> Filter(Expression<Func<T, bool>> exp)
        {
            try
            {
                var dbSet = this.dbContext.Set<T>().AsNoTracking().AsQueryable();
                if (exp != null)
                {
                    dbSet = dbSet.Where(exp);
                }

                return dbSet;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
