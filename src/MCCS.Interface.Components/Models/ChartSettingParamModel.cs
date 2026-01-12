using MCCS.Interface.Components.Enums;

namespace MCCS.Interface.Components.Models
{
    public class ChartSettingParamModel
    {
        public ChartTypeEnum Type { get; set; }

        public required XyBindCollectionItem? XAxisParam { get; set; }

        public required XyBindCollectionItem? YAxisParam { get; set; }
    }
}
