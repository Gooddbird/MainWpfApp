using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;
using OxyPlot;
using System.Windows;

public class Bolt : TcpClient
{

    //固定参数
    int ChNum = 2; //通道数
    public int MAXWAVESIZE = 8178;//最大波形深度
    double sampleTime = 5; //采样间隔 ns
    bool IsZeroBuf = true;  // 当前采集数据是否为零应力波形数据

    //参数结构体
    public struct BoltData
    {
        //轴力计算方法
        public string stressCalTech;  // 纵波法ZB 横纵法HZB 残余应力CYYL

        /****************页面显示数据*******************/
        public double axialForce;  //轴力 MPa
        public double timeDelay; //时延 ns
        public double maxXcorr; //波形相似度
        public double maxTWXcorr; //横波最大互相关系数
        public double echoTime; //纵波回波时间
        public double echoTransTime; //横波回波时间
        public double timeRatio; //声时比
        public List<double[]> lstuintWaveDataBuff; //波形数据 长度MAXWAVESIZE
        public List<double[]> lstuintZeroWaveDataBuff; //零应力波形数据 基准波形 长度MAXWAVESIZE
        public List<int> lstWaveDataLen; //波形长度

        public int LWavaChIndx; //纵波通道索引，起始为1,最大为ChNum
        public int LWaveTDEStart; //纵波时延估计起始点
        public int LWaveTEDEnd; //纵波时延估计结束点

        public int TWavaChIndx; //横波通道索引，起始为1,最大为ChNum
        public int TWaveTEDStart; //横波时延估计起始点
        public int TWaveTEDEnd; //横波时延估计结束点

        /****************板卡设置参数*******************/
        //全局参数
        public double pulsWidt;  //激励脉宽 ns
        public double exciVolt;  //激励电压 V
        public double prf;      //激发频率 Hz
        public double dataDepth; //采集深度 Kb
        public double damping; //阻抗 Ω
        //通道参数
        public List<double> lstTrigTimeDelay; //波形延迟 ns
        public List<double> lstGain; //增益 dB
        public int currChInx; //当前采集通道 从1开始

        /****************UTMath参数*******************/
        public int interTimes; //插值倍数（2 4 6 8 16 32 128 ...）固定
        public double Ks; //轴力系数 MPa/ns
        public double KT; //温度系数 1/℃
        public double zeroWaveEchoTime; // 零轴力波形传播时间 ns
        public double zeroTransWaveEchoTime; // 零轴力横波形传播时间 ns
        public double T1; //测试温度 ℃
        public double T0; //零应力温度 ℃(测量)
        public double boltLength; //螺栓长度 mm
        public double clamLength; //夹持长度 mm

        public double samplingFreq;  //采样率 Hz 1/sampleTime*1e9
        public double lowCutOff; //低截至频率 Hz 固定
        public double highCutOff; //高截至频率 Hz 固定

        public bool IsCanStartCal; // 是否可以开始测量

    };
    public BoltData boltData;

    //公用参数
    public long getWaveSysTime; //TcpClient 上次取得波形数据时间

    public BoltMath utsMath;

    //构造函数
    public Bolt()
    {
        boltData = new BoltData();
        utsMath = new BoltMath();
    }

