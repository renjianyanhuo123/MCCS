using MCCS.Infrastructure.TestModels;
using MCCS.Infrastructure.TestModels.Commands;
using MCCS.Infrastructure.TestModels.ControlParams;

namespace MCCS.Station.ControlChannelManagers
{
    public interface IControlChannel
    {
        /// <summary>
        /// 阀门状态
        /// </summary>
        ValveStatusEnum ValveStatus { get; }
        /// <summary>
        /// 控制通道是否处于起振状态
        /// </summary>
        bool IsDynamicVibration { get; }
        /// <summary>
        /// 控制模式
        /// </summary>
        SystemControlState ControlState { get; }
        /// <summary>
        /// 操作控制阀门
        /// </summary>
        /// <param name="isOpen"></param>
        /// <returns></returns>
        bool OperationValve(bool isOpen);
        /// <summary>
        /// 手动控制
        /// </summary>
        /// <param name="speed"></param>
        /// <returns></returns>
        DeviceCommandContext ManualControl(float speed);

        /// <summary>
        /// 静态控制
        /// </summary>
        /// <param name="controlParam"></param>
        /// <returns></returns>
        DeviceCommandContext StaticControl(StaticControlParams controlParam);
        /// <summary>
        /// 动态控制
        /// </summary>
        /// <param name="dynamicControlParam"></param>
        /// <returns></returns>
        DeviceCommandContext DynamicControl(DynamicControlParams dynamicControlParam);
        /// <summary>
        /// 停止控制
        /// </summary>
        /// <returns></returns>
        DeviceCommandContext StopControl();
    }
}
