namespace MCCS.Collecter.DllNative
{
    public static class AddressContanst
    {
        #region DLL名称
        // private const string DllName = "POPNETCtrl01.dll";
        // 使用常量
        public const string DllName = @"POPNETCtrl01.dll";
        #endregion

        // POPNet基本参数
        public const int POPPowerONState = 542;        // byte    POPNet 上电状态  上电=0   2024-12-17
        public const int Addr_AI_SampleRate = 20;      // integer 通道采样频率 次/秒
        public const int Addr_AI_Enabled = 24;         // array[1..6] of byte 通道允许  =0 禁止   0或1    Flash
        public const int Addr_AI_Polarity = 30;        // array[1..6] of byte 传感器极性   0-正极性  1-负极性  Flash
        public const int Addr_AI_FilterNum = 36;       // array[1..6] of byte 滤波系数    范围  1-250     Flash
        public const int Addr_AI_HardG = 42;           // array[1..6] of byte 放大倍数      范围 1---100？          Flash
        public const int Addr_AI_MaxValue = 48;        // array[1..6] of single 传感器最大值     Flash
        public const int Addr_AI_MinValue = 72;        // array[1..6] of single 传感器最小值                        Flash
        public const int Addr_AI_MaxLmtValue = 96;     // array[1..6] of single 传感器最大限位值
        public const int Addr_AI_MinLmtValue = 120;    // array[1..6] of single 传感器最小限位值
        public const int Addr_AI_Value0 = 144;         // array[1..6] of single 传感器本身零点                    Flash
        public const int Addr_AI_SoftG = 168;          // array[1..6] of single 软件增益    范围  0.001  to  100      Flash
        public const int Addr_AI_Zero0 = 192;          // array[1..6] of single 软件调零值
        public const int Addr_AI_Val0 = 216;           // array[1..6] of single AD  原始值     软件清零用
        public const int Addr_AI_Voltage = 240;        // array[1..6] of single AD 电压值    范围  -10-10V   标定用
        public const int Addr_AI_Value = 264;          // array[1..6] of single AD最终值   试验力
        public const int Addr_AI_FilterEn = 294;       // array[1..6] of byte AI  端口（5点滤波）滤波允许   1=允许  滤波方式 去掉最大 最小 3点平均   2024-9-14

        // SSI参数
        public const int Addr_SSI_Enabled = 300;       // array[1..2] of Byte 通道允许  =0 禁止     0或1          Flash
        public const int Addr_SSI_Polarity = 302;      // array[1..2] of Byte 传感器极性      0-正极性    1-负极性    Flash
        public const int Addr_SSI_DataType = 304;      // array[1..2] of Byte 传感器数据类型   0-二进制   1-格雷格码  Flash
        public const int Addr_SSI_DataBits = 306;      // array[1..2] of byte 传感器数据位数   范围 20--30     缺省 25  Flash
        public const int Addr_SSI_Senstivity = 308;    // array[1..2] of Single 传感器分辨率         Flash
        public const int Addr_SSI_MaxVelo = 316;       // array[1..2] of single 防干扰最大位移速度    mm/s*1000/控制频率      Flash
        public const int Addr_SSI_CableLength = 324;   // array[1..2] of single 传感器数据线长度                  Flash
        public const int Addr_SSI_MaxValue = 332;      // array[1..2] of single 传感器最大值                      Flash
        public const int Addr_SSI_MinValue = 340;      // array[1..2] of single 传感器最小值                    Flash
        public const int Addr_SSI_MaxLmtValue = 348;   // array[1..2] of single 传感器最大限位值
        public const int Addr_SSI_MinLmtValue = 356;   // array[1..2] of single 传感器最小限位值
        public const int Addr_SSI_SoftG = 364;         // array[1..2] of single 软件增益      范围  0.001  to  100   Flash
        public const int Addr_SSI_SSI0 = 372;          // array[1..2] of integer 原始读数
        public const int Addr_SSI_Value0 = 380;        // array[1..2] of single 传感器本身零点                Flash
        public const int Addr_SSI_Zero0 = 388;         // array[1..2] of single 软件调零值
        public const int Addr_SSI_Val0 = 396;          // array[1..2] of single 原始位移  软件调零用
        public const int Addr_SSI_Val = 404;           // array[1..2] of single 位移采样值
        public const int Addr_SSI_Databak = 412;       // array[1..2] of single
        public const int Addr_SSI_SErrorCount = 420;   // array[1..2] of byte

