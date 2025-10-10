using System.Windows.Input;
using MCCS.WorkflowSetting.Components.ViewModels;
using MCCS.WorkflowSetting.Models.Nodes;

namespace MCCS.WorkflowSetting
{
    public class WorkflowCanvasPageViewModel : BindingBase
    { 

        /// <summary>
        /// 待插入节点的位置后面
        /// </summary>
        public BaseNode? InsertBeforeNode { get; private set; }

        public WorkflowCanvasPageViewModel()
        { 
        } 
    }
}
