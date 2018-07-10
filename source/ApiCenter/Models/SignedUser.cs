namespace WarrantyApiCenter.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;

    /// <summary>
    /// 已经登录的用户
    /// </summary>
    public class SignedUser
    {
        public SignedUser(string userToken, string userId, ClaimsPrincipal userClaims)
        {
            this.UserToken = userToken;
            this.UserId = userId;
            this.UserClaims = userClaims;
        }

        // jwt签名凭证
        public string UserToken { get; set; }

        // 用户唯一id名
        public string UserId { get; set; }

        // 用户声明属性
        public ClaimsPrincipal UserClaims { get; set; }
    }
}
