﻿using System;


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
        //    USTBolt CurrentUstBolt = new USTBolt();
        //    /*************初始化**************/
        //    CurrentUstBolt.USTBDataInit();
        //    /*************连接板卡**************/
        //    while (true)
        //    {
        //        CurrentUstBolt.tcpConnect();
        //        if (CurrentUstBolt.tcpConnFlag == 1)
        //        {
        //            CurrentUstBolt.setPara();
        //            CurrentUstBolt.tcpClientThreadStart();
        //            break;
        //        }
        //    }

            
        //    /*************写入参数**************/
        //    //写入基准波形
        //    double[] waveDataTmp = CurrentUstBolt.utsMath.readCsvZeroWaveData(@"D:\CSharpWork\USTnetBolt\USTBolt_Client\SimWaveData8178.csv");
        //    Array.Copy(waveDataTmp, CurrentUstBolt.ustbData.lstuintZeroWaveDataBuff[0], waveDataTmp.Length);
        //    Array.Copy(waveDataTmp, CurrentUstBolt.ustbData.lstuintZeroWaveDataBuff[1], waveDataTmp.Length);
        //    //Array.Copy(waveDataTmp, 0, ustbClient.ustbData.lstuintZeroWaveDataBuff[2], 0, waveDataTmp.Length);
        //    //Array.Copy(waveDataTmp, 0, ustbClient.ustbData.lstuintZeroWaveDataBuff[3], 0, waveDataTmp.Length);
        //    //
        //    /*************下发设置**************/
        //    CurrentUstBolt.setPara();
        //    /*************进行轴力计算**************/
        //    CurrentUstBolt.StartStressCalThread();

        //    /*************模拟其他**************/
        //    long lastSysTime = currentTimeMills();
        //    while (true)
        //    {
        //        if((currentTimeMills() - lastSysTime) > 5000)
        //        {
        //            CurrentUstBolt.setPara();
        //            lastSysTime = currentTimeMills();
        //        }
        //    }
        //}
    }
}
