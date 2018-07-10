// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WarrantyManage.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;

    public class ServiceController : Controller
    {
        private DataLibrary.DataContext db;

        public ServiceController(DataLibrary.DataContext db)
        {
            this.db = db;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            return this.View();
        }

        [HttpPost]
        public string SearchProduct(string batcode, string validcode)
        {
            if (batcode == null)
            {
                return "产品批号不能为空！";
            }

            if (validcode == null)
            {
                return "批号校验码不能为空！";
            }

            var rs = this.db.PdProduct.FirstOrDefault(x => x.Batcode == batcode && x.Randomcode == validcode);
            if (rs == null)
            {
                return "未找到当前批次产品或校验码不正确！";
            }

            return "success";
        }
    }
}
