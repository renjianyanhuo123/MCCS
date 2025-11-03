using System.Diagnostics;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using MCCS.Collecter.DllNative;
using MCCS.Collecter.DllNative.Models;
using MCCS.Collecter.Memory;
using MCCS.Infrastructure.TestModels;
using MCCS.Infrastructure.TestModels.DataAcquisition;

namespace MCCS.Collecter.HardwareAdapters;

/// <summary>
/// POPNetCtrl 硬件适配器（真实硬件）
/// </summary>
public class POPNetHardwareAdapter : IHardwareAdapter
{
    private readonly int _deviceAddressId;
    private readonly BehaviorSubject<HardwareConnectionStatus> _statusSubject;
    private IntPtr _deviceHandle = IntPtr.Zero;
    private readonly IMemoryPool _memoryPool;
    private readonly int _structSize;
    private bool _disposed = false;

    public long DeviceId { get; }
    public string DeviceName { get; }
    public HardwareConnectionStatus Status => _statusSubject.Value;
    public IObservable<HardwareConnectionStatus> StatusStream => _statusSubject.AsObservable();

    public POPNetHardwareAdapter(
        long deviceId,
        string deviceName,
        int deviceAddressId,
        IMemoryPool memoryPool)
    {
        DeviceId = deviceId;
        DeviceName = deviceName;
        _deviceAddressId = deviceAddressId;
        _memoryPool = memoryPool ?? throw new ArgumentNullException(nameof(memoryPool));
        _statusSubject = new BehaviorSubject<HardwareConnectionStatus>(HardwareConnectionStatus.Disconnected);
        _structSize = Marshal.SizeOf(typeof(TNet_ADHInfo));
    }

    public async Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            var result = POPNetCtrl.NetCtrl01_ConnectToDev(_deviceAddressId, ref _deviceHandle);

            if (result == AddressContanst.OP_SUCCESSFUL)
            {
                _statusSubject.OnNext(HardwareConnectionStatus.Connected);
#if DEBUG
                Debug.WriteLine($"✓ 设备 {DeviceName} 连接成功，句柄: 0x{_deviceHandle:X}");
#endif
                return true;
            }

            _statusSubject.OnNext(HardwareConnectionStatus.Error);
#if DEBUG
            Debug.WriteLine($"✗ 设备 {DeviceName} 连接失败，错误码: {result}");
#endif
            return false;
        }, cancellationToken);
    }

    public async Task<bool> DisconnectAsync(CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            if (_deviceHandle == IntPtr.Zero) return false;

            // 软件退出 (关闭阀台, DA=0)
            POPNetCtrl.NetCtrl01_Soft_Ext(_deviceHandle);

            var result = POPNetCtrl.NetCtrl01_DisConnectToDev(_deviceHandle);

            if (result == AddressContanst.OP_SUCCESSFUL)
            {
                _deviceHandle = IntPtr.Zero;
                _statusSubject.OnNext(HardwareConnectionStatus.Disconnected);
#if DEBUG
                Debug.WriteLine($"✓ 设备 {DeviceName} 断开成功");
#endif
                return true;
            }

#if DEBUG
            Debug.WriteLine($"✗ 设备 {DeviceName} 断开失败，错误码: {result}");
#endif
            return false;
        }, cancellationToken);
    }

    public RawHardwareData ReadData()
    {
        uint count = 0;

        // 检查是否有数据可读
        if (POPNetCtrl.NetCtrl01_GetAD_HDataCount(_deviceHandle, ref count) != AddressContanst.OP_SUCCESSFUL || count == 0)
        {
            return CreateEmptyData();
        }

        // 使用 RAII 模式从内存池租用缓冲区
        using var buffer = _memoryPool.Rent(_structSize);

        // 读取最后一个数据点（最新的）
        for (uint i = 0; i < count; i++)
        {
            if (POPNetCtrl.NetCtrl01_GetAD_HInfo(_deviceHandle, buffer.Pointer, (uint)buffer.Size) != AddressContanst.OP_SUCCESSFUL)
            {
                return CreateEmptyData();
            }

            // 只处理最后一个数据
            if (i == count - 1)
            {
                var nativeData = Marshal.PtrToStructure<TNet_ADHInfo>(buffer.Pointer);
                return ConvertToRawData(nativeData);
            }
        }

        return CreateEmptyData();
    }

    public async Task<RawHardwareData> ReadDataAsync(CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => ReadData(), cancellationToken);
    }

    public async Task<bool> SetControlModeAsync(SystemControlState controlMode, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            var result = POPNetCtrl.NetCtrl01_Set_SysCtrlstate(_deviceHandle, (byte)controlMode);
            return result == AddressContanst.OP_SUCCESSFUL;
        }, cancellationToken);
    }

    public async Task<bool> SetValveStateAsync(bool isOpen, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            var v = isOpen ? 1u : 0u;
            var result = POPNetCtrl.NetCtrl01_Set_StationCtrl(_deviceHandle, v, 0);
            return result == AddressContanst.OP_SUCCESSFUL;
        }, cancellationToken);
    }

    public async Task<bool> EmergencyStopAsync(CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            var result = POPNetCtrl.NetCtrl01_Set_TestStartState(_deviceHandle, 0); // 停止测试
            return result == AddressContanst.OP_SUCCESSFUL;
        }, cancellationToken);
    }

    private RawHardwareData CreateEmptyData()
    {
        return new RawHardwareData
        {
            Timestamp = Stopwatch.GetTimestamp()
        };
    }

    private RawHardwareData ConvertToRawData(TNet_ADHInfo nativeData)
    {
        return new RawHardwareData
        {
            Timestamp = Stopwatch.GetTimestamp(),
            Net_AD_N = nativeData.Net_AD_N,
            Net_AD_S = nativeData.Net_AD_S,
            Net_PosVref = nativeData.Net_PosVref,
            Net_PosE = nativeData.Net_PosE,
            Net_CtrlDA = nativeData.Net_CtrlDA,
            Net_CycleCount = nativeData.Net_CycleCount,
            Net_SysState = nativeData.Net_SysState,
            Net_FeedLoadN = nativeData.Net_FeedLoadN,
            Net_DIVal = nativeData.Net_DIVal,
            Net_DOVal = nativeData.Net_DOVal,
            Net_D_PosVref = nativeData.Net_D_PosVref,
            Net_PrtErrState = nativeData.Net_PrtErrState,
            Net_TimeCnt = nativeData.Net_TimeCnt
        };
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_deviceHandle != IntPtr.Zero)
            {
                DisconnectAsync().Wait();
            }

            _statusSubject?.OnCompleted();
            _statusSubject?.Dispose();
            _disposed = true;
        }
    }
}
