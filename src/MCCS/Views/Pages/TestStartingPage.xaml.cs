using HelixToolkit.Wpf;
using MCCS.ViewModels.Others;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using MCCS.ViewModels.Pages;

namespace MCCS.Views.Pages
{
    /// <summary>
    /// TestStartingPage.xaml 的交互逻辑
    /// </summary>
    public partial class TestStartingPage : UserControl
    {
        private Dictionary<Model3D, Model3DViewModel> _modelViewModelMap;
        // private GeometryModel3D lastHoveredModel = null;
        private TestStartingPageViewModel _viewModel;
        // private Material originalMaterial = null;
        private BillboardTextGroupVisual3D _billboardTextGroup;
        // private Dictionary<string, LinesVisual3D> labelLines = [];


        public TestStartingPage()
        {
            InitializeComponent();
            // Loaded += TestStartingPage_Loaded;
            _viewModel = DataContext as TestStartingPageViewModel ?? throw new ArgumentNullException(nameof(_viewModel));
            // 初始化模型映射字典
            _modelViewModelMap = new Dictionary<Model3D, Model3DViewModel>();
            // 初始化Billboard服务
            _billboardTextGroup = new BillboardTextGroupVisual3D
            {
                Background = new SolidColorBrush(Colors.White),
                BorderBrush = new SolidColorBrush(Colors.Black),
                Foreground = new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(1),
                FontSize = 18,
                Padding = new Thickness(2),
                Offset = new Vector(20, 20),
                PinBrush = new SolidColorBrush(Colors.Gray)
            };
            viewPort.Children.Add(_billboardTextGroup);
            // LoadModels();
            // 订阅模型加载完成事件
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(TestStartingPageViewModel.LoadedModelsCount))
                {
                    // 当模型加载完成时，更新模型映射并添加到视图
                    UpdateModelsInViewport();
                }
            };
        }

        private void UpdateModelsInViewport()
        {
            foreach (var modelVm in _viewModel.Model3DList)
            {
                if (modelVm is not { IsLoaded: true, Model: not null } ||
                    _modelViewModelMap.ContainsValue(modelVm)) continue;
                // 添加模型到视图
                var modelVisual = new ModelVisual3D { Content = modelVm.Model };
                ModelsGroupContainer.Children.Add(modelVisual);

                // 更新模型映射
                _modelViewModelMap[modelVm.Model] = modelVm;

                // 创建标签
                // _billboardService.CreateLabel(modelVm);
            }
        }

        private void SetMaterialAndAddToDictionary(Model3DGroup modelGroup, string name, Color color)
        {
            foreach (var child in modelGroup.Children)
            {
                switch (child)
                {
                    case GeometryModel3D geometryModel:
                        geometryModel.Material = new DiffuseMaterial(new SolidColorBrush(color));
                        geometryModel.BackMaterial = new DiffuseMaterial(new SolidColorBrush(color));
                        _modelViewModelMap[geometryModel] = new Model3DViewModel { Name = name, OriginalColor = color };
                        break;
                    case Model3DGroup childGroup:
                        SetMaterialAndAddToDictionary(childGroup, name, color);
                        break;
                }
            }
        }

        //private GeometryModel3D CreateModel(MeshGeometry3D mesh, string name, Color color)
        //{
        //    // 创建模型并设置材质
        //    var material = new DiffuseMaterial(new SolidColorBrush(color));
        //    var model = new GeometryModel3D
        //    {
        //        Geometry = mesh,
        //        Material = material,
        //        BackMaterial = material
        //    };

        //    return model;
        //}

        private void AddModelToViewport(GeometryModel3D model)
        {
            var modelVisual = new ModelVisual3D { Content = model };
            ModelsGroupContainer.Children.Add(modelVisual);
        }

        private void ViewPort_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left) return;
            var hitResult = VisualTreeHelper.HitTest(viewPort, e.GetPosition(viewPort));

            // 重置所有模型状态
            _viewModel.ResetAllModels();

            // 处理选中事件
            if (hitResult is RayMeshGeometry3DHitTestResult { ModelHit: { } hitModel })
            {
                var selectedVm = FindViewModel(hitModel);
                _viewModel.SelectedModel = selectedVm;
            }
            else
            {
                _viewModel.SelectedModel = null;
            }
        }

        private void ViewPort_MouseMove(object sender, MouseEventArgs e)
        {
            // 处理鼠标悬停事件
            var hitResult = VisualTreeHelper.HitTest(viewPort, e.GetPosition(viewPort));

            // 重置所有模型的悬停状态
            foreach (var modelVm in _viewModel.Model3DList)
            {
                modelVm.IsHovered = false;
            }

            // 设置当前悬停模型状态
            if (hitResult is not RayMeshGeometry3DHitTestResult { ModelHit: { } hitModel }) return;
            var hoveredVm = FindViewModel(hitModel);
            hoveredVm.IsHovered = true;
        }

        // 根据Model3D查找对应的ViewModel
        private Model3DViewModel FindViewModel(Model3D model)
        {
            // 直接查找
            if (_modelViewModelMap.TryGetValue(model, out var viewModel))
            {
                return viewModel;
            }

            // 如果模型是一个组的一部分，需要遍历查找父模型
            if (model is GeometryModel3D geometryModel)
            {
                return (from pair in _modelViewModelMap where IsPartOfModel(geometryModel, pair.Key) 
                        select pair.Value).FirstOrDefault();
            }

            return null;
        }

        /// <summary>
        /// 检查一个GeometryModel3D是否是Model3D的一部分
        /// </summary>
        /// <param name="geometryModel"></param>
        /// <param name="parentModel"></param>
        /// <returns></returns>
        private bool IsPartOfModel(GeometryModel3D geometryModel, Model3D parentModel)
        {
            if (parentModel == geometryModel) return true;
            return parentModel is Model3DGroup modelGroup 
                   && modelGroup.Children
                       .Any(child => IsPartOfModel(geometryModel, child));
        }
    }

    public class ModelData
    {
        public string Name { get; set; }
        public Color OriginalColor { get; set; }
        public string Description { get; set; }
        public string Value { get; set; }
        public Point3D Position { get; set; }
    }
}
