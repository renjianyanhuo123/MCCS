using MCCS.Interface.Components.Enums;

namespace MCCS.Interface.Components.Registry
{
    public class InterfaceInfo
    {
        /// <summary>
        /// 界面组件唯一标识
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 界面组件名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 界面组件描述
        /// </summary>
        public string Description { get; set; } = string.Empty; 

        /// <summary>
        /// 界面组件分类
        /// </summary>
        public InterfaceComponentCategory Category { get; set; }

        /// <summary>
        /// 界面组件图标（Material Design Icon名称）
        /// </summary>
        public string Icon { get; set; } = "Cog";

        /// <summary>
        /// 界面组件版本
        /// </summary>
        public string Version { get; set; } = "1.0.0";

        /// <summary>
        /// 界面组件作者
        /// </summary>
        public string Author { get; set; } = string.Empty; 

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
        /// 参数类型定义
        /// </summary>
        public string? Parameters { get; set; }
    }
}
