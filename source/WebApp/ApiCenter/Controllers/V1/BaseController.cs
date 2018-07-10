namespace WarrantyApiCenter.Controllers.V1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Controller父类，实现了版本控制
    /// </summary>
    [ApiVersion("1.0")]

    // [Produces("application/json")]
    [Route("api/v{version:apiVersion}/[Controller]")]
    public class BaseController : Controller
    {
    }
}
