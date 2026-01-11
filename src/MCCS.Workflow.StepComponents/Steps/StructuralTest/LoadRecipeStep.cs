using System.IO;

using MCCS.Workflow.StepComponents.Attributes;
using MCCS.Workflow.StepComponents.Core;
using MCCS.Workflow.StepComponents.Models;
using MCCS.Workflow.StepComponents.Parameters;
using System.Text.Json;

using MCCS.Workflow.StepComponents.Enums;

namespace MCCS.Workflow.StepComponents.Steps.StructuralTest
{
    /// <summary>
    /// 加载试验配方步骤
    /// 入参：recipeId | recipeJson
    /// 出参：context.Recipe、RunId、DataFolder
    /// </summary>
    [StepComponent("load-recipe", "加载配方",
        Description = "加载试验配方，初始化运行上下文",
        Category = ComponentCategory.SafetyAndSetup,
        Icon = "FileDocumentOutline",
        Tags = ["配方", "加载", "初始化"])]
    public class LoadRecipeStep : BaseWorkflowStep
    {
        [StepInput("RecipeId")]
        public string? RecipeId { get; set; }

        [StepInput("RecipeJson")]
        public string? RecipeJson { get; set; }

        [StepInput("DataRootPath")]
        public string DataRootPath { get; set; } = string.Empty;

        protected override IEnumerable<IComponentParameter> DefineParameters()
        {
            yield return new StringParameter
            {
                Name = "RecipeId",
                DisplayName = "配方ID",
                Description = "要加载的配方ID，与RecipeJson二选一",
                IsRequired = false,
                Order = 1
            };

            yield return new MultilineTextParameter
            {
                Name = "RecipeJson",
                DisplayName = "配方JSON",
                Description = "配方的JSON内容，与RecipeId二选一",
                IsRequired = false,
                Rows = 10,
                Order = 2
            };

            yield return new StringParameter
            {
                Name = "DataRootPath",
                DisplayName = "数据根目录",
                Description = "试验数据存储的根目录",
                IsRequired = true,
                DefaultValue = "D:\\TestData",
                Order = 3
            };
        }

        protected override async Task<StepResult> ExecuteAsync(StepExecutionContext context)
        {
            var recipeId = GetParameter<string>("RecipeId");
            var recipeJson = GetParameter<string>("RecipeJson");
            var dataRootPath = GetParameter<string>("DataRootPath") ?? "D:\\TestData";

            TestRecipe? recipe = null;

            // 优先使用 RecipeJson
            if (!string.IsNullOrEmpty(recipeJson))
            {
                try
                {
                    recipe = JsonSerializer.Deserialize<TestRecipe>(recipeJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                catch (Exception ex)
                {
                    return StepResult.Fail($"解析配方JSON失败: {ex.Message}");
                }
            }
            else if (!string.IsNullOrEmpty(recipeId))
            {
                // TODO: 从站点服务加载配方
                // recipe = await _recipeService.GetRecipeByIdAsync(recipeId);
                return StepResult.Fail($"从ID加载配方功能待实现: {recipeId}");
            }

            if (recipe == null)
            {
                return StepResult.Fail("未能加载配方，请提供RecipeId或RecipeJson");
            }

            // 生成运行ID和数据目录
            var runId = Guid.NewGuid();
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var dataFolder = Path.Combine(dataRootPath, $"{recipe.Name}_{timestamp}");

            // 创建数据目录
            try
            {
                Directory.CreateDirectory(dataFolder);
            }
            catch (Exception ex)
            {
                return StepResult.Fail($"创建数据目录失败: {ex.Message}");
            }

            // 初始化试验运行上下文
            var testRunContext = new TestRunContext
            {
                RunId = runId,
                Recipe = recipe,
                DataFolder = dataFolder,
                StartTime = DateTime.Now
            };

            testRunContext.AddEvent("LoadRecipeStep", $"配方加载成功: {recipe.Name}");

            // 保存到工作流变量
            context.SetVariable("TestRunContext", testRunContext);
            context.SetVariable("Recipe", recipe);
            context.SetVariable("RunId", runId.ToString());
            context.SetVariable("DataFolder", dataFolder);

            await Task.CompletedTask;

            return StepResult.Succeed(new Dictionary<string, object?>
            {
                ["ResultCode"] = StepResultCode.OK,
                ["RunId"] = runId.ToString(),
                ["RecipeName"] = recipe.Name,
                ["TestType"] = recipe.TestType.ToString(),
                ["DataFolder"] = dataFolder
            });
        }
    }
}
