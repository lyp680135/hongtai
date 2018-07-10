namespace WarrantyApiCenter.Controllers.Token
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.IdentityModel.Tokens;
    using Newtonsoft.Json;
    using WarrantyApiCenter.Models;
    using static DataLibrary.EnumList;

    /// <summary>
    /// 经销商登录，获取Token
    /// </summary>
    [Produces("application/json")]
    [Route("api/TokenBySaler")]
    public class TokenBySalerController : Controller
    {
        private Common.IService.ILoginService loginService;

        public TokenBySalerController(Common.IService.ILoginService loginService)
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
            DataLibrary.SaleSeller saleSeller = null;

            var result = this.loginService.LoginBySaleSeller(phone, code, ref refMsg, ref saleSeller);

            if (result == false)
            {
                return new ResponseModel(ApiResponseStatus.Failed, refMsg, string.Empty);
            }
            else
            {
                var encodedJwt = TokenModel.Create(saleSeller.Id.ToString(), saleSeller.Name, saleSeller.Mobile, SystemMemberType.Seller);

                var responseModel = new ResponseModel(ApiResponseStatus.Success, refMsg, JsonConvert.SerializeObject(new
                    {
                        access_token = encodedJwt,
                    }));

                return responseModel;
            }
        }
    }
}
