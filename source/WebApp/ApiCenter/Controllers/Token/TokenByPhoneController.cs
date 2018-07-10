namespace WarrantyApiCenter.Controllers.Token
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using WarrantyApiCenter.Models;
    using static DataLibrary.EnumList;

    /// <summary>
    /// 钢厂人员使用短信验证码登录
    /// </summary>
    [Produces("application/json")]
    [Route("api/TokenByPhone")]
    public class TokenByPhoneController : Controller
    {
        private Common.IService.ILoginService loginService;

        public TokenByPhoneController(Common.IService.ILoginService loginService)
        {
            this.loginService = loginService;
        }

        [HttpGet]
        public ResponseModel Get(string phone, string code)
        {
            if (string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(code))
            {
                return new ResponseModel(ApiResponseStatus.Failed, "参数错误", string.Empty);
            }

            string refMsg = string.Empty;
            DataLibrary.MngAdmin mngAdmin = null;

            var result = this.loginService.LoginByManagePhone(phone, code, this.Request.HttpContext.Connection.RemoteIpAddress.ToString(), ref refMsg, ref mngAdmin);

            if (result == false)
            {
                return new ResponseModel(ApiResponseStatus.Failed, refMsg, string.Empty);
            }
            else
            {
                var encodedJwt = TokenModel.Create(mngAdmin.Id.ToString(), mngAdmin.RealName, mngAdmin.GroupManage.Json, SystemMemberType.Manage);

                var responseModel = new ResponseModel(ApiResponseStatus.Success, refMsg, JsonConvert.SerializeObject(new
                    {
                        access_token = encodedJwt,
                    }));

                return responseModel;
            }
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
