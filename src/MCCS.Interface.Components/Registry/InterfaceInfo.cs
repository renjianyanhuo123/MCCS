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
        /// 为组件设置参数视图名称
        /// </summary>
        public string SetParamViewName { get; set; } = string.Empty;
        /// <summary>
        /// 该组件是否可以设置参数
        /// </summary>
        public bool IsCanSetParam { get; set; }

        /// <summary>
        /// 视图模型类型（ViewModel）
        /// </summary>
        public Type? ViewModelType { get; set; }

        /// <summary>
        /// 参数类型（用于创建组件时传递的参数类型）
        /// </summary>
        public Type? ParameterType { get; set; }
    }
}
