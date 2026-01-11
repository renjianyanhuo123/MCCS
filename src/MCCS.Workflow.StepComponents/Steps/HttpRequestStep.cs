using System.Net.Http;
using System.Text;
using MCCS.Workflow.StepComponents.Attributes;
using MCCS.Workflow.StepComponents.Core;
using MCCS.Workflow.StepComponents.Parameters;

namespace MCCS.Workflow.StepComponents.Steps
{
    /// <summary>
    /// HTTP请求步骤 - 发送HTTP请求
    /// </summary>
    [StepComponent("http-request", "HTTP请求",
        Description = "发送HTTP请求并获取响应",
        Category = ComponentCategory.Network,
        Icon = "Web",
        Tags = ["HTTP", "请求", "API", "网络", "REST"])]
    public class HttpRequestStep : BaseWorkflowStep
    {
        private static readonly HttpClient _httpClient = new()
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        [StepInput("Url")]
        public string Url { get; set; } = string.Empty;

        [StepInput("Method")]
        public string Method { get; set; } = "GET";

        [StepInput("Headers")]
        public List<KeyValueItem>? Headers { get; set; }

        [StepInput("Body")]
        public string? Body { get; set; }

        [StepInput("ContentType")]
        public string ContentType { get; set; } = "application/json";

        [StepInput("Timeout")]
        public int Timeout { get; set; } = 30;

        protected override IEnumerable<IComponentParameter> DefineParameters()
        {
            yield return new StringParameter
            {
                Name = "Url",
                DisplayName = "请求URL",
                Description = "HTTP请求的目标URL，支持变量替换",
                IsRequired = true,
                Placeholder = "https://api.example.com/data",
                Order = 1,
                Group = "基本设置"
            };

            yield return new SelectParameter
            {
                Name = "Method",
                DisplayName = "请求方法",
                Description = "HTTP请求方法",
                IsRequired = true,
                DefaultValue = "GET",
                Options = new List<SelectOption>
                {
                    new("GET", "GET"),
                    new("POST", "POST"),
                    new("PUT", "PUT"),
                    new("DELETE", "DELETE"),
                    new("PATCH", "PATCH")
                },
                Order = 2,
                Group = "基本设置"
            };

            yield return new KeyValueListParameter
            {
                Name = "Headers",
                DisplayName = "请求头",
                Description = "HTTP请求头",
                KeyLabel = "Header名称",
                ValueLabel = "Header值",
                Order = 3,
                Group = "请求头"
            };

            yield return new MultilineTextParameter
            {
                Name = "Body",
                DisplayName = "请求体",
                Description = "HTTP请求体内容（用于POST/PUT/PATCH请求）",
                Rows = 5,
                Placeholder = "{\"key\": \"value\"}",
                Order = 4,
                Group = "请求体"
            };

            yield return new SelectParameter
            {
                Name = "ContentType",
                DisplayName = "Content-Type",
                Description = "请求体的内容类型",
                DefaultValue = "application/json",
                Options = new List<SelectOption>
                {
                    new("application/json", "JSON"),
                    new("application/x-www-form-urlencoded", "表单"),
                    new("text/plain", "纯文本"),
                    new("application/xml", "XML")
                },
                Order = 5,
                Group = "请求体"
            };

            yield return new IntegerParameter
            {
                Name = "Timeout",
                DisplayName = "超时时间(秒)",
                Description = "请求超时时间",
                DefaultValue = 30,
                MinValue = 1,
                MaxValue = 300,
                Order = 6,
                Group = "高级设置"
            };
        }

        protected override async Task<StepResult> ExecuteAsync(StepExecutionContext context)
        {
            var url = GetParameter<string>("Url") ?? string.Empty;
            var method = GetParameter<string>("Method") ?? "GET";
            var headers = GetParameter<List<KeyValueItem>>("Headers");
            var body = GetParameter<string>("Body");
            var contentType = GetParameter<string>("ContentType") ?? "application/json";
            var timeout = GetParameter<int>("Timeout");

            // 替换URL中的变量
            url = context.ReplaceVariables(url);
            body = body != null ? context.ReplaceVariables(body) : null; 

            try
            {
                using var request = new HttpRequestMessage(new HttpMethod(method), url);

                // 添加请求头
                if (headers != null)
                {
                    foreach (var header in headers.Where(h => h.IsEnabled))
                    {
                        var headerValue = context.ReplaceVariables(header.Value);
                        request.Headers.TryAddWithoutValidation(header.Key, headerValue);
                    }
                }

                // 添加请求体
                if (!string.IsNullOrEmpty(body) && method is "POST" or "PUT" or "PATCH")
                {
                    request.Content = new StringContent(body, Encoding.UTF8, contentType);
                }

                using var cts = CancellationTokenSource.CreateLinkedTokenSource(context.CancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(timeout > 0 ? timeout : 30));

                var response = await _httpClient.SendAsync(request, cts.Token);
                var responseBody = await response.Content.ReadAsStringAsync(cts.Token); 

                var output = new Dictionary<string, object?>
                {
                    ["StatusCode"] = (int)response.StatusCode,
                    ["ReasonPhrase"] = response.ReasonPhrase,
                    ["Body"] = responseBody,
                    ["IsSuccess"] = response.IsSuccessStatusCode,
                    ["Headers"] = response.Headers.ToDictionary(h => h.Key, h => string.Join(", ", h.Value))
                };

                if (response.IsSuccessStatusCode)
                {
                    return StepResult.Succeed(output);
                }
                else
                {
                    return new StepResult
                    {
                        Success = false,
                        ErrorMessage = $"HTTP请求失败: {response.StatusCode} {response.ReasonPhrase}",
                        OutputData = output
                    };
                }
            }
            catch (TaskCanceledException)
            {
                return StepResult.Fail("请求超时");
            }
            catch (HttpRequestException ex)
            {
                return StepResult.Fail($"HTTP请求异常: {ex.Message}");
            }
        }
    }
}
