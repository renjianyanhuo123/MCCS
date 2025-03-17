using MCCS.Models;

namespace MCCS.ViewModels.Others
{
    public class TestViewModel : BindableBase
    {
        private int _id;
        private string _code;
        private string _name;
        private string _stanard;
        private string _person;
        private string _remark;
        private string _filePath;
        private TestStatus _status;
        private DateTime? _startTime;
        private DateTime? _endTime;


        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }
        /// <summary>
        /// 试样编号
        /// </summary>
        public string Code
        {
            get => _code;
            set => SetProperty(ref _code, value);
        }
        /// <summary>
        /// 试样名称
        /// </summary>
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
        /// <summary>
        /// 试样标准
        /// </summary>
        public string Standard
        {
            get => _stanard;
            set => SetProperty(ref _stanard, value);
        }
        /// <summary>
        /// 试验人员
        /// </summary>
        public string Person
        {
            get => _person;
            set => SetProperty(ref _person, value);
        }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark
        {
            get => _remark;
            set => SetProperty(ref _remark, value);
        }
        /// <summary>
        /// 文件路径
        /// </summary>
        public string FilePath
        {
            get => _filePath;
            set => SetProperty(ref _filePath, value);
        }
        /// <summary>
        /// 试验状态
        /// </summary>
        public TestStatus Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? StartTime
        {
            get => _startTime;
            set => SetProperty(ref _startTime, value);
        }
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime
        {
            get => _endTime;
            set => SetProperty(ref _endTime, value);
        }
    }
}
