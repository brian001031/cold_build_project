using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Runtime.InteropServices;  //引用dll,2017-03-24 by Dragon
using System.Globalization;            //多國語言介面會引用到的,2017-03-24 by Dragon
using System.Resources;                //多國語言介面會引用到的,2017-03-24 by Dragon
using System.Reflection;               //多國語言介面會引用到的,2017-03-24 by Dragon
using System.Threading;                //多國語言介面會引用到的,2017-03-24 by Dragon


//new add  File IO && ODBC Connect Reference 2017.04.18
using System.IO;
using System.Data.Odbc;

//new add SQLCLIENT LINK PROTROCAL 2017.04.26
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Diagnostics;



namespace MMS_BrowseDatabase
{
    public partial class Form_MMSBrowse : Form
    {      
        bool bCanlender = false;
        bool bPuchware = false, bPick = false, breturn = false;   
        int g_Year, g_Month, g_Day;
    
        //物料資料庫各連結參數，2017.04.20，Brian
        //string Server = "10.1.160.27";  // "192.168.1.3"  
        string Server = "10.1.160.95";  // "192.168.1.3"  
        string Database = "MMS";  // "jt8"              
        string dbuid = "Brian-SQL";  // "brian"
        string dbpwd = "Client123";  // "brian123"

        //gap flag array 
//      char[] separators = { ' ', '\n', '\r', '\t', ',', '!' };
        char[] separators = { ',' };

        
        //各資料表名稱
        private string g_sPuchWareHsuing = "PurchasingWarehousingSummary";
        private string g_sWorkTable = "WorkOrderPicking";
        private string g_sReturnJob = "ReturnWarehouse";

        List<string> g_ListRowID = new List<string>();
        List<string> g_lsColumName;


        string sSqlColumn = "FileID,ChangeCategory,ChangeTransNumer,ChangeTransLine,ChangeDepartment,DebitDate,MaterialNameNumber,SourceSingleNumber,SourceLineItem,PurchaseOrderNumber,DataCreatorMember,DataCreatorDepartment,ScanLabelCount,BindingNumber,SearchTime,BindingLotNumber,AutoOrMannulMode,ActualStrogeNumber,FloatStorageNumber,ProductDateRecord,Invalid";

        string sSqlDateSerial = "FileID,BindingNumber";
        string sSqlDatePick = "Pick_ID,Picking_DateTime";
        string sSqlDateReturn = "ReturnID,Return_DateTime";
        
        private string  g_sClassSelect = string.Empty;
        
        //物料管理資料庫Table名稱
        string[] g_sDBTableDirect = { "PurchasingWarehousingSummary", "WorkOrderPicking", "ReturnWarehouse"};

        //類別選項
        string[] g_sClass = { "-------","隸屬部門", "操作時間", "流水號搜尋", "異動單號搜尋", "來源需求單號" };

        //公司選項
        string[] g_sCompany = { "---", "實盈台北", "實盈常熟" };

        //部門類別
        string[] g_sDeptTaipei = { "---","採購", "資材", "品保"};
        string[] g_sDeptChina = { "---", "製造一部", "製造二部 ", "製造三部", "資材部", "品保部", "財務部" };

                
        //M
        string[] g_sRecord_M = { "---", "", "", "" };
        
        //D
        string[] g_sRecord_D = { "---", "", "", "" };
        

        //語系
        string[] g_sSetLanguage = { "en-US", "zh-TW", "zh-CN" };
       
        //序列
        string[] g_sSerial = { "-----", "Serial-"};


        //項次
        string[] g_sItemNum = { "-----", "全部"};
        

        //字串語系宣告
        string g_sLanguage;

        string g_sDBTableName = string.Empty;

        string g_sID = string.Empty;
        string g_sDepart = string.Empty;


        //動態字型大小宣告
        private FontStyle g_fsView;
        private FontFamily g_fmInfo;
        private Font g_ftType;


        //引用Dll宣告
        DBProcess.Class.Structure.UserInformation g_structUserInformation = new DBProcess.Class.Structure.UserInformation();


        #region 多國語言介面宣告,2017-03-24 by Dragon
        //要給對的資源檔路徑，才能正確讀取 //以 "Localize.RES.localize" 為例
        //MicroPhoneTestTools 是本project的namespace //Resourcees 是資源檔的目錄 //localize是資源檔的名稱, 後面的語系及副檔名不需要填入
        // static ResourceManager SysLanguageRM = new ResourceManager("MMS.Resources.localize", Assembly.GetExecutingAssembly());


        static ResourceManager SysLanguageRM = new ResourceManager("MMS_BrowseDatabase.Resources.localize", Assembly.GetExecutingAssembly());
        CultureInfo SysLanguageCI = new CultureInfo(CultureInfo.CurrentCulture.Name);                 //得到系統目前的語言設定
        #endregion

        public Form_MMSBrowse()
        {
            InitializeComponent();

            //Test Here
            /*
            g_structUserInformation.sLoginID = "30-Curry";
            g_structUserInformation.sDepartment = "勇士warrior";                                           
            CatchData(g_sSetLanguage[1], g_structUserInformation);
            */ 
        }
         
        #region 設定多國語言UI介面,2017-03-24 by Dragon
        private void SetAPLanguage()
        {
            try
            {
                //設定系統語系
                Thread.CurrentThread.CurrentCulture = SysLanguageCI;
                Thread.CurrentThread.CurrentUICulture = SysLanguageCI;
                g_sLanguage = SysLanguageCI.Name;                      //得到語系,2017-3-28 by Dragon

                //Title

                //MenuStrip

                //Lable
                this.lbl_UserRecord.Text = SysLanguageRM.GetString("lbl_UserRecord");
                this.lbl_SignInBody.Text = SysLanguageRM.GetString("lbl_SignInBody");
                this.lbl_OPdatetime.Text = SysLanguageRM.GetString("lbl_OPdatetime");

                //Group
                
                //RadioButton
                this.rbn_MaterialPuchWahsing.Text = SysLanguageRM.GetString("rbn_MaterialPuchWahsing");
                this.rbn_WorkPicking.Text = SysLanguageRM.GetString("rbn_WorkPicking");
                this.rbn_ReturnMaterial.Text = SysLanguageRM.GetString("rbn_ReturnMaterial");


                //Button
                //this.btn_ClearItem.Text = SysLanguageRM.GetString("btn_ClearItem");
                this.btn_ConfirmSearch.Text = SysLanguageRM.GetString("btn_ConfirmSearch");


                //MessageBox  

                //CheckBox
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error: ");
            }
        }
        #endregion


        public void CatchData(string sLanguage, DBProcess.Class.Structure.UserInformation allstruct)
        {
            //tbx_ReciveNum.Text = TextData;
            g_sID = lbl_FixiedRun.Text = allstruct.sLoginID.ToString();

            g_sDepart = allstruct.sDepartment.ToString();
          
            g_sLanguage = sLanguage;                       //得到主視窗傳來語系,2017-05-10.Add by Dragon.
            SysLanguageCI = new CultureInfo(g_sLanguage);  //得到主視窗傳來語系,2017-05-10.Add by Dragon.
            SetAPLanguage();

        }


        private void Form1_Load(object sender, EventArgs e)
        {
            tm_SysTime.Enabled = true;
            tm_SysTime.Interval = 100;
             
            for (int i  = 0; i < g_sClass.Count(); i++)
                cbx_SelectItem.Items.Add(g_sClass[i]);

            cbx_statusItem1.Visible = cbx_statusItem2.Visible = cbx_statusItem3.Visible = lbl_cbxItem1.Visible = lbl_cbxItem2.Visible = lbl_cbxItem3.Visible = false;
            tbx_InputSearch.Visible = bCanlender = false;

            cbx_SelectItem.Text = g_sClass[0];
            dgv_MMSData.AllowUserToAddRows = false;

            //cbx_statusItem1.Text = cbx_statusItem2.Text = cbx_statusItem3.Text = cbx_SelectItem.Text = g_sClass[0];
            //TextFormatFlags txFormflag = TextFormatFlags.Default;
        }