    //USTBData结构体初始化
    public void USTBDataInit()
    {
        MAXWAVESIZE = mainwin.MaxSize;
        //轴力计算方法
        boltData.stressCalTech = "ZB";  // 纵波法ZB 横纵法HZB 残余应力CYYL

        /****************页面显示数据*******************/
        boltData.axialForce = 9999.99;  //轴力 MPa
        boltData.timeDelay = 9999.99; //时延 ns
        boltData.maxXcorr = 0.0; //最大互相关系数
        boltData.maxTWXcorr = 0.0; //横波最大互相关系数
        boltData.echoTime = 9999.99; //纵波回波时间
        boltData.echoTransTime = 9999.99; //横波回波时间
        boltData.timeRatio = 9999.99; //声时比

        boltData.lstuintWaveDataBuff = new List<double[]>();
        boltData.lstuintZeroWaveDataBuff = new List<double[]>();
        boltData.lstWaveDataLen = new List<int>();
        double[] waveDataTmp = new double[MAXWAVESIZE];
        double[] zeroWaveDataTmp = new double[MAXWAVESIZE];
        for (int chInx = 0; chInx < ChNum; chInx++)
        {
            boltData.lstuintWaveDataBuff.Add(waveDataTmp);      //波形数据 长度MAXWAVESIZE
            boltData.lstuintZeroWaveDataBuff.Add(zeroWaveDataTmp);  //零应力波形数据 基准波形 长度MAXWAVESIZE
            boltData.lstWaveDataLen.Add(MAXWAVESIZE);              //波形长度
        }

        boltData.LWavaChIndx = 1; //纵波通道索引，起始为1,最大为ChNum
        boltData.LWaveTDEStart = 1; //纵波时延估计起始点
        boltData.LWaveTEDEnd = MAXWAVESIZE; //纵波时延估计结束点

        boltData.TWavaChIndx = 2; //横波通道索引，起始为2,最大为ChNum
        boltData.TWaveTEDStart = 1; //横波时延估计起始点
        boltData.TWaveTEDEnd = MAXWAVESIZE; //横波时延估计结束点

        /****************板卡设置参数*******************/
        //全局参数
        boltData.currChInx = 1; //当前采集通道 从1开始
        boltData.pulsWidt = 200;  //激励脉宽 ns
        boltData.exciVolt = 50;  //激励电压 V
        boltData.prf = 500;      //激发频率 Hz
        boltData.dataDepth = MAXWAVESIZE; //采集深度 byte
        boltData.damping = 80; //阻抗 Ω
        //通道参数
        boltData.lstTrigTimeDelay = new List<double>();
        boltData.lstGain = new List<double>();
        for (int chInx = 0; chInx < ChNum; chInx++)
        {
            boltData.lstTrigTimeDelay.Add(0); //延迟触发 ns
            boltData.lstGain.Add(20);           //增益 dB
        }
        /****************UTMath参数*******************/
        boltData.interTimes = 16; //插值倍数(2 4 6 8 16 32 64 128 ...)
        boltData.Ks = 1; //轴力系数 MPa/ns
        boltData.KT = 0; //温度系数 1/℃
        boltData.zeroWaveEchoTime = 9999.99; // 零轴力波形传播时间 ns
        boltData.zeroTransWaveEchoTime = 9999.99; // 零轴力横波形传播时间 ns
        boltData.T1 = 25.00; //测试温度 ℃
        boltData.T0 = 25.00; //零应力温度 ℃(测量)
        boltData.boltLength = 150; //螺栓长度 mm
        boltData.clamLength = 110; //夹持长度 mm

        boltData.samplingFreq = 1 / sampleTime * 1e9;  //采样率 Hz 1/sampleTime*1e9
        boltData.lowCutOff = 0.5e6; //滤波 低截至频率 Hz
        boltData.highCutOff = 10e6; //滤波 高截至频率 Hz

        boltData.IsCanStartCal = false;

       // boltData.IsZeroBuf = true;
    }

