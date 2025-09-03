using MCCS.Core.Models.Devices;

namespace MCCS.Models.Hardwares
{
    public class HardwareListItemViewModel : BindableBase
    {
        private long _id;
        /// <summary>
        /// 硬件ID
        /// </summary>
        public long Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        private string _name;
        /// <summary>
        /// 硬件名称
        /// </summary>
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private DeviceTypeEnum _type;
        /// <summary>
        /// 硬件类型
        /// </summary>
        public DeviceTypeEnum Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }

        private string _description;
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        private string _createTime;
        public string CreateTime
        {
            get => _createTime;
            set => SetProperty(ref _createTime, value);
        }

        private string _updateTime;
        public string UpdateTime
        {
            get => _updateTime;
            set => SetProperty(ref _updateTime, value);
        } 
    }
}
