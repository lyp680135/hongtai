namespace WarrantyApiCenter.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using DataLibrary;
    using static DataLibrary.EnumList;

    /// <summary>
    /// API返回数据实体类
    /// </summary>
    public class ResponseModel
    {
        public ResponseModel(ApiResponseStatus status, string msg, string data)
        {
            this.Status = status;
            this.Msg = msg ?? throw new ArgumentNullException(nameof(msg));
            this.Data = data ?? throw new ArgumentNullException(nameof(data));
        }

        public ApiResponseStatus Status { get; set; }

        public string Msg { get; set; }

        public string Data { get; set; }
    }
}
