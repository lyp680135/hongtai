namespace Common.Service
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Common.IService;
    using DataLibrary;
    using Util;

    public class QualityService : IQualityService
    {
        private IBaseService<PdQuality> pdQuality;
        private IBaseService<MngSetting> mngSetting;
        private IBaseService<PdQualityProductPreset> pdQualityProductPreset;

        public QualityService(IBaseService<PdQuality> pdQuality, IBaseService<MngSetting> mngSetting, IBaseService<PdQualityProductPreset> pdQualityProductPreset)
        {
            this.pdQuality = pdQuality;
            this.mngSetting = mngSetting;
            this.pdQualityProductPreset = pdQualityProductPreset;
        }

        public List<PdQuality> GetQualityData(List<Tuple<string, int?>> batcodeMidList, bool isSimpleVer = false)
        {
            var qualityList = new List<PdQuality>();
            var mngsetInfo = this.mngSetting.FindSingle();

            // 流程版
            if (mngsetInfo.SystemVersion == EnumList.SystemVersion.流程版本 && !isSimpleVer)
            {
                if (batcodeMidList != null && batcodeMidList.Count > 0)
                {
                    // 循环炉批号,根据炉批号取质量数据
                    batcodeMidList.ToList().ForEach(o =>
                    {
                        var qualityInfo = this.pdQuality.FindSingle(f => f.Batcode == o.Item1 && f.CheckStatus == EnumList.CheckStatus_PdQuality.审核通过 && f.CreateFlag == (int)EnumList.SystemVersion.流程版本);
                        if (qualityInfo != null)
                        {
                            qualityList.Add(qualityInfo);
                        }
                    });
                }
            }

            // 简单版
            else if (mngsetInfo.SystemVersion == EnumList.SystemVersion.简单版本 || isSimpleVer)
            {
                // 根据炉批号和材质id查询所有的关系数据
                var qidList = new List<PdQualityProductPreset>();
                if (batcodeMidList != null && batcodeMidList.Count > 0)
                {
                    batcodeMidList.ToList().ForEach(o =>
                    {
                        var dataPresetInfo = this.pdQualityProductPreset.FindSingle(f => f.Batcode == o.Item1 && f.Materialid == o.Item2);
                        if (dataPresetInfo != null)
                        {
                            qidList.Add(dataPresetInfo);
                        }
                    });
                }

                if (qidList.Count > 0)
                {
                    // 根据关系数据表查找质量数据
                    qidList.ForEach(o =>
                    {
                        var qualityInfoById = this.pdQuality.FindSingle(f => f.Id == o.Qid);
                        if (qualityInfoById != null)
                        {
                            qualityList.Add(qualityInfoById);
                        }
                    });
                }
            }

            return qualityList;
        }
    }
}
