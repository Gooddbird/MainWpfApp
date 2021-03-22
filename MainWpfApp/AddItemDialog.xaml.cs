using MainWpfApp.ViewModels;
using SQLite;
using System;
using System.Windows;

namespace MainWpfApp {
    /// <summary>
    /// AddItemDialog.xaml 的交互逻辑
    /// </summary>
    public partial class AddItemDialog : Window
    {
        public BoltModel CurrentBolt { get; set; }      // 当前操作螺栓项目

        public bool isSuccessd = false;
        public MainWindow mainwin = (MainWindow)Application.Current.MainWindow;

        public AddItemDialog()
        {
            this.Owner = Application.Current.MainWindow;
            WindowStartupLocation = WindowStartupLocation.CenterScreen; // 弹出窗口居中
            InitializeComponent();
            CurrentBolt = new BoltModel();
            Bolt_Para.DataContext = CurrentBolt;
        }

        /// <summary>
        /// 新增项目确定按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddItemOk_Click(object sender, RoutedEventArgs e) {
            //MessageBox.Show(MainWindow.Proj_path);

            if (String.IsNullOrEmpty(BoltId.Text))
            {
                MessageBox.Show("请输入项目编号！");
            }
            else {
                // MainWindow.BoltsToSave.Add(CurrentBolt);
                // Close();
                try {
                    var db = new MainWpfApp.ViewModels.DbConnection(mainwin.Proj_path);
                    BoltModel bolt = db.Find<BoltModel>(CurrentBolt.Bolt_id);
                    if (bolt != null)
                    {
                        if (MessageBox.Show("已存在该项目，是否替换？", "提示", MessageBoxButton.YesNo) == MessageBoxResult.No) {
                            return; 
                        }
                    }
                    int rt = db.InsertOrReplace(CurrentBolt, typeof(BoltModel));
                    if (rt == 1)
                    {
                        isSuccessd = true;
                    }
                    else
                    {
                        isSuccessd = false;
                        MessageBox.Show("添加失败，请重试！");
                    }
                }
                catch (SQLiteException)
                {
                    isSuccessd = false;
                    MessageBox.Show("添加失败，请重试！");
                }
                Close();
            }
        }
    }
}
