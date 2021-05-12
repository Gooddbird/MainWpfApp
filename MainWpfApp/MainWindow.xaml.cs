using MainWpfApp.Util;
using MainWpfApp.ViewModels;
using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Timers;
using OxyPlot;

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
        public WavePlotModel WavePlotModel { get; set; }
        public StressPlotModel StressPlotModel { get; set; }
        public Bolt CurrentBoltClient; 
        public bool IsLockWave = false;                                     // 是否锁定波形 默认为否
        public bool IsTesting = false;                                     // 是否正在测量
        public event PropertyChangedEventHandler PropertyChanged;
        public int MaxSize = 8178;                                          // 最大波形采集深度，一般不更改
        public int WaveUpdateDelay = 200;                                   // 波形更新频率控制
        public string Proj_name;                                            // 工程名字
        public int index = 1;                                               // 当前测量结果横坐标
        public bool IsRealtimeLog = false;                                  // 是否实时测量
        public bool PlotFlag = false;
        public System.Timers.Timer timer = new System.Timers.Timer();
        public ProgressWindow progressWindow;

        public MainWindow() {
            Application.Current.MainWindow = this;
           // while (window.IsCompleted == false) { }
            InitializeComponent();
            InitObjects();
            InitConnection();
            InitPlotModel();

            //// 等待获取第一个波形数据
            while (CurrentBoltClient.boltData.IsCanStartCal == false) { };
            StartBtn.IsChecked = true;

            //progressWindow = new ProgressWindow();
            //progressWindow.ShowDialog();
            //Progress();
            // window.ProgressBegin(CurrentBoltClient);
        }


        //public void Progress() {
        //    Thread thread = new Thread(new ThreadStart(() =>
        //    {
        //        for (int i = 0; i <= 100; i++)
        //        {
        //            if (CurrentBoltClient.boltData.IsCanStartCal == true)
        //            {
        //                progressWindow.progressBar1.Dispatcher.BeginInvoke((ThreadStart)delegate { progressWindow.progressBar1.Value = 100; });
        //                Thread.Sleep(1000);
        //                break;
        //            }
        //            progressWindow.progressBar1.Dispatcher.BeginInvoke((ThreadStart)delegate { progressWindow.progressBar1.Value = i; });
        //            Thread.Sleep(300);
        //        }
        //        // this.Close();

        //    }));
        //    thread.Start();

        //}

        /// <summary>
        /// 初始化数据源、UI、绑定对象 
        /// </summary>
        private void InitObjects() {
            BoltComboList.ItemsSource = null;
            CurrentBolt = new BoltModel();
            Bolt_Para.DataContext = CurrentBolt;
            BuildBoltComboList(-1);
            CurrentBoltClient = new Bolt();
            CurrentBoltClient.USTBDataInit();
            Proj_Name.DataContext = Proj_name;

            DateTime dateTime = DateTime.Now;
            DateTime start = dateTime.AddDays(-7);
            DateTime end = dateTime.AddDays(1);

            EndDateCal.SelectedDate = end;
            EndTimeText.Text = end.Year.ToString() + "-" + end.Month.ToString() + "-" + end.Day.ToString();

            StartDateCal.SelectedDate = start;
            StartTimeText.Text = start.Year.ToString() + "-" + start.Month.ToString() + "-" + start.Day.ToString();

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
                Title = "数据库",
                Owner = this,
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
            if (Proj_path == null) {
                MessageBox.Show("请先打开工程！");
                return;
            }
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
                if (Proj_path == null || Proj_path == "") {
                    return;
                }
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
                index = 1;
                string start = startTime.ToString("yyyy-MM-dd HH:mm:ss:ffff");
                string end = endTime.ToString("yyyy-MM-dd HH:mm:ss:ffff");
                // 清除原来的点
                StressPlotModel.points.Clear();
                StressPlotModel.stressWave.ClearSelection();
                StressPlotModel.stressPlot.InvalidatePlot(true);
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
                float maxY = -10000;
                float minY = 10000;
                foreach (BoltLogModel boltLog in boltLogs)
                {
                    maxY = Math.Max(maxY, boltLog.AxialForce);
                    minY = Math.Min(minY, boltLog.AxialForce);
                    StressPlotModel.points.Add(new StressLogPoint(index, boltLog.AxialForce, boltLog.TestTime, boltLog.TimeDelay, boltLog.MaxXcorr, boltLog.Id));
                    index++;
                    StressPlotModel.stressPlot.InvalidatePlot(true);
                }
                StressPlotModel.xAxis.Maximum = index + 10;
                StressPlotModel.xAxis.Reset();
                StressPlotModel.yAxis.Maximum = maxY + 400;
                StressPlotModel.yAxis.Minimum = minY - 200;
                StressPlotModel.yAxis.Reset();
            }
            catch (SQLiteException)
            {
                db.Rollback();
                MessageBox.Show("获取测量记录失败，请重试！");
            }
            catch (Exception) {
                MessageBox.Show("获取测量记录失败，请重试！");
            }

        }

        /// <summary>
        /// 初始化波形
        /// </summary>
        private void InitPlotModel() {
            WavePlotModel = new WavePlotModel();
            WavePlotModel.Init();
            StressPlotModel = new StressPlotModel();
            StressPlotModel.Init();
        }
        
        /// <summary>
        /// 初始化 连接板卡 下发板卡参数 开启tcp线程从板卡获取实时波形数据 
        /// </summary>
        private void InitConnection() {
            while (CurrentBoltClient.TcpConnFlag != 1) {
                CurrentBoltClient.TcpConnect();
            }
            /*************连接板卡**************/
            CurrentBoltClient.TcpConnectThreadStart();
            // 开启UI线程
            StartUIThread();
            // 开启tcp线程获取板卡回送数据
            CurrentBoltClient.TcpClientThreadStart();
            /*************写入参数**************/
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
                    if (CurrentBoltClient.TcpConnFlag == 0)
                    {
                        Thread.Sleep(5000);
                    }
                    else
                    {
                        // 注：子线程要获取主线程UI 需经过Dispacther.Invoke
                        Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,
                            new Action(() => {
                                AxialForce.Text = CurrentBoltClient.boltData.axialForce.ToString();
                                TimeDelay.Text = CurrentBoltClient.boltData.timeDelay.ToString();
                                MaxXcorr.Text = CurrentBoltClient.boltData.maxXcorr.ToString();
                                CurrentBoltClient.boltData.pulsWidt = Convert.ToDouble(pulsWidt.Text);
                                CurrentBoltClient.boltData.exciVolt = Convert.ToDouble(exciVolt.Text);
                                CurrentBoltClient.boltData.prf = Convert.ToDouble(prf.Text);
                                CurrentBoltClient.boltData.damping = Convert.ToDouble(damping.Text);
                                CurrentBoltClient.boltData.lstGain[0]= Convert.ToDouble(GainText.Text);
                                CurrentBoltClient.boltData.Ks= Convert.ToDouble(Ks.Text);
                                CurrentBoltClient.boltData.KT= Convert.ToDouble(KT.Text);
                                CurrentBoltClient.boltData.T0= Convert.ToDouble(ZeroTem.Text);
                                CurrentBoltClient.boltData.T1= Convert.ToDouble(TestTem.Text);
                            }));
                        CurrentBoltClient.SetPara();
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


        private void StartTest() {
            IsLockWave = false;
            //LockWaveBtn.IsChecked = false;
            IsTesting = true;
            CurrentBoltClient.boltData.LWaveTDEStart = WavePlotModel.GetLWaveXStart();
            CurrentBoltClient.boltData.LWaveTEDEnd = WavePlotModel.GetLWaveXEnd();
            CurrentBoltClient.StartStressCalThread();
        }

        /// <summary>
        /// 开始测量按钮按下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartBtn_Checked(object sender, RoutedEventArgs e) {
            StartTest();
        }

        private void StartBtn_Unchecked(object sender, RoutedEventArgs e) {
            IsTesting = false;
        }

        /// <summary>
        /// 记录一次结果
        /// </summary>
        private void SingleLog(bool testType) {
            try
            {
                DateTime nowTime = DateTime.Now;
                double force;
                double timeDelay;
                double maxXcorr;
                /// 单点记录时 为确保实时性 记录值为当前UI界面显示值 
                /// 实时记录时 因为非主线程不方便获取UI元素值 直接存实时计算结果值
                if (testType == true)
                {
                    force = Convert.ToDouble(AxialForce.Text);
                    timeDelay = Convert.ToDouble(TimeDelay.Text);
                    maxXcorr = Convert.ToDouble(MaxXcorr.Text);
                }
                else {
                    force = CurrentBoltClient.boltData.axialForce;
                    timeDelay = CurrentBoltClient.boltData.timeDelay;
                    maxXcorr = CurrentBoltClient.boltData.maxXcorr;
                }

                // 插入记录到db
                CurrentBoltLog = new BoltLogModel
                {
                    Bolt_id = CurrentBolt.Bolt_id,
                    AxialForce = (float)force,
                    TimeDelay = (float)timeDelay,
                    MaxXcorr = (float)maxXcorr,
                    TestTime = nowTime.ToString("yyyy-MM-dd HH:mm:ss:ffff")
                };
                db.Insert(CurrentBoltLog, typeof(BoltLogModel));

                // 绘制当前点
                StressPlotModel.points.Add(new StressLogPoint(index, force, nowTime.ToString("yyyy-MM-dd HH:mm:ss:ffff"), timeDelay, maxXcorr, CurrentBoltLog.Id));
                index++;
                StressPlotModel.yAxis.Maximum = force + 200;
                StressPlotModel.yAxis.Reset();
                // stressPlotModel.stressWave.ItemsSource = stressPlotModel.points;
                StressPlotModel.stressPlot.InvalidatePlot(true);
            }
            catch (SQLiteException)
            {
                timer.Enabled = false;
                Console.WriteLine("insert exception!, sql: ");
                MessageBox.Show("发生异常，请重试！");
                db.Rollback();
            }
            catch (Exception e)
            {
                timer.Enabled = false;
                Console.WriteLine(e.ToString());
                MessageBox.Show("发生异常，请重启！");
            }
        }

        /// <summary>
        /// 单点记录按钮点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SingleLogBtn_Click(object sender, RoutedEventArgs e) {
            if (CurrentBolt == null || CurrentBolt.Bolt_id == null) {
                MessageBox.Show("请先选择螺栓！");
                return;
            }
            if (!IsTesting) {
                MessageBox.Show("未在测量过程中！");
                return;
            }
            if (IsRealtimeLog) {
                MessageBox.Show("正在实时记录！");
                return;
            }
            SingleLog(true);
        }

        private void StartTimeBtn_MouseDown(object sender, MouseButtonEventArgs e) {
            
            if(StartDateCal.Visibility == Visibility.Hidden)
            {
                StartDateCal.Visibility = Visibility.Visible;
                return;
            } else if(StartDateCal.Visibility == Visibility.Visible)
            {
                StartDateCal.Visibility = Visibility.Hidden;
                if(StartDateCal.SelectedDate.HasValue) {
                    DateTime dateTime = StartDateCal.SelectedDate.Value;
                    StartTimeText.Text = dateTime.Year.ToString() + "-" + dateTime.Month.ToString() + "-" + dateTime.Day.ToString();
                }
            }
        }

        private void EndTimeBtn_MouseDown(object sender, MouseButtonEventArgs e) {
            if (EndDateCal.Visibility == Visibility.Hidden)
            {
                EndDateCal.Visibility = Visibility.Visible;
                return;
            }
            else if (EndDateCal.Visibility == Visibility.Visible)
            {
                EndDateCal.Visibility = Visibility.Hidden;
                if (EndDateCal.SelectedDate.HasValue) {
                    DateTime dateTime = EndDateCal.SelectedDate.Value;
                    EndTimeText.Text = dateTime.Year.ToString() + "-" + dateTime.Month.ToString() + "-" + dateTime.Day.ToString();
                }
            }
        }

        private void SearchLogsBtn_MouseDown(object sender, MouseButtonEventArgs e) {
            if (CurrentBolt == null || CurrentBolt.Bolt_id == null)
            {
                MessageBox.Show("请先选择螺栓！");
                return;
            }
            DateTime start;
            DateTime end;
            int count = 30;
            if (StartDateCal.SelectedDate.HasValue)
            {
                start = StartDateCal.SelectedDate.Value;
                count = 10000;
            }
            else {
                start = DateTime.Today.AddDays(-7);
            }
            if (EndDateCal.SelectedDate.HasValue)
            {
                end = EndDateCal.SelectedDate.Value;
            }
            else {

                end = DateTime.Now;
            }
            UpdateStressPlot(start, end, count);
        }

        private void RealtimeLog(object sender, System.Timers.ElapsedEventArgs e) {

            if (!IsTesting)
            {
                MessageBox.Show("未在测量过程中。实时记录停止！");
                RealtimeLogBtn.IsChecked = false;
                timer.Enabled = false;
                return;
            }
            SingleLog(false);
        }

        private void RealtimeLogBtn_Checked(object sender, RoutedEventArgs e) {
            if (CurrentBolt == null || CurrentBolt.Bolt_id == null)
            {
                MessageBox.Show("请先选择螺栓！");
                RealtimeLogBtn.IsChecked = false;
                return;
            }
            if (!IsTesting)
            {
                MessageBox.Show("未在测量过程中！");
                RealtimeLogBtn.IsChecked = false;
                return;
            }
            IsRealtimeLog = true;
            timer.Interval = 500;
            timer.Enabled = true;
            timer.Elapsed += new ElapsedEventHandler(RealtimeLog);
            timer.Start();
        }

        private void RealtimeLogBtn_Unchecked(object sender, RoutedEventArgs e) {
            IsRealtimeLog = false;
            timer.Enabled = false;
        }

        private void SaveZeroBtn_Click(object sender, RoutedEventArgs e) {
            Task.Factory.StartNew(() =>
            {
                if (PlotFlag == true) {
                    return;
                }
                IsLockWave = true;
                PlotFlag = true;
                WavePlotModel.ZeroWave.Points.Clear();
                WavePlotModel.LWavePlotModel.InvalidatePlot(true);
                int i = 0;
                for (int t = 0; t < MaxSize; t++)
                {
                    CurrentBoltClient.boltData.lstuintZeroWaveDataBuff[0][t] = CurrentBoltClient.boltData.lstuintWaveDataBuff[0][t];
                }
                var zeroWaveList = CurrentBoltClient.boltData.lstuintZeroWaveDataBuff[0];
                i = 0;
                while (i < MaxSize)
                {
                    WavePlotModel.ZeroWave.Points.Add(new DataPoint(i, zeroWaveList[i]));
                    i++;
                    WavePlotModel.LWavePlotModel.InvalidatePlot(true);
                }
                CurrentBoltClient.boltData.LWaveTDEStart = WavePlotModel.GetLWaveXStart();
                CurrentBoltClient.boltData.LWaveTEDEnd = WavePlotModel.GetLWaveXEnd();
                if (IsTesting) {
                    IsTesting = false;
                    Thread.Sleep(300);
                    StartTest();
                }
                IsLockWave = false;
                PlotFlag = false;
            }); 
            
        }

        //private void ConnectBtn_Click(object sender, RoutedEventArgs e) {

        //    //progressWindow = new ProgressWindow();
        //   // progressWindow.ShowDialog();


        //    InitConnection();
        //    InitPlotModel();
        //    // 等待获取第一个波形数据
        //    //while (CurrentBoltClient.boltData.IsCanStartCal == false) { };
        //    //StartBtn.IsChecked = true;

        //}
    }
    
}
