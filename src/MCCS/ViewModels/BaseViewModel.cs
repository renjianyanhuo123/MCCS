using System.Windows;

namespace MCCS.ViewModels;

public class BaseViewModel(IEventAggregator eventAggregator, IDialogService dialogService)
    : BindableBase, INavigationAware
{
    protected readonly IEventAggregator _eventAggregator = eventAggregator;
    protected IDialogService _dialogService = dialogService;
    protected string _parentView = string.Empty;

    public BaseViewModel(IEventAggregator eventAggregator) : this(eventAggregator, null)
    {
    }

    public bool IsNavigationTarget(NavigationContext navigationContext)
    {
        //throw new NotImplementedException();
        return true;
    }

    public void OnNavigatedFrom(NavigationContext navigationContext)
    {
        //throw new NotImplementedException();
    }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        string viewName = navigationContext.Parameters.GetValue<string>("Parent");
        if (viewName != null)
        {
            _parentView = viewName;
        }
    }

    /// <summary>
    /// 异步修改绑定到UI的属性
    /// </summary>
    /// <param name="callback"></param>
    protected void PropertyChangeAsync(Action callback)
    {
        if (Application.Current == null) return;

        Application.Current.Dispatcher.Invoke(callback);
    }
}