        // 试验力多点校正
        public const int Addr_AD_Multi_Enabled = 1284;      // byte 允许=1  禁止=0
        public const int Addr_AD_Mult_PNum = 1285;          // byte 标定点数
        public const int Addr_AD_Multi_DspV_0 = 1288;       // single 通道当前显示值 DspV[0]
        public const int Addr_AD_Multi_Standard_0 = 1340;   // single 通道当前标准值 Standard[0]
        public const int Addr_AD_Multi_N = 1392;            // single 校正前的采样值

        // 油压测力通道
        public const int Addr_N_Area = 1396;           // single 油缸无杆腔面积 mm^2
        public const int Addr_Y_Area = 1400;           // single 油缸有杆腔面积 mm^2
        public const int Addr_Mpa_NSoftG = 1404;       // single 油压测力softG
        public const int Addr_MPa_ADN = 1408;          // single 油压力
        public const int Addr_MPa_ADN0 = 1412;         // single 油压力 的软件零点

        // 滤波参数
        public const int Mpa1_FilterNum = 1238;        // byte AI1AI2滤波系数
        public const int Mpa2_FilterNum = 1239;        // byte AI3AI4滤波系数

        // DA输出参数
        public const int Addr_DA_OutVal = 424;         // array[1..4] of single DA 输出
        public const int Addr_DA_OutValBak = 440;      // array[1..4] of single DA 输出备份
        public const int Addr_DA_MaxLmtVal = 456;      // array[1..4] of single DA 输出 最大限位       Flash
        public const int Addr_DA_Polarity = 472;       // array[1..4] of byte DA 极性     Flash
        public const int Addr_DA_DeadU = 488;          // array[1..4] of single DA 死区 输出                Flash

        // 数字IO参数
        public const int Addr_DigOut_Initial = 504;    // integer 初始化  DO输出              Flash
        public const int Addr_DigOut_Stop = 508;       // integer 断网 及故障  DO输出          Flash
        public const int Addr_DigOut_invet = 500;      // integer 控制器上电到软件连接成功时间段 DO 输出    255   2024-9-14
        public const int Addr_DigOut_OutVal = 512;     // integer 正常 DO输出
        public const int Addr_DigIn_InVal = 516;       // integer DI输入
        public const int Addr_DigIn_Count = 520;       // integer DI定时器计数
        public const int Addr_DigIn_CountSet = 524;    // integer DI定时器时间                         Flash

        // 闭环静态控制类参数
        public const int Addr_N_S_SP = 528;            // single 静态试验力PID 控制参数    Flash
        public const int Addr_N_S_SI = 532;            // single 静态试验力PID 控制参数    Flash
        public const int Addr_N_S_SD = 536;            // single 静态试验力PID 控制参数    Flash
        public const int Addr_N_S_FitLevel = 540;      // byte 力控制强度
        public const int Addr_S_S_SP = 544;            // single 静态位移PID 控制参数        Flash
        public const int Addr_S_S_SI = 548;            // single 静态位移PID 控制参数        Flash
        public const int Addr_S_S_SD = 552;            // single 静态位移PID 控制参数        Flash
        public const int Addr_S_S_FitLevel = 556;      // byte 位移控制强度
        public const int Addr_S_CtrlU = 560;           // single 静态控制电压       -1  to 1
        public const int Addr_S_PosVref = 568;         // double 静态命令值
        public const int Addr_S_PosE = 576;            // double 静态控制误差
        public const int Addr_S_DataVref = 584;        // double 加载速度增量
        public const int Addr_S_N_PosSet = 592;        // single 试验力目标值
        public const int Addr_S_N_VeloSet = 596;       // single 试验力加载速度 kN/Sec
        public const int Addr_S_S_PosSet = 600;        // single 位移目标值
        public const int Addr_S_S_VeloSet = 604;       // single 位移加载速度 mm/Sec
        public const int Addr_S_ei = 608;              // single PID 误差   命令值-反馈值
        public const int Addr_S_ei_1 = 612;            // single PID前一次误差
        public const int Addr_S_Isum = 616;            // single PID  积分值
        public const int Addr_S_OpenU = 628;           // Single 开环电压

