using System.Threading.Tasks;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using System.Threading;
using System.Windows;
using System;

namespace MainWpfApp.ViewModels {
    public class WavePlotModel {

        public PlotModel LWavePlotModel { get; set; }   // 纵波
        public PlotModel TWavePlotModel { get; set; }   // 横波
        private int MaxWaveSize;                        // 最大波采集深度
        private LinearAxis xAxisL;                      // 纵波横坐标
        private LinearAxis yAxisL;                      // 纵波纵坐标
        public MainWindow mainwin;                      // 主窗口
        public LineSeries LWave { get; set; }
        public LineSeries ZeroWave { get; set; }

        public WavePlotModel() {
            mainwin = (MainWindow)Application.Current.MainWindow;
            MaxWaveSize = mainwin.MaxSize;
        }

        /// <summary>
        /// 初始化波形图 绘制零应力参考波形
        /// </summary>
        public void Init() {
            // mainwin.TransversePlot.DataContext = mainwin.wavePlotModel;
            mainwin.LongitudinalPlot.DataContext = mainwin.WavePlotModel;
            LWavePlotModel = new PlotModel();
            // TWavePlotModel = new PlotModel();

            /** 纵波 **/
            yAxisL = new LinearAxis()
            {
                /* y轴 */
                Position = AxisPosition.Left,
                Minimum = -100,
                Maximum = 100,
                Title = "回波强度",
                TitlePosition = 0.5,
                MinorGridlineStyle = LineStyle.Solid,
                MajorGridlineStyle = LineStyle.Solid,
                AbsoluteMaximum = 110,
                AbsoluteMinimum = -110,
            };
            xAxisL = new LinearAxis()
            {
                /* x轴 */
                Position = AxisPosition.Bottom,
                Minimum = 0,
                Maximum = MaxWaveSize,
                Title = "长度",
                TitlePosition = 0.5,
                MinorGridlineStyle = LineStyle.Solid,
                MajorGridlineStyle = LineStyle.Solid,
                TickStyle = TickStyle.Inside,
                AbsoluteMaximum = 8200,
                AbsoluteMinimum = 0,
            };
            
            LWavePlotModel.Axes.Add(yAxisL);
            LWavePlotModel.Axes.Add(xAxisL);
            LWavePlotModel.Title = "纵波波形";
            
            PrintStressWave();
            PrintZeroStressWave();
        }

        /// <summary>
        /// 绘制零应力波形
        /// </summary>
        public void PrintZeroStressWave() {
            // 纵波零力波形绘制
            ZeroWave = new LineSeries() { Title = "参考波形", InterpolationAlgorithm = InterpolationAlgorithms.CanonicalSpline };

            LWavePlotModel.Series.Add(ZeroWave);
        }

        /// <summary>
        /// 板卡波形图绘制
        /// </summary>
        public void PrintStressWave() {
            LWave = new LineSeries() { Title = "实际波形", InterpolationAlgorithm = InterpolationAlgorithms.CanonicalSpline };
           
            LWavePlotModel.Series.Add(LWave);
            /** 绘图逻辑在 TcpClient 中获取波形数据部分**/
        }

        /// <summary>
        /// 取纵波X轴当前起始点 取其小数部分 小于0返回0
        /// </summary>
        /// <returns></returns>
        public int GetLWaveXStart() {
            int start = (int)xAxisL.ActualMinimum;

            if (start >= 0 && start < MaxWaveSize) {
                return start;
            }
            return 0;
        }

        /// <summary>
        /// 取纵波X轴当前结束 取其小数部分 小于0返回0
        /// </summary>
        /// <returns></returns>
        public int GetLWaveXEnd() {
            int end = (int)xAxisL.ActualMaximum;
            if (end > 0 && end < MaxWaveSize) {
                return end;
            }
            return MaxWaveSize; 
        }


    }
   
   
 
   
}
