namespace MCCS.Station.Core.Parameters
{
    /// <summary>
    /// 参数解耦
    /// </summary>
    /// <typeparam name="T">参数类型</typeparam>
    public interface IHasParameters<T> where T : BaseParameter
    {
        /// <summary>
        /// 参数对象
        /// </summary>
        T Parameters { get; set; }
    }
}