        // 闭环动态控制类参数
        public const int Addr_N_D_SP = 632;            // single 动态试验力PID 控制参数    Flash
        public const int Addr_N_D_SI = 636;            // single 动态试验力PID 控制参数    Flash
        public const int Addr_N_D_SD = 640;            // single 动态试验力PID 控制参数    Flash
        public const int Addr_N_D_FitLevel = 644;      // byte 力控制强度
        public const int Addr_S_D_SP = 648;            // single 动态位移PID 控制参数        Flash
        public const int Addr_S_D_SI = 652;            // single 动态位移PID 控制参数        Flash
        public const int Addr_S_D_SD = 656;            // single 动态位移PID 控制参数        Flash
        public const int Addr_S_D_FitLevel = 660;      // byte 位移控制强度

        public const int Addr_D_CtrlU = 664;           // single 动态 控制电压       -1  to 1
        public const int Addr_D_PosVref = 672;         // double 动态命令值
        public const int Addr_D_PosE = 680;            // double 动态控制误差
        public const int Addr_D_ei = 688;              // single PID 误差   命令值-反馈值
        public const int Addr_D_ei_1 = 692;            // single PID前一次误差
        public const int Addr_D_Isum = 696;            // single PID  积分值

        public const int Addr_D_TestCycleCount = 712;  // integer 动态循环次数
        public const int Addr_CtrlTime = 716;          // single 闭环控制频率  单位 ms    sysCFG文件中获得
        public const int Addr_S_CtrlMode = 720;        // byte 静态控制模式
        public const int Addr_D_CtrlMode = 721;        // byte 动态控制模式
        public const int Addr_SysCtrlstate = 722;      // byte 控制方式
        public const int Addr_StartState = 723;        // byte 试验开始状态  0 或1
        public const int Addr_StationState = 724;      // byte 阀台状态
        public const int Addr_DynamicEnabled = 725;    // byte 动态允许        0或1
        public const int Addr_SysState = 768;          // byte 系统状态
        public const int Addr_LoadN_CtrlChan = 727;    // byte 力反馈通道      Flash
        public const int Addr_LoadS_CtrlChan = 728;    // byte 位移反馈通道     Flash
        public const int Addr_DA_CtrlChan = 729;       // byte AD 控制通道            Flash
        public const int Addr_LoadN = 732;             // single 闭环反馈力
        public const int Addr_LoadS = 736;             // single 闭环反馈位移
        public const int Addr_LoadMaxN = 740;          // single 闭环反馈通道力最大值
        public const int Addr_LoadMaxS = 744;          // single 闭环反馈通道 位移最大值
        public const int Addr_CtrlDA = 748;            // single 控制电压 -10V-10V
        public const int Addr_PosVref = 752;           // single 闭环命令值
        public const int Addr_PosE = 756;              // single 闭环控制误差
        public const int Addr_ADFrequency = 760;
        public const int Addr_CtrlULimit = 764;        // single 控制电压限位

        // 保护参数
        public const int Addr_PrtPosE_ErrState = 816;     // byte 失控保护状态
        public const int Addr_PrtPosE_Enabled = 817;      // byte 功能允许
        public const int Addr_PrtPosE = 818;
        public const int Addr_PrtPosE_MaxE = 820;         // single 最大允许误差
        public const int Addr_PrtPosE_MaxVSet = 824;      // single 最大位移速度 mm/s
        public const int Addr_PrtPosE_Buf = 828;          // array[0..4] of single 5次连续滤波
        public const int Addr_PrtSys_MheMaxN = 848;       // single 设备允许最大力
        public const int Addr_PrtSys_MheMinN = 852;       // single 设备允许最小力
        public const int Addr_PrtSys_NMaxLmt_E = 856;     // Byte 系统设置最大力保护 允许
        public const int Addr_PrtSys_NMinLmt_E = 857;     // Byte 系统设置最小力保护 允许
        public const int Addr_PrtSys_SMaxLmt_E = 858;     // Byte 系统设置最大位移保护 允许
        public const int Addr_PrtSys_SMinLmt_E = 859;     // Byte 系统设置最小位移保护 允许
        public const int Addr_PrtSys_NMaxLmt_Val = 860;   // single 系统设置最大力保护值
        public const int Addr_PrtSys_NMinLmt_Val = 864;   // single 系统设置最小力保护值
        public const int Addr_PrtSys_SMaxLmt_Val = 868;   // single 系统设置最大位移保护值
        public const int Addr_PrtSys_SMinLmt_Val = 872;   // single 系统设置最小位移保护值
        public const int Addr_StationOFF_DoVal = 876;     // Integer 关闭阀台时的DO值
        public const int Addr_PrtErrState = 880;          // integer 保护状态   =0 无错误    bits 错误类型
        public const int Addr_PrtErrStateBak = 884;

