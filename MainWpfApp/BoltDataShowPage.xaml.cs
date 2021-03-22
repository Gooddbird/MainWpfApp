using MainWpfApp.ViewModels;
using SQLite;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace MainWpfApp {
    /// <summary>
    /// BoltDataShowPage.xaml 的交互逻辑
    /// </summary>
    public partial class BoltDataShowPage : Page {
        public BoltDataShowPage() {
            InitializeComponent();
            BoltList = mainwin.db.Bolts.ToList();
            BoltsTable.ItemsSource = BoltList; 
        }
        public List<BoltModel> BoltList = new List<BoltModel>();        // 当前螺栓列表 绑定前端datagrid控件 随时变换
        public static MainWindow mainwin = (MainWindow)Application.Current.MainWindow;

        /// <summary>
        /// 返回按钮点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackBtn_Click(object sender, RoutedEventArgs e) {

            
            Window win = (Window)this.Parent;
            win.Close();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e) {
            try {
                // 删除
                foreach (BoltModel t in GetDelBolts(BoltList, mainwin.db.Bolts.ToList())) {
                    mainwin.db.Delete(t);
                }

                // 新增或修改
                foreach (BoltModel t in BoltList) {
                    mainwin.db.InsertOrReplace(t);
                }
                mainwin._BoltList = mainwin.db.Bolts.ToList();
                var win = (MainWindow)Application.Current.MainWindow;
                win.BuildBoltComboList(0);
            }
            catch (SQLiteException) {
                mainwin.db.Rollback();
                MessageBox.Show("更新失败，请重试！");
            }
        }

        /// <summary>
        /// 找出list1中不存在而list2中存在的元素 即待删除的元素
        /// </summary>
        /// <param name="list1"></param>
        /// <param name="list2"></param>
        /// <returns></returns>
        private List<BoltModel> GetDelBolts(List<BoltModel> list1, List<BoltModel> list2) {
            List<BoltModel> re = new List<BoltModel>();
            foreach (BoltModel tmp in list2){
                if (list1.Contains(tmp) == false) {
                    re.Add(tmp); 
                }
            }
            return re;
        }
    }
}
