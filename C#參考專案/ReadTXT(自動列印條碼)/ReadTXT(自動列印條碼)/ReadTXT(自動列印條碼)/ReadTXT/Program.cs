using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;


namespace ReadTXT
{
    static class Program
    {

        static PrintBarcodeList print;


        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] sArray)
        {       
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
 
            //修改成一開始啟動接收字串，自動列印條碼           
             print = new PrintBarcodeList(sArray);
             Application.Run(print);

            //手動列印條碼
            // Application.Run(new ReadTxt(sArray));       
     
        }
    }
}
