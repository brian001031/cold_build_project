using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PurchaseWarehousing
{
     /*
        ※Out 與 ref 都是以 By Reference 作為參數傳遞
        ※Out 與 ref 不同點
        ● ref 必須先將參數做初始化而Out則不需要
        ● out 一定要修改傳入的參數 而 ref 則不用
        ● ref 需要在執行前初始化參數(給值)而 out 是在程式結束前需要初始化參數(給值)
     */

    public class Time
    {
        //宣告此Time 的各類參數
        private int Year;
        private int Month;
        private int Date;
        private int Hour;
        private int Minute;
        private int Second;
              
        //執行顯示一開始時間狀態
        public void DisplayCurrentTime()
        {
            /*
            System.Console.WriteLine("{0}/{1}/{2} {3}:{4}:{5}",
            Month, Date, Year, Hour, Minute, Second);
            */ 
        }

        //取得小時
        public int GetHour()
        {
            return Hour;
        }
        public void GetTime_Out(out int h, out int m, out int s)
        {
            h = Hour;
            m = Minute;
            s = Second;
        }
        public void GetTime_REF(ref int h, ref int m, ref int s)
        {
            h = Hour;
            m = Minute;
            s = Second;
        }
        // 通用結構
        public Time(System.DateTime dt)
        {
            Year = dt.Year;
            Month = dt.Month;
            Date = dt.Day;
            Hour = dt.Hour;
            Minute = dt.Minute;
            Second = dt.Second;
        }
    }
}
