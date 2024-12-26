OpcDataCollector
一个基于 C# 的工业数据采集与可视化系统，支持 OPC DA 和以太网协议连接，用于实时数据采集、历史数据存储、报警监控与数据分析。

项目功能
1. 服务器连接
支持 OPC DA 和 以太网 连接。
动态加载连接参数：
OPC DA：配置 OPC 服务器地址。
以太网：配置 IP 地址和端口号。
连接状态实时显示。
2. 实时数据采集
从 OPC DA 或以太网设备中采集实时数据。
动态显示实时数据曲线。
支持数据点数限制，优化图表性能。
3. 数据存储
使用 SQLite 数据库存储历史数据。
支持按时间范围查询历史数据。
4. 报警监控
支持用户自定义报警阈值。
实时监控数据，当数据超出阈值时触发报警。
报警记录存储到数据库，可供后续分析。
5. 数据导出
支持将历史数据导出为 CSV 文件。
安装与运行
1. 环境要求
操作系统：Windows 10 或更高版本
开发环境：Visual Studio 2022 或更高版本
.NET 版本：.NET Framework 4.7.2 或以上
2. 克隆项目
使用以下命令克隆项目到本地：

bash
复制代码
git clone https://github.com/yourusername/OpcDataCollector.git
3. 安装依赖
项目依赖以下库，请确保已安装：

LiveCharts：用于数据可视化
System.Data.SQLite：用于本地数据库操作
安装依赖的 NuGet 包：

bash
复制代码
Install-Package LiveCharts.Wpf
Install-Package System.Data.SQLite
4. 运行项目
在 Visual Studio 中打开项目文件夹。
配置正确的 OPC 服务器地址或以太网设备地址。
按 F5 运行项目。
使用指南
1. 连接设备
进入“服务器连接”页面。
选择连接方式（OPC DA 或以太网）。
输入连接参数并点击“连接”按钮。
确认连接状态显示为“连接成功”。
2. 查看实时数据
成功连接设备后，切换到“实时数据”页面。
实时曲线会动态更新设备的运行数据。
3. 配置报警
在实时数据页面，输入报警阈值。
点击“设置阈值”，系统会监控数据是否超出阈值。
超出阈值时，在报警列表中显示报警信息。
4. 查询历史数据
在“历史数据”页面点击“查询历史数据”按钮。
按时间范围显示存储的历史数据。
5. 导出数据
在“历史数据”页面点击“导出历史数据”按钮。
选择保存路径，生成包含历史数据的 CSV 文件。
项目结构
arduino
复制代码
OpcDataCollector/
│
├── MainWindow.xaml              // 主界面布局
├── MainWindow.xaml.cs           // 主界面逻辑
├── OpcDaClient.cs               // OPC DA 客户端实现
├── DataStorage.cs               // 数据存储模块
├── README.md                    // 项目说明文档
├── DataLog.db                   // SQLite 数据库（运行时生成）
├── Resources/                   // 静态资源文件
└── ...
技术栈
WPF：构建用户界面。
LiveCharts：实现实时数据和历史数据的动态曲线展示。
SQLite：存储历史数据与报警记录。
TcpClient：实现以太网设备的 TCP 通信。
演示截图
1. 服务器连接

2. 实时数据

3. 历史数据

开发计划
 实现 OPC DA 和以太网连接。
 支持实时数据采集与展示。
 支持报警功能与数据存储。
 支持历史数据查询与导出。
 支持 MODBUS 协议连接。
 实现多用户权限管理。
贡献
欢迎提交 Issues 和 Pull Requests 来帮助改进此项目。