        //選擇"物料入庫"
        private void rbn_MaterialPuchWahsing_CheckedChanged(object sender, EventArgs e)
        {
            if (rbn_MaterialPuchWahsing.Checked)
            {
                g_sDBTableName = g_sDBTableDirect[0];

                //顯示動態 (物料入庫)
                g_fsView = rbn_MaterialPuchWahsing.Font.Style;
                g_fmInfo = new FontFamily(rbn_MaterialPuchWahsing.Font.Name);
                g_ftType = new Font(g_fmInfo, 16, g_fsView);
                rbn_MaterialPuchWahsing.Font = g_ftType;
                rbn_MaterialPuchWahsing.ForeColor = Color.Blue;

                //顯示動態 (工單領料)
                g_fsView = rbn_WorkPicking.Font.Style;
                g_fmInfo = new FontFamily(rbn_WorkPicking.Font.Name);
                g_ftType = new Font(g_fmInfo, 10, g_fsView);
                rbn_WorkPicking.Font = g_ftType;
                rbn_WorkPicking.ForeColor = Color.Black;

                //顯示動態 (退料作業)
                g_fsView = rbn_ReturnMaterial.Font.Style;
                g_fmInfo = new FontFamily(rbn_ReturnMaterial.Font.Name);
                g_ftType = new Font(g_fmInfo, 10, g_fsView);
                rbn_ReturnMaterial.Font = g_ftType;
                rbn_ReturnMaterial.ForeColor = Color.Black;

                //啟動符號更新
                bPuchware = true;
                bPick = breturn = false;           
            }        
        }


        //選擇"工單領料"
        private void rbn_WorkPicking_CheckedChanged(object sender, EventArgs e)
        {

            if (rbn_WorkPicking.Checked)
            {
                g_sDBTableName = g_sDBTableDirect[1];


                //顯示動態 (物料入庫)
                g_fsView = rbn_MaterialPuchWahsing.Font.Style;
                g_fmInfo = new FontFamily(rbn_MaterialPuchWahsing.Font.Name);
                g_ftType = new Font(g_fmInfo, 10, g_fsView);
                rbn_MaterialPuchWahsing.Font = g_ftType;
                rbn_MaterialPuchWahsing.ForeColor = Color.Black;


                //顯示動態 (工單領料)
                g_fsView = rbn_WorkPicking.Font.Style;
                g_fmInfo = new FontFamily(rbn_WorkPicking.Font.Name);
                g_ftType = new Font(g_fmInfo, 16, g_fsView);
                rbn_WorkPicking.Font = g_ftType;
                rbn_WorkPicking.ForeColor = Color.Red;

                //顯示動態 (退料作業)
                g_fsView = rbn_ReturnMaterial.Font.Style;
                g_fmInfo = new FontFamily(rbn_ReturnMaterial.Font.Name);
                g_ftType = new Font(g_fmInfo, 10, g_fsView);
                rbn_ReturnMaterial.Font = g_ftType;
                rbn_ReturnMaterial.ForeColor = Color.Black;

                //啟動符號更新
                bPick  = true;
                bPuchware = breturn = false;    
            }        
        }


        //選擇"退料作業"
        private void rbn_ReturnMaterial_CheckedChanged(object sender, EventArgs e)
        {
            if (rbn_ReturnMaterial.Checked)
            {
                g_sDBTableName = g_sDBTableDirect[2];



                //顯示動態 (物料入庫)
                g_fsView = rbn_MaterialPuchWahsing.Font.Style;
                g_fmInfo = new FontFamily(rbn_MaterialPuchWahsing.Font.Name);
                g_ftType = new Font(g_fmInfo, 10, g_fsView);
                rbn_MaterialPuchWahsing.Font = g_ftType;
                rbn_MaterialPuchWahsing.ForeColor = Color.Black;


                //顯示動態 (工單領料)
                g_fsView = rbn_WorkPicking.Font.Style;
                g_fmInfo = new FontFamily(rbn_WorkPicking.Font.Name);
                g_ftType = new Font(g_fmInfo, 10, g_fsView);
                rbn_WorkPicking.Font = g_ftType;
                rbn_WorkPicking.ForeColor = Color.Black;

                //顯示動態 (退料作業)
                g_fsView = rbn_ReturnMaterial.Font.Style;
                g_fmInfo = new FontFamily(rbn_ReturnMaterial.Font.Name);
                g_ftType = new Font(g_fmInfo, 16, g_fsView);
                rbn_ReturnMaterial.Font = g_ftType;
                rbn_ReturnMaterial.ForeColor = Color.Green;

                //啟動符號更新
                breturn  = true;
                bPuchware = bPick = false;              
            }        
        }

