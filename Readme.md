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
