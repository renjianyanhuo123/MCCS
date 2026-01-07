using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;

using MCCS.Station.Core.ControllerManagers.Entities;
using MCCS.Station.Core.DllNative.Models;

namespace MCCS.Station.Core.HardwareDevices
{
    /// <summary>
    /// Rx.NET 数据流扩展方法，提供多源组合、数据选择和性能优化能力
    /// </summary>
    public static class DataStreamExtensions
    {
        #region 多源组合 - 用于图像绘制等需要同步多个数据源的场景

        /// <summary>
        /// 组合多个控制器的数据流，返回最新的数据组合
        /// 适用于需要同步多个控制器数据进行图像绘制的场景
        /// </summary>
        /// <param name="controllers">控制器集合</param>
        /// <returns>组合后的数据流，包含所有控制器的最新数据</returns>
        public static IObservable<SampleBatch<TNet_ADHInfo>[]> CombineDataStreams(
            this IEnumerable<IController> controllers)
        {
            var controllerList = controllers.ToList();
            if (controllerList.Count == 0)
                return Observable.Empty<SampleBatch<TNet_ADHInfo>[]>();

            if (controllerList.Count == 1)
                return controllerList[0].DataStream.Select(x => new[] { x });

            // 使用 CombineLatest 同步多个数据源的最新数据
            return controllerList
                .Select(c => c.DataStream)
                .CombineLatest();
        }

        /// <summary>
        /// 按时间窗口同步多个控制器的数据流
        /// </summary>
        /// <param name="controllers">控制器集合</param>
        /// <param name="windowDuration">时间窗口</param>
        /// <returns>在时间窗口内收集的数据</returns>
        public static IObservable<IList<SampleBatch<TNet_ADHInfo>[]>> CombineDataStreamsWithWindow(
            this IEnumerable<IController> controllers,
            TimeSpan windowDuration)
        {
            return controllers.CombineDataStreams()
                .Buffer(windowDuration);
        }

        /// <summary>
        /// 组合两个控制器的数据流（优化版本，避免数组创建）
        /// </summary>
        public static IObservable<(SampleBatch<TNet_ADHInfo> First, SampleBatch<TNet_ADHInfo> Second)> CombineTwo(
            this IController first,
            IController second)
        {
            return first.DataStream.CombineLatest(
                second.DataStream,
                (a, b) => (a, b));
        }

        /// <summary>
        /// 组合三个控制器的数据流
        /// </summary>
        public static IObservable<(SampleBatch<TNet_ADHInfo> First, SampleBatch<TNet_ADHInfo> Second, SampleBatch<TNet_ADHInfo> Third)> CombineThree(
            this IController first,
            IController second,
            IController third)
        {
            return Observable.CombineLatest(
                first.DataStream,
                second.DataStream,
                third.DataStream,
                (a, b, c) => (a, b, c));
        }

        #endregion

        #region 数据选择器 - 提取特定通道数据

        /// <summary>
        /// 从批量数据中提取试验力通道（Net_FeedLoadN）
        /// </summary>
        public static IObservable<float[]> SelectForce(this IObservable<SampleBatch<TNet_ADHInfo>> source)
        {
            return source.Select(batch =>
            {
                var forces = new float[batch.SampleCount];
                for (var i = 0; i < batch.SampleCount; i++)
                {
                    forces[i] = batch.Values[i].Net_FeedLoadN;
                }
                return forces;
            });
        }

        /// <summary>
        /// 从批量数据中提取位移通道（Net_AD_S[0] - SSI通道0）
        /// </summary>
        public static IObservable<float[]> SelectPosition(this IObservable<SampleBatch<TNet_ADHInfo>> source)
        {
            return source.Select(batch =>
            {
                var positions = new float[batch.SampleCount];
                for (var i = 0; i < batch.SampleCount; i++)
                {
                    positions[i] = batch.Values[i].Net_AD_S[0];
                }
                return positions;
            });
        }

