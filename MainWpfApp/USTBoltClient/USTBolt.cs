using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;
using OxyPlot;

public class USTBolt : TcpClient
{

    //固定参数
    int ChNum = 2; //通道数
    public int MAXWAVESIZE = 8178;//最大波形深度
    double sampleTime = 5; //采样间隔 ns

    //参数结构体
    public struct USTBData
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

    };
    public USTBData ustbData;

    //公用参数
    public long getWaveSysTime; //TcpClient 上次取得波形数据时间

    public UTSMath utsMath;

    //构造函数
    public USTBolt()
    {
        ustbData = new USTBData();
        utsMath = new UTSMath();
    }

    //USTBData结构体初始化
    public void USTBDataInit()
    {
        MAXWAVESIZE = mainwin.MaxSize;
        //轴力计算方法
        ustbData.stressCalTech = "ZB";  // 纵波法ZB 横纵法HZB 残余应力CYYL

        /****************页面显示数据*******************/
        ustbData.axialForce = 9999.99;  //轴力 MPa
        ustbData.timeDelay = 9999.99; //时延 ns
        ustbData.maxXcorr = 0.0; //最大互相关系数
        ustbData.maxTWXcorr = 0.0; //横波最大互相关系数
        ustbData.echoTime = 9999.99; //纵波回波时间
        ustbData.echoTransTime = 9999.99; //横波回波时间
        ustbData.timeRatio = 9999.99; //声时比

        ustbData.lstuintWaveDataBuff = new List<double[]>();
        ustbData.lstuintZeroWaveDataBuff = new List<double[]>();
        ustbData.lstWaveDataLen = new List<int>();
        double[] waveDataTmp = new double[MAXWAVESIZE];
        double[] zeroWaveDataTmp = new double[MAXWAVESIZE];
        for (int chInx = 0; chInx < ChNum; chInx++)
        {
            ustbData.lstuintWaveDataBuff.Add(waveDataTmp);      //波形数据 长度MAXWAVESIZE
            ustbData.lstuintZeroWaveDataBuff.Add(zeroWaveDataTmp);  //零应力波形数据 基准波形 长度MAXWAVESIZE
            ustbData.lstWaveDataLen.Add(MAXWAVESIZE);              //波形长度
        }

        ustbData.LWavaChIndx = 1; //纵波通道索引，起始为1,最大为ChNum
        ustbData.LWaveTDEStart = 1; //纵波时延估计起始点
        ustbData.LWaveTEDEnd = MAXWAVESIZE; //纵波时延估计结束点

        ustbData.TWavaChIndx = 2; //横波通道索引，起始为2,最大为ChNum
        ustbData.TWaveTEDStart = 1; //横波时延估计起始点
        ustbData.TWaveTEDEnd = MAXWAVESIZE; //横波时延估计结束点

        /****************板卡设置参数*******************/
        //全局参数
        ustbData.currChInx = 1; //当前采集通道 从1开始
        ustbData.pulsWidt = 200;  //激励脉宽 ns
        ustbData.exciVolt = 50;  //激励电压 V
        ustbData.prf = 500;      //激发频率 Hz
        ustbData.dataDepth = MAXWAVESIZE; //采集深度 byte
        ustbData.damping = 80; //阻抗 Ω
        //通道参数
        ustbData.lstTrigTimeDelay = new List<double>();
        ustbData.lstGain = new List<double>();
        for (int chInx = 0; chInx < ChNum; chInx++)
        {
            ustbData.lstTrigTimeDelay.Add(0); //延迟触发 ns
            ustbData.lstGain.Add(20);           //增益 dB
        }
        /****************UTMath参数*******************/
        ustbData.interTimes = 16; //插值倍数(2 4 6 8 16 32 64 128 ...)
        ustbData.Ks = 1; //轴力系数 MPa/ns
        ustbData.KT = 0; //温度系数 1/℃
        ustbData.zeroWaveEchoTime = 9999.99; // 零轴力波形传播时间 ns
        ustbData.zeroTransWaveEchoTime = 9999.99; // 零轴力横波形传播时间 ns
        ustbData.T1 = 25.00; //测试温度 ℃
        ustbData.T0 = 25.00; //零应力温度 ℃(测量)
        ustbData.boltLength = 150; //螺栓长度 mm
        ustbData.clamLength = 110; //夹持长度 mm

        ustbData.samplingFreq = 1 / sampleTime * 1e9;  //采样率 Hz 1/sampleTime*1e9
        ustbData.lowCutOff = 0.5e6; //滤波 低截至频率 Hz
        ustbData.highCutOff = 10e6; //滤波 高截至频率 Hz

    }

    /**
    * 将参数下发到板卡
    * 
    */
    public bool setPara()
    {
        if (tcpConnFlag == 0)
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
        bytesCMD[2] = (byte)(ustbData.pulsWidt / 10);
        //设置激励电压
        byte byteTmp;
        if (ustbData.exciVolt == 50)
            byteTmp = (byte)0x00;
        else if (ustbData.exciVolt == 100)
            byteTmp = (byte)0x01;
        else if (ustbData.exciVolt == 150)
            byteTmp = (byte)0x02;
        else if (ustbData.exciVolt == 200)
            byteTmp = (byte)0x03;
        else if (ustbData.exciVolt == 250)
            byteTmp = (byte)0x04;
        else if (ustbData.exciVolt == 300)
            byteTmp = (byte)0x05;
        else
        {
            Console.WriteLine("激励电压设置错误,默认-200V");
            byteTmp = (byte)0x03;
        }
        bytesCMD[3] = (byte)(byteTmp);
        //重复频率
        if (ustbData.prf == 25)
            byteTmp = (byte)0x00;
        else if (ustbData.prf == 50)
            byteTmp = (byte)0x01;
        else if (ustbData.prf == 100)
            byteTmp = (byte)0x02;
        else if (ustbData.prf == 250)
            byteTmp = (byte)0x03;
        else if (ustbData.prf == 500)
            byteTmp = (byte)0x04;
        else if (ustbData.prf == 1000)
            byteTmp = (byte)0x05;
        else if (ustbData.prf == 2000)
            byteTmp = (byte)0x06;
        else
        {
            Console.WriteLine("重复频率设置错误，默认100Hz");
            byteTmp = (byte)0x02;
        }
        bytesCMD[4] = (byte)(byteTmp);
        //波形采样深度
        //bytesCMD[5] = (byte)(ustbData.dataDepth );
        bytesCMD[5] = (byte)0x0A; //不可修改，默认16384个数据
        //阻抗
        if (ustbData.damping == 400)
            byteTmp = (byte)0x00;
        else if (ustbData.damping == 80)
            byteTmp = (byte)0x01;
        bytesCMD[6] = (byte)(byteTmp);
        //无效位
        bytesCMD[7] = (byte)0x00;
        //命令发送到板卡
        if (!tcpSendData(bytesCMD))
        {
            Console.WriteLine("设置全局参数失败");
            return false;
        }

        Thread.Sleep(100);
        /********通道参数***********/
        //通道
        int chinx = ustbData.currChInx - 1;
        bytesCMD[1] = (byte)(chinx + 1);
        //延迟出发时间
        int intTmp = (int)(ustbData.lstTrigTimeDelay[chinx] / 10);

        bytesCMD[2] = (byte)(intTmp >> 8);
        bytesCMD[3] = (byte)intTmp;
        //增益
        intTmp = (int)(ustbData.lstGain[chinx] / 0.1);
        bytesCMD[4] = (byte)(intTmp >> 8);
        bytesCMD[5] = (byte)intTmp;
        //无效位置
        bytesCMD[6] = (byte)0x00;
        bytesCMD[7] = (byte)0x00;
        //命令发送到板卡
        if (!tcpSendData(bytesCMD))
        {
            Console.WriteLine("第" + chinx + "通道设置失败");
            return false;
        }

        Console.WriteLine("通道命令：" + bytes2HexString(bytesCMD));
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
            if (tcpConnFlag != 0) //tcp未断开
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
                            datatmp += ustbData.lstuintWaveDataBuff[chinx][datai - 5 + i] + ustbData.lstuintWaveDataBuff[chinx][datai + 5 - i];
                        }
                        ustbData.lstuintWaveDataBuff[chinx][datai] = datatmp / (num * 2);
                    }
                }

                // 滤波
                for (int chinx = 0; chinx < ChNum; chinx++)
                {
                    ustbData.lstuintWaveDataBuff[chinx] = utsMath.ZeroPhaseFIR(ustbData.lstuintWaveDataBuff[chinx],
                        ustbData.samplingFreq, ustbData.lowCutOff, ustbData.highCutOff);
                }

                // 将处理波形写入文件 调试用
                //writWaveDataToCSV(ustbData.lstuintWaveDataBuff[ustbData.currChInx - 1],
                //    "D:\\CSharpWork\\USTnetBolt\\USTBolt_Client\\WaveData滤波后.CSV");
                //轴力计算
                if (ustbData.stressCalTech == "ZB") //纵波法
                {
                    int LWavaChIndx = ustbData.LWavaChIndx;
                    int LwaveLen = ustbData.lstWaveDataLen[LWavaChIndx];
                    int LwaveTDELen = ustbData.LWaveTEDEnd - ustbData.LWaveTDEStart;
                    double[] zeroWaveData = new double[LwaveTDELen];
                    double[] testWaveData = new double[LwaveTDELen];
                    Array.Copy(ustbData.lstuintWaveDataBuff[LWavaChIndx], ustbData.LWaveTDEStart, testWaveData, 0, LwaveTDELen);
                    Array.Copy(ustbData.lstuintZeroWaveDataBuff[LWavaChIndx], ustbData.LWaveTDEStart, zeroWaveData, 0, LwaveTDELen);

                    var stressTuple = utsMath.GetBoltAxialForce_ZB_JX(zeroWaveData, testWaveData,
                        sampleTime, ustbData.interTimes, ustbData.zeroWaveEchoTime,
                        ustbData.Ks, ustbData.KT, ustbData.T1, ustbData.T0, "GCCFZP");

                    ustbData.axialForce = stressTuple.Item1;
                    ustbData.timeDelay = stressTuple.Item2;
                    ustbData.maxXcorr = utsMath.MaxValue(stressTuple.Item3);

                    Console.WriteLine("纵波法，轴力：" + ustbData.axialForce); //调试用

                }
                else if (ustbData.stressCalTech == "HZB") //横纵波法
                {
                    Console.WriteLine("横纵波法");
                }
                else if (ustbData.stressCalTech == "CYYL") //残余应力
                {
                    Console.WriteLine("残余应力");
                }
                else
                {
                    Console.WriteLine("计算方法选择错误");

                    ustbData.axialForce = 9999.99;
                    ustbData.timeDelay = 9999.99;
                    ustbData.maxXcorr = 0.00;
                    Thread.Sleep(500);  //降低计算频率
                }

                if ((currentTimeMills() - getWaveSysTime) > 3000) //波形数据停止更新3秒
                {
                    Console.WriteLine("波形数据停止更新");
                }

                Thread.Sleep(30);  //降低计算频率
            }
            else
            {
                Console.WriteLine("StressCalThread: TCP断开连接");
                Thread.Sleep(1000);
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
            int ChInx = ustbData.currChInx - 1;
            ustbData.lstWaveDataLen[ChInx] = waveLen;
            for (int i = 0; i < waveLen; i++) //解析数据
            {
                ustbData.lstuintWaveDataBuff[ChInx][i] = bytesRead[i * 2 + 10] * 256 + bytesRead[i * 2 + 1 + 10] - 512;
                //ustbData.lstuintWaveDataBuff[ChInx][i] = ustbData.lstuintWaveDataBuff[ChInx][i] / 512 * 100; //转化为100%
            }
            /*  绘图 */ 
            try
            {
                mainwin.wavePlotModel.LWave.Points.Clear();
                int i = 0;
                var LWaveList = ustbData.lstuintWaveDataBuff[0];
                while (true)
                {
                    if (i == waveLen)
                    {
                        break;
                    }
                    mainwin.wavePlotModel.LWave.Points.Add(new DataPoint(i, LWaveList[i]));
                    i++;
                }
                mainwin.wavePlotModel.LWavePlotModel.InvalidatePlot(true);
            }
            catch (Exception) {
                mainwin.wavePlotModel.LWave.Points.Clear();
                mainwin.wavePlotModel.LWavePlotModel.InvalidatePlot(true);
            }

            getWaveSysTime = currentTimeMills(); //更新波形获取时间
            //其他操作 调试用
            //Console.WriteLine("波形数据格式解析完成，更新时间：" + getWaveSysTime.ToString());
            //writWaveDataToCSV(ustbData.lstuintWaveDataBuff[ChInx],
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
    public static long currentTimeMills()
    {
        return (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds);
    }

    /**将byte[]转换为HexString*/
    public String bytes2HexString(byte[] data)
    {
        StringBuilder sb = new StringBuilder(data.Length * 3);
        foreach (byte b in data)
        {
            sb.Append(Convert.ToString(b, 16).PadLeft(2, '0').PadRight(3, ' '));
        }

        return sb.ToString().ToUpper();
    }

    /*将byte[]转换为int*/
    public static int bytes2Int(byte[] bytes)
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
    public void writWaveDataToCSV(double[] waveData, string filsname)
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

