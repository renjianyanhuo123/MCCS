using System.Windows.Media;

namespace MCCS.Models.MethodManager
{
    public enum StepTypeEnum
    {
        Cycle,
        Decision,
        Delay
    }

    public class WorkflowSettingItemModel : BindableBase
    {
        public long Id { get; set; }

        private string _name = string.Empty; 
        public string Name
        {
            get => _name; 
            set => SetProperty(ref _name, value);
        }

        private string _description = string.Empty;
        public string Description
        {
            get => _description; 
            set => SetProperty(ref _description, value);
        }

        private string _iconStr = string.Empty;
        public string IconStr
        {
            get => _iconStr;
            set => SetProperty(ref _iconStr, value);
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        private Brush _iconBackground = Brushes.Transparent; 
        public Brush IconBackground
        {
            get => _iconBackground;
            set => SetProperty(ref _iconBackground, value);
        }

        private StepTypeEnum _stepType;
        public StepTypeEnum StepType
        {
            get => _stepType;
            set => SetProperty(ref _stepType, value);
        }
    }
}
