using System;
using MathNet.Numerics.IntegralTransforms;
using MathNet.Filtering.FIR;
using System.IO;
using System.Collections.Generic;
using System.Linq;


public class UTSMath
    {

        /**************************参数***********************************/
        double[] zeroWaveData; //零应力波形
        //double[] zeroTransWaveData; //零应力横波波形
        double[] testWaveData; //测试波形(测量)
        //double[] testTransWaveData; //测试横波波形(测量)

        //double sampleTime; //采样间隔 ns
        //int interTimes; //插值倍数
        //double Ks; //轴力系数 MPa/ns
        //double KT; //温度系数 1/℃
        //double zeroWaveEchoTime; // 零轴力波形传播时间 ns
        //double zeroTransWaveEchoTime; // 零横波轴力波形传播时间 ns
        //double T1; //测试温度 ℃
        //double T0; //零应力温度 ℃(测量)
        //double boltLength; //螺栓长度 mm
        //double clamLength; //夹持长度 mm

        //滤波系数
        double[] filterNumerator = {0.0134169326548185,0.00483562665814739,
                                    0.00232339727148646,-0.00277233314801145,-0.00981376683727655,
                                    -0.0172941757607681,-0.023056978829635,-0.0246453390289033,
                                    -0.0198304360001989,-0.00719869469498182,0.013363387149358,
                                    0.0404277888364643,0.0711081023694416,0.101472719665053,
                                    0.127218323285673,0.144470795575703,0.150542077872666,
                                    0.144470795575703,0.127218323285673,0.101472719665053,
                                    0.0711081023694416,0.0404277888364643,0.013363387149358,-0.00719869469498182,
                                    -0.0198304360001989,-0.0246453390289033,-0.023056978829635,
                                    -0.0172941757607681,-0.00981376683727655,-0.00277233314801145,
                                    0.00232339727148646,0.00483562665814739,0.0134169326548185};

        /**************************以下为功能函数***********************************/
        /**
         * 计算螺栓轴向力 贾雪论文公式 纵波法
         * @param:
         *      zeroWaveData    零应力波形
         *      testWaveData    测试波形
         *      sampleTime      采样间隔 ns
         *      interTimes      插值倍数(2 4 8 16 32 64 128 256...)
         *      zeroWaveEchoTime 零轴力波形传播时间 ns
         *      Ks              轴力系数 MPa/ns
         *      KT              温度系数 1/℃
         *      T1              测试温度 ℃
         *      T0              零应力温度 ℃
         *      TDETech         时延估计算法 GCC GCCFZP FindPeak
         * 
         * @return stressTuple
         * [轴力(MPa),时延(),互相关系数]
         */
        public Tuple<double, double, double[]> GetBoltAxialForce_ZB_JX(
            double[] zeroWaveData, double[] testWaveData,
            double sampleTime, int interTimes, double zeroWaveEchoTime, 
            double Ks, double KT, double T1, double T0, string TDETech)
        {
            double stress = 9999.0;
            double TDE = 9999.0;
            double[] xcorr;
            Tuple<double, double, double[]> stressTuple;
            if (zeroWaveData.Length != testWaveData.Length)
            {
                Console.WriteLine("测试波形与基准波形长度不一致,轴力设置为默认值：9999.0");
                xcorr = new double[1];
                stressTuple = new Tuple<double, double, double[]>(stress,TDE,xcorr);
                return stressTuple;
            }
            /***********时延估计***********/

            if (TDETech == "GCC") //GCC
            {
                //插值
                double[] spl_zeroWaveData;
                double[] spl_testWaveData;
                spl_zeroWaveData = waveDataSplint(zeroWaveData, interTimes);
                spl_testWaveData = waveDataSplint(testWaveData, interTimes);
                // 时延估计
                var TDETuple = GCCTDE(spl_zeroWaveData, spl_testWaveData, sampleTime / interTimes);
                TDE = TDETuple.Item1;
                xcorr = TDETuple.Item2;
            }
            else if(TDETech == "GCCFZP") //GCCFZP
            {
                var TDETuple = GCC_FZPTDE(zeroWaveData, testWaveData, sampleTime, interTimes);
                TDE = TDETuple.Item1;
                xcorr = TDETuple.Item2;
            }
            else if(TDETech == "FindPeak") //FindPeak
            {
                //插值
                double[] spl_zeroWaveData;
                double[] spl_testWaveData;
                spl_zeroWaveData = waveDataSplint(zeroWaveData, interTimes);
                spl_testWaveData = waveDataSplint(testWaveData, interTimes);
                // 时延估计
                var TDETuple = FindPeakTDE(spl_zeroWaveData, spl_testWaveData, sampleTime / interTimes);
                TDE = TDETuple.Item1;
                xcorr = new double[1];
            }
            else
            {
                Console.WriteLine("时延估计方法选择错误, 轴力设置为默认值：9999.0");
                xcorr = new double[1];
                stressTuple = new Tuple<double, double, double[]>(stress, TDE, xcorr);
                return stressTuple;
            }


            // 螺栓轴向力 单位：MPa
            stress = Ks * (TDE - (zeroWaveEchoTime + TDE) * KT * (T1 - T0));

            stressTuple = new Tuple<double, double, double[]>(stress, TDE, xcorr);
            return stressTuple;
        }

        /**
         * 零相位延迟FIR滤波
         * @param:
         *      dataSerial          输入波形
         *      samplingFreq        采样率 Hz
         *      lowCutOff           低截至频率 Hz
         *      highCutOff          高截至频率 Hz
         * 
         * @return FIRResult        滤波后数据
         */
        public double[] ZeroPhaseFIR(double[] dataSerial, double samplingFreq, double lowCutOff, double highCutOff)
        {

            IList<double> coef = new List<double>();
            double[] hf = FirCoefficients.BandPass(samplingFreq, lowCutOff, highCutOff, 64); //获得滤波系数
            foreach (double number in hf)
            {
                coef.Add(number);
            }

            OnlineFirFilter filter = new OnlineFirFilter(coef); 
            double[] FIRResult = filter.ProcessSamples(dataSerial); //正向滤波
            FIRResult = filter.ProcessSamples(FIRResult.Reverse().ToArray()); //反向滤波
            FIRResult = FIRResult.Reverse().ToArray(); //反转

            return FIRResult;
        }



        /**
        * 查找数组的最大值索引
        * @param arr
        * 输入数组
        * @return i_Pos
        * 最大值索引
        */
        public int MaxIndex<T>(T[] arr) where T : IComparable<T>
        {
            var i_Pos = 0;
            var value = arr[0];
            for (var i = 1; i < arr.Length; ++i)
            {
                var _value = arr[i];
                if (_value.CompareTo(value) > 0)
                {
                    value = _value;
                    i_Pos = i;
                }
            }
            return i_Pos;
        }

        /**
        * 查找数组的最大值
        * @param arr
        * 输入数组
        * @return value
        * 最大值
        */
        public T MaxValue<T>(T[] arr) where T : IComparable<T>
        {
            var i_Pos = 0;
            var value = arr[0];
            for (var i = 1; i < arr.Length; ++i)
            {
                var _value = arr[i];
                if (_value.CompareTo(value) > 0)
                {
                    value = _value;
                    i_Pos = i;
                }
            }
            return value;
        }

        /**
         * L2范数
         * 
         */
        public double normL2(double[] arr)
        {
            double normL2 = 0;
            for (int i=0;i<arr.Length;i++)
            {
                normL2 = normL2 + arr[i] * arr[i];
            }
            normL2 = Math.Sqrt(normL2);
            return normL2;
        }

        /**
         * 广义互相关时延估计算法
         * @param zeroWaveData
         * 零应力波形
         * @param testWaveData
         * 测试应力波形
         * @param sampleTime
         * 采样间隔
         * @return TDETuple
         * [时延，互相关系数数组]
         * 时延，单位:s 拉应力时返回正数，压应力返回负数
         */
        public Tuple<double, double[]> GCCTDE(double[] zeroWaveData, double[] testWaveData, double sampleTime)
        {
            Tuple<double, double[]> TDETuple;
            double timeDelay = 9999.09;
            double[] abs_xCorr;
            if (zeroWaveData.Length != testWaveData.Length)
            {
                abs_xCorr = new double[128];
                TDETuple = new Tuple<double, double[]>(timeDelay, abs_xCorr);
                return TDETuple;
            }
            int realLength = zeroWaveData.Length;

            //将数据长度补零为2的幂
            int power = (int)(Math.Log(realLength-1) / Math.Log(2)) + 1;
            int fftDataLen = 1 << power;
            double[] pad_zeroWaveData = new double[fftDataLen]; //zeropadding
            double[] pad_testWaveData = new double[fftDataLen]; //zeropadding
            Array.Copy(zeroWaveData, pad_zeroWaveData, realLength);
            Array.Copy(testWaveData, pad_testWaveData, realLength);

            //傅里叶变换   
            MathNet.Numerics.Complex32[] zeroWaveFFTData = new MathNet.Numerics.Complex32[fftDataLen];
            MathNet.Numerics.Complex32[] testWaveFFTData = new MathNet.Numerics.Complex32[fftDataLen];
            for (int i = 0; i < fftDataLen; i++)  //转为复数
            {
                zeroWaveFFTData[i] = new MathNet.Numerics.Complex32((float)pad_zeroWaveData[i], 0);
                testWaveFFTData[i] = new MathNet.Numerics.Complex32((float)pad_testWaveData[i], 0);
            }
            Fourier.Forward(zeroWaveFFTData);//傅里叶变换
            Fourier.Forward(testWaveFFTData);//傅里叶变换

            //计算zeroWaveFFTData*(testWaveFFTData的共轭复数)
            MathNet.Numerics.Complex32[] xCorrFFT = new MathNet.Numerics.Complex32[fftDataLen];
            MathNet.Numerics.Complex32 Conj;
            for (int i = 0; i < fftDataLen; i++)
            {
                Conj = MathNet.Numerics.Complex32.Conjugate(testWaveFFTData[i]);
                xCorrFFT[i] = MathNet.Numerics.Complex32.Multiply(zeroWaveFFTData[i], Conj);
            }
            //逆傅里叶变换
            Fourier.Inverse(xCorrFFT);
            abs_xCorr = new double[fftDataLen];
            double scaling_factor = normL2(zeroWaveData) * normL2(testWaveData); //归一化缩放因子为2范数积
            for (int i = 0; i < fftDataLen; i++)
            {
                abs_xCorr[i] = MathNet.Numerics.Complex32.Abs(xCorrFFT[i]) ;
                abs_xCorr[i] = abs_xCorr[i] * Math.Sqrt(fftDataLen) / scaling_factor; //归一化
            }
            //查找abs_xCorr最大值对应位置
            int maxPos = MaxIndex(abs_xCorr);
            //计算时延 单位：s
            timeDelay = (fftDataLen - maxPos);
            if (timeDelay > (double)fftDataLen / 2)
            {
                timeDelay = -1 * maxPos;
            }
            timeDelay = timeDelay * sampleTime;

            TDETuple = new Tuple<double, double[]>(timeDelay, abs_xCorr);
            return TDETuple;
        }

        /**
         * 广义互相关频域补零时延估计算法
         * @param zeroWaveData
         * 零应力波形
         * @param testWaveData
         * 测试应力波形
         * @param sampleTime
         * 采样间隔
         * @param interTimes
         * 插值倍数
         * @return TDETuple
         * [时延，互相关系数数组]
         * 时延，单位:s 拉应力时返回正数，压应力返回负数
         */
        public Tuple<double, double[]> GCC_FZPTDE(double[] zeroWaveData, double[] testWaveData, double sampleTime,int interTimes)
        {
            Tuple<double, double[]> TDETuple;
            double timeDelay = 9999.09;
            double[] abs_xCorr;
            if (zeroWaveData.Length != testWaveData.Length)
            {
                abs_xCorr = new double[128];
                TDETuple = new Tuple<double, double[]>(timeDelay, abs_xCorr);
                return TDETuple;
            }
            int realLength = zeroWaveData.Length;

            //将数据长度补零为2的幂
            int fftDataLen = 1 << ((int)(Math.Log(realLength-1) / Math.Log(2)) + 1);
            double[] pad_zeroWaveData = new double[fftDataLen]; //zeropadding
            double[] pad_testWaveData = new double[fftDataLen]; //zeropadding
            Array.Copy(zeroWaveData, pad_zeroWaveData, realLength);
            Array.Copy(testWaveData, pad_testWaveData, realLength);

            //傅里叶变换
            MathNet.Numerics.Complex32[] zeroWaveFFTData = new MathNet.Numerics.Complex32[fftDataLen];
            MathNet.Numerics.Complex32[] testWaveFFTData = new MathNet.Numerics.Complex32[fftDataLen];
            for (int i = 0; i < fftDataLen; i++)  //转为复数
            {
                zeroWaveFFTData[i] = new MathNet.Numerics.Complex32((float)pad_zeroWaveData[i], 0);
                testWaveFFTData[i] = new MathNet.Numerics.Complex32((float)pad_testWaveData[i], 0);
            }
            Fourier.Forward(zeroWaveFFTData);//傅里叶变换
            Fourier.Forward(testWaveFFTData);//傅里叶变换

            //计算zeroWaveFFTData*(testWaveFFTData的共轭复数)
            MathNet.Numerics.Complex32[] xCorrFFT = new MathNet.Numerics.Complex32[fftDataLen* interTimes];
            MathNet.Numerics.Complex32 testWaveFFTDataConj;
            for (int i = 0; i < fftDataLen/2; i++) //正域赋值
            {
                testWaveFFTDataConj = MathNet.Numerics.Complex32.Conjugate(testWaveFFTData[i]);
                xCorrFFT[i] = MathNet.Numerics.Complex32.Multiply(zeroWaveFFTData[i], testWaveFFTDataConj);
            }
            int n = 0;
            for (int i = fftDataLen/2; i < fftDataLen ; i++) //负域赋值
            {
                n = i + (fftDataLen * interTimes - fftDataLen);
                xCorrFFT[n] = MathNet.Numerics.Complex32.Multiply(zeroWaveFFTData[i], MathNet.Numerics.Complex32.Conjugate(testWaveFFTData[i]));
            }
           
            //逆傅里叶变换
            Fourier.Inverse(xCorrFFT);
            fftDataLen = fftDataLen * interTimes;
            abs_xCorr = new double[fftDataLen];
            double scaling_factor = normL2(zeroWaveData) * normL2(testWaveData); //归一化缩放因子为2范数积
            for (int i = 0; i < fftDataLen; i++)
            {
                abs_xCorr[i] = MathNet.Numerics.Complex32.Abs(xCorrFFT[i]);
                abs_xCorr[i] = abs_xCorr[i] * (Math.Sqrt((float)fftDataLen)) / scaling_factor; //归一化
            }
            //查找abs_xCorr最大值对应位置
            int maxPos = MaxIndex(abs_xCorr);
            //计算时延 单位：s
            timeDelay = (fftDataLen - maxPos);
            if (timeDelay > (double)fftDataLen / 2)
            {
                timeDelay = -1 * maxPos;
            }
            timeDelay = timeDelay * sampleTime / interTimes;

            TDETuple = new Tuple<double, double[]>(timeDelay, abs_xCorr);
            return TDETuple;
        }


        /**
        * 寻峰时延估计算法
        * @param zeroWaveData
        * 零应力波形
        * @param testWaveData
        * 测试应力波形
        * @param sampleTime
        * 采样间隔
        * @return TDETuple
        * [时延]
        * 时延，单位:s 拉应力时返回正数，压应力返回负数
        */
        public Tuple<double> FindPeakTDE(double[] zeroWaveData, double[] testWaveData, double sampleTime)
        {
            Tuple<double> TDETuple;
            double timeDelay = 9999.09;
            //寻峰时延
            int zeroMaxIndex = MaxIndex(zeroWaveData);
            int testMaxIndex = MaxIndex(testWaveData);
            timeDelay = (testMaxIndex - zeroMaxIndex)* sampleTime;

            TDETuple = new Tuple<double>(timeDelay);
            return TDETuple;
        }


        /**************************以下为测试函数***********************************/

        /**
        * 从csv文件读取应力波形数据
        * @return waveData
        * 应力波形数据
        */
        public double[] readCsvZeroWaveData(string CSVFileName)
        {
            int waveLen = 0;
            String lineCSV;
            try
            {
                StreamReader reader = new StreamReader(CSVFileName);
                while (!reader.EndOfStream) //获取行数
                {
                    string str = reader.ReadLine();
                    waveLen++;
                }
                double[] waveDave = new double[waveLen];
                reader.Close();

                reader = new StreamReader(CSVFileName);
                for (int i = 0; i < waveLen; i++)
                {
                    lineCSV = reader.ReadLine();
                    String[] item = lineCSV.Split(',');//根据逗号切分
                    waveDave[i] = Convert.ToDouble(item[0]);
                }

                reader.Close();
                return waveDave;

            }
            catch (Exception e)
            {
                waveLen = 1000;
                Double[] waveDave = new Double[waveLen];
                for (int i = 0; i < waveLen; i++)
                {
                    waveDave[i] = 0.0;
                }
                Console.WriteLine("csv文件读取错误");
                Console.WriteLine(e.Message);
                return waveDave;
            }

        }

        /**
         * 生成延迟数据
         * @param ZeroWaveData
         * 零应力波形
         * @param timeDelay
         * 时间延迟 单位：格
         * @return testWaveData
         * 时延波形 延迟timeDelay格
         */
        public double[] SimuTestWaveData(double[] zeroWaveData, int timeDelay)
        {
            double[] testWaveData = new double[zeroWaveData.Length];
            for (int i = 0; i < zeroWaveData.Length; i++)
                testWaveData[i] = zeroWaveData[i];
            if (timeDelay >= 0)
                Array.Copy(zeroWaveData, 0, testWaveData, 0, timeDelay);
            if (zeroWaveData.Length - timeDelay >= 0)
                Array.Copy(zeroWaveData, 0, testWaveData, timeDelay, zeroWaveData.Length - timeDelay);
            return testWaveData;
        }

        /**
         * 将波形数据写入CSV
         * @param waveData
         * 应力波形
         * @param filsname
         * 文件名
         */
        public void writWaveDataToCSV(double[] waveData, String filsname)
        {
            try
            {
                StreamWriter writer = new StreamWriter(filsname);
                String outStr;
                for (int i = 0; i < waveData.Length; i++)
                {
                    outStr = String.Format("%.2f,\n", waveData[i]);
                    writer.Write(outStr);
                }

                writer.Close();
            }
            catch (Exception e)
            {
            }
        }

        /**
        * 三样条等间距插值
        * @param waveData
        * 波形数据
        * @param interTimes
        * 插值倍数
        * @return splintWaveData
        * 插值后波形数据，数据长度 waveData.length*interTimes
        */
        public double[] waveDataSplint(double[] waveData, int interTimes)
        {
            int waveLen = waveData.Length;
            int waveLen_inter = (waveLen - 1) * interTimes;
            double[] splintWaveData = new double[waveLen_inter];
            //生成时间序列
            double[] x = new double[waveLen];
            for (int i = 0; i < waveLen; i++)
            {
                x[i] = i * interTimes;
            }
            //插值
            MathNet.Numerics.Interpolation.IInterpolation splint;
            splint = MathNet.Numerics.Interpolate.CubicSpline(x, waveData);
            for (int i = 0; i < waveLen_inter; i++)
            {
                splintWaveData[i] = splint.Interpolate(i);
            }

            return splintWaveData;

        }

        /**取系统毫秒时（对应java的currentTimeMillis）*/
        public static long currentTimeMills()
        {
            return (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds);
        }

        /**
         * 插值时延估计测试
         */
        public void testSplGCCTDE()
        {

            double sampleTime = 1; //采样间隔
            int interTimes = 64; //插值倍数
            Console.WriteLine("--------------------------------------------------------------------------");
            Console.WriteLine("------------------------------读取数据----------------------------------");
            zeroWaveData = readCsvZeroWaveData(@"D:\CSharpWork\USTnetBolt\USTBolt_Server\SimWaveData8192.csv");
            Console.WriteLine("------------------------------生成模拟数据----------------------------------");
            testWaveData = SimuTestWaveData(zeroWaveData, 60);
            Console.WriteLine("------------------------------进行时延估计----------------------------------");
            while (true)
            {
                long startTime = currentTimeMills();
                // 插值时延估计
                int waveLen = zeroWaveData.Length;
                int spl_waveLen = waveLen * interTimes;
                /**************不同时延估计算法******************/
                //var TDETuple = GCCTDE(zeroWaveData, testWaveData, sampleTime); //无插值 时延估计
                //插值
                //double[] spl_zeroWaveData;
                //double[] spl_testWaveData;
                //spl_zeroWaveData = waveDataSplint(zeroWaveData, interTimes);
                //spl_testWaveData = waveDataSplint(testWaveData, interTimes);
                //// 时延估计
                //var TDETuple = GCCTDE(spl_zeroWaveData, spl_testWaveData, sampleTime / interTimes); //先插值后估计
                var TDETuple = GCC_FZPTDE(zeroWaveData, testWaveData, sampleTime, interTimes); //广义互相关频域补零时延估计
                /**********************************************/

                double timeDelay = TDETuple.Item1;
                double[] abs_xCorr = TDETuple.Item2;
                //获取运算时间
                long stopTime = currentTimeMills();
                double timeCost = (stopTime - startTime);
                Console.WriteLine("时延估计时间: " + timeDelay + "ms");
                Console.WriteLine("时延估计运算时间: " + timeCost + "ms");
                Console.WriteLine("最大互相关系数: " + MaxValue(abs_xCorr));
                Console.WriteLine("--------------------------------------------------------------------------");
            }
        }


        /***********************主函数**************************/
        /*static void Main(string[] args)
        {
            UTSMath utsMath = new UTSMath();
            utsMath.testSplGCCTDE();
        }*/
    }

