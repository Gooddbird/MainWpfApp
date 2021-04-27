using MainWpfApp.Util;
using MainWpfApp.ViewModels;
using OxyPlot.Axes;
using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;

namespace MainWpfApp {
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged {

        public string Proj_path { get; set; }                               // 工程db路径
        public DbConnection db;                                             // 工程db连接对象
        public BoltModel CurrentBolt { get; set; }                          // 当前选择螺栓项目
        public BoltLogModel CurrentBoltLog { get; set; }                    // 当前测量记录对
        public List<BoltModel> _BoltList = new List<BoltModel>();           // 螺栓列表
        public WavePlotModel wavePlotModel { get; set; }
        public StressPlotModel stressPlotModel { get; set; }
        public USTBolt ustBolt; 
        public bool IsLockWave = false;                                     // 是否锁定波形 默认为否
        public bool IsTesting = false;                                     // 是否正在测量
        public event PropertyChangedEventHandler PropertyChanged;
        public int MaxSize = 8178;                                          // 最大波形采集深度，一般不更改
        public int WaveUpdateDelay = 200;                                   // 波形更新频率控制
        public string Proj_name;                                            // 工程名字
        public int index = 1;                                               // 当前测量结果横坐标

        public MainWindow() {
            Application.Current.MainWindow = this;
            InitializeComponent();
            BoltComboList.ItemsSource = null;
            CurrentBolt = new BoltModel();
            Bolt_Para.DataContext = CurrentBolt;
            BuildBoltComboList(-1);
            ustBolt = new USTBolt();
            Init();
            InitWave();
            InitStressPlot();
            Proj_Name.DataContext = Proj_name;
        }


        /// <summary>
        /// 按键监听
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.P))
            {
                // Crtl + P 新建工程
                Util.InitUtil.AddProjFun();
            }
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.N))
            {
                // 新建项目
                AddItemFun();
            }
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.S))
            {
                // 保存工程
                SaveProjFun();
            }

        }

        /// <summary>
        /// 数据库按钮 点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenData_Click(object sender, RoutedEventArgs e) {
            if (Proj_path == null) {
                MessageBox.Show("请先打开工程！");
                return;
            }
            NavigationWindow window = new NavigationWindow
            {
                Source = new Uri("BoltDataShowPage.xaml", UriKind.Relative),
                Owner = this
            };
            window.ShowDialog();
        }
        
        /// <summary>
        /// 新建工程
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddProj_Click(object sender, RoutedEventArgs e) {
            Util.InitUtil.AddProjFun(); 
        }

        /// <summary>
        /// 保存工程
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveProj_Click(object sender, RoutedEventArgs e) {
            SaveProjFun(); 
        }

        /// <summary>
        /// 打开工程
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenProj_Click(object sender, RoutedEventArgs e) {
            Util.InitUtil.OpenProjFun();
            ConnectDB();
        }

        /// <summary>
        /// 新建螺栓项目
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddItem_Click(object sender, RoutedEventArgs e) {
            AddItemFun();          
        }


        /// <summary>
        /// 另存为
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveProjAs_Click(object sender, RoutedEventArgs e) {
            Util.InitUtil.SaveProjAsFun();
        }

        /// <summary>
        /// 添加螺栓项目(此处只是预先添加 实际要点击保存后才会插入到db)
        /// </summary>
        private void AddItemFun() {
            if (Proj_path == null) {
                MessageBox.Show("请先打开工程！");
                return;
            }
            // 打开新建螺栓项目窗口
            AddItemDialog dialog = new AddItemDialog();
            dialog.ShowDialog();
            if (dialog.isSuccessd == true) {
                // 添加成功则更新索引到新增的螺栓对象
                _BoltList = db.Bolts.ToList();
                BuildBoltComboList(_BoltList.Count - 1);
            }

        }

        /// <summary>
        /// 保存功能 将数据保存至db文件
        /// </summary>
        private void SaveProjFun() {
            try
            {
                int i = BoltComboList.SelectedIndex;
                db.Update(CurrentBolt, typeof(BoltModel));
                _BoltList = db.Bolts.ToList();
                BuildBoltComboList(i);
            }
            catch (SQLiteException)
            {
                db.Rollback();
                MessageBox.Show("保存失败！");
            }
        }

        
        /// <summary>
        /// copy 螺栓参数 触发修改事件
        /// </summary>
        /// <param name="m1"></param>
        /// <param name="m2"></param>
        public static void CopyBoltPara(BoltModel m1, BoltModel m2) {
            m1.Bolt_id = m2.Bolt_id;
            m1.Material = m2.Material;
            m1.Standards = m2.Standards;
            m1.Stress_coefficient = m2.Stress_coefficient;
            m1.Bolt_length = m2.Bolt_length;
            m1.Clamp_length = m2.Clamp_length;
            m1.Nominal_diameter = m2.Nominal_diameter;
            m1.Description = m2.Description;
        }
        

        /// <summary>
        /// 连接db 设置螺栓初始参数
        /// </summary>
        public void ConnectDB() {
            try
            {
                db = new DbConnection(Proj_path);
                _BoltList = db.Bolts.ToList();
                BuildBoltComboList(0);
            }
            catch (SQLiteException)
            {
                MessageBox.Show("连接数据库失败，请重试！");
            }
        }

        /// <summary>
        /// 螺栓选择框绑定数据源
        /// </summary>
        /// <param name="i"> 当前选择螺栓index </param>
        public void BuildBoltComboList(int i = -1) {
            BoltComboList.ItemsSource = _BoltList;
            BoltComboList.DisplayMemberPath = "Bolt_id";
            // BoltComboList.SelectedIndex = i;
            BoltComboList.SelectedValuePath = "Bolt_id";
            BoltModel tmp;
            if (_BoltList.Count >= i + 1 && i != -1)
            {
                tmp = _BoltList[i];
                BoltComboList.SelectedIndex = i;
            }
            else {
                tmp = new BoltModel();
                BoltComboList.SelectedIndex = -1;
            }
            CopyBoltPara(CurrentBolt, tmp);
        }

        /// <summary>
        /// 下拉框选择项改变事件 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BoltComboList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
            int index = BoltComboList.SelectedIndex;
            BoltModel tmp;
            if (index == -1 || _BoltList.Count < (index + 1))
            {
                tmp = new BoltModel();
            }
            else {
                tmp = _BoltList[index]; 
            }
            CopyBoltPara(CurrentBolt, tmp);
            DateTime start = DateTime.Now.AddDays(-7); 
            DateTime end = DateTime.Now;
            UpdateStressPlot(start, end, 30);
        }

        private void UpdateStressPlot(DateTime startTime, DateTime endTime, int count) {
            if (CurrentBolt == null) {
                MessageBox.Show("请先选择螺栓");
            }
            try
            {
                string start = startTime.ToString();
                string end = endTime.ToString();
                stressPlotModel.points.Clear();
                string sql = string.Format(
                    "SELECT * FROM t_bolt_logs " +
                    "WHERE Bolt_id='{0}' " +
                    "and TestTime > '{1}' " +
                    "and TestTime <= '{2}' " +
                    "ORDER BY TestTime DESC " + 
                    "LIMIT {3};",
                    CurrentBolt.Bolt_id, start, end, count);
                List<BoltLogModel> boltLogs = db.Query<BoltLogModel>(sql);
                boltLogs.Reverse();
                float maxY = -10000000;
                float minY = 10000000;
                foreach (BoltLogModel boltLog in boltLogs) {
                    maxY = Math.Max(maxY, boltLog.AxialForce);
                    minY = Math.Min(minY, boltLog.AxialForce);
                    stressPlotModel.points.Add(new StressLogPoint(index, boltLog.AxialForce, boltLog.TestTime, boltLog.TimeDelay, boltLog.MaxXcorr, boltLog.Id));
                    index++;
                    stressPlotModel.stressPlot.InvalidatePlot(true);
                }
                stressPlotModel.xAxis.Maximum = index + 10;
                stressPlotModel.xAxis.Reset();
                stressPlotModel.yAxis.Reset();
            }
            catch (SQLiteException) {
                db.Rollback();
                MessageBox.Show("获取测量记录失败，请重试！");
            }
        }

        /// <summary>
        /// 初始化波形
        /// </summary>
        private void InitWave() {
            wavePlotModel = new WavePlotModel();
            wavePlotModel.Init();
        }

        private void InitStressPlot() {
            stressPlotModel = new StressPlotModel();
            stressPlotModel.Init();
        }

        
        /// <summary>
        /// 初始化 连接板卡 下发板卡参数 开启tcp线程从板卡获取实时波形数据 
        /// </summary>
        private void Init() {
            /*************初始化**************/
            ustBolt.USTBDataInit();
            /*************连接板卡**************/
            while (true)
            {
                ustBolt.tcpConnect();
                if (ustBolt.tcpConnFlag == 1)
                {
                    break;
                }
                else
                {
                    Console.WriteLine("tcp connecting!");
                }
            }
            // 开启UI线程
            StartUIThread();
            // 开启tcp线程获取板卡回送数据
            ustBolt.tcpClientThreadStart();
            /*************写入参数**************/
            //写入基准波形
            //double[] waveDataTmp = ustBolt.utsMath.readCsvZeroWaveData(@"C:\Users\hhhhh\Desktop\design\Project\USTBolt_Client\SimWaveData8178.csv");
            //Array.Copy(waveDataTmp, ustBolt.ustbData.lstuintZeroWaveDataBuff[0], waveDataTmp.Length);
            //Array.Copy(waveDataTmp, ustBolt.ustbData.lstuintZeroWaveDataBuff[1], waveDataTmp.Length);

        }


        /// <summary>
        /// 参数线程 定时执行
        /// 1. 获取测量结果 更新UI
        /// 2. 下发板卡参数
        /// </summary>
        private void StartUIThread() {
            Task.Factory.StartNew(() => {
                while (true)
                {
                    if (ustBolt.tcpConnFlag == 0)
                    {
                        Thread.Sleep(5000);
                    }
                    else
                    {
                        // 注：子线程要获取主线程UI 需经过Dispacther.Invoke
                        Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,
                            new Action(() => {
                                AxialForce.Text = ustBolt.ustbData.axialForce.ToString();
                                TimeDelay.Text = ustBolt.ustbData.timeDelay.ToString();
                                MaxXcorr.Text = ustBolt.ustbData.maxXcorr.ToString();
                                ustBolt.ustbData.pulsWidt = Convert.ToDouble(pulsWidt.Text);
                                ustBolt.ustbData.exciVolt = Convert.ToDouble(exciVolt.Text);
                                ustBolt.ustbData.prf = Convert.ToDouble(prf.Text);
                                ustBolt.ustbData.damping = Convert.ToDouble(damping.Text);
                                ustBolt.ustbData.Ks= Convert.ToDouble(Ks.Text);
                                ustBolt.ustbData.KT= Convert.ToDouble(KT.Text);
                                ustBolt.ustbData.T0= Convert.ToDouble(ZeroTem.Text);
                                ustBolt.ustbData.T1= Convert.ToDouble(TestTem.Text);
                            }));
                        ustBolt.setPara();
                        Thread.Sleep(1000);
                    }
                }

            });
        }
        
        
        /// <summary>
        /// 锁定波形按钮选中事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LockWaveBtn_Checked(object sender, RoutedEventArgs e) {
            IsLockWave = true;
        }

        /// <summary>
        /// 锁定波形按钮非选中事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LockWaveBtn_Unchecked(object sender, RoutedEventArgs e) {
            IsLockWave = false;
        }

        /// <summary>
        /// 主窗口关闭事件 关闭所有其他的线程 包括绘波形线程 TCP线程等
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, CancelEventArgs e) {
            Process.GetCurrentProcess().Kill();
        }


        /// <summary>
        /// 开始测量按钮按下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartBtn_Checked(object sender, RoutedEventArgs e) {
            IsLockWave = false;
            IsTesting = true;
            ustBolt.ustbData.LWaveTDEStart = wavePlotModel.GetLWaveXStart();
            ustBolt.ustbData.LWaveTEDEnd = wavePlotModel.GetLWaveXEnd();
            ustBolt.StartStressCalThread();
        }

        private void StartBtn_Unchecked(object sender, RoutedEventArgs e) {
            IsTesting = false;
        }

        private void SingleLogBtn_Click(object sender, RoutedEventArgs e) {
            if (CurrentBolt == null || CurrentBolt.Bolt_id == null) {
                MessageBox.Show("请先选择螺栓！");
                return;
            }
            if (!IsTesting) {
                MessageBox.Show("未在测量过程中！");
                return;
            }
            try
            {
                DateTime nowTime = DateTime.Now;
                double force = Convert.ToDouble(AxialForce.Text);
                double timeDelay = Convert.ToDouble(TimeDelay.Text);
                double maxXcorr = Convert.ToDouble(MaxXcorr.Text);
                // 插入记录到db
                CurrentBoltLog = new BoltLogModel
                {
                    Bolt_id = CurrentBolt.Bolt_id,
                    AxialForce = (float)force,
                    TimeDelay = (float)timeDelay,
                    MaxXcorr = (float)maxXcorr,
                    TestTime = nowTime.ToString()
                };
                db.Insert(CurrentBoltLog, typeof(BoltLogModel));

                // 绘制当前点
                stressPlotModel.points.Add(new StressLogPoint(index, force, nowTime.ToString(), timeDelay, maxXcorr, CurrentBoltLog.Id));
                index++;
                // stressPlotModel.stressWave.ItemsSource = stressPlotModel.points;
                stressPlotModel.stressPlot.InvalidatePlot(true);
            }
            catch (SQLiteException)
            {
                Console.WriteLine("insert exception!, sql: ");
                MessageBox.Show("发生异常，请重试！");
                db.Rollback();
            }
            catch (Exception) {
                MessageBox.Show("发生异常，请重启！");
            }

        }
    }
    
}
