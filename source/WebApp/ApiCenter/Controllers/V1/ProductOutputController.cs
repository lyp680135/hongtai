// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WarrantyApiCenter.Controllers.V1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Common.IService;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using WarrantyApiCenter.Models;
    using static DataLibrary.EnumList;

    [Produces("application/json")]
    public class ProductOutputController : BaseController
    {
        private DataLibrary.DataContext db;
        private IUserService userService;

        public ProductOutputController(DataLibrary.DataContext dataContext, IUserService user)
        {
            this.db = dataContext;
            this.userService = user;
        }

        /// <summary>
        /// 查询该炉批号下的出货状态
        /// </summary>
        /// <param name="batCode">炉批哈</param>
        /// <param name="deliveryType">类型</param>
        /// <returns>ResponseModel</returns>
        public ResponseModel Get(string batCode, int deliveryType)
        {
            if (string.IsNullOrEmpty(batCode))
            {
                return new ResponseModel(ApiResponseStatus.Failed, "请输入炉批号", string.Empty);
            }

            var productList = new List<DataLibrary.PdProduct>();
            if (deliveryType == (int)DataLibrary.EnumList.ProductQualityLevel.定尺)
            {
                productList = this.db.PdProduct.OrderBy(o => o.Bundlecode).Where(p => p.Batcode == batCode && p.Lengthtype == (int)deliveryType).ToList();
            }
            else
            {
                productList = this.db.PdProduct.OrderBy(o => o.Bundlecode).Where(p => p.Batcode == batCode && p.Lengthtype != (int)DataLibrary.EnumList.ProductQualityLevel.定尺).ToList();
            }

            if (productList == null || productList.Count <= 0)
            {
                return new ResponseModel(ApiResponseStatus.Failed, "不存在该炉批号的产品", string.Empty);
            }

            var jArray = new JArray();
            List<int> outList = new List<int>();
            List<int> list = new List<int>();

            // 按照是否出货分组
            productList.ForEach(o =>
            {
                var pdSocktOut = this.db.PdStockOut.FirstOrDefault(f => f.Productid == o.Id);
                if (pdSocktOut != null)
                {
                    outList.Add(o.Id);
                }
                else
                {
                    list.Add(o.Id);
                }
            });
            list.ForEach(o =>
            {
                var jObject = new JObject();
                var proInfo = this.db.PdProduct.FirstOrDefault(f => f.Id == o);
                if (proInfo != null)
                {
                    jObject.Add("bundlecode", proInfo.Bundlecode);
                    jObject.Add("ifoutput", "未出货");
                }
                jArray.Add(jObject);
            });
            outList.ForEach(o =>
            {
                var jObject = new JObject();
                var proInfo = this.db.PdProduct.FirstOrDefault(f => f.Id == o);
                if (proInfo != null)
                {
                    jObject.Add("bundlecode", proInfo.Bundlecode);
                    jObject.Add("ifoutput", "已出货");
                }
                jArray.Add(jObject);
            });

            return new ResponseModel(ApiResponseStatus.Success, string.Empty, JsonConvert.SerializeObject(jArray));
        }
    }
}
