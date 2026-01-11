using MCCS.Workflow.StepComponents.Attributes;
using MCCS.Workflow.StepComponents.Core;
using MCCS.Workflow.StepComponents.Models;
using MCCS.Workflow.StepComponents.Parameters;

namespace MCCS.Workflow.StepComponents.Steps.StructuralTest
{
    /// <summary>
    /// 设备连接步骤
    /// 入参：设备清单、通讯参数
    /// 出参：各设备连接状态、固件/版本信息
    /// </summary>
    [StepComponent("connect-devices", "设备连接",
        Description = "建立与控制器、作动器、液压站、DAQ等设备的通讯连接",
        Category = ComponentCategory.SafetyAndSetup,
        Icon = "LanConnect",
        Tags = ["设备", "连接", "通讯"])]
    public class ConnectDevicesStep : BaseWorkflowStep
    {
        [StepInput("ControllerAddress")]
        public string ControllerAddress { get; set; } = string.Empty;

        [StepInput("DaqAddress")]
        public string DaqAddress { get; set; } = string.Empty;

        [StepInput("TimeoutSeconds")]
        public int TimeoutSeconds { get; set; } = 30;

        protected override IEnumerable<IComponentParameter> DefineParameters()
        {
            yield return new StringParameter
            {
                Name = "ControllerAddress",
                DisplayName = "控制器地址",
                Description = "控制器的IP地址或通讯端口",
                IsRequired = true,
                Placeholder = "192.168.1.100",
                Order = 1
            };

            yield return new StringParameter
            {
                Name = "DaqAddress",
                DisplayName = "采集器地址",
                Description = "数据采集器的IP地址或通讯端口",
                IsRequired = false,
                Placeholder = "192.168.1.101",
                Order = 2
            };

            yield return new IntegerParameter
            {
                Name = "TimeoutSeconds",
                DisplayName = "超时时间(秒)",
                Description = "设备连接超时时间",
                IsRequired = true,
                DefaultValue = 30,
                MinValue = 5,
                MaxValue = 120,
                Order = 3
            };
        }

        protected override async Task<StepResult> ExecuteAsync(StepExecutionContext context)
        {
            var controllerAddress = GetParameter<string>("ControllerAddress");
            var daqAddress = GetParameter<string>("DaqAddress");
            var timeout = GetParameter<int>("TimeoutSeconds");

            var testRunContext = context.GetVariable<TestRunContext>("TestRunContext");
            if (testRunContext == null)
            {
                return StepResult.Fail("未找到试验运行上下文，请先执行LoadRecipeStep");
            }

            var deviceConnections = new Dictionary<string, bool>();
            var deviceVersions = new Dictionary<string, string>();
            var failedDevices = new List<string>();

            // TODO: 向站点服务发送设备连接指令
            // 控制器连接
            if (!string.IsNullOrEmpty(controllerAddress))
            {
                // TODO: var result = await _stationService.ConnectControllerAsync(controllerAddress, timeout);
                // 模拟连接成功
                deviceConnections["Controller"] = true;
                deviceVersions["Controller"] = "v1.0.0"; // TODO: 从实际连接获取
                testRunContext.AddEvent("ConnectDevicesStep", $"控制器连接成功: {controllerAddress}");
            }

            // DAQ连接
            if (!string.IsNullOrEmpty(daqAddress))
            {
                // TODO: var result = await _stationService.ConnectDaqAsync(daqAddress, timeout);
                deviceConnections["DAQ"] = true;
                deviceVersions["DAQ"] = "v2.1.0"; // TODO: 从实际连接获取
                testRunContext.AddEvent("ConnectDevicesStep", $"采集器连接成功: {daqAddress}");
            }

            // 更新设备状态
            testRunContext.Machine.DeviceConnections = deviceConnections;
            testRunContext.Machine.DeviceVersions = deviceVersions;

            await Task.CompletedTask;

            if (failedDevices.Count > 0)
            {
                return StepResult.Fail($"设备连接失败: {string.Join(", ", failedDevices)}");
            }

            return StepResult.Succeed(new Dictionary<string, object?>
            {
                ["ResultCode"] = StepResultCode.OK,
                ["DeviceConnections"] = deviceConnections,
                ["DeviceVersions"] = deviceVersions
            });
        }
    }
}
