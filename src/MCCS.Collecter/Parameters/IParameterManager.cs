namespace MCCS.Collecter.Parameters
{
    public interface IParameterManager
    {
        /// <summary>
        /// 获取参数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        T Get<T>(string id) where T : BaseParameter;
        /// <summary>
        /// 保存参数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <param name="parameters"></param>
        void Save<T>(string id, T parameters) where T : BaseParameter;
    }
}
