using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using System.Threading;
using System.Windows;

namespace MainWpfApp.ViewModels
{
    public class WavePlotModel {

        public PlotModel LWavePlotModel { get; set; }   // 纵波
        public PlotModel TWavePlotModel { get; set; }   // 横波
        public MainWindow mainwin;

        public WavePlotModel() {
            mainwin = (MainWindow)Application.Current.MainWindow;
        }

        /// <summary>
        /// 初始化波形图 绘制零应力参考波形
        /// </summary>
        public void Init() {
            mainwin.TransversePlot.DataContext = mainwin.wavePlotModel;
            mainwin.LongitudinalPlot.DataContext = mainwin.wavePlotModel;
            LWavePlotModel = new PlotModel();
            TWavePlotModel = new PlotModel();

            /** 纵波 **/
            var yAxisT = new LinearAxis()
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
            var xAxisT = new LinearAxis()
            {
                /* x轴 */
                Position = AxisPosition.Bottom,
                Minimum = 0,
                Maximum = 8178,
                Title = "长度",
                TitlePosition = 0.5,
                MinorGridlineStyle = LineStyle.Solid,
                MajorGridlineStyle = LineStyle.Solid,
            };
            int i = 0;
            // 纵波零力波形绘制
            var zeroWave = new LineSeries() { Title = "参考波形", InterpolationAlgorithm = InterpolationAlgorithms.CanonicalSpline };
            var zeroWaveList = mainwin.ustBolt.ustbData.lstuintZeroWaveDataBuff[0];
            Task.Factory.StartNew(() => {
                while (true)
                {
                    if (zeroWaveList == null)
                    {
                        Thread.Sleep(500);
                        continue;
                    }
                    if (i == 8178)
                    {
                        break;
                    }
                    zeroWave.Points.Add(new DataPoint(i, zeroWaveList[i]));
                    i++;
                    //LWavePlotModel.InvalidatePlot(true);
                    //Thread.Sleep(500);
                }
            });
            LWavePlotModel.Axes.Add(yAxisT);
            LWavePlotModel.Axes.Add(xAxisT);
            LWavePlotModel.Series.Add(zeroWave);
            LWavePlotModel.Title = "纵波波形";


            /** 横波波形图 **/
            var yAxisL = new LinearAxis()
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
            var xAxisL = new LinearAxis()
            {
                /* x轴 */
                Position = AxisPosition.Bottom,
                Minimum = 0,
                Maximum = 8178,
                Title = "长度",
                TitlePosition = 0.5,
                MinorGridlineStyle = LineStyle.Solid,
                MajorGridlineStyle = LineStyle.Solid,
            };
            TWavePlotModel.Axes.Add(yAxisL);
            TWavePlotModel.Axes.Add(xAxisL);
            TWavePlotModel.Title = "横波波形";

            PrintStressWave();
        }

        /// <summary>
        /// 板卡波形图绘制
        /// </summary>
        public void PrintStressWave() {
            var LWave = new LineSeries() { Title = "实际波形",  InterpolationAlgorithm = InterpolationAlgorithms.CanonicalSpline};
            Task.Factory.StartNew(()=> {
                while (true) {
                    int i = 0;
                    var LWaveList = mainwin.ustBolt.ustbData.lstuintWaveDataBuff[0];
                    while (true) {
                        if (i == 8178) {
                            break;
                        }
                        LWave.Points.Add(new DataPoint(i, LWaveList[i]));
                        i++;
                    }
                    Thread.Sleep(1000);
                }
            });
            LWavePlotModel.Series.Add(LWave);
        }

    }
   
   
 
   
}