        //當選擇功能類別，如:檔案操作時間、流水號搜尋...等
        private void cbx_SelectItem_SelectedIndexChanged(object sender, EventArgs e)
        {
            string sSelectOption = cbx_SelectItem.Text;

            //InitAllComBoBoxItem();

            //Delet ALL Combobox Item
            IterationRemoveCbxItem(cbx_statusItem1);
            IterationRemoveCbxItem(cbx_statusItem2);
            IterationRemoveCbxItem(cbx_statusItem3);

            tbx_InputSearch.Text = string.Empty;


            //"隸屬部門", "操作時間", "流水號搜尋", "異動單號搜尋", "來源需求單號"
            //針對選擇的項目作物件呈現及功能狀態配置
            if( sSelectOption == "隸屬部門")
            {
                cbx_statusItem1.Visible = cbx_statusItem2.Visible = cbx_statusItem1.Visible = lbl_cbxItem1.Visible= lbl_cbxItem2.Visible = true;
                cbx_statusItem3.Visible = tbx_InputSearch.Visible  = lbl_cbxItem3.Visible = bCanlender = false;

                for (int i = 0; i < g_sCompany.Count(); i++)
                    cbx_statusItem1.Items.Add(g_sCompany[i]);

                cbx_statusItem1.Text = cbx_statusItem2.Text = g_sCompany[0];

                lbl_cbxItem1.Text = "公司";
                lbl_cbxItem2.Text = "部門";

                            
            }
            else if( sSelectOption == "操作時間")
            {                             
                bCanlender = true;

                cbx_statusItem2.Visible = cbx_statusItem1.Visible =  cbx_statusItem3.Visible = true;
                tbx_InputSearch.Visible  = false;
                lbl_cbxItem1.Visible = lbl_cbxItem2.Visible = lbl_cbxItem3.Visible = true;

              
                //Item new add Year 
                for (int i_Start_Y = 2011; i_Start_Y <= g_Year; i_Start_Y++)
                {
                    if (i_Start_Y == 2011)
                    {
                        cbx_statusItem1.Items.Add("---");                                                  
                    }
                    
                    cbx_statusItem1.Items.Add(i_Start_Y.ToString());
                }

                cbx_statusItem1 .Text= cbx_statusItem2.Text = cbx_statusItem3.Text = g_sCompany[0];  


                lbl_cbxItem1.Text = "年";
                lbl_cbxItem2.Text = "月";
                lbl_cbxItem3.Text = "日";
            
            }
            else if( sSelectOption == "流水號搜尋")
            {

              // g_sSerial
                Font fnt = new Font(tbx_InputSearch.Font.FontFamily, 16.0F);//Edit your size asper your requirement. it's float value
                tbx_InputSearch.Font = fnt;

                tbx_InputSearch.SetWatermark("請輸入號碼,多組加逗");
                this.tbx_InputSearch.BackColor = Color.Black;
                this.tbx_InputSearch.ForeColor = Color.WhiteSmoke;

               tbx_InputSearch.Visible = cbx_statusItem1.Visible = true;
               cbx_statusItem2.Visible = cbx_statusItem3.Visible = lbl_cbxItem2.Visible = lbl_cbxItem3.Visible = bCanlender = false;

               for (int i = 0; i < g_sSerial.Count(); i++)
                   cbx_statusItem1.Items.Add(g_sSerial[i]);

               cbx_statusItem1.Text = g_sSerial[0];

               lbl_cbxItem1.Text = "序列";
               //lbl_cbxItem2.Text = "數字";
                

            }
            else if( sSelectOption == "異動單號搜尋")
            {
                int iMaxItem = 20;

                cbx_statusItem1.Visible = true;
                cbx_statusItem2.Visible = cbx_statusItem3.Visible = bCanlender = false;
                lbl_cbxItem1.Visible =tbx_InputSearch.Visible = true;
                lbl_cbxItem2.Visible = lbl_cbxItem3.Visible = false;
                
                lbl_cbxItem1.Text = "項次";

                Font fnt = new Font(tbx_InputSearch.Font.FontFamily, 16.0F);//Edit your size asper your requirement. it's float value
                tbx_InputSearch.Font = fnt;


                tbx_InputSearch.SetWatermark("請輸入異動單號...");
                this.tbx_InputSearch.BackColor = Color.Black;
                this.tbx_InputSearch.ForeColor = Color.WhiteSmoke;

                for (int k = 1; k <= iMaxItem; k++)
                {
                    if (k == 1)
                    {
                        for (int item = 0; item < g_sItemNum.Count(); item++)
                            cbx_statusItem1.Items.Add(g_sItemNum[item]);
                    }
                    
                      cbx_statusItem1.Items.Add(k.ToString());
                }

                cbx_statusItem1.Text = g_sItemNum[0];

            }
            else if (sSelectOption == "來源需求單號")
            {

                Font fnt = new Font(tbx_InputSearch.Font.FontFamily, 16.0F);//Edit your size asper your requirement. it's float value
                tbx_InputSearch.Font = fnt;


                tbx_InputSearch.SetWatermark("請輸入來源需求單號...");
                this.tbx_InputSearch.BackColor = Color.Black;
                this.tbx_InputSearch.ForeColor = Color.WhiteSmoke;


                cbx_statusItem1.Visible = cbx_statusItem2.Visible = cbx_statusItem3.Visible = bCanlender = false;
                cbx_SelectItem.Visible = tbx_InputSearch.Visible = true;
                lbl_cbxItem1.Visible = lbl_cbxItem2.Visible = lbl_cbxItem3.Visible = false;

            }
            else  //指選取一開始預設"---"
            {
               
                //將全部ComboBox 全初始化設定
                bCanlender = false;
                cbx_statusItem1.Text = cbx_statusItem2.Text = cbx_statusItem3.Text  = g_sClass[0];

                tbx_InputSearch.Visible = lbl_cbxItem1.Visible = lbl_cbxItem2.Visible = false;
                cbx_statusItem1.Visible = cbx_statusItem2.Visible = cbx_statusItem3.Visible = bCanlender = false;
            }

        }

        //使用迭代做兩次刪除        
        private void IterationRemoveCbxItem( ComboBox cbx_Status)
        {
            for (int i = 0; i < cbx_Status.Items.Count; i++)
            {
                cbx_Status.Items.RemoveAt(i);
            }
            for (int j = 0; j < cbx_Status.Items.Count; j++)
            {
                IterationRemoveCbxItem(cbx_Status);
            }

        }

        private void InitAllComBoBoxItem()
        {
            //cbx_statusItem1.Text = cbx_statusItem2.Text = cbx_statusItem3.Text = cbx_SelectItem.Text = g_sClass[0];

            //清除cbx_statusItem1
           // cbx_statusItem1.Items.Clear();
            for (int i = 0; i < cbx_statusItem1.Items.Count; i++)
                cbx_statusItem1.Items.RemoveAt(i);        

            //清除cbx_statusItem2           
           // cbx_statusItem2.Items.Clear();
            for (int j = 0; j < cbx_statusItem2.Items.Count; j++)
                cbx_statusItem2.Items.RemoveAt(j);        

            //清除cbx_statusItem3                       
          //  cbx_statusItem3.Items.Clear();
            for (int k = 0; k < cbx_statusItem3.Items.Count; k++)
                cbx_statusItem3.Items.RemoveAt(k);

            tbx_InputSearch.Text = string.Empty;

            /*
            for (int i = 0; i < cbx_SelectItem.Items.Count; i++)
                cbx_SelectItem.Items.RemoveAt(i);        
            */
        
        }


        //系統時間tick行為模式
        private void tm_SysTime_Tick(object sender, EventArgs e)
        {
            DateTime dtn = DateTime.Now;

            g_Year  = dtn.Year;
            g_Month = dtn.Month;
            g_Day = dtn.Day;

            int h = dtn.Hour;
            int m = dtn.Minute;
            int s = dtn.Second;

            lbl_datetimeRun.Text = Convert.ToString(h) + ":" + Convert.ToString(m) + ":" + Convert.ToString(s);    
        }


        private void Search_Current_D_Combine(int M)
        {
            int i_Febrauary = 0;
            string sSelectYear = cbx_statusItem1.Text;

            switch(M)
            {
                case 1:
                case 3:
                case 5:
                case 7:
                case 8:
                case 10:
                case 12:
                    //Large Month
                    for (int day = 1; day <= 31; day++)                        
                        cbx_statusItem3.Items.Add(day.ToString());
                    
                    break;

                case 4:
                case 6:
                case 9:
                case 11:

                    //Small Month
                    for (int day = 1; day <= 30; day++)                        
                        cbx_statusItem3.Items.Add(day.ToString());
                                       
                    break;

                case 2:

                    int iselectY = int.Parse(sSelectYear);

                    i_Febrauary = ((iselectY - 2012) % 4 == 0) ? 29 : 28;

                    for (int day = 1; day <= i_Febrauary; day++)                       
                        cbx_statusItem3.Items.Add(day.ToString());
                                                                                 
                    break;

                default:
                    break;
            }

        }

        private void cbx_statusItem1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string sSelectIndex = cbx_statusItem1.Text;
             
            if( sSelectIndex == "實盈台北")
            {
                cbx_statusItem2.Items.Clear();

                for (int j = 0; j < cbx_statusItem2.Items.Count; j++)
                    cbx_statusItem2.Items.RemoveAt(j);   

                for (int i = 0; i < g_sDeptTaipei.Count(); i++)
                    cbx_statusItem2.Items.Add(g_sDeptTaipei[i]);

                cbx_statusItem2.Text = g_sDeptTaipei[0];

            }
            else if (sSelectIndex == "實盈常熟")
            {
                cbx_statusItem2.Items.Clear();

                for (int j = 0; j < cbx_statusItem2.Items.Count; j++)
                    cbx_statusItem2.Items.RemoveAt(j);   

                for (int i = 0; i < g_sDeptChina.Count(); i++)
                    cbx_statusItem2.Items.Add(g_sDeptChina[i]);

                cbx_statusItem2.Text = g_sDeptChina[0];
            }
            else
            {
                int n;

                IterationRemoveCbxItem(cbx_statusItem2);
                IterationRemoveCbxItem(cbx_statusItem3);


                cbx_statusItem2.Items.Add("---");
                cbx_statusItem3.Items.Add("---");


                if (int.TryParse(sSelectIndex, out n))
                {
                    //mean search Y M D
                    if (sSelectIndex.Length == 4 && n >= 2011 )
                    {
                        //display M Iitemstatus2                        
                        if (n == g_Year)
                        {
                            for (int M = 1; M <= g_Month; M++)
                                cbx_statusItem2.Items.Add(M.ToString());                                                
                        }
                        else
                        {
                            for (int M = 1; M <= 12; M++)
                                cbx_statusItem2.Items.Add(M.ToString());                        
                        
                        }

                    }
                    //search Serial or other else type
                    else {
                        cbx_statusItem2.Text = cbx_statusItem3.Text = "---";
                    }

                    cbx_statusItem2.Text = cbx_statusItem3.Text = "---";
                    
                }
                else
                {
                    IterationRemoveCbxItem(cbx_statusItem2);

                    cbx_statusItem2.Items.Add("---");
                    cbx_statusItem2.Text = "---";
                }               
            }


        }

