namespace WarrantyApiCenter.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Caching.Memory;

    /// <summary>
    /// Token机制使用，记录当前已登录（JWT验证）的用户
    /// </summary>
    public class StoreSignedUser
    {
        private static MemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());

        /// <summary>
        /// 新增已登录用户
        /// </summary>
        /// <param name="user">user</param>
        public static void AddSignedUser(SignedUser user)
        {
            memoryCache.Set(user.UserToken, user, new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(60 * 8)));

            // 8个小时的 相对过期时间（如果有这期间有访问，则继续延长）
        }

        /// <summary>
        /// 检查用户是否已登录
        /// </summary>
        /// <param name="tokenHeader">tokenHeader</param>
        /// <returns>SignedUser</returns>
        public static SignedUser ExistsSignedUser(string tokenHeader)
        {
            var obj = memoryCache.Get(tokenHeader);
            return obj as SignedUser;
        }
    }
}