        /// <summary>
        /// 从批量数据中提取指定的 AD 通道数据
        /// </summary>
        /// <param name="source">数据源</param>
        /// <param name="channelIndex">通道索引 (0-5)</param>
        public static IObservable<float[]> SelectAdChannel(
            this IObservable<SampleBatch<TNet_ADHInfo>> source,
            int channelIndex)
        {
            if (channelIndex is < 0 or > 5)
                throw new ArgumentOutOfRangeException(nameof(channelIndex), "通道索引必须在 0-5 之间");

            return source.Select(batch =>
            {
                var values = new float[batch.SampleCount];
                for (var i = 0; i < batch.SampleCount; i++)
                {
                    values[i] = batch.Values[i].Net_AD_N[channelIndex];
                }
                return values;
            });
        }

        /// <summary>
        /// 从批量数据中提取指定的 SSI 通道数据
        /// </summary>
        /// <param name="source">数据源</param>
        /// <param name="channelIndex">通道索引 (0-1)</param>
        public static IObservable<float[]> SelectSsiChannel(
            this IObservable<SampleBatch<TNet_ADHInfo>> source,
            int channelIndex)
        {
            if (channelIndex is < 0 or > 1)
                throw new ArgumentOutOfRangeException(nameof(channelIndex), "通道索引必须在 0-1 之间");

            return source.Select(batch =>
            {
                var values = new float[batch.SampleCount];
                for (var i = 0; i < batch.SampleCount; i++)
                {
                    values[i] = batch.Values[i].Net_AD_S[channelIndex];
                }
                return values;
            });
        }

        /// <summary>
        /// 自定义数据选择器，提取多个通道数据用于图像绘制
        /// </summary>
        /// <param name="source">数据源</param>
        /// <param name="selector">选择器函数</param>
        public static IObservable<TResult[]> SelectChannel<TResult>(
            this IObservable<SampleBatch<TNet_ADHInfo>> source,
            Func<TNet_ADHInfo, TResult> selector)
        {
            return source.Select(batch =>
            {
                var values = new TResult[batch.SampleCount];
                for (var i = 0; i < batch.SampleCount; i++)
                {
                    values[i] = selector(batch.Values[i]);
                }
                return values;
            });
        }

        /// <summary>
        /// 提取力-位移对数据，用于绘制应力-应变曲线
        /// </summary>
        public static IObservable<(float Force, float Position)[]> SelectForcePosition(
            this IObservable<SampleBatch<TNet_ADHInfo>> source)
        {
            return source.Select(batch =>
            {
                var pairs = new (float Force, float Position)[batch.SampleCount];
                for (var i = 0; i < batch.SampleCount; i++)
                {
                    pairs[i] = (batch.Values[i].Net_FeedLoadN, batch.Values[i].Net_AD_S[0]);
                }
                return pairs;
            });
        }

        #endregion

        #region 性能优化操作符

        /// <summary>
        /// 降采样：每 N 个批次取一个
        /// </summary>
        public static IObservable<SampleBatch<TNet_ADHInfo>> Downsample(
            this IObservable<SampleBatch<TNet_ADHInfo>> source,
            int factor)
        {
            return source.Where((_, index) => index % factor == 0);
        }

        /// <summary>
        /// 按时间降采样：在指定时间间隔内只取最新的一个
        /// </summary>
        public static IObservable<SampleBatch<TNet_ADHInfo>> SampleByTime(
            this IObservable<SampleBatch<TNet_ADHInfo>> source,
            TimeSpan interval)
        {
            return source.Sample(interval);
        }

        /// <summary>
        /// 批量缓冲：将多个批次合并为一个大批次
        /// </summary>
        public static IObservable<SampleBatch<TNet_ADHInfo>> BufferBatches(
            this IObservable<SampleBatch<TNet_ADHInfo>> source,
            int batchCount)
        {
            return source.Buffer(batchCount)
                .Where(batches => batches.Count > 0)
                .Select(batches =>
                {
                    var totalCount = batches.Sum(b => b.SampleCount);
                    var values = new TNet_ADHInfo[totalCount];
                    var offset = 0;

                    foreach (var batch in batches)
                    {
                        Array.Copy(batch.Values, 0, values, offset, batch.SampleCount);
                        offset += (int)batch.SampleCount;
                    }

                    return new SampleBatch<TNet_ADHInfo>
                    {
                        DeviceId = batches[0].DeviceId,
                        SequenceStart = batches[0].SequenceStart,
                        SampleCount = totalCount,
                        Values = values
                    };
                });
        }

