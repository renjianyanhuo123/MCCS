using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Windows;

namespace MCCS.Example
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Critical;
        }
    }

}
