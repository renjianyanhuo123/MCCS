using System.Diagnostics;

namespace MCCS.Infrastructure.Helper
{
    public class ProcessManager(
        string executablePath,
        string arguments = "",
        string workingDirectory = "",
        bool createNoWindow = true)
        : IDisposable
    {
        private Process? _process;
        private readonly string _executablePath = executablePath ?? throw new ArgumentNullException(nameof(executablePath));
        private readonly CancellationTokenSource _cts = new(); 

        private bool _isDisposed;    

        public event EventHandler<string>? OutputReceived;
        public event EventHandler<string>? ErrorReceived;
        public event EventHandler<int>? ProcessExited; 

        public bool IsRunning => _process is { HasExited: false };

        public void Start()
        {
            if (IsRunning)
                throw new InvalidOperationException("Process already running."); 
            try
            {
                _process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = _executablePath,
                        Arguments = arguments,
                        UseShellExecute = false,
                        RedirectStandardOutput = createNoWindow,
                        RedirectStandardError = createNoWindow,
                        RedirectStandardInput = createNoWindow,
                        CreateNoWindow = createNoWindow,
                        WorkingDirectory = workingDirectory
                    },
                    EnableRaisingEvents = true
                };

                _process.OutputDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
#if DEBUG
                        Debug.WriteLine($"{e.Data}");
#endif
                        OutputReceived?.Invoke(this, e.Data);
                    }
                };

                _process.ErrorDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        ErrorReceived?.Invoke(this, e.Data);
                };

                _process.Exited += (s, e) =>
                {
                    var exitCode = _process?.ExitCode ?? -1;
                    ProcessExited?.Invoke(this, exitCode);
                    //var runtime = (_process.ExitTime - _process.StartTime).TotalMilliseconds;
                    //Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] 退出 | 代码={_process.ExitCode} | 运行={runtime}ms");
                    //if (_shouldRestart && !_cts.IsCancellationRequested)
                    //    await HandleRestartAsync();
                };

                _process.Start();
                _process.BeginOutputReadLine();
                _process.BeginErrorReadLine(); 
            }
            catch
            {
                // ignored
            }
        }

        public void Stop(int timeoutMs = 3000)
        {
            if (!IsRunning) return; 
            try
            {
                // 优雅关闭
                _process!.StandardInput.WriteLine("stop"); 
                if (!_process.WaitForExit(timeoutMs))
                {
                    // 超时强杀
                    _process.Kill(true);
                }
            }
            catch
            {
                _process?.Kill(true);
            }
        }

        public async Task StopAsync(int timeoutMs = 5000)
        {
            if (!IsRunning) return;
            try
            {
                // 优雅关闭
                await _process!.StandardInput.WriteLineAsync("stop"); 
                if (!_process.WaitForExit(timeoutMs))
                {
                    // 超时强杀
                    _process.Kill(true);
                }
            }
            catch
            {
                _process?.Kill(true);
            }
        } 

        public async Task RestartAsync()
        {
            await StopAsync(); 
            Start();
        } 

        private void CleanupProcess()
        {
            if (_process == null) return;
            try
            { 
                if (!_process.HasExited)
                {
                    _process.Kill();
                }

                _process.Dispose();
            }
            catch
            {
                // ignored
            }
            finally
            {
                _process = null;
            }
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true; 
            _cts.Cancel();  
            _cts.Dispose(); 
            CleanupProcess();
        }
    }
}
