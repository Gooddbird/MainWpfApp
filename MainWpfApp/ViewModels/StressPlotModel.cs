using System;
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
            var xAxis = new TimeSpanAxis() {
                Position = AxisPosition.Bottom,
            };
            stressPlotModel.Axes.Add(xAxis);
        }
    }
}
