namespace Common.Service
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Common.IService;
    using DataLibrary;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Util;
    using static DataLibrary.EnumList;

    public class LoginService : ILoginService
    {
        private DataLibrary.DataContext db;

        public LoginService(DataLibrary.DataContext dataContext)
        {
            this.db = dataContext;
        }

        /// <summary>
        /// 钢厂人员使用帐号密码登录
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="signPwd">密码</param>
        /// <param name="loginIp">登录IP</param>
        /// <param name="msg">处理消息</param>
        /// <param name="mngAdmin">当前登录的Admin实体</param>
        /// <returns>登录是否成功状态</returns>
        public bool LoginByManage(string userName, string signPwd, string loginIp, ref string msg, ref DataLibrary.MngAdmin mngAdmin)
        {
            var result = this.db.MngAdmin.FirstOrDefault(c => (c.UserName == userName && c.Password == signPwd) || (c.RealName == userName && c.Password == signPwd));
            if (result != null)
            {
                if (result.InJob.HasValue && Convert.ToBoolean(result.InJob))
                {
                    result.LoginIp = loginIp;
                    result.LoginTime = DateTime.Now;
                    result.LoginTimes = result.LoginTimes.ToInt() + 1;
                    this.db.SaveChanges();

                    mngAdmin = result;

                    return true;
                }
                else
                {
                    msg = "该帐户己被锁定，请联系管理员！";
                    return false;
                }
            }
            else
            {
                msg = "用户名或密码错误！";
                return false;
            }
        }

        /// <summary>
        /// 钢厂人员使用手机验证码登录
        /// </summary>
        /// <param name="mobile">手机号</param>
        /// <param name="code">验证码</param>
        /// <param name="loginIp">登录IP</param>
        /// <param name="msg">处理消息</param>
        /// <param name="mngAdmin">当前登录的Admin实体</param>
        /// <returns>登录是否成功状态</returns>
        public bool LoginByManagePhone(string mobile, string code, string loginIp, ref string msg, ref MngAdmin mngAdmin)
        {
            var mngAdmin1 = this.db.MngAdmin.FirstOrDefault(c => c.UserName == mobile);
            if (mngAdmin1 != null)
            {
                var model_code = this.db.BaseMobileCode.OrderByDescending(c => c.Sendtime)
                  .FirstOrDefault(c => c.Mobile == mobile && c.Code == code
                    && c.CodeType == MobileCodeType.登录验证码 && c.MemberType == SystemMemberType.Manage
                    && c.IsValaid == false);

                if (model_code != null)
                {
                    TimeSpan timeSpan = DateTime.Now - model_code.Sendtime;
                    if (timeSpan.Minutes > 10)
                    {
                        msg = "验证码已超时，请重新获取";
                        return false;
                    }
                    else
                    {
                        mngAdmin1.LoginIp = loginIp;
                        mngAdmin1.LoginTime = DateTime.Now;
                        mngAdmin1.LoginTimes = mngAdmin1.LoginTimes.ToInt() + 1;

                        model_code.IsValaid = true;
                        this.db.SaveChanges();

                        mngAdmin = mngAdmin1;
                        return true;
                    }
                }
                else
                {
                    msg = "验证码错误";
                    return false;
                }
            }
            else
            {
                msg = $"您的手机号未获得供货商《产品质量证明书》授权，请联系您的供货商获得授权或由供货商提供《产品质量证明书》";
                return false;
            }
        }

        /// <summary>
        /// 经销商人员登录
        /// </summary>
        /// <param name="mobile">手机号</param>
        /// <param name="code">验证码</param>
        /// <param name="msg">处理消息</param>
        /// <param name="saleSeller">当前登录的经销商人员实体</param>
        /// <returns>登录是否成功状态</returns>
        public bool LoginBySaleSeller(string mobile, string code, ref string msg, ref SaleSeller saleSeller)
        {
            var saleSeller1 = this.db.SaleSeller.FirstOrDefault(c => c.Mobile.Contains(mobile));
            if (saleSeller1 != null)
            {
                var model_code = this.db.BaseMobileCode.OrderByDescending(c => c.Sendtime).FirstOrDefault(c => c.Mobile == mobile && c.Code == code
                  && c.CodeType == MobileCodeType.登录验证码 && c.MemberType == SystemMemberType.Seller
                  && c.IsValaid == false);

                /*   测试时不需要输手机验证码
                                if (true)
                                {
                                    saleSeller = saleSeller1;
                                    return true;
                                }
                */

                if (model_code != null)
                {
                    TimeSpan timeSpan = DateTime.Now - model_code.Sendtime;
                    if (timeSpan.Minutes > 10)
                    {
                        msg = "验证码已超时，请重新获取";
                        return false;
                    }
                    else
                    {
                        saleSeller = saleSeller1;
                        model_code.IsValaid = true;
                        this.db.SaveChanges();
                        return true;
                    }
                }
                else
                {
                    msg = "验证码错误";
                    return false;
                }
            }
            else
            {
                msg = mobile + "不存在于系统中";
                return false;
            }
        }

        /// <summary>
        /// 发送验证码(调用商交宝接口)
        /// </summary>
        /// <param name="mobile">手机号</param>
        /// <param name="systemMemberType">会员类别枚举</param>
        /// <param name="msg">处理消息</param>
        /// <param name="isProduction">是否环境</param>
        /// <returns>发送是否成功状态</returns>
        public bool SendMobileCode_SJB(string mobile, SystemMemberType systemMemberType, ref string msg, bool isProduction)
        {
            var setting = this.db.MngSetting.FirstOrDefault();
            if (setting != null)
            {
                if (systemMemberType == SystemMemberType.Manage)
                {
                    var mng_user = this.db.MngAdmin.FirstOrDefault(c => c.UserName == mobile);
                    if (mng_user != null)
                    {
                        // 执行发送操作
                        return this.SendMobileCode_SJB_Excute(setting, mobile, systemMemberType, ref msg, isProduction);
                    }
                    else
                    {
                        msg = $"您的手机号未获得供货商《产品质量证明书》授权，请联系您的供货商获得授权或由供货商提供《产品质量证明书》";
                        return false;
                    }
                }
                else if (systemMemberType == SystemMemberType.Seller)
                {
                    var saleSeller = this.db.SaleSeller.FirstOrDefault(c => c.Mobile.Contains(mobile));
                    if (saleSeller != null)
                    {
                        // 执行发送操作
                        return this.SendMobileCode_SJB_Excute(setting, mobile, systemMemberType, ref msg, isProduction);
                    }
                    else
                    {
                        msg = $"您的手机号未获得供货商《产品质量证明书》授权，请联系您的供货商获得授权或由供货商提供《产品质量证明书》";
                        return false;
                    }
                }

                return true;
            }
            else
            {
                msg = "系统设置表未初始化";
                return false;
            }
        }

        /// <summary>
        /// 执行发送短信验证码操作（调用商交宝接口）
        /// </summary>
        /// <param name="setting">系统基础配置类</param>
        /// <param name="mobiles">手机号</param>
        /// <param name="systemMemberType">会员类别枚举</param>
        /// <param name="msg">处理消息</param>
        /// <param name="isProduction">是否生产环境</param>
        /// <returns>发送是否成功状态</returns>
        public bool SendMobileCode_SJB_Excute(MngSetting setting, string mobiles, SystemMemberType systemMemberType, ref string msg, bool isProduction)
        {
            var code = new Util.Helpers.Random().Next(1001, 9999); // 登录验证码生成

            string responData = string.Empty;

            // 生产环境才发送验证码
            if (isProduction)
            {
                var reData = new
                {
                    accountno = 100001,
                    con = $"欢迎使用{setting.Name},您的登录验证码：{code}",
                    mobiles = mobiles,
                    smstype = setting.SJBSMSTypeId
                };

                string actionName = "sendsms";

                // 生成Token
                long GetDateTimeMinuteTimesan(DateTime dateTime)
                {
                    var start = new DateTime(1970, 1, 1, 0, 0, 0);
                    TimeSpan ts = dateTime.ToUniversalTime() - start;
                    return Convert.ToInt32(ts.TotalMinutes);
                }

                string token = Util.Helpers.Encrypt.Md5By32(setting.SJBSMSCode + actionName + GetDateTimeMinuteTimesan(DateTime.Now));
                Dictionary<string, string> dicData = new Dictionary<string, string>
            {
                { "Source", "WarrantyManage" },
                { "Token", token },
                { "RequestData", JsonConvert.SerializeObject(reData) }
            };
                responData = Util.Helpers.HttpHelper.HttpPost(
                    setting.APIRoot + "api/iportals/" + actionName,
                    dicData,
                    encoding: Encoding.UTF8,
                timeOut: 10000);
            }
            else
            {
                // 非生产环境验证码都为1234
                responData = "{\"Status\": 200}";
                code = 1234;
            }

            JObject jo = JObject.Parse(responData);

            if (Util.Extensions.ToInt(jo["Status"]) == 200)
            {
                var addModel = new BaseMobileCode()
                {
                    Code = code.ToString(),
                    CodeType = MobileCodeType.登录验证码,
                    MemberType = systemMemberType,
                    Mobile = mobiles,
                    Sendtime = DateTime.Now,
                    IsValaid = false
                };

                this.db.BaseMobileCode.Add(addModel);

                if (this.db.SaveChanges() > 0)
                {
                    return true;
                }
                else
                {
                    msg = "短信验证码保存失败";
                    return false;
                }
            }
            else
            {
                msg = "商交宝短信接口发送失败";
                return false;
            }
        }
    }
}
