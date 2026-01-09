using System.Collections.Concurrent;
using System.Reactive.Linq;

using MCCS.Infrastructure.Models.ProjectManager;
using MCCS.Infrastructure.TestModels.Commands;
using MCCS.Station.Core.ControllerManagers;
using MCCS.Station.Core.HardwareDevices;
using MCCS.Station.Core.SignalManagers.Signals;

namespace MCCS.Station.Core.SignalManagers
{
    public sealed class SignalManager : ISignalManager
    {
        private readonly IControllerManager _controllerManager;
        private readonly List<HardwareSignalChannel> _signals = [];
        // 缓存信号数据流，避免重复创建Observable
        private readonly ConcurrentDictionary<long, IObservable<DataPoint<float>>> _signalStreamCache = new();

        public SignalManager(IControllerManager controllerManager)
        {
            _controllerManager = controllerManager;
        }

        public void Initialization(IEnumerable<HardwareSignalConfiguration> signalConfigurations)
        {
            foreach (var configuration in signalConfigurations)
            {
                _signals.Add(new HardwareSignalChannel(configuration));
            }
        }

        #region 采集信号
        /// <summary>
        /// 获取单个信号的数据流
        /// 使用缓存确保相同signalId返回相同的Observable实例
        /// </summary>
        public IObservable<DataPoint<float>> GetSignalDataStream(long signalId)
        {
            return _signalStreamCache.GetOrAdd(signalId, id =>
            {
                var signChannel = _signals.FirstOrDefault(s => s.SignalId == id);
                if (signChannel == null)
                    throw new ArgumentException($"Cannot find signal with ID: {id}", nameof(signalId)); 
                var controller = _controllerManager.GetControllerInfo(signChannel.BelongControllerId);
                return controller.IndividualDataStream
                    .Select(info => new DataPoint<float>
                    { 
                        DeviceId = info.DeviceId,
                        Unit = signChannel.Configuration.Unit ?? "",
                        Timestamp = info.Timestamp,
                        Value = signChannel.SignalAddressIndex < 10
                            ? info.Value.Net_AD_N[signChannel.SignalAddressIndex]
                            : info.Value.Net_AD_S[signChannel.SignalAddressIndex % 10]
                    })
                    .Publish()
                    .RefCount();
            });
        }

        public IObservable<List<ProjectDataRecordModel>> GetProjectDataRecords()
        {
            // TODO：只能取第一个控制器了，后面肯定要根据实际情况修改的
            var controller = _controllerManager.GetControllers().First();
            return controller.DataStream.Select(batch =>
            {
                var res = new List<ProjectDataRecordModel>();
                foreach (var data in batch.Values)
                {
                    var recordData = new ProjectDataRecordModel();
                    // 一次性采集所有的信号
                    foreach (var signal in _signals)
                    {
                        recordData.SignalItems.Add(new ProjectSignalItemModel
                        {
                            RecordId = recordData.RecordId,
                            SignalKey = "",
                            SignalId = signal.SignalId,
                            Unit = signal.Configuration.Unit,
                            Value = signal.SignalAddressIndex < 10 ? data.Net_AD_N[signal.SignalAddressIndex] : data.Net_AD_S[signal.SignalAddressIndex % 10]
                        });
                    }
                    res.Add(recordData);
                }
                return res;
            });
        }

        public DeviceCommandContext SetSignalTare(long signalId)
        {
            var context = new DeviceCommandContext
            {
                IsValid = true
            };

            var signalChannel = _signals.FirstOrDefault(c => c.SignalId == signalId);
            if (signalChannel == null) throw new ArgumentNullException("signalId is null");
            var controller = _controllerManager.GetControllerInfo(signalChannel.BelongControllerId);
            // TODO: 为了兼容旧的模式;因为目前直接传入的位移或力，正确的方案应该是传入SignalID
            var controlType = signalChannel.Configuration.Unit == "mm" ? 0 : 1;
            controller.SetSignalTare(controlType);
            return context;
        }

        #endregion

    }
}
