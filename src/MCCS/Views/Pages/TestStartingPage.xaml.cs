namespace MCCS.Views.Pages
{
    /// <summary>
    /// TestStartingPage.xaml 的交互逻辑
    /// </summary>
    public partial class TestStartingPage
    { 

        public TestStartingPage()
        {
            InitializeComponent();
        } 

        protected override async void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            var viewModel = DataContext as ViewModels.Pages.TestStartingPageViewModel;
            viewModel?.InitializeDataSubscriptions();
            await viewModel?.LoadModelsCommand.Execute()!;
            // await viewModel.InitialCurves();
        }
          
    }
}
