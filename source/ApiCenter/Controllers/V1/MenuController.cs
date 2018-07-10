namespace WarrantyApiCenter.Controllers.V1
{
    using System.Linq;
    using Common.IService;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using WarrantyApiCenter.Models;
    using static DataLibrary.EnumList;

    /// <summary>
    /// 菜单
    /// </summary>
    [Produces("application/json")]
    public class MenuController : BaseController
    {
        private IUserService userService;
        private DataLibrary.DataContext db;

        public MenuController(IUserService userService, DataLibrary.DataContext dataContext)
        {
            this.userService = userService;
            this.db = dataContext;
        }

        [HttpGet]
        public ResponseModel Get()
        {
            JArray ja = new JArray();

            // 如果是钢厂人员
            if (this.userService.IsManageMember)
            {
                // 钢厂人员登录实体类
                var applicationUser = this.userService.ApplicationUser;
                int[] permissions = applicationUser.PermissionIds.ToArray();

                // 调用 数据库配置的菜单
                var menuList = this.db.MngMenuclass.Where(c => c.PermissionType == PermissionType.WAP
                      && c.IsPermission == false
                      && permissions.Contains(c.Id)).OrderBy(c => c.Sequence).ToList();

                foreach (var ml in menuList)
                {
                    ja.Add(new JObject { { "name", ml.ClassName }, { "url", ml.Url } });
                }
            }
            else
            {
                // var salerUser = _userService.SaleSellerUser; //经销商实体类

                // 在这里写死菜单按钮
                ja.Add(new JObject { { "name", "授权资源" }, { "url", "/WarrantyAuth" } });
                ja.Add(new JObject { { "name", "质保书打印" }, { "url", "/WarrantyPrint" } });
            }

            return new ResponseModel(DataLibrary.EnumList.ApiResponseStatus.Success, "请求成功", JsonConvert.SerializeObject(ja));
        }

        [HttpGet("{id}")]
        public JsonResult Get(int id)
        {
            return this.Json("value");
        }

        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}