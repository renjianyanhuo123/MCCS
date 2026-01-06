namespace MCCS.Station.Host
{
    internal class Program
    { 
        private static readonly CancellationTokenSource _cts = new();

        static void ReadCommands()
        {
            while (true)
            {
                var line = Console.ReadLine();
                if (line?.Trim().ToLower() != "stop") continue;
                Console.WriteLine("Stop command received.");
                Cleanup();
                break;
            }
        }

        static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("Application Running!");
            _ = Task.Run(ReadCommands); 
            while (!_cts.Token.IsCancellationRequested)
            {
                await Task.Delay(2000, _cts.Token);
            } 
        } 

        /// <summary>
        /// 清理资源
        /// </summary>
        private static void Cleanup()
        {
            _cts.Cancel();
            _cts.Dispose();
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 资源清理完成");
        }
    }
}
