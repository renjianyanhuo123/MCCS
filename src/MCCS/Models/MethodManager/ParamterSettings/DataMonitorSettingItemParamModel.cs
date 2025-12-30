using MCCS.Models.CurveModels;

namespace MCCS.Models.MethodManager.ParamterSettings
{
    public class DataMonitorSettingItemParamModel
    {
        public required XyBindCollectionItem PseudoChannel { get; set; }
        /// <summary>
        /// 专门用于显示的单位(int类型写死于界面中)
        /// </summary>
        public int Unit { get; set; }

        public int RetainBit { get; set; }
    }
}
