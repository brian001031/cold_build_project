using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;


using Brushes = System.Drawing.Brushes;

using System.Drawing.Drawing2D;

using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Controls;


using Gma.QrCodeNet.Encoding;
using Gma.QrCodeNet.Encoding.Windows.Render;
//using Image2 = System.Windows.Controls.Image;
using Bitmap = System.Drawing.Bitmap;
using Image = System.Drawing.Image;

//using ImageSource = System.Windows.Media.ImageSource;


//使用 PresentationCore.dll 影像物件參考引用，2016.11.04，Brian
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls.Primitives;
using BitmapImage = System.Windows.Media.Imaging.BitmapImage;


using System.Windows.Input;
using System.IO;

using com.google.zxing;
using com.google.zxing.common;
using com.google.zxing.qrcode;
using com.google.zxing.qrcode.decoder;

//using ByteMartiax = com.google.zxing.BarcodeFormat;
//using Bitmap = com.google.zxing.BinaryBitmap;
using System.Windows;
using System.Xaml;
using System.Xaml.Permissions;

using System.Windows.Automation;

using bitmap = System.Drawing.Bitmap;

//using ThoughtWorks.QRCode.Codec;
using ThoughtWorks.QRCode.Codec;
using ThoughtWorks.QRCode.Codec.Data;
using ThoughtWorks.QRCode.Codec.Util;


//use .Net Protocal
using System.Net;




namespace _2ArrayBarcode
{
    public partial class RWBarcode : Form
    {
        HttpWebResponse res;
        int imgWidth;
        int imgHeight;


        public RWBarcode()
        {
            InitializeComponent();
        }

       


        private void RWBarcode_Load(object sender, EventArgs e)
        {
            //TxtSource.Text = @"http:\";

        }

        private void QRCode_Click(object sender, EventArgs e)
        {



            if (TxtSource.Text =="")
            {
              //  MessageBox.Show("請在QR CODE 寫入字串，YKS! @.@");
                System.Windows.Forms.MessageBox.Show("請在QR CODE 寫入字串，YKS! @.@");
                return;
            }

            //輸入要製作二維條碼的字串
            //string codeString = "http://einboch.pixnet.net/blog";
            //TxtSource.Text = "http://www.google.com.tw";
           
            string codeString = TxtSource.Text.ToString();

            string fileSavePath = AppDomain.CurrentDomain.BaseDirectory + "QR2code.jpg";  

            //實例化，設定錯誤修正容量
            /*
              Level L (Low)      7%  of codewords can be restored. 
              Level M (Medium)   15% of codewords can be restored. 
              Level Q (Quartile) 25% of codewords can be restored. 
              Level H (High)     30% of codewords can be restored. 
            */
            QrEncoder encoder = new QrEncoder(Gma.QrCodeNet.Encoding.ErrorCorrectionLevel.H);

            //編碼
            QrCode code = encoder.Encode(codeString);

            //定義線條寬度
            int moduleSizeInPixels = 5;

            //繪二維條碼圖初始化
            GraphicsRenderer renderer = new GraphicsRenderer(new FixedModuleSize(moduleSizeInPixels, QuietZoneModules.Two), Brushes.Black, Brushes.White);

            //留白區大小
            System.Drawing.Point padding = new System.Drawing.Point(10, 16);

            //取得條碼圖大小
            DrawingSize dSize = renderer.SizeCalculator.GetSize(code.Matrix.Width);
             imgWidth = dSize.CodeWidth + 2 * padding.X;
             imgHeight = dSize.CodeWidth + 2 * padding.Y;
            //設定影像大小

           // Bitmap b = new Bitmap(imgWidth, imgHeight);

            Image img = new Bitmap(imgWidth, imgHeight);

            //Bi = new Bitmap(imgWidth, imgHeight);

            
            /*
            MemoryStream ms = new MemoryStream();

            Bi.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            BitmapImage bImage = new BitmapImage();
            bImage.BeginInit();
            bImage.StreamSource = new MemoryStream(ms.ToArray());
            bImage.EndInit();
            ms.Dispose();
            Bi.Dispose();using System.Windows.Automation;

           */
          //  img1.Source = bImage;

         

         //   i.Source = img1.Source = bImage;

           pic_Img1.Width = imgWidth;
           pic_Img1.Height = imgHeight;
            //繪製二維條碼圖
           Graphics g = Graphics.FromImage(img);
            renderer.Draw(g, code.Matrix, padding);

            pic_Img1.Image = img;


            /*
            //以下為取得亂數ID 
            //因為使用了無參數的建構子，預設是使用時間作為亂數種子，故短時間內很容易得到重複的亂數。 
            //建議可以這樣用，即可解決容易重複的問題:
            Random rnd = new Random(Guid.NewGuid().GetHashCode());

            TxtSource.Text = rnd.ToString();
            */
            img.Save(fileSavePath, System.Drawing.Imaging.ImageFormat.Jpeg);
            
      
        }

