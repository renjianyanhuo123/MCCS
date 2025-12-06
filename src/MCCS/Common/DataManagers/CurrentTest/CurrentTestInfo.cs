using MCCS.Infrastructure.TestModels;

namespace MCCS.Common.DataManagers.CurrentTest
{
    public class CurrentTestInfo
    {
        private readonly StateMachine<TestState> _stateMachine;

        public CurrentTestInfo()
        { 
            _stateMachine = new StateMachine<TestState>(TestState.NoStarting);

            _stateMachine.AddTransition(TestState.NoStarting, TestState.Running, OnStarted);
            _stateMachine.AddTransition(TestState.Pause, TestState.Running);
            _stateMachine.AddTransition(TestState.Running, TestState.Pause);
            _stateMachine.AddTransition(TestState.Running, TestState.Stop, OnStoped);
            _stateMachine.AddTransition(TestState.Pause, TestState.Stop, OnStoped);
        }
        /// <summary>
        /// 试验名称
        /// </summary>
        public string Name { get; private set; } = string.Empty;
        /// <summary>
        /// 当前试验开始时间
        /// </summary>
        public DateTimeOffset StartTime { get; private set; }
        /// <summary>
        /// 当前状态
        /// </summary>
        public TestState State => _stateMachine.CurrentState;
        /// <summary>
        /// 结束/终止时间
        /// </summary>
        public DateTimeOffset EndTime { get; private set; }

        /// <summary>
        /// 监听开始后回调的函数
        /// </summary>
        private void OnStarted()
        {
            Name = $"{DateTime.Now:yyyyMMdd hh:mm:sss}";
            StartTime = DateTimeOffset.Now;
        }

        private void OnStoped()
        {
            EndTime = DateTimeOffset.Now;
            // TODO：开始把当前实验信息存储到本地
        }

        /// <summary>
        /// 开始
        /// </summary>
        public bool Start()
        { 
            return _stateMachine.TryTransition(TestState.Running);
        }

        /// <summary>
        /// 暂停
        /// </summary>
        public bool Pause()
        {
            return _stateMachine.TryTransition(TestState.Pause);
        }

        /// <summary>
        /// 继续
        /// </summary>
        public bool Continue()
        {
            return _stateMachine.TryTransition(TestState.Running);
        }

        /// <summary>
        /// 停止/终止
        /// </summary>
        public bool Stop()
        {
            return _stateMachine.TryTransition(TestState.Stop);
        }
    }
}
