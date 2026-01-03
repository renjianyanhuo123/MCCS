using MCCS.Workflow.StepComponents.Core;
using MCCS.Workflow.StepComponents.Parameters;

namespace MCCS.Workflow.StepComponents.Registry
{
    /// <summary>
    /// 步骤信息 - 用于UI展示和步骤选择
    /// </summary>
    public class StepInfo
    {
        /// <summary>
        /// 步骤唯一标识
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 步骤名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 步骤描述
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 步骤分类
        /// </summary>
        public ComponentCategory Category { get; set; }

        /// <summary>
        /// 步骤图标（Material Design Icon名称）
        /// </summary>
        public string Icon { get; set; } = "Cog";

        /// <summary>
        /// 步骤版本
        /// </summary>
        public string Version { get; set; } = "1.0.0";

        /// <summary>
        /// 作者
        /// </summary>
        public string Author { get; set; } = string.Empty;

        /// <summary>
        /// 标签
        /// </summary>
        public string[] Tags { get; set; } = Array.Empty<string>();

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 排序顺序
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// 步骤类型
        /// </summary>
        public Type StepType { get; set; } = null!;

        /// <summary>
        /// 参数定义列表
        /// </summary>
        public IReadOnlyList<IComponentParameter>? ParameterDefinitions { get; set; }
    }
}
