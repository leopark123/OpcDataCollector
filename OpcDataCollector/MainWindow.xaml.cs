using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;

namespace OpcDataCollector
{
    public partial class MainWindow : Window
    {
        private OpcDaClient opcClient; // OPC 客户端，用于连接和读取 PLC 数据
        private DataStorage dataStorage; // 数据库存储操作类
        private Timer timer; // 定时器，用于定时采集数据
        private double threshold = 500; // 默认报警阈值
        private Dictionary<string, ChartValues<double>> dataValues = new Dictionary<string, ChartValues<double>>(); // 实时数据曲线
        private ChartValues<double> predictedValues = new ChartValues<double>(); // 预测值曲线
        private const int MaxPoints = 50; // 图表最多显示的数据点

        public MainWindow()
        {
            InitializeComponent();

            // 初始化图表
            InitializeChart();

            // 异步初始化数据库和 OPC 客户端
            Task.Run(() => InitializeApp());
        }

        /// <summary>
        /// 异步初始化应用程序，包括数据库和 OPC 客户端
        /// </summary>
        private async Task InitializeApp()
        {
            // 并行初始化数据库和 OPC 客户端
            var databaseTask = Task.Run(() => InitializeDatabase());
            var opcClientTask = Task.Run(() => InitializeOpcDaClient());

            await Task.WhenAll(databaseTask, opcClientTask); // 等待两个初始化任务完成

            // 启动数据采集
            StartDataCollection();
        }

        /// <summary>
        /// 初始化 SQLite 数据库
        /// </summary>
        private void InitializeDatabase()
        {
            string databaseFile = "CollectedData.db";
            dataStorage = new DataStorage(databaseFile);
            dataStorage.InitializeDatabase(); // 初始化数据库表结构
            Console.WriteLine("数据库初始化完成！");
        }

        /// <summary>
        /// 初始化 OPC DA 客户端
        /// </summary>
        private void InitializeOpcDaClient()
        {
            opcClient = new OpcDaClient();
            opcClient.Connect("opcda://localhost/RSLinx OPC Server"); // 连接到本地 RSLinx OPC Server
            opcClient.AddItems(new List<string> { "[MyPlc]T3.ACC", "[MyPlc]T4.ACC" }); // 添加监控的 PLC 数据项
            Console.WriteLine("OPC DA 客户端初始化完成！");
        }

        /// <summary>
        /// 初始化图表，设置时间轴、曲线和图例
        /// </summary>
        private void InitializeChart()
        {
            DataChart.LegendLocation = LegendLocation.Right; // 图例显示位置（右侧）

            // 初始化实时数据曲线，每个数据项一条曲线
            foreach (var tag in new List<string> { "[MyPlc]T3.ACC", "[MyPlc]T4.ACC" })
            {
                var chartValues = new ChartValues<double>();
                dataValues[tag] = chartValues;
                DataChart.Series.Add(new LineSeries
                {
                    Title = tag, // 曲线名称
                    Values = chartValues, // 数据绑定
                    LineSmoothness = 0.5, // 曲线平滑度
                    PointGeometry = null // 不显示数据点
                });
            }

            // 初始化预测值曲线
            DataChart.Series.Add(new LineSeries
            {
                Title = "预测值", // 预测曲线名称
                Values = predictedValues,
                LineSmoothness = 0.5,
                StrokeDashArray = new System.Windows.Media.DoubleCollection { 4 }, // 设置虚线
                PointGeometry = null // 不显示数据点
            });

            Console.WriteLine("图表初始化完成！");
        }

        /// <summary>
        /// 启动定时器开始采集数据
        /// </summary>
        private void StartDataCollection()
        {
            timer = new Timer(1000); // 每秒采集一次数据
            timer.Elapsed += TimerElapsed; // 注册定时器事件
            timer.AutoReset = true; // 定时器自动重复
            timer.Start();

            // 手动调用一次采集，避免初次延迟
            TimerElapsed(this, null);
        }