        private byte[] GetRGBValues(Bitmap bmp)
        {
            // Lock the bitmap's bits. 
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height);
            System.Drawing.Imaging.BitmapData bmpData = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);

            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;

            // Declare an array to hold the bytes of the bitmap.
            int bytes = bmpData.Stride * bmp.Height;
            byte[] rgbValues = new byte[bytes];
            // Copy the RGB values into the array.
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);
            bmp.UnlockBits(bmpData);

            return rgbValues;
        }

        private static byte[] ToByteArray(Image img)
        {
            byte[] byteArray = new byte[0];
            using (MemoryStream stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
                stream.Close();

                byteArray = stream.ToArray();
            }
            return byteArray;
        }


       


        private void btn_Read_Click(object sender, EventArgs e)
        {
            string sUrl = string.Empty;

            try
            {
                QRCodeDecoder decoder = new QRCodeDecoder();

                QRCodeBitmapImage qrNew;

                String decodedString = decoder.decode(qrNew = new QRCodeBitmapImage(new bitmap(pic_Img1.Image)));

                tbx_ReadStr.Text = decodedString;

                sUrl = tbx_ReadStr.Text.ToString();

                if (!sUrl.Contains("http://") && !sUrl.Contains("https://"))
                {
                    sUrl = "http://" + sUrl;
                }

                //判斷網址是否正確可參訪，2016.11.10，Brian                
                //Uri urlCheck = new Uri("http://tw.yahoo.com");
                Uri urlCheck = new Uri(sUrl);


                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(urlCheck);
                myRequest.Method = "HEAD";
                myRequest.Timeout = 10000;  //超過時間10秒
                res = (HttpWebResponse)myRequest.GetResponse();
                //return (res.StatusCode == HttpStatusCode.OK);

                //可連結網址                
                if(res.StatusCode == HttpStatusCode.OK)
                {
                    webSign.Navigate(tbx_ReadStr.Text);

                }              
                else //其餘Error狀況                 
                {
                    System.Windows.Forms.MessageBox.Show("~非正確Http:\\URL 網址，NG連結~ \n");          

                }



            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("pic_Img1 沒有QR圖片做解析 \n" + ex.Message);
            }
        
            /*

           // System.Windows.Controls.Image myImage3 = new System.Windows.Controls.Image();
          
      
            string sOriginal = AppDomain.CurrentDomain.BaseDirectory + "QR2code.jpg";


            BitmapImage bi3 = new BitmapImage();

            bi3.BeginInit();

            //bi3.UriSource = new Uri("smiley_stackpanel.PNG", UriKind.Relative);
            //bi3.UriSource = new Uri(sOriginal, UriKind.Absolute);
            bi3.UriSource = new Uri(sOriginal, UriKind.Relative);

            bi3.EndInit();

            WriteableBitmap wbmap = new WriteableBitmap(bi3);            
            //WriteableBitmap wbmap = new WriteableBitmap(250, 250, 300, 300, PixelFormats.Bgra32, null);
            //Bitmap bitMP = Bitmap.FromFile(sOriginal) as Bitmap;

            Bitmap bitMP = new bitmap(sOriginal);

            int width = bi3.PixelWidth; 
             int height = bi3.PixelHeight;

            

            MemoryStream memoryStream = new MemoryStream();
            bitMP.Save(memoryStream, ImageFormat.Bmp);




            byte[] byteArray = memoryStream.GetBuffer();


           // RGBLuminanceSource lsource = new RGBLuminanceSource(byteArray, width, height);

          //  LuminanceSource source = new RGBLuminanceSource(byteArray, bitMP.Width, bitMP.Height);

          //  LuminanceSource source = new RGBLuminanceSource(ToByteArray(), bitMP.Width, bitMP.Height);

         //   LuminanceSource source = new RGBLuminanceSource(byteArray, bitMP.Width, bitMP.Height);
            /*
          //  ImageSource  
            QRCodeReader qrRead = new QRCodeReader();
            
            //RGBLuminanceSource luminiance = new RGBLuminanceSource(wbmap, wbmap.PixelWidth, wbmap.PixelHeight);

//            RGBLuminanceSource luminiance = new RGBLuminanceSource(GetRGBValues(bitMP), wbmap.PixelWidth, wbmap.PixelHeight);
          

            RGBLuminanceSource luminiance = new RGBLuminanceSource(wbmap, wbmap.PixelWidth, wbmap.PixelHeight);
            

            HybridBinarizer binarizer = new HybridBinarizer(luminiance); 
            BinaryBitmap binBitmap = new BinaryBitmap(binarizer);

            Result results; 
            try 
            {

                results = qrRead.decode(binBitmap); 
                tbx_ReadStr.Text = results.Text; 
                
            }
            catch (Exception ex)
            {
                tbx_ReadStr.Text = "Error:" + ex.GetType() + ":" + ex.Message;
            }  

            */
            
        }

        //當有瀏覽網址一直在做變化，Update URL，2016.11.10，Brian
        private void webSign_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            tbx_ReadStr.Text = webSign.Url.ToString();

            webSign.ScriptErrorsSuppressed = true;

        }

        //將http:// 網址內容 轉換XML
        private void btn_GetXML_Click(object sender, EventArgs e)
        {
            string url = webSign.Url.ToString();

             try 
            {
                string getdata = string.Empty;


                if (tbx_ReadStr.Text != string.Empty)
                {
                    HttpWebRequest MyHttpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                    HttpWebResponse MyHttpWebResponse = (HttpWebResponse)MyHttpWebRequest.GetResponse();


                    //Stream MyStream = res.GetResponseStream();
                    Stream MyStream = MyHttpWebResponse.GetResponseStream();

                    StreamReader MyStreamReader = new StreamReader(MyStream, Encoding.GetEncoding("UTF-8"));
                    getdata = MyStreamReader.ReadToEnd();

                    MyStreamReader.Close();
                    MyStream.Close();

                    //System.Windows.Forms.MessageBox.Show(getdata.ToString());
                    tbx_Xml.Text = getdata;
                }
              
                 
                
            }
            catch (Exception ex)
            {
                //tbx_ReadStr.Text = "Error:" + ex.GetType() + ":" + ex.Message;
                System.Windows.MessageBox.Show("沒有網址可轉換XML" + ex.Message);
            }  
        }

        private void btn_SignOut_Click(object sender, EventArgs e)
        {
                if (null == webSign.Document)
               {
                  return;
               }

               webSign.Document.Cookie = "";

               HtmlElementCollection links = webSign.Document.Links;
              if (null == links)
              {
                  return;
              }
  
              foreach (HtmlElement link in links)
              {
                  if (null == link.InnerText)
                  {
                      continue;
                  }

                  if (link.InnerText.Equals("退出"))
                  {
                      link.InvokeMember("click"); //激發鏈接的點擊事件
                      break;
                  }
              }
        }
    }
}
