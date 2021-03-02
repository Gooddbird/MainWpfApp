using MainWpfApp.ViewModels;
using SQLite;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace MainWpfApp {
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged {
        public MainWindow() {
            InitializeComponent();
            BoltComboList.ItemsSource = null;
            CurrentBolt = new BoltModel();
            BoltModel t = new BoltModel();
            _BoltList.Add(t);
            Bolt_Para.DataContext = CurrentBolt;
            BuildBoltComboList(-1);
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
            //Window1 win = new Window1();
            //win.Show();
            BoltDataShowPage page = new BoltDataShowPage();
            Content = page;
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
            SetPara();
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
                MessageBox.Show("保存失败！");
            }
        }

        public static string Proj_path { get; set; }    // 工程db路径

        private DbConnection db;                        // 工程db连接对象

        public BoltModel CurrentBolt { get; set; }      // 当前选择螺栓项目

        public static List<BoltModel> _BoltList = new List<BoltModel>();   // 螺栓列表

        public event PropertyChangedEventHandler PropertyChanged;
 
        public static void CopyBoltPara(BoltModel m1, BoltModel m2) {
            m1.Bolt_id = m2.Bolt_id;
            m1.Material = m2.Material;
            m1.Standards = m2.Standards;
            m1.Stress_coefficient = m2.Stress_coefficient;
            m1.Bolt_length = m2.Bolt_length;
            m1.Clamp_length = m2.Clamp_length;
            m1.Nominal_diameter = m2.Nominal_diameter;
        }

        /// <summary>
        /// 主面板参数初始化
        /// </summary>
        public void SetPara() {
            try
            {
                db = new MainWpfApp.ViewModels.DbConnection(Proj_path);
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
        }
    }
    
}
