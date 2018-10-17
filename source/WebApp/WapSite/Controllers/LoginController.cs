namespace WarrantyEnterpriseMobileStation.Controllers
{
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Common.IService;
    using Common.Service;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Models;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Util;
    using static DataLibrary.EnumList;

    public class LoginController : Controller
    {
        private readonly IHostingEnvironment hostingEnvironments;
        private ILoginService loginService;
        private ISettingService settingService;
        private DataLibrary.DataContext db;

        public LoginController(ILoginService loginService, DataLibrary.DataContext db, ISettingService settingService, IHostingEnvironment hostingEnvironment)
        {
            this.loginService = loginService;
            this.settingService = settingService;
            this.db = db;
            this.hostingEnvironments = hostingEnvironment;
        }

        /// <summary>
        /// 发送手机验证码
        /// </summary>
        /// <param name="mobile">手机号</param>
        /// <param name="systemMemberType">会员类别</param>
        /// <returns>发送结果</returns>
        [HttpPost]
        public JsonResult SendMobileCode(string mobile, SystemMemberType systemMemberType)
        {
            string refMsg = string.Empty;
            if (this.loginService.SendMobileCode_SJB(mobile, systemMemberType, ref refMsg, this.hostingEnvironments.IsProduction()))
            {
                return this.Json(new ResponseModel(
                    ApiResponseStatus.Success,
                    refMsg,
                    data: string.Empty));
            }
            else
            {
                return this.Json(new ResponseModel(
                    ApiResponseStatus.Failed,
                    refMsg,
                    data: string.Empty));
            }
        }

        /// <summary>
        /// 钢厂人员、经销商使用手机号验证登录
        /// </summary>
        /// <param name="mobile">手机号</param>
        /// <param name="code">验证码</param>
        /// <param name="systemMemberType">会员类别</param>
        /// <returns>登录结果</returns>
        [HttpPost]
        public JsonResult LoginByPhone(string mobile, string code, SystemMemberType systemMemberType)
        {
            var setting = this.settingService.MngSetting;
#if DEBUG
            var responData = Util.Helpers.HttpHelper.HttpGet(
                systemMemberType == SystemMemberType.Manage ?
                $"http://localhost:41178/api/TokenByPhone?phone={mobile}&code={code}" :
                $"http://localhost:41178/api/TokenBySaler?phone={mobile}&code={code}",
                System.Text.Encoding.UTF8);
#else

            var responData = Util.Helpers.HttpHelper.HttpGet(
                systemMemberType == SystemMemberType.Manage ?
                $"{setting.Domain_WebApi}api/TokenByPhone?phone={mobile}&code={code}" :
                $"{setting.Domain_WebApi}api/TokenBySaler?phone={mobile}&code={code}",
                System.Text.Encoding.UTF8);
#endif

            var webApi_ResponseModel = JsonConvert.DeserializeObject<WebApiResponseModel>(responData);

            if (webApi_ResponseModel != null && webApi_ResponseModel.Status == ApiResponseStatus.Success)
            {
#if DEBUG
                var tokenModel = new
                {
                    access_token = JObject.Parse(webApi_ResponseModel.Data)["access_token"],
                    redirect = "http://localhost:8080/"
                };
#else
                var tokenModel = new
                {
                    access_token = JObject.Parse(webApi_ResponseModel.Data)["access_token"],
                    redirect = setting.Domain_WAPManage
                };
#endif
                if (systemMemberType == SystemMemberType.Seller)
                {
                    // 用逗号分给得经销商手机号就不能登陆，应先分割逗号再进行登陆
                    var saleSeller = this.db.SaleSeller.FirstOrDefault(c => c.Mobile.Contains(mobile));
                    this.SetClaimsIdentity(saleSeller.Id.ToString(), saleSeller.Name, saleSeller.Mobile, SystemMemberType.Seller);
                }
                else if (systemMemberType == SystemMemberType.Manage)
                {
                    DataLibrary.MngAdmin mngAdmin = this.db.MngAdmin.FirstOrDefault(c => c.UserName == mobile);
                    this.SetClaimsIdentity(mngAdmin.Id.ToString(), mngAdmin.RealName, mngAdmin.GroupManage.SafeString(), SystemMemberType.Manage);
                }

                return this.Json(new ResponseModel(ApiResponseStatus.Success, string.Empty, JsonConvert.SerializeObject(tokenModel)));
            }
            else
            {
                return this.Json(new ResponseModel(ApiResponseStatus.Failed, webApi_ResponseModel.Msg, string.Empty));
            }
        }

        /// <summary>
        /// 使用用户名密码登录
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="pwd">密码</param>
        /// <returns>登录结果</returns>
        [HttpPost]
        public JsonResult LoginByUserName(string userName, string pwd)
        {
            string signPwd = Util.Helpers.Encrypt.Md5By32(pwd);
            var setting = this.settingService.MngSetting;
#if DEBUG
            var responData = Util.Helpers.HttpHelper.HttpGet(
                $"http://localhost:41178/api/Token?userName={userName}&pwd={pwd}",
                System.Text.Encoding.UTF8);
#else
            var responData = Util.Helpers.HttpHelper.HttpGet(
                $"{setting.Domain_WebApi}api/Token?userName={userName}&pwd={pwd}",
                System.Text.Encoding.UTF8);
#endif
            var webApi_ResponseModel = JsonConvert.DeserializeObject<WebApiResponseModel>(responData);

            if (webApi_ResponseModel != null && webApi_ResponseModel.Status == ApiResponseStatus.Success)
            {
#if DEBUG
                var tokenModel = new
                {
                    access_token = JObject.Parse(webApi_ResponseModel.Data)["access_token"],
                    redirect = "http://localhost:8080/"
                };
#else
                var tokenModel = new
                {
                    access_token = JObject.Parse(webApi_ResponseModel.Data)["access_token"],
                    redirect = setting.Domain_WAPManage
                };
#endif
                DataLibrary.MngAdmin mngAdmin = this.db.MngAdmin.FirstOrDefault(c => c.UserName == userName && c.Password == signPwd);
                this.SetClaimsIdentity(mngAdmin.Id.ToString(), mngAdmin.RealName, mngAdmin.GroupManage.SafeString(), SystemMemberType.Manage);

                return this.Json(new ResponseModel(
                    ApiResponseStatus.Success,
                    string.Empty,
                   data: JsonConvert.SerializeObject(tokenModel)));
            }
            else
            {
                return this.Json(new ResponseModel(ApiResponseStatus.Failed, webApi_ResponseModel.Msg, string.Empty));
            }
        }

        /// <summary>
        /// 获取系统配置，供移动管理端使用
        /// </summary>
        /// <param name="domain_webapi">webapi URL</param>
        /// <param name="systemConfig">系统配置</param>
        /// <returns>操作状态</returns>
        private bool GetSystemConfig(string domain_webapi, ref string systemConfig)
        {
            var responData_SystemConfig = Util.Helpers.HttpHelper.HttpGet(
            string.Format("{0}api/SystemConfig", domain_webapi),
            System.Text.Encoding.UTF8);

            if (!string.IsNullOrEmpty(responData_SystemConfig))
            {
                JObject jo_systemConfig = JObject.Parse(responData_SystemConfig);
                if (Util.Extensions.ToInt(jo_systemConfig["status"]) == 1)
                {
                    systemConfig = JObject.Parse(jo_systemConfig["data"].ToString()).ToString();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 设置登录凭据
        /// </summary>
        /// <param name="nameIdentifier">nameIdentifier</param>
        /// <param name="name">name</param>
        /// <param name="role">role</param>
        /// <param name="systemMemberType">systemMemberType</param>
        private void SetClaimsIdentity(string nameIdentifier, string name, string role, SystemMemberType systemMemberType)
        {
            var claimsIdentity = new ClaimsIdentity(
                new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, nameIdentifier),
                new Claim(ClaimTypes.Name, name),
                new Claim(ClaimTypes.Role, role),
            }, systemMemberType.ToString());

            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            Task task_sign = this.HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                  claimsPrincipal,
                  properties: new AuthenticationProperties() { });
            task_sign.Wait();
        }
    }
}