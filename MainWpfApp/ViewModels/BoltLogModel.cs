using MainWpfApp.Util;
using SQLite;

namespace MainWpfApp.ViewModels {
    [Table("t_bolt_logs")]
    public class BoltLogModel : BindEx{

        private int _Id;    // 主键
        private string _Bolt_id;// 螺栓项目id
        private float _AxialForce; // 轴力
        private float _TimeDelay; // 时延
        private float _MaxXcorr; // 互相关系数 
        private string _TestTime; //测量时间

        [PrimaryKey][AutoIncrement]
        public int Id {
            get { return _Id; }
            set {
                _Id = value;
                OnPropertyChanged("Id");
            }
        }

        [NotNull][Indexed]
        public string Bolt_id {
            get { return _Bolt_id; }
            set
            {
                _Bolt_id = value;
                OnPropertyChanged("Proj_id");
            }
        }
        public float AxialForce {
            get { return _AxialForce; }
            set { _AxialForce = value;
                OnPropertyChanged("axialForce");
            }
        }
        public float TimeDelay{
            get { return _TimeDelay; }
            set { _TimeDelay= value;
                OnPropertyChanged("axialForce");
            }
        }
        public float MaxXcorr{
            get { return _MaxXcorr; }
            set { _MaxXcorr = value;
                OnPropertyChanged("MaxXcorr");
            }
        }

        [NotNull][Indexed]
        public string TestTime {
            get { return _TestTime; }
            set { _TestTime = value;
                OnPropertyChanged("TestTime");
            }
        }


    }
}
