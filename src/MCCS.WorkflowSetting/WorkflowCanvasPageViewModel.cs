using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using MCCS.WorkflowSetting.Components.ViewModels;
using MCCS.WorkflowSetting.Models.Edges;
using MCCS.WorkflowSetting.Models.Nodes;

namespace MCCS.WorkflowSetting
{
    public class WorkflowCanvasPageViewModel : BindingBase
    {
        private double _canvasWidth = -1;
        private double _canvasHeight = -1;

        public const double _addActionDistance = 35.0;
        private const double _startNodeHeight = 80.0;

        public ObservableCollection<WorkflowConnection> Connections { get; private set; } = [];

        public ObservableCollection<BaseNode> Nodes { get; private set; } = [];

        /// <summary>
        /// 待插入节点的位置后面
        /// </summary>
        public BaseNode? InsertBeforeNode { get; private set; }

        public WorkflowCanvasPageViewModel()
        {
            LoadedCommand = new RelayCommand(ExecuteLoadedCommand, _ => true);
            NodeClickCommand = new RelayCommand(ExecuteNodeClickCommand, _ => true);
        }

        #region Command 
        public ICommand LoadedCommand { get; }

        public ICommand NodeClickCommand { get; }

        #endregion

        #region Private Method
        private void ExecuteLoadedCommand(object? param)
        { 
            if (param is Canvas canvas)
            {
                _canvasWidth = canvas.Width;
                _canvasHeight = canvas.Height; 
            }
            Nodes.Clear();
            Connections.Clear();
            Nodes.Add(new StartNode
            {
                Width = 60,
                Height = 60,
                Type = NodeTypeEnum.Start,
                Name = "Start"
            });
            Nodes.Add(new AddOpNode
            {
                Name = "Add", 
                Type = NodeTypeEnum.Action, 
                Width = 20, 
                Height = 20
            });
            Nodes.Add(new EndNode
            {
                Name = "End",
                Type = NodeTypeEnum.End,
                Width = 80,
                Height = 56
            });
            UpdateNodePosition();
            UpdateConnection();
        }

        private void ExecuteNodeClickCommand(object? param)
        {

        }

        public void UpdateNodePosition()
        {
            for (var i = 0; i < Nodes.Count; i++)
            {
                Nodes[i].Index = i + 1;
                var yDistance = i == 0 ? _startNodeHeight : Nodes[i - 1].Position.Y + Nodes[i - 1].Height + _addActionDistance;
                Nodes[i].Position = new Point(_canvasWidth / 2.0 - Nodes[i].Width / 2.0, yDistance);
            }
        }

        public void UpdateConnection()
        {
            for (var i = 1; i < Nodes.Count; i++)
            {
                var startPoint = Nodes[i - 1].CenterPoint with { Y = Nodes[i - 1].Position.Y + Nodes[i - 1].Height };
                var endPoint = new Point(Nodes[i].CenterPoint.X, Nodes[i].Position.Y);
                if (i <= Connections.Count)
                { 
                    Connections[i - 1].Points = [startPoint, endPoint];
                }
                else
                {
                    Connections.Add(new WorkflowConnection()
                    {
                        Id = Guid.NewGuid().ToString("N"), 
                        Type = startPoint.X == endPoint.X ? ConnectionTypeEnum.Sequential : ConnectionTypeEnum.Conditional,
                        Points = [startPoint, endPoint]
                    });
                }
            }
        }

        #endregion
    }
}
