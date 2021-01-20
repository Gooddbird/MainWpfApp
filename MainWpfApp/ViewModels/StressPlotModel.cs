using System;
using OxyPlot;
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
            var funcSerial = new FunctionSeries((x) => { return Math.Sin(x); }, 0, 10, 0.1, "y=sin(x)");
            stressPlotModel.Series.Add(funcSerial);
        }
    }
}
