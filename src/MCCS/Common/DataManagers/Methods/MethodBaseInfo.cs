using MCCS.Core.Models.MethodManager;
using Newtonsoft.Json;

namespace MCCS.Common.DataManagers.Methods
{
    [method: JsonConstructor]
    public class MethodBaseInfo(
        string name,
        MethodTypeEnum methodType,
        TestTypeEnum testType,
        string standard,
        string code,
        string remark)
        : BindableBase
    {
        /// <summary>
        /// 方法名称
        /// </summary>  
        public string Name { get; set; } = name;

        /// <summary>
        /// 方法类型
        /// </summary> 
        public MethodTypeEnum MethodType { get; private set; } = methodType;

        /// <summary>
        /// 试验类型
        /// </summary> 
        public TestTypeEnum TestType { get; private set; } = testType;

        /// <summary>
        /// 方法标准
        /// </summary>
        public string Standard
        {
            get;
            set;
        } = standard;

        public string Code
        {
            get;
            private set;
        } = code;

        public string Remark
        {
            get; set;
        } = remark;
        

    }
}
