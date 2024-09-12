using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

//使用執行續參考
using System.Threading;
using System.Threading.Tasks;

//新增輸出入連結參考
using System.IO;
using System.Globalization;

//新增加條碼連結參考
using BarcodeLib;
using ZXing.Common;
using ZXing;
using System.Drawing.Printing;



namespace ReadTXT
{
    public partial class PrintBarcodeList : Form
    {
        /**************全域變數宣告***************/
        string[] g_sCmdRecive;

        //搜尋表單路徑，預設桌面做測試
        string g_sSearchPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

        string g_sTipTopPath = "C:\\TIPTOP\\"; //ERP執行讀取列印條碼程式位置

        string g_sTemp = string.Empty;
        string g_FullTXTFile = string.Empty;

        DateTime g_DtNext;
        Thread g_thStartExport;
        private Font g_ptFont;
        private StreamReader g_StreamToPrint;

        //條碼列印次數
        int g_iBarTimes = 0;

        /***************宣告結束******************/
        public PrintBarcodeList(string[] args)
        {
            InitializeComponent();
            g_sCmdRecive = args;

            //清空欄位
            this.tbx_ErpSendChar.Text = "";

            //索引CMD 欄位數量
            for (int ilen = 0; ilen < g_sCmdRecive.Count(); ilen++)
            {
                //強制將字串轉成大寫，以免執行ERROR ，2017.03.17，Brian        
                g_sCmdRecive[ilen] = g_sCmdRecive[ilen].ToUpper();

                //字串累加
                g_sTemp += g_sCmdRecive[ilen] + "-";
            }

            if (g_sTemp.Length == 0)
            {
                g_sTemp = "沒有任何字串傳送 ! 請再次確認 ";
            }

            //重新排版
            g_sTemp = g_sTemp.Substring(0, g_sTemp.Length - 1);

            //顯示接收欄位
            this.tbx_ErpSendChar.Text = g_sTemp;

            this.tbx_ErpSendChar.ReadOnly = true;
        }

        private void PrintBarcodeList_Load(object sender, EventArgs e)
        {
            //一開始先動倒數模式
            g_DtNext = DateTime.Now.AddSeconds(5);
            tm_CountSpan.Start();
        }

        private void tm_CountSpan_Tick(object sender, EventArgs e)
        {
            TimeSpan tm_SpanRun = g_DtNext.Subtract(DateTime.Now);

            string sRunSec = Convert.ToString(tm_SpanRun.Seconds);



            //倒數完畢
            if (int.Parse(sRunSec) == 0)
            {
                tm_CountSpan.Enabled = false;
                lbl_TimeSpan.Text = "倒數計時完畢 !!!";

                //啟動轉成條碼表單格式並做列印準備
                g_thStartExport = new Thread(ExportBarcodeList);

                //多執行續設定Enable
                g_thStartExport.SetApartmentState(ApartmentState.MTA);

                //啟動執行
                g_thStartExport.Start();

            }
            else //正在倒數計時
            {
                int iRunSec = int.Parse(sRunSec);
                lbl_TimeSpan.Text = "倒數 " + sRunSec + " 秒 ";
            }
        }

