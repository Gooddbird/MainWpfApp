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
        public LineSeries zeroWave { get; set; }

        public WavePlotModel() {
            mainwin = (MainWindow)Application.Current.MainWindow;
            MaxWaveSize = mainwin.MaxSize;
        }

        /// <summary>
        /// 初始化波形图 绘制零应力参考波形
        /// </summary>
        public void Init() {
            // mainwin.TransversePlot.DataContext = mainwin.wavePlotModel;
            mainwin.LongitudinalPlot.DataContext = mainwin.wavePlotModel;
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
            };
            
            LWavePlotModel.Axes.Add(yAxisL);
            LWavePlotModel.Axes.Add(xAxisL);
            LWavePlotModel.Title = "纵波波形";
            


            /** 横波波形图 **/
            //var yAxisT = new LinearAxis()
            //{
            //    /* y轴 */
            //    Position = AxisPosition.Left,
            //    Minimum = -100,
            //    Maximum = 100,
            //    Title = "回波强度",
            //    TitlePosition = 0.5,
            //    MinorGridlineStyle = LineStyle.Solid,
            //    MajorGridlineStyle = LineStyle.Solid,
            //};
            //var xAxisT = new LinearAxis()
            //{
            //    /* x轴 */
            //    Position = AxisPosition.Bottom,
            //    Minimum = 0,
            //    Maximum = MaxWaveSize, 
            //    Title = "长度",
            //    TitlePosition = 0.5,
            //    MinorGridlineStyle = LineStyle.Solid,
            //    MajorGridlineStyle = LineStyle.Solid,
            //};
            //TWavePlotModel.Axes.Add(yAxisT);
            //TWavePlotModel.Axes.Add(xAxisT);
            //TWavePlotModel.Title = "横波波形";
            
            PrintStressWave();
            PrintZeroStressWave();
        }

        /// <summary>
        /// 绘制零应力波形
        /// </summary>
        public void PrintZeroStressWave() {
            // 纵波零力波形绘制
            zeroWave = new LineSeries() { Title = "参考波形", InterpolationAlgorithm = InterpolationAlgorithms.CanonicalSpline };
            //int i = 0;
            //var zeroWaveList = mainwin.ustBolt.ustbData.lstuintZeroWaveDataBuff[0];
            //Task.Factory.StartNew(() => {
            //    while (true)
            //    {
            //        if (zeroWaveList == null)
            //        {
            //            Thread.Sleep(500);
            //            continue;
            //        }
            //        if (i == MaxWaveSize)
            //        {
            //            break;
            //        }
            //        zeroWave.Points.Add(new DataPoint(i, zeroWaveList[i]));
            //        i++;
            //    }
            //});
            LWavePlotModel.Series.Add(zeroWave);
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
            if (end >= 0 && end < MaxWaveSize) {
                return end;
            }
            return MaxWaveSize; 
        }


    }
   
   
 
   
}
