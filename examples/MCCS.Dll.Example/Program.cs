using Newtonsoft.Json;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

using MCCS.Station.DllNative;
using MCCS.Station.DllNative.Models;

namespace POPNetCtrlConsoleTest
{
    class Program
    {
        private static IntPtr _deviceHandle = IntPtr.Zero;
        private static bool _isRunning = false;
        private static Thread? _dataMonitorThread;

        static void Main(string[] args)
        {
            Console.Title = "POPNet控制器测试程序";
            Console.WriteLine("========================================");
            Console.WriteLine("POPNet控制器 C# 控制台测试程序");
            Console.WriteLine("版本: 1.0");
            Console.WriteLine("========================================\n");
            try
            {
                // 初始化DLL
                if (!InitializeDLL())
                {
                    Console.WriteLine("按任意键退出...");
                    Console.ReadKey();
                    return;
                }

                // 连接设备
                if (!ConnectDevice(0))
                {
                    Console.WriteLine("按任意键退出...");
                    Console.ReadKey();
                    return;
                }

                // 启动数据监控线程
                StartDataMonitor();

                // 显示主菜单
                ShowMainMenu();

                // 断开设备连接
                DisconnectDevice();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n发生错误: {ex.Message}");
                Console.WriteLine($"堆栈跟踪: {ex.StackTrace}");
            }
            finally
            {
                Console.WriteLine("\n程序结束，按任意键退出...");
                Console.ReadKey();
            }
        }

        #region 初始化和连接

