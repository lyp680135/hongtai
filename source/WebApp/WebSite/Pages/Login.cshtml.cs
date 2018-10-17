namespace WarrantyManage.Pages
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using System.Web;
    using Common.IService;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Models;
    using Util;
    using static DataLibrary.EnumList;

    public class LoginModel : BaseModel<DataLibrary.DataContext>
    {
        private Common.IService.ILoginService loginService;
        private Common.IService.ILogService logService;
        private Common.IService.IUserService userService;

        public LoginModel(DataLibrary.DataContext db, ILoginService loginService, Common.IService.ILogService logService, ISettingService settingService, IUserService userService)
             : base(db)
        {
            this.loginService = loginService;
            this.logService = logService;
            this.SettingService = settingService;
            this.userService = userService;
        }

        public Common.IService.ISettingService SettingService { get; set; }

        public string Msg { get; set; }

        [BindProperty]
        public string UserName { get; set; }

        [BindProperty]
        public string Password { get; set; }

        [BindProperty]
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets 0用户名登录 1手机号登录 2设置密码
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public int IsFirstLogin { get; set; }

        public void OnGet()
        {
            if (this.Request.Cookies["UserName"] != null)
            {
                this.UserName = HttpUtility.UrlDecode(this.Request.Cookies["UserName"]);
            }

            this.Msg = string.Empty;
        }

        public async void OnPostAsync()
        {
            // 用户名与手机号登录都在这
            if (string.IsNullOrEmpty(this.UserName))
            {
                this.Msg = "请输入登录用户名！";
                return;
            }

            var flag = false; // 是否登录成功标识
            string refMsg = string.Empty;
            DataLibrary.MngAdmin result = null;

            var loginIp = this.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            if (this.IsFirstLogin == 1)
            {
                if (string.IsNullOrEmpty(this.Code))
                {
                    this.Msg = "请输入验证码！";
                    return;
                }

                flag = this.loginService.LoginByManagePhone(this.UserName, this.Code, loginIp, ref refMsg, ref result);
            }
            else
            {
                if (string.IsNullOrEmpty(this.Password))
                {
                    this.Msg = "请输入密码！";
                    return;
                }

                string signPwd = Util.Helpers.Encrypt.Md5By32(this.Password);
                flag = this.loginService.LoginByManage(this.UserName, signPwd, loginIp, ref refMsg, ref result);
            }

            if (flag)
            {
                await this.SetLoginIdentity(result);

                this.Response.Cookies.Append("UserName", HttpUtility.UrlEncode(this.UserName));
                if (this.IsFirstLogin == 1)
                {
                    this.Response.Redirect("/Login?IsFirstLogin=2");
                }
                else
                {
                    this.Response.Redirect("/Manage/Index");
                }
            }
            else
            {
                this.Msg = refMsg;
            }
        }

        private async Task<bool> SetLoginIdentity(DataLibrary.MngAdmin result)
        {
            var claimsIdentity = new ClaimsIdentity(
                    new Claim[]
                {
                            new Claim(ClaimTypes.NameIdentifier, result.Id.ToString()),
                            new Claim(ClaimTypes.Name, result.RealName),
                            new Claim(ClaimTypes.Role, result.GroupManage.SafeString()),
                }, SystemMemberType.Manage.ToString());

            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await this.HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                claimsPrincipal);
            return true;
        }
    }
}