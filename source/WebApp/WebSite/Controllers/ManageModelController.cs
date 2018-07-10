namespace WarrantyManage.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Common.IService;
    using Common.Service;
    using DataLibrary;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    public class ManageModelController : Controller
    {
        private DataLibrary.DataContext db;
        private IUserService userService;
        private Util.Helpers.MySqlHelper mySqlHelper;

        // 注入DataContext
        public ManageModelController(DataLibrary.DataContext db, IUserService userService, Util.Helpers.MySqlHelper mySqlHelper)
        {
            this.db = db;
            this.userService = userService;
            this.mySqlHelper = mySqlHelper;
        }

        /// <summary>
        /// 添加模型的方法
        /// </summary>
        /// <param name="rowsData">请求数据</param>
        /// <param name="modelDes">模型名称</param>
        /// <returns>ActionResult</returns>
        public ActionResult AddModel(string rowsData, string modelDes)
        {
            var msg = string.Empty;
            if (string.IsNullOrEmpty(rowsData))
            {
                msg += "数据为空,";
            }

            if (string.IsNullOrEmpty(modelDes))
            {
                msg += "模型描述为空";
            }

            if (!string.IsNullOrEmpty(msg))
            {
                return this.AjaxResult(false, msg);
            }

            // 不追踪查询,提高查询效率
            var manageModelInfo = this.db.SiteModel.Where(w => w.Description == modelDes).AsNoTracking().ToList();

            // 根据模型名查询模型,校验模型是否存在
            if (manageModelInfo.Count() > 0)
            {
                return this.AjaxResult(false, "该模型已存在");
            }

            // 截取Rows
            var rowList = rowsData.Substring(0, rowsData.Length - 1).Split('$');

            try
            {
                var fileNameList = new List<string>();
                foreach (var item in rowList)
                {
                    var baseMangeModel = item.Split(';');
                    if (!fileNameList.Any(a => a == baseMangeModel[0]))
                    {
                        fileNameList.Add(baseMangeModel[0]);
                    }
                }

                if (rowList.Count() != fileNameList.Count)
                {
                    return this.AjaxResult(false, "存在重复字段,无法添加");
                }

                foreach (var item in rowList)
                {
                    var baseMangeModel = item.Split(';');
                    this.db.SiteModelColumn.Add(new SiteModelColumn
                    {
                        FildName = baseMangeModel[0],
                        FildDescription = baseMangeModel[1],
                        FildType = (EnumList.FileType)Convert.ToInt32(baseMangeModel[2]),
                        FildLength = Convert.ToInt32(baseMangeModel[3]),
                        FildIsNull = Convert.ToInt32(baseMangeModel[4]),
                        PageShowType = (EnumList.PageShowType)Convert.ToInt32(baseMangeModel[5]),
                        FildValue = string.Join(",", this.GetKeys(baseMangeModel[6])),

                        // 权重,按照枚举值来
                        FildWeight = Convert.ToInt32(baseMangeModel[5])
                    });
                    this.db.SaveChanges();
                }

                // 查询出,刚才添加数据的主键
                string sqlText = string.Format(@" select * from sitemodelcolumn order by Id desc limit {0} ", rowList.Count());
                var baseManageModelList = this.db.SiteModelColumn.FromSql(sqlText).ToList();
                List<int> bmmId = new List<int>();
                baseManageModelList.ForEach(o =>
                {
                    // 不存在就添加
                    if (!bmmId.Any(x => x == o.Id))
                    {
                        bmmId.Add(o.Id);
                    }
                });

                this.db.SiteModel.Add(new SiteModel
                {
                    BaseManageId = string.Join(",", bmmId),
                    CreateTime = (int)Util.Extensions.GetCurrentUnixTime(),
                    Description = modelDes,
                    Uid = this.userService.ApplicationUser.Mng_admin.Id,
                    CheckStatus_ManageModel = EnumList.CheckStatus_ManageModel.等待审核
                });

                this.db.SaveChanges();

                // 查询出最新的一条数据
                var manageModelByDes = this.db.SiteModel.OrderByDescending(o => o.Id).FirstOrDefault(f => f.Description == modelDes);
                if (manageModelByDes == null)
                {
                    return this.AjaxResult(false, "模型:" + modelDes + "添加失败");
                }

                // 表名
                string tableName = "sitecontent_" + manageModelByDes.Id.ToString();

                // 创建数据库
                this.db.Database.ExecuteSqlCommand(this.GetSqlStr(tableName, baseManageModelList));
            }

            // 如果有异常就删除刚才的表数据
            catch
            {
                var manageModel = this.db.SiteModel.OrderByDescending(o => o.Id).FirstOrDefault(f => f.Description == modelDes);
                if (manageModel != null)
                {
                    // 先删除模型实体
                    this.db.SiteModel.Remove(manageModel);
                    this.db.SaveChanges();

                    // 找出该模型下的所有字段
                    var bmmId = manageModel.BaseManageId.Split(',').ToList();
                    var baseManageModeList = new List<SiteModelColumn>();
                    bmmId.ForEach(o =>
                    {
                        // 一定要加不追踪查询(AsNoTracking),不然会报错
                        var baseMangeModelInfo = this.db.SiteModelColumn.FirstOrDefault(f => f.Id == Convert.ToInt32(o));
                        if (baseMangeModelInfo != null)
                        {
                            baseManageModeList.Add(baseMangeModelInfo);
                        }
                    });
                    this.db.SiteModelColumn.RemoveRange(baseManageModeList);
                    this.db.SaveChanges();
                }

                return this.AjaxResult(false, "添加失败");
            }

            return this.AjaxResult(true, "添加成功");
        }

        /// <summary>
        /// 查看详情
        /// </summary>
        /// <param name="id">模型id</param>
        /// <returns>ActionResult</returns>
        public ActionResult ListModel(int id)
        {
            var manageModel = this.db.SiteModel.FirstOrDefault(f => f.Id == id);
            if (manageModel == null)
            {
                return this.AjaxResult(false, "不存在该模型");
            }

            var bmmId = manageModel.BaseManageId;
            if (string.IsNullOrEmpty(bmmId))
            {
                return this.AjaxResult(false, "该模型无字段");
            }

            var idList = bmmId.Split(',').ToList();
            List<SiteModelColumn> bmmList = new List<SiteModelColumn>();
            idList.ForEach(o =>
            {
                var baseManageModelInfo = this.db.SiteModelColumn.FirstOrDefault(f => f.Id == Convert.ToInt32(o));
                if (baseManageModelInfo != null)
                {
                    bmmList.Add(baseManageModelInfo);
                }
            });
            return this.AjaxResult(true, bmmList);
        }

        /// <summary>
        /// 编辑模型
        /// </summary>
        /// <param name="manageModelId">模型id</param>
        /// <param name="rowsData">请求数据</param>
        /// <param name="modelDes">模型名称</param>
        /// <returns>ActionResult</returns>
        public ActionResult EditModel(int manageModelId, string rowsData, string modelDes)
        {
            // 数据校验
            if (manageModelId <= 0)
            {
                return this.AjaxResult(false, "模型编号为空");
            }

            if (string.IsNullOrEmpty(rowsData))
            {
                return this.AjaxResult(false, "编辑模型数据为空");
            }

            if (string.IsNullOrEmpty(modelDes))
            {
                return this.AjaxResult(false, "模型描述为空");
            }

            var manageModelInfo = this.db.SiteModel.AsNoTracking().FirstOrDefault(f => f.Id == manageModelId);
            if (manageModelInfo == null)
            {
                return this.AjaxResult(false, "不存在该模型,无法编辑");
            }

            var manageModelByDes = this.db.SiteModel.AsNoTracking().FirstOrDefault(f => f.Description == modelDes);

            if (manageModelByDes != null && manageModelId != manageModelByDes.Id)
            {
                return this.AjaxResult(false, "已存在该模型名称,无法编辑");
            }

            // 分表表名
            string tableName = "sitecontent_" + manageModelId.ToString();
            var bmmId = manageModelInfo.BaseManageId.Split(',').ToList();
            if (bmmId.Count <= 0)
            {
                return this.AjaxResult(false, "该模型无字段,无法编辑");
            }

            // 找出该模型下的所有字段
            var baseManageModeList = new List<SiteModelColumn>();
            bmmId.ForEach(o =>
            {
                // 一定要加不追踪查询(AsNoTracking),不然会报错
                var baseMangeModelInfo = this.db.SiteModelColumn.AsNoTracking().FirstOrDefault(f => f.Id == Convert.ToInt32(o));
                if (baseMangeModelInfo != null)
                {
                    baseManageModeList.Add(baseMangeModelInfo);
                }
            });
            if (baseManageModeList.Count <= 0)
            {
                return this.AjaxResult(false, "该模型无字段,无法编辑");
            }

            try
            {
                // 先做校验
                var rowList = rowsData.Substring(0, rowsData.Length - 1).Split('$');
                var fileNameList = new List<string>();
                foreach (var item in rowList)
                {
                    var baseMangeModel = item.Split(';');
                    if (!fileNameList.Any(a => a == baseMangeModel[0]))
                    {
                        fileNameList.Add(baseMangeModel[0]);
                    }
                }

                if (rowList.Count() != fileNameList.Count)
                {
                    return this.AjaxResult(false, "存在重复字段,无法添加");
                }

                // 先删除该模型下的所有字段
                this.db.SiteModelColumn.RemoveRange(baseManageModeList);
                this.db.SaveChanges();

                // 删除分表
                string sqlDelTable = string.Format(" drop table if exists {0}", tableName);
                this.db.Database.ExecuteSqlCommand(sqlDelTable);

                foreach (var item in rowList)
                {
                    var baseMangeModel = item.Split(';');
                    this.db.SiteModelColumn.Add(new SiteModelColumn
                    {
                        FildName = baseMangeModel[0],
                        FildDescription = baseMangeModel[1],
                        FildType = (EnumList.FileType)Convert.ToInt32(baseMangeModel[2]),
                        FildLength = Convert.ToInt32(baseMangeModel[3]),
                        FildIsNull = Convert.ToInt32(baseMangeModel[4]),
                        PageShowType = (EnumList.PageShowType)Convert.ToInt32(baseMangeModel[5]),
                        FildValue = string.Join(",", this.GetKeys(baseMangeModel[6])),

                        // 排序按照枚举值来
                        FildWeight = Convert.ToInt32(baseMangeModel[5])
                    });
                    this.db.SaveChanges();
                }

                // 查询出,刚才添加数据的主键
                string sqlText = string.Format(@" select * from sitemodelcolumn order by Id desc limit {0} ", rowList.Count());
                var baseManageModelList = this.db.SiteModelColumn.FromSql(sqlText).ToList();
                List<int> newbmmId = new List<int>();
                baseManageModelList.ForEach(o =>
                {
                    // 不存在就添加
                    if (!newbmmId.Any(x => x == o.Id))
                    {
                        newbmmId.Add(o.Id);
                    }
                });

                // 最后修改模型
                this.db.SiteModel.Update(new SiteModel
                {
                    Id = manageModelId,
                    BaseManageId = string.Join(",", newbmmId),
                    Description = modelDes,
                    CreateTime = (int)Util.Extensions.GetCurrentUnixTime(),
                    Uid = this.userService.ApplicationUser.Mng_admin.Id,
                    CheckStatus_ManageModel = manageModelInfo.CheckStatus_ManageModel
                });
                this.db.SaveChanges();

                // 创建数据库
                this.db.Database.ExecuteSqlCommand(this.GetSqlStr(tableName, baseManageModelList));
            }

            // 如果有异常就删除刚才的表数据
            catch
            {
                var manageModel = this.db.SiteModel.OrderByDescending(o => o.Id).FirstOrDefault(f => f.Id == manageModelId);
                if (manageModel != null)
                {
                    // 先删除模型实体
                    this.db.SiteModel.Remove(manageModel);
                    this.db.SaveChanges();

                    // 找出该模型下的所有字段
                    var bmmIds = manageModel.BaseManageId.Split(',').ToList();
                    var baseManageModeLists = new List<SiteModelColumn>();
                    bmmIds.ForEach(o =>
                    {
                        var baseMangeModelInfo = this.db.SiteModelColumn.FirstOrDefault(f => f.Id == Convert.ToInt32(o));
                        if (baseMangeModelInfo != null)
                        {
                            baseManageModeLists.Add(baseMangeModelInfo);
                        }
                    });
                    this.db.SiteModelColumn.RemoveRange(baseManageModeLists);
                    this.db.SaveChanges();
                }

                return this.AjaxResult(false, "修改失败");
            }

            return this.AjaxResult(true, "编辑成功");
        }

        /// <summary>
        /// 删除模型逻辑
        /// </summary>
        /// <param name="ids">模型Id</param>
        /// <returns>ActionResult</returns>
        public ActionResult DeleteModel(string ids)
        {
            var idList = ids.Substring(0, ids.Length - 1).Split(';').ToList();
            var manageModelList = this.db.SiteModel.Where(w => idList.Contains(w.Id.ToString())).ToList();
            if (manageModelList == null || manageModelList.Count <= 0)
            {
                return this.AjaxResult(false, "不存在模型,无法删除");
            }

            using (var tran = this.db.Database.BeginTransaction())
            {
                foreach (var item in idList)
                {
                    var contentGroup = this.db.SiteCategory.Where(w => w.ModelId == Convert.ToInt16(item)).Select(s => s.ModelId).ToList();
                    if (contentGroup.Count <= 0)
                    {
                        try
                        {
                            var manageModel = this.db.SiteModel.FirstOrDefault(f => f.Id == Convert.ToInt16(item));
                            if (manageModel != null)
                            {
                                // 先删除模型实体
                                this.db.SiteModel.Remove(manageModel);
                                this.db.SaveChanges();

                                // 找出该模型下的所有字段
                                var bmmId = manageModel.BaseManageId.Split(',').ToList();
                                var baseManageModeList = new List<SiteModelColumn>();
                                bmmId.ForEach(o =>
                                {
                                    // 一定要加不追踪查询(AsNoTracking),不然会报错
                                    var baseMangeModelInfo = this.db.SiteModelColumn.AsNoTracking().FirstOrDefault(f => f.Id == Convert.ToInt32(o));
                                    if (baseMangeModelInfo != null)
                                    {
                                        baseManageModeList.Add(baseMangeModelInfo);
                                    }
                                });
                                this.db.SiteModelColumn.RemoveRange(baseManageModeList);
                                this.db.SaveChanges();

                                // 然后删除分表
                                string tableName = string.Format("sitecontent_{0}", item.ToString());
                                string sqlStr = string.Format("drop table if exists {0}", tableName);
                                this.db.Database.ExecuteSqlCommand(sqlStr);
                            }
                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();
                            return this.AjaxResult(false, ex.Message);
                            throw ex;
                        }
                    }
                }

                tran.Commit();
            }

            return this.AjaxResult(true, "删除成功");
        }

        /// <summary>
        /// 检查模型
        /// </summary>
        /// <param name="id">id</param>
        /// <returns>ActionResult</returns>
        public ActionResult CheckModel(int id)
        {
            string tableName = "sitecontent_" + id.ToString();
            string sqlText = string.Format(@"select count(0) from " + tableName + " ");
            string tableExits = string.Format(@"select COUNT(1) as count from INFORMATION_SCHEMA.TABLES where TABLE_NAME='{0}'", tableName);
            if (System.Convert.ToInt32(this.mySqlHelper.ExecuteEntity(tableExits, CommandType.Text, null)) <= 0)
            {
                return this.AjaxResult(false, "该模型表不存在,无法编辑");
            }

            int count = System.Convert.ToInt32(this.mySqlHelper.ExecuteEntity(sqlText, CommandType.Text, null));
            if (count > 0)
            {
                return this.AjaxResult(false, "该模型已存在内容,无法编辑");
            }

            return this.AjaxResult(true, string.Empty);
        }

        /// <summary>
        /// 生成创建Table的sql
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="baseManageModelList">模型数据集合</param>
        /// <returns>string</returns>
        private string GetSqlStr(string tableName, List<SiteModelColumn> baseManageModelList)
        {
            string sqlCreateTable = string.Empty;
            sqlCreateTable += " CREATE TABLE  " + tableName + " (Id int(11) NOT NULL AUTO_INCREMENT PRIMARY KEY, ";
            baseManageModelList.ForEach(o =>
            {
                // 字段名
                string fileName = string.Format(@"`{0}`", o.FildName) + "  ";

                // 字段类型
                string fileType = this.GetFileType((int)o.FildType);
                var fileLengthStr = " ";

                // 字段长度
                var fileLength = Convert.ToInt32(o.FildLength);
                if (o.FildType == EnumList.FileType.数字 || o.FildType == EnumList.FileType.时间)
                {
                    fileLength = 11;
                }
                else if (o.FildType == EnumList.FileType.货币)
                {
                    fileLength = 18;
                }

                if (o.PageShowType == EnumList.PageShowType.富文本框)
                {
                    fileType = "longtext";
                    fileLength = 0;
                }
                else if (o.PageShowType == EnumList.PageShowType.手机号)
                {
                    fileType = this.GetFileType((int)EnumList.FileType.文本);
                    fileLength = 255;
                }
                if (fileLength != 0)
                {
                    fileLengthStr = "(" + fileLength.ToString() + ")";
                }

                // 是否为空
                string fileIsNull = o.FildIsNull == 0 ? " DEFAULT NULL " : " NOT NULL ";
                sqlCreateTable += fileName + fileType + fileLengthStr + fileIsNull + ",";
            });
            sqlCreateTable += " cid int DEFAULT NULL ,";
            sqlCreateTable = sqlCreateTable.Substring(0, sqlCreateTable.Length - 1);
            sqlCreateTable += " ) ";
            return sqlCreateTable;
        }

        private string GetFileType(int fileType)
        {
            var fileType1 = string.Empty;
            switch (fileType)
            {
                case 1:
                    fileType1 = "varchar";
                    break;
                case 2:
                    fileType1 = "int";
                    break;
                case 3:
                    fileType1 = "int";
                    break;
                case 4:
                    fileType1 = "decimal";
                    break;
            }

            return fileType1;
        }

        /// <summary>
        /// Ajax返回格式
        /// </summary>
        /// <param name="res">返回值</param>
        /// <param name="content">返回数据</param>
        /// <returns>ActionResult</returns>
        private ActionResult AjaxResult(bool res, string content)
        {
            return this.Json(new { state = res, content = content });
        }

        /// <summary>
        /// Ajax返回格式
        /// </summary>
        /// <param name="res">返回值</param>
        /// <param name="content">返回数据</param>
        /// <returns>ActionResult</returns>
        private ActionResult AjaxResult(bool res, object content)
        {
            return this.Json(new { state = res, content = content });
        }

        private List<string> GetKeys(string keywords)
        {
            /* Task_120，查带有空格关键词时会自动删除关键词中的空格 ， 2017-02-22，章圣钻  start*/
            var keys = keywords.Replace("\n", ",").Replace("，", ",").Replace("、", ",").Replace("。", ",").Split(',');
            List<string> res = new List<string>();
            foreach (var item in keys)
            {
                var key = item.Trim().ToLower();
                if (!string.IsNullOrEmpty(key) && !res.Contains(key))
                {
                    res.Add(key);
                }
            }

            return res;
            /*end*/
        }
    }
}