namespace DataLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// 系统枚举列表
    /// </summary>
    public class EnumList
    {
        /// <summary>
        /// 权限类别
        /// </summary>
        public enum PermissionType
        {
            PC = 0,
            WAP = 1,
            ALL = 2
        }

        /// <summary>
        /// 计量方式
        /// </summary>
        public enum MeteringMode
        {
            磅计 = 0,
            理计 = 1,
            抄码 = 2
        }

        /// <summary>
        /// 交货状态
        /// </summary>
        public enum DeliveryType
        {
            直条定尺 = 0,
            直条非尺 = 1,
            盘卷 = 2
        }

        /// <summary>
        /// 产品质量等级
        /// </summary>
        public enum ProductQualityLevel
        {
            短尺 = -1,
            定尺 = 0,
            长尺 = 1,
            标准 = 10,
        }

        /// <summary>
        /// 系统初始角色列表
        /// </summary>
        public enum GroupManage
        {
            管理员 = 1,
            业务经理 = 2,
            品质主管 = 3,
            质量员 = 4,
            入库操作员 = 5,
            出库操作员 = 6,
            网站管理员 = 7,
            炉号工 = 8
        }

        /// <summary>
        /// 产品质量数据录入审核
        /// </summary>
        public enum CheckStatus_PdQuality
        {
            不需要审核 = -1,
            等待审核 = 0,
            审核通过 = 1,
            审核不通过 = 2
        }

        /// <summary>
        /// 新闻类别表（包含产品、风采）枚举标识
        /// </summary>
        public enum NewsSystemType
        {
            xw = 1,

            // 企业风采
            fc = 2,

            // 企业产品
            cp = 3,

            // 集团风采
            jfc = 4,

            // 集团产品
            jcp = 5
        }

        /// <summary>
        /// WEB API 实体返回状态码
        /// </summary>
        public enum ApiResponseStatus
        {
            /// <summary>
            /// 失败
            /// </summary>
            Failed = 0,

            /// <summary>
            /// 成功
            /// </summary>
            Success = 1
        }

        /// <summary>
        /// 系统会员分类
        /// </summary>
        public enum SystemMemberType
        {
            /// <summary>
            /// 钢厂人员
            /// </summary>
            Manage = 0,

            /// <summary>
            /// 经销商
            /// </summary>
            Seller = 1
        }

        /// <summary>
        /// 手机短信验证码发送类别
        /// </summary>
        public enum MobileCodeType
        {
            登录验证码 = 1
        }

        /// <summary>
        /// 页面元素类型
        /// </summary>
        public enum PageShowType
        {
            文本 = 1,
            图片 = 2,
            数字 = 3,
            邮箱 = 4,
            手机号 = 5,
            富文本框 = 6,
            下拉框 = 7,
            上传 = 8,
            单选按钮 = 9,
            复选框 = 10,
            时间选择框 = 11
        }

        /// <summary>
        /// 字段类型
        /// </summary>
        public enum FileType
        {
            文本 = 1,
            数字 = 2,
            时间 = 3,
            货币 = 4
        }

        /// <summary>
        /// 模型录入审核
        /// </summary>
        public enum CheckStatus_ManageModel
        {
            等待审核 = 0,
            审核通过 = 1,
            审核不通过 = 2
        }

        /// <summary>
        /// 字段是否为空
        /// </summary>
        public enum FileIsNull
        {
            不必填 = 0,
            必填 = 1
        }

        /// <summary>
        /// 是否已经添加模型内容
        /// </summary>
        public enum HasModelContent
        {
            否 = 0,
            是 = 1
        }

        /// <summary>
        /// 系统的标识
        /// </summary>
        public enum SystemVersion
        {
            流程版本 = 0,
            简单版本 = 1
        }

        /// <summary>
        /// 友情链接显示位置控制
        /// </summary>
        public enum SiteLinkPostion
        {
            PC端显示 = 1,
            移动端显示 = 2,
            PC移动全显示 = 3
        }

        /// <summary>
        /// 显示链接样式控制
        /// </summary>
        public enum SiteLinkType
        {
            文字链接 = 1,
            图片链接 = 2
        }

        /// <summary>
        /// 是否显示
        /// </summary>
        public enum SiteIsShow
        {
            是 = 1,
            否 = 0
        }

        /// <summary>
        /// 质保书状态
        /// </summary>
        public enum SalePrintlogStatus
        {
            预览未下载 = 0,
            已下载 = 1,
            已撤回 = 10
        }

        /// <summary>
        /// 指标类别
        /// </summary>
        public enum TargetCategory
        {
            化学指标 = 1,
            物理指标 = 2
        }
    }
}
