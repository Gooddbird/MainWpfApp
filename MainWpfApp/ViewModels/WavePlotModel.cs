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

            var yAxis = new LinearAxis()
            {
                Position = AxisPosition.Left,
                Minimum = -100,
                Maximum = 100,
                Title = "回波强度",
                TitlePosition = 0.5,
                MinorGridlineStyle = LineStyle.Solid,
                MajorGridlineStyle = LineStyle.Solid,
            };
            var xAxis = new LinearAxis()
            {
                Position = AxisPosition.Bottom,
                Minimum = 0,
                Maximum = 60,
                Title = "长度(mm)",
                TitlePosition = 0.5,
                MinorGridlineStyle = LineStyle.Solid,
                MajorGridlineStyle = LineStyle.Solid,
            };
            //线条
            //var lineSerial = new LineSeries() { Title = "直线实例"};
            //lineSerial.Points.Add(new DataPoint(0, 0));
            //lineSerial.Points.Add(new DataPoint(10, 10));
            SimplePlotModel.Axes.Add(yAxis);
            SimplePlotModel.Axes.Add(xAxis);
            //SimplePlotModel.Series.Add(lineSerial);
            SimplePlotModel.Title = "横波波形";

            //函数sin(x)
            var funcSerial = new FunctionSeries((x) => { return Math.Sin(x); }, 0, 10, 0.1, "y=sin(x)");
            SimplePlotModel.Series.Add(funcSerial);
            SimplePlotModel_2 = SimplePlotModel;
            SimplePlotModel_2.Title = "纵波波形";
        }
    }
   
   
 
   
}
