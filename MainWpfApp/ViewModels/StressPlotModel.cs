using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using SQLite;

namespace MainWpfApp.ViewModels {
    /*
     * 应力绘图模块
     * 
     */   
    public class StressPlotModel
    {
        public PlotModel stressPlot { get; set; }
        public SelectableScatterSeries stressWave { get; set; }
        public MainWindow mainwin;                                  // 主窗口
        public List<StressLogPoint> points;
        public LinearAxis xAxis;
        public LinearAxis yAxis;


        public StressPlotModel() {
            stressPlot = new PlotModel();
            mainwin = (MainWindow)Application.Current.MainWindow;
        }

        public void Init() {
            stressPlot.Title = "轴力测量结果";
            points = new List<StressLogPoint>();
            mainwin.StressPlot.DataContext = this;
            var start =  DateTimeAxis.ToDouble(DateTime.Today);
            // var start = DateTimeAxis.ToDouble(DateTime.Now.AddSeconds(10));
            var end = DateTimeAxis.ToDouble(DateTime.Now.AddDays(1));
            xAxis = new LinearAxis() {
                Position = AxisPosition.Bottom,
                Minimum = 0,
                Maximum = 100,
                AbsoluteMinimum = 0,
            };
            yAxis = new LinearAxis()
            {
                Position = AxisPosition.Left,
                Title = "轴力大小/MPa",
                Minimum = 0,
                Maximum = 2000,
                AbsoluteMaximum = 1000000,
                AbsoluteMinimum = -500000,
            };
            stressPlot.Axes.Add(xAxis);
            stressPlot.Axes.Add(yAxis);
            stressWave = new SelectableScatterSeries
            {
                MarkerType = MarkerType.Circle,
                ItemsSource = points,
                IsDataPointSelectable = true,
                TrackerFormatString = "序号: {X}\n测量时间: {TestTime}\n轴力: {Y} MPa\n时延: {timeDelay} ns\n相似度: {MaxXcorr}"
            };
            stressPlot.Series.Add(stressWave);
        }

    }

    /// <summary>
    /// 测量结果记录点
    /// </summary>
    public class StressLogPoint : IDataPointProvider {
        public double X { get; set; }   // 测量序号
        public double Y { get; set; }   // 轴力
        public string TestTime { get; set; } // 测量时间字符串表示
        public double TimeDelay { get; set; } // 时延
        public double MaxXcorr { get; set; } // 互相关系数 
        public int Id;                          // 数据库主键
        public StressLogPoint(double x, double y, string time,double timeDelay, double maxXcorr, int id) {
            X = x;
            Y = y;
            TestTime = time;
            TimeDelay = timeDelay;
            MaxXcorr = maxXcorr;
            Id = id;
        }
        public DataPoint GetDataPoint() {
            return new DataPoint(X, Y);
        }
    }

    public class SelectedScatterSeries : LineSeries {
    }

    /// <summary>
    /// 可选择的散点图，选中点标红
    /// </summary>
    public class SelectableScatterSeries : LineSeries {
        public bool IsDataPointSelectable { get; set; }

        // public ScatterPoint CurrentSelection { get; set; }

        public StressLogPoint CurrentSelection { get; set; }

        public OxyColor SelectedDataPointColor { get; set; } = OxyColors.Red;

        public double SelectedMarkerSize { get; set; }

        private StressLogPoint point;

        public SelectableScatterSeries() {
            SelectedMarkerSize = MarkerSize;
            MouseDown += SelectableLineSeries_MouseDown;
            KeyDown += SelectableScatterSeries_KeyDown;
        }

        public void SelectableScatterSeries_KeyDown(object sender, OxyKeyEventArgs e) {
            if (!IsDataPointSelectable || e.Key != OxyKey.D || point == null)
            {
                return;
            }
            if (e.Key != OxyKey.D)
            {
                return;
            }
            try
            {
                string msg = string.Format("确认删除该测量结果吗？：\n时间：{0}\n 轴力：{1}", point.TestTime, point.Y.ToString());
                if (MessageBox.Show(msg, "提示", MessageBoxButton.YesNo) == MessageBoxResult.No)
                {
                    return;
                }

                // 轴力图消除点
                var activeSeries = (sender as Series);
                var currentPlotModel = activeSeries.PlotModel;
                ClearCurrentSelection(currentPlotModel);
                MainWindow win = (MainWindow)Application.Current.MainWindow;
                win.StressPlotModel.points.Remove(point);
                win.StressPlotModel.stressPlot.InvalidatePlot(true);

                // db删除点
                win.db.Delete<BoltLogModel>(point.Id);
            }
            catch (SQLiteException)
            {
                MessageBox.Show("数据库删除失败，请重试");
            }
            catch (Exception) {
                MessageBox.Show("删除失败，请重试");
            }
        }

        /// <summary>
        /// 定义鼠标点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectableLineSeries_MouseDown(object sender, OxyMouseDownEventArgs e) {
            if (IsDataPointSelectable)
            {
                var activeSeries = (sender as OxyPlot.Series.Series);
                var currentPlotModel = activeSeries.PlotModel;
                var nearestPoint = activeSeries.GetNearestPoint(e.Position, false);
                point = (StressLogPoint)nearestPoint.Item;
                CurrentSelection = point;
                currentPlotModel = ClearCurrentSelection(currentPlotModel);
                List<StressLogPoint> items = new List<StressLogPoint>();
                items.Add(point);
                var selectedSeries = new SelectedScatterSeries
                {
                    MarkerSize = MarkerSize + 2,
                    MarkerFill = SelectedDataPointColor,
                    MarkerType = MarkerType,
                    ItemsSource = items,
                    TrackerFormatString = "序号: {X}\n测量时间: {TestTime}\n轴力: {Y} MPa\n时延: {timeDelay} ns\n相似度: {MaxXcorr}"
                };

                currentPlotModel.Series.Add(selectedSeries);
                currentPlotModel.InvalidatePlot(true);
            }
        }

        /// <summary>
        /// 清除所选 点
        /// </summary>
        /// <param name="plotModel"></param>
        /// <returns></returns>
        private PlotModel ClearCurrentSelection(PlotModel plotModel) {
            while (plotModel.Series.Any(x => x is SelectedScatterSeries))
            {
                plotModel.Series.Remove(plotModel.Series.First(x => x is SelectedScatterSeries));
            }
            return plotModel;
        }

    }
}
