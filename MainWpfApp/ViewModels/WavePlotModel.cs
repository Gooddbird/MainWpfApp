using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;

namespace MainWpfApp.ViewModels
{
    class WavePlotModel {

        public PlotModel SimplePlotModel { get; set; }
        public PlotModel SimplePlotModel_2 { get; set; }
        public WavePlotModel() {
            SimplePlotModel = new PlotModel();
            //线条
            var lineSerial = new LineSeries() { Title = "直线实例" };
            lineSerial.Points.Add(new DataPoint(0, 0));
            lineSerial.Points.Add(new DataPoint(10, 10));
            SimplePlotModel.Series.Add(lineSerial);

            //函数sin(x)
            var funcSerial = new FunctionSeries((x) => { return Math.Sin(x); }, 0, 10, 0.1, "y=sin(x)");
            SimplePlotModel.Series.Add(funcSerial);
            SimplePlotModel_2 = SimplePlotModel;
        }
    }
   
   
 
   
}
