### 项目介绍
#### (1)MCCS 主项目
#### (2)MCCS.Station 站点模块
#### (3)MCCS.Common.Resources 通用资源界面模块
#### (4)MCCS.Infrastructure 基础设施模块
#### (5)MCCS.WorkflowSetting 工作流设置模块
#### (6)MCCS.WorkFlow.StepComponents 工作流组件模块
#### (7)MCCS.UserControl 用户控件模块
#### (8)MCCS.Controls 自定义控件模块

Station负责“能安全地跑起来”（设备层的可用性、保护、闭环控制、采集）
    Station = 把“硬件复杂性 + 实时控制 + 安全保护 + 采集存储 + 状态管理”封装起来，对上提供“段执行（Segment）+ 状态/事件/数据流”的稳定接口。
Workflow负责“这一试验怎么跑”（流程编排、人机交互、分段组织、结果与报告、审计追溯）

Station设计功能:
1,连接与生命周期管理（Connectivity & Lifecycle）
2,资源注册与拓扑管理（Resource Registry）
3,配置管理（Configuration Management）
4,安全联锁与安全内核（Safety Kernel & Interlocks）
5,限位与保护（Limits & Protection）
6,控制内核与段执行（Control Kernel & Segment Execution）
7,采集与数据记录（Acquisition & Recording）
8,告警、诊断与维护（Alarms & Diagnostics）
10,审计与运行日志（Audit & Trace）
11,仿真与回放（Simulation & Replay）

站点状态机
Offline -> Connecting -> Online -> Ready -> Running -> Ready
                                    ↓         ↓
                                 Faulted/EStop -> Recovering 
维度	        文件	                     状态值
连接与资源	ConnectivityStatus.cs	Disconnected/Connecting/Degraded/Ready
激活与能量	ActivationStatus.cs	    Off/Low/High + 过渡态
运行流程	    ProcessStatus.cs	    Idle/Armed/Running/Paused/Completed等
安全与保护	SafetyStatus.cs	        Normal/Warning/Limited/Interlocked/Failsafe/EStop

StatusAggregator	  四维合成 + 能力计算 + 问题追踪
ProcessStateMachine	  只管流程（Idle→Armed→Running...）
SafetySupervisor	  整合三层保护，独立于状态机
StationHealthService  资源树自底向上计算健康状态
CommandGate	          所有命令统一过闸（放行/改写/拒绝）
StationSafetyContext  整合所有组件的便捷入口

┌─────────────────────────────────────────────────────────────┐
│                   SafetySupervisor (安全主管)                 │
│  - 整合三层保护机制，对状态机有"硬打断/降级/锁定"权限           │
├─────────────────────────────────────────────────────────────┤
│  LimitEngine      │  InterlockEngine  │  EStopMonitor       │
│  (软限位/过程保护)  │  (联锁/条件禁止)   │  (急停/失控保护)     │
│  - 保护试样/过程   │  - 防止危险动作    │  - 保护设备与人身    │
│  - 系统仍可控     │  - 需要复位动作    │  - 必须人工干预      │
└─────────────────────────────────────────────────────────────┘
1. EStop / Failsafe     (最高优先级，任何时候都盖过)
2. Interlocked          (需要清除联锁才能继续)
3. Limited              (软限位/过程保护，部分操作受限)
4. Degraded             (资源缺失/异常)
5. Running/Paused/Idle  (状态机流程)

流程执行（Segment Execution）
1、开始数据采集
2、停止数据采集
3、发送控制端命令

通道阀门仅在满足以下全部条件时才允许开启：

1、液压系统处于允许状态（油源就绪、压力正常）
2、系统未处于急停状态
3、通道无超限、无硬件/软件故障
4、操作员或流程发出明确的通道使能指令

任何单一条件不满足，系统均禁止通道阀门开启。