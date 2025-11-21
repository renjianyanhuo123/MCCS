using MCCS.Collecter.ControllerManagers.Entities;
using MCCS.Collecter.HardwareDevices;

namespace MCCS.Collecter.ControllerManagers
{
    public interface IControllerManager : IDisposable
    { 
        /// <summary>
        /// 初始化DLL信息
        /// </summary>
        /// <param name="isMock"></param>
        /// <returns></returns>
        bool InitializeDll(bool isMock = false);

        /// <summary>
        /// 获取控制器
        /// </summary>
        /// <param name="controllerId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        IController GetControllerInfo(long controllerId);
        /// <summary>
        /// 创建控制器
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        bool CreateController(HardwareDeviceConfiguration configuration);
        /// <summary>
        /// 移除控制器
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        bool RemoveController(int deviceId);
        /// <summary>
        /// 操作整个实验(这个暂时放在这里)
        /// </summary>
        /// <param name="isStart"></param>
        /// <returns></returns>
        bool OperationTest(bool isStart);
        /// <summary>
        /// 开启所有控制器的数据采集
        /// 这里相当于硬件上的总开关
        /// </summary>
        void StartAllControllers();
        /// <summary>
        /// 停止所有控制器的数据采集并断开连接
        /// 这里相当于硬件上的总开关
        /// </summary>
        void StopAllControllers();
    }
}