        // 传感器信息
        public const int Addr_AD_SensorNO = 888;       // array[0..15,0..5] of char 力传感器编号        16个字符
        public const int Addr_AD_Unit = 984;           // array[0..7,0..5] of char AD通道物理单位        8个字符
        public const int Addr_SSI_SensorNO = 1032;     // array[0..15,0..1] of char 位移传感器编号        16个字符
        public const int Addr_SSI_Unit = 1064;         // array[0..7,0..1] of char SSI通道物理单位        8个字符
        public const int Addr_CaliDate = 1080;         // array[0..9] of char 传感器标定日期        10个字符
        public const int Addr_ELF_Ver = 1096;          // array[0..15] of char 控制软件固化软件版本号       16个字符
        public const int Addr_DLL_Ver = 1112;          // array[0..15] of char Dll软件版本号       16个字符
        public const int Addr_POPNET_Ver = 1128;       // array[0..15] of char 控制器硬件电路板软件版本号       16个字符

        // 动态控制参数
        public const int Addr_D_PosVref0 = 1144;           // single 标准信号
        public const int Addr_SetN_Adjust = 1148;          // single 调幅调相后信号
        public const int Addr_SetS_Adjust = 1152;          // single 调幅调相后信号
        public const int Addr_Osci_Enbaled = 1156;         // byte 动态允许
        public const int Addr_Osci_HaltStte = 1157;        // byte 暂停
        public const int Addr_Osci_EndState = 1158;        // byte 结束状态
        public const int Addr_Osci_EndCmdState = 1159;     // byte 结束命令

        public const int Addr_Osci_CtrlOpt = 1160;         // Integer 动态控制选项 中值 起振 停振
        public const int Addr_WaveOutValue = 1164;         // single 振幅标准信号输出
        public const int Addr_WaveOutValue_Adjust = 1168;  // Single 调幅调相后振幅输出
        public const int Addr_MeanAOutValuet = 1172;       // single 中值输出

        public const int Addr_Osci_Count = 1176;           // integer 标准信号离散周期计数
        public const int Addr_OsciCtrl_Count = 1180;       // integer 调幅调相信号离散周期计数
        public const int Addr_SoftDebugState = 1184;       // byte 动态模拟调试状态
        public const int Addr_Osi_CtrlStage = 1185;        // byte 动态控制阶段 起振 中值 停振 结束

        public const int Addr_WAVE_Shape = 1186;           // byte 波形
        public const int Addr_WAVE_A = 1188;               // single 波形 振幅
        public const int Addr_WAVE_N = 1192;               // integer 周期
        public const int Addr_WAV_HZ = 1196;               // single 频率
        public const int Addr_WAVE_MeanA = 1200;           // single 中值
        public const int Addr_WAVE_CountSet = 1204;        // integer 循环次数

        public const int Addr_AP_StartStop = 1208;         // single 起振 停振
        public const int Addr_AP_Cmd = 1212;               // single 调幅
        public const int Addr_AP_Adjust = 1216;            // single 调整命令值
        public const int Addr_PH_Cmd = 1220;               // single 调相
        public const int Addr_PH_Adjust = 1224;            // single 调相命令值
        public const int Addr_PH0 = 1228;                  // single 初始相位
        public const int Addr_PH0Set = 1232;               // single 初始相位设定值 -360---360

        public const int Addr_APCmdEnabled = 1236;         // byte 波形起点生效
        public const int Addr_PHCmdEnabled = 1237;         // Byte

        public const int Addr_OsiStartStopTime = 1240;         // single 起振 停振时间
        public const int Addr_MeanA_Velo_N = 1244;             // single 中值调整速度 kN/s
        public const int Addr_MeanA_Velo_S = 1248;             // single 中值调整速度 mm/s

        public const int Addr_OsiStart_CleCount = 1252;        // integer 起振计数
        public const int Addr_OsiStart_CleCountSet = 1256;     // integer 起振次数
        public const int Addr_OsiStart_Enabled = 1260;         // byte 起振允许
        public const int Addr_OsiStart_State = 1261;           // byte 起振状态

