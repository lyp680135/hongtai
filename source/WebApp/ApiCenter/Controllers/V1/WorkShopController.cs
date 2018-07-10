// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WarrantyApiCenter.Controllers.V1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Common.IService;
    using Common.Service;
    using DataLibrary;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Util;
    using WarrantyApiCenter.Models;
    using WarrantyApiCenter.Models.DataModel.WorkShop;
    using static DataLibrary.EnumList;

    [Produces("application/json")]
    public class WorkShopController : BaseController
    {
        private DataLibrary.DataContext db;
        private IQrCodeAuthService qrCodeAuthService;
        private IUserService userService;

        public WorkShopController(DataContext dataContext, IQrCodeAuthService qrCodeAuthService, IUserService userService)
        {
            this.db = dataContext;
            this.qrCodeAuthService = qrCodeAuthService;
            this.userService = userService;
        }

        /// <summary>
        /// 获取所有车间
        /// </summary>
        /// <returns>ResponseModel</returns>
        [HttpGet]
        public ResponseModel Get()
        {
            var list = this.db.PdWorkshop.ToList();
            return new ResponseModel(ApiResponseStatus.Success, string.Empty, JsonConvert.SerializeObject(list));
        }

        /// <summary>
        /// 获取某个车间的二维码授权数据
        /// </summary>
        /// <param name="id">id</param>
        /// <returns>ResponseModel</returns>
        [HttpGet("{id}")]
        public ResponseModel Get(int id)
        {
            WorkShopData workShopData = this.GetWorkShopData(id);
            if (workShopData != null)
            {
                return new ResponseModel(ApiResponseStatus.Success, string.Empty, JsonConvert.SerializeObject(workShopData));
            }
            else
            {
                return new ResponseModel(ApiResponseStatus.Failed, "该车间不存在", string.Empty);
            }
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public ResponseModel Put(int id, [FromForm]string value)
        {
            var request = JsonConvert.DeserializeObject<WorkShopData>(value.Base64ToString());

            if (request != null)
            {
                foreach (var data in request.Data)
                {
                    data.Spec.ForEach(action =>
                    {
                        if (action.EditNumber != 0)
                        {
                            this.qrCodeAuthService.SetAvailableNumber(request.Id, action.SpecId, action.EditNumber, this.userService.ApplicationUser.Mng_admin.Id);
                        }
                    });
                }

                return new ResponseModel(ApiResponseStatus.Success, string.Empty, JsonConvert.SerializeObject(this.GetWorkShopData(request.Id)));
            }
            else
            {
                return new ResponseModel(ApiResponseStatus.Failed, "传入的数值不正确", string.Empty);
            }
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        /// <summary>
        /// 根据某个车间获取授权页面需要返回的数据
        /// </summary>
        /// <param name="workShopId">workShopId</param>
        /// <returns>WorkShopData</returns>
        private WorkShopData GetWorkShopData(int workShopId)
        {
            var query_work = this.db.PdWorkshop.FirstOrDefault(c => c.Id == workShopId);
            if (query_work != null)
            {
                WorkShopData workShopData = new WorkShopData();
                workShopData.Id = query_work.Id;
                workShopData.Name = query_work.Name;

                // 所有品名
                var list_pm = this.db.BaseProductClass.ToList();

                // 所有材质
                var list_cz = this.db.BaseProductMaterial.ToList();

                // 所有规格
                var list_spec = this.db.BaseSpecifications.ToList();

                // 获取品名与材质的分组聚合
                var query_groupPmCz = (from c in list_spec
                                       group c by new { c.Classid, c.Materialid } into g
                                       select g).ToList();
                var list_data = new List<DataList>();
                foreach (var q in query_groupPmCz)
                {
                    DataList dataList = new DataList();
                    dataList.ClassId = q.Key.Classid.Value;
                    dataList.ClassName = list_pm.First(c => c.Id == q.Key.Classid.Value).Name;
                    dataList.MaterialId = q.Key.Materialid.Value;
                    dataList.MaterialName = list_cz.First(c => c.Id == q.Key.Materialid.Value).Name;

                    var query_spec = list_spec.Where(c => c.Classid == q.Key.Classid.Value && c.Materialid == q.Key.Materialid.Value);
                    var list_dataspec = new List<DataList_Spec>();
                    foreach (var qs in query_spec)
                    {
                        list_dataspec.Add(new DataList_Spec()
                        {
                            SpecId = qs.Id,
                            SpecName = list_spec.First(c => c.Id == qs.Id).Specname,
                            Number_Auth = this.qrCodeAuthService.GetAuthNumber(query_work.Id, qs.Id),   // 当前车间的授权数
                            Number_Available = this.qrCodeAuthService.GetAvailableNumber(query_work.Id, qs.Id), // 当前车间的剩余可用数
                        });
                    }

                    dataList.Spec = list_dataspec;
                    list_data.Add(dataList);
                }

                workShopData.Data = list_data;

                return workShopData;
            }
            else
            {
                return null;
            }
        }
    }
}