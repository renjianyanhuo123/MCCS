using MCCS.Collecter.HardwareDevices;
using MCCS.Collecter.SignalManagers.Signals;
using MCCS.Infrastructure.TestModels;
using MCCS.Infrastructure.TestModels.ControlParams;

namespace MCCS.Collecter.SignalManagers
{
    public interface ISignalManager
    {
        void Initialization(IEnumerable<HardwareSignalConfiguration> signalConfigurations);
        /// <summary>
        /// 根据信号ID获取控制状态
        /// </summary>
        /// <param name="signalId"></param>
        /// <returns></returns>
        SystemControlState GetControlStateBySignalId(long signalId);
        /// <summary>
        /// 根据信号ID设置控制器状态
        /// </summary>
        /// <param name="signalId"></param>
        /// <param name="controlMode"></param>
        /// <returns></returns>
        bool SetControlStateBySignalId(long signalId, SystemControlState controlMode);
        /// <summary>
        /// 设置静态模式运行参数
        /// </summary>
        /// <param name="signalId">信号ID</param>
        /// <param name="tmpCtrlMode">静态运行模式</param>
        /// <param name="tmpVelo">速度</param>
        /// <param name="tmpPos">目标值</param>
        /// <returns></returns>
        int SetStaticControlMode(long signalId, StaticLoadControlEnum tmpCtrlMode, float tmpVelo, float tmpPos);
        IObservable<DataPoint<float>> GetSignalDataStream(long signalId);
    }
}
