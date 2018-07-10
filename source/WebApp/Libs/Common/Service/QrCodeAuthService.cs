namespace Common.Service
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Common.IService;
    using Util;

    /// <summary>
    /// 车间二维码数量授权服务 - 刘钟
    /// </summary>
    public class QrCodeAuthService : IQrCodeAuthService
    {
        private DataLibrary.DataContext db;

        public QrCodeAuthService(DataLibrary.DataContext dataContext)
        {
            this.db = dataContext;
        }

        /// <summary>
        /// 批量添加数据
        /// </summary>
        /// <param name="list_add">要添加的数据集合</param>
        /// <returns>影响行数</returns>
        public int AddRange(List<DataLibrary.PdQRCodeAuth> list_add)
        {
            this.db.PdQRCodeAuth.AddRange(list_add);
            return this.db.SaveChanges();
        }

        /// <summary>
        /// 获取上个月的所有授权设置数据
        /// </summary>
        /// <returns>返回元组数据，第一个为上月的授权数据集合，第二个为PdQRCodeAuth表最大的ID</returns>
        public Tuple<List<DataLibrary.PdQRCodeAuth>, int> GetDataForPrevMonth()
        {
            DateTime now = DateTime.Now.ToShortDateString().ToDate();

            DateTime lasteMonth_FirstDay = now.AddDays(1 - now.Day).AddMonths(-1); // 上月的第一天
            DateTime thisMonth_FirstDay = new DateTime(now.Year, now.Month, 1); // 这个月第一天

            var item1 = this.db.PdQRCodeAuth.Where(c =>
                 c.AuthDate >= (int)lasteMonth_FirstDay.GetUnixTimeFromDateTime()
                 && c.AuthDate < (int)thisMonth_FirstDay.GetUnixTimeFromDateTime()).ToList();

            var item2 = this.db.PdQRCodeAuth.Max(c => c.Id);

            return new Tuple<List<DataLibrary.PdQRCodeAuth>, int>(item1, item2);
        }

        /// <summary>
        /// 查询当月是否设置授权数据
        /// </summary>
        /// <returns>有为true,无为false</returns>
        public bool IsExistsDataForThisMonty()
        {
            DateTime now = DateTime.Now;
            DateTime dt_Start = new DateTime(now.Year, now.Month, 1);
            DateTime dt_End = dt_Start.AddMonths(1);

            var model_QrCodeAuth = this.db.PdQRCodeAuth.Count(c => c.AuthDate >= (int)dt_Start.GetUnixTimeFromDateTime() && c.AuthDate < (int)dt_End.GetUnixTimeFromDateTime());

            if (model_QrCodeAuth > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 根据车间与规格ID获取当月授权数
        /// </summary>
        /// <param name="workShopId">车间ID</param>
        /// <param name="specId">规格ID</param>
        /// <returns>当前授权数</returns>
        public int GetAuthNumber(int workShopId, int specId)
        {
            DateTime now = DateTime.Now;
            DateTime dt_Start = new DateTime(now.Year, now.Month, 1);
            DateTime dt_End = dt_Start.AddMonths(1);

            var model_QrCodeAuth = this.db.PdQRCodeAuth.FirstOrDefault(c => c.WorkshopId == workShopId && c.Specid == specId
           && c.AuthDate >= (int)dt_Start.GetUnixTimeFromDateTime() && c.AuthDate < (int)dt_End.GetUnixTimeFromDateTime());

            if (model_QrCodeAuth != null && model_QrCodeAuth.Number > 0)
            {
                return model_QrCodeAuth.Number;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// 根据规格ID获取当月剩余可用数
        /// </summary>
        /// <param name="workShopId">车间ID</param>
        /// <param name="specId">规格ID</param>
        /// <returns>当月剩余可用数</returns>
        public int GetAvailableNumber(int workShopId, int specId)
        {
            DateTime now = DateTime.Now;
            DateTime dt_Start = new DateTime(now.Year, now.Month, 1);
            DateTime dt_End = dt_Start.AddMonths(1);

            var model_QrCodeAuth = this.db.PdQRCodeAuth.FirstOrDefault(c => c.WorkshopId == workShopId && c.Specid == specId
            && c.AuthDate >= (int)dt_Start.GetUnixTimeFromDateTime() && c.AuthDate < (int)dt_End.GetUnixTimeFromDateTime());

            if (model_QrCodeAuth != null)
            {
                int availableNumber = model_QrCodeAuth.Number - this.GetUseNumber(workShopId, specId);
                return availableNumber >= 0 ? availableNumber : 0;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// 根据规格ID获取当前车间本月已使用数
        /// </summary>
        /// <param name="workShopId">车间ID</param>
        /// <param name="specId">规格ID</param>
        /// <returns>本月已使用数</returns>
        public int GetUseNumber(int workShopId, int specId)
        {
            DateTime now = DateTime.Now;
            DateTime dt_Start = new DateTime(now.Year, now.Month, 1);
            DateTime dt_End = dt_Start.AddMonths(1);

            int useNum = this.db.PdQRCodePrintedLog.Count(c => c.WorkshopId == workShopId && c.SpecId == specId
              && c.Createtime >= (int)dt_Start.GetUnixTimeFromDateTime()
              && c.Createtime < (int)dt_End.GetUnixTimeFromDateTime());

            return useNum;
        }

        /// <summary>
        /// 新增减少本月车间二维码授权使用数(如果本月不存在授权纪录，则自动新增)
        /// </summary>
        /// <param name="workShopId">车间ID</param>
        /// <param name="specId">规格ID</param>
        /// <param name="editNumber">需要更改的授权数值</param>
        /// <param name="adder">操作人ID</param>
        /// <returns>更改后的PdQRCodeAuth实体，操作失败返回NULL</returns>
        public DataLibrary.PdQRCodeAuth SetAvailableNumber(int workShopId, int specId, int editNumber, int adder)
        {
            DateTime now = DateTime.Now;
            DateTime dt_Start = new DateTime(now.Year, now.Month, 1);
            DateTime dt_End = dt_Start.AddMonths(1);

            int dtNow = (int)now.GetUnixTimeFromDateTime();

            var query_PdQrCodeAuth = this.db.PdQRCodeAuth.FirstOrDefault(c => c.WorkshopId == workShopId && c.Specid == specId
            && c.AuthDate >= (int)dt_Start.GetUnixTimeFromDateTime() && c.AuthDate < (int)dt_End.GetUnixTimeFromDateTime());
            if (query_PdQrCodeAuth == null)
            {
                var model_Spec = this.db.BaseSpecifications.FirstOrDefault(c => c.Id == specId);

                if (model_Spec != null && model_Spec.Classid.HasValue && model_Spec.Materialid.HasValue)
                {
                    var modelAdd = new DataLibrary.PdQRCodeAuth()
                    {
                        AuthDate = (int)dt_Start.GetUnixTimeFromDateTime(), // 授权时间都为每月1号
                        Adder = adder,
                        Classid = model_Spec.Classid.Value,
                        Createtime = dtNow,
                        Materialid = model_Spec.Materialid.Value,
                        Number = editNumber,
                        Specid = specId,
                        WorkshopId = workShopId,
                    };
                    this.db.PdQRCodeAuth.Add(modelAdd);
                    this.db.SaveChanges();

                    return modelAdd;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                query_PdQrCodeAuth.Adder = adder;
                query_PdQrCodeAuth.AuthDate = (int)dt_Start.GetUnixTimeFromDateTime(); // 授权时间都为每月1号
                query_PdQrCodeAuth.Number += editNumber;
                query_PdQrCodeAuth.Createtime = dtNow;

                this.db.SaveChanges();
                return query_PdQrCodeAuth;
            }
        }
    }
}
