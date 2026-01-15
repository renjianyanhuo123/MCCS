using System.ComponentModel;

namespace MCCS.Interface.Components.Enums
{
    public enum ControlModeTypeEnum
    {
        /// <summary>
        /// 手动
        /// </summary>
        [Description("手动控制")]
        Manual = 0,
        /// <summary>
        /// 静态
        /// </summary>
        [Description("静态控制")]
        Static = 1,
        /// <summary>
        /// 疲劳
        /// </summary>
        [Description("疲劳控制")]
        Fatigue = 2,
        /// <summary>
        /// 程序
        /// </summary>
        [Description("程序控制")]
        Programmable = 3
    }
}
