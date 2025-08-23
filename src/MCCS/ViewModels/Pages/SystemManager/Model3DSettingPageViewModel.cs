using System.Collections.ObjectModel;
using MaterialDesignThemes.Wpf;
using MCCS.Core.Repositories;
using MCCS.Events.SystemManager;
using MCCS.Models.Model3D;
using MCCS.Views.Dialogs;

namespace MCCS.ViewModels.Pages.SystemManager
{
    public class Model3DSettingPageViewModel : BaseViewModel
    {
        public const string Tag = "Model3DSetting";

        private readonly IModel3DDataRepository _model3DDataRepository;
        private readonly IContainerProvider _containerProvider;

        public Model3DSettingPageViewModel(IModel3DDataRepository model3DDataRepository,
            IContainerProvider containerProvider,
            IEventAggregator eventAggregator) : base(eventAggregator)
        {
            _model3DDataRepository = model3DDataRepository;
            _containerProvider = containerProvider;
            _eventAggregator.GetEvent<NotificationAddModel3DEvent>().Subscribe(OnAddModel3DEvent);
        }

        #region Command
        public AsyncDelegateCommand LoadCommand => new(ExecuteLoadCommand);
        public AsyncDelegateCommand AddModelCommand => new(ExecuteAddModelCommand);
        #endregion

        #region Property 
        public ObservableCollection<Model3DBaseInfoModel> Model3Ds { get; } = [];
        #endregion

        #region Private Method
        private void OnAddModel3DEvent(NotificationAddModel3DEventParam param)
        {
            var list = _model3DDataRepository.GetModelBaseInfos(c => true);
            Model3Ds.Clear();
            foreach (var item in list)
            {
                Model3Ds.Add(new Model3DBaseInfoModel
                {
                    Id = item.Id,
                    CreateTime = item.CreateTime,
                    UpdateTime = item.UpdateTime,
                    Name = item.Name,
                    IsUse = item.IsUse
                });
            }
        }

        private async Task ExecuteAddModelCommand()
        {
            var addModel3D = _containerProvider.Resolve<AddModel3DDialog>();
            var result = await DialogHost.Show(addModel3D, "RootDialog");
        }

        private async Task ExecuteLoadCommand()
        {
            var list = await _model3DDataRepository.GetModelBaseInfosAsync(c => true);
            Model3Ds.Clear();
            foreach (var item in list)
            {
                Model3Ds.Add(new Model3DBaseInfoModel
                {
                    Id = item.Id,
                    CreateTime = item.CreateTime,
                    UpdateTime = item.UpdateTime,
                    Name = item.Name,
                    IsUse = item.IsUse
                });
            }
        }
        #endregion
    }
}
