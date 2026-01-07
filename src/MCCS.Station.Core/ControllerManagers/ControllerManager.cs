using System.Reactive.Linq;

using MCCS.Infrastructure.Helper;
using MCCS.Station.Core.ControllerManagers.Entities;
using MCCS.Station.Core.DllNative;
using MCCS.Station.Core.DllNative.Models;
using MCCS.Station.Core.HardwareDevices;

namespace MCCS.Station.Core.ControllerManagers
{
    /// <summary>
    /// 控制器管理(务必保持单例)
    /// </summary>
    public sealed class ControllerManager : IControllerManager
    {
        private static volatile bool _isDllInitialized;
        private readonly Dictionary<long, IController> _controllers = [];
        private bool _isMock; 

        public bool InitializeDll(bool isMock = false)
        {
            if (_isDllInitialized || isMock) 
            {
                _isMock = true;
                return true;
            }
            if (!FileHelper.FileExists(AddressContanst.DllName)) throw new DllNotFoundException("DLL文件不存在");
            var result = POPNetCtrl.NetCtrl01_Init();
            if (result == AddressContanst.OP_SUCCESSFUL)
            {
                _isDllInitialized = true;
                return true;
            }
            throw new Exception($"DLL初始化失败,错误码:{result}"); 
        } 
         
        public IController GetControllerInfo(long controllerId)
        { 
            return _controllers[controllerId];
        }

        public IList<IController> GetControllers()
        {
            return _controllers.Values.ToList();
        }

        public bool OperationTest(bool isStart)
        {
            var temp = isStart ? 1u : 0u;
            return _controllers.Select(controller => controller.Value.OperationTest(temp)).All(success => success);
        } 

        public bool CreateController(HardwareDeviceConfiguration configuration)
        {
            IController controller;
            if (_isMock)
            {
                controller = new MockControllerHardwareDevice(configuration);
            }
            else
            {
                controller = new BwControllerHardwareDevice(configuration);
            } 
            controller.ConnectToHardware();
            _controllers.Add(configuration.DeviceId, controller);
            return true;
        }
         
        public bool RemoveController(int deviceId)
        {
            var controller = _controllers[deviceId]; 
            controller.DisconnectFromHardware(); 
            _controllers.Remove(deviceId);
            controller.Dispose();
            return true;
        }
         
        public void StartAllControllers()
        {
            foreach (var controller in _controllers.Values)
            { 
                controller.StartDataAcquisition();
            }
        }
         
        public void StopAllControllers()
        {
            foreach (var controller in _controllers.Values)
            {
                controller.StopDataAcquisition();
            }
        }

        public IObservable<SampleBatch<TNet_ADHInfo>[]> GetCombinedDataStream()
        {
            var controllerList = _controllers.Values.ToList();
            if (controllerList.Count == 0)
                return Observable.Empty<SampleBatch<TNet_ADHInfo>[]>();

            if (controllerList.Count == 1)
                return controllerList[0].DataStream.Select(x => new[] { x });

            // 使用 CombineLatest 同步多个数据源的最新数据
            return controllerList
                .Select(c => c.DataStream)
                .CombineLatest();
        }

        public IObservable<HardwareConnectionStatus[]> GetCombinedStatusStream()
        {
            var controllerList = _controllers.Values.ToList();
            if (controllerList.Count == 0)
                return Observable.Empty<HardwareConnectionStatus[]>();

            return controllerList
                .Select(c => c.StatusStream)
                .CombineLatest();
        }

        public void Dispose()
        {
            foreach (var controller in _controllers.Values)
            {
                controller.Dispose();
            }
            _controllers.Clear();
            if (_isDllInitialized)
            {
                _isDllInitialized = false;
            }
            NativeBufferPool.Clear();
        }
    }
}
