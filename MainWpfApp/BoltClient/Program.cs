using System;


namespace USTBolt_Examp
{
    class USTBolt_Examp
    {
        /**取系统毫秒时（对应java的currentTimeMillis）*/
        public static long currentTimeMills()
        {
            return (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds);
        }


        /***********************主函数**************************/
        //static void Main(string[] args)
        //{
        //    Bolt CurrentBoltClient = new Bolt();
        //    /*************初始化**************/
        //    CurrentBoltClient.USTBDataInit();
        //    /*************连接板卡**************/
        //    while (true)
        //    {
        //        CurrentBoltClient.TcpConnect();
        //        if (CurrentBoltClient.TcpConnFlag == 1)
        //        {
        //            CurrentBoltClient.setPara();
        //            CurrentBoltClient.TcpClientThreadStart();
        //            break;
        //        }
        //    }

            
        //    /*************写入参数**************/
        //    //写入基准波形
        //    double[] waveDataTmp = CurrentBoltClient.utsMath.readCsvZeroWaveData(@"D:\CSharpWork\USTnetBolt\USTBolt_Client\SimWaveData8178.csv");
        //    Array.Copy(waveDataTmp, CurrentBoltClient.boltData.lstuintZeroWaveDataBuff[0], waveDataTmp.Length);
        //    Array.Copy(waveDataTmp, CurrentBoltClient.boltData.lstuintZeroWaveDataBuff[1], waveDataTmp.Length);
        //    //Array.Copy(waveDataTmp, 0, ustbClient.boltData.lstuintZeroWaveDataBuff[2], 0, waveDataTmp.Length);
        //    //Array.Copy(waveDataTmp, 0, ustbClient.boltData.lstuintZeroWaveDataBuff[3], 0, waveDataTmp.Length);
        //    //
        //    /*************下发设置**************/
        //    CurrentBoltClient.setPara();
        //    /*************进行轴力计算**************/
        //    CurrentBoltClient.StartStressCalThread();

        //    /*************模拟其他**************/
        //    long lastSysTime = currentTimeMills();
        //    while (true)
        //    {
        //        if((currentTimeMills() - lastSysTime) > 5000)
        //        {
        //            CurrentBoltClient.setPara();
        //            lastSysTime = currentTimeMills();
        //        }
        //    }
        //}
    }
}
