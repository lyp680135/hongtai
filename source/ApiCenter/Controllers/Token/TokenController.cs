namespace WarrantyApiCenter.Controllers.Token
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Security.Claims;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.IdentityModel.Tokens;
    using Newtonsoft.Json;
    using Util;
    using WarrantyApiCenter.Models;
    using static DataLibrary.EnumList;

    /// <summary>
    /// 获取Token(登录身份)
    /// </summary>
    [Produces("application/json")]
    [Route("api/Token")]
    public class TokenController : Controller
    {
        private Common.IService.ILoginService loginService;

        public TokenController(Common.IService.ILoginService loginService)
        {
            this.loginService = loginService;
        }

        [HttpGet]
        public ResponseModel Get(string userName, string pwd)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(pwd))
            {
                return new Models.ResponseModel(
                      ApiResponseStatus.Failed, "参数错误", string.Empty);
            }

            string refMsg = string.Empty;
            DataLibrary.MngAdmin mngAdmin = null;

            var result = this.loginService.LoginByManage(userName, Util.Helpers.Encrypt.Md5By32(pwd), this.Request.HttpContext.Connection.RemoteIpAddress.ToString(), ref refMsg, ref mngAdmin);

            if (result == false)
            {
                return new Models.ResponseModel(
                        ApiResponseStatus.Failed, refMsg, string.Empty);
            }
            else
            {
                var encodedJwt = TokenModel.Create(mngAdmin.Id.ToString(), mngAdmin.RealName, mngAdmin.GroupManage.SafeString(), SystemMemberType.Manage);

                var responseModel = new ResponseModel(ApiResponseStatus.Success, refMsg, JsonConvert.SerializeObject(new
                    {
                        access_token = encodedJwt,
                    }));

                return responseModel;
            }
        }

        // POST: api/Token
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Token/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Token/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}