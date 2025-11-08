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
        /// 根据单信号ID获取控制状态
        /// </summary>
        /// <param name="signalId"></param>
        /// <returns></returns>
        SystemControlState GetControlStateBySignalId(long signalId);
        /// <summary>
        /// 根据单信号ID设置控制器状态
        /// </summary>
        /// <param name="signalId"></param>
        /// <param name="controlMode"></param>
        /// <returns></returns>
        bool SetControlStateBySignalId(long signalId, SystemControlState controlMode);
        /// <summary>
        /// 设置单信号静态模式运行参数
        /// </summary>
        /// <param name="signalId">信号ID</param>
        /// <param name="tmpCtrlMode">静态运行模式</param>
        /// <param name="tmpVelo">速度</param>
        /// <param name="tmpPos">目标值</param>
        /// <returns></returns>
        int SetStaticControlMode(long signalId, StaticLoadControlEnum tmpCtrlMode, float tmpVelo, float tmpPos);
        /// <summary>
        /// 设置单通道的超限次数
        /// </summary>
        /// <param name="signalId"></param>
        /// <param name="freq"></param>
        /// <returns></returns>
        int SetValleyPeakFilterNum(long signalId, int freq);
        /// <summary>
        /// 设置单通道动态控制动作
        /// </summary>
        /// <param name="signalId">信号ID</param>
        /// <param name="tmpMeanA">中值</param>
        /// <param name="tmpA">幅值</param>
        /// <param name="tmpFreq">频率</param>
        /// <param name="tmpWaveShap">波形</param>
        /// <param name="tmpCtrlMode">控制模式(力、位移)</param>
        /// <param name="tmpAP">调幅值</param>
        /// <param name="tmpPH">调相值</param>
        /// <param name="tmpCountSet">循环次数</param>
        /// <param name="tmpCtrlOpt">控制过程选项  是否中值调整 起停振过程</param>
        /// <returns>=0  操作成功   =1 设备未连接  =2 设备断开错误</returns>
        int SetDynamicControlMode(long signalId, float tmpMeanA, float tmpA,
            float tmpFreq, byte tmpWaveShap, byte tmpCtrlMode, float tmpAP, float tmpPH,
            int tmpCountSet, int tmpCtrlOpt);
        IObservable<DataPoint<float>> GetSignalDataStream(long signalId);
    }
}
