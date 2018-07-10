﻿namespace WarrantyManage.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using UEditorNetCore;

    [Route("api/[controller]")] // 配置路由
    public class UEditorController : Controller
    {
        private UEditorService ue;

        public UEditorController(UEditorService ue)
        {
            this.ue = ue;
        }

        public void Do()
        {
            this.ue.DoAction(this.HttpContext);
        }
    }
}