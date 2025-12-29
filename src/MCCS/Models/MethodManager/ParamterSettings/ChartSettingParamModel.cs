using MCCS.Models.CurveModels;

namespace MCCS.Models.MethodManager.ParamterSettings
{
    public class ChartSettingParamModel
    {
        public ChartTypeEnum Type { get; set; }

        public required XyBindCollectionItem? XAxisParam { get; set; }

        public required XyBindCollectionItem? YAxisParam { get; set; }
    }
}
