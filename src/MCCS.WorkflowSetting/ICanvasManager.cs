using System.Windows.Controls;

namespace MCCS.WorkflowSetting
{
    public interface ICanvasManager
    {
        void Inititial(Canvas canvas);

        void RenderWorkflowByJson(string json);

        void Add();
    }
}