        private void tbx_InputSearch_TextChanged(object sender, EventArgs e)
        {            
                     

            



        }

        private void tbx_InputSearch_MouseDown(object sender, MouseEventArgs e)
        {          
            tbx_InputSearch.ClearWatermark();
        }

        private void cbx_statusItem2_SelectedIndexChanged(object sender, EventArgs e)
        {
            string sM_Select = cbx_statusItem2.Text;

            int iSelect_M = 0;


            IterationRemoveCbxItem(cbx_statusItem3);

            cbx_statusItem3.Items.Add("---");

            //if (iSelect_M >= 1 && iSelect_M <= 12 && bCanlender)
            if (int.TryParse(sM_Select, out iSelect_M) && bCanlender)
            {                
                int iCurrentYear = int.Parse(cbx_statusItem1.Text);

                //find Belong to select M & D
                if ( iCurrentYear == g_Year  &&  iSelect_M == g_Month)
                {                                      
                    //Cutrrent Days in this Month
                    for (int day = 1; day <= g_Day; day++)
                        cbx_statusItem3.Items.Add(day.ToString());
                }
                else
                {
                    Search_Current_D_Combine(iSelect_M);
                }

                
                cbx_statusItem3.Text = "---";
            }
            else
            {
                 

                            
            }
            
        }

