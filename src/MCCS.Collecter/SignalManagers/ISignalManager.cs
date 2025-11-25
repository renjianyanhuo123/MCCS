using MCCS.Collecter.HardwareDevices;
using MCCS.Collecter.SignalManagers.Signals;
using MCCS.Infrastructure.Models.ProjectManager;
using MCCS.Infrastructure.TestModels.Commands;

namespace MCCS.Collecter.SignalManagers
{
    public interface ISignalManager
    {
        void Initialization(IEnumerable<HardwareSignalConfiguration> signalConfigurations);

       
        /// <summary>
        /// 根据单个信号ID获取数据流
        /// </summary>
        /// <param name="signalId"></param>
        /// <returns></returns>
        IObservable<DataPoint<float>> GetSignalDataStream(long signalId); 
        

        // TODO:目前先采用这种方案;后期可能是直接动态链接库采集所有控制器数据后,然后统一上传
        /// <summary>
        /// 所有的控制器和信号的存储
        /// </summary>
        /// <returns></returns>
        IObservable<List<ProjectDataRecordModel>> GetProjectDataRecords();

        /// <summary>
        /// 设置信号清零
        /// </summary>
        /// <param name="signalId"></param>
        /// <returns></returns>
        DeviceCommandContext SetSignalTare(long signalId);
    }
}
