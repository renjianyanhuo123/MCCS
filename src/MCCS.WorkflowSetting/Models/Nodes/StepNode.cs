using System.Windows;
using System.Windows.Media;
using MCCS.WorkflowSetting.Components;

namespace MCCS.WorkflowSetting.Models.Nodes
{
    public class StepNode : BaseNode
    {
        public StepNode(string name,string title, Brush titleBackground, double width, double height, int level = 0, int order = -1) 
            : base(name, width, height, level, order)
        {
            Type = NodeTypeEnum.Process;
            Title = title;
            TitleBackground = titleBackground;
            Content = new WorkflowStepNode()
            {
                Title = Title,
                TitleBackground = TitleBackground,
            };
            Content.Width = width;
            Content.Height = height;
        }

        public bool IsSettingNode { get; set; } = false;

        /// <summary>
        /// 节点标题
        /// </summary>
        public string Title { get; private set; }
        /// <summary>
        /// 节点背景色
        /// </summary>
        public Brush TitleBackground { get; private set; } 
    }
}
