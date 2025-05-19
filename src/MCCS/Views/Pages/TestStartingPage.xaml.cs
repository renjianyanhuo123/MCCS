using HelixToolkit.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MCCS.Views.Pages
{
    /// <summary>
    /// TestStartingPage.xaml 的交互逻辑
    /// </summary>
    public partial class TestStartingPage : UserControl
    {
        public TestStartingPage()
        {
            InitializeComponent();
            LoadModels();
        }

        public void LoadModels()
        {
            ModelImporter importer = new ModelImporter();

            // 加载第一个模型
            var model1 = importer.Load(@"F:\Futuristic_Trike_High_Poly_2.stl");

            // 加载第二个模型
            // Model3DGroup model2 = importer.Load(@"C:\Models\Model2.stl");

            // 创建ModelVisual3D容器分别承载每个模型
            ModelVisual3D visual1 = new ModelVisual3D { Content = model1 };
            // ModelVisual3D visual2 = new ModelVisual3D { Content = model2 };

            // 将多个模型添加到HelixViewport3D中
            viewPort3d.Children.Add(visual1);
            // viewPort3d.Children.Add(visual2);
        }
    }
}