        //將table column 欄位針對語系做調整
        private void TableColumnLanguageCheck(DataTable dt,List<string> slistName)
        {
             string sTemp = string.Empty;

             if(bPuchware)
             {
                for (int iNum = 0; iNum < slistName.Count(); iNum++)
               {
                 switch (iNum)
                 {
          
                     case 0:
                         //FileID
                         if (g_sLanguage != "en-US")
                             sTemp = (g_sLanguage == "zh-TW") ? "序列號碼" : "序列号码";
                         else
                             sTemp = slistName[iNum].ToString();
                         break;
                     case 1:
                         //ChangeCategory
                         if (g_sLanguage != "en-US")
                             sTemp = (g_sLanguage == "zh-TW") ? "編號" : "编号";
                         else
                             sTemp =  slistName[iNum].ToString();
                         break;

                     case 2:
                         //ChangeTransNumer
                         if (g_sLanguage != "en-US")
                             sTemp = (g_sLanguage == "zh-TW") ? "異動單號" : "异动单号";
                         else
                             sTemp =  slistName[iNum].ToString();
                         break;
                 
                    case 3:
                         //ChangeTransLine
                         if (g_sLanguage != "en-US")
                             sTemp = (g_sLanguage == "zh-TW") ? "異動項次" : "异动项次";
                         else
                             sTemp =  slistName[iNum].ToString();
                         break;
                    case 4:
                         //ChangeDepartment
                         if (g_sLanguage != "en-US")
                             sTemp = (g_sLanguage == "zh-TW") ? "異動部門" : "异动部门";
                         else
                             sTemp =  slistName[iNum].ToString();
                         break;
                
                    case 5:
                         //DebitDate
                         if (g_sLanguage != "en-US")
                             sTemp = (g_sLanguage == "zh-TW") ? "生產日期" : "生产日期";
                         else
                             sTemp =  slistName[iNum].ToString();
                         break;
                   
                    case 6:
                         //MaterialNameNumber
                         if (g_sLanguage != "en-US")
                             sTemp = (g_sLanguage == "zh-TW") ? "物料編號" : "物料编号";
                         else
                             sTemp = slistName[iNum].ToString();
                         break;
                    case 7:
                         //SourceSingleNumber
                         if (g_sLanguage != "en-US")
                             sTemp = (g_sLanguage == "zh-TW") ? "來源單號" : "来源单号";
                         else
                             sTemp = slistName[iNum].ToString();
                         break;

                    case 8:
                         //SourceLineItem
                         if (g_sLanguage != "en-US")
                             sTemp = (g_sLanguage == "zh-TW") ? "來源項次" : "来源项次";
                         else
                             sTemp = slistName[iNum].ToString();
                         break;
                  
                    case 9:
                         //PurchaseOrderNumber
                         if (g_sLanguage != "en-US")
                             sTemp = (g_sLanguage == "zh-TW") ? "採購單號" : "采购单号";
                         else
                             sTemp = slistName[iNum].ToString();
                         break;
                  
                    case 10:
                         //DataCreatorMember
                         if (g_sLanguage != "en-US")
                             sTemp = (g_sLanguage == "zh-TW") ? "登入人員" : "登入人员";
                         else
                             sTemp = slistName[iNum].ToString();
                         break;

                    case 11:
                         //DataCreatorDepartment
                         if (g_sLanguage != "en-US")
                             sTemp = (g_sLanguage == "zh-TW") ? "登入部門" : "登入部门";
                         else
                             sTemp = slistName[iNum].ToString();
                         break;
                 
                    case 12:
                         //ScanLabelCount
                         if (g_sLanguage != "en-US")
                             sTemp = (g_sLanguage == "zh-TW") ? "掃描次數" : "扫描次数";
                         else
                             sTemp = slistName[iNum].ToString();
                         break;

                    case 13:
                         //BindingNumber
                         if (g_sLanguage != "en-US")
                             sTemp = (g_sLanguage == "zh-TW") ? "流水號" : "流水号";
                         else
                             sTemp = slistName[iNum].ToString();
                         break;

                    case 14:
                         //SearchTime
                         if (g_sLanguage != "en-US")
                             sTemp = (g_sLanguage == "zh-TW") ? "作業時間" : "作业时间";
                         else
                             sTemp = slistName[iNum].ToString();
                         break;

                    case 15:
                         //BindingLotNumber
                         if (g_sLanguage != "en-US")
                             sTemp = (g_sLanguage == "zh-TW") ? "綁定號" : "绑定号";
                         else
                             sTemp = slistName[iNum].ToString();
                         break;

                    case 16:
                         //AutoOrMannulMode
                         if (g_sLanguage != "en-US")
                             sTemp = (g_sLanguage == "zh-TW") ? "手自動模式" : "手自动模式";
                         else
                             sTemp = slistName[iNum].ToString();
                         break;                  
                    case 17:
                         //ActualStrogeNumber
                         if (g_sLanguage != "en-US")
                             sTemp = (g_sLanguage == "zh-TW") ? "實際存量" : "实际存量";
                         else
                             sTemp = slistName[iNum].ToString();
                         break;

                    case 18:
                         //FloatStorageNumber
                         if (g_sLanguage != "en-US")
                             sTemp = (g_sLanguage == "zh-TW") ? "浮動存量" : "浮动存量";
                         else
                             sTemp = slistName[iNum].ToString();
                         break;

                    case 19:
                         //ProductDateRecord
                         if (g_sLanguage != "en-US")
                             sTemp = (g_sLanguage == "zh-TW") ? "產品貼封日期" : "产品贴封日期";
                         else
                             sTemp = slistName[iNum].ToString();
                         break;
                    case 20:
                         //Invalid
                         if (g_sLanguage != "en-US")
                             sTemp = (g_sLanguage == "zh-TW") ? "無效" : "无效";
                         else
                             sTemp = slistName[iNum].ToString();
                        break;      
                   	                     
                     default:
                         break;
                 }
                 dt.Columns.Add(sTemp);
               }                          
             }
             else if(bPick)
             {
                 for (int iNum = 0; iNum < slistName.Count(); iNum++)
                {
                   switch (iNum)
                   {
                     case 0:
                           //Pick_ID
                         if (g_sLanguage != "en-US")
                             sTemp = (g_sLanguage == "zh-TW") ? "領取序號" : "领取序号";
                         else
                             sTemp = slistName[iNum].ToString();
                         break;
                     case 1:
                         //Picking_Category_Number
                         if (g_sLanguage != "en-US")
                             sTemp = (g_sLanguage == "zh-TW") ? "領取異動單號" : "领取异动单号";
                         else
                             sTemp =  slistName[iNum].ToString();
                         break;
                     case 2:
                         //Picking_Member
                         if (g_sLanguage != "en-US")
                             sTemp = (g_sLanguage == "zh-TW") ? "領取人員" : "领取人员";
                         else
                             sTemp = slistName[iNum].ToString();
                         break;
                     case 3:
                         //Picking_Department
                         if (g_sLanguage != "en-US")
                             sTemp = (g_sLanguage == "zh-TW") ? "領取部門" : "领取部门";
                         else
                             sTemp = slistName[iNum].ToString();
                         break;

                     case 4:
                         //Picking_MaterialNumber
                         if (g_sLanguage != "en-US")
                             sTemp = (g_sLanguage == "zh-TW") ? "領取物料號碼" : "领取物料号码";
                         else
                             sTemp = slistName[iNum].ToString();
                         break;

                     case 5:
                         //Picking_SourceLineItem
                         if (g_sLanguage != "en-US")
                             sTemp = (g_sLanguage == "zh-TW") ? "領取項次" : "领取项次";
                         else
                             sTemp = slistName[iNum].ToString();
                         break;

                     case 6:
                         //Search_Time
                         if (g_sLanguage != "en-US")
                             sTemp = (g_sLanguage == "zh-TW") ? "執行時間" : "执行时间";
                         else
                             sTemp = slistName[iNum].ToString();
                         break;

                     case 7:
                         //Picking_DateTime
                         if (g_sLanguage != "en-US")
                             sTemp = (g_sLanguage == "zh-TW") ? "領用日期" : "领用日期";
                         else
                             sTemp = slistName[iNum].ToString();
                         break;

                     case 8:
                         //Picking_Serial_Count
                         if (g_sLanguage != "en-US")
                             sTemp = (g_sLanguage == "zh-TW") ? "流水號" : "流水号";
                         else
                             sTemp = slistName[iNum].ToString();
                         break;

                     case 9:
                         //Pickup_Quantity
                         if (g_sLanguage != "en-US")
                             sTemp = (g_sLanguage == "zh-TW") ? "領用數量" : "领用数量";
                         else
                             sTemp = slistName[iNum].ToString();
                         break;

                     case 10:
                         //Pickup_TheWareHouse
                         if (g_sLanguage != "en-US")
                             sTemp = (g_sLanguage == "zh-TW") ? "領用倉別" : "领用仓别";
                         else
                             sTemp = slistName[iNum].ToString();
                         break;

                     case 11:
                         //Pickup_From_PuchSerialNumber
                         if (g_sLanguage != "en-US")
                             sTemp = (g_sLanguage == "zh-TW") ? "領用存取流水號" : "领用存取流水号";
                         else
                             sTemp = slistName[iNum].ToString();
                         break;

                     case 12:
                         //RequireListNumber
                         if (g_sLanguage != "en-US")
                             sTemp = (g_sLanguage == "zh-TW") ? "需求單號" : "需求单号";
                         else
                             sTemp = slistName[iNum].ToString();
                         break;
                                                                
                     default:
                         break;
                  }
                 dt.Columns.Add(sTemp);
               }
             
             }
             else if (breturn)
             {
                for (int iNum = 0; iNum < slistName.Count(); iNum++)
                {
                   switch (iNum)
                   {
                     case 0:
                           //ReturnID
                         if (g_sLanguage != "en-US")
                             sTemp = (g_sLanguage == "zh-TW") ? "退料序號" : "退料序号";
                         else
                             sTemp = slistName[iNum].ToString();
                         break;
                     case 1:
                         //Return_Category_Number
                         if (g_sLanguage != "en-US")
                             sTemp = (g_sLanguage == "zh-TW") ? "編號" : "编号";
                         else
                             sTemp =  slistName[iNum].ToString();
                         break;

                     case 2:
                         //Return_Member
                         if (g_sLanguage != "en-US")
                             sTemp = (g_sLanguage == "zh-TW") ? "退料人員" : "退料人员";
                         else
                             sTemp = slistName[iNum].ToString();
                         break;
                     case 3:
                         //Return_Department
                         if (g_sLanguage != "en-US")
                             sTemp = (g_sLanguage == "zh-TW") ? "退料部門" : "退料部门";
                         else
                             sTemp = slistName[iNum].ToString();
                         break;

                     case 4:
                         //Return_MaterialPartNumber
                         if (g_sLanguage != "en-US")
                             sTemp = (g_sLanguage == "zh-TW") ? "退出物料號" : "退出物料号";
                         else
                             sTemp = slistName[iNum].ToString();
                         break;

                     case 5:
                         //Return_SourceLineItem
                         if (g_sLanguage != "en-US")
                             sTemp = (g_sLanguage == "zh-TW") ? "退料項次" : "退料项次";
                         else
                             sTemp = slistName[iNum].ToString();
                         break;
                     case 6:
                         //Search_Time
                         if (g_sLanguage != "en-US")
                             sTemp = (g_sLanguage == "zh-TW") ? "執行時間" : "执行时间";
                         else
                             sTemp = slistName[iNum].ToString();
                         break;

                     case 7:
                         //Return_DateTime
                         if (g_sLanguage != "en-US")
                             sTemp = (g_sLanguage == "zh-TW") ? "退料日期" : "退料日期";
                         else
                             sTemp = slistName[iNum].ToString();
                         break;

                     case 8:
                         //Return_Quantity
                         if (g_sLanguage != "en-US")
                             sTemp = (g_sLanguage == "zh-TW") ? "退料數量" : "退料数量";
                         else
                             sTemp = slistName[iNum].ToString();
                         break;

                     case 9:
                         //Return_TheWareHouse
                         if (g_sLanguage != "en-US")
                             sTemp = (g_sLanguage == "zh-TW") ? "退放指定倉別" : "退放指定仓别";
                         else
                             sTemp = slistName[iNum].ToString();
                         break;
                     case 10:
                         //Return_SourceListNumber
                         if (g_sLanguage != "en-US")
                             sTemp = (g_sLanguage == "zh-TW") ? "退料來源單號" : "退料来源单号";
                         else
                             sTemp = slistName[iNum].ToString();
                         break;

                     default:
                         break;
                 }

                 dt.Columns.Add(sTemp);

               }
                
             }                         
        }
                                 
