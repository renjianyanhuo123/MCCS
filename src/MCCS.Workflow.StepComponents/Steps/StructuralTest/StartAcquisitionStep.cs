using System.IO;

using MCCS.Workflow.StepComponents.Attributes;
using MCCS.Workflow.StepComponents.Core;
using MCCS.Workflow.StepComponents.Enums;
using MCCS.Workflow.StepComponents.Models;
using MCCS.Workflow.StepComponents.Parameters;

namespace MCCS.Workflow.StepComponents.Steps.StructuralTest
{
    /// <summary>
    /// 开始数据采集步骤
    /// 入参：采样率、通道、文件格式、分段策略
    /// 出参：数据文件句柄/路径
    /// </summary>
    [StepComponent("start-acquisition", "开始采集",
        Description = "启动数据采集系统",
        Category = ComponentCategory.DataAndReport,
        Icon = "RecordCircle",
        Tags = ["采集", "数据", "开始", "记录"])]
    public class StartAcquisitionStep : BaseWorkflowStep
    {
        [StepInput("SampleRate")]
        public double SampleRate { get; set; } = 1000;

        [StepInput("Channels")]
        public string Channels { get; set; } = string.Empty;

        [StepInput("DataFormat")]
        public string DataFormat { get; set; } = "CSV";

        [StepInput("SegmentedStorage")]
        public bool SegmentedStorage { get; set; } = false;

        [StepInput("MaxRecordsPerSegment")]
        public int MaxRecordsPerSegment { get; set; } = 100000;

        protected override IEnumerable<IComponentParameter> DefineParameters()
        {
            yield return new DoubleParameter
            {
                Name = "SampleRate",
                DisplayName = "采样率(Hz)",
                Description = "数据采样频率",
                IsRequired = true,
                DefaultValue = 1000,
                MinValue = 1,
                MaxValue = 100000,
                Order = 1
            };

            yield return new StringParameter
            {
                Name = "Channels",
                DisplayName = "采集通道",
                Description = "要采集的通道ID，逗号分隔，留空表示全部",
                IsRequired = false,
                Placeholder = "Force,Displacement,Strain",
                Order = 2
            };

            yield return new SelectParameter
            {
                Name = "DataFormat",
                DisplayName = "数据格式",
                Description = "数据存储格式",
                IsRequired = true,
                DefaultValue = "CSV",
                Options =
                [
                    new SelectOption("CSV", "CSV 文本"),
                    new SelectOption("Binary", "二进制"),
                    new SelectOption("MDF", "MDF格式"),
                    new SelectOption("TDMS", "TDMS格式")
                ],
                Order = 3
            };

            yield return new BooleanParameter
            {
                Name = "SegmentedStorage",
                DisplayName = "分段存储",
                Description = "是否将数据分段存储到多个文件",
                DefaultValue = false,
                Order = 4
            };

            yield return new IntegerParameter
            {
                Name = "MaxRecordsPerSegment",
                DisplayName = "每段最大记录数",
                Description = "分段存储时每个文件的最大记录数",
                IsRequired = true,
                DefaultValue = 100000,
                MinValue = 1000,
                Order = 5
            };
        }

        protected override async Task<StepResult> ExecuteAsync(StepExecutionContext context)
        {
            var sampleRate = GetParameter<double>("SampleRate");
            var channelsStr = GetParameter<string>("Channels");
            var dataFormat = GetParameter<string>("DataFormat") ?? "CSV";
            var segmentedStorage = GetParameter<bool>("SegmentedStorage");
            var maxRecordsPerSegment = GetParameter<int>("MaxRecordsPerSegment");

            var testRunContext = context.GetVariable<TestRunContext>("TestRunContext");
            if (testRunContext == null)
            {
                return StepResult.Fail("未找到试验运行上下文");
            }

            // 解析通道列表
            List<string> channels;
            if (string.IsNullOrEmpty(channelsStr))
            {
                channels = new List<string> { "Force", "Displacement", "Strain", "Time" };
            }
            else
            {
                channels = channelsStr.Split(',')
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList();
            }

            // 生成数据文件路径
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var extension = dataFormat.ToLower() switch
            {
                "csv" => ".csv",
                "binary" => ".bin",
                "mdf" => ".mdf",
                "tdms" => ".tdms",
                _ => ".dat"
            };
            var dataFilePath = Path.Combine(testRunContext.DataFolder, $"TestData_{timestamp}{extension}");

            // TODO: 向站点服务发送开始采集指令
            // var acquisitionConfig = new AcquisitionConfig
            // {
            //     SampleRate = sampleRate,
            //     Channels = channels,
            //     DataFormat = dataFormat,
            //     OutputPath = dataFilePath,
            //     SegmentedStorage = segmentedStorage,
            //     MaxRecordsPerSegment = maxRecordsPerSegment
            // };
            // var acquisitionId = await _stationService.StartAcquisitionAsync(acquisitionConfig);

            var acquisitionId = Guid.NewGuid().ToString();

            // 记录采集结果
            var acquisitionResult = new AcquisitionResult
            {
                Success = true,
                DataFiles = new List<string> { dataFilePath },
                StartTime = DateTime.Now
            };

            testRunContext.SetStepOutput("AcquisitionResult", acquisitionResult);
            testRunContext.AddEvent("StartAcquisitionStep",
                $"数据采集已启动: 采样率 {sampleRate} Hz, 通道数 {channels.Count}, 格式 {dataFormat}");

            context.SetVariable("AcquisitionId", acquisitionId);
            context.SetVariable("AcquisitionDataPath", dataFilePath);

            await Task.CompletedTask;

            return StepResult.Succeed(new Dictionary<string, object?>
            {
                ["ResultCode"] = StepResultCode.OK,
                ["AcquisitionId"] = acquisitionId,
                ["DataFilePath"] = dataFilePath,
                ["SampleRate"] = sampleRate,
                ["Channels"] = channels,
                ["DataFormat"] = dataFormat
            });
        }
    }
}
