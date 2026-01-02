using System.Linq.Expressions;
using MCCS.Infrastructure.Models;
using MCCS.Infrastructure.Models.MethodManager;

namespace MCCS.Infrastructure.Repositories.Method
{
    public interface IMethodRepository
    {
        /// <summary>
        /// Asynchronously retrieves the method definition associated with the specified identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the method to retrieve. Must be a positive value.</param>
        /// <returns>—A task that represents the asynchronous operation. The task result contains a <see cref="MethodModel"/>
        /// representing the requested method, or <see langword="null"/> if no method with the specified identifier
        /// exists.</returns>
        Task<MethodModel> GetMethodAsync(long id); 
        /// <summary>
        /// Asynchronously retrieves a list of <see cref="MethodModel"/> instances that satisfy the specified filter
        /// expression.
        /// </summary>
        /// <param name="expression">An expression used to filter the <see cref="MethodModel"/> instances to return. Only methods for which the
        /// expression evaluates to <see langword="true"/> are included in the result.</param>
        /// <returns>—A task that represents the asynchronous operation. The task result contains a list of <see
        /// cref="MethodModel"/> objects matching the filter; the list is empty if no matches are found.</returns>
        Task<List<MethodModel>> GetMethodsAsync(Expression<Func<MethodModel, bool>> expression); 
        /// <summary>
        /// Asynchronously retrieves a paged collection of <see cref="MethodModel"/> instances that match the specified
        /// filter criteria.
        /// </summary>
        /// <param name="pageIndex">The zero-based index of the page to retrieve. Must be greater than or equal to 0.</param>
        /// <param name="pageSize">The number of items to include in each page. Must be greater than 0.</param>
        /// <param name="expression">An expression used to filter the <see cref="MethodModel"/> instances to include in the result. Cannot be
        /// <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see
        /// cref="PageModel{MethodModel}"/> with the filtered <see cref="MethodModel"/> instances for the specified
        /// page. If no items match the filter, the collection will be empty.</returns>
        Task<PageModel<MethodModel>> GetPageMethodsAsync(int pageIndex, int pageSize, Expression<Func<MethodModel, bool>> expression); 
        /// <summary>
        /// Asynchronously retrieves the interface setting associated with the specified method identifier.
        /// </summary>
        /// <param name="methodId">The unique identifier of the method for which to retrieve the interface setting.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see
        /// cref="MethodInterfaceSettingModel"/> for the specified method, or <see langword="null"/> if no setting is
        /// found.</returns>
        Task<MethodInterfaceSettingModel> GetInterfaceSettingAsync(long methodId); 
        /// <summary>
        /// Asynchronously retrieves the list of UI component models available for method configuration.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see
        /// cref="MethodUiComponentsModel"/> objects describing the available UI components. The list is empty if no
        /// components are available.</returns>
        Task<List<MethodUiComponentsModel>> GetUiComponentsAsync();
        /// <summary>
        /// 获取所有方法的界面组件信息
        /// </summary>
        /// <returns></returns>
        List<MethodUiComponentsModel> GetUiComponents();
        /// <summary>
        /// 获取单个方法的界面组件信息
        /// </summary>
        /// <param name="componentId"></param>
        /// <returns></returns>
        Task<MethodUiComponentsModel> GetMethodUiComponentByIdAsync(long componentId);
        /// <summary>
        /// 获取单个方法的工作流配置信息
        /// </summary>
        /// <param name="methodId"></param>
        /// <returns></returns>
        Task<MethodWorkflowSettingModel> GetMethodWorkflowSettingAsync(long methodId);
        /// <summary>
        /// Asynchronously deletes the method with the specified identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the method to delete.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="ValueTask{Boolean}"/> representing the asynchronous operation. The result is <see
        /// langword="true"/> if the method was successfully deleted; otherwise, <see langword="false"/>.</returns>
        ValueTask<bool> DeleteMethodAsync(long id, CancellationToken cancellationToken = default);
        /// <summary>
        /// Asynchronously adds a new method definition to the repository.
        /// </summary>
        /// <param name="method">The <see cref="MethodModel"/> instance representing the method to add. Cannot be <c>null</c>.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="ValueTask{TResult}"/> whose result is the unique identifier assigned to the newly added method.</returns>
        ValueTask<long> AddMethodAsync(MethodModel method, CancellationToken cancellationToken = default);
        /// <summary>
        /// Adds a new interface setting to the collection.
        /// </summary>
        /// <param name="model">The interface setting to add. Cannot be <c>null</c>.</param>
        /// <returns>The unique identifier of the newly added interface setting.</returns>
        long AddInterfaceSetting(MethodInterfaceSettingModel model);
    }
}
