namespace Common.Service
{
    public enum CommonResultStatus
    {
        Failed = 0,
        Success = 1,
    }

    public class CommonResult
    {
        /// <summary>
        /// 返回状态： 0，失败，1，成功
        /// </summary>
        public int Status;

        /// <summary>
        /// 返回提醒
        /// </summary>
        public string Message;

        /// <summary>
        /// 返回失败状态时的错误原因
        /// </summary>
        public string Reason;

        /// <summary>
        /// 返回成功时的数据
        /// </summary>
        public object Data;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommonResult"/> class.
        /// 构造返回值结构体
        /// </summary>
        /// <param name="status">返回状态： 0，失败，1，成功</param>
        /// <param name="message">返回提醒</param>
        /// <param name="reason">返回失败状态时的错误原因</param>
        /// <param name="data">返回成功时的数据</param>
        public CommonResult(CommonResultStatus status, string message, string reason, object data = null)
        {
            this.Status = (int)status;
            this.Message = message;
            this.Reason = reason;
            this.Data = data;
        }
    }
}
