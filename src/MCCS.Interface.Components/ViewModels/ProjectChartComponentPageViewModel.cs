using System.Collections.ObjectModel;

using LiveChartsCore;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;

using MCCS.Interface.Components.Attributes;
using MCCS.Interface.Components.Enums;
using MCCS.Interface.Components.Models;
using MCCS.Interface.Components.Models.ParamterModels;
using MCCS.Interface.Components.ViewModels.Parameters;

using SkiaSharp;

namespace MCCS.Interface.Components.ViewModels
{
    [InterfaceComponent(
        "chart-component",
        "图表组件",
        InterfaceComponentCategory.Display,
        Description = "用于控制通道操作和参数设置",
        IsCanSetParam = true,
        SetParamViewName = nameof(MethodChartSetParamPageViewModel),
        Icon = "Cogs",
        Order = 3)]
    public class ProjectChartComponentPageViewModel : BaseComponentViewModel
    {
        public ProjectChartComponentPageViewModel(

            ChartSettingParamModel parameter
        )
        {
            CurveSeries =
            [
                new LineSeries<CurveMeasureValueModel>()
                {
                    Values = ObservableValues,
                    Mapping = (model, _) => new Coordinate(model.XValue, model.YValue),
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
