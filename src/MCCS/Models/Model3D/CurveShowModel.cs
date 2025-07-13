using LiveChartsCore;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView;
using System.Collections.ObjectModel;

namespace MCCS.Models.Model3D
{
    public class CurveShowModel : BindableBase
    {
        public CurveShowModel(string xAxe, string yAxe) 
        {
            CurveSeries =
            [
                new LineSeries<CurveMeasureValueModel>()
                {
                    Values = ObservableValues,
                    Mapping = (model, index) => new Coordinate(model.XValue, model.YValue),
                    Fill = null
                }
            ];
            XAxes =
            [
                new() {
                    Name = xAxe
                }
            ];
            YAxes =
            [
                new() {
                    Name = yAxe
                }
            ];
        }
        /// <summary>
        /// 曲线标题
        /// </summary>
        public string Title { get; set; } = string.Empty;
        /// <summary>
        /// 设备Id
        /// </summary>
        public required string DeviceId { get; set; }
        /// <summary>
        /// Expander头部名称
        /// </summary>
        public string ExpanderHeaderName { get; set; } = string.Empty;
        /// <summary>
        /// 曲线内容集合(可以有多种不同的图表)
        /// </summary>
        public ObservableCollection<ISeries> CurveSeries { get; private set; }

        public ObservableCollection<CurveMeasureValueModel> ObservableValues { get; private set; } = [];
        /// <summary>
        /// X轴信息
        /// </summary>
        public Axis[] XAxes { get; private set; }
        /// <summary>
        /// Y轴信息   
        /// </summary>
        public Axis[] YAxes { get; private set; }
    }
}