        // ERP 指定 TXT 檔做轉換並將編號條碼一併呈現
        private void ExportBarcodeList()
        {
            FileInfo[] filesInfo = null;
            //DirectoryInfo di = new DirectoryInfo(g_sPathFile);
            string sTXTFilePath = string.Empty;    

            //測試正常OK!
            //MessageBox.Show("進入第一個執行續 TEST HERE!");        

            //產生Code128條碼
            Code128 c128 = new Code128();
            
            //圖形高度
            c128.Height = 50;
       
            //條碼圖像格式
            c128.ValueFont = new Font("Cambria", 10, FontStyle.Regular);

            //取第2筆ERP傳送字串 (txt 檔名不含路徑)
            string sSub = g_sCmdRecive[1].ToString().Substring(0, g_sCmdRecive[1].Length-4);

            System.Drawing.Image img = c128.GetCodeImage(sSub, Code128.Encode.Code128A);

            this.pbx_1arrayBarcode.Image = img;

            //讀取完整單據txt檔(桌面路徑) ->測試用
            //g_FullTXTFile = sTXTFilePath = g_sSearchPath + "\\" + g_sCmdRecive[1] + ".txt";
            //使用路徑 "C:\\TIPTOP\\"  ->ERP系統指定
           // g_FullTXTFile = sTXTFilePath = g_sTipTopPath + g_sCmdRecive[0] + ".txt";
            g_FullTXTFile = sTXTFilePath = g_sCmdRecive[0];

            try
            {
                //txt檔不存在
                if (!File.Exists(sTXTFilePath))
                {
                    MessageBox.Show(g_sCmdRecive[0] + "不存在");
                    return;
                }
                else
                {

                    //讀取編法採用default 預設
                    StreamReader sr = new StreamReader(sTXTFilePath, System.Text.Encoding.Default);

                    //===逐行讀取，直到檔尾===
                    while (!sr.EndOfStream)
                    {
                        //tbx_ReadALL.Text +=sr.ToString()+"\n";
                        tbx_Readtxt.Text += sr.ReadLine() + Environment.NewLine;
                    }

                    //MessageBox.Show("TXT檔讀取OK!");
                    lbl_Readtxt.Text = "TXT檔名: " + g_sCmdRecive[0].ToString() + " 讀取OK!";

                    g_thStartExport.Join(2000);

                    //啟動第2個執行續
                    g_thStartExport = new Thread(PreviewPrintList);
                    g_thStartExport.Start();
                }
            }
            catch (Exception cp)
            {
                //抓錯誤訊息
                MessageBox.Show(cp.Message, "讀取 TXT File 警告!!!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                throw cp;
            }
        }

        //執行列印前預覽畫面
        private void PreviewPrintList()
        {
            //MessageBox.Show("Test THEAD 2 WORK HERE!!!");
           
            try
            {
                g_StreamToPrint = new StreamReader(g_FullTXTFile, System.Text.Encoding.Default);
                
                try
                {
                    //純測試用
                    //printFont = new Font("Arial", 12);

                    //修正與原txt檔格式(細明體,12pt)
                    g_ptFont = new Font("MingLiU", 12);

                   // ptPvwDig1.Document = this.ptDoc1;

                    PrintDocument ptDoc = new PrintDocument();

                    ptDoc.PrintPage += new PrintPageEventHandler(this.ptDoc1_PrintPage);

                    if (ptPvwDig1.ShowDialog() == DialogResult.OK)
                    {
                        ptDoc.Print();

                        //g_thStartExport.Join(2000);                        
                        //this.Close();
                    }
                }
                finally
                {
                    g_StreamToPrint.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }        
        }

        private void ptDoc1_PrintPage(object sender, PrintPageEventArgs e)
        {
            float linesPerPage = 0;
            float yPos = 0;
            int count = 0, count2 = 0;

            float leftMargin = e.MarginBounds.Left;
            float topMargin = e.MarginBounds.Top;
            float topMarginRight = e.MarginBounds.Right;
            float yPosRight = 0;

            string sline = null;
            bool fCheck = false;

            //計算每頁的行數
            linesPerPage = e.MarginBounds.Height / g_ptFont.GetHeight(e.Graphics);


            //迴圈持續列印條件:當頁數讀超過而且文件檔已讀到尾端
            while (count < linesPerPage && ((sline = g_StreamToPrint.ReadLine()) != null))
            {
                g_iBarTimes++;

                if (g_iBarTimes == 1)
                {
                    //條碼寫入Print
                    yPosRight = leftMargin + (20 * g_ptFont.GetHeight(e.Graphics));
                    e.Graphics.DrawImage(this.pbx_1arrayBarcode.Image, yPosRight, 15);
                }

                yPos = topMargin + (count * g_ptFont.GetHeight(e.Graphics));
                e.Graphics.DrawString(sline, g_ptFont, Brushes.Black, leftMargin, yPos, new StringFormat());

                count++;
            }


             // If more lines exist, print another page.
            // 如果讀的行內容仍存在，繼續印下一頁
            if (sline != null)
            {
                e.HasMorePages = true;
            }
            else
            {
                if (g_iBarTimes == 0)
                {

                   //再讀一次，前次已讀取並釋出內容
                    if (!fCheck)
                        g_StreamToPrint = new StreamReader(g_FullTXTFile, System.Text.Encoding.Default);
 
                    //修正與原txt檔格式(細明體,12pt)
                    g_ptFont = new Font("MingLiU", 12);

                    while (count2 < linesPerPage && ((sline = g_StreamToPrint.ReadLine()) != null))
                    {

                        g_iBarTimes++;

                        if (g_iBarTimes == 1)
                        {
                            //條碼寫入Print
                            yPosRight = leftMargin + (20 * g_ptFont.GetHeight(e.Graphics));

                           e.Graphics.DrawImage(this.pbx_1arrayBarcode.Image, yPosRight, 10);
          
                            //將讀取Flag啟動
                            fCheck = true;
                        }


                        yPos = topMargin + (count2 * g_ptFont.GetHeight(e.Graphics));
                        e.Graphics.DrawString(sline, g_ptFont, Brushes.Black, leftMargin, yPos, new StringFormat());

                        count2++;
                    }

                    //判斷是否為接續頁
                    if (sline != null) //接續頁
                    {                        
                        e.HasMorePages = true;

                    }
                    else //結束
                    {
                        e.HasMorePages = false;
                        g_iBarTimes = 0;
                        fCheck = false;
                     }
                }                    
                else
                {
                    e.HasMorePages = false;
                    //條碼圖案重新開始reset
                    g_iBarTimes = 0;
                    fCheck = true;

                    g_thStartExport.Join(2000);                        
                    this.Close();

                }                
            }                          
        }       

     }   
}