        private void Read_MMS_DBTable_ID_PART()
        {
            string strtarpath = string.Format("server={0};database={1};uid={2};pwd={3};Connect Timeout = 180", Server, Database, dbuid, dbpwd);
            //string strtarpath = "Data Source=DRAGONSERVER;Initial Catalog=MMS;Persist Security Info=True;User ID=Brian-SQl";           

            int iID = 0 ;
            
            SqlConnection icn = new SqlConnection();
            icn.ConnectionString = strtarpath;

            //stop current 
            if (icn.State == ConnectionState.Open) icn.Close();
            //open start!
            icn.Open();

            try
            {
                
                //step(2):
                SqlCommand isc = new SqlCommand();
                isc.Connection = icn;


                if (bPuchware)
                {
                    isc.CommandText = "SELECT " + sSqlColumn + " FROM " + g_sPuchWareHsuing + " WHERE DataCreatorMember = "+ "'" + lbl_FixiedRun.Text + "'";
                }
                else if (bPick)
                {
                    isc.CommandText = "SELECT * FROM " + g_sWorkTable + " WHERE Picking_Member = " + "'" + lbl_FixiedRun.Text + "'";
                }
                else if (breturn)
                {
                    isc.CommandText = "SELECT * FROM " + g_sReturnJob + " WHERE Return_Member = " + "'" + lbl_FixiedRun.Text + "'";
                }

                isc.CommandTimeout = 3000;

                //step(3) streamreader Enable!
                //SqlDataReader:從數據庫獲取行
                SqlDataReader sqlDr = isc.ExecuteReader();  // sqlComad.ExecuteReader();
                //FieldCount  取得目前資料列中的資料行數目。
                DataTable dt = new DataTable();

                int iDrCount = sqlDr.FieldCount;

                //find colums 
                for (int iCnt = 0; iCnt < iDrCount; iCnt++)
                {
                    string a = sqlDr.GetName(iCnt);
                    //dt.Columns.Add(a);    
                    g_lsColumName.Add(a);
                                  
                }

                //轉換當前選擇字體，2017.04.24 , Brian                     
                TableColumnLanguageCheck(dt, g_lsColumName);

                //將DataReader前進到下一個資料
                while (sqlDr.Read())
                {
                    //定義一個數組，便於讀出每一行資料
                    String[] subitems = new String[iDrCount];
                    //用循環讀出每一行資料
                    for (int iCnt = 0; iCnt < iDrCount; iCnt++)
                    {
                        //讀出每一行資料保存到數組中
                        subitems[iCnt] = sqlDr[iCnt].ToString();
                        string sValue = sqlDr[iCnt].ToString();

                        iID++;
                    }
                    //將讀出的行資料增表的行中
                    dt.Rows.Add(subitems);
                }

                //DataGridView的來源為“dt”這個表
                //  dgv_MMSTable.DataSource = dt;  
                dgv_MMSData.DataSource = dt;

                if (dgv_MMSData.DataSource != null && iID >= 1)
                {

                    
                }
                else
                {
                    //MessageBox.Show("連接MMS-DB-PurchasingWarehousingSummary 資料表Error! ");
                    MessageBox.Show("登入人員: (" + lbl_FixiedRun.Text + ") 無資料表任何紀錄!");
                
                }
                

                //Step 6：close SyncObject 與 資料配接器物件來釋放物件所佔用的資源 mean Free not use memory session
                //if (sqlDr.Read() != null)

                sqlDr.Close();
                isc.Dispose();
                icn.Close();
                icn.Dispose();
            }
            catch (Exception k)
            {
                MessageBox.Show("Error link DBRead!~~");
                throw k;
            }
        }

        private void Read_MMS_DBTable_Date()
        {
            string strtarpath = string.Format("server={0};database={1};uid={2};pwd={3};Connect Timeout = 180", Server, Database, dbuid, dbpwd);
            //string strtarpath = "Data Source=DRAGONSERVER;Initial Catalog=MMS;Persist Security Info=True;User ID=Brian-SQl";           

            g_ListRowID = new List<string>();

            SqlConnection icn = new SqlConnection();
            icn.ConnectionString = strtarpath;

            //stop current 
            if (icn.State == ConnectionState.Open) icn.Close();
            //open start!
            icn.Open();

            try
            {
                //step(2):
                SqlCommand isc = new SqlCommand();
                isc.Connection = icn;
                
                if( bPuchware )
                {                
                    isc.CommandText = "SELECT " + sSqlDateSerial + " FROM " + g_sPuchWareHsuing;                
                }
                else if( bPick )
                {
                    isc.CommandText = "SELECT " + sSqlDatePick + " FROM " + g_sWorkTable;
                }
                else if( breturn )                
                {
                    isc.CommandText = "SELECT " + sSqlDateReturn + " FROM " + g_sReturnJob;                
                }
                
                isc.CommandTimeout = 3000;

                //step(3) streamreader Enable!
                //SqlDataReader:從數據庫獲取行
                SqlDataReader sqlDr = isc.ExecuteReader();  // sqlComad.ExecuteReader();
                //FieldCount  取得目前資料列中的資料行數目。
               // DataTable dt = new DataTable();

                string sYear = cbx_statusItem1.Text;
                string sMonth = cbx_statusItem2.Text;
                string sDay = cbx_statusItem3.Text;

                int iDrCount = sqlDr.FieldCount;

                //find colums 
              /*  for (int iCnt = 0; iCnt < iDrCount; iCnt++)
                {
                    string a = sqlDr.GetName(iCnt);
                    dt.Columns.Add(a);
                }
                */

                //將DataReader前進到下一個資料
                while (sqlDr.Read())
                {
                    //定義一個數組，便於讀出每一行資料
                    String[] subitems = new String[iDrCount];
                  
                    //用循環讀出每一行資料
                    for (int iCnt = 0; iCnt < iDrCount; iCnt++)
                    {
                        //讀出每一行資料保存到數組中
                        subitems[iCnt] = sqlDr[iCnt].ToString();
                        string sValue = sqlDr[iCnt].ToString();
                       
                        if (iCnt == 1)
                        {
                            string[] sDate = sValue.Split(' ');

                            //取第一組字串(ex:2017/1/12)
                            if (sDate[0] == sYear + "/" + sMonth + "/" + sDay
                                    || sDate[0] == sDay + "/" + sMonth + "/" + sYear)
                            {
                                g_ListRowID.Add(subitems[0]);
                            }
                            
                        }       
                    }
                                               
                    //將讀出的行資料增表的行中
                    //dt.Rows.Add(subitems);
                }

                if (g_ListRowID.Count >=1)
                {
                    string sItemNumber = string.Empty;
                    string s_param2 = string.Empty;

                    for (int i = 0; i < g_ListRowID.Count; i++)
                        sItemNumber += g_ListRowID[i] + ",";

                    sItemNumber = sItemNumber.Substring(0, sItemNumber.Length - 1);

                    string[] s_field = sItemNumber.Split(',');

                    for (int i = 0; i < s_field.Length; i++)
                    {                                                
                        s_field[i] = "'" + s_field[i].Trim() + "'";
                    }

                    s_param2 = string.Join(",", s_field);


                    //先關閉Read,避免重新讀入異常
                    sqlDr.Close();
                                        
                    //重新再下一次COMMAD                    
                    isc = new SqlCommand();
                    isc.Connection = icn;

                    if (bPuchware)
                    {
                        isc.CommandText = "SELECT " + sSqlColumn + " FROM " + g_sPuchWareHsuing + " WHERE FileID" + " IN " + "(" + s_param2 + ")";
                    }
                    else if (bPick)
                    {
                        isc.CommandText = "SELECT * FROM " + g_sWorkTable + " WHERE Pick_ID" + " IN " + "(" + s_param2 + ")";                        
                    }
                    else if (breturn)
                    {
                        isc.CommandText = "SELECT * FROM " + g_sReturnJob + " WHERE ReturnID" + " IN " + "(" + s_param2 + ")";                                                                       
                    }

                    isc.CommandTimeout = 3000;

                    sqlDr = isc.ExecuteReader();  // sqlComad.ExecuteReader();

                    DataTable dt = new DataTable();                    
                    iDrCount = sqlDr.FieldCount;

                    //find colums 
                    for (int iCnt = 0; iCnt < iDrCount; iCnt++)
                    {
                        string a = sqlDr.GetName(iCnt);
                        //dt.Columns.Add(a);
                        g_lsColumName.Add(a);
                    }

                  
                    TableColumnLanguageCheck(dt, g_lsColumName);

                    while (sqlDr.Read())
                    {
                        //定義一個數組，便於讀出每一行資料
                        String[] subitems = new String[iDrCount];

                        //用循環讀出每一行資料
                        for (int iCnt = 0; iCnt < iDrCount; iCnt++)
                        {
                            //讀出每一行資料保存到數組中
                            subitems[iCnt] = sqlDr[iCnt].ToString();
                            string sValue = sqlDr[iCnt].ToString();                           
                        }

                        //將讀出的行資料增表的行中
                        dt.Rows.Add(subitems);
                    }

                    //DataGridView的來源為“dt”這個表
                    dgv_MMSData.DataSource = dt;  


                    if(dgv_MMSData.DataSource !=null)
                    {
                        
                    
                    }

                }
                else{

                    dgv_MMSData.DataSource = null;
                    MessageBox.Show("查詢無此日期: " + sYear + "/" + sMonth + "/" + sDay + " 任何紀錄資料");
                    //return;
                }

                                             
                sqlDr.Close();
                isc.Dispose();
                icn.Close();
                icn.Dispose();
            }
            catch (Exception k)
            {
                MessageBox.Show("Error link DBRead!~~");
                throw k;
            }
        }