        /// <summary>
        /// 时间窗口缓冲：按时间窗口合并批次
        /// </summary>
        public static IObservable<SampleBatch<TNet_ADHInfo>> BufferByTime(
            this IObservable<SampleBatch<TNet_ADHInfo>> source,
            TimeSpan windowDuration)
        {
            return source.Buffer(windowDuration)
                .Where(batches => batches.Count > 0)
                .Select(batches =>
                {
                    var totalCount = batches.Sum(b => b.SampleCount);
                    var values = new TNet_ADHInfo[totalCount];
                    var offset = 0;

                    foreach (var batch in batches)
                    {
                        Array.Copy(batch.Values, 0, values, offset, batch.SampleCount);
                        offset += (int)batch.SampleCount;
                    }

                    return new SampleBatch<TNet_ADHInfo>
                    {
                        DeviceId = batches[0].DeviceId,
                        SequenceStart = batches[0].SequenceStart,
                        SampleCount = totalCount,
                        Values = values
                    };
                });
        }

        #endregion

        #region 背压处理

        /// <summary>
        /// 当下游处理较慢时，丢弃中间数据，只保留最新的
        /// 适用于实时显示场景，可以容忍数据丢失
        /// </summary>
        public static IObservable<SampleBatch<TNet_ADHInfo>> DropIfBusy(
            this IObservable<SampleBatch<TNet_ADHInfo>> source)
        {
            var subject = new BehaviorSubject<SampleBatch<TNet_ADHInfo>?>(null);
            source.Subscribe(subject);
            return subject.Where(x => x != null).Select(x => x!);
        }

        /// <summary>
        /// 使用特定调度器处理数据，避免阻塞源
        /// </summary>
        public static IObservable<SampleBatch<TNet_ADHInfo>> ObserveOnPoolThread(
            this IObservable<SampleBatch<TNet_ADHInfo>> source)
        {
            return source.ObserveOn(ThreadPoolScheduler.Instance);
        }

        #endregion

        #region 数据展开

        /// <summary>
        /// 将批量数据展开为单个数据点流
        /// 注意：这会增加开销，仅在需要逐点处理时使用
        /// </summary>
        public static IObservable<RawSample<TNet_ADHInfo>> Expand(
            this IObservable<SampleBatch<TNet_ADHInfo>> source)
        {
            return source.SelectMany(batch =>
            {
                var samples = new RawSample<TNet_ADHInfo>[batch.SampleCount];
                for (var i = 0; i < batch.SampleCount; i++)
                {
                    samples[i] = new RawSample<TNet_ADHInfo>
                    {
                        DeviceId = batch.DeviceId,
                        SequenceIndex = batch.SequenceStart + i,
                        Value = batch.Values[i]
                    };
                }
                return samples;
            });
        }

        #endregion

        #region 状态流扩展

        /// <summary>
        /// 组合多个控制器的状态流
        /// </summary>
        public static IObservable<HardwareConnectionStatus[]> CombineStatusStreams(
            this IEnumerable<IController> controllers)
        {
            var controllerList = controllers.ToList();
            if (controllerList.Count == 0)
                return Observable.Empty<HardwareConnectionStatus[]>();

            return controllerList
                .Select(c => c.StatusStream)
                .CombineLatest();
        }

        /// <summary>
        /// 检测是否所有控制器都已连接
        /// </summary>
        public static IObservable<bool> AllConnected(
            this IEnumerable<IController> controllers)
        {
            return controllers.CombineStatusStreams()
                .Select(statuses => statuses.All(s => s == HardwareConnectionStatus.Connected));
        }

        /// <summary>
        /// 检测是否有任意控制器断开
        /// </summary>
        public static IObservable<bool> AnyDisconnected(
            this IEnumerable<IController> controllers)
        {
            return controllers.CombineStatusStreams()
                .Select(statuses => statuses.Any(s => s != HardwareConnectionStatus.Connected));
        }

        #endregion
    }
}
