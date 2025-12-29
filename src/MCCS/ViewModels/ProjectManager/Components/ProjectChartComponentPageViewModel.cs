using System.Collections.ObjectModel;

using LiveChartsCore;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;

using MCCS.Models;
using MCCS.Models.MethodManager.ParamterSettings;

using SkiaSharp;

namespace MCCS.ViewModels.ProjectManager.Components
{
    public class ProjectChartComponentPageViewModel : BindableBase
    {
        public ProjectChartComponentPageViewModel(ChartSettingParamModel parameter)
        {
            CurveSeries =
            [
                new LineSeries<CurveMeasureValueModel>()
                {
                    Values = ObservableValues,
                    Mapping = (model, index) => new Coordinate(model.XValue, model.YValue),
                    Fill = null,
                    GeometrySize = 0,
                    AnimationsSpeed = TimeSpan.Zero,
                    EasingFunction = null,
                    LineSmoothness = 1,
                }
            ];
            XAxes =
            [
                new Axis {
                    Name = parameter.XAxisParam?.DisplayName,
                    Labeler = value => value.ToString("N2"), // 保留两位小数
                    NamePaint = new SolidColorPaint
                    {
                        Color = SKColors.Black,
                        SKTypeface = SKTypeface.FromFamilyName("Microsoft YaHei")  // 指定中文字体
                    },
                    LabelsPaint = new SolidColorPaint
                    {
                        Color = SKColors.Black,
                        SKTypeface = SKTypeface.FromFamilyName("Microsoft YaHei")  // 标签也要加
                    },
                    AnimationsSpeed = TimeSpan.FromMilliseconds(0)
                }
            ];
            YAxes =
            [
                new Axis {
                    Name = parameter.YAxisParam?.DisplayName,
                    NamePaint = new SolidColorPaint
                    {
                        Color = SKColors.Black,
                        SKTypeface = SKTypeface.FromFamilyName("Microsoft YaHei")  // 指定中文字体
                    },
                    LabelsPaint = new SolidColorPaint
                    {
                        Color = SKColors.Black,
                        SKTypeface = SKTypeface.FromFamilyName("Microsoft YaHei")  // 标签也要加
                    }
                }
            ];
        }


        #region Property
        /// <summary>
        /// 曲线标题
        /// </summary>
        public string Title { get; set; } = string.Empty;
        /// <summary>
        /// 曲线内容集合(可以有多种不同的图表)
        /// </summary>
        public ObservableCollection<ISeries> CurveSeries { get; private set; }
        /// <summary>
        /// 所有的点(通用)
        /// </summary>
        public ObservableCollection<CurveMeasureValueModel> ObservableValues { get; private set; } = [];
        /// <summary>
        /// X轴信息
        /// </summary>
        public Axis[] XAxes { get; private set; }
        /// <summary>
        /// Y轴信息   
        /// </summary>
        public Axis[] YAxes { get; private set; }
        #endregion
    }
}
