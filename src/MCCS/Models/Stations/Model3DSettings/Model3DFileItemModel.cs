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
        /// 绑定的控制通道ID集合
        /// </summary>
        public List<long> BindedControlChannelIds { get; set; } = []; 
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

        private long _bindedControlChannelId;
        public long BindedControlChannelId
        {
            get => _bindedControlChannelId; 
            set => SetProperty(ref _bindedControlChannelId, value);
        }

        private long _bindedModelId;
        public long BindedModelId
        {
            get => _bindedModelId; 
            set => SetProperty(ref _bindedModelId, value);
        }

        private long _bindedModelFileId;
        public long BindedModelFileId
        {
            get => _bindedModelFileId;
            set => SetProperty(ref _bindedModelFileId, value);
        }

        private SharpDX.Color _backgroundColor = SharpDX.Color.Black; 
        public SharpDX.Color BackgroundColor
        {
            get => _backgroundColor; 
            set => SetProperty(ref _backgroundColor, value);
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
    }
}
