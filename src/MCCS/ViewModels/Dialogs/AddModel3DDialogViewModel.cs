using MaterialDesignThemes.Wpf;  
using MCCS.Infrastructure.Repositories;

namespace MCCS.ViewModels.Dialogs
{
    public sealed class AddModel3DDialogViewModel: BaseViewModel
    {
        public const string Tag = "AddModel3DDialog";
        private readonly IModel3DDataRepository _model3DDataRepository;

        public AddModel3DDialogViewModel(IModel3DDataRepository model3DDataRepository, IEventAggregator eventAggregator) : base(eventAggregator)
        {
            _model3DDataRepository = model3DDataRepository;
        }

        #region Property
        private string _modelName = string.Empty;
        public string ModelName
        {
            get => _modelName;
            set => SetProperty(ref _modelName, value);
        }

        private bool _isUse;
        public bool IsUse
        {
            get => _isUse;
            set => SetProperty(ref _isUse, value);
        }
        #endregion

        #region Command
        public AsyncDelegateCommand OkCommand => new(ExecuteOkCommand);
        public DelegateCommand CancelCommand => new(ExecuteCancelCommand);
        #endregion

        #region private methods

        private async Task ExecuteOkCommand()
        {
            //if (string.IsNullOrWhiteSpace(ModelName)) return;
            //var addModel = new Model3DBaseInfo()
            //{
            //    Name = ModelName
            //};
            //var modelId = await _model3DDataRepository.AddModel3DAsync(addModel);
            //if (modelId > 0)
            //{
            //    DialogHost.CloseDialogCommand.Execute(false, null);
            //    _eventAggregator.GetEvent<NotificationAddModel3DEvent>().Publish(new NotificationAddModel3DEventParam(modelId));
            //}
        }

        private void ExecuteCancelCommand()
        {
            DialogHost.CloseDialogCommand.Execute(false, null);
        }

        #endregion
    }
}
