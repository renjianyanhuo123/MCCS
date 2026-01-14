using MCCS.Interface.Components.Enums;

namespace MCCS.Interface.Components.Attributes
{
    /// <summary>
    /// 界面组件特性，用于标记一个类作为界面组件
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class InterfaceComponentAttribute : Attribute
    {
        public InterfaceComponentAttribute(string id, string name, InterfaceComponentCategory category)
        {
            Id = id;
            Name = name;
            Category = category;
        }

        /// <summary>
        /// 组件唯一标识
        /// </summary>
        public string Id { get; } 
        /// <summary>
        /// 组件名称
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 组件种类
        /// </summary>
        public InterfaceComponentCategory Category { get; set; } 
        /// <summary>
        /// 组件描述
        /// </summary>
        public string Description { get; set; } = string.Empty;  
        /// <summary>
        /// 组件图标（Material Design Icon名称）
        /// </summary>
        public string Icon { get; set; } = "Cog"; 
        /// <summary>
        /// 组件版本
        /// </summary>
        public string Version { get; set; } = "1.0.0"; 
        /// <summary>
        /// 作者
        /// </summary>
        public string Author { get; set; } = string.Empty; 
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; } = true; 
        /// <summary>
        /// 排序顺序
        /// </summary>
        public int Order { get; set; } = 0;
    }
}