    /**
    * 将参数下发到板卡
    * 
    */
    public bool SetPara()
    {
        if (TcpConnFlag == 0)
        {
            Console.WriteLine("setPara：TCP连接断开");
            return false;
        }
        /****************设置板卡参数*******************/
        /********全局参数***********/
        byte[] bytesCMD = new byte[8];

        //标志头
        bytesCMD[0] = (byte)0xff;
        bytesCMD[1] = (byte)0x00;
        //设置脉宽
        bytesCMD[2] = (byte)(boltData.pulsWidt / 10);
        //设置激励电压
        byte byteTmp;
        if (boltData.exciVolt == 50)
            byteTmp = (byte)0x00;
        else if (boltData.exciVolt == 100)
            byteTmp = (byte)0x01;
        else if (boltData.exciVolt == 150)
            byteTmp = (byte)0x02;
        else if (boltData.exciVolt == 200)
            byteTmp = (byte)0x03;
        else if (boltData.exciVolt == 250)
            byteTmp = (byte)0x04;
        else if (boltData.exciVolt == 300)
            byteTmp = (byte)0x05;
        else
        {
            Console.WriteLine("激励电压设置错误,默认-200V");
            byteTmp = (byte)0x03;
        }
        bytesCMD[3] = (byte)(byteTmp);
        //重复频率
        if (boltData.prf == 25)
            byteTmp = (byte)0x00;
        else if (boltData.prf == 50)
            byteTmp = (byte)0x01;
        else if (boltData.prf == 100)
            byteTmp = (byte)0x02;
        else if (boltData.prf == 250)
            byteTmp = (byte)0x03;
        else if (boltData.prf == 500)
            byteTmp = (byte)0x04;
        else if (boltData.prf == 1000)
            byteTmp = (byte)0x05;
        else if (boltData.prf == 2000)
            byteTmp = (byte)0x06;
        else
        {
            Console.WriteLine("重复频率设置错误，默认100Hz");
            byteTmp = (byte)0x02;
        }
        bytesCMD[4] = (byte)(byteTmp);
        //波形采样深度
        //bytesCMD[5] = (byte)(boltData.dataDepth );
        bytesCMD[5] = (byte)0x0A; //不可修改，默认16384个数据
        //阻抗
        if (boltData.damping == 400)
            byteTmp = (byte)0x00;
        else if (boltData.damping == 80)
            byteTmp = (byte)0x01;
        bytesCMD[6] = (byte)(byteTmp);
        //无效位
        bytesCMD[7] = (byte)0x00;
        //命令发送到板卡
        if (!TcpSendData(bytesCMD))
        {
            Console.WriteLine("设置全局参数失败");
            return false;
        }

        Thread.Sleep(100);
        /********通道参数***********/
        //通道
        int chinx = boltData.currChInx - 1;
        bytesCMD[1] = (byte)(chinx + 1);
        //延迟出发时间
        int intTmp = (int)(boltData.lstTrigTimeDelay[chinx] / 10);

        bytesCMD[2] = (byte)(intTmp >> 8);
        bytesCMD[3] = (byte)intTmp;
        //增益
        intTmp = (int)(boltData.lstGain[chinx] / 0.1);
        bytesCMD[4] = (byte)(intTmp >> 8);
        bytesCMD[5] = (byte)intTmp;
        //无效位置
        bytesCMD[6] = (byte)0x00;
        bytesCMD[7] = (byte)0x00;
        //命令发送到板卡
        if (!TcpSendData(bytesCMD))
        {
            Console.WriteLine("第" + chinx + "通道设置失败");
            return false;
        }

        Console.WriteLine("通道命令：" + Bytes2HexString(bytesCMD));
        Thread.Sleep(100);

        return true;
    }


