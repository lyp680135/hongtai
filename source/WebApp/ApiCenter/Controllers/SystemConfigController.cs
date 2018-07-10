namespace WarrantyApiCenter.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using WarrantyApiCenter.Models;
    using static DataLibrary.EnumList;

    [Produces("application/json")]
    [Route("api/SystemConfig")]
    public class SystemConfigController : Controller
    {
        private DataLibrary.DataContext db;

        public SystemConfigController(DataLibrary.DataContext dataContext)
        {
            this.db = dataContext;
        }

        // GET: api/SystemConfig
        [HttpGet]
        public ResponseModel Get()
        {
            var setting = this.db.MngSetting.OrderBy(c => c.Id).FirstOrDefault();

            if (setting != null)
            {
                var dataModel = new
                {
                    Domain = setting.Domain,
                    Domain_PC = setting.Domain_PC,
                    Domain_WAP = setting.Domain_WAP,
                    Domain_WAPManage = setting.Domain_WAPManage,
                    Domain_WebApi = setting.Domain_WebApi,
                    Domain_SJBFile = setting.ImgPath,
                    Name = setting.Name,
                    NameEn = setting.NameEn,
                    Domain_QRCode = setting.Domain_QRCode
                };

                return new ResponseModel(ApiResponseStatus.Success, string.Empty, JsonConvert.SerializeObject(dataModel));
            }
            else
            {
                return new ResponseModel(ApiResponseStatus.Failed, "系统设置未初始化", string.Empty);
            }
        }

        // POST: api/SystemConfig
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/SystemConfig/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
