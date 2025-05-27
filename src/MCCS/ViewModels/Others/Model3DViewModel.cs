using System.Windows.Media;
using System.Windows.Media.Media3D;
using MCCS.Models;

namespace MCCS.ViewModels.Others
{
    public class Model3DViewModel : BindableBase
    {
        private readonly ModelData _modelData;
        private bool _isSelected;
        private bool _isHovered;
        private bool _isLoaded;
        private bool _isLoading;
        private string _loadingStatus;
        private Material _currentMaterial;
        private Material _originalMaterial;

        public Model3DViewModel(ModelData modelData)
        {
            _modelData = modelData ?? throw new ArgumentNullException(nameof(modelData));

            // 创建原始材质
            _originalMaterial = new DiffuseMaterial(new SolidColorBrush(modelData.OriginalColor));
            _currentMaterial = _originalMaterial;
        }

        public string Id => _modelData.Id;
        public string Name => _modelData.Name;
        public string Description => _modelData.Description;
        public string FilePath => _modelData.FilePath;
        public Point3D Position => _modelData.Position;
        public string FileName => _modelData.FileName;
        public Model3D Model => _modelData.Model;
        public string DisplayText => _modelData.DisplayText;

        public bool IsLoaded
        {
            get => _isLoaded;
            set => SetProperty(ref _isLoaded, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string LoadingStatus
        {
            get => _loadingStatus;
            set => SetProperty(ref _loadingStatus, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (SetProperty(ref _isSelected, value))
                {
                    UpdateModelMaterial();
                }
            }
        }

        public bool IsHovered
        {
            get => _isHovered;
            set
            {
                if (SetProperty(ref _isHovered, value))
                {
                    UpdateModelMaterial();
                }
            }
        }

        public Material CurrentMaterial
        {
            get => _currentMaterial;
            set => SetProperty(ref _currentMaterial, value);
        }

        // 更新模型的材质基于选中和悬停状态
        private void UpdateModelMaterial()
        {
            if (IsSelected)
            {
                // 选中状态 - 蓝色
                CurrentMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.Blue));
            }
            else if (IsHovered)
            {
                // 悬停状态 - 半透明橙色
                var hoverColor = Colors.Orange;
                hoverColor.A = 180;
                CurrentMaterial = new DiffuseMaterial(new SolidColorBrush(hoverColor));
            }
            else
            {
                // 默认状态 - 原始颜色
                CurrentMaterial = _originalMaterial;
            }

            // 如果模型已加载，应用材质
            ApplyMaterialToModel(_modelData.Model, CurrentMaterial);
        }

        /// <summary>
        /// 递归应用材质到模型
        /// </summary>
        /// <param name="model"></param>
        /// <param name="material"></param>
        private void ApplyMaterialToModel(Model3D model, Material material)
        {
            switch (model)
            {
                case GeometryModel3D geometryModel:
                    geometryModel.Material = material;
                    geometryModel.BackMaterial = material;
                    break;
                case Model3DGroup modelGroup:
                {
                    foreach (var child in modelGroup.Children)
                    {
                        ApplyMaterialToModel(child, material);
                    }
                    break;
                }
            }
        }

        // 设置加载完成的模型
        public void SetLoadedModel(Model3D model)
        {
            _modelData.Model = model;
            IsLoaded = true;
            IsLoading = false;
            LoadingStatus = "加载完成";

            // 应用当前材质
            UpdateModelMaterial();

            // OnPropertyChanged(nameof(Model));
        }
    }
}
