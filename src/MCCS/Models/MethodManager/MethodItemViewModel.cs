namespace MCCS.Models.MethodManager
{
    public class MethodItemViewModel : BindableBase
    {
        private long _id;
        public long Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        /// <summary>
        /// 方法名称
        /// </summary> 
        private string _name = string.Empty;
        public string Name 
        { 
            get => _name; 
            set => SetProperty(ref _name, value);
        }

        /// <summary>
        /// 方法类型
        /// </summary>
        private string _methodType = string.Empty; 
        public string MethodType
        {
            get => _methodType; 
            set => SetProperty(ref _methodType, value);
        }

        /// <summary>
        /// 试验类型
        /// </summary>
        private string _testType = string.Empty; 
        public string TestType
        {
            get => _testType; 
            set => SetProperty(ref _testType, value);
        } 
        /// <summary>
        /// 方法标准
        /// </summary> 
        private string _standard = string.Empty;
        public string Standard
        {
            get => _standard; 
            set => SetProperty(ref _standard, value);
        }

        /// <summary>
        /// 文件路径
        /// </summary> 
        private string _filePath = string.Empty;
        public string FilePath
        {
            get => _filePath; 
            set => SetProperty(ref _filePath, value);
        }

        private string _createTime = string.Empty; 
        public string CreateTime
        {
            get => _createTime;
            set => SetProperty(ref _createTime, value);
        }

        private string _updateTime = string.Empty;
        public string UpdateTime
        {
            get => _updateTime;
            set => SetProperty(ref _updateTime, value);
        }
    }
}
