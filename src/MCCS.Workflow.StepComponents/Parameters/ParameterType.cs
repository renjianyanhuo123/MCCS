namespace MCCS.Workflow.StepComponents.Parameters
{
    /// <summary>
    /// 参数类型
    /// </summary>
    public enum ParameterType
    {
        /// <summary>
        /// 字符串
        /// </summary>
        String,

        /// <summary>
        /// 整数
        /// </summary>
        Integer,

        /// <summary>
        /// 浮点数
        /// </summary>
        Double,

        /// <summary>
        /// 布尔值
        /// </summary>
        Boolean,

        /// <summary>
        /// 日期时间
        /// </summary>
        DateTime,

        /// <summary>
        /// 下拉选择（单选）
        /// </summary>
        Select,

        /// <summary>
        /// 多选
        /// </summary>
        MultiSelect,

        /// <summary>
        /// 文件路径
        /// </summary>
        FilePath,

        /// <summary>
        /// 文件夹路径
        /// </summary>
        FolderPath,

        /// <summary>
        /// 密码（加密存储）
        /// </summary>
        Password,

        /// <summary>
        /// 多行文本
        /// </summary>
        MultilineText,

        /// <summary>
        /// JSON对象
        /// </summary>
        Json,

        /// <summary>
        /// 表达式（支持变量引用）
        /// </summary>
        Expression,

        /// <summary>
        /// 颜色
        /// </summary>
        Color,

        /// <summary>
        /// 键值对列表
        /// </summary>
        KeyValueList
    }
}
