using System.Windows.Input;

namespace MCCS.UserControl
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute;

        /// <summary>
        /// 创建一个始终可执行的命令
        /// </summary>
        /// <param name="execute">执行逻辑</param>
        public RelayCommand(Action<object?> execute) : this(execute, null)
        {
        }
        
        /// <summary>
        /// 创建一个命令
        /// </summary>
        /// <param name="execute">执行逻辑</param>
        /// <param name="canExecute">可执行性判断逻辑</param>
        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// 判断命令是否可执行
        /// </summary>
        /// <param name="parameter">命令参数</param>
        /// <returns>命令是否可执行</returns>
        public bool CanExecute(object? parameter)
        {
            if(_canExecute != null) return _canExecute(parameter);
            return false;
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="parameter">命令参数</param>
        public void Execute(object? parameter)
        {
            _execute(parameter);
        }

        /// <summary>
        /// 可执行性改变事件
        /// </summary>
        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// 手动触发可执行性改变事件
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