    //开启轴力计算线程
    public void StartStressCalThread()
    {
        //创建一个轴力计算线程
        Thread stressThread;
        stressThread = new Thread(new ThreadStart(StressCalThread));
        stressThread.Start();
    }
    /************************以下为轴力线程**************************/
    /*********轴力计算线程*********/
    public void StressCalThread()
    {
        while (true)
        {
            if (mainwin.IsTesting == false) {
                return;
            }
            if (TcpConnFlag != 0) //tcp未断开
            {
                //跳点处理
                for (int chinx = 0; chinx < ChNum; chinx++)
                {
                    int[] jmpDataPot = { 1022, 3070, 5118, 7166 };
                    double datatmp = 0; int num = 5;
                    for (int j = 0; j < jmpDataPot.Length; j++)
                    {
                        int datai = jmpDataPot[j];
                        for (int i = 0; i < num; i++)
                        {
                            datatmp += boltData.lstuintWaveDataBuff[chinx][datai - 5 + i] + boltData.lstuintWaveDataBuff[chinx][datai + 5 - i];
                        }
                        boltData.lstuintWaveDataBuff[chinx][datai] = datatmp / (num * 2);
                    }
                }

                // 滤波
                //for (int chinx = 0; chinx < ChNum; chinx++)
                //{
                //    boltData.lstuintWaveDataBuff[chinx] = utsMath.ZeroPhaseFIR(boltData.lstuintWaveDataBuff[chinx],
                //        boltData.samplingFreq, boltData.lowCutOff, boltData.highCutOff);
                //}

                // 将处理波形写入文件 调试用
                //writWaveDataToCSV(boltData.lstuintWaveDataBuff[boltData.currChInx - 1],
                //    "D:\\CSharpWork\\USTnetBolt\\USTBolt_Client\\WaveData滤波后.CSV");
                //轴力计算
                if (boltData.stressCalTech == "ZB") //纵波法
                {
                    int LWavaChIndx = boltData.LWavaChIndx;
                    int LwaveLen = boltData.lstWaveDataLen[LWavaChIndx];
                    int LwaveTDELen = boltData.LWaveTEDEnd - boltData.LWaveTDEStart;
                    if (LwaveLen == 0 || LwaveTDELen == 0) {
                        Thread.Sleep(100);
                        continue;
                    }
                    double[] zeroWaveData = new double[LwaveTDELen];
                    double[] testWaveData = new double[LwaveTDELen];
                    Array.Copy(boltData.lstuintWaveDataBuff[LWavaChIndx], boltData.LWaveTDEStart, testWaveData, 0, LwaveTDELen);
                    Array.Copy(boltData.lstuintZeroWaveDataBuff[LWavaChIndx], boltData.LWaveTDEStart, zeroWaveData, 0, LwaveTDELen);

                    //var stressTuple = utsMath.GetBoltAxialForce_ZB_JX(zeroWaveData, testWaveData,
                    //    sampleTime, boltData.interTimes, boltData.zeroWaveEchoTime,
                    //    boltData.Ks, boltData.KT, boltData.T1, boltData.T0, "GCCFZP");

                    var stressTuple = utsMath.GetBoltAxialForce_ZB_JX(zeroWaveData, testWaveData,
                        sampleTime, boltData.interTimes, boltData.zeroWaveEchoTime,
                        boltData.Ks, boltData.KT, boltData.T1, boltData.T0, "GCC");
                    boltData.axialForce = stressTuple.Item1;
                    boltData.timeDelay = stressTuple.Item2;
                    boltData.maxXcorr = utsMath.MaxValue(stressTuple.Item3);

                    Console.WriteLine("纵波法，轴力：" + boltData.axialForce); //调试用

                }
                else if (boltData.stressCalTech == "HZB") //横纵波法
                {
                    Console.WriteLine("横纵波法");
                }
                else if (boltData.stressCalTech == "CYYL") //残余应力
                {
                    Console.WriteLine("残余应力");
                }
                else
                {
                    Console.WriteLine("计算方法选择错误");

                    boltData.axialForce = 9999.99;
                    boltData.timeDelay = 9999.99;
                    boltData.maxXcorr = 0.00;
                    Thread.Sleep(500);  //降低计算频率
                }

                if ((CurrentTimeMills() - getWaveSysTime) > 3000) //波形数据停止更新3秒
                {
                    // Console.WriteLine("波形数据停止更新");
                    MessageBox.Show("波形数据停止更新！");
                    return;
                }

                Thread.Sleep(30);  //降低计算频率
            }
            else
            {
                Console.WriteLine("StressCalThread: TCP断开连接");
                MessageBox.Show("服务端已断开连接！");
                // Thread.Sleep(1000);
                return;
            }
        }
    }

