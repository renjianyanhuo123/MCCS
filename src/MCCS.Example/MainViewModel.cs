using System.Diagnostics;
using System.Windows.Input;
using MCCS.UserControl;
using MCCS.UserControl.Params;

namespace MCCS.Example
{
    public class MainViewModel : BindingBase
    {
        private object _param;
        private int _total;
        public MainViewModel()
        {
            Total = 317;
        }

        public int Total
        {
            get => _total;
            set => SetProperty(ref _total, value);
        }


        public ICommand OnPageChanged => new RelayCommand(param =>
        {
            var p = param as PageChangedParam;
            Thread.Sleep(1000);
            Debug.WriteLine($"当前页:{p.CurrentPage},PageSize:{p.PageSize}");
        }, _ => true);
    }
}
