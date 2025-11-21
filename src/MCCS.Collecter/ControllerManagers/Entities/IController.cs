using MCCS.Collecter.DllNative.Models;
using MCCS.Collecter.HardwareDevices;
using MCCS.Infrastructure.TestModels;
using MCCS.Infrastructure.TestModels.ControlParams;

namespace MCCS.Collecter.ControllerManagers.Entities
{
    public interface IController: IHardwareDevice
    {
        /// <summary>
        /// 设备ID
        /// </summary>
        long DeviceId { get; }
        /// <summary>
        /// 批量数据流
        /// </summary>
        IObservable<DataPoint<List<TNet_ADHInfo>>> DataStream { get; }
        /// <summary>
        /// 单个数据流
        /// </summary>
        IObservable<DataPoint<TNet_ADHInfo>> IndividualDataStream { get; } 
        /// <summary>
        /// 状态流
        /// </summary>
        IObservable<HardwareConnectionStatus> StatusStream { get; }
        /// <summary>
        /// 设置试验状态
        /// </summary>
        /// <param name="isStart"></param>
        /// <returns></returns>
        bool OperationTest(uint isStart);
        /// <summary>
        /// 获取阀门状态
        /// </summary>
        /// <returns></returns>
        ValveStatusEnum GetValveStatus();

        /// <summary>
        /// 设置阀门状态
        /// </summary> 
        /// <param name="isOpen"></param>
        /// <returns></returns>
        int SetValveStatus(bool isOpen);
        /// <summary>
        /// 获取控制状态
        /// </summary> 
        /// <returns></returns>
        SystemControlState GetControlState();
        /// <summary>
        /// 设置控制器状态
        /// </summary>
        /// <param name="controlMode"></param>
        /// <returns></returns>
        bool SetControlState(SystemControlState controlMode);
        /// <summary>
        /// 设置静态模式运行参数
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        int SetStaticControlMode(StaticControlParams param);
        /// <summary>
        /// 设置超限次数
        /// </summary> 
        /// <param name="freq"></param>
        /// <returns></returns>
        int SetValleyPeakFilterNum(int freq);
        /// <summary>
        /// 设置动态控制动作
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        int SetDynamicControlMode(DynamicControlParams param);
        /// <summary>
        /// 设置动态暂停控制
        /// </summary>
        /// <param name="tmpActMode">0-当前位置暂停 1-有停振 起振过程</param>
        /// <param name="tmpHaltState">1-暂停，0-取消暂停</param>
        /// <returns></returns>
        int SetDynamicStopControl(int tmpActMode, int tmpHaltState); 
    }
}