        private void Read_MMS_DBTable_SERIAL()
        {
            string strtarpath = string.Format("server={0};database={1};uid={2};pwd={3};Connect Timeout = 180", Server, Database, dbuid, dbpwd);
            //string strtarpath = "Data Source=DRAGONSERVER;Initial Catalog=MMS;Persist Security Info=True;User ID=Brian-SQl";           
            
            int iID = 0;
            string s_param2 = string.Empty;
            string s_FinalSerial = string.Empty;
            s_param2 = tbx_InputSearch.Text;

//            string[] sSplitFlag = s_param2.Split(separators, StringSplitOptions.None);  //' ', '\n', '\r', '\t', ',', ',', '!'

           // string[] sSplitFlag = s_param2.Split(",", StringSplitOptions.RemoveEmptyEntries);  //' ', '\n', '\r', '\t', ',', ',', '!'
            string[] sSplitFlag = s_param2.Split(',');


            if (cbx_statusItem1.Text != "Serial-")
            {
                MessageBox.Show("請選取Serial- 在執行 ");
                return;            
            }


            //代表無任何ID數字 或是 不是','做分隔
            //if (sSplitFlag.Length == 0)
            if (!s_param2.Contains(',')) 
            {
                int n = 0;
                if (int.TryParse(s_param2,out n))
                {
                    s_param2 = "'" + s_param2.Trim() + "'";

                    s_FinalSerial = s_param2;
                }
                else
                {
                    if (s_param2.ToString()=="")
                    {
                        MessageBox.Show("請輸入查詢Serial-ID號碼，謝謝");                   
                    }
                    else
                    {
                        MessageBox.Show("不是用 ','分隔，請改此符號，謝謝");
                    
                    }

                   return;            
                }

            }
            else
            {
                for (int t = 0; t < sSplitFlag.Length; t++)
                {
                    sSplitFlag[t] = "'" + sSplitFlag[t].Trim() + "'";
                }

                s_FinalSerial = string.Join(",", sSplitFlag);            
            }
                        
            SqlConnection icn = new SqlConnection();
            icn.ConnectionString = strtarpath;

            //stop current 
            if (icn.State == ConnectionState.Open) icn.Close();
            //open start!
            icn.Open();

            try
            {                               
                //step(2):
                SqlCommand isc = new SqlCommand();
                isc.Connection = icn;
                
                if (bPuchware)
                {
                    isc.CommandText = "SELECT " + sSqlColumn + " FROM " + g_sPuchWareHsuing + " WHERE FileID" + " IN " + "(" + s_FinalSerial + ")";
                }
                else if (bPick)
                {
                    isc.CommandText = "SELECT * FROM " + g_sWorkTable + " WHERE Pick_ID" + " IN " + "(" + s_FinalSerial + ")";
                }
                else if (breturn)
                {
                    isc.CommandText = "SELECT * FROM " + g_sReturnJob + " WHERE ReturnID" + " IN " + "(" + s_FinalSerial + ")";
                }

                isc.CommandTimeout = 3000;

                //step(3) streamreader Enable!
                //SqlDataReader:從數據庫獲取行
                SqlDataReader sqlDr = isc.ExecuteReader();  // sqlComad.ExecuteReader();
                //FieldCount  取得目前資料列中的資料行數目。
                DataTable dt = new DataTable();

                int iDrCount = sqlDr.FieldCount;

                //find colums 
                for (int iCnt = 0; iCnt < iDrCount; iCnt++)
                {
                    string a = sqlDr.GetName(iCnt);
                    //dt.Columns.Add(a);
                    g_lsColumName.Add(a);
                
                }

                TableColumnLanguageCheck(dt, g_lsColumName);

                //將DataReader前進到下一個資料
                while (sqlDr.Read())
                {
                    //定義一個數組，便於讀出每一行資料
                    String[] subitems = new String[iDrCount];
                    //用循環讀出每一行資料
                    for (int iCnt = 0; iCnt < iDrCount; iCnt++)
                    {
                        //讀出每一行資料保存到數組中
                        subitems[iCnt] = sqlDr[iCnt].ToString();
                        string sValue = sqlDr[iCnt].ToString();

                        iID++;
                    }
                    //將讀出的行資料增表的行中
                    dt.Rows.Add(subitems);
                }

                //DataGridView的來源為“dt”這個表
                //  dgv_MMSTable.DataSource = dt;  
                dgv_MMSData.DataSource = dt;

                if (dgv_MMSData.DataSource == null )
                {
                    MessageBox.Show(" 無任何輸入序號之紀錄資料! ");
                }
              
                //Step 6：close SyncObject 與 資料配接器物件來釋放物件所佔用的資源 mean Free not use memory session            
                sqlDr.Close();
                isc.Dispose();
                icn.Close();
                icn.Dispose();
            }
            catch (Exception k)
            {
                MessageBox.Show("Error link DBRead!~~");
                throw k;
            }
        }


