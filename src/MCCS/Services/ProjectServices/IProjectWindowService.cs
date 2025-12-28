namespace MCCS.Services.ProjectServices
{
    public interface IProjectWindowService
    {
        /// <summary>
        /// 显示弹出窗口并迁移内容
        /// </summary>
        void ShowPopupWindow();

        /// <summary>
        /// 窗口关闭事件
        /// </summary>
        event EventHandler WindowClosed;
    }
}
