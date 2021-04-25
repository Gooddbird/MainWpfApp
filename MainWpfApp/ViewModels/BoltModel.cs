using MainWpfApp.Util;
using SQLite;

namespace MainWpfApp.ViewModels {

    [Table("t_bolts")]
    public class BoltModel : BindEx
    {

        private string _Bolt_id;// 螺栓项目id

        private string _Material;// 螺栓材质

        private double _Standards; // 螺栓等级标准

        private double _Stress_coefficient; // 应力系数

        private double _Bolt_length; // 螺栓长度

        private double _Clamp_length;// 夹持长度

        private double _Nominal_diameter; //公称直径

        private string _Description;    // 备注

        [PrimaryKey]
        public string Bolt_id {
            get { return _Bolt_id; }
            set { _Bolt_id = value;
                OnPropertyChanged("Proj_id");
            }
        }         

        public string Material {
            get { return _Material; }
            set { _Material = value;
                OnPropertyChanged("Material");
            }
        }        

        public double Standards {
            get { return _Standards; }
            set { _Standards = value;
                OnPropertyChanged("Standards");
            }
        }      

        public double Stress_coefficient {
            get { return _Stress_coefficient; }
            set { _Stress_coefficient = value;
                OnPropertyChanged("Stress_coefficient");
            }
        } 

        public double Bolt_length {
            get { return _Bolt_length; }
            set { _Bolt_length = value;
                OnPropertyChanged("Bolt_length");
            }
        }    

        public double Clamp_length {
            get { return _Clamp_length; }
            set { _Clamp_length = value;
                OnPropertyChanged("Clamp_length");
            }
        }    

        public double Nominal_diameter
        {
            get { return _Nominal_diameter; }
            set
            {
                _Nominal_diameter = value;
                OnPropertyChanged("Nominal_diameter");
            }
        } 

        public string Description
        {
            get { return _Description; }
            set
            {
                _Description = value;
                OnPropertyChanged("Description");
            }
        }
    }
   
}
