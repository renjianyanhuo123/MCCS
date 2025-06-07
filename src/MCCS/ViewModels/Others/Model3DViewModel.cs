using MCCS.Core.Models.Model3D;
using System.Windows.Media.Media3D;
using HelixToolkit.SharpDX.Core.Model.Scene;
using MCCS.Common;
using HelixToolkit.SharpDX.Core;
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
        private Transform3D _transform;
        private readonly Model3DData _model3DData;
        private readonly SceneNode _sceneNode;

        public Model3DViewModel(SceneNode sceneNode, Model3DData model3DData)
        {
            _model3DData = model3DData;
            _sceneNode = sceneNode;
            UpdateMaterial();

            // 为场景节点设置Tag，以便在点击时识别
            if (_sceneNode != null)
            {
                _sceneNode.Tag = this; 
                // 为所有子节点也设置Tag
                foreach (var node in _sceneNode.Traverse()) node.Tag = this;
            }
        }

        public Model3DData Model3DData => _model3DData; 

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

        public bool IsSelectable => _model3DData.Type == ModelType.Actuator;

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
            foreach (var node in _sceneNode.Traverse())
            {
                if (node is not MaterialGeometryNode m) continue;
                m.Material = EnumToMaterial.GetMaterialFromEnum(MaterialEnum.Original);
                if (IsSelected)
                {
                    m.Material = EnumToMaterial.GetMaterialFromEnum(MaterialEnum.Selected);
                }
                else if (IsHovered)
                {
                    m.Material = EnumToMaterial.GetMaterialFromEnum(MaterialEnum.Hover);
                }
                else
                {
                    m.Material = EnumToMaterial.GetMaterialFromEnum(MaterialEnum.Original);
                }
            }
        } 
        #endregion
    }
}