        /// <summary>
        /// 定时器触发事件，用于定时采集 OPC 数据
        /// </summary>
        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            var results = opcClient.ReadItems();
            foreach (var result in results)
            {
                if (result.Quality == Opc.Da.Quality.Good)
                {
                    double value = Convert.ToDouble(result.Value);
                    DateTime now = DateTime.Now;

                    // 插入数据到数据库
                    dataStorage.InsertData(result.ItemName, value, now);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        // 添加实时数据到图表
                        if (!dataValues.ContainsKey(result.ItemName))
                        {
                            AddChartSeries(result.ItemName);
                        }

                        dataValues[result.ItemName].Add(value);

                        // 限制曲线显示的最大点数
                        if (dataValues[result.ItemName].Count > MaxPoints)
                        {
                            dataValues[result.ItemName].RemoveAt(0);
                        }
                    });
                }
            }
        }

        /// <summary>
        /// 查询历史数据并显示到图表
        /// </summary>
        private void QueryHistory_Click(object sender, RoutedEventArgs e)
        {
            // 查询最近 1 小时的历史数据
            var results = dataStorage.QueryDataByTimeRange(DateTime.Now.AddHours(-1), DateTime.Now);

            if (results.Count == 0)
            {
                MessageBox.Show("未查询到历史数据！"); // 如果没有数据，提示用户
                return;
            }

            // 清除当前图表数据
            DataChart.Series.Clear();

            // 按数据项分组，将历史数据添加到图表中
            foreach (var tagGroup in results.GroupBy(r => r.TagName))
            {
                var historyValues = new ChartValues<double>();
                foreach (var result in tagGroup)
                {
                    historyValues.Add(result.Value);
                }

                DataChart.Series.Add(new LineSeries
                {
                    Title = tagGroup.Key, // 数据项名称
                    Values = historyValues,
                    LineSmoothness = 0.5,
                    PointGeometry = null // 不显示数据点
                });
            }
        }

        /// <summary>
        /// 导出历史数据到 CSV 文件
        /// </summary>
        private void ExportData_Click(object sender, RoutedEventArgs e)
        {
            // 查询最近 1 小时的历史数据
            var data = dataStorage.QueryDataByTimeRange(DateTime.Now.AddHours(-1), DateTime.Now);

            if (data.Count == 0)
            {
                MessageBox.Show("未查询到历史数据！"); // 如果没有数据，提示用户
                return;
            }

            // 打开文件保存对话框
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "CSV 文件|*.csv",
                Title = "导出数据",
                FileName = "历史数据.csv" // 默认文件名
            };

            // 用户选择保存路径后，执行导出操作
            if (saveFileDialog.ShowDialog() == true)
            {
                using (var writer = new System.IO.StreamWriter(saveFileDialog.FileName))
                {
                    writer.WriteLine("TagName,Value,Timestamp"); // 写入列名
                    foreach (var record in data)
                    {
                        writer.WriteLine($"{record.TagName},{record.Value},{record.Timestamp:yyyy-MM-dd HH:mm:ss}");
                    }
                }

                MessageBox.Show($"历史数据已成功导出到 {saveFileDialog.FileName}！");
            }
        }

        /// <summary>
        /// 添加新的曲线到图表
        /// </summary>
        private void AddChartSeries(string tagName)
        {
            var chartValues = new ChartValues<double>();
            dataValues[tagName] = chartValues;
            DataChart.Series.Add(new LineSeries
            {
                Title = tagName, // 曲线名称
                Values = chartValues,
                LineSmoothness = 0.5, // 曲线平滑度
                PointGeometry = null // 不显示数据点
            });
        }
       

        /// <summary>
        /// 设置报警阈值
        /// </summary>
        private void SetThreshold_Click(object sender, RoutedEventArgs e)
        {
            // 检查用户输入的阈值是否为有效数字
            if (double.TryParse(ThresholdInput.Text, out double newThreshold))
            {
                threshold = newThreshold; // 更新阈值
                MessageBox.Show($"报警阈值已设置为：{threshold}");
            }
            else
            {
                MessageBox.Show("请输入有效的数字！"); // 提示用户输入无效
            }
        }

    }
}
