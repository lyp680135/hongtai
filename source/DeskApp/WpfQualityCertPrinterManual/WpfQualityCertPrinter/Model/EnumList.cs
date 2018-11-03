using System;
using System.Collections.Generic;
using System.Text;

namespace DataLibrary
{
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
        /// 系统初始角色列表
        /// </summary>
        public enum GroupManage
        {
            管理员 = 1,
            业务经理 = 2,
            品质主管 = 3,
            质量员 = 4
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
        /// 系统的标识
        /// </summary>
        public enum SystemVersion
        {
            流程版本 = 0,
            简单版本 = 1
        }
        /// <summary>
        /// 材质是否作废
        /// </summary>
        public enum MaterialIsCancel
        {
            未作废 = 0,
            作废 = 1
        }
    }
}
