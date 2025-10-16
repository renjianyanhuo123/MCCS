using System.Diagnostics;
using System.Reactive.Linq;
using MCCS.Collecter.DllNative;

namespace MCCS.Collecter.HardwareDevices.BwController
{
    public sealed class BwControllerHardwareDevice : ControllerHardwareDeviceBase
    {
        public BwControllerHardwareDevice(HardwareDeviceConfiguration configuration) : base(configuration)
        {
            _statusSubscription = Observable.Interval(TimeSpan.FromSeconds(3))
                .Subscribe(onNext:c =>
                {
                    uint t = 0;
                    POPNetCtrl.NetCtrl01_ReadConectState(_deviceHandle, ref t);
                    var res = t switch
                    {
                        0 => HardwareConnectionStatus.Connected,
                        1 => HardwareConnectionStatus.Disconnected,
                        2 => HardwareConnectionStatus.Error,
                        _ => HardwareConnectionStatus.Disconnected
                    };
                    _statusSubject.OnNext(res);
                }, onError: exception =>
                {
                    _statusSubject.OnNext(HardwareConnectionStatus.Disconnected);
                });
        }

        public override bool ConnectToHardware()
        {
            var result = POPNetCtrl.NetCtrl01_ConnectToDev(DeviceId, ref _deviceHandle);
            if (result == AddressContanst.OP_SUCCESSFUL)
            {
#if DEBUG
                Debug.WriteLine($"✓ 设备连接成功，句柄: 0x{DeviceId:X}");
#endif
                
                return true;
            }
            else
            {
#if DEBUG
                Debug.WriteLine($"✗ 设备连接失败，错误码: {result}");
#endif
                return false;
            }
        }

        public override bool DisconnectFromHardware()
        {
            if (_deviceHandle == IntPtr.Zero) return false;
            // 软件退出（关闭阀台，DA=0）
            POPNetCtrl.NetCtrl01_Soft_Ext(_deviceHandle);

            var result = POPNetCtrl.NetCtrl01_DisConnectToDev(_deviceHandle);
            if (result == AddressContanst.OP_SUCCESSFUL)
            {
#if DEBUG
                Debug.WriteLine("✓ 设备断开成功");
#endif
                _deviceHandle = IntPtr.Zero;
                return true;
            }
            else
            {
#if DEBUG
                Debug.WriteLine($"✗ 设备断开失败，错误码: {result}");
#endif
                return false;
            }
        }

        public IO
    }
}