        private void Read_MMS_DBTable_ChangeList()            
        {
            string strtarpath = string.Format("server={0};database={1};uid={2};pwd={3};Connect Timeout = 180", Server, Database, dbuid, dbpwd);
            //string strtarpath = "Data Source=DRAGONSERVER;Initial Catalog=MMS;Persist Security Info=True;User ID=Brian-SQl";           

            string sSingleList = tbx_InputSearch.Text.ToString();
            string s_FinalSerial = string.Empty;

            int itry = 0;


            if (cbx_statusItem1.Text == "-----")
            {
                MessageBox.Show("請選取項次再執行搜尋 ");
                return;
            }
            
            SqlConnection icn = new SqlConnection();
            icn.ConnectionString = strtarpath;

            //stop current 
            if (icn.State == ConnectionState.Open) icn.Close();
            //open start!
            icn.Open();

            try
            {
                string sItem = cbx_statusItem1.Text.ToString();
                //step(2):
                SqlCommand isc = new SqlCommand();
                isc.Connection = icn;

                if (bPuchware)
                {
                    if (sItem == "全部")
                        isc.CommandText = "SELECT " + sSqlColumn + " FROM " + g_sPuchWareHsuing + " WHERE ChangeTransNumer = " + "'" + sSingleList + "'";
                    else
                        isc.CommandText = "SELECT " + sSqlColumn + " FROM " + g_sPuchWareHsuing + " WHERE ChangeTransNumer = " + "'" + sSingleList + "'"+" AND ChangeTransLine = " + "'" + sItem + "'";
                                        
                }
                else if (bPick)
                {
                    if (sItem == "全部")                      
                        isc.CommandText = "SELECT * FROM " + g_sWorkTable + " WHERE Picking_Category_Number = "+ "'" + sSingleList + "'";
                    else
                      isc.CommandText = "SELECT * FROM " + g_sWorkTable + " WHERE Picking_Category_Number = "+ "'" + sSingleList + "'"+" AND Picking_SourceLineItem = " + "'" + sItem + "'";

                }
                else if (breturn)
                {
                    if (sItem == "全部")
                        isc.CommandText = "SELECT * FROM " + g_sReturnJob + " WHERE Return_Category_Number = " + "'" + sSingleList + "'";
                    else
                        isc.CommandText = "SELECT * FROM " + g_sReturnJob + " WHERE Return_Category_Number = " + "'" + sSingleList + "'" + " AND Return_SourceLineItem = " + "'" + sItem + "'";        
                }

                isc.CommandTimeout = 3000;

                //step(3) streamreader Enable!
                //SqlDataReader:從數據庫獲取行
                SqlDataReader sqlDr = isc.ExecuteReader();  // sqlComad.ExecuteReader();
                //FieldCount  取得目前資料列中的資料行數目。
                DataTable dt = new DataTable();

                int iDrCount = sqlDr.FieldCount;

                //find colums 
                for (int iCnt = 0; iCnt < iDrCount; iCnt++)
                {
                    string a = sqlDr.GetName(iCnt);
                    //dt.Columns.Add(a);
                    g_lsColumName.Add(a);
                
                }

                TableColumnLanguageCheck(dt, g_lsColumName);

                //將DataReader前進到下一個資料
                while (sqlDr.Read())
                {
                    //定義一個數組，便於讀出每一行資料
                    String[] subitems = new String[iDrCount];
                    //用循環讀出每一行資料
                    for (int iCnt = 0; iCnt < iDrCount; iCnt++)
                    {
                        //讀出每一行資料保存到數組中
                        subitems[iCnt] = sqlDr[iCnt].ToString();
                        string sValue = sqlDr[iCnt].ToString();

                        itry++;
                    }
                    //將讀出的行資料增表的行中
                    dt.Rows.Add(subitems);
                }

                //DataGridView的來源為“dt”這個表
                //  dgv_MMSTable.DataSource = dt;  
                dgv_MMSData.DataSource = dt;

                if (dgv_MMSData.DataSource != null && itry >= 1)
                {
                    
                }
                else
                {
                    if (sItem == "全部")
                        MessageBox.Show("無此異動單存在!");                                                                                           
                    else
                        MessageBox.Show("此異動單(" + sSingleList + ")無此項次可顯示!");                                                                       
                }

                //Step 6：close SyncObject 與 資料配接器物件來釋放物件所佔用的資源 mean Free not use memory session            
                sqlDr.Close();
                isc.Dispose();
                icn.Close();
                icn.Dispose();
            }
            catch (Exception k)
            {
                MessageBox.Show("Error link DBRead!~~");
                throw k;
            }   
        }

        private void Read_MMS_DBTable_SrcRequest()
        {
            string strtarpath = string.Format("server={0};database={1};uid={2};pwd={3};Connect Timeout = 180", Server, Database, dbuid, dbpwd);
            //string strtarpath = "Data Source=DRAGONSERVER;Initial Catalog=MMS;Persist Security Info=True;User ID=Brian-SQl";           

            string sSrcRequestNum = tbx_InputSearch.Text.ToString();
            string s_FinalSerial = string.Empty;

            int itry = 0;
        
            SqlConnection icn = new SqlConnection();
            icn.ConnectionString = strtarpath;

            //stop current 
            if (icn.State == ConnectionState.Open) icn.Close();
            //open start!
            icn.Open();

            try
            {
               
                //step(2):
                SqlCommand isc = new SqlCommand();
                isc.Connection = icn;

                if (bPuchware)
                {
                    isc.CommandText = "SELECT " + sSqlColumn + " FROM " + g_sPuchWareHsuing + " WHERE SourceSingleNumber = " + "'" + sSrcRequestNum + "'";              
                }
                else if (bPick)
                {
                    isc.CommandText = "SELECT * FROM " + g_sWorkTable + " WHERE RequireListNumber = " + "'" + sSrcRequestNum + "'";            
                }
                else if (breturn)
                {
                    isc.CommandText = "SELECT * FROM " + g_sReturnJob + " WHERE Return_SourceListNumber = " + "'" + sSrcRequestNum + "'";
                }

                isc.CommandTimeout = 3000;

                //step(3) streamreader Enable!
                //SqlDataReader:從數據庫獲取行
                SqlDataReader sqlDr = isc.ExecuteReader();  // sqlComad.ExecuteReader();
                //FieldCount  取得目前資料列中的資料行數目。
                DataTable dt = new DataTable();

                int iDrCount = sqlDr.FieldCount;

                //find colums 
                for (int iCnt = 0; iCnt < iDrCount; iCnt++)
                {
                    string a = sqlDr.GetName(iCnt);
                    //dt.Columns.Add(a);
                    g_lsColumName.Add(a);
                }


                TableColumnLanguageCheck(dt, g_lsColumName);

                //將DataReader前進到下一個資料
                while (sqlDr.Read())
                {
                    //定義一個數組，便於讀出每一行資料
                    String[] subitems = new String[iDrCount];
                    //用循環讀出每一行資料
                    for (int iCnt = 0; iCnt < iDrCount; iCnt++)
                    {
                        //讀出每一行資料保存到數組中
                        subitems[iCnt] = sqlDr[iCnt].ToString();
                        string sValue = sqlDr[iCnt].ToString();

                        itry++;
                    }
                    //將讀出的行資料增表的行中
                    dt.Rows.Add(subitems);
                }

                //DataGridView的來源為“dt”這個表
                //  dgv_MMSTable.DataSource = dt;  
                dgv_MMSData.DataSource = dt;

                if (dgv_MMSData.DataSource != null && itry >= 1)
                {

                }
                else
                {
                   MessageBox.Show("無此來源需求單存在!");
                }

                //Step 6：close SyncObject 與 資料配接器物件來釋放物件所佔用的資源 mean Free not use memory session            
                sqlDr.Close();
                isc.Dispose();
                icn.Close();
                icn.Dispose();
            }
            catch (Exception k)
            {
                MessageBox.Show("Error link DBRead!~~");
                throw k;
            }   

        
        
        
        
        }


        //確認篩選搜尋，依cbx_SelectItem 各項選擇條件式
        private void btn_ConfirmSearch_Click(object sender, EventArgs e)
        {             
   
              //類別選項
             //"隸屬部門", "操作時間", "流水號搜尋", "異動單號搜尋", "來源需求單號" 
            dgv_MMSData.DataSource = null;
            g_lsColumName = new List<string>();
           
            if (cbx_SelectItem.Text == "隸屬部門")
            {
                if (cbx_statusItem1.Text == "---" || cbx_statusItem2.Text == "---")
                {
                    MessageBox.Show("請選取公司和部門再做確認，TKS!");                
                }
                else 
                {
                    Read_MMS_DBTable_ID_PART();                                    
                }            
            }
            else if (cbx_SelectItem.Text == "操作時間")
            {

                Read_MMS_DBTable_Date();

            }
            else if (cbx_SelectItem.Text == "流水號搜尋")
            {

                Read_MMS_DBTable_SERIAL();

            }
            else if (cbx_SelectItem.Text == "異動單號搜尋")
            {

                Read_MMS_DBTable_ChangeList();


            }
            else if (cbx_SelectItem.Text == "來源需求單號")
            {

                Read_MMS_DBTable_SrcRequest();

            }            
            else
            {

                
            

            }


        }


       
    }
}