        /// <summary>
        /// 初始化DLL
        /// </summary>
        static bool InitializeDLL()
        {
            Console.WriteLine("正在初始化DLL...");
            try
            {
                int result = POPNetCtrl.NetCtrl01_Init();
                if (result == AddressContanst.OP_SUCCESSFUL)
                {
                    Console.WriteLine("✓ DLL初始化成功");
                    return true;
                }
                else
                {
                    Console.WriteLine($"✗ DLL初始化失败，错误码: {result}");
                    return false;
                }
            }
            catch (DllNotFoundException)
            {
                Console.WriteLine("✗ 找不到 POPNETCtrl01.dll 文件，请确保DLL文件在程序目录或系统路径中");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ DLL初始化异常: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 连接设备
        /// </summary>
        static bool ConnectDevice(int deviceInstance)
        {
            Console.WriteLine($"\n正在连接设备 {deviceInstance}...");
            try
            {
                int t = POPNetCtrl.NetCtrl01_Init();
                Thread.Sleep(1000);
                
                for (var t1 = 0; 1 <= 3;t1++) 
                {
                    int result = POPNetCtrl.NetCtrl01_ConnectToDev(t1, ref _deviceHandle);
                    if (result == AddressContanst.OP_SUCCESSFUL)
                    {
                        Console.WriteLine($"✓ 设备连接成功，句柄: 0x{_deviceHandle:X}");

                        // 读取设备版本信息
                        ReadDeviceVersion();

                        return true;
                    }
                    else
                    {
                        Console.WriteLine($"✗ 设备连接失败，错误码: {result}");
                        Console.WriteLine(GetErrorDescription(result));
                        // return false;
                    }
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ 设备连接异常: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 断开设备连接
        /// </summary>
        static void DisconnectDevice()
        {
            if (_deviceHandle != IntPtr.Zero)
            {
                Console.WriteLine("\n正在断开设备连接...");

                // 停止数据监控
                _isRunning = false;
                if (_dataMonitorThread != null && _dataMonitorThread.IsAlive)
                {
                    _dataMonitorThread.Join(1000);
                }

                // 软件退出（关闭阀台，DA=0）
                POPNetCtrl.NetCtrl01_Soft_Ext(_deviceHandle);

                int result = POPNetCtrl.NetCtrl01_DisConnectToDev(_deviceHandle);
                if (result == AddressContanst.OP_SUCCESSFUL)
                {
                    Console.WriteLine("✓ 设备断开成功");
                }
                else
                {
                    Console.WriteLine($"✗ 设备断开失败，错误码: {result}");
                }

                _deviceHandle = IntPtr.Zero;
            }
        }

        #endregion

        #region 主菜单

        static void ShowMainMenu()
        {
            while (true)
            {
                Console.WriteLine("\n========================================");
                Console.WriteLine("主菜单");
                Console.WriteLine("========================================");
                Console.WriteLine("1. 参数读写测试");
                Console.WriteLine("2. 控制命令测试");
                Console.WriteLine("3. 传感器数据读取");
                Console.WriteLine("4. 静态控制测试");
                Console.WriteLine("5. 动态控制测试");
                Console.WriteLine("6. IO控制测试");
                Console.WriteLine("7. 保护参数设置");
                Console.WriteLine("8. Flash操作");
                Console.WriteLine("9. 系统信息查询");
                Console.WriteLine("0. 退出程序");
                Console.WriteLine("========================================");
                Console.Write("请选择操作 (0-9): ");

                string? input = Console.ReadLine();
                if (string.IsNullOrEmpty(input))
                {
                    Console.WriteLine("无效的选择，请重新输入");
                    continue;
                }

                switch (input)
                {
                    case "1":
                        TestParameterReadWrite();
                        break;
                    case "2":
                        TestControlCommands();
                        break;
                    case "3":
                        TestSensorDataRead();
                        break;
                    case "4":
                        TestStaticControl();
                        break;
                    case "5":
                        TestDynamicControl();
                        break;
                    case "6":
                        TestIOControl();
                        break;
                    case "7":
                        TestProtectionParameters();
                        break;
                    case "8":
                        TestFlashOperations();
                        break;
                    case "9":
                        TestSystemInfo();
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("无效的选择，请重新输入");
                        break;
                }
            }
        }

        #endregion

        #region 测试功能

        /// <summary>
        /// 参数读写测试
        /// </summary>
        static void TestParameterReadWrite()
        {
            Console.WriteLine("========================================");
            Console.WriteLine("参数读写测试");
            Console.WriteLine("========================================\n");

            // 1. 读写AI采样频率
            Console.WriteLine("1. 测试AI采样频率读写...");
            int result = POPNetCtrl.NetCtrl01_AskReadAddr(_deviceHandle, AddressContanst.Addr_AI_SampleRate);
            Thread.Sleep(50); // 等待数据返回

            int sampleRate = 0;
            result = POPNetCtrl.NetCtrl01_nReadAddrVal(_deviceHandle, AddressContanst.Addr_AI_SampleRate, ref sampleRate);
            if (result == AddressContanst.OP_SUCCESSFUL)
            {
                Console.WriteLine($"   当前AI采样频率: {sampleRate} Hz");
            }

            // 写入新的采样频率
            int newSampleRate = 1000;
            result = POPNetCtrl.NetCtrl01_nWriteAddr(_deviceHandle, AddressContanst.Addr_AI_SampleRate, newSampleRate);
            if (result == AddressContanst.OP_SUCCESSFUL)
            {
                Console.WriteLine($"   ✓ 设置AI采样频率为: {newSampleRate} Hz");
            }

            // 2. 读写AI通道使能
            Console.WriteLine("\n2. 测试AI通道1使能状态...");
            result = POPNetCtrl.NetCtrl01_AskReadAddr(_deviceHandle, AddressContanst.Addr_AI_Enabled);
            Thread.Sleep(50);

            byte enabled = 0;
            result = POPNetCtrl.NetCtrl01_bReadAddrVal(_deviceHandle, AddressContanst.Addr_AI_Enabled, ref enabled);
            if (result == AddressContanst.OP_SUCCESSFUL)
            {
                Console.WriteLine($"   当前AI通道1使能状态: {(enabled == 1 ? "启用" : "禁用")}");
            }

            // 3. 读写传感器极性
            Console.WriteLine("\n3. 测试传感器极性...");
            result = POPNetCtrl.NetCtrl01_AskReadAddr(_deviceHandle, AddressContanst.Addr_AI_Polarity);
            Thread.Sleep(50);

            byte polarity = 0;
            result = POPNetCtrl.NetCtrl01_bReadAddrVal(_deviceHandle, AddressContanst.Addr_AI_Polarity, ref polarity);
            if (result == AddressContanst.OP_SUCCESSFUL)
            {
                Console.WriteLine($"   当前传感器极性: {(polarity == 0 ? "正极性" : "负极性")}");
            }

            // 4. 读写软件增益
            Console.WriteLine("\n4. 测试软件增益...");
            result = POPNetCtrl.NetCtrl01_AskReadAddr(_deviceHandle, AddressContanst.Addr_AI_SoftG);
            Thread.Sleep(50);

            float softG = 0;
            result = POPNetCtrl.NetCtrl01_fReadAddrVal(_deviceHandle, AddressContanst.Addr_AI_SoftG, ref softG);
            if (result == AddressContanst.OP_SUCCESSFUL)
            {
                Console.WriteLine($"   当前软件增益: {softG}");
            }

            // 写入新的软件增益
            float newSoftG = 1.5f;
            result = POPNetCtrl.NetCtrl01_fWriteAddr(_deviceHandle, AddressContanst.Addr_AI_SoftG, newSoftG);
            if (result == AddressContanst.OP_SUCCESSFUL)
            {
                Console.WriteLine($"   ✓ 设置软件增益为: {newSoftG}");
            }

            // 5. 读写PID参数
            Console.WriteLine("\n5. 测试静态力控PID参数...");
            result = POPNetCtrl.NetCtrl01_AskReadAddr(_deviceHandle, AddressContanst.Addr_N_S_SP);
            Thread.Sleep(50);

            float kp = 0;
            result = POPNetCtrl.NetCtrl01_fReadAddrVal(_deviceHandle, AddressContanst.Addr_N_S_SP, ref kp);
            if (result == AddressContanst.OP_SUCCESSFUL)
            {
                Console.WriteLine($"   当前Kp: {kp}");
            }

            Console.WriteLine("\n按任意键返回主菜单...");
            Console.ReadKey();
        }

        /// <summary>
        /// 控制命令测试
        /// </summary>
        static void TestControlCommands()
        {
            Console.WriteLine("========================================");
            Console.WriteLine("控制命令测试");
            Console.WriteLine("========================================\n");

            // 1. 控制方式设置
            Console.WriteLine("1. 设置控制方式...");
            Console.WriteLine("   0=开环  1=静态  2=动态");
            Console.Write("   请输入控制方式: ");
            string? ctrlInput = Console.ReadLine();
            if (byte.TryParse(ctrlInput, out byte ctrlState) && ctrlState <= 2)
            {
                int result = POPNetCtrl.NetCtrl01_Set_SysCtrlstate(_deviceHandle, ctrlState);
                if (result == AddressContanst.OP_SUCCESSFUL)
                {
                    string[] modes = ["开环", "静态", "动态"];
                    Console.WriteLine($"   ✓ 控制方式设置为: {modes[ctrlState]}");
                }
                else
                {
                    Console.WriteLine($"   ✗ 设置失败，错误码: {result}");
                }
            }

            // 2. 阀台控制
            Console.WriteLine("\n2. 阀台控制...");
            Console.Write("   打开阀台? (Y/N): ");
            string? valve = Console.ReadLine();
            if (string.Equals(valve, "Y", StringComparison.OrdinalIgnoreCase))
            {
                int result = POPNetCtrl.NetCtrl01_Set_StationCtrl(_deviceHandle, 1, 0);
                if (result == AddressContanst.OP_SUCCESSFUL)
                {
                    Console.WriteLine("   ✓ 阀台已打开");
                }
                else
                {
                    Console.WriteLine($"   ✗ 阀台打开失败，错误码: {result}");
                }
            }
            else
            {
                int result = POPNetCtrl.NetCtrl01_Set_StationCtrl(_deviceHandle, 0, 0);
                if (result == AddressContanst.OP_SUCCESSFUL)
                {
                    Console.WriteLine("   ✓ 阀台已关闭");
                }
                else
                {
                    Console.WriteLine($"   ✗ 阀台关闭失败，错误码: {result}");
                }
            }

            // 3. 试验开始/停止
            Console.WriteLine("\n3. 试验控制...");
            Console.Write("   启动试验? (Y/N): ");
            string? test = Console.ReadLine() ?? string.Empty;
            byte startState = test.Equals("Y", StringComparison.CurrentCultureIgnoreCase) ? (byte)1 : (byte)0;
            int res = POPNetCtrl.NetCtrl01_Set_TestStartState(_deviceHandle, startState);
            if (res == AddressContanst.OP_SUCCESSFUL)
            {
                Console.WriteLine($"   ✓ 试验{(startState == 1 ? "已启动" : "已停止")}");
            }

            // 4. 软件清零
            Console.WriteLine("\n4. 软件清零...");
            Console.WriteLine("   0=位移通道  1=试验力通道");
            Console.Write("   请选择清零通道: ");
            if (byte.TryParse(Console.ReadLine(), out byte zeroMode) && zeroMode <= 1)
            {
                res = POPNetCtrl.NetCtrl01_Set_offSet(_deviceHandle, zeroMode);
                if (res == AddressContanst.OP_SUCCESSFUL)
                {
                    Console.WriteLine($"   ✓ {(zeroMode == 0 ? "位移" : "力")}通道已清零");
                }
            }

            Console.WriteLine("\n按任意键返回主菜单...");
            Console.ReadKey();
        }

        /// <summary>
        /// 传感器数据读取测试
        /// </summary>
        static void TestSensorDataRead()
        {
            Console.WriteLine("========================================");
            Console.WriteLine("传感器数据读取测试 (按ESC退出)");
            Console.WriteLine("========================================\n");

            Console.WriteLine("正在实时读取传感器数据...\n");

            while (true)
            {
                if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
                {
                    break;
                }

                // 读取试验力
                POPNetCtrl.NetCtrl01_AskReadAddr(_deviceHandle, AddressContanst.Addr_LoadN);
                Thread.Sleep(20);
                float force = 0;
                POPNetCtrl.NetCtrl01_fReadAddrVal(_deviceHandle, AddressContanst .Addr_LoadN, ref force);

                // 读取位移
                POPNetCtrl.NetCtrl01_AskReadAddr(_deviceHandle, AddressContanst.Addr_LoadS);
                Thread.Sleep(20);
                float displacement = 0;
                POPNetCtrl.NetCtrl01_fReadAddrVal(_deviceHandle, AddressContanst.Addr_LoadS, ref displacement);

                // 读取控制电压
                POPNetCtrl.NetCtrl01_AskReadAddr(_deviceHandle, AddressContanst.Addr_CtrlDA);
                Thread.Sleep(20);
                float voltage = 0;
                POPNetCtrl.NetCtrl01_fReadAddrVal(_deviceHandle, AddressContanst.Addr_CtrlDA, ref voltage);

                // 读取AI原始值
                POPNetCtrl.NetCtrl01_AskReadAddr(_deviceHandle, AddressContanst.Addr_AI_Value);
                Thread.Sleep(20);
                float aiValue = 0;
                POPNetCtrl.NetCtrl01_fReadAddrVal(_deviceHandle, AddressContanst.Addr_AI_Value, ref aiValue);

                // 读取SSI位移值
                POPNetCtrl.NetCtrl01_AskReadAddr(_deviceHandle, AddressContanst.Addr_SSI_Val);
                Thread.Sleep(20);
                float ssiValue = 0;
                POPNetCtrl.NetCtrl01_fReadAddrVal(_deviceHandle, AddressContanst.Addr_SSI_Val, ref ssiValue);

                // 清屏并显示数据
                Console.SetCursorPosition(0, 5);
                Console.WriteLine($"试验力:      {force,10:F3} kN    ");
                Console.WriteLine($"位移:        {displacement,10:F3} mm    ");
                Console.WriteLine($"控制电压:    {voltage,10:F3} V     ");
                Console.WriteLine($"AI原始值:    {aiValue,10:F3}       ");
                Console.WriteLine($"SSI位移值:   {ssiValue,10:F3} mm    ");
                Console.WriteLine("\n按 ESC 键退出...");

                Thread.Sleep(100);
            }

            Console.WriteLine("\n按任意键返回主菜单...");
            Console.ReadKey();
        }

        /// <summary>
        /// 静态控制测试
        /// </summary>
        static void TestStaticControl()
        {
            Console.WriteLine("========================================");
            Console.WriteLine("静态控制测试");
            Console.WriteLine("========================================\n");

            Console.WriteLine("静态控制模式:");
            Console.WriteLine("  0 = 开环控制");
            Console.WriteLine("  1 = 等速力控制");
            Console.WriteLine("  2 = 等速位移控制");
            Console.WriteLine("  3 = 力保持");
            Console.WriteLine("  4 = 位移保持");
            Console.WriteLine("  5 = 力控位移目标");
            Console.WriteLine("  6 = 位移控力目标");

            Console.Write("\n请选择控制模式 (0-6): ");
            if (!uint.TryParse(Console.ReadLine(), out uint ctrlMode) || ctrlMode > 6)
            {
                Console.WriteLine("无效的控制模式");
                Console.ReadKey();
                return;
            }

            Console.Write("请输入速度 (kN/s 或 mm/s): ");
            if (!float.TryParse(Console.ReadLine(), out float velocity))
            {
                Console.WriteLine("无效的速度值");
                Console.ReadKey();
                return;
            }

            Console.Write("请输入目标值 (kN 或 mm): ");
            if (!float.TryParse(Console.ReadLine(), out float target))
            {
                Console.WriteLine("无效的目标值");
                Console.ReadKey();
                return;
            }

            // 设置控制方式为静态
            int result = POPNetCtrl.NetCtrl01_Set_SysCtrlstate(_deviceHandle, 1);
            if (result != AddressContanst.OP_SUCCESSFUL)
            {
                Console.WriteLine($"设置控制方式失败，错误码: {result}");
                Console.ReadKey();
                return;
            }

            // 设置静态控制模式
            result = POPNetCtrl.NetCtrl01_S_SetCtrlMod(_deviceHandle, ctrlMode, velocity, target);
            if (result == AddressContanst.OP_SUCCESSFUL)
            {
                Console.WriteLine($"\n✓ 静态控制设置成功");
                Console.WriteLine($"  控制模式: {ctrlMode}");
                Console.WriteLine($"  速度: {velocity}");
                Console.WriteLine($"  目标值: {target}");

                // 启动试验
                Console.Write("\n是否启动试验? (Y/N): ");
                string? startTestInput = Console.ReadLine();
                if (string.Equals(startTestInput, "Y", StringComparison.OrdinalIgnoreCase))
                {
                    result = POPNetCtrl.NetCtrl01_Set_TestStartState(_deviceHandle, 1);
                    if (result == AddressContanst.OP_SUCCESSFUL)
                    {
                        Console.WriteLine("✓ 试验已启动");
                        Console.WriteLine("\n正在监控数据 (按任意键停止)...\n");

                        while (!Console.KeyAvailable)
                        {
                            // 读取当前值
                            POPNetCtrl.NetCtrl01_AskReadAddr(_deviceHandle, AddressContanst.Addr_LoadN);
                            Thread.Sleep(20);
                            float force = 0;
                            POPNetCtrl.NetCtrl01_fReadAddrVal(_deviceHandle, AddressContanst.Addr_LoadN, ref force);

                            POPNetCtrl.NetCtrl01_AskReadAddr(_deviceHandle, AddressContanst.Addr_LoadS);
                            Thread.Sleep(20);
                            float disp = 0;
                            POPNetCtrl.NetCtrl01_fReadAddrVal(_deviceHandle, AddressContanst.Addr_LoadS, ref disp);

                            Console.SetCursorPosition(0, Console.CursorTop - 1);
                            Console.WriteLine($"当前力: {force,8:F3} kN    当前位移: {disp,8:F3} mm    ");

                            Thread.Sleep(100);
                        }
                        Console.ReadKey(true);

                        // 停止试验
                        POPNetCtrl.NetCtrl01_Set_TestStartState(_deviceHandle, 0);
                        Console.WriteLine("\n✓ 试验已停止");
                    }
                }
            }
            else
            {
                Console.WriteLine($"✗ 静态控制设置失败，错误码: {result}");
            }

            Console.WriteLine("\n按任意键返回主菜单...");
            Console.ReadKey();
        }

        /// <summary>
        /// 动态控制测试
        /// </summary>
        static void TestDynamicControl()
        {
            Console.WriteLine("========================================");
            Console.WriteLine("动态控制测试");
            Console.WriteLine("========================================\n");

            Console.WriteLine("波形类型:");
            Console.WriteLine("  0 = 正弦波");
            Console.WriteLine("  1 = 三角波");
            Console.WriteLine("  2 = 方波");
            Console.WriteLine("  3 = 锯齿波");

            Console.Write("\n请选择波形类型 (0-3): ");
            if (!byte.TryParse(Console.ReadLine(), out byte waveShape) || waveShape > 3)
            {
                Console.WriteLine("无效的波形类型");
                Console.ReadKey();
                return;
            }

            Console.Write("请输入中值 (kN 或 mm): ");
            if (!float.TryParse(Console.ReadLine(), out float meanValue))
            {
                Console.WriteLine("无效的中值");
                Console.ReadKey();
                return;
            }

            Console.Write("请输入振幅 (kN 或 mm): ");
            if (!float.TryParse(Console.ReadLine(), out float amplitude))
            {
                Console.WriteLine("无效的振幅");
                Console.ReadKey();
                return;
            }

            Console.Write("请输入频率 (Hz): ");
            if (!float.TryParse(Console.ReadLine(), out float frequency))
            {
                Console.WriteLine("无效的频率");
                Console.ReadKey();
                return;
            }

            Console.Write("请输入循环次数 (0=无限循环): ");
            if (!int.TryParse(Console.ReadLine(), out int cycleCount))
            {
                Console.WriteLine("无效的循环次数");
                Console.ReadKey();
                return;
            }

            Console.Write("控制模式 (0=力控制, 1=位移控制): ");
            if (!byte.TryParse(Console.ReadLine(), out byte ctrlMode) || ctrlMode > 1)
            {
                Console.WriteLine("无效的控制模式");
                Console.ReadKey();
                return;
            }

            // 设置控制方式为动态
            int result = POPNetCtrl.NetCtrl01_Set_SysCtrlstate(_deviceHandle, 2);
            if (result != AddressContanst.OP_SUCCESSFUL)
            {
                Console.WriteLine($"设置控制方式失败，错误码: {result}");
                Console.ReadKey();
                return;
            }

            // 设置动态控制参数
            // tmpCtrlOpt: 控制选项，包括中值调整、起振、停振等
            int ctrlOpt = 0x07; // 0x01=中值允许, 0x02=起振允许, 0x04=停振允许
            float ap = 100.0f;  // 调幅 100%
            float ph = 0.0f;    // 调相 0度

            result = POPNetCtrl.NetCtrl01_Osci_SetWaveInfo(
                0,              // 设备实例号
                meanValue,      // 中值
                amplitude,      // 振幅
                frequency,      // 频率
                waveShape,      // 波形
                ctrlMode,       // 控制模式
                ap,             // 调幅
                ph,             // 调相
                cycleCount,     // 循环次数
                ctrlOpt         // 控制选项
            );

            if (result == AddressContanst.OP_SUCCESSFUL)
            {
                Console.WriteLine($"\n✓ 动态控制参数设置成功");
                Console.WriteLine($"  波形: {new string[] { "正弦波", "三角波", "方波", "锯齿波" }[waveShape]}");
                Console.WriteLine($"  中值: {meanValue}");
                Console.WriteLine($"  振幅: {amplitude}");
                Console.WriteLine($"  频率: {frequency} Hz");
                Console.WriteLine($"  循环次数: {(cycleCount == 0 ? "无限" : cycleCount.ToString())}");
                Console.WriteLine($"  控制模式: {(ctrlMode == 0 ? "力控制" : "位移控制")}");

                // 启动试验
                Console.Write("\n是否启动动态试验? (Y/N): ");
                string? startDynamicTestInput = Console.ReadLine();
                if (string.Equals(startDynamicTestInput, "Y", StringComparison.OrdinalIgnoreCase))
                {
                    result = POPNetCtrl.NetCtrl01_Set_TestStartState(_deviceHandle, 1);
                    if (result == AddressContanst.OP_SUCCESSFUL)
                    {
                        Console.WriteLine("✓ 动态试验已启动");
                        Console.WriteLine("\n正在监控数据 (按 S 停止, A 调幅, P 调相, ESC 退出)...\n");

                        while (true)
                        {
                            if (Console.KeyAvailable)
                            {
                                ConsoleKeyInfo key = Console.ReadKey(true);
                                if (key.Key == ConsoleKey.Escape)
                                {
                                    break;
                                }
                                else if (key.Key == ConsoleKey.S)
                                {
                                    // 停止试验
                                    POPNetCtrl.NetCtrl01_Osci_SetEndState(0, 0); // 立即停止
                                    Console.WriteLine("\n试验停止命令已发送");
                                }
                                else if (key.Key == ConsoleKey.A)
                                {
                                    // 调幅
                                    Console.Write("\n请输入调幅百分比 (0-200): ");
                                    if (float.TryParse(Console.ReadLine(), out float newAp))
                                    {
                                        POPNetCtrl.NetCtrl01_Osci_SetAP(0, newAp);
                                        Console.WriteLine($"调幅设置为: {newAp}%");
                                    }
                                }
                                else if (key.Key == ConsoleKey.P)
                                {
                                    // 调相
                                    Console.Write("\n请输入调相角度 (-360 to 360): ");
                                    if (float.TryParse(Console.ReadLine(), out float newPh))
                                    {
                                        POPNetCtrl.NetCtrl01_Osci_SetPH(0, newPh);
                                        Console.WriteLine($"调相设置为: {newPh}°");
                                    }
                                }
                            }

                            // 读取当前循环次数
                            POPNetCtrl.NetCtrl01_AskReadAddr(_deviceHandle, AddressContanst.Addr_D_TestCycleCount);
                            Thread.Sleep(20);
                            int currentCycle = 0;
                            POPNetCtrl.NetCtrl01_nReadAddrVal(_deviceHandle, AddressContanst.Addr_D_TestCycleCount, ref currentCycle);

                            // 读取极大值和极小值
                            POPNetCtrl.NetCtrl01_AskReadAddr(_deviceHandle, AddressContanst.Addr_CycleMaxLoadN);
                            Thread.Sleep(20);
                            float maxForce = 0;
                            POPNetCtrl.NetCtrl01_fReadAddrVal(_deviceHandle, AddressContanst.Addr_CycleMaxLoadN, ref maxForce);

                            POPNetCtrl.NetCtrl01_AskReadAddr(_deviceHandle, AddressContanst.Addr_CycleMinLoadN);
                            Thread.Sleep(20);
                            float minForce = 0;
                            POPNetCtrl.NetCtrl01_fReadAddrVal(_deviceHandle, AddressContanst.Addr_CycleMinLoadN, ref minForce);

                            Console.SetCursorPosition(0, Console.CursorTop - 1);
                            Console.WriteLine($"循环: {currentCycle,6}    极大值: {maxForce,8:F3}    极小值: {minForce,8:F3}    ");

                            Thread.Sleep(100);
                        }

                        // 停止试验
                        POPNetCtrl.NetCtrl01_Set_TestStartState(_deviceHandle, 0);
                        Console.WriteLine("\n✓ 动态试验已停止");
                    }
                }
            }
            else
            {
                Console.WriteLine($"✗ 动态控制设置失败，错误码: {result}");
            }

            Console.WriteLine("\n按任意键返回主菜单...");
            Console.ReadKey();
        }

        /// <summary>
        /// IO控制测试
        /// </summary>
        static void TestIOControl()
        {
            Console.WriteLine("========================================");
            Console.WriteLine("IO控制测试");
            Console.WriteLine("========================================\n");

            // 读取DI状态
            Console.WriteLine("1. 读取数字输入 (DI) 状态...");
            POPNetCtrl.NetCtrl01_AskReadAddr(_deviceHandle, AddressContanst.Addr_DigIn_InVal);
            Thread.Sleep(50);
            int diValue = 0;
            int result = POPNetCtrl.NetCtrl01_nReadAddrVal(_deviceHandle, AddressContanst.Addr_DigIn_InVal, ref diValue);
            if (result == AddressContanst.OP_SUCCESSFUL)
            {
                Console.WriteLine($"   DI值: 0x{diValue:X4} (二进制: {Convert.ToString(diValue, 2).PadLeft(16, '0')})");
                for (int i = 0; i < 16; i++)
                {
                    bool bitValue = (diValue & (1 << i)) != 0;
                    Console.WriteLine($"   DI{i}: {(bitValue ? "高" : "低")}");
                }
            }

            // 设置DO输出
            Console.WriteLine("\n2. 设置数字输出 (DO)...");
            Console.Write("   请输入DO值 (十六进制，如 00FF): 0x");
            string doInput = Console.ReadLine() ?? string.Empty;
            if (int.TryParse(doInput, System.Globalization.NumberStyles.HexNumber, null, out int doValue))
            {
                result = POPNetCtrl.NetCtrl01_DigOut(_deviceHandle, doValue);
                if (result == AddressContanst.OP_SUCCESSFUL)
                {
                    Console.WriteLine($"   ✓ DO设置成功: 0x{doValue:X4}");
                    Console.WriteLine($"   二进制: {Convert.ToString(doValue, 2).PadLeft(16, '0')}");
                }
                else
                {
                    Console.WriteLine($"   ✗ DO设置失败，错误码: {result}");
                }
            }

            // 读取DO当前值
            Console.WriteLine("\n3. 读取当前DO输出值...");
            POPNetCtrl.NetCtrl01_AskReadAddr(_deviceHandle, AddressContanst.Addr_DigOut_OutVal);
            Thread.Sleep(50);
            int doReadValue = 0;
            result = POPNetCtrl.NetCtrl01_nReadAddrVal(_deviceHandle, AddressContanst.Addr_DigOut_OutVal, ref doReadValue);
            if (result == AddressContanst.OP_SUCCESSFUL)
            {
                Console.WriteLine($"   当前DO值: 0x{doReadValue:X4}");
            }

            Console.WriteLine("\n按任意键返回主菜单...");
            Console.ReadKey();
        }

        /// <summary>
        /// 保护参数设置测试
        /// </summary>
        static void TestProtectionParameters()
        {
            Console.WriteLine("========================================");
            Console.WriteLine("保护参数设置");
            Console.WriteLine("========================================\n");

            // 1. 失控保护
            Console.WriteLine("1. 失控保护设置");
            Console.Write("   启用失控保护? (Y/N): ");
            byte prtEnabled = string.Equals(Console.ReadLine(), "Y", StringComparison.OrdinalIgnoreCase) ? (byte)1 : (byte)0;
            POPNetCtrl.NetCtrl01_bWriteAddr(_deviceHandle, AddressContanst.Addr_PrtPosE_Enabled, prtEnabled);

            if (prtEnabled == 1)
            {
                Console.Write("   最大允许误差 (kN 或 mm): ");
                if (float.TryParse(Console.ReadLine(), out float maxError))
                {
                    POPNetCtrl.NetCtrl01_fWriteAddr(_deviceHandle, AddressContanst.Addr_PrtPosE_MaxE, maxError);
                    Console.WriteLine($"   ✓ 失控保护已启用，最大误差: {maxError}");
                }
            }
            else
            {
                Console.WriteLine("   ✓ 失控保护已禁用");
            }

            // 2. 力保护
            Console.WriteLine("\n2. 力保护设置");
            Console.Write("   启用最大力保护? (Y/N): ");
            byte maxNEnabled = Console.ReadLine()?.ToUpper() == "Y" ? (byte)1 : (byte)0;
            POPNetCtrl.NetCtrl01_bWriteAddr(_deviceHandle, AddressContanst.Addr_PrtSys_NMaxLmt_E, maxNEnabled);

            if (maxNEnabled == 1)
            {
                Console.Write("   最大力保护值 (kN): ");
                if (float.TryParse(Console.ReadLine(), out float maxN))
                {
                    POPNetCtrl.NetCtrl01_fWriteAddr(_deviceHandle, AddressContanst.Addr_PrtSys_NMaxLmt_Val, maxN);
                    Console.WriteLine($"   ✓ 最大力保护: {maxN} kN");
                }
            }

            Console.Write("   启用最小力保护? (Y/N): ");
            byte minNEnabled = Console.ReadLine()?.ToUpper() == "Y" ? (byte)1 : (byte)0;
            POPNetCtrl.NetCtrl01_bWriteAddr(_deviceHandle, AddressContanst.Addr_PrtSys_NMinLmt_E, minNEnabled);

            if (minNEnabled == 1)
            {
                Console.Write("   最小力保护值 (kN): ");
                if (float.TryParse(Console.ReadLine(), out float minN))
                {
                    POPNetCtrl.NetCtrl01_fWriteAddr(_deviceHandle, AddressContanst.Addr_PrtSys_NMinLmt_Val, minN);
                    Console.WriteLine($"   ✓ 最小力保护: {minN} kN");
                }
            }

            // 3. 位移保护
            Console.WriteLine("\n3. 位移保护设置");
            Console.Write("   启用最大位移保护? (Y/N): ");
            byte maxSEnabled = Console.ReadLine()?.ToUpper() == "Y" ? (byte)1 : (byte)0;
            POPNetCtrl.NetCtrl01_bWriteAddr(_deviceHandle, AddressContanst.Addr_PrtSys_SMaxLmt_E, maxSEnabled);

            if (maxSEnabled == 1)
            {
                Console.Write("   最大位移保护值 (mm): ");
                if (float.TryParse(Console.ReadLine(), out float maxS))
                {
                    POPNetCtrl.NetCtrl01_fWriteAddr(_deviceHandle, AddressContanst.Addr_PrtSys_SMaxLmt_Val, maxS);
                    Console.WriteLine($"   ✓ 最大位移保护: {maxS} mm");
                }
            }

            Console.Write("   启用最小位移保护? (Y/N): ");
            byte minSEnabled = Console.ReadLine()?.ToUpper() == "Y" ? (byte)1 : (byte)0;
            POPNetCtrl.NetCtrl01_bWriteAddr(_deviceHandle, AddressContanst.Addr_PrtSys_SMinLmt_E, minSEnabled);

            if (minSEnabled == 1)
            {
                Console.Write("   最小位移保护值 (mm): ");
                if (float.TryParse(Console.ReadLine(), out float minS))
                {
                    POPNetCtrl.NetCtrl01_fWriteAddr(_deviceHandle, AddressContanst.Addr_PrtSys_SMinLmt_Val, minS);
                    Console.WriteLine($"   ✓ 最小位移保护: {minS} mm");
                }
            }

            // 4. 读取保护状态
            Console.WriteLine("\n4. 当前保护状态");
            POPNetCtrl.NetCtrl01_AskReadAddr(_deviceHandle, AddressContanst.Addr_PrtErrState);
            Thread.Sleep(50);
            int errState = 0;
            POPNetCtrl.NetCtrl01_nReadAddrVal(_deviceHandle, AddressContanst.Addr_PrtErrState, ref errState);

            Console.WriteLine($"   保护状态码: 0x{errState:X}");
            if (errState == 0)
            {
                Console.WriteLine("   ✓ 无保护触发");
            }
            else
            {
                if ((errState & 0x01) != 0) Console.WriteLine("   ⚠ 传感器最大力保护触发");
                if ((errState & 0x02) != 0) Console.WriteLine("   ⚠ 传感器最小力保护触发");
                if ((errState & 0x04) != 0) Console.WriteLine("   ⚠ 系统最大力保护触发");
                if ((errState & 0x08) != 0) Console.WriteLine("   ⚠ 系统最小力保护触发");
                if ((errState & 0x10) != 0) Console.WriteLine("   ⚠ 系统最大位移保护触发");
                if ((errState & 0x20) != 0) Console.WriteLine("   ⚠ 系统最小位移保护触发");
                if ((errState & 0x40) != 0) Console.WriteLine("   ⚠ 失控保护触发");
                if ((errState & 0x80) != 0) Console.WriteLine("   ⚠ SSI=0 保护触发");
            }

            Console.WriteLine("\n按任意键返回主菜单...");
            Console.ReadKey();
        }

        /// <summary>
        /// Flash操作测试
        /// </summary>
        static void TestFlashOperations()
        {
            Console.WriteLine("========================================");
            Console.WriteLine("Flash操作");
            Console.WriteLine("========================================\n");

            Console.WriteLine("警告: 写入Flash操作会永久保存参数到控制器");
            Console.Write("确定要将当前参数写入Flash? (Y/N): ");

            string? flashInput = Console.ReadLine();
            if (string.Equals(flashInput, "Y", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("\n正在写入Flash，请稍候...");
                int result = POPNetCtrl.NetCtrl01_WriteToFlash(_deviceHandle);

                if (result == AddressContanst.OP_SUCCESSFUL)
                {
                    Console.WriteLine("✓ Flash写入成功！");
                    Console.WriteLine("  参数已永久保存到控制器");
                }
                else
                {
                    Console.WriteLine($"✗ Flash写入失败，错误码: {result}");
                    Console.WriteLine(GetErrorDescription(result));
                }
            }
            else
            {
                Console.WriteLine("操作已取消");
            }

            Console.WriteLine("\n按任意键返回主菜单...");
            Console.ReadKey();
        }

        /// <summary>
        /// 系统信息查询
        /// </summary>
        static void TestSystemInfo()
        {
            Console.WriteLine("========================================");
            Console.WriteLine("系统信息查询");
            Console.WriteLine("========================================\n");

            // 读取设备版本信息
            ReadDeviceVersion();

            // 读取传感器信息
            Console.WriteLine("\n传感器信息:");
            Console.WriteLine("------------");

            // 力传感器编号
            StringBuilder sensorNo = new StringBuilder(16);
            POPNetCtrl.NetCtrl01_AskReadAddr(_deviceHandle, AddressContanst.Addr_AD_SensorNO);
            Thread.Sleep(50);
            POPNetCtrl.NetCtrl01_strReadAddrVal(_deviceHandle, AddressContanst.Addr_AD_SensorNO, sensorNo, 16);
            Console.WriteLine($"力传感器编号: {sensorNo}");

            // 力传感器单位
            StringBuilder unit = new StringBuilder(8);
            POPNetCtrl.NetCtrl01_AskReadAddr(_deviceHandle, AddressContanst.Addr_AD_Unit);
            Thread.Sleep(50);
            POPNetCtrl.NetCtrl01_strReadAddrVal(_deviceHandle, AddressContanst.Addr_AD_Unit, unit, 8);
            Console.WriteLine($"力传感器单位: {unit}");

            // 位移传感器编号
            StringBuilder ssiSensorNo = new StringBuilder(16);
            POPNetCtrl.NetCtrl01_AskReadAddr(_deviceHandle, AddressContanst.Addr_SSI_SensorNO);
            Thread.Sleep(50);
            POPNetCtrl.NetCtrl01_strReadAddrVal(_deviceHandle, AddressContanst.Addr_SSI_SensorNO, ssiSensorNo, 16);
            Console.WriteLine($"位移传感器编号: {ssiSensorNo}");

            // 位移传感器单位
            StringBuilder ssiUnit = new StringBuilder(8);
            POPNetCtrl.NetCtrl01_AskReadAddr(_deviceHandle, AddressContanst.Addr_SSI_Unit);
            Thread.Sleep(50);
            POPNetCtrl.NetCtrl01_strReadAddrVal(_deviceHandle, AddressContanst.Addr_SSI_Unit, ssiUnit, 8);
            Console.WriteLine($"位移传感器单位: {ssiUnit}");

            // 标定日期
            StringBuilder caliDate = new StringBuilder(10);
            POPNetCtrl.NetCtrl01_AskReadAddr(_deviceHandle, AddressContanst.Addr_CaliDate);
            Thread.Sleep(50);
            POPNetCtrl.NetCtrl01_strReadAddrVal(_deviceHandle, AddressContanst.Addr_CaliDate, caliDate, 10);
            Console.WriteLine($"标定日期: {caliDate}");

            // 读取系统状态
            Console.WriteLine("\n系统状态:");
            Console.WriteLine("------------");

            POPNetCtrl.NetCtrl01_AskReadAddr(_deviceHandle, AddressContanst.Addr_SysState);
            Thread.Sleep(50);
            byte sysState = 0;
            POPNetCtrl.NetCtrl01_bReadAddrVal(_deviceHandle, AddressContanst.Addr_SysState, ref sysState);
            Console.WriteLine($"系统状态: {sysState}");

            POPNetCtrl.NetCtrl01_AskReadAddr(_deviceHandle, AddressContanst.Addr_SysCtrlstate);
            Thread.Sleep(50);
            byte ctrlState = 0;
            POPNetCtrl.NetCtrl01_bReadAddrVal(_deviceHandle, AddressContanst.Addr_SysCtrlstate, ref ctrlState);
            string[] ctrlModes = { "开环", "静态", "动态" };
            Console.WriteLine($"控制方式: {(ctrlState < 3 ? ctrlModes[ctrlState] : "未知")}");

            POPNetCtrl.NetCtrl01_AskReadAddr(_deviceHandle, AddressContanst.Addr_StationState);
            Thread.Sleep(50);
            byte stationState = 0;
            POPNetCtrl.NetCtrl01_bReadAddrVal(_deviceHandle, AddressContanst.Addr_StationState, ref stationState);
            Console.WriteLine($"阀台状态: {(stationState == 1 ? "打开" : "关闭")}");

            POPNetCtrl.NetCtrl01_AskReadAddr(_deviceHandle, AddressContanst.Addr_StartState);
            Thread.Sleep(50);
            byte startState = 0;
            POPNetCtrl.NetCtrl01_bReadAddrVal(_deviceHandle, AddressContanst.Addr_StartState, ref startState);
            Console.WriteLine($"试验状态: {(startState == 1 ? "运行中" : "停止")}");

            // 读取控制频率
            POPNetCtrl.NetCtrl01_AskReadAddr(_deviceHandle, AddressContanst.Addr_CtrlTime);
            Thread.Sleep(50);
            float ctrlTime = 0;
            POPNetCtrl.NetCtrl01_fReadAddrVal(_deviceHandle, AddressContanst.Addr_CtrlTime, ref ctrlTime);
            Console.WriteLine($"控制频率: {ctrlTime} ms ({1000.0f / ctrlTime:F1} Hz)");

            Console.WriteLine("\n按任意键返回主菜单...");
            Console.ReadKey();
        }
        #endregion

        #region 辅助功能

        /// <summary>
        /// 读取设备版本信息
        /// </summary>
        static void ReadDeviceVersion()
        {
            Console.WriteLine("设备版本信息:");
            Console.WriteLine("------------");

            // 读取控制器固件版本
            StringBuilder elfVer = new StringBuilder(16);
            int result = POPNetCtrl.NetCtrl01_AskReadAddr(_deviceHandle, AddressContanst.Addr_ELF_Ver);
            Thread.Sleep(50);
            result = POPNetCtrl.NetCtrl01_strReadAddrVal(_deviceHandle, AddressContanst.Addr_ELF_Ver, elfVer, 16);
            if (result == AddressContanst.OP_SUCCESSFUL)
            {
                Console.WriteLine($"控制器固件版本: {elfVer}");
            }

            // 读取DLL版本
            StringBuilder dllVer = new StringBuilder(16);
            result = POPNetCtrl.NetCtrl01_AskReadAddr(_deviceHandle, AddressContanst.Addr_DLL_Ver);
            Thread.Sleep(50);
            result = POPNetCtrl.NetCtrl01_strReadAddrVal(_deviceHandle, AddressContanst.Addr_DLL_Ver, dllVer, 16);
            if (result == AddressContanst.OP_SUCCESSFUL)
            {
                Console.WriteLine($"DLL版本: {dllVer}");
            }

            // 读取POPNet版本
            StringBuilder popnetVer = new StringBuilder(16);
            result = POPNetCtrl.NetCtrl01_AskReadAddr(_deviceHandle, AddressContanst.Addr_POPNET_Ver);
            Thread.Sleep(50);
            result = POPNetCtrl.NetCtrl01_strReadAddrVal(_deviceHandle, AddressContanst.Addr_POPNET_Ver, popnetVer, 16);
            if (result == AddressContanst.OP_SUCCESSFUL)
            {
                Console.WriteLine($"POPNet硬件版本: {popnetVer}");
            }
        }

        /// <summary>
        /// 启动数据监控线程
        /// </summary>
        static void StartDataMonitor()
        {
            _isRunning = true;
            _dataMonitorThread = new Thread(DataMonitorProc);
            _dataMonitorThread.IsBackground = true;
            _dataMonitorThread.Start();
        }
        // 打印数组数据
        private static void PrintADInfoArray(TNet_ADHInfo[] infoArray)
        {
            for (int i = 0; i < infoArray.Length; i++)
            {
                Console.WriteLine($"\n=== AD信息 #{i + 1} ===");
                PrintSingleADInfo(infoArray[i]);
            }
        }

        private static void PrintSingleADInfo(TNet_ADHInfo info)
        {
            Console.WriteLine($"Net_AD_N: [{string.Join(", ", info.Net_AD_N)}]");
            Console.WriteLine($"Net_AD_S: [{string.Join(", ", info.Net_AD_S)}]");
            Console.WriteLine($"Net_PosVref: {info.Net_PosVref}");
            Console.WriteLine($"Net_PosE: {info.Net_PosE}");
            Console.WriteLine($"Net_CtrlDA: {info.Net_CtrlDA}");
            Console.WriteLine($"Net_CycleCount: {info.Net_CycleCount}");
            Console.WriteLine($"Net_SysState: {info.Net_SysState}");
            Console.WriteLine($"Net_DIVal: {info.Net_DIVal}");
            Console.WriteLine($"Net_DOVal: {info.Net_DOVal}");
            Console.WriteLine($"NEt_D_PosVref: {info.Net_D_PosVref}");
            Console.WriteLine($"Net_FeedLoadN: {info.Net_FeedLoadN}");
            Console.WriteLine($"Net_PrtErrState: {info.Net_PrtErrState}");
            Console.WriteLine($"Net_TimeCnt: {info.Net_TimeCnt}");
        }
        /// <summary>
        /// 数据监控线程
        /// </summary>
        static void DataMonitorProc()
        {
            while (_isRunning)
            {
                try
                {
                    // 检查是否有高速数据
                    uint dataCount1 = 1;
                    var stopwatch = Stopwatch.StartNew();
                    uint dataCount = 0;
                    int result = POPNetCtrl.NetCtrl01_GetAD_HDataCount(_deviceHandle, ref dataCount);
                    Console.WriteLine($"当前数据数量: {dataCount}, 返回码: {result}");
                    //if (result == AddressContanst.OP_SUCCESSFUL && dataCount > 0)
                    //{

                    //}
                    // 计算单个结构体大小
                    int structSize = Marshal.SizeOf(typeof(TNet_ADHInfo));
                    uint totalSize = (uint)(structSize * dataCount1);

                    // 分配非托管内存
                    IntPtr buffer = BufferPool.Rent();
                    try
                    {
                        // 初始化内存
                        for (int i = 0; i < dataCount1; i++)
                        {
                            TNet_ADHInfo initData = new TNet_ADHInfo();
                            IntPtr structPtr = IntPtr.Add(buffer, i * structSize);
                            Marshal.StructureToPtr(initData, structPtr, false);
                        }
                        for (int j = 0; j < dataCount; j++)
                        {
                            // 调用DLL函数
                            int result1 = POPNetCtrl.NetCtrl01_GetAD_HInfo(
                                _deviceHandle,
                                buffer,
                                totalSize
                            );
                            if (result1 == 0)
                            {
                                Console.WriteLine("操作成功");

                                // 从内存中读取数据
                                TNet_ADHInfo[] resultArray = new TNet_ADHInfo[dataCount1];
                                for (int i = 0; i < dataCount1; i++)
                                {
                                    IntPtr structPtr = IntPtr.Add(buffer, i * structSize);
                                    resultArray[i] = Marshal.PtrToStructure<TNet_ADHInfo>(structPtr);
                                }
                                Console.WriteLine($"当前接受数据：{dataCount1}");
                                //PrintADInfoArray(resultArray);
                            }
                            else
                            {
                                // Console.WriteLine($"操作失败，错误码: {result}");
                            }
                        } 
                    }
                    finally
                    {
                        // 归还内存
                        BufferPool.Return(buffer);
                    }
                    stopwatch.Stop();
                    Console.WriteLine($"执行时间: {stopwatch.ElapsedMilliseconds}ms");

                    Thread.Sleep(1);
                }
                catch (Exception ex)
                {
                    // 记录错误但不中断线程
                    Console.WriteLine($"数据监控线程错误: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 获取错误描述
        /// </summary>
        static string GetErrorDescription(int errorCode)
        {
            switch (errorCode)
            {
                case AddressContanst.OP_SUCCESSFUL:
                    return "操作成功";
                case AddressContanst.DEVICE_NOT_CONNECTED:
                    return "设备未连接";
                case AddressContanst.DEVICE_NOT_OPENED:
                    return "设备未打开";
                case AddressContanst.DEVICE_BUSY:
                    return "设备忙";
                case AddressContanst.DEVICE_NOT_RESPONDING:
                    return "设备无响应";
                case AddressContanst.OP_DATA_ERROR:
                    return "数据错误";
                case AddressContanst.CRC_ERROR:
                    return "CRC校验错误";
                case AddressContanst.INVALID_LEN_RETURNED:
                    return "返回数据长度无效";
                case AddressContanst.INVALID_REQUEST:
                    return "无效的请求";
                case AddressContanst.DEVICE_NOT_EXIST:
                    return "设备不存在";
                default:
                    return $"未知错误 (代码: {errorCode})";
            }
        }

        #endregion
    }
}