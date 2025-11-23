using MCCS.Infrastructure.Models.TestInfo;

namespace MCCS.ViewModels.Others
{
    public class TestViewModel : BindableBase
    {
        private long _id;
        private string _code = string.Empty;
        private string _name = string.Empty;
        private string _stanard = string.Empty;
        private string _person = string.Empty;
        private string _remark = string.Empty;
        private string _filePath = string.Empty;
        private TestStatus _status;
        private DateTime? _startTime;
        private DateTime? _endTime;
        private string _processingStr = string.Empty;
        private DateTime? _createtime;

        public long Id
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
            set 
            {
                if (_startTime != value) 
                {
                    ProcessingStr = CaluateProcessingStr(value, EndTime);
                    SetProperty(ref _startTime, value);
                }
            }
        }
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime
        {
            get => _endTime;
            set 
            {
                if (value != _endTime) 
                {
                    ProcessingStr = CaluateProcessingStr(StartTime, value);
                    SetProperty(ref _endTime, value);
                }
            }
        }

        /// <summary>
        /// 累计时长, 以,区分 3个则为时,分,秒  2个则为分和秒 
        /// </summary>
        public string ProcessingStr 
        {
            get => _processingStr;
            set 
            {
                SetProperty(ref _processingStr, value);
            }
        }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime
        {
            get => _createtime;
            set => SetProperty(ref _createtime, value);
        }

        /// <summary>
        /// 计算时间差
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        private string CaluateProcessingStr(DateTime? start, DateTime? end) 
        {
            if(start == null) return string.Empty;
            if (end == null) end = DateTime.Now;
            var timeDiff = end - start;
            if (timeDiff.Value.TotalSeconds < 0) return string.Empty;
            int hours = timeDiff.Value.Hours;
            int minutes = timeDiff.Value.Minutes;
            int seconds = timeDiff.Value.Seconds;
            if (hours > 0) return $"{hours:D2}时{minutes:D2}分{seconds:D2}秒";
            return $"{minutes:D2}分{seconds:D2}秒";
        }
    }
}
