namespace WarrantyApiCenter.Controllers.Token
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using WarrantyApiCenter.Models;

    /// <summary>
    /// 检查Token是否有效
    /// </summary>
    [Produces("application/json")]
    [Route("api/TokenCheck")]
    public class TokenCheckController : Controller
    {
        [HttpGet]
        public ResponseModel Get()
        {
            return new ResponseModel(DataLibrary.EnumList.ApiResponseStatus.Success, "Token有效", string.Empty);
        }
    }
}
