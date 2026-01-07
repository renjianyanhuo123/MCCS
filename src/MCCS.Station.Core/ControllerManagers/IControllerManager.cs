using MCCS.Station.Core.ControllerManagers.Entities;
using MCCS.Station.Core.DllNative.Models;
using MCCS.Station.Core.HardwareDevices;

namespace MCCS.Station.Core.ControllerManagers
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
        /// 获取当前所有可用控制器
        /// </summary>
        /// <returns></returns>
        IList<IController> GetControllers();

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

        /// <summary>
        /// 获取所有控制器的组合数据流
        /// 使用 CombineLatest 同步多个数据源，适用于图像绘制等场景
        /// </summary>
        /// <returns>组合后的数据流</returns>
        IObservable<SampleBatch<TNet_ADHInfo>[]> GetCombinedDataStream();

        /// <summary>
        /// 获取所有控制器的组合状态流
        /// </summary>
        /// <returns>组合后的状态流</returns>
        IObservable<HardwareConnectionStatus[]> GetCombinedStatusStream();
    }
}
