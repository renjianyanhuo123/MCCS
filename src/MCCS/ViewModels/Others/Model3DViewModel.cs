using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Model.Scene;
using HelixToolkit.Wpf.SharpDX;
using MCCS.Common; 
using SharpDX;
using System.Diagnostics;
using MCCS.Infrastructure.Models.Model3D;

namespace MCCS.ViewModels.Others
{
    public class Model3DViewModel : BindableBase, IDisposable
    {
        private bool _isSelected;
        private bool _isHovered; 
         
        private readonly Model3DData _model3DData;
        private readonly SceneNode _sceneNode; 

        public Model3DViewModel(SceneNode sceneNode, Model3DData model3DData, System.Windows.Media.Color material)
        {
            _model3DData = model3DData;
            _sceneNode = sceneNode;
            MaterialColor = material;
            UpdateMaterial(); 
            // 为场景节点设置Tag，以便在点击时识别
            _sceneNode.Tag = model3DData.Key;
            ModelId = model3DData.Key;
            // 为所有子节点也设置Tag
            foreach (var node in _sceneNode.Traverse()) node.Tag = model3DData.Key;
        }

        public string ModelId { get; private set; }

        public Model3DData Model3DData => _model3DData; 

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

        private bool _isMultipleSelected;
        public bool IsMultipleSelected
        {
            get => _isMultipleSelected;
            set
            {
                if (SetProperty(ref _isMultipleSelected, value))
                {
                    UpdateMaterial();
                } 
            }
        }

        private System.Windows.Media.Color _materialColor;
        public System.Windows.Media.Color MaterialColor
        {
            get => _materialColor;
            set => SetProperty(ref _materialColor, value);
        }

        /// <summary>
        /// (模拟模型移动)位移默认偏移值 = 0.0
        ///</summary>
        public float DisplacementOffsetValue { get; set; }

        /// <summary>
        /// (模拟模型移动)力默认偏移值 = 0.0
        /// </summary>
        public float ForceOffsetValue { get; set; }

        /// <summary>
        /// 取消模型移动
        /// </summary>
        public CancellationTokenSource CancelModelMove { get; set; } = new();

        /// <summary>
        /// 控制的订阅设备链接
        /// </summary>
        public IDisposable? DeviceSubscription { get; set; } = null;
        #region private method  
        /// <summary>
        /// 更新当前模型的材质
        /// </summary>
        private void UpdateMaterial()
        {
            foreach (var node in _sceneNode.Traverse())
            {
                if (node is not MaterialGeometryNode m) continue;
                if (IsSelected)
                {
                    m.Material = EnumToMaterial.GetMaterialFromEnum(MaterialEnum.Selected);
                }
                else if (IsHovered)
                {
                    m.Material = EnumToMaterial.GetMaterialFromEnum(MaterialEnum.Hover);
                }
                else if (IsMultipleSelected)
                {
                    m.Material = EnumToMaterial.GetMaterialFromEnum(MaterialEnum.MultipleSelected);
                }
                else
                {
                    m.Material = ColorConvertPhongMaterial(MaterialColor);
                }
            }
        } 
        /// <summary>
        /// 颜色转换为Phong材质
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        private static HelixToolkit.Wpf.SharpDX.Material ColorConvertPhongMaterial(System.Windows.Media.Color color)
        {
            return new PhongMaterial
            {
                DiffuseColor =
                    new SharpDX.Color(color.R, color.G, color.B, color.A),
                SpecularColor = SharpDX.Color.White,
                SpecularShininess = 32
            };
        }
        #endregion

        #region public method
        /// <summary>
        /// 寻找模型的中心
        /// </summary>
        /// <returns></returns>
        public Vector3 GetModelCenterFromGeometry()
        {
            var minBound = new Vector3(float.MaxValue);
            var maxBound = new Vector3(float.MinValue);
            bool hasGeometry = false;

            foreach (var node in _sceneNode.Traverse())
            {
                if (node is GeometryNode geometryNode && geometryNode.Geometry != null)
                {
                    var nodeBounds = geometryNode.BoundsWithTransform;
                    minBound = Vector3.Min(minBound, nodeBounds.Minimum);
                    maxBound = Vector3.Max(maxBound, nodeBounds.Maximum);
                    hasGeometry = true;
                }
            }
            if (hasGeometry)
            {
                return (minBound + maxBound) / 2;
            }

            return Vector3.Zero;
        }
        public void PositionChange(Vector3 directionVec, float moveValue) 
        {
            var currentPosition = _sceneNode.ModelMatrix.TranslationVector;
            var newPosition = currentPosition + directionVec * moveValue;
            var matrix = _sceneNode.ModelMatrix;
            matrix.M41 = newPosition.X;
            matrix.M42 = newPosition.Y;
            matrix.M43 = newPosition.Z;
            _sceneNode.ModelMatrix = matrix;
        }


        private const float _modelMaxPosition = 130.0f;
        private const float _modelMinPosition = -1030.0f;
        /// <summary>
        /// 比例位置
        /// </summary>
        /// <param name="directionVec"></param>
        /// <param name="proportionPosition">根据当前模型兼容的最大-最小的比例位置 </param> 
        public void SetPosition(Vector3 directionVec, float proportionPosition)
        {
            var d = _modelMaxPosition - _modelMinPosition;
            var newPosition = _modelMaxPosition - d * proportionPosition;
            var matrix = _sceneNode.ModelMatrix;
            if (directionVec.X != 0)
            {
                matrix.M41 = newPosition;
            } 
            if (directionVec.Y != 0)
            {
                matrix.M42 = newPosition;
            }
            if (directionVec.Z != 0)
            {
                matrix.M43 = newPosition;
            }
#if DEBUG
            // Debug.WriteLine($"移动位置:{newPosition:F2}");
#endif
            _sceneNode.ModelMatrix = matrix;
        }

        public void Dispose()
        {
            _sceneNode.Dispose();
            DeviceSubscription?.Dispose(); 
            CancelModelMove.Dispose();
        }
        #endregion
    }
}
