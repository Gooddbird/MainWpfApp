using MainWpfApp.ViewModels;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;

namespace MainWpfApp.Util {
    public class InitUtil : BindEx {

        public static BoltModel CurrentBolt { get; internal set; }
        public static MainWindow mainwin = (MainWindow)System.Windows.Application.Current.MainWindow;

        public static void SaveProjAsFun() {

        }
       
        public static void AddProjFun() {
            SaveFileDialog sfd = OpenSaveFileWin();
            if (sfd != null)
            {
                //获得保存文件的路径
                mainwin.Proj_path = sfd.FileName; //保存
                using (FileStream fsWrite = new FileStream(mainwin.Proj_path, FileMode.OpenOrCreate, FileAccess.Write))
                {

                }
            }
        }

        public static void OpenProjFun() {
            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                InitialDirectory = "c:\\",
                Filter = "db files (*.db)|*.db",
                FilterIndex = 1,
                RestoreDirectory = true
            };
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                // MessageBox.Show(openFileDialog1.FileName);
                mainwin.Proj_path = openFileDialog1.FileName;
                mainwin.Proj_name = Path.GetFileNameWithoutExtension(mainwin.Proj_path);
                mainwin.Proj_Name.Text = mainwin.Proj_name;
            }
        }

        private static bool OpenFileWindow() {
            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                InitialDirectory = "c:\\",
                Filter = "db files (*.db)|*.db|All files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true
            };
            return openFileDialog1.ShowDialog() == DialogResult.OK;
        }

        private static SaveFileDialog OpenSaveFileWin() {
            SaveFileDialog sfd = new SaveFileDialog
            {
                //设置保存文件对话框的标题
                Title = "请选择要保存的文件路径",
                Filter = "db files (*.db)|*.db",
                //保存对话框是否记忆上次打开的目录 
                RestoreDirectory = true,
            };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                return sfd;
            }
            else
            {
                return null;
            }
        }
    }

    /// <summary>
    /// 继承该类后会绑定双向事件通知
    /// </summary>
    public class BindEx : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        protected internal virtual void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
