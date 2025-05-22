using HelixToolkit.Wpf;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MCCS.Views.Pages
{
    /// <summary>
    /// TestStartingPage.xaml 的交互逻辑
    /// </summary>
    public partial class TestStartingPage : UserControl
    {
        private Dictionary<GeometryModel3D, ModelData> modelDataDictionary = new Dictionary<GeometryModel3D, ModelData>();
        private GeometryModel3D lastHoveredModel = null;
        private Material originalMaterial = null;
        private BillboardTextGroupVisual3D billboardTextGroup;
        private Dictionary<string, LinesVisual3D> labelLines = [];


        public TestStartingPage()
        {
            InitializeComponent();
            // Loaded += TestStartingPage_Loaded;
            // 创建一个BillboardTextGroupVisual3D用于显示模型数据
            billboardTextGroup = new BillboardTextGroupVisual3D
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
            viewPort.Children.Add(billboardTextGroup);
            LoadModels();
        }

        private void LoadModels()
        {
            // 还可以导入外部模型，例如OBJ、STL等
            ImportExternalModel("model1", @"F:\models\others\model1.stl", new Point3D(-5, 0, 0), Colors.Green);
            ImportExternalModel("model2", @"F:\models\others\model2.stl", new Point3D(0, 0, 0), Colors.Green);
            ImportExternalModel("model3", @"F:\models\others\model2.stl", new Point3D(5, 0, 0), Colors.Green);
        }
        // 可选：导入外部3D模型
        private void ImportExternalModel(string name, string filePath, Point3D position, Color color)
        {
            try
            {
                // 根据文件扩展名选择适当的导入器
                Model3D model3D = null;

                if (filePath.EndsWith(".obj", StringComparison.OrdinalIgnoreCase))
                {
                    var importer = new ObjReader();
                    model3D = importer.Read(filePath);
                }
                else if (filePath.EndsWith(".stl", StringComparison.OrdinalIgnoreCase))
                {
                    var importer = new StLReader();
                    model3D = importer.Read(filePath);
                }

                if (model3D == null) return;
                // 设置模型的位置
                var transform = new TranslateTransform3D(position.X, position.Y, position.Z);
                model3D.Transform = transform;

                switch (model3D)
                {
                    // 如果导入的模型是模型组，需要递归遍历设置材质和添加到字典
                    case Model3DGroup modelGroup:
                        SetMaterialAndAddToDictionary(modelGroup, name, color);
                        break;
                    case GeometryModel3D geometryModel:
                        geometryModel.Material = new DiffuseMaterial(new SolidColorBrush(color));
                        geometryModel.BackMaterial = new DiffuseMaterial(new SolidColorBrush(color));
                        modelDataDictionary[geometryModel] = new ModelData { Name = name, OriginalColor = color };
                        break;
                }

                // 添加到视图
                var modelVisual = new ModelVisual3D { Content = model3D };
                modelsGroup.Children.Add(modelVisual);
                // 添加Billboard文本
                AddBillboardText(position + new Vector3D(0, 0, 5), name,
                    "外部导入模型\n文件: " + System.IO.Path.GetFileName(filePath));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导入模型失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetMaterialAndAddToDictionary(Model3DGroup modelGroup, string name, Color color)
        {
            foreach (var child in modelGroup.Children)
            {
                if (child is GeometryModel3D geometryModel)
                {
                    geometryModel.Material = new DiffuseMaterial(new SolidColorBrush(color));
                    geometryModel.BackMaterial = new DiffuseMaterial(new SolidColorBrush(color));
                    modelDataDictionary[geometryModel] = new ModelData { Name = name, OriginalColor = color };
                }
                else if (child is Model3DGroup childGroup)
                {
                    SetMaterialAndAddToDictionary(childGroup, name, color);
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
            modelsGroup.Children.Add(modelVisual);
        }

        private void ViewPort_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                var hitResult = VisualTreeHelper.HitTest(viewPort, e.GetPosition(viewPort));

                // 重置所有模型的材质
                ResetAllModelMaterials();

                // 处理点击事件
                if (hitResult is RayMeshGeometry3DHitTestResult meshHit)
                {
                    if (meshHit.ModelHit is GeometryModel3D hitModel && modelDataDictionary.TryGetValue(hitModel, out var value))
                    {
                        // 设置选中高亮显示（使用明亮的黄色）
                        var highlightMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.Blue));
                        hitModel.Material = highlightMaterial;
                        hitModel.BackMaterial = highlightMaterial;

                        // 显示选中的模型名称
                        SelectedModelText.Text = value.Name;
                    }
                    else
                    {
                        SelectedModelText.Text = "无";
                    }
                }
                else
                {
                    SelectedModelText.Text = "无";
                }
            }
        }

        private void ViewPort_MouseMove(object sender, MouseEventArgs e)
        {
            // 处理鼠标悬停事件
            var hitResult = VisualTreeHelper.HitTest(viewPort, e.GetPosition(viewPort));

            // 重置上一个悬停的模型
            if (lastHoveredModel != null && originalMaterial != null)
            {
                lastHoveredModel.Material = originalMaterial;
                lastHoveredModel.BackMaterial = originalMaterial;
                lastHoveredModel = null;
                originalMaterial = null;
            }

            // 设置当前悬停的模型高亮
            if (hitResult is RayMeshGeometry3DHitTestResult meshHit)
            {
                if (meshHit.ModelHit is GeometryModel3D hitModel && modelDataDictionary.ContainsKey(hitModel))
                {
                    // 保存原始材质和当前悬停的模型引用
                    lastHoveredModel = hitModel;
                    originalMaterial = hitModel.Material;

                    // 设置悬停高亮显示（使用半透明的橙色）
                    var hoverColor = Colors.Orange;
                    hoverColor.A = 180; // 设置一定的透明度
                    var hoverMaterial = new DiffuseMaterial(new SolidColorBrush(hoverColor));
                    hitModel.Material = hoverMaterial;
                    hitModel.BackMaterial = hoverMaterial;
                }
            }
        }

        private void ResetAllModelMaterials()
        {
            foreach (var pair in modelDataDictionary)
            {
                var model = pair.Key;
                var data = pair.Value;
                var material = new DiffuseMaterial(new SolidColorBrush(data.OriginalColor));
                model.Material = material;
                model.BackMaterial = material;
            }
        }

        private void AddBillboardText(Point3D position, string title, string details)
        {
            // 创建一个位置在模型上方的Billboard文本
            var textPosition = position + new Vector3D(0, 0, 1.2); // 根据模型位置调整高度

            // 创建标题文本（显示模型名称）BillboardTextGroupVisual3D
            var titleText = new BillboardTextVisual3D
            {
                Text = title,
                Position = textPosition,
                FontSize = 14,
                Foreground = new SolidColorBrush(Colors.Black),
                Background = new SolidColorBrush(Color.FromArgb(150, 255, 255, 255)), // 半透明白色背景
                BorderBrush = new SolidColorBrush(Colors.DarkGray),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(5, 2, 5, 2),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            // 创建详细信息文本
            var detailsText = new BillboardTextVisual3D
            {
                Text = details,
                Position = textPosition + new Vector3D(0, 0, 0.3), // 位置在标题下方
                FontSize = 12,
                Foreground = new SolidColorBrush(Colors.DarkBlue),
                Background = new SolidColorBrush(Color.FromArgb(130, 240, 240, 240)), // 半透明灰色背景
                BorderBrush = new SolidColorBrush(Colors.LightGray),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(5, 2, 5, 2),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            // 将文本添加到Billboard文本组
            billboardTextGroup.Children.Add(titleText);
            billboardTextGroup.Children.Add(detailsText);
             

            // 添加从模型到标签的连接线
            //AddConnectingLine(position, textPosition, name);
            //billboardTextGroup.Items.Add(new BillboardTextItem()
            //{
            //    Text = title,
            //    Position = textPosition,
            //    DepthOffset = 0,
            //    WorldDepthOffset = 0.2
            //});
            //billboardTextGroup.Items.Add(new BillboardTextItem()
            //{
            //    Text = details,
            //    Position = textPosition + new Vector3D(0, 0, 0.3),
            //    DepthOffset = 0,
            //    WorldDepthOffset = 0.2
            //});
        }

        private void AddConnectingLine(Point3D modelPosition, Point3D labelPosition, string modelId)
        {
            // 创建连接线的点集合
            var points = new Point3DCollection
            {
                modelPosition,
                labelPosition
            };

            // 创建连接线
            var line = new LinesVisual3D
            {
                Points = points,
                Thickness = 1,
                Color = Colors.LightSkyBlue
            };

            // 添加到视图
            viewPort.Children.Add(line);

            // 保存线的引用以便后续更新
            labelLines[modelId] = line;
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
