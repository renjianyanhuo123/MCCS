using System.Windows;
using MCCS.WorkflowSetting.Components.ViewModels;

namespace MCCS.WorkflowSetting.Models.Edges
{
    public class WorkflowConnection : BindingBase
    {
        public WorkflowConnection()
        {  
        } 

        public string Id { get; set; } 
        public ConnectionTypeEnum Type { get; set; } 
        /// <summary>
        /// 是否已渲染
        /// </summary>
        public bool IsRender { get; set; } = false; 

        private List<Point> _points = [];
        public List<Point> Points { 
            get => _points;
            set => SetProperty(ref _points, value);
        } 
    }
}
