using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Media;
using MCCS.Models.Model3D;

namespace MCCS.Models.Stations.Model3DSettings
{
    public class Model3DFileItemModel : BindableBase
    {
        private long _id;
        public long Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        private string _key = string.Empty;
        public string Key
        {
            get => _key;
            set => SetProperty(ref _key, value);
        }

        private string _fileName = string.Empty;
        public string FileName
        {
            get => _fileName;
            set => SetProperty(ref _fileName, value);
        }

        private string _filePath = string.Empty;
        public string FilePath
        {
            get => _filePath;
            set => SetProperty(ref _filePath, value);
        }

        private int _materialColor = 0;
        public int MaterialColor
        {
            get => _materialColor;
            set => SetProperty(ref _materialColor, value);
        }
        /// <summary>
        /// 是否可控制
        /// </summary>
        private bool _isCanControl;
        public bool IsCanControl
        {
            get => _isCanControl;
            set => SetProperty(ref _isCanControl, value);
        }
        /// <summary>
        /// 选中映射的硬件设备
        /// </summary>
        private MapDeviceModel _selectedMapDevice;
        public MapDeviceModel SelectedMapDevice 
        { 
            get => _selectedMapDevice;
            set
            {
                if (Equals(_selectedMapDevice, value)) return;
                if(_selectedMapDevice != null) _selectedMapDevice.IsSelected = false;
                SetProperty(ref _selectedMapDevice, value);
                _selectedMapDevice.IsSelected = true;
            }
        }

        /// <summary>
        /// 绑定的控制通道ID集合
        /// </summary>  
        public ObservableCollection<BindingControlChannelItemModel> BindedControlChannelIds { get; } = [];
    }

    public class BindedChannelModelBillboardTextInfo : BindableBase
    {
        public int Index { get; set; }

        private long _id; 
        public long Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }
         
        private long _bindedModelId;
        public long BindedModelId
        {
            get => _bindedModelId; 
            set => SetProperty(ref _bindedModelId, value);
        } 

        private SharpDX.Color _backgroundColor = SharpDX.Color.Black; 
        public SharpDX.Color BackgroundColor
        {
            get => _backgroundColor; 
            set => SetProperty(ref _backgroundColor, value);
        }

        private Color _backgroundColor1;
        public Color BackgroundColor1
        {
            get => _backgroundColor1;
            set
            {
                if (SetProperty(ref _backgroundColor1, value))
                {
                    BackgroundColor = new SharpDX.Color(_backgroundColor1.R, _backgroundColor1.G, _backgroundColor1.B,
                        _backgroundColor1.A);
                }
            }
        }

        private Color _fontColor1;
        public Color FontColor1
        {
            get => _fontColor1;
            set
            {
                if (SetProperty(ref _fontColor1, value))
                {
                    FontColor = new SharpDX.Color(_fontColor1.R, _fontColor1.G, _fontColor1.B,
                        _fontColor1.A);
                }
            }
        }
        /// <summary>
        /// 看板名称
        /// </summary> 
        private string _billboardName = string.Empty;  
        public string BillboardName
        {
            get => _billboardName; 
            set => SetProperty(ref _billboardName, value);
        }

        /// <summary>
        /// 字体颜色
        /// </summary>
        private SharpDX.Color _fontColor;
        public SharpDX.Color FontColor
        {
            get => _fontColor; 
            set => SetProperty(ref _fontColor, value);
        }

        /// <summary>
        /// 字体大小
        /// </summary>
        private int _fontSize;
        public int FontSize
        {
            get => _fontSize; 
            set => SetProperty(ref _fontSize, value);
        }

        /// <summary>
        /// 广告牌大小
        /// </summary>
        private float _scale;
        public float Scale
        {
            get => _scale;
            set => SetProperty(ref _scale, value);
        }

        private double _xDistance; 
        public double XDistance
        {
            get => _xDistance;
            set => SetProperty(ref _xDistance, value);
        }

        private double _yDistance;
        public double YDistance
        {
            get => _yDistance;
            set => SetProperty(ref _yDistance, value);
        }

        private double _zDistance;
        public double ZDistance
        {
            get => _zDistance;
            set => SetProperty(ref _zDistance, value);
        }

        private Model3DFileItemModel _selectedModel;
        public Model3DFileItemModel SelectedModel
        {
            get => _selectedModel;
            set => SetProperty(ref _selectedModel, value);
        }
        /// <summary>
        /// 广告牌类型
        /// </summary>
        private int _billboardType; 
        public int BillboardType
        {
            get => _billboardType;
            set => SetProperty(ref _billboardType, value);
        }

        private BindingControlChannelItemModel _selectedBindedChannel;
        [Obsolete]
        public BindingControlChannelItemModel SelectedBindedChannel
        {
            get => _selectedBindedChannel;
            set => SetProperty(ref _selectedBindedChannel, value);
        }
        /// <summary>
        /// 选中的虚拟通道
        /// </summary>
        private BindingPseudoChannelItemModel _selectedPseudoChannel; 
        public BindingPseudoChannelItemModel SelectedPseudoChannel
        {
            get => _selectedPseudoChannel;
            set => SetProperty(ref _selectedPseudoChannel, value);
        }
    }
}
