using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;

//新增新連結
using System.IO;

//new add  Barcode Lib  2017.02.13
using BarcodeLib;

//new add  Barcode Lib  2017.02.14
using ZXing.Common;
using ZXing;
using System.Drawing.Printing;


//新增THREADING ，2017.03.17
using System.Threading;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace ReadTXT
{
    public partial class ReadTxt : Form
    {
        public Thread th1; //新增th1 執行續物件，2017.03.17
        string[] sCmd;
        string sTemp = string.Empty;


        private string g_sPathFile = string.Empty;
        string sTPathFile = "";
        Bitmap bmp;

        PrintDocument pd = new System.Drawing.Printing.PrintDocument();	//宣告 印表機文件

        private Font printFont;
        private StreamReader streamToPrint;
        PaperSize paperSize = new PaperSize("papersize", 150, 500);//set the paper size
        private int iBarTimes = 0;
        string sPRINT = string.Empty;

        string sPlitr = @"C:\TIPTOP\";
        string sp = "";


        //int  isplitLen = 

        int totalnumber = 0;//this is for total number of items of the list or array
        int itemperpage = 0;//this is for no of item per page 

        
      
        public delegate void SetValue(string value);

        public void majorSetvalue(string value1)
        {
            if(this.InvokeRequired)
            {
                this.Invoke(new SetValue(majorSetvalue), value1);                
            }
            else
            {             
                this.tbx_allReciver.Text = value1;
            }
        }


        public void RunThread()
        {
            /*
            for (int k = 0; k < 10000000; k++)
            {        
                majorSetvalue(k.ToString());            
            } 
             */          
        }


        public ReadTxt(string[] args)
        {
            InitializeComponent();

            sCmd = args;

            //清空欄位
            this.tbx_allReciver.Text="";

            //索引CMD 欄位數量
            for (int ilen = 0; ilen < sCmd.Count(); ilen++)
            {
                //強制將字串轉成大寫，以免執行ERROR ，2017.03.17，Brian        
                sCmd[ilen] = sCmd[ilen].ToUpper();

                //字串累加
                sTemp += sCmd[ilen] + "-";               

            }

            if (sTemp.Length == 0)
            {
              // MessageBox.Show("沒有任何字串傳送 ! 請再次確認 "); 
                sTemp ="沒有任何字串傳送 ! 請再次確認 ";             
            }
                              
            //重新排版
            //sTemp = sTemp.Substring(0, sTemp.Length - 4);
            
            //強制將字串轉成大寫，以免執行ERROR
           // TextInfo tinfo = CultureInfo.CurrentCulture.TextInfo;  //先呼叫tinfo做轉換
          //  tinfo.ToUpper(sTemp);

            sTemp = sTemp.Substring(0, sTemp.Length - 1);

            //顯示接收欄位
             this.tbx_allReciver.Text = sTemp;
              
        }

        private void ReadTxt_Load(object sender, EventArgs e)
        {            
            th1 = new Thread(new ThreadStart(RunThread));
            th1.Start();
                      
        }

        private void btn_open_Click(object sender, EventArgs e)
        {
            string sPattern="*.txt";
            lbx_txtinfo.Items.Clear();
            
            for(int t= 0; t < lbx_txtinfo.Items.Count;t++)
            {
                lbx_txtinfo.Items.RemoveAt(t);            
            }

            //確認開啟               
            if (fbd_ReadTXT.ShowDialog() == DialogResult.OK)
            {
                //Show Select Path  
                tbx_FilePath.Text = fbd_ReadTXT.SelectedPath;

                // RootPath
                 g_sPathFile = tbx_FilePath.Text.ToString();


                 //begin start path search File
                 FileInfo[] filesInfo = null;
                 DirectoryInfo di = new DirectoryInfo(g_sPathFile);
                 
                 try
                 {
                      filesInfo = di.GetFiles(sPattern, SearchOption.TopDirectoryOnly);     
                       

                       // foreach (var fi in di.GetFiles("test?.txt"))
                         foreach (var fi in filesInfo)
                        {
                            lbx_txtinfo.Items.Add(fi.Name);
                           //Console.WriteLine(fi.Name);
                         }

                       // MessageBox.Show("搜尋TXT檔完畢!");
                      
                 }
                 catch (Exception cp)
                 {
                     //抓錯誤訊息
                     MessageBox.Show(cp.Message, " TXTFile 警告!!!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                     throw cp;
                 }

            }
            else // 開啟Fail 
            {
                this.tbx_FilePath.Text = "Open  FolderBrowserDialog NG !";
               // g_PathFailString = tbx_FilePath.Text;
            }
        }

        private void btn_ReadTxt_Click(object sender, EventArgs e)
        {
            tbx_ReadALL.Text = "";
            string slbx =lbx_txtinfo.Text.ToString();

            // Get UTF-8 and UTF-16 encoders.
           // Encoding utf8 = Encoding.UTF8;
           // Encoding utf16 = Encoding.Unicode;

            Encoding bigUnicode = Encoding.UTF8;

            //string sTPathFile = "@"+g_sPathFile+lbx_txtinfo.Text;
            sTPathFile = g_sPathFile  +"\\" +slbx;
            StreamReader sr = new StreamReader(sTPathFile, System.Text.Encoding.Default);
            //StreamReader sr = new StreamReader(sTPathFile, System.Text.Encoding.UTF8);


            //===一次讀取全部內容===
            /*
            string a = sr.ReadToEnd();
            tbx_ReadALL.Text += a.ToString() + Environment.NewLine;
            */

            //產生Code128
            Code128 c128 = new Code128();
            c128.Height = 50;

//          c128.ValueFont = new Font("細明體", 12, FontStyle.Regular);
//          System.Drawing.Image img = c128.GetCodeImage("83500-KBY-3050-HL-NH045PD",Code128.Encode.EAN128);

            c128.ValueFont = new Font("Cambria", 10, FontStyle.Regular);
            
            
            //System.Drawing.Image img = c128.GetCodeImage("83500-KBY-3050-HL-NH045PD", Code128.Encode.Code128A); 
            System.Drawing.Image img = c128.GetCodeImage(sCmd[0], Code128.Encode.Code128A); 

            this.pic_Img.Image = img;

            //===逐行讀取，直到檔尾===
             while (!sr.EndOfStream)
             {             
                //tbx_ReadALL.Text +=sr.ToString()+"\n";
                 tbx_ReadALL.Text +=sr.ReadLine()+Environment.NewLine;                
             }

             sPRINT = tbx_ReadALL.Text;

            // BarcodeType128();


             MessageBox.Show("TXT檔讀取取完畢!!!");

        }


        private void BarcodeType128()
        {            
            BarcodeWriter bw = new BarcodeWriter();


            bw.Format = BarcodeFormat.CODE_128;

            //setting 長和寬
            bw.Options.Width = 150;
            bw.Options.Height = 50;

           //bw.Options.PureBarcode = false;

            bw.Options.PureBarcode = true;

            Bitmap bitmap = bw.Write("TYPE/code128");

            this.pic_Img.Image = (Image)bitmap;
        
        }

        private void btn_Print_Click(object sender, EventArgs e)
        {
            /*
            PageSetupDialog pagesetupDialog = new PageSetupDialog();
            //1.2 設定[版面設定對話方塊]的屬性
            pagesetupDialog.AllowMargins = true;				//取得或設定數值，指示是否啟用對話方塊邊界區段。
            pagesetupDialog.AllowOrientation = true;			//取得或設定數值，指示是否啟用對話方塊的方向區段 (橫向和縱向)。
            pagesetupDialog.AllowPaper = true;					//取得或設定數值，指示是否啟用對話方塊的紙張區段 (紙張大小和紙張來源)。
            pagesetupDialog.AllowPrinter = true;				//取得或設定數值，指示是否啟用 [印表機] 按鈕。

            pagesetupDialog.ShowHelp = true;					//取得或設定數值，指示 [說明] 按鈕是否為可見的。
            pagesetupDialog.ShowNetwork = true;					//取得或設定數值，指示 [網路] 按鈕是否為可見的。


            pd.DocumentName = sTPathFile;
            pagesetupDialog.Document = pd;

           if (pagesetupDialog.ShowDialog() == DialogResult.OK)
            {
                pd.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(Print_Page);
                pd.Print();
            }//	lblShowPageSetup.Text = "";
            
            Graphics g = this.CreateGraphics();

            bmp = new Bitmap( this.Size.Width, this.Size.Height,g);

            Graphics mg = Graphics.FromImage(bmp);

            mg.CopyFromScreen(this.Location.X,this.Location.Y,3,3,this.Size);
             */
            //printPreviewDialog1.ShowDialog();


            try
            {
                //streamToPrint = new StreamReader("C:\\My Documents\\MyFile.txt");
                streamToPrint = new StreamReader(sTPathFile, System.Text.Encoding.Default);

                try
                {
                    //純測試用
                    //printFont = new Font("Arial", 12);

                    //修正與原txt檔格式(細明體,12pt),Brian,2017.03.07
                    printFont = new Font("MingLiU", 12);
                    

                    // PrintDocument pd = new PrintDocument();
                    // pd.PrintPage += new PrintPageEventHandler(this.pd_PrintPage);
                    PrintDocument ptDoc = new PrintDocument();

                    ptDoc.PrintPage += new PrintPageEventHandler(this.Print_Page);


                    if (printPreviewDialog1.ShowDialog() == DialogResult.OK)
                        ptDoc.Print();
                }
                finally
                {
                    streamToPrint.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void Print_Page(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        { 
            //只列印單前頁面
            /*
                e.Graphics.DrawString(tbx_ReadALL.Text,new Font("Times New Roman",9,FontStyle.Bold),Brushes.Black,new Point(100,100));
                e.Graphics.DrawImage(this.pic_Img.Image, new Point(100, 100));
             */

            //列印多頁(方法1)
            /*
            Object pt = null;
            PrintMulit_Page(pt, e);
             */


            //列印多頁(方法2)
            string sFlag = "===================";
            float linesPerPage = 0;
            float yPos = 0;
            int count = 0, count2 = 0;

            float leftMargin = e.MarginBounds.Left;
            float topMargin = e.MarginBounds.Top;
            float topMarginRight = e.MarginBounds.Right;
            float yPosRight = 0;

            string sline = null;
            bool fCheck = false;

                                         
            // Calculate the number of lines per page.
            //計算每頁的行數
            linesPerPage = e.MarginBounds.Height / printFont.GetHeight(e.Graphics);


            //迴圈持續列印條件:當頁數讀超過而且文件檔已讀到尾端，2017.03.01,Brian
            while (count < linesPerPage && ((sline = streamToPrint.ReadLine()) != null))
            {
                iBarTimes++;

                if (iBarTimes == 1)
                {
                    //條碼寫入Print
                    yPosRight = leftMargin+ (20 * printFont.GetHeight(e.Graphics));
                     //e.Graphics.DrawImage(this.pic_Img.Image, new Point(10, 25));
                    e.Graphics.DrawImage(this.pic_Img.Image, yPosRight, 15);
                     
                    //預留欄位寫入符號
                    //e.Graphics.DrawString(sFlag, new Font("Times New Roman", 12, FontStyle.Bold), Brushes.Black, new Point(100, 100));                                    
                }
                                               
                yPos = topMargin + (count * printFont.GetHeight(e.Graphics));
                e.Graphics.DrawString(sline, printFont, Brushes.Black, leftMargin, yPos, new StringFormat());
                
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
                if (iBarTimes == 0 )
                {

                   //再讀一次，前次已讀取並釋出內容
                    if (!fCheck)
                       streamToPrint = new StreamReader(sTPathFile, System.Text.Encoding.Default);

                    //字型定義，純測試用
                   // printFont = new Font("Arial", 10);

                    //修正與原txt檔格式(細明體,12pt),Brian,2017.03.07
                    printFont = new Font("MingLiU", 12);

                    while (count2 < linesPerPage && ((sline = streamToPrint.ReadLine()) != null))
                    {

                        iBarTimes++;

                        if (iBarTimes == 1)
                        {
                            //條碼寫入Print
                            yPosRight = leftMargin + (20 * printFont.GetHeight(e.Graphics));

                            //e.Graphics.DrawImage(this.pic_Img.Image, new Point(10, 25));
                            e.Graphics.DrawImage(this.pic_Img.Image, yPosRight, 10);


                            //預留欄位寫入符號
                            //e.Graphics.DrawString(sFlag, new Font("Times New Roman", 12, FontStyle.Bold), Brushes.Black, new Point(100, 100));                                    


                            //將讀取Flag啟動
                            fCheck = true;

                        }


                        yPos = topMargin + (count2 * printFont.GetHeight(e.Graphics));
                        e.Graphics.DrawString(sline, printFont, Brushes.Black, leftMargin, yPos, new StringFormat());

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
                        iBarTimes = 0;
                        fCheck = false;

                    }

                    /***** 以下為TEST 列印*****/
                    /*
                    e.Graphics.DrawString(tbx_ReadALL.Text, new Font("Times New Roman", 9, FontStyle.Bold), Brushes.Black, new Point(100, 100));
                    e.Graphics.DrawImage(this.pic_Img.Image, new Point(100, 100));                           
                     */

                    /*
                    float currentY = 10;// declare  one variable for height measurement
                    e.Graphics.DrawString("Print in Multiple Pages", DefaultFont, Brushes.Black, 10, currentY);//this will print one heading/title in every page of the document
                    currentY += 15;

                    while (totalnumber <= 50) // check the number of items
                    {
                        e.Graphics.DrawString(totalnumber.ToString(), DefaultFont, Brushes.Black, 50, currentY);//print each item
                        currentY += 20; // set a gap between every item
                        totalnumber += 1; //increment count by 1
                        if (itemperpage < 20) // check whether  the number of item(per page) is more than 20 or not
                        {
                            itemperpage += 1; // increment itemperpage by 1
                            e.HasMorePages = false; // set the HasMorePages property to false , so that no other page will not be added

                        }

                        else // if the number of item(per page) is more than 20 then add one page
                        {
                            itemperpage = 0; //initiate itemperpage to 0 .
                            e.HasMorePages = true; //e.HasMorePages raised the PrintPage event once per page .
                            return;//It will call PrintPage event again

                        }
                    }
                    */
                }
                else
                {
                    e.HasMorePages = false;
                    //條碼圖案重新開始reset
                    iBarTimes = 0;
                    fCheck = true;
                }
                

            }                          
        }

    }
}
