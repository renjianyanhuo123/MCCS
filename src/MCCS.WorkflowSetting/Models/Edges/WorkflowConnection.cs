using System.Windows; 

namespace MCCS.WorkflowSetting.Models.Edges
{
    public class WorkflowConnection : BindableBase
    {
        public string Id { get; set; } = string.Empty;
        public ConnectionTypeEnum Type { get; set; }  
        private List<Point> _points = [];
        public List<Point> Points { 
            get => _points;
            set => SetProperty(ref _points, value);
        } 
    }
}
