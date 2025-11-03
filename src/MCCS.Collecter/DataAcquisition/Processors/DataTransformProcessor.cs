using System.Collections.Concurrent;
using System.Reactive.Linq;
using MCCS.Infrastructure.TestModels;
using MCCS.Infrastructure.TestModels.Commands;
using MCCS.Infrastructure.TestModels.DataAcquisition;

namespace MCCS.Collecter.DataAcquisition.Processors;

/// <summary>
/// 数据转换处理器
/// 将原始硬件数据转换为应用层数据模型
/// </summary>
public class DataTransformProcessor : IDataProcessor
{
    private readonly ConcurrentDictionary<long, HardwareSignalChannel> _signalChannels;

    public string Name => "Transform";

    public DataTransformProcessor(ConcurrentDictionary<long, HardwareSignalChannel> signalChannels)
    {
        _signalChannels = signalChannels ?? throw new ArgumentNullException(nameof(signalChannels));
    }

    public IObservable<ProcessedData> Process(IObservable<RawHardwareData> source)
    {
        return source.Select(raw =>
        {
            var batchModel = ConvertToCollectModel(raw);

            return new ProcessedData
            {
                Raw = raw,
                CollectModel = batchModel,
                Timestamp = raw.Timestamp,
                Quality = DataQuality.Good
            };
        });
    }

    private BatchCollectItemModel ConvertToCollectModel(RawHardwareData raw)
    {
        var result = new BatchCollectItemModel
        {
            Net_PosVref = raw.Net_PosVref,
            Net_PosE = raw.Net_PosE,
            Net_CtrlDA = raw.Net_CtrlDA,
            Net_CycleCount = raw.Net_CycleCount,
            Net_SysState = raw.Net_SysState,
            Net_DIVal = raw.Net_DIVal,
            Net_DOVal = raw.Net_DOVal,
            Net_D_PosVref = raw.Net_D_PosVref,
            Net_FeedLoadN = raw.Net_FeedLoadN,
            Net_PrtErrState = raw.Net_PrtErrState,
            Net_TimeCnt = raw.Net_TimeCnt
        };

        // 映射信号通道数据
        foreach (var signal in _signalChannels.Values)
        {
            var index = signal.SignalAddressIndex;

            if (index < 10 && index < raw.Net_AD_N.Length)
            {
                // AI 通道 (0-9)
                result.Net_AD_N.Add(signal.SignalId, raw.Net_AD_N[index]);
            }
            else
            {
                // SSI 通道 (10+)
                var sIndex = index % 10;
                if (sIndex < raw.Net_AD_S.Length)
                {
                    result.Net_AD_S.Add(signal.SignalId, raw.Net_AD_S[sIndex]);
                }
            }
        }

        return result;
    }
}