        public const int Addr_OsiStop_Enabled = 1262;          // byte 停振允许
        public const int Addr_OsiStop_CleCount = 1264;         // integer 停振计数
        public const int Addr_OsiStop_CleCountSet = 1268;      // integer 停振次数
        public const int Addr_OsiStop_State = 1272;            // byte 停振状态
        public const int Addr_OsiStop_ActMode = 1273;          // byte 停振模式 有误停振过程

        public const int Addr_OsiMean_Enabled = 1274;          // byte 中值允许
        public const int Addr_OsiMean_State = 1275;            // byte 中值状态
        public const int Addr_OsiMean_CurVal = 1276;           // single 中值调整值
        public const int Addr_OsiMean_DateVref = 1280;         // single 中值速度

        // 位移多点校正
        public const int Addr_S_Multi_Enabled = 1436;      // byte 位移多点校正允许
        public const int Addr_S_Mult_PNum = 1437;          // byte 位移多点校正数据点数
        public const int Addr_S_Multi_DspV0 = 1440;        // Single 位移多点校正显示值
        public const int Addr_S_Multi_DspV1 = 1444;        // single 表格显示值
        public const int Addr_S_Multi_DspV2 = 1448;
        public const int Addr_S_Multi_DspV3 = 1452;
        public const int Addr_S_Multi_DspV4 = 1456;
        public const int Addr_S_Multi_DspV5 = 1460;
        public const int Addr_S_Multi_DspV6 = 1464;
        public const int Addr_S_Multi_DspV7 = 1468;
        public const int Addr_S_Multi_DspV8 = 1472;
        public const int Addr_S_Multi_DspV9 = 1476;
        public const int Addr_S_Multi_DspV10 = 1480;
        public const int Addr_S_Multi_DspV11 = 1484;
        public const int Addr_S_Multi_DspV12 = 1488;
        public const int Addr_S_Multi_Standard0 = 1492;    // single 表格标准值
        public const int Addr_S_Multi_Standard1 = 1496;
        public const int Addr_S_Multi_Standard2 = 1500;
        public const int Addr_S_Multi_Standard3 = 1504;
        public const int Addr_S_Multi_Standard4 = 1508;
        public const int Addr_S_Multi_Standard5 = 1512;
        public const int Addr_S_Multi_Standard6 = 1516;
        public const int Addr_S_Multi_Standard7 = 1520;
        public const int Addr_S_Multi_Standard8 = 1524;
        public const int Addr_S_Multi_Standard9 = 1528;
        public const int Addr_S_Multi_Standard10 = 1532;
        public const int Addr_S_Multi_Standard11 = 1536;
        public const int Addr_S_Multi_Standard12 = 1540;
        public const int Addr_S_Multi = 1544;              // single 位移多点校正原始值 校正前

        // 传感器模拟
        public const int Addr_SSI0CheckEnabled = 1548;     // Byte SSI=0 系统保护允许
        public const int Addr_AI_DemoEnabled0 = 1549;      // Byte AI传感器模拟允许
        public const int Addr_AI_DemoEnabled1 = 1550;
        public const int Addr_AI_DemoEnabled2 = 1551;
        public const int Addr_AI_DemoEnabled3 = 1552;
        public const int Addr_AI_DemoEnabled4 = 1553;
        public const int Addr_AI_DemoEnabled5 = 1554;
        public const int Addr_SSI_DemoEnabled0 = 1555;     // Byte SSI传感器模拟允许
        public const int Addr_SSI_DemoEnabled1 = 1556;
        public const int Addr_AI_DemoValue0 = 1560;        // Single AI传感器模拟值
        public const int Addr_AI_DemoValue1 = 1564;
        public const int Addr_AI_DemoValue2 = 1568;
        public const int Addr_AI_DemoValue3 = 1572;
        public const int Addr_AI_DemoValue4 = 1576;
        public const int Addr_AI_DemoValue5 = 1580;
        public const int Addr_SSI_DemoValue0 = 1584;       // Single SSI传感器模拟值
        public const int Addr_SSI_DemoValue1 = 1588;

