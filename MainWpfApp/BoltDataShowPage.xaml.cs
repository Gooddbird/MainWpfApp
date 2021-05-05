using MainWpfApp.ViewModels;
using SQLite;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MainWpfApp {
    /// <summary>
    /// BoltDataShowPage.xaml 的交互逻辑
    /// </summary>
    public partial class BoltDataShowPage : Page {
        public BoltDataShowPage() {
            InitializeComponent();

            RefreshData();
            // 默认显示螺栓数据表
            currentTable = Table.BoltsTable;
            BoltsTable.Visibility = Visibility.Visible;
            BoltLogsTable.Visibility = Visibility.Hidden;
        }

        // 显示的表格
        public enum Table {
            BoltsTable = 1,         // 螺栓表
            BoltLogsTable = 2       // 测量记录表
        };

        public Table currentTable;                                      // 当前显示table
        public List<BoltModel> BoltList = new List<BoltModel>();        // 当前螺栓列表 绑定前端datagrid控件 随时变换
        public List<BoltLogModel> BoltLogList = new List<BoltLogModel>();
        public static MainWindow mainwin = (MainWindow)Application.Current.MainWindow;
        public string BoltTableOldValue = "";                                         // 单元格旧值
        public string BoltTableNewValue = "";                                         // 单元格新值
        public bool isBoltTableChanged = false;                                       // DataGrid是否修改过
        public string LogTableOldValue = "";                                         // 单元格旧值
        public string LogTableNewValue = "";                                         // 单元格新值
        public bool isLogTableChanged = false;                                       // DataGrid是否修改过
        public bool IsNoticed = false;                                               // 是否已经提示过


        /// <summary>
        /// 返回按钮点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackBtn_Click(object sender, RoutedEventArgs e) {
            Notice(); 
            Window win = (Window)this.Parent;
            win.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            Notice();
        }

        /// <summary>
        /// 提示是否需要保存
        /// </summary>
        private void Notice() {
            if (IsNoticed == true) {
                return;
            }
            if (currentTable == Table.BoltsTable)
            {
                if (isBoltTableChanged)
                {
                    if (MessageBox.Show("是否将螺栓表的修改提交到数据库？", "提示", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        SaveToDb();
                    }
                }

            }
            else if (currentTable == Table.BoltLogsTable)
            {
                if (isLogTableChanged) {
                    if (MessageBox.Show("是否将测量记录表的修改提交到数据库？", "提示", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        SaveToDb();
                    }

                }
            }
            IsNoticed = true;
        }

        /// <summary>
        /// 找出list1中不存在而list2中存在的元素 即待删除的元素
        /// </summary>
        /// <param name="list1"></param>
        /// <param name="list2"></param>
        /// <returns></returns>
        private List<T> GetDelObjs<T>(List<T> list1, List<T> list2) {
            List<T> re = new List<T>();
            foreach (T tmp in list2) {
                if (list1.Contains(tmp) == false)
                {
                    re.Add(tmp);
                }
            }
            return re;
        }
        

        /// <summary>
        /// 将更改提交到DB
        /// </summary>
        private void SaveToDb() {
            try
            {
                if (currentTable == Table.BoltsTable)
                {
                    // 删除
                    foreach (BoltModel t in GetDelObjs<BoltModel>(BoltList, mainwin.db.Bolts.ToList()))
                    {
                        mainwin.db.Delete(t);
                    }

                    // 新增或修改
                    foreach (BoltModel t in BoltList)
                    {
                        mainwin.db.InsertOrReplace(t);
                    }
                    mainwin._BoltList = mainwin.db.Bolts.ToList();
                    var win = (MainWindow)Application.Current.MainWindow;
                    win.BuildBoltComboList(0);
                }
                else if (currentTable == Table.BoltLogsTable)
                {
                    foreach (BoltLogModel t in GetDelObjs<BoltLogModel>(BoltLogList, mainwin.db.BoltLogs.ToList()))
                    {
                        mainwin.db.Delete(t);
                    }

                    // 新增或修改
                    foreach (BoltLogModel t in BoltLogList)
                    {
                        mainwin.db.InsertOrReplace(t);
                    }
                }

            }
            catch (SQLiteException)
            {
                mainwin.db.Rollback();
                MessageBox.Show("更新失败，请重试！");
            }
        }

        /// <summary>
        /// 提交到数据库 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveBtn_Click(object sender, RoutedEventArgs e) {
            SaveToDb(); 
        }

        private void CheckBoltsTableBtn_Click(object sender, RoutedEventArgs e) {
            BoltList = mainwin.db.Bolts.ToList();
            BoltsTable.ItemsSource = BoltList;
            if (currentTable == Table.BoltsTable)
            {
                return;
            } else {
                Notice();
                currentTable = Table.BoltsTable;    
                BoltsTable.Visibility = Visibility.Visible;
                BoltLogsTable.Visibility = Visibility.Hidden;
            }
        }

        private void CheckBoltLogsTableBtn_Click(object sender, RoutedEventArgs e) {
            BoltLogList = mainwin.db.BoltLogs.ToList();
            BoltLogsTable.ItemsSource = BoltLogList;
            if (currentTable == Table.BoltLogsTable)
            {
                return;
            }
            else {
                Notice();
                currentTable = Table.BoltLogsTable;
                BoltLogsTable.Visibility = Visibility.Visible;
                BoltsTable.Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// 刷新数据
        /// </summary>
        private void RefreshData() {
            BoltList = mainwin.db.Bolts.ToList();
            BoltLogList = mainwin.db.BoltLogs.ToList();
            BoltsTable.ItemsSource = BoltList;
            BoltLogsTable.ItemsSource = BoltLogList;
        }

        private void RefreshDataBtn_Click(object sender, RoutedEventArgs e) {
            RefreshData();
        }

        private void Page_KeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.S))
            {
                SaveToDb();
            }
        }

        private void BoltsTable_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e) {
            BoltTableNewValue = (e.EditingElement as TextBox).Text;
            if (BoltTableOldValue != BoltTableNewValue) {
                isBoltTableChanged = true;
            }
        }

        private void BoltsTable_BeginningEdit(object sender, DataGridBeginningEditEventArgs e) {
            BoltTableOldValue = (e.Column.GetCellContent(e.Row) as TextBlock).Text;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e) {
            Notice();
            Window win = (Window)this.Parent;
        }

        private void BoltLogsTable_BeginningEdit(object sender, DataGridBeginningEditEventArgs e) {
            LogTableOldValue = (e.Column.GetCellContent(e.Row) as TextBlock).Text;
        }

        private void BoltLogsTable_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e) {
            TextBox textBox = e.EditingElement as TextBox;
            LogTableNewValue = textBox.Text;
            if (LogTableNewValue !=  LogTableOldValue)
            {
                isLogTableChanged = true;
                
                textBox.Foreground = new SolidColorBrush(Colors.Red);
                textBox.Background = new SolidColorBrush(Colors.Blue);
            }
        }

        private void DelRow() {
            if (currentTable == Table.BoltsTable)
            {
                BoltModel boltModel = BoltsTable.SelectedItem as BoltModel;
                if (boltModel != null)
                {
                    BoltList.Remove(boltModel);
                    isBoltTableChanged = true;
                    BoltsTable.SelectedItem = null;
                    BoltsTable.ItemsSource = null;
                    BoltsTable.ItemsSource = BoltList;
                }
            }
            else if (currentTable == Table.BoltLogsTable)
            {
                BoltLogModel boltLogModel = BoltLogsTable.SelectedItem as BoltLogModel;
                if (boltLogModel != null)
                {
                    BoltLogList.Remove(boltLogModel);
                    isLogTableChanged = true;
                    BoltLogsTable.SelectedItem = null;
                    BoltLogsTable.ItemsSource = null;
                    BoltLogsTable.ItemsSource = BoltLogList;
                }
            }
        }
        private void DeleteBtn_Click(object sender, RoutedEventArgs e) {
            DelRow(); 
        }
    }
}
