using System;
using System.Collections.Generic;
using System.Windows;
using MainWpfApp.ViewModels;

namespace MainWpfApp {
    /// <summary>
    /// Window1.xaml 的交互逻辑
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            // CreateTable();
            InitializeComponent();
        }
        public void CreateTable() {
            //await ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
            string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string dbpath = System.IO.Path.Combine(path, "test.db");
            // MessageBox.Show(dbpath);
            using (var db = new DbConnection(dbpath))
            {
                List<BoltModel> bolts = db.Bolts.Where(m => true).ToList();
                this.BoltsTable.ItemsSource = bolts;
            }
        }
    }

    
}
