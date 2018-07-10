namespace WarrantyApiCenter.Models
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Security.Claims;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.IdentityModel.Tokens;
    using static DataLibrary.EnumList;

    public class TokenModel
    {
        /// <summary>
        /// 生成Token
        /// </summary>
        /// <param name="nameIdentifier">nameIdentifier</param>
        /// <param name="name">name</param>
        /// <param name="role">role</param>
        /// <param name="systemMemberType">systemMemberType</param>
        /// <returns>Token</returns>
        public static string Create(string nameIdentifier, string name, string role, SystemMemberType systemMemberType)
        {
            string tokenSecurity = "ThisIsXiaoyuTokenByWarrantyManage@2018-01-19#Cks3*&2kvmn32sch32D(VdwW";

            JwtSecurityToken jwt = new JwtSecurityToken(
            issuer: null,
            audience: name,
            claims: new Claim[] { },
            expires: DateTime.Now.AddHours(2),
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSecurity)), SecurityAlgorithms.HmacSha256));
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var claimsIdentity = new ClaimsPrincipal(new ClaimsIdentity(
                new Claim[]
             {
                            new Claim(ClaimTypes.NameIdentifier, nameIdentifier),
                            new Claim(ClaimTypes.Name, name),
                            new Claim(ClaimTypes.Role, role)
             }, systemMemberType.ToString()));

            StoreSignedUser.AddSignedUser(new SignedUser(encodedJwt, nameIdentifier.ToString(), claimsIdentity));

            return encodedJwt;
        }
    }
}
