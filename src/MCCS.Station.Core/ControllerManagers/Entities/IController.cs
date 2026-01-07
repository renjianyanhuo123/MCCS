using MCCS.Infrastructure.TestModels;
using MCCS.Infrastructure.TestModels.ControlParams;
using MCCS.Station.Core.DllNative.Models;
using MCCS.Station.Core.HardwareDevices;

namespace MCCS.Station.Core.ControllerManagers.Entities
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
        IObservable<SampleBatch<TNet_ADHInfo>> DataStream { get; }
        /// <summary>
        /// 单个数据流 - 从批量数据展开为单条数据
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

        /// <summary>
        /// 控制通道清零 
        /// </summary>
        /// <param name="controlType">0=位移通道   1=试验力通道</param>
        /// <returns>0-成功  10-控制器不在静态状态 20-控制器正处于静态运行状态</returns>
        int SetSignalTare(int controlType);
        /// <summary>
        /// 获取控制器的静态控制模式
        /// </summary>
        /// <returns></returns>
        StaticLoadControlEnum GetStaticLoadControl();
    }
}
