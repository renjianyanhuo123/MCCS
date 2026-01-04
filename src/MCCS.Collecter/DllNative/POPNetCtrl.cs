using System.Runtime.InteropServices;
using System.Text;

namespace MCCS.Station.DllNative
{
    public static class POPNetCtrl
    { 
        #region DLL函数导入 
        /// <summary>
        /// 动态连接库参数初始化
        /// 读取动态库配置文件,最大设备数 ,定时器时间 ,AD采样频率等
        /// 应用程序启动时，仅调用一次
        /// </summary>
        /// <returns>错误号 =0 无错误  其他预留</returns>
        [DllImport(AddressContanst.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int NetCtrl01_Init();

        /// <summary>
        /// POPNET控制器出厂参数表初始化
        /// </summary>
        /// <param name="hInstance">设备号</param>
        /// <returns>错误号 =0 无错误  其他预留</returns>
        [DllImport(AddressContanst.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int NetCtrl01_PopNet_Init(int hInstance);

        /// <summary>
        /// 设备连接
        /// </summary>
        /// <param name="hInstance">设备号 0…n   第一个控制器为0 ，依次类推</param>
        /// <param name="hDevice">设备句柄---返回值</param>
        /// <returns>=0  操作成功   =1 设备未连接  =2 设备打开错误</returns>
        [DllImport(AddressContanst.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int NetCtrl01_ConnectToDev(int hInstance, ref IntPtr hDevice);

        /// <summary>
        /// 设备断开
        /// </summary>
        /// <param name="hDevice">设备句柄</param>
        /// <returns>=0 操作成功   =1 设备未连接  =2 设备断开错误</returns>
        [DllImport(AddressContanst.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int NetCtrl01_DisConnectToDev(IntPtr hDevice);

        /// <summary>
        /// 读取通讯状态值
        /// </summary>
        /// <param name="hDevice">设备句柄</param>
        /// <param name="outValue">通讯状态值</param>
        /// <returns>=0  操作成功   =1 设备未连接  =2 设备断开错误</returns>
        [DllImport(AddressContanst.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int NetCtrl01_ReadConectState(IntPtr hDevice, ref uint outValue);

        /// <summary>
        /// 获取连续数据在缓存区的数据组数
        /// </summary>
        /// <param name="hDevice">设备句柄</param>
        /// <param name="nRult">数据组数  =0   表示无数据</param>
        /// <returns>=0  操作成功   =1 设备未连接  =2 设备断开错误</returns>
        [DllImport(AddressContanst.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int NetCtrl01_GetAD_HDataCount(IntPtr hDevice, ref uint nRult);

        /// <summary>
        /// 获取一组的连续数据
        /// </summary>
        /// <param name="hDevice">设备句柄</param>
        /// <param name="inBytes1">数据结构   TAD_HInfo</param>
        /// <param name="inBytesSize1">数据结构的字节数</param>
        /// <returns>=0  操作成功   =1 设备未连接  =2 设备断开错误</returns>
        [DllImport(AddressContanst.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int NetCtrl01_GetAD_HInfo(IntPtr hDevice, IntPtr InBytes1, uint inBytesSize1);

        /// <summary>
        /// 向某个地址写浮点类型数据
        /// </summary>
        /// <param name="hDevice">设备句柄</param>
        /// <param name="addr">参数地址</param>
        /// <param name="fVal">需要写入的数值</param>
        /// <returns>=0  操作成功   =1 设备未连接  =2 设备断开错误</returns>
        [DllImport(AddressContanst.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int NetCtrl01_fWriteAddr(IntPtr hDevice, uint addr, float fVal);

        /// <summary>
        /// 向某个地址写整数类型数据
        /// </summary>
        /// <param name="hDevice">设备句柄</param>
        /// <param name="addr">参数地址</param>
        /// <param name="nVal">需要写入的数值</param>
        /// <returns>=0  操作成功   =1 设备未连接  =2 设备断开错误</returns>
        [DllImport(AddressContanst.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int NetCtrl01_nWriteAddr(IntPtr hDevice, uint addr, int nVal);

        /// <summary>
        /// 向某个地址写字节类型数据
        /// </summary>
        /// <param name="hDevice">设备句柄</param>
        /// <param name="addr">参数地址</param>
        /// <param name="nVal">需要写入的数值</param>
        /// <returns>=0  操作成功   =1 设备未连接  =2 设备断开错误</returns>
        [DllImport(AddressContanst.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int NetCtrl01_bWriteAddr(IntPtr hDevice, uint addr, byte nVal);

        /// <summary>
        /// 向某个地址写字符串类型数据
        /// </summary>
        /// <param name="hDevice">设备句柄</param>
        /// <param name="addr">参数地址</param>
        /// <param name="sVal">需要写入的字符串</param>
        /// <param name="sLen">字符串长度</param>
        /// <returns>=0  操作成功   =1 设备未连接  =2 设备断开错误</returns>
        [DllImport(AddressContanst.DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int NetCtrl01_StrWriteAddr(IntPtr hDevice, uint addr, string sVal, uint sLen);

        /// <summary>
        /// 向指定地址发起查询数据请求
        /// </summary>
        /// <param name="hDevice">设备句柄</param>
        /// <param name="addr">需要查询参数的地址</param>
        /// <returns>=0  操作成功   =1 设备未连接  =2 设备断开错误</returns>
        [DllImport(AddressContanst.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int NetCtrl01_AskReadAddr(IntPtr hDevice, uint addr);

        /// <summary>
        /// 所有参数表的值发送到上位机驱动DLL缓存中
        /// </summary>
        /// <param name="hDevice">设备句柄</param>
        /// <returns>=0  操作成功   =1 设备未连接  =2 设备断开错误</returns>
        [DllImport(AddressContanst.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int NetCtrl01_AskReadAllReg(IntPtr hDevice);

        /// <summary>
        /// 读取查询参数的值(浮点类型)
        /// </summary>
        /// <param name="hDevice">设备句柄</param>
        /// <param name="addr">需要查询的参数地址</param>
        /// <param name="fRult">对应参数在控制器中的值</param>
        /// <returns>=0  操作成功   =1 设备未连接  =2 设备断开错误</returns>
        [DllImport(AddressContanst.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int NetCtrl01_fReadAddrVal(IntPtr hDevice, uint addr, ref float fRult);

        /// <summary>
        /// 读取查询参数的值(整型)
        /// </summary>
        /// <param name="hDevice">设备句柄</param>
        /// <param name="inAddr">需要查询的参数地址</param>
        /// <param name="nSult">对应参数在控制器中的值</param>
        /// <returns>=0  操作成功   =1 设备未连接  =2 设备断开错误</returns>
        [DllImport(AddressContanst.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int NetCtrl01_nReadAddrVal(IntPtr hDevice, uint inAddr, ref int nSult);

        /// <summary>
        /// 读取查询参数的值(字节类型)
        /// </summary>
        /// <param name="hDevice">设备句柄</param>
        /// <param name="inAddr">需要查询的参数地址</param>
        /// <param name="nSult">对应参数在控制器中的值</param>
        /// <returns>=0  操作成功   =1 设备未连接  =2 设备断开错误</returns>
        [DllImport(AddressContanst.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int NetCtrl01_bReadAddrVal(IntPtr hDevice, uint inAddr, ref byte nSult);

        /// <summary>
        /// 读取查询参数的值(字符串类型)
        /// </summary>
        /// <param name="hDevice">设备句柄</param>
        /// <param name="addr">需要查询的参数地址</param>
        /// <param name="sRult">对应参数在控制器中的值</param>
        /// <param name="sLen">需要读取字符串长度</param>
        /// <returns>=0  操作成功   =1 设备未连接  =2 设备断开错误</returns>
        [DllImport(AddressContanst.DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int NetCtrl01_strReadAddrVal(IntPtr hDevice, uint addr, StringBuilder sRult, uint sLen);

        /// <summary>
        /// 设置参数写入到Flash中
        /// </summary>
        /// <param name="hDevice">设备句柄</param>
        /// <returns>=0  操作成功   =1 设备未连接  =2 设备断开错误</returns>
        [DllImport(AddressContanst.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int NetCtrl01_WriteToFlash(IntPtr hDevice);

        /// <summary>
        /// 设置静态控制模式
        /// </summary>
        /// <param name="hDevice">设备句柄</param>
        /// <param name="tmpCtrlMode">控制模式</param>
        /// <param name="tmpVelo">速度 mm或kN/秒</param>
        /// <param name="tmpPos">目标值 kN 或mm</param>
        /// <returns>=0  操作成功   =1 设备未连接  =2 设备断开错误</returns>
        [DllImport(AddressContanst.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int NetCtrl01_S_SetCtrlMod(IntPtr hDevice, uint tmpCtrlMode, float tmpVelo, float tmpPos);

        /// <summary>
        /// 设置控制方式 0=开环  1=静态 2=动态
        /// </summary>
        /// <param name="hDevice">设备句柄</param>
        /// <param name="tmpSysCtrlState">控制方式</param>
        /// <returns>=0  操作成功   =1 设备未连接  =2 设备断开错误</returns>
        [DllImport(AddressContanst.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int NetCtrl01_Set_SysCtrlstate(IntPtr hDevice, byte tmpSysCtrlState);

        /// <summary>
        /// 设置阀台状态 0-关闭  1-打开
        /// </summary>
        /// <param name="hDevice">设备句柄</param>
        /// <param name="onOffState">阀台状态</param>
        /// <param name="tmpDOValue">DO操作值</param>
        /// <returns>=0  操作成功   =1 设备未连接  =2 设备断开错误</returns>
        [DllImport(AddressContanst.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int NetCtrl01_Set_StationCtrl(IntPtr hDevice, uint onOffState, uint tmpDOValue);

        /// <summary>
        /// 设置试验开始状态
        /// </summary>
        /// <param name="hDevice">设备句柄</param>
        /// <param name="tmpStartState">=0 停止 =1开始</param>
        /// <returns>=0  操作成功   =1 设备未连接  =2 设备断开错误</returns>
        [DllImport(AddressContanst.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int NetCtrl01_Set_TestStartState(IntPtr hDevice, byte tmpStartState);

        /// <summary>
        /// 控制通道软件清零
        /// </summary>
        /// <param name="hDevice">设备句柄</param>
        /// <param name="tmpCtrlMode">0=位移通道   1=试验力通道</param>
        /// <returns>=0  操作成功   =1 设备未连接  =2 设备断开错误</returns>
        [DllImport(AddressContanst.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int NetCtrl01_Set_offSet(IntPtr hDevice, byte tmpCtrlMode);

        /// <summary>
        /// DigOut 输出
        /// </summary>
        /// <param name="hDevice">设备句柄</param>
        /// <param name="tmpDoVal">输出值    Bit 0  或1</param>
        /// <returns>=0  操作成功   =1 设备未连接  =2 设备断开错误</returns>
        [DllImport(AddressContanst.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int NetCtrl01_DigOut(IntPtr hDevice, int tmpDoVal);

        /// <summary>
        /// 软件退出  关闭阀台 全部DA=0  DO  DigOut_Initial  设定值
        /// </summary>
        /// <param name="hDevice">设备句柄</param>
        /// <returns>=0  操作成功   =1 设备未连接  =2 设备断开错误</returns>
        [DllImport(AddressContanst.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int NetCtrl01_Soft_Ext(IntPtr hDevice);

        /// <summary>
        /// 恢复
        /// </summary>
        /// <param name="hDevice">设备句柄</param>
        /// <returns>=0  操作成功   =1 设备未连接  =2 设备断开错误</returns>
        [DllImport(AddressContanst.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int NetCtrl01_Recovery(IntPtr hDevice);

        #endregion

        #region 动态控制命令
        /// <summary>
        /// 信号发生器波形参数 动态应用动作
        /// </summary>
        /// <param name="hInstance">设备号</param>
        /// <param name="tmpMeanA">中值</param>
        /// <param name="tmpA">幅值</param>
        /// <param name="tmpFreq">频率</param>
        /// <param name="tmpWaveShap">波形</param>
        /// <param name="tmpCtrlMode">控制模式(力、位移)</param>
        /// <param name="tmpAP">调幅值</param>
        /// <param name="tmpPH">调相值</param>
        /// <param name="tmpCountSet">循环次数</param>
        /// <param name="tmpCtrlOpt">控制过程选项  是否中值调整 起停振过程</param>
        /// <returns>=0  操作成功   =1 设备未连接  =2 设备断开错误</returns>
        [DllImport(AddressContanst.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int NetCtrl01_Osci_SetWaveInfo(int hInstance, float tmpMeanA, float tmpA,
            float tmpFreq, byte tmpWaveShap, byte tmpCtrlMode, float tmpAP, float tmpPH,
            int tmpCountSet, int tmpCtrlOpt);

        /// <summary>
        /// 幅值调整 (波形设置后)
        /// </summary>
        /// <param name="hInstance">设备号</param>
        /// <param name="aPercent">幅值百分比</param>
        /// <returns>=0  操作成功   =1 设备未连接  =2 设备断开错误</returns>
        [DllImport(AddressContanst.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int NetCtrl01_Osci_SetAP(int hInstance, float aPercent);

        /// <summary>
        /// 相位调整  设置调相位 -360 --360 (波形设置后)
        /// </summary>
        /// <param name="hInstance">设备号</param>
        /// <param name="tmpDu">相位角度</param>
        /// <returns>=0  操作成功   =1 设备未连接  =2 设备断开错误</returns>
        [DllImport(AddressContanst.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int NetCtrl01_Osci_SetPH(int hInstance, float tmpDu);

        /// <summary>
        /// 设置动态试验停止状态
        /// </summary>
        /// <param name="hInstance">设备号</param>
        /// <param name="tmpEndActMode">0=立即停止    1=停振过程停止</param>
        /// <returns>=0  操作成功   =1 设备未连接  =2 设备断开错误</returns>
        [DllImport(AddressContanst.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int NetCtrl01_Osci_SetEndState(int hInstance, byte tmpEndActMode);

        /// <summary>
        /// 设置暂停状态
        /// </summary>
        /// <param name="hInstance">设备号</param>
        /// <param name="tmpActMode">0=当前位置暂停     1=有停振 起振过程</param>
        /// <param name="tmpHaltState">1=暂停      0=取消暂停</param>
        /// <returns>=0  操作成功   =1 设备未连接  =2 设备断开错误</returns>
        [DllImport(AddressContanst.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int NetCtrl01_Osci_SetHaltState(int hInstance, byte tmpActMode, byte tmpHaltState);

        /// <summary>
        /// 设置波形初始相位
        /// </summary>
        /// <param name="hInstance">设备号</param>
        /// <param name="waveInitPH">初始相位</param>
        /// <returns>=0  操作成功   =1 设备未连接  =2 设备断开错误</returns>
        public static int NetCtrl01_Osci_SetWave_initPH(int hInstance, float waveInitPH)
        {
            return NetCtrl01_fWriteAddr((IntPtr)hInstance, 1232, waveInitPH);
        }

        #endregion

        #region 文件波功能 (2025-3-17)

        /// <summary>
        /// 文件波 设置最大数据点数
        /// </summary>
        /// <param name="hInstance">设备句柄</param>
        /// <param name="tmpMaxDataCount">最大点数</param>
        /// <returns>=0  操作成功   =1 设备未连接  =2 设备断开错误</returns>
        [DllImport(AddressContanst.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int NetCtrl01_SetWaveFMaxDataCount(IntPtr hInstance, int tmpMaxDataCount);

        /// <summary>
        /// 文件波  设置某个数据点的幅值
        /// </summary>
        /// <param name="hInstance">设备句柄</param>
        /// <param name="tmpDataCount">指定点号</param>
        /// <param name="tmpVal">对应波形值</param>
        /// <returns>=0  操作成功   =1 设备未连接  =2 设备断开错误</returns>
        [DllImport(AddressContanst.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int NetCtrl01_SetWaveFDataVal(IntPtr hInstance, int tmpDataCount, float tmpVal);

        /// <summary>
        /// 文件波  读取某个数据点的幅值
        /// </summary>
        /// <param name="hDevice">设备句柄</param>
        /// <param name="tmpDataCount">指定点号</param>
        /// <param name="tmpfVal">返回值</param>
        /// <returns>=0  操作成功   =1 设备未连接  =2 设备断开错误</returns>
        [DllImport(AddressContanst.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int NetCtrl01_GetWaveFDataVal(IntPtr hDevice, int tmpDataCount, ref float tmpfVal);

        /// <summary>
        /// 文件波  读取文件下载状态
        /// operation: 0 = idle; 3: write; 4: verify; 5:successful; 6: fail
        /// </summary>
        /// <param name="hDevice">设备句柄</param>
        /// <param name="nOperation">操作状态</param>
        /// <param name="fPercentage">下载数据百分比</param>
        /// <returns>=0  操作成功   =1 设备未连接  =2 设备断开错误</returns>
        [DllImport(AddressContanst.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int NetCtrl01_GetWaveDownloadState(IntPtr hDevice, ref int nOperation, ref float fPercentage);

        /// <summary>
        /// 文件波   dll 开始数据下传输到POPNET
        /// </summary>
        /// <param name="hDevice">设备句柄</param>
        /// <returns>=0  操作成功   =1 设备未连接  =2 设备断开错误</returns>
        [DllImport(AddressContanst.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int NetCtrl01_SetWaveFStart(IntPtr hDevice);

        /// <summary>
        /// 文件波   dll 数据下传输到POPNET 结束
        /// </summary>
        /// <param name="hDevice">设备句柄</param>
        /// <returns>=0  操作成功   =1 设备未连接  =2 设备断开错误</returns>
        [DllImport(AddressContanst.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int NetCtrl01_SetWaveFEnd(IntPtr hDevice);

        /// <summary>
        /// 动态保护状态清除
        /// </summary>
        /// <param name="hDevice">设备句柄</param>
        /// <returns>=0  操作成功   =1 设备未连接  =2 设备断开错误</returns>
        [DllImport(AddressContanst.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int NetCtrl01_ValleyPeak_Clear(IntPtr hDevice);

        /// <summary>
        /// 字节数组转结构体
        /// </summary>
        public static T BytesToStruct<T>(byte[] bytes) where T : struct
        {
            int size = Marshal.SizeOf<T>();
            if (bytes.Length < size)
                throw new ArgumentException($"Array ({bytes.Length}) < ({size})");

            IntPtr ptr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.Copy(bytes, 0, ptr, size);
                return Marshal.PtrToStructure<T>(ptr);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }
        #endregion
    }
}