    //接收数据处理函数
    public override void RecDataHandle(byte[] mesBuffer, int bytesReadLen)
    {
        //从mesBuffer提取数据
        byte[] bytesRead = new byte[bytesReadLen];
        Array.Copy(mesBuffer, 0, bytesRead, 0, bytesReadLen);
        int dataLen = 16384; //波形长度
        int waveLen = (dataLen - 10 - 2 - 14 - 2) / 2;
        if (bytesReadLen != dataLen) //验证波形长度
        {
            //Console.WriteLine("波形长度错误"); //调试用
            return;
        }

        //处理数据
        byte[] heardByte = {(byte) 0xfe, (byte) 0xfe,
                            (byte) 0xfe, (byte) 0xfe,
                            (byte) 0xfe, (byte) 0xfe,
                            (byte) 0x00, (byte) 0x00,
                            (byte) 0x00, (byte) 0x00}; //10字节

        byte[] tipByte = { (byte)0xfe, (byte)0x02 }; //2字节
        byte[] endByte = { (byte)0xfe, (byte)0x03 }; //2字节

        byte[] bytesTmp = new byte[10];
        byte[] bytesTmp1 = new byte[2];
        byte[] bytesTmp2 = new byte[2];
        Array.Copy(bytesRead, 0, bytesTmp, 0, 10);  //数据头
        Array.Copy(bytesRead, (dataLen - 2 - 14 - 2), bytesTmp1, 0, 2); //tip位
        Array.Copy(bytesRead, dataLen - 2, bytesTmp2, 0, 2); //结束位

        /* **解析数据包*****/
        if (EqualsArray(bytesTmp, heardByte) &&
            EqualsArray(bytesTmp1, tipByte) &&
            EqualsArray(bytesTmp2, endByte)) //验证数据头 tip位 和 结束位
        {
            int ChInx = boltData.currChInx - 1;
            boltData.lstWaveDataLen[ChInx] = waveLen;
            for (int i = 0; i < waveLen; i++) //解析数据
            {
                boltData.lstuintWaveDataBuff[ChInx][i] = bytesRead[i * 2 + 10] * 256 + bytesRead[i * 2 + 1 + 10] - 512;
                //boltData.lstuintWaveDataBuff[ChInx][i] = boltData.lstuintWaveDataBuff[ChInx][i] / 512 * 100; //转化为100%
            }
            
            /*  绘图 */ 
            try
            {
                mainwin.WavePlotModel.LWave.Points.Clear();
                int i = 0;
                var LWaveList = boltData.lstuintWaveDataBuff[0];
                while (true)
                {
                    if (i == waveLen)
                    {
                        break;
                    }
                    mainwin.WavePlotModel.LWave.Points.Add(new DataPoint(i, LWaveList[i]));
                    i++;
                }
                mainwin.WavePlotModel.LWavePlotModel.InvalidatePlot(true);

                // 零应力波形绘制
                if (IsZeroBuf == true)
                {
                    for (int j = 0; j < ChNum; j++)
                    {
                        for (int t = 0; t < waveLen; t++)
                        {
                            boltData.lstuintZeroWaveDataBuff[j][t] = boltData.lstuintWaveDataBuff[j][t];
                        }
                    }
                    var zeroWaveList = boltData.lstuintZeroWaveDataBuff[0];
                    i = 0;
                    // mainwin.wavePlotModel.zeroWave.Points.Clear();
                    while (i < waveLen)
                    {
                        mainwin.WavePlotModel.ZeroWave.Points.Add(new DataPoint(i, zeroWaveList[i]));
                        i++;
                    }
                    mainwin.WavePlotModel.LWavePlotModel.InvalidatePlot(true);
                    IsZeroBuf = false;
                }
                boltData.IsCanStartCal = true;
            }
            catch (Exception) {
                mainwin.WavePlotModel.LWave.Points.Clear();
                mainwin.WavePlotModel.LWavePlotModel.InvalidatePlot(true);
            }
            
            getWaveSysTime = CurrentTimeMills(); //更新波形获取时间
            //其他操作 调试用
            //Console.WriteLine("波形数据格式解析完成，更新时间：" + getWaveSysTime.ToString());
            //writWaveDataToCSV(boltData.lstuintWaveDataBuff[ChInx],
            //    "D:\\CSharpWork\\USTnetBolt\\USTBolt_Client\\WaveData滤波前.CSV");
        }
        else
        {
            //调试用
            //Console.WriteLine("波形数据格式错误," +
            //    "数据头：" + bytes2HexString(bytesTmp) +
            //    "tip位：" + bytes2HexString(bytesTmp1) +
            //    "结束位：" + bytes2HexString(bytesTmp2));
            return;
        }
    }

    /************************辅助方法**************************/
    /**判断byte[]数组是否相等*/
    public static bool EqualsArray(byte[] bt1, byte[] bt2)
    {
        var len1 = bt1.Length;
        var len2 = bt2.Length;
        if (len1 != len2)
        {
            return false;
        }
        for (var i = 0; i < len1; i++)
        {
            if (bt1[i] != bt2[i])
                return false;
        }
        return true;
    }

    /**取系统毫秒时（对应java的currentTimeMillis）*/
    public static long CurrentTimeMills()
    {
        return (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds);
    }

    /**将byte[]转换为HexString*/
    public String Bytes2HexString(byte[] data)
    {
        StringBuilder sb = new StringBuilder(data.Length * 3);
        foreach (byte b in data)
        {
            sb.Append(Convert.ToString(b, 16).PadLeft(2, '0').PadRight(3, ' '));
        }

        return sb.ToString().ToUpper();
    }

    /*将byte[]转换为int*/
    public static int Bytes2Int(byte[] bytes)
    {
        int number = 0;
        for (int i = 0; i < 4; i++)
        {
            number += bytes[i] << i * 8;
        }
        return number;
    }

    /**
        * 将波形数据写入CSV
        * @param waveData
        * 应力波形
        * @param filsname
        * 文件名
        */
    public void WritWaveDataToCSV(double[] waveData, string filsname)
    {
        try
        {
            StreamWriter writer = new StreamWriter(filsname);
            string outStr;
            for (int i = 0; i < waveData.Length; i++)
            {
                outStr = waveData[i].ToString() + "\n";
                writer.Write(outStr);
            }

            writer.Close();
        }
        catch (Exception e)
        {
        }
    }

}

