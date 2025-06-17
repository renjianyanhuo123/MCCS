using Prism.Events;

namespace MCCS.ViewModels.Pages.ControlCommandPages
{
    public class ViewStaticControlViewModel : BaseViewModel
    {
        public const string Tag = "StaticControl";
        public ViewStaticControlViewModel(IEventAggregator eventAggregator) : base(eventAggregator)
        {
        }

        #region 页面属性
        private string _unitText = "clear";

        public string UnitText
        {
            get => _unitText;
            set => SetProperty(ref _unitText, value);
        }
        #endregion
    }
}
