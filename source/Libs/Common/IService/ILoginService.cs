namespace Common.IService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using DataLibrary;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Util;
    using static DataLibrary.EnumList;

    /// <summary>
    /// 系统统一登录接口
    /// </summary>
    public interface ILoginService
    {
        /// <summary>
        /// 钢厂人员使用帐号密码登录
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="signPwd">密码</param>
        /// <param name="loginIp">登录IP</param>
        /// <param name="msg">处理消息</param>
        /// <param name="mngAdmin">当前登录的Admin实体</param>
        /// <returns>登录是否成功状态</returns>
        bool LoginByManage(string userName, string signPwd, string loginIp, ref string msg, ref DataLibrary.MngAdmin mngAdmin);

        /// <summary>
        /// 钢厂人员使用手机验证码登录
        /// </summary>
        /// <param name="mobile">手机号</param>
        /// <param name="code">验证码</param>
        /// <param name="loginIp">登录IP</param>
        /// <param name="msg">处理消息</param>
        /// <param name="mngAdmin">当前登录的Admin实体</param>
        /// <returns>登录是否成功状态</returns>
        bool LoginByManagePhone(string mobile, string code, string loginIp, ref string msg, ref MngAdmin mngAdmin);

        /// <summary>
        /// 经销商人员登录
        /// </summary>
        /// <param name="mobile">手机号</param>
        /// <param name="code">验证码</param>
        /// <param name="msg">处理消息</param>
        /// <param name="saleSeller">当前登录的经销商人员实体</param>
        /// <returns>登录是否成功状态</returns>
        bool LoginBySaleSeller(string mobile, string code, ref string msg, ref SaleSeller saleSeller);

        /// <summary>
        /// 发送验证码(调用商交宝接口)
        /// </summary>
        /// <param name="mobile">手机号</param>
        /// <param name="systemMemberType">会员类别枚举</param>
        /// <param name="msg">处理消息</param>
        /// <param name="isProduction">是否生产环境</param>
        /// <returns>发送是否成功状态</returns>
        bool SendMobileCode_SJB(string mobile, SystemMemberType systemMemberType, ref string msg, bool isProduction);

        /// <summary>
        /// 执行发送短信验证码操作（调用商交宝接口）
        /// </summary>
        /// <param name="setting">系统基础配置类</param>
        /// <param name="mobiles">手机号</param>
        /// <param name="systemMemberType">会员类别枚举</param>
        /// <param name="msg">处理消息</param>
        /// <param name="isProduction">是否生产环境</param>
        /// <returns>发送是否成功状态</returns>
        bool SendMobileCode_SJB_Excute(MngSetting setting, string mobiles, SystemMemberType systemMemberType, ref string msg, bool isProduction);
    }
}