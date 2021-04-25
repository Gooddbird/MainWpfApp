using System;
using System.Collections.Generic;
using System.Windows;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace MainWpfApp.ViewModels {
    /*
     * 应力绘图模块
     * 
     */   
    public class StressPlotModel
    {
        public PlotModel stressPlot { get; set; }
        public ScatterSeries stressWave { get; set; }
        public MainWindow mainwin;                      // 主窗口
        public List<StressLogPoint> points;
        public StressPlotModel() {
            stressPlot = new PlotModel();
            mainwin = (MainWindow)Application.Current.MainWindow;
        }

        public void Init() {
            points = new List<StressLogPoint>();
            mainwin.StressPlot.DataContext = this; 
            var start = DateTimeAxis.ToDouble(DateTime.Now);
            var end = DateTimeAxis.ToDouble(DateTime.Now.AddMinutes(1));
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
            stressPlot.Axes.Add(xAxis);
            stressPlot.Axes.Add(yAxis);
            stressWave = new ScatterSeries();
            stressWave.ItemsSource = points;
            stressWave.TrackerFormatString = "测量时间: {TestTime}\n轴力: {Y} MPa\n时延: {TimeDelay} ns\n相似度: {MaxXcorr}";
            stressPlot.Series.Add(stressWave);
        }
    }

    /// <summary>
    /// 测量结果记录点
    /// </summary>
    public class StressLogPoint : IScatterPointProvider {
        public double X { get; set; }   // 测量时间
        public double Y { get; set; }   // 轴力
        public string TestTime { get; set; } // 测量时间字符串表示
        public double TimeDelay { get; set; } // 时延
        public double MaxXcorr { get; set; } // 互相关系数 

        public StressLogPoint(double x, double y, double timeDelay, double maxXcorr) {
            X = x;
            Y = y;
            TimeDelay = timeDelay;
            MaxXcorr = maxXcorr;
            TestTime = DateTimeAxis.ToDateTime(x).ToString();
        }

        public ScatterPoint GetScatterPoint() {
            return new ScatterPoint(X, Y);
        }
    }
}
