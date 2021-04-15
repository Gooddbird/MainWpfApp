using System;
using System.Windows;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace MainWpfApp.ViewModels {
    /*
     * 应力绘图模块
     * 
     */   
    class StressPlotModel
    {
        public PlotModel stressPlotModel { get; set; }
        public StressPlotModel() {
            stressPlotModel = new PlotModel();
            Init(); 
        }

        public void Init() {
            var start = DateTimeAxis.ToDouble(DateTime.Now);
            var end = DateTimeAxis.ToDouble(DateTime.Now.AddMinutes(10));
            var xAxis = new DateTimeAxis() {
                Position = AxisPosition.Bottom,
                Minimum = start,
                Maximum = end,
            };
            var yAxis = new LinearAxis()
            {
                Position = AxisPosition.Left,
                Title = "轴力大小/MPa",
                Minimum = 0,
                Maximum = 2000,
            };
            stressPlotModel.Axes.Add(xAxis);
            stressPlotModel.Axes.Add(yAxis);

        }
    }
}
