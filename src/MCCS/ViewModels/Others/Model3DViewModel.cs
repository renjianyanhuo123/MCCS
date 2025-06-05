using MCCS.Core.Models.Model3D;
using System.Windows.Media.Media3D;
using HelixToolkit.SharpDX.Core.Model.Scene;
using MCCS.Common; 
using SharpDX;

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
            UpdateTransform();
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
        private void UpdateTransform()
        {
            var tg = new Transform3DGroup();
            // Scale
            tg.Children.Add(_model3DData.ScaleStr.ToVector<ScaleTransform3D>());

            // Rotation (Euler angles)
            var vRotationVec = _model3DData.RotationStr.ToVector<Vector3>();
            tg.Children.Add(new RotateTransform3D(
                new AxisAngleRotation3D(new Vector3D(1, 0, 0), vRotationVec.X)));
            tg.Children.Add(new RotateTransform3D(
                new AxisAngleRotation3D(new Vector3D(0, 1, 0), vRotationVec.Y)));
            tg.Children.Add(new RotateTransform3D(
                new AxisAngleRotation3D(new Vector3D(0, 0, 1), vRotationVec.Z)));

            // Translation
            tg.Children.Add(_model3DData.PositionStr.ToVector<TranslateTransform3D>());

            Transform = tg;
        }

        private void UpdateMaterial()
        {
            if (_sceneNode is not MaterialGeometryNode geometryNode) return;
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
