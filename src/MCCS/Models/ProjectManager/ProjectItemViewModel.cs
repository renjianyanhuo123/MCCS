namespace MCCS.Models.ProjectManager
{
    public class ProjectItemViewModel : BindableBase
    {
        private long _id;
        public long Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private string _code = string.Empty;
        public string Code
        {
            get => _code;
            set => SetProperty(ref _code, value);
        }

        private string _methodName = string.Empty;
        public string MethodName
        {
            get => _methodName;
            set => SetProperty(ref _methodName, value);
        }

        private string _standard = string.Empty;
        public string Standard
        {
            get => _standard;
            set => SetProperty(ref _standard, value);
        }

        private string _person = string.Empty;
        public string Person
        {
            get => _person;
            set => SetProperty(ref _person, value);
        }

        private string _filePath = string.Empty;
        public string FilePath
        {
            get => _filePath;
            set => SetProperty(ref _filePath, value);
        }

        private string _testTime = string.Empty;
        public string TestTime
        {
            get => _testTime;
            set => SetProperty(ref _testTime, value);
        }

        private string _startTime = string.Empty;
        public string StartTime
        {
            get => _startTime;
            set => SetProperty(ref _startTime, value);
        }

        private string _remark = string.Empty;
        public string Remark
        {
            get => _remark;
            set => SetProperty(ref _remark, value);
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
