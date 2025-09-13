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

        public List<long> BindedControlChannelIds { get; set; } = [];
    }
}
