using LiveChartsCore;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using MCCS.Collecter.PseudoChannelManagers;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Reactive.Linq;

namespace MCCS.Models.CurveModels
{
    public class CurveShowModel : BindableBase, IDisposable
    {
        private readonly IPseudoChannelManager _channelManager;
        private IDisposable? _tableDispose;
        private long _tableInitTime = 0;
        private const int MaxVisiblePoints = 500;

        private readonly XYBindCollectionItem _selectedXBindItem;
        private readonly XYBindCollectionItem _selectedYBindItem;

        public CurveShowModel(XYBindCollectionItem xAxe, XYBindCollectionItem yAxe, IPseudoChannelManager pseudoChannelManager)
        {
            _channelManager = pseudoChannelManager;
            _selectedXBindItem = xAxe;
            _selectedYBindItem = yAxe;
            Title = $"{xAxe.DisplayName}-{yAxe.DisplayName}";
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
                    LineSmoothness = 1
                }
            ];
            XAxes =
            [
                new Axis {
                    Name = _selectedXBindItem.DisplayName,
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
                    Name = _selectedYBindItem.DisplayName,
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
            StartUpdateDataShow();
        } 

        /// <summary>
        /// 开始更新表格数据
        /// </summary>
        private void StartUpdateDataShow()
        { 
            IObservable<CurveMeasureValueModel> combinedStream;
            _tableInitTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var yChannelStream = _channelManager.GetPseudoChannelById(_selectedYBindItem.Id).GetPseudoChannelStream();
            if (_selectedXBindItem.Id != 0)
            {
                var xChannelStream = _channelManager.GetPseudoChannelById(_selectedXBindItem.Id).GetPseudoChannelStream();
                combinedStream = yChannelStream.CombineLatest(xChannelStream, (y, x) => new CurveMeasureValueModel
                {
                    XValue = x.Value,
                    YValue = y.Value
                });
            }
            else
            {
                combinedStream = yChannelStream.Select(s => new CurveMeasureValueModel
                {
                    XValue = (s.Timestamp - _tableInitTime) / 1000.0,
                    YValue = s.Value
                });
            } 
            _tableDispose = combinedStream
                .Buffer(TimeSpan.FromMilliseconds(400))
                .Where(batch => batch.Count > 0)
                .Subscribe(data =>
            {
                ObservableValues.AddRange(data);
                var count = ObservableValues.Count - MaxVisiblePoints;
                // 批量移除以提高性能
                for (var i = 0; i < count; i++)
                {
                    ObservableValues.RemoveAt(0);
                }
            });
        }
         

        /// <summary>
        /// 曲线ID
        /// </summary>
        public string CurveId { get; private set; } = Guid.NewGuid().ToString("N"); 
        /// <summary>
        /// 是否显示
        /// </summary>
        private bool _isShow = false;
        public bool IsShow
        {
            get => _isShow;
            set => SetProperty(ref _isShow, value);
        } 
        /// <summary>
        /// 曲线标题
        /// </summary>
        public string Title { get; set; } = string.Empty; 
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

        public void Dispose()
        {
            _tableDispose?.Dispose();
        }
    }
}
