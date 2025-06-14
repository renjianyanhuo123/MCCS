using MCCS.Core.Models.Model3D;
using System.Windows.Media.Media3D;
using HelixToolkit.SharpDX.Core.Model.Scene;
using MCCS.Common;
using HelixToolkit.SharpDX.Core;
using SharpDX;

namespace MCCS.ViewModels.Others
{
    public class Model3DViewModel : BindableBase
    {
        private bool _isSelected;
        private bool _isHovered; 

        // private HelixToolkit.Wpf.SharpDX.Material _currentMaterial;
        // private Transform3D _transform;
        private readonly Model3DData _model3DData;
        private readonly SceneNode _sceneNode;

        // BillboardText3D相关属性 一般包括: 力 和 位移
        private double _forceNum;
        private double _displacementNum;

        // 性能优化：缓存文本格式
        private const string ForceFormat = "Force: {0:F2} kN";
        private const string DisplacementFormat = "Displacement: {0:F2} mm";

        public Model3DViewModel(SceneNode sceneNode, Model3DData model3DData)
        {
            _model3DData = model3DData;
            _sceneNode = sceneNode;
            UpdateMaterial();
            if (model3DData.Type == ModelType.Actuator) 
            {
                InitializeDataLabels();
            }
            // 为场景节点设置Tag，以便在点击时识别
            _sceneNode.Tag = this; 
            // 为所有子节点也设置Tag
            foreach (var node in _sceneNode.Traverse()) node.Tag = this;
        }

        public Model3DData Model3DData => _model3DData;

        #region BillboardText3D集合 
        public List<TextInfo> DataLabels { get; private set; } = [];

        /// <summary>
        /// 所有点的集合
        /// </summary>
        public Vector3Collection ConnectPoints { get; private set; } = [];

        /// <summary>
        /// 连接线集合
        /// 例如: 0,1,2,3  表示0---1  2---3
        /// </summary>
        public IntCollection ConnectCollection { get; private set; } = [];
        #endregion

        public double ForceNum 
        {
            get => _forceNum;
            set 
            {
                if (_forceNum != value) 
                {
                    _forceNum = value;
                    UpdateLabels(_forceNum, _displacementNum);
                }
            }
        }

        public double DisplacementNum
        {
            get => _displacementNum;
            set 
            {
                if (_displacementNum != value)
                {
                    _displacementNum = value;
                    UpdateLabels(_forceNum, _displacementNum);
                }
            }
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
        private void InitializeDataLabels()
        {
            // 获取模型的边界框来确定标签位置
            var labelOffset = new Vector3(20, 20, 20);
            var center = GetModelCenterFromGeometry();
            // 创建力标签
            var forceLabel = new TextInfoExt
            {
                Text = $"Force: {ForceNum} kN",
                Origin = center + labelOffset,
                Foreground = Color4.White,
                Background = new Color4(0, 0, 0, 0.7f), // 半透明背景
                FontFamily = "Microsoft YaHei",
                Scale = 1f, 
            };

            // 创建位移标签
            var displacementLabel = new TextInfo($"Displacement: {DisplacementNum} mm",  labelOffset + center + new Vector3(0,2,0))
            {
                Foreground = Color4.White,
                Background = new Color4(0, 0, 0, 0.7f), // 半透明背景
                Scale = 1f,
                VerticalAlignment = BillboardVerticalAlignment.Top
            };

            DataLabels.Add(forceLabel);
            DataLabels.Add(displacementLabel);

            // 创建连接线
            CreateConnectionLines(center, labelOffset);
        }

        /// <summary>
        /// 更新当前模型的材质
        /// </summary>
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

        /// <summary>
        /// 创建好当前模型中的连接线
        /// </summary>
        /// <param name="modelCenter"></param>
        /// <param name="labelOffset"></param>
        private void CreateConnectionLines(Vector3 center, Vector3 labelOffset)
        {
            // 以中心为起点
            ConnectPoints.Add(center);
            ConnectPoints.Add(center + labelOffset);
            ConnectCollection.Add(0);
            ConnectCollection.Add(1);
        }

        /// <summary>
        /// 更新当前模型的标签
        /// </summary>
        /// <param name="force"></param>
        /// <param name="displacement"></param>
        private void UpdateLabels(double force, double displacement)
        {
            if (DataLabels.Count >= 2)
            {
                DataLabels[0].Text = string.Format(ForceFormat, force);
                DataLabels[1].Text = string.Format(DisplacementFormat, displacement);
            }
        }

        /// <summary>
        /// 寻找模型的中心
        /// </summary>
        /// <returns></returns>
        private Vector3 GetModelCenterFromGeometry()
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
        #endregion
    }
}
