// <copyright file="WebApiResponseModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using static DataLibrary.EnumList;

    /// <summary>
    /// 与WebApi中心交互时的数据返回格式
    /// </summary>
    public class WebApiResponseModel
    {
        public WebApiResponseModel(ApiResponseStatus status, string msg, string data)
        {
            this.Status = status;
            this.Msg = msg;
            this.Data = data;
        }

        public ApiResponseStatus Status { get; set; }

        public string Msg { get; set; }

        public string Data { get; set; }
    }
}
