using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MCCS.ViewModels.Others
{
    public class Model3DViewModel : BindableBase
    {
        private string _name; 
        private Model3D _model;
        private Point3D _position;
        private bool _isSelected;
        private bool _isHovered; 
        private bool _isInteractive;
        private Material _currentMaterial;
        private Material _originalMaterial;
        private bool _isClickable;

        public string Id { get; set; }

        public string FilePath { get; set; }

        public bool IsClickable
        {
            get => _isClickable;
            set => SetProperty(ref _isClickable, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public Point3D Position
        {
            get => _position;
            set => SetProperty(ref _position, value);
        }

        public bool IsInteractive
        {
            get => _isInteractive;
            set => SetProperty(ref _isInteractive, value);
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

        public Model3D Model
        {
            get => _model;
            set => SetProperty(ref _model, value);
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

        public Material OriginalMaterial
        {
            get => _originalMaterial;
            set => SetProperty(ref _originalMaterial, value);
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
            ApplyMaterialToModel(Model, CurrentMaterial);
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
            Model = model;  
            // 应用当前材质
            UpdateModelMaterial();

            // OnPropertyChanged(nameof(Model));
        }
    }
}
