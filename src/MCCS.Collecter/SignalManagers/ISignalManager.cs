using MCCS.Collecter.HardwareDevices;
using MCCS.Collecter.SignalManagers.Signals;

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
         
    }
}
