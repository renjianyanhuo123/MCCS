using MCCS.Core.Models.Model3D;
using System.Windows.Media.Media3D;
using HelixToolkit.SharpDX.Core.Model.Scene;
using MCCS.Common;
using HelixToolkit.SharpDX.Core; 
using HelixToolkit.Wpf.SharpDX;
using System.Collections.ObjectModel;
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
        private Transform3D _transform;
        private readonly Model3DData _model3DData;
        private readonly SceneNode _sceneNode;

        // BillboardText3D相关属性 一般包括: 力 和 位移
        private List<TextInfo> _dataLabels;
        private List<LineGeometry3D> _connectionLines;

        private double _forceNum;
        private double _displacementNum;

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
            if (_sceneNode != null)
            {
                _sceneNode.Tag = this; 
                // 为所有子节点也设置Tag
                foreach (var node in _sceneNode.Traverse()) node.Tag = this;
            }
        }

        public Model3DData Model3DData => _model3DData;

        #region BillboardText3D集合 
        public List<TextInfo> DataLabels { get; set; } = [];

        public List<LineGeometry3D> ConnectionLines { get; set; } = [];
        #endregion

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
        private void InitializeDataLabels()
        {
            // 获取模型的边界框来确定标签位置
            // X=0.000, Y=-0.136, Z=0.991
            // var offset = 2.0f;
            var labelOffset = new Vector3(0,0,0);

            // 创建力标签
            var forceLabel = new TextInfo($"Force: {ForceNum} kN", labelOffset) 
            {
                Foreground = Color4.White,
                Background = Color4.Black,
                Scale = 1f, 
            };

            // 创建位移标签
            var displacementLabel = new TextInfo($"位移: {DisplacementNum} mm",  labelOffset + new Vector3(0, 20, 0))
            {
                Foreground = Color4.White,
                Background = Color4.Black,
                Scale = 1f,
                VerticalAlignment = BillboardVerticalAlignment.Top
            };

            DataLabels.Add(forceLabel);
            DataLabels.Add(displacementLabel);

            // 创建连接线
            // CreateConnectionLines(modelCenter, labelOffset);
        }

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

        private void CreateConnectionLines(Vector3 modelCenter, Vector3 labelOffset)
        {
            var linePoints = new Vector3[]
            {
                modelCenter,
                modelCenter + new Vector3(labelOffset.X * 0.8f, labelOffset.Y * 0.8f, labelOffset.Z)
            };

            var lineGeometry = new LineGeometry3D
            {
                Positions = new Vector3Collection(linePoints),
                Indices = new IntCollection([0, 1]),
                Colors = []
            };

            ConnectionLines.Add(lineGeometry);
        }

        private void UpdateLabels(double force, double displacement)
        {
            if (DataLabels.Count >= 2)
            {
                DataLabels[0].Text = $"力: {force} kN";
                DataLabels[1].Text = $"位移: {displacement} mm";
            }
        }
        #endregion
    }
}