        // 动态文件波
        public const int Addr_WaveF_dT = 772;              // single 文件波  数据点的时间间隔 秒  >=控制频率
        public const int Addr_WaveF_MaxDataCount = 776;    // integer 文件波 最大数据点      <=60000
        public const int Addr_WaveF_MaxVal = 780;          // single 文件波  最大值
        public const int Addr_WaveF_MinVal = 784;          // single 文件波 最小值
        public const int Addr_WaveF_nP = 788;              // integer 每次控制间隔点
        public const int Addr_WaveF_State = 792;           // byte 文件波状态   =1 表示正常  0=没有数据
        public const int Addr_WaveF_LineEnabled = 793;     // byte 文件波线性差值   =1 允许

        // 动态保护
        public const int Addr_PrtSys_ValleyPeak_LmtE_0 = 1592;     // byte 力极大值上限允许
        public const int Addr_PrtSys_ValleyPeak_LmtE_1 = 1593;     // byte 位移极大值上限允许
        public const int Addr_PrtSys_ValleyPeak_LmtE_2 = 1594;     // byte 力极小值上限 允许
        public const int Addr_PrtSys_ValleyPeak_LmtE_3 = 1595;     // byte 位移极小值上限允许
        public const int Addr_PrtSys_ValleyPeak_LmtE_4 = 1596;     // byte 力极大值下限允许
        public const int Addr_PrtSys_ValleyPeak_LmtE_5 = 1597;     // byte 位移极大值下限允许
        public const int Addr_PrtSys_ValleyPeak_LmtE_6 = 1598;     // byte 力极小值下限允许
        public const int Addr_PrtSys_ValleyPeak_LmtE_7 = 1599;     // byte 位移极小值下限允许
        public const int Addr_PrtSys_ValleyPeak_LmtVal_0 = 1600;   // Single 力极大值上限值
        public const int Addr_PrtSys_ValleyPeak_LmtVal_1 = 1604;   // Single 位移极大值上限值
        public const int Addr_PrtSys_ValleyPeak_LmtVal_2 = 1608;   // Single 力极小值上限值
        public const int Addr_PrtSys_ValleyPeak_LmtVal_3 = 1612;   // Single 位移极小值上限值
        public const int Addr_PrtSys_ValleyPeak_LmtVal_4 = 1616;   // Single 力极大值下限值
        public const int Addr_PrtSys_ValleyPeak_LmtVal_5 = 1620;   // Single 位移极大值下限值
        public const int Addr_PrtSys_ValleyPeak_LmtVal_6 = 1624;   // Single 力极小值下限值
        public const int Addr_PrtSys_ValleyPeak_LmtVal_7 = 1628;   // Single 位移极大值下限值
        public const int Addr_ValleyPeak_dCoun = 1632;              // byte 间隔波形数量  1或2
        public const int Addr_ValleyPeak_FilterNum = 1633;          // byte 连续超出次数
        public const int Addr_CycleMaxLoadN = 1636;                 // single 力极大值
        public const int Addr_CycleMaxLoadS = 1640;                 // single 位移极大值
        public const int Addr_CycleMinLoadN = 1644;                 // single 力极小值
        public const int Addr_CycleMinLoadS = 1648;                 // single 位移极小值
        public const int Addr_ValleyPeak_ErrCode = 1652;            // integer 动态超限状态
        public const int Addr_ValleyPeak_ErrCodeBaK = 1656;         // integer 动态超限状态备份

        // 模拟量输出  DA3--位移  DA4---力
        public const int DAQ_E_0 = 1660;               // byte 位移允许      1=允许
        public const int DAQ_E_1 = 1661;               // byte 力允许         1=允许
        public const int DAQ_Sensitive_0 = 1664;       // single 位移灵敏度  mv/mm
        public const int DAQ_Sensitive_1 = 1668;       // single 力 灵敏度   mv/KN
        public const int DAQ_mv_V0_0 = 1672;           // single 位移 DA3零点        mv
        public const int DAQ_mv_V0_1 = 1676;           // single 力   DA4 零点       mV

        // 返回值参考
        public const int OP_SUCCESSFUL = 0;
        public const int DEVICE_NOT_CONNECTED = 1;
        public const int DEVICE_NOT_OPENED = 2;
        public const int DEVICE_BUSY = 3;
        public const int DEVICE_NOT_RESPONDING = 4;
        public const int OP_DATA_ERROR = 5;
        public const int CRC_ERROR = 6;
        public const int INVALID_LEN_RETURNED = 7;
        public const int INVALID_REQUEST = 8;
        public const int DEVICE_NOT_EXIST = 9;
    }
}
