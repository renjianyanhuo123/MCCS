using Newtonsoft.Json;

namespace MCCS.Station.Abstractions.Models
{
    [method: JsonConstructor]
    public class StationSitePseudoChannelInfo(long id, string name, string formula, bool hasTare, double rangeMax, double rangeMin, string unit)
    {
        public long Id { get; private set; } = id; 
        public string Name { get; private set; } = name;
        public string Formula { get; private set; } = formula;
        public bool HasTare { get; private set; } = hasTare;
        public double RangeMax { get; private set; } = rangeMax;
        public double RangeMin { get; private set; } = rangeMin;
        public string Unit { get; private set; } = unit;
        /// <summary>
        /// 所绑定的信号ID(减小序列化大小)
        /// </summary>
        public List<long> BindSignalIds { get; set; } = [];
    }
}
