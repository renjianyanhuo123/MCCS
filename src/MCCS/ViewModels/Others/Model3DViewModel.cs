using MCCS.Core.Models.Model3D;
using System.Windows.Media.Media3D;
using HelixToolkit.SharpDX.Core.Model.Scene;
using MCCS.Common; 
using SharpDX;
using Assimp;

namespace MCCS.ViewModels.Others
{
    public class Model3DViewModel : BindableBase
    {
        private string _name; 
        private Model3D _model;
        private Point3D _position;
        private bool _isSelected;
        private bool _isHovered; 

        private HelixToolkit.Wpf.SharpDX.Material _currentMaterial;
        private bool _isClickable;
        private MeshGeometry3D _geometry;
        private Transform3D _transform;
        private readonly Model3DData _model3DData;
        private readonly SceneNode _sceneNode;

        public Model3DViewModel(SceneNode sceneNode, Model3DData model3DData)
        {
            _model3DData = model3DData;
            _sceneNode = sceneNode;
            UpdateMaterial();
        }

        public Model3DData Model3DData { get; } 

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

        public SceneNode SceneNode => _sceneNode;

        public bool IsSelectable { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (SetProperty(ref _isSelected, value))
                {
                    UpdateMaterial();
                }
            }
        }

        public Transform3D Transform
        {
            get => _transform;
            set => SetProperty(ref _transform, value);
        }

        public bool IsHovered
        {
            get => _isHovered;
            set
            {
                if (SetProperty(ref _isHovered, value))
                {
                    UpdateMaterial();
                }
            }
        }

        #region private method 

        private void UpdateMaterial()
        {
            var t = _sceneNode.GetType();
            if (_sceneNode is not MeshNode geometryNode) return; 

            if (IsSelected)
            {
                geometryNode.Material = EnumToMaterial.GetMaterialFromEnum(MaterialEnum.Selected);
            }
            else if (IsHovered)
            {
                geometryNode.Material = EnumToMaterial.GetMaterialFromEnum(MaterialEnum.Hover);
            }
            else
            {
                geometryNode.Material = EnumToMaterial.GetMaterialFromEnum(MaterialEnum.Original);
            }
        } 
        #endregion
    }
}
