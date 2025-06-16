using MCCS.Events;
using MCCS.Models;
using MCCS.ViewModels.Others.Controllers;
using System.Collections.ObjectModel;

namespace MCCS.ViewModels.Pages.Controllers
{
    public class ControllerMainPageViewModel : BaseViewModel
    {
        public const string Tag = "ControllerMainPage";

        private readonly IEventAggregator _eventAggregator;

        private ObservableCollection<ControllerItemModel> _channels = []; 
        private bool _isShowController = false;
        private bool _isParticipateControl = false;
        private Dictionary<string, ControlInfo> _controlInfoDic = [];

        public ControllerMainPageViewModel(IEventAggregator eventAggregator, IDialogService dialogService) : base(eventAggregator, dialogService)
        {
            _eventAggregator = eventAggregator;
            eventAggregator.GetEvent<ControlEvent>().Subscribe(RenderChannels); 
        }

        #region Proterty
        /// <summary>
        /// 是否显示控制区域
        /// </summary>
        public bool IsShowController
        {
            get => _isShowController;
            set => SetProperty(ref _isShowController, value);
        }

        public bool IsParticipateControl 
        {
            get => _isParticipateControl;
            set => SetProperty(ref _isParticipateControl, value);
        }

        /// <summary>
        /// 控制器通道列表
        /// </summary>
        public ObservableCollection<ControllerItemModel> Channels 
        {
            get => _channels;
            set => SetProperty(ref _channels, value);
        }
        #endregion

        #region Command 
        #endregion

        #region private method
        private void RenderChannels(ControlEventParam param)
        {
            Channels.Clear();
            if (param == null) return;
            var success = _controlInfoDic.TryGetValue(param.ChannelId, out var controlInfo);
            if (success)
            {

            }
            else 
            {

            }
            // IsShowController = channels.Any();
        }

        private void ExecuteParticipateControlCommand() 
        {

        }

        //public override void OnNavigatedTo(NavigationContext navigationContext)
        //{
        //    base.OnNavigatedTo(navigationContext);
        //    if (navigationContext.Parameters.ContainsKey("Channels"))
        //    {
        //        var channels = navigationContext.Parameters["Channels"] as List<ControllerItemModel>;
        //        if (channels == null) return;
        //        for (int i = 0; i < channels.Count; i++)
        //        {
        //            channels[i].Index = i + 1;
        //            Channels.Add(channels[i]);
        //        }
        //    }
        //}
        #endregion
    }
}
