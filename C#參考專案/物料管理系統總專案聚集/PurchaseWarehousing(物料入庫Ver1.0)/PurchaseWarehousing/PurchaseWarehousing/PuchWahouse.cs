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

//new add XML Format ,2017.05.08,Brian
using System.Xml;




namespace PurchaseWarehousing
{
    public partial class PuchWarehousing : Form
    {
        #region 全域變數宣告

        Thread thRun;

        char[] AutoOrMannul = { 'A', 'M' };

        //物料資料庫各連結參數，2017.04.20，Brian
        //string Server = "10.1.160.27";  // "192.168.1.3"  
        string Server = "10.1.160.95";  // "192.168.1.3"  
        string Database = "MMS";  // "jt8"              
        string dbuid = "Brian-SQL";  // "brian"
        string dbpwd = "Client123";  // "brian123"

        //字串數值bool宣告
        string g_sLanguage;

        string[] sIvalidFlag = { "Y", "N" };

        //判斷是否為正常異動單據產生
        bool g_bSingleList;
        bool g_bAuto = false;

        //判斷採購或工單入庫，2017.06.08,Brian
        bool bPuchWareCheck = false;


        private bool bcheckall = false;

        private string sSelectFinal;

        private string sID;
        private string sDepart;

        private Thread th1;

        //新增動態字串
        List<string> g_lsCountGroup = new List<string>();
        List<int> g_lsCountSlect = new List<int>();
        List<int> g_lStrogeNumber = new List<int>();

        //caculus float search time,2017.05.05,Brian
        double dbCount = 0;

        //條碼增加次數
        int iadd = 0 ,itemp = 0;    
        int iPartNO ;
        int g_SelectRow = 0;
        int iItemNum = 0;

        //new add ScanBarcode KeyPress "Enter" to Confirm，2017.06.06，Brian
        public int g_iEnterPress = 0; 

        //進度百分比表MAX數值,2017.05.08,Brian
        int iCentMaxValue = 0;

        //icount algorithm time 
        int iCaculusSecond = 0;

        //Find 資料庫最大ID 2017,05,02,Brian
        int g_iIdMaxLog = 0;

        //2017.05.05，searchtime scan and record
        int iSearchtime = 0;

        //確認入庫判斷數值變數
        float g_fbarcodeNum = 0, g_fTableNum = 0;

        //動態字型大小宣告
        private FontStyle g_fsView;
        private FontFamily g_fmInfo;
        private Font g_ftType;

        //新增Table類別結構
        TableInfo tbinfo;
        List<string> g_lsColumName;
        List<int> g_Listcount;
        List<float> g_AddItemNum = new List<float>();
        List<string> g_ConfigYN ;
        List<string> g_GroupItem = new List<string>();

        List<string> g_sProductDate = new List<string>();
        
        //新增DAList
        List<DataInfo> DAList = new List<DataInfo>();

        string g_sIDCollect;
        private string sItemNumber = "'10','2S'";

        //測試用
        //private string sItemNumber = "'10','17'";

        //公用表之資料庫索引字串(all Column)
        //private string g_sInforMixSelectCmd = "SELECT * FROM pof_file";

        //公用表之資料庫索引字串(direct Column)
        private string g_sInforMixSelectCmd = "SELECT" + " pof00,pof01,pof02,pof04,pof11,pof111,pof112,pof16,pof18,pof19,pof20,pofudate,pofbconf " + "FROM pof_file";

        //公用表之資料庫索引字串( Column)，2017.04.26，Brian
       // private string g_sInforMixCheckCmd = "SELECT" + " pof01,pof02,pofbconf " + "FROM pof_file";

        //更新物料表欄位(Invalid)，2017.06.14，Brian       
        private string SqlUpdatePuchWareTable = "UPDATE PurchasingWarehousingSummary SET Invalid = ";
        

        //test Update pof_file  2017.04.05 Brian
        private string sOdbcUpdateConf = "UPDATE pof_file SET pofbconf =";

        //將公用表個欄位值索引後複製到物料管理資料表，2017.04.28，Brian
        private string InforMixCopyToSql = "SELECT" + " pof02,pof03,pof07,pof11,pof18,pof19,pof20 " + "FROM pof_file";


        //Insert SQL 物料Table
        private string SqlInsertTry = "INSERT INTO PurchasingWarehousingSummary";

//        string sSqlColumn = "[ChangeDepartment],[DebitDate],[MaterialNameNumber],[SourceSingleNumber],[SourceLineItem]";
      
        //更新為以下搜索項，2017.06.08，Brian        
        string sSqlColumn = "[FileID],[ChangeCategory],[ChangeTransNumer],[ChangeTransLine],[ChangeDepartment],[DebitDate],[MaterialNameNumber],[SourceSingleNumber],[SourceLineItem],[PurchaseOrderNumber],[DataCreatorMember],[DataCreatorDepartment],[ScanLabelCount],[BindingNumber],[SearchTime],[BindingLotNumber],[AutoOrMannulMode],[ActualStrogeNumber],[FloatStorageNumber],[ProductDateRecord],[Invalid]";

        //原測試OK!
        //string sSqlColumn = "[FileID],[ChangeTransNumer],[ChangeDepartment],[DebitDate],[MaterialNameNumber],[SourceSingleNumber],[SourceLineItem],[DataCreatorMember],[DataCreatorDepartment],[ScanLabelCount],[BindingNumber],[SearchTime],[BindingLotNumber],[AutoOrMannulMode],[ActualStrogeNumber],[FloatStorageNumber],[ProductDateRecord]";


        //SQL.MMS.dbo.table Name　
        String g_sTableName = "PurchasingWarehousingSummary";

        String g_sTableName_2 = "WorkSingleStorage";


        //新增群組Group 命名ID，2016.08.19，By Brian
        string[] g_sGroupName = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

        //儲存 XML 路徑
      //  public string sXMLfilePath = System.AppDomain.CurrentDomain.BaseDirectory;
         public string sXMLfilePath = "D:\\MMS";


        //更新SQL 指定欄位 SearchTime，2017.05.05，Brian
        private string SqlUpdate = "UPDATE PurchasingWarehousingSummary SET SearchTime = ";

        //利用StopWatch 來使用計時消耗時間
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();//引用stopwatch物件
        
        //引用Dll宣告
        //DataInformation.Structure.structUserInformation g_structUserInformation = new DataInformation.Structure.structUserInformation();

        //引用Classs宣告
        //Class.Structure.UserInformation g_UserInformation = new Class.Structure.UserInformation();
        DBProcess.Class.Structure.UserInformation g_structUserInformation = new DBProcess.Class.Structure.UserInformation();
       

        //視窗間溝通之宣告
        #endregion

        #region 多國語言介面宣告,2017-03-24 by Dragon
        //要給對的資源檔路徑，才能正確讀取 //以 "Localize.RES.localize" 為例
        //MicroPhoneTestTools 是本project的namespace //Resourcees 是資源檔的目錄 //localize是資源檔的名稱, 後面的語系及副檔名不需要填入
       // static ResourceManager SysLanguageRM = new ResourceManager("MMS.Resources.localize", Assembly.GetExecutingAssembly());


        static ResourceManager SysLanguageRM = new ResourceManager("PurchaseWarehousing.Resources.localize", Assembly.GetExecutingAssembly());                      
        CultureInfo SysLanguageCI = new CultureInfo(CultureInfo.CurrentCulture.Name);                 //得到系統目前的語言設定
        #endregion


        private string strTest;

        public string String1
        {
            set {
                strTest = value;            
            }        
        }

        public void setValue()
        {         
          this.tbx_IDCheck.Text =  strTest;      
        }

        public PuchWarehousing()
        {
            InitializeComponent();

            #region 根據得到的系統語言,來設定AP的語系,如果都沒有就選英文,2017-03-28 by Dragon
            try
            {
                g_sLanguage = SysLanguageCI.Name;                      //得到語系,2017-3-28 by Dragon
                if (g_sLanguage == "en-US")
                {
                    SysLanguageCI = new CultureInfo("en-US");
                }
                else if (g_sLanguage == "zh-TW")
                {
                    SysLanguageCI = new CultureInfo("zh-TW");
                }
                else if (g_sLanguage == "zh-CN")
                {
                    SysLanguageCI = new CultureInfo("zh-CN");
                }
                else
                {
                    SysLanguageCI = new CultureInfo("en-US");
                }

                SetAPLanguage();    //設定多國語言UI介面,2017-03-24 by Dragon
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            #endregion         
        }


        //測試收到MMS傳來的資訊，2017.05.04，Brian
        //public void CatchData(string TextData)
//        public void CatchData(DataInformation.Structure.structUserInformation allstruct)
       // public void CatchData(DBProcess.Class.Structure.UserInformation allstruct)

        public void CatchData(string sLanguage, DBProcess.Class.Structure.UserInformation allstruct)        
        {                    
            //tbx_ReciveNum.Text = TextData;
           sID = tbx_IDCheck.Text = allstruct.sLoginID.ToString();            
           sDepart = tbx_DepartCheck.Text = allstruct.sDepartment.ToString();

           tbx_IDCheck.ReadOnly = tbx_DepartCheck.ReadOnly = true;

           g_sLanguage = sLanguage;                       //得到主視窗傳來語系,2017-05-10.Add by Dragon.
           SysLanguageCI = new CultureInfo(g_sLanguage);  //得到主視窗傳來語系,2017-05-10.Add by Dragon.
           SetAPLanguage();       
        }

        private void btn_Readprivate_Click(object sender, EventArgs e)
        {


        }

        private void btn_Updateprivate_Click(object sender, EventArgs e)
        {


        }

        private void btn_Readpublic_Click(object sender, EventArgs e)
        {



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
              //  meustrpItem_System.Text = SysLanguageRM.GetString("meustrpItem_System");
                //  btn_Readprivate.Text = SysLanguageRM.GetString("btn_Readprivate");
                //  lbl_StatusLogID.Text = SysLanguageRM.GetString("lbl_StatusLogID");
                //  tbx_ColumnCount.Text = SysLanguageRM.GetString("tbx_ColumnCount");

                this.lbl_TitleName.Text = SysLanguageRM.GetString("lbl_TitleName");
                

                //new add Componet device and setting GetString ,2017.04.17,Brian 
                                                            
                //Lable
                this.lbl_Run1.Text = SysLanguageRM.GetString("lbl_Run1");
                this.lbl_Run2.Text = SysLanguageRM.GetString("lbl_Run2");
                this.lbl_SqlStatus.Text = SysLanguageRM.GetString("lbl_SqlStatus");                
                this.lbl_WareScan.Text = SysLanguageRM.GetString("lbl_WareScan");
                this.lbl_ReasonReport.Text = SysLanguageRM.GetString("lbl_ReasonReport");
                this.lbl_ObjectBarcode.Text = SysLanguageRM.GetString("lbl_ObjectBarcode");
                this.lbl_SourceNum.Text = SysLanguageRM.GetString("lbl_SourceNum");
                this.lbl_PuchWarePaper.Text = SysLanguageRM.GetString("lbl_PuchWarePaper");
                this.lbl_SelectRowItem.Text = SysLanguageRM.GetString("lbl_SelectRowItem");
                this.lbl_WaitRun.Text = SysLanguageRM.GetString("lbl_WaitRun");
                this.lbl_ID.Text = SysLanguageRM.GetString("lbl_ID");
                this.lbl_Department.Text = SysLanguageRM.GetString("lbl_Department");
                this.lbl_ChangeClass.Text = SysLanguageRM.GetString("lbl_ChangeClass");
                                                                                                             
                //Group
                this.gbx_DataTable.Text = SysLanguageRM.GetString("gbx_DataTable");
                this.gbx_MannulCheck.Text = SysLanguageRM.GetString("gbx_MannulCheck");
                this.gbx_AutoMode.Text = SysLanguageRM.GetString("gbx_AutoMode");
                this.gbx_MannulMode.Text = SysLanguageRM.GetString("gbx_MannulMode");
                 
                //RadioButton
                this.rbn_AutoScan.Text = SysLanguageRM.GetString("rbn_AutoScan");
                this.rbn_ManualScan.Text = SysLanguageRM.GetString("rbn_ManualScan");
            
                //Button
                this.btn_Reset.Text = SysLanguageRM.GetString("btn_Reset");               
                this.btn_addItem.Text = SysLanguageRM.GetString("btn_addItem");
                this.btn_AutoConfirm.Text = SysLanguageRM.GetString("btn_AutoConfirm");
                this.btn_ResonConfirm.Text = SysLanguageRM.GetString("btn_ResonConfirm");                
                this.btn_WareSingConfirm.Text = SysLanguageRM.GetString("btn_WareSingConfirm");               
                this.btn_ReadSourceTable.Text = SysLanguageRM.GetString("btn_ReadSourceTable");               
                this.btn_Readpublic.Text = SysLanguageRM.GetString("btn_Readpublic");
                this.btn_PreviewCheck.Text = SysLanguageRM.GetString("btn_PreviewCheck");
                this.btn_addItem2.Text = SysLanguageRM.GetString("btn_addItem2");                
                this.btn_ManualConfirm.Text = SysLanguageRM.GetString("btn_ManualConfirm");
                this.btn_ClearItem.Text = SysLanguageRM.GetString("btn_ClearItem");
   
                //MessageBox  

                //CheckBox

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error: ");
            }
        }
        #endregion

        //找出Insert 最大項次，2017.05.05，Brian
        public int DB_FileIdFindMax()
        {
            int icatch = 1;
            string sSingleList = tbx_WareSingScan.Text.ToString();
            string s_cmd = "";
            string s_InforMixCon = string.Format("server={0};database={1};uid={2};pwd={3};Connect Timeout = 180", Server, Database, dbuid, dbpwd);

            //+"'" + " WHERE pof01 = " + "'" + sSingleList + "'" + " AND pof02 = " + "'" + sItemNum + "'" + " AND pof20 = " + "'" + sPuch + "'"; 

            //採購入庫
            if (bPuchWareCheck) 
            {
                s_cmd = " SELECT MAX(FileID) FROM " + g_sTableName + " WHERE ChangeTransNumer = " + "'" + sSingleList + "'" + " AND SourceLineItem = " + "'" + iPartNO.ToString() + "'"; 
            }
            else //工單入庫
            {
                s_cmd = " SELECT MAX(FileID) FROM " + g_sTableName + " WHERE ChangeTransNumer = " + "'" + sSingleList + "'" + " AND ChangeTransLine = " + "'" + iPartNO.ToString() + "'";             
                 //s_cmd = " SELECT MAX(Work_ID) FROM " + g_sTableName_2 + " WHERE Work_Category_Number = " + "'" + sSingleList + "'" + " AND Work_SourceLineItem = " + "'" + iPartNO.ToString() + "'";         
            }
            
           
            SqlConnection icn = new SqlConnection();

            icn.ConnectionString = s_InforMixCon;

            if (icn.State == ConnectionState.Open)
                icn.Close();

            try
            {
                icn.Open();

                SqlCommand cmd = new SqlCommand(s_cmd, icn);
                cmd.CommandText = s_cmd;
                cmd.CommandTimeout = 600;
                cmd.CommandType = CommandType.Text;

                SqlDataReader oleDr = cmd.ExecuteReader(); //從ole讀取數據
                int OleCount = oleDr.FieldCount;

                //create a DataTable(new)
                DataTable dt = new DataTable();

                //find colums 
                for (int iCnt = 0; iCnt < OleCount; iCnt++)
                {
                    string a = oleDr.GetName(iCnt);
                    dt.Columns.Add(a);
                }

                //將DataReader前進到下一個資料
                while (oleDr.Read())
                {
                    //定義一個數組，便於讀出每一行資料
                    String[] s_subitems = new String[OleCount];
                    //用循環讀出每一行資料
                    for (int iCnt = 0; iCnt < OleCount; iCnt++)
                    {
                        //讀出每一行資料保存到數組中
                        s_subitems[iCnt] = oleDr[iCnt].ToString();
                        //  string sValue = sqlDr[iCnt].ToString();

                        icatch = int.Parse(s_subitems[iCnt]);

                        return icatch;
                        
                    }
                    //將讀出的行資料增表的行中
                    dt.Rows.Add(s_subitems);
                }
               
                oleDr.Close();
                cmd.Dispose();
                icn.Close();               

            }
            catch (Exception cp)
            {
                //抓錯誤訊息
                MessageBox.Show(cp.Message, " DB_FileIdFindMax 警告!!!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                throw cp;
            }

            return 1;
        }



        public int DB_FileIdFindMaxLog()
        {
            string s_cmd = "";
            string s_InforMixCon = string.Format("server={0};database={1};uid={2};pwd={3};Connect Timeout = 180", Server, Database, dbuid, dbpwd);

            int iIdFirst = 1;

            s_cmd = " SELECT MAX(FileID) FROM " + g_sTableName;
            //s_cmd = " SELECT MAX(FileID) FROM " + g_iIdMaxLog;


            SqlConnection icn = new SqlConnection();

            icn.ConnectionString = s_InforMixCon;

            if (icn.State == ConnectionState.Open) 
                icn.Close();

            try
            {
                icn.Open();

                SqlCommand cmd = new SqlCommand(s_cmd, icn);
                cmd.CommandText = s_cmd;
                cmd.CommandTimeout = 600;
                cmd.CommandType = CommandType.Text;

                SqlDataReader oleDr = cmd.ExecuteReader(); //從ole讀取數據
                int OleCount = oleDr.FieldCount;

                //create a DataTable(new)
                DataTable dt = new DataTable();

                //find colums 
                for (int iCnt = 0; iCnt < OleCount; iCnt++)
                {
                    string a = oleDr.GetName(iCnt);
                    dt.Columns.Add(a);
                }

                //將DataReader前進到下一個資料
                while (oleDr.Read())
                {
                    //定義一個數組，便於讀出每一行資料
                    String[] s_subitems = new String[OleCount];
                    //用循環讀出每一行資料
                    for (int iCnt = 0; iCnt < OleCount; iCnt++)
                    {
                        //讀出每一行資料保存到數組中
                        s_subitems[iCnt] = oleDr[iCnt].ToString();
                        //  string sValue = sqlDr[iCnt].ToString();

                        if (s_subitems[iCnt] =="")
                            return iIdFirst;
                        else
                        {
                            g_iIdMaxLog = int.Parse(s_subitems[iCnt]);
                            return g_iIdMaxLog + 1;                                           
                        }
                    }
                    //將讀出的行資料增表的行中
                    dt.Rows.Add(s_subitems);
                }

                /*
                dgv_SqlTableData.DataSource = dt;

                g_iIdMaxLog = int.Parse(dgv_SqlTableData.Rows[0].Cells[0].Value.ToString());
                
                if (g_iIdMaxLog < 0)
                {
                    g_iIdMaxLog = 0;
                }
                else
                {
                    g_iIdMaxLog = g_iIdMaxLog + 1;
                }
                dgv_SqlTableData.DataSource = null;
                
                 */ 

                oleDr.Close();
                cmd.Dispose();
                icn.Close();

                return iIdFirst;

            }
            catch (Exception cp)
            {
                //抓錯誤訊息
                MessageBox.Show(cp.Message, " DB_FileIdFindMaxLog 警告!!!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                throw cp;
            }

        }

        //將table column 欄位針對語系做調整，2017.04.24，Brian
        private void TableColumnLanguageCheck(DataTable dt, List<string> slistName)
        {
            string sTemp = string.Empty;
            tbinfo = new TableInfo();
            
            for (int iNum = 0; iNum < slistName.Count(); iNum++)
            {
                switch (iNum)
                {                    
                    case 0:
                        //pof00
                        if (g_sLanguage != "en-US")
                            sTemp = tbinfo.SingalListNum = (g_sLanguage == "zh-TW") ? "異動別" : "异动别";                      
                        else                        
                            sTemp = tbinfo.SingalListNum = slistName[iNum].ToString();                        
                        break;
                    case 1:
                        //pof01
                        if (g_sLanguage != "en-US")
                            sTemp = tbinfo.SingalListNum = (g_sLanguage == "zh-TW") ? "異動編號" : "异动编号";
                        else
                            sTemp = tbinfo.SingalListNum = slistName[iNum].ToString();
                        break;

                    case 2:
                        //pof02
                        if (g_sLanguage != "en-US")
                            sTemp = tbinfo.Item = (g_sLanguage == "zh-TW") ? "項次" : "项次";
                        else
                            sTemp = tbinfo.Item = slistName[iNum].ToString();                                              
                        break;
                    case 3:
                        //pof04
                        if (g_sLanguage != "en-US")
                            sTemp = tbinfo.Vender = (g_sLanguage == "zh-TW") ? "廠商" : "厂商";     
                        else
                            sTemp = tbinfo.Vender = slistName[iNum].ToString();                        
                       
                        break;
                    case 4:
                        //pof11      
                        if (g_sLanguage != "en-US")
                            sTemp = tbinfo.PartNo = (g_sLanguage == "zh-TW") ? "料號" : "料号";
                        else
                            sTemp = tbinfo.PartNo = slistName[iNum].ToString();                        
                       
                        break;
                    case 5:
                        //pof111      
                        if (g_sLanguage != "en-US")
                            sTemp = tbinfo.ChObjName = (g_sLanguage == "zh-TW") ? "中文品名" : "中文品名";
                        else
                            sTemp = tbinfo.ChObjName = slistName[iNum].ToString();                        
                       
                        break;
                    case 6:
                        //pof112          
                        if (g_sLanguage != "en-US")
                            sTemp = tbinfo.Spec = (g_sLanguage == "zh-TW") ? "規格" : "规格";
                        else
                            sTemp = tbinfo.Spec = slistName[iNum].ToString();                        
                       
                        break;
                    case 7:
                        //pof16          
                        if (g_sLanguage != "en-US")
                            sTemp = tbinfo.TotalAmontStorage = (g_sLanguage == "zh-TW") ? "入庫量" : "入库量";
                        else
                            sTemp = tbinfo.TotalAmontStorage = slistName[iNum].ToString();                        
                       
                        break;
                    case 8:
                        //pof18          
                        if (g_sLanguage != "en-US")
                            sTemp = tbinfo.SourceSingle = (g_sLanguage == "zh-TW") ? "來源單號" : "来源单号";
                        else
                            sTemp = tbinfo.SourceSingle  = slistName[iNum].ToString();

                        break;
                    case 9:
                        //pof19          
                        if (g_sLanguage != "en-US")
                            sTemp = tbinfo.SourceItem = (g_sLanguage == "zh-TW") ? "來源項次" : "来源项次";
                        else
                            sTemp = tbinfo.SourceItem = slistName[iNum].ToString();
                        break;
                    case 10:
                        //pof20      
                        if (g_sLanguage != "en-US")                                               
                            sTemp = tbinfo.PurchOrderNo= (g_sLanguage == "zh-TW") ? "採購單號" : "采购单号";
                        else
                            sTemp = tbinfo.PurchOrderNo = slistName[iNum].ToString();                                               
                        break;
                    case 11:
                        //pofudate      
                        if (g_sLanguage != "en-US")
                            sTemp = tbinfo.ProductDate = (g_sLanguage == "zh-TW") ? "資料建立日" : "资料建立日";
                        else
                            sTemp = tbinfo.ProductDate = slistName[iNum].ToString();
                        break;                   
                    case 12:
                        //pofbconf      
                        if (g_sLanguage != "en-US")
                            sTemp = tbinfo.CheckConfStatus = (g_sLanguage == "zh-TW") ? "確認設定" : "确认设定";
                        else
                            sTemp = tbinfo.CheckConfStatus = slistName[iNum].ToString();
                        break;
                                        
                    default:
                        break;
                }

                dt.Columns.Add(sTemp);

            }

        }


        private void btn_WareSingConfirm_Click(object sender, EventArgs e)
        {
            string strConnection = "Dsn=ERP-032;uid=dragon;database=erp032;host=10.1.1.2;srvr=c02;serv=50151;pro=onsoctcp;cloc=zh_tw.big5;dloc=zh_tw.big5;vmb=0;curb=0;scur=0;icur=0;oac=1;optofc=0;rkc=0;odtyp=0;fbs=4096;ddfp=0;dnl=0";

            //一開始預設異動單據為false(意旨沒有搜尋到)
            g_bSingleList = false;
            int itry = 0 ;
            g_ConfigYN = new List<string>();

            //取得之掃描異動單據之單號
            string sSingleList = tbx_WareSingScan.Text.ToString();

            //一開始按鈕被景色為紅色
            btn_WareSingConfirm.BackColor = Color.Red;

            OdbcConnection odbc = new OdbcConnection();
            odbc.ConnectionString = strConnection;

            //stop current 
            if (odbc.State == ConnectionState.Open)
                odbc.Close();

            //open start!
            odbc.Open();

            try {
                
                OdbcCommand odbcmd = new OdbcCommand();
                odbcmd.Connection = odbc;
                //查尋採購入庫專用CMD，2017.04.26,Brian 舊式(只有10 採購入庫)
                //odbcmd.CommandText = g_sInforMixSelectCmd + " WHERE pof01 = " + "'" + sSingleList + "'" + " AND pof00 = 10";

                //查尋採購入庫專用CMD，2017.06.08,Brian 新式(10 採購入庫 和 2S 工單入庫)                                
                odbcmd.CommandText = g_sInforMixSelectCmd + " WHERE pof00" + " IN " + "(" + sItemNumber + ")" + " AND pof01 = " + "'" + sSingleList + "'";

                OdbcDataReader odbRead = odbcmd.ExecuteReader();

                dgv_SqlTableData.AllowUserToAddRows = false;
                g_lsColumName = new List<string>();


                DataTable dt = new DataTable();

                int iDrCount = odbRead.FieldCount; //sqlDr.FieldCount;


                //find colums 
                for (int iCnt = 0; iCnt < iDrCount; iCnt++)
                {
                    // 2 mean pof02 ,18 mean pof16, 22 mean pof20                    
                    string a = odbRead.GetName(iCnt);
                   // dt.Columns.Add(a);

                    //新增動態陣列
                    g_lsColumName.Add(a);
                    
                    //dt.Rows.Add((DataRow)sqlDr[iCnt]);
                }

                 //轉換當前選擇字體，2017.04.24 , Brian                     
                 TableColumnLanguageCheck(dt, g_lsColumName);

                
                //將DataReader前進到下一個資料
                while (odbRead.Read())
                {
                    //定義一個數組，便於讀出每一行資料
                    String[] subitems = new String[iDrCount];
                    //用循環讀出每一行資料
                    for (int iCnt = 0; iCnt < iDrCount; iCnt++)
                    {
                        //讀出每一行資料保存到數組中                       
                        subitems[iCnt] = odbRead[iCnt].ToString();
                        string sValue = odbRead[iCnt].ToString();
                       
                        if(sValue == sSingleList)
                            itry++;

                        if (iCnt == iDrCount-1)
                            g_ConfigYN.Add(sValue);
                 
                    }
                    //將讀出的行資料增表的行中                                                                  
                      dt.Rows.Add(subitems);
                }                
               // dt.Rows[dt.Rows.Count-1].Delete();
                                                             
                //DataGridView的來源為“dt”這個表               
                dgv_SqlTableData.DataSource = dt;

                //because this Singlelist Number just OnlyOne ，so bellow Judge the situation
                //this Singlelist Number maybe find Related of all ,so No limit in here ，2017.04.19，Brian
                //if (dgv_SqlTableData.DataSource != null && itry ==1)
                if (dgv_SqlTableData.DataSource != null && itry >= 1)                
                {
                    g_bSingleList = true;

                    //lbl_Run2.Visible = rbn_AutoScan.Visible = rbn_ManualScan.Visible = true;

                    //手動隱藏
                    //lbl_Run2.Visible = rbn_AutoScan.Visible = true;

                    lbl_Run2.Visible  = true;


                    rbn_ManualScan .Checked = rbn_AutoScan.Checked = false;

                  
                    //顯示動態Label
                    g_fsView = lbl_SqlStatus.Font.Style;
                    g_fmInfo = new FontFamily(lbl_SqlStatus.Font.Name);
                    g_ftType = new Font(g_fmInfo, 20, g_fsView);
                    lbl_SqlStatus.Font = g_ftType;
                    lbl_SqlStatus.ForeColor = Color.Green;

                    if (g_sLanguage == "en-US")
                    {
                        //SysLanguageCI = new CultureInfo("en-US");
                        lbl_SqlStatus.Text = "+ Public table link to Ok +";
                    }
                    else if (g_sLanguage == "zh-TW")
                    {
                        // SysLanguageCI = new CultureInfo("zh-TW");
                        lbl_SqlStatus.Text = "+公用表連結正常+";                       
                    }
                    else if (g_sLanguage == "zh-CN")
                    {
                        //SysLanguageCI = new CultureInfo("zh-CN");
                        lbl_SqlStatus.Text = "+公用表连结正常+";
                    }

                    //gbx_AutoMode.Visible  = true;

                    //int iMaxID = DB_FileIdFindMaxLog();
                   // MessageBox.Show("最大ID值為: " + iMaxID.ToString());
                }
                else
                {
                    g_bSingleList = false;

                    gbx_AutoMode.Visible = gbx_MannulMode.Visible = rbn_ManualScan .Visible= rbn_AutoScan.Visible = false;

                    rbn_ManualScan.Checked = rbn_AutoScan.Checked = false;

                    //顯示動態Label
                    g_fsView = lbl_SqlStatus.Font.Style;
                    g_fmInfo = new FontFamily(lbl_SqlStatus.Font.Name);
                    g_ftType = new Font(g_fmInfo, 20, g_fsView);
                    lbl_SqlStatus.Font = g_ftType;
                    lbl_SqlStatus.ForeColor = Color.Red;


                    if (g_sLanguage == "en-US")
                    {
                        //SysLanguageCI = new CultureInfo("en-US");
                        lbl_SqlStatus.Text = "PublicTable-link failed ";
                        lbl_SelectRowItem.Text = "Waiting for choice";
                        lbl_ChangeType.Text = "No Data";
                        MessageBox.Show("System common form no such transaction documents, please re-confirm whether it is wrong!");

                    }
                    else if (g_sLanguage == "zh-TW")
                    {
                        // SysLanguageCI = new CultureInfo("zh-TW");
                        lbl_SqlStatus.Text = "-公用表連結失敗-";
                        lbl_SelectRowItem.Text = "等待選擇";
                        lbl_ChangeType.Text = "無資料";
                        MessageBox.Show("系統公用表無此異動單據，請再次確認是否有誤!");
                    }
                    else if (g_sLanguage == "zh-CN")
                    {
                        //SysLanguageCI = new CultureInfo("zh-CN");
                        lbl_SqlStatus.Text = "-公用表连结失败-";
                        lbl_SelectRowItem.Text = "等待选择";
                        lbl_ChangeType.Text = "无资料";
                        MessageBox.Show("系统公用表无此异动单据，请再次确认是否有误!");
                    }
                }
                
                //讀取資源釋放
                odbRead.Close();
                odbcmd.Dispose();
                odbc.Close();                     
            }
            catch(Exception k)
            {
                MessageBox.Show("Error No SingleList Exist!");
                throw k;   
            }

            //執行流程結束按鈕被景色為深綠色
            btn_WareSingConfirm.BackColor = Color.Turquoise;

        }

        private void PuchaseWarehousing_Load(object sender, EventArgs e)
        {
           // tbx_TestALL.Visible = false;

            //設定選擇DataTable 模式，暫定FullRowSelect,2017.04.20，brian
            dgv_SqlTableData.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            //設定選擇ListBox 選擇項目模式，暫定MultiExtended,2017.04.24，brian
           // lbx_ItemNum.SelectionMode = SelectionMode.MultiExtended;
            lbx_ItemNum.SelectionMode = SelectionMode.MultiSimple;


            //以下為各物件顯示狀態模式預設值
            btn_addItem .Visible = btn_ReadSourceTable.Visible = btn_Readpublic.Visible = lbl_Run2.Visible = false;
            cbx_ItemNum.Visible= rbn_AutoScan.Visible = rbn_ManualScan.Visible = false;
            gbx_MannulCheck.Visible = gbx_AutoMode.Visible = gbx_MannulMode.Visible = false;

            btn_ResonConfirm .Visible  = lbl_ReasonReport.Visible = tbx_Reason.Visible = false;
            lbl_ChangeClass.Visible = lbl_ChangeType.Visible = false;
            btn_DelPuchExist.Visible = lbl_Invalid.Visible = false;
            tbx_Reason.Text = string.Empty;
           
        }

        private void rbn_AutoScan_CheckedChanged(object sender, EventArgs e)
        {            
            /*
            string sClass = tbx_WareSingScan.Text.ToString();
            sClass = sClass.Trim();
            
            if(sClass.StartsWith("sib-") || sClass.StartsWith("sia-"))
                bPuchWareCheck = true ;
            else   
                bPuchWareCheck = false ;
            */

            if (rbn_AutoScan.Checked)
            {
                //gbx_MannulCheck.Visible = rbn_ManualScan.Visible = gbx_MannulMode.Visible = false;
                gbx_MannulCheck.Visible  = gbx_MannulMode.Visible = false;
                lbx_ItemNum.Visible = gbx_AutoMode.Visible = true;
                tbx_Reason.ReadOnly = btn_addItem.Visible = false;
                lbl_ChangeClass.Visible = lbl_ChangeType.Visible = true;
                g_bAuto = true;


                //顯示動態Label
                g_fsView = lbl_ChangeType.Font.Style;
                g_fmInfo = new FontFamily(lbl_ChangeType.Font.Name);
                g_ftType = new Font(g_fmInfo, 15, g_fsView);
                lbl_ChangeType.Font = g_ftType;
                lbl_ChangeType.ForeColor = Color.DarkRed;
                
                if (g_sLanguage == "en-US")
                {
                    lbl_ChangeType.Text = (bPuchWareCheck == true) ? "PuchWarehouse" : "WorkWarehouse";
                }
                else if (g_sLanguage == "zh-TW")
                {
                    lbl_ChangeType.Text = (bPuchWareCheck == true) ? "採購入庫" : "工單入庫";
                }
                else if (g_sLanguage == "zh-CN")
                {
                    lbl_ChangeType.Text = (bPuchWareCheck == true) ? "采购入库" : "工单入库";
                }
            }
            else
            {
            
            }

        }

        private void rbn_ManualScan_CheckedChanged(object sender, EventArgs e)
        {
            if (rbn_ManualScan.Checked)
            {
                //gbx_MannulMode .Visible= rbn_AutoScan.Visible = gbx_AutoMode.Visible = false;
                gbx_AutoMode.Visible = false;
                gbx_MannulCheck.Visible = true;

                if (gbx_MannulCheck.Visible)
                {
                    lbl_ReasonReport.Visible = tbx_Reason.Visible = btn_ResonConfirm.Visible = true;
                    tbx_Reason.Text = string.Empty;
                }

                g_bAuto = false;
            }
            else
            {
            


            }


            
          //  lbl_ReasonReport.Visible = tbx_Reason.Visible = false;
         //   tbx_Reason.Text = string.Empty;       
            
        }

        private void btn_Reset_Click(object sender, EventArgs e)
        {            
            //將物件顯示屬性重新制定，2017.04.19，Brian
            tbx_Reason.ReadOnly= lbx_ItemNum.Visible = lbl_Run2.Visible = false;
            dgv_SqlTableData.DataSource = null;
            rbn_AutoScan.Visible = rbn_ManualScan.Visible = false;
            btn_addItem .Visible= rbn_AutoScan.Checked = rbn_ManualScan.Checked = false;
            gbx_MannulCheck .Visible= gbx_AutoMode.Visible = gbx_MannulMode.Visible = false;
            tbx_WareSingScan.Text = tbx_Reason.Text = string.Empty;
            btn_DelPuchExist.Visible = lbl_Invalid.Visible = false;
            lbl_ChangeClass.Visible = lbl_ChangeType.Visible = lbl_dgvSelectStatus.Visible = false;           
            lbl_SelectRowItem.Text = "等待選擇";

            //顯示動態Label
            g_fsView = lbl_SqlStatus.Font.Style;
            g_fmInfo = new FontFamily(lbl_SqlStatus.Font.Name);
            g_ftType = new Font(g_fmInfo, 16, g_fsView);
            lbl_SqlStatus.Font = g_ftType;
            lbl_SqlStatus.ForeColor = Color.Orange;

            /*
            g_fsView = lbl_ChangeType.Font.Style;
            g_fmInfo = new FontFamily(lbl_ChangeType.Font.Name);
            g_ftType = new Font(g_fmInfo, 16, g_fsView);
            lbl_ChangeType.Font = g_ftType;
            lbl_ChangeType.ForeColor = Color.Yellow;
           */

            tbx_SourceText.Text = "";
            pgb_Status.Value = 0;

            //List 變數 Reset，2017.04.26，Brian             
            g_lsCountSlect = new List<int>();
            g_AddItemNum = new List<float>();
            g_sProductDate = new List<string>();


            //畫面全清除
            lbx_ItemNum.Items.Clear();
            iadd = itemp = 0;
            
            //刪除全部ListBox項目
            for (int iDel = 0; iDel < lbx_ItemNum.Items.Count; iDel++)
            {
                lbx_ItemNum.Items.RemoveAt(iDel);
            }

            if (g_sLanguage == "en-US")
            {
                lbl_SqlStatus.Text = "Link state";
                lbl_ChangeType.Text = "==";
            }
            else if (g_sLanguage == "zh-TW")
            {
                lbl_SqlStatus.Text = "連結狀態---";
                lbl_ChangeType.Text = "==";
            }
            else if (g_sLanguage == "zh-CN")
            {
                lbl_SqlStatus.Text = "连结状态---";
                lbl_ChangeType.Text = "==";
            }
            
        }

        private void btn_ResonConfirm_Click(object sender, EventArgs e)
        {


            gbx_MannulMode.Visible = (!string.IsNullOrEmpty(tbx_Reason.Text)) ? true : false;


            if (tbx_Reason.Text != "" && gbx_MannulMode.Visible)
            {
                lbl_ReasonReport.Visible = tbx_Reason.Visible = btn_ResonConfirm.Visible = true;
                tbx_Reason.ReadOnly = true;                                            
            }
            else
            {
                if (g_sLanguage == "en-US")
                {
                    MessageBox.Show("Please re-confirm whether there is a reason to write, thank you!");
                }
                else if (g_sLanguage == "zh-TW")
                {
                    MessageBox.Show("請再次確認是否有寫入理由，謝謝!");
                }
                else if (g_sLanguage == "zh-CN")
                {
                    MessageBox.Show("请再次确认是否有写入理由，谢谢!");
                }            
            }
        }

        //透過物料條碼上的資訊，做加總數量計算(視每個廠商編碼狀況做微調)
        //如JSA001-75200345-000-10000-2015-10-25-000-60000
        //第一為廠商代碼，第三到五一整串為料號
        //第七為最小包裝數量
        //第十三為總數量        
        private void btn_addItem_Click(object sender, EventArgs e)
        {

            if (dgv_SqlTableData.SelectedRows == null)
            {
                MessageBox.Show("請先選擇項次上的物料再做新增動作!");
            }
            else
            {
                //測試正常
                //MessageBox.Show(sSelectFinal);

                //資料庫字串
                string sFinalComp = sSelectFinal.Substring(0, sSelectFinal.Length - 1);
                //資料庫字串上的字串做切割
                string[] sarray = sFinalComp.Split('^');
                //MessageBox.Show(sFinalComp);
                                
                string sCutSrc = tbx_SourceText.Text.ToString();

                int iFindCount = sCutSrc.LastIndexOf("-");
                //string sCutthis1 = sCutDash.Substring(0,iFindCount);
                string[] sCut = sCutSrc.Split('-');
                string sFindNum = sCut[sCut.Length-1]; //取最後一筆為入庫量依據

                //再透過跟資料庫做比對
                string sDbCounnt = sarray[sarray.Length - 2];

                g_lsCountGroup.Add(sFindNum);

                //cbx_ItemNum.Items.Add(g_lsCountGroup[iadd]);
                string sListCharAll = "表單項次(" + iPartNO.ToString() + ") ,掃描物料包裝量(" + sFindNum + " pcs ) 增加扣帳項目";

                //iPartNO
                lbx_ItemNum.Items.Add(sListCharAll);


                //將物料管理欄位清空，2017.04.21，Brian
                th1 = new Thread(new ThreadStart(ClearTextSourceBox));
                th1.Start();

                //MessageBox.Show("-符號字串在第 " + iFindCount.ToString() + " 個" + "解析字串為:" + sFindNum + "資料庫數量:" + sDbCounnt);
                // MessageBox.Show("-符號字串在第 " + iFindCount.ToString() + " 個" + "解析字串為:" + sFindNum + "資料庫數量:" + sDbCounnt);
                MessageBox.Show(" 已加入OK !");  
            
               // if (tbx_SourceText.Text.ToString() != null)
               // tbx_SourceText.Text = string.Empty; 
                iadd++;
            }

        }

        //委派函式
        delegate void callbyUI();

        private void StopTimer()
        {
            
            if(this.InvokeRequired)
            {
                //建立一個在控制視窗上執行的委派
                callbyUI cb = new callbyUI(StopTimer);
               // tm_Caculus.Dispose();
                //在擁有控制項基礎視窗控制代碼的執行緒上執行委派。
                this.Invoke(cb);
            
            }
            else
            {
               // tm_Caculus.Stop();

                string ss = lbl_TestTime.Text.ToString();

                dbCount = Convert.ToDouble(ss);

                iSearchtime = Convert.ToInt32(dbCount);                         
            }

                    
        }



        private void ClearTextSourceBox()
        {
            tbx_SourceText.ReadOnly = true;

            Thread.Sleep(3000);
            
            if (!string.IsNullOrEmpty(tbx_SourceText.Text.ToString()))
            {
                tbx_SourceText.Text = string.Empty;
            }
             
        }

        private bool IsDeletInvalid()
        {
            string s_InforMixCon = string.Format("server={0};database={1};uid={2};pwd={3};Connect Timeout = 180", Server, Database, dbuid, dbpwd);
            
            //取得之掃描異動單據之單號
            string sSingleList = tbx_WareSingScan.Text.ToString();

            //+"'" + " WHERE pof01 = " + "'" + sSingleList + "'" + " AND pof02 = " + "'" + sItemNum + "'" + " AND pof20 = " + "'" + sPuch + "'"; 
            string s_cmd = string.Empty;
            g_sIDCollect = string.Empty;
            
            string sCheckItem = dgv_SqlTableData.Rows[g_SelectRow].Cells[2].Value.ToString();
            int iItemNum = int.Parse(sCheckItem);

            //s_cmd = " SELECT FileID FROM " + g_sTableName + " WHERE ChangeTransNumer = " + "'" + sSingleList + "'" + " AND ChangeTransLine = " + "'" + iItemNum.ToString() + "'" + " AND NOT Invalid = 'Y'";                        
            //s_cmd = " SELECT FileID FROM " + g_sTableName + " WHERE ChangeTransNumer = " + "'" + sSingleList + "'" + " AND ChangeTransLine = " + "'" + iItemNum.ToString() + "'" + " AND Invalid = 'NULL'";                        
            s_cmd = " SELECT FileID FROM " + g_sTableName + " WHERE ChangeTransNumer = " + "'" + sSingleList + "'" + " AND ChangeTransLine = " + "'" + iItemNum.ToString() + "'" + " AND Invalid = " + "'" + sIvalidFlag[1]+ "'";                        

                        
            SqlConnection icn = new SqlConnection();

            icn.ConnectionString = s_InforMixCon;

            if (icn.State == ConnectionState.Open)
                icn.Close();

            try
            {
                icn.Open();

                SqlCommand cmd = new SqlCommand(s_cmd, icn);
                cmd.CommandText = s_cmd;
                cmd.CommandTimeout = 600;
                cmd.CommandType = CommandType.Text;

                SqlDataReader oleDr = cmd.ExecuteReader(); //從ole讀取數據
                int OleCount = oleDr.FieldCount;

                //create a DataTable(new)
                DataTable dt = new DataTable();

                //find colums 
                for (int iCnt = 0; iCnt < OleCount; iCnt++)
                {
                    string a = oleDr.GetName(iCnt);
                    dt.Columns.Add(a);
                }

                //將DataReader前進到下一個資料
                while (oleDr.Read())
                {
                    //定義一個數組，便於讀出每一行資料
                    String[] s_subitems = new String[OleCount];
                    //用循環讀出每一行資料
                    for (int iCnt = 0; iCnt < OleCount; iCnt++)
                    {
                        //讀出每一行資料保存到數組中
                        s_subitems[iCnt] = oleDr[iCnt].ToString();

                        g_sIDCollect += s_subitems[iCnt] + ",";

                     //   icatch = int.Parse(s_subitems[iCnt]);
                       // return icatch;

                    }
                    //將讀出的行資料增表的行中
                    dt.Rows.Add(s_subitems);
                }

                if (g_sIDCollect != "")                
                    g_sIDCollect = g_sIDCollect.Substring(0, g_sIDCollect.Length - 1);

                oleDr.Close();
                cmd.Dispose();
                icn.Close();

            }
            catch (Exception cp)
            {
                //抓錯誤訊息
                MessageBox.Show(cp.Message, " DB_FileIdFindMax 警告!!!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                throw cp;
            }

            return (g_sIDCollect == "") ? false : true ;            
        }


        //select current Item of dgv_SqlTableData and Sendto another destination
        private void dgv_SqlTableData_SelectionChanged(object sender, EventArgs e)
        {           
            int iSelectNum = dgv_SqlTableData.Rows.GetRowCount(DataGridViewElementStates.Selected), iNum = 0;
            
            string sClass = tbx_WareSingScan.Text.ToString();
            sClass = sClass.Trim();
            
            string sTemp = string.Empty;           
            sSelectFinal = string.Empty;

            bool bchkInvalid = false;
            
            //採購入庫
            if (sClass.StartsWith("sib-") || sClass.StartsWith("sia-"))
                bPuchWareCheck = true;
            else //工單入庫            
                bPuchWareCheck = false;


            if (iSelectNum == 1)
            {
                foreach (DataGridViewRow row in dgv_SqlTableData.SelectedRows)
                {
                    g_SelectRow = iNum = row.Index;

                    int iAlais = iNum + 1;

                    iPartNO = iAlais;
                    row.Selected = true;

                  //  tbx_SourceText.Text = "選擇為第"+iNum.ToString()+"個Row Data";
                   // MessageBox.Show(iNum.ToString());
                   
                    lbl_SelectRowItem.Text = "選擇為第" + iAlais.ToString() + "個Row Data";
                   
                    
                    for (int i = 0; i < dgv_SqlTableData.ColumnCount; i++)
                    {
                        sTemp = ( !string.IsNullOrEmpty(dgv_SqlTableData.Rows[iNum].Cells[i].Value.ToString())) ? dgv_SqlTableData.Rows[iNum].Cells[i].Value.ToString():" ";

//                        sSelectFinal += "Column " + i + " 欄位:" + sTemp + Environment.NewLine;
                        
                        sTemp = sTemp.Trim();

                        sSelectFinal += sTemp +"^";


                    }
                  //  MessageBox.Show(sSelectFinal);
                }
            }
            else if (iSelectNum == 0)
            {
              lbl_Run2.Visible = rbn_AutoScan.Visible = gbx_AutoMode.Visible = rbn_AutoScan.Visible = lbl_ChangeClass.Visible = lbl_ChangeType.Visible = false;

            }

            //判定是否要做"作廢"
            bchkInvalid = IsDeletInvalid();

            //資料庫有存在之前的Insert Data
            if (bchkInvalid)
            {               
                lbl_ChangeClass.Visible = lbl_ChangeType.Visible = gbx_AutoMode.Visible = false;
                rbn_AutoScan.Visible = false;
                lbl_dgvSelectStatus.Visible = true;
                btn_DelPuchExist.Visible = lbl_Invalid.Visible = true;


                //顯示動態Label
                g_fsView = lbl_dgvSelectStatus.Font.Style;
                g_fmInfo = new FontFamily(lbl_dgvSelectStatus.Font.Name);
                g_ftType = new Font(g_fmInfo, 16, g_fsView);
                lbl_dgvSelectStatus.Font = g_ftType;

                //有異常
                if (CheckPofCONG() != true)
                {                    
                    lbl_dgvSelectStatus.ForeColor = Color.Red;
                    lbl_dgvSelectStatus.Text = "NG!異常，請立即作廢";
                     
                    //MessageBox.Show("公用表有異常，請立即作廢重新入庫!");
                }
                else //正常
                {
                    lbl_dgvSelectStatus.ForeColor = Color.Green;
                    lbl_dgvSelectStatus.Text = "PASS * 作業正常";                                
                }
            }
            else //資料庫無存在此異動單號Data
            {
                //lbl_ChangeClass.Visible = lbl_ChangeType.Visible = gbx_AutoMode.Visible = rbn_AutoScan.Visible = true;              
                rbn_AutoScan.Visible = true;
                rbn_AutoScan.Checked = false;
                gbx_AutoMode.Visible = false;
                btn_DelPuchExist.Visible = lbl_Invalid.Visible = false;
                lbl_dgvSelectStatus.Visible = false;
                tbx_SourceText.Text = "";
                
                g_lsCountSlect = new List<int>();
                g_AddItemNum = new List<float>();
                g_sProductDate = new List<string>();

                //畫面全清除
                lbx_ItemNum.Items.Clear();

                iadd = itemp = 0;

                //刪除全部ListBox項目
                for (int iDel = 0; iDel < lbx_ItemNum.Items.Count; iDel++)
                {
                    lbx_ItemNum.Items.RemoveAt(iDel);
                }                              
            }

            //MessageBox.Show("Row ID 為: " + g_sIDCollect.ToString());
        }

        private void BtnTestClick()
        {
            int iStandard = 6, iTotal = 0;

            if (dgv_SqlTableData.SelectedRows == null)
            {
                MessageBox.Show("請先選擇項次上的物料再做新增動作!");
            }
            else
            {                
                
                //先行判斷是否料號重複，2017.05.02，Brian
                
                string[] sSplit= tbx_SourceText.Text.Split('-');

                iTotal = sSplit.Length;

                if (iTotal != iStandard)
                {
                    MessageBox.Show("請掃描正確條碼格式以便作業，謝謝!");
                    tbx_SourceText.Text = "";
                    return;
                }


                // 第 0 筆為廠商代碼  第 1 和 2 筆為產品料號,第 4 到 6 筆為生產年月日
                string sVender = sSplit[0];
                string sPartNo = sSplit[1] + "-" + sSplit[2];
                // 舊式欄位
                //string sDate = sSplit[4] + "/" + sSplit[5] + "/" + sSplit[6];
                // 新式欄位
                string sDate = sSplit[3];

                /*
                char[] chartest = sDate.ToCharArray();
                for (int i = 0; i < chartest.Length; i++)
                {
                    if (i == 3 || i == 5)
                    {
                        chartest[i] += '/';
                    
                    }                                
                }
                
                string sDateNew = new String(chartest);
                */

                //資料庫字串
                string sFinalComp = sSelectFinal.Substring(0, sSelectFinal.Length - 1);
                //資料庫字串上的字串做切割
                string[] sarray = sFinalComp.Split('^');
                 
                /*
                string  str = sarray[9].ToString();
                str = str.Substring(0,8);
                str.Trim();
                */
        
                //廠商,所交產品料號,生產日期比對，確認是否重複，2017.05.02，Brian
//                if (sVender.Contains(sarray[3]) && sPartNo.Contains(sarray[4]) && sDate.Contains(str))
                //if (sVender.Contains(sarray[3]) && sPartNo.Contains(sarray[4]) && sDateNew.StartsWith(str))
                 //if (sVender.Contains(sarray[3]) && sPartNo.Contains(sarray[4]))
                if (sPartNo.Contains(sarray[4]))                
                {
                    //目前只能判斷 重複次數->1就顯示警告，其餘就繼續累加Items，2017.05.02，Brian
                    /*
                    for (int ick = 0; ick < lbx_ItemNum.Items.Count; ick++)
                    {                        
                        string scheck = lbx_ItemNum.Items[itemp-1].ToString();
                       
                        //比對Items 的Group是否重複
                        if (scheck.Contains(g_GroupItem[itemp-1]) == true && iMulitCheck <= 0)
                        {
                            iMulitCheck++;
               
                            MessageBox.Show("偵測到重複料號，請選擇另外加入，TKS!");

                            return;
                        }                       
                    }                    
                     */

                    //採購入庫需多比對"廠牌" sVender，2017.06.09，Brian
                    if (bPuchWareCheck == true && !sVender.Contains(sarray[3]))
                    {
                        MessageBox.Show("條碼比對Vender(廠牌)有誤! 不是此入庫之廠商代號");
                        tbx_SourceText.Text = "";
                        return;
                    }
                   

                    //針對Group做重複確認
                    g_GroupItem.Add(g_sGroupName[iadd]);


                    //pof00,pof01,pof02,pof04,pof11,pof111,pof112,pof16,pof18,pof19,pof20,pofudate,pofbconf";

                    //再透過跟資料表pof16(入庫量)做比對 
                     //舊式
                    //string sDbCounnt = sarray[sarray.Length - 4];
                     //新式
                    string sDbCounnt = sarray[sarray.Length - 6]; 

                    //資料表pof02(項次)做比對 
                    //舊式                    
                     //string sPartNum = sarray[sarray.Length - 9];
                    //新式                    
                    string sPartNum = sarray[sarray.Length - 11];

                    iItemNum = int.Parse(sPartNum);

                    //物品包裝解析條碼字串
                    string sCutSrc = tbx_SourceText.Text.ToString();

                    //找出最後一筆欄位為入庫量
                    int iFindCount = sCutSrc.LastIndexOf("-");

                    //initial tmp
                    int itmp = 0, icheck = 0;

                    //直接做分割字串陣列
                    string[] sCut = sCutSrc.Split('-');
                    //string sFindNum = sCut[sCut.Length - 1]; //取最後一筆為物料Packet入庫量依據(舊式)
                    string sFindNum = sCut[sCut.Length - 2]; //取最後一筆為物料Packet入庫量依據(新式)


                    //物料包裝數以浮點數表示
                    int ivalue0 = int.Parse(sFindNum);
                    float fvalue0 = (float)ivalue0;

                    //測試累加變化是否正常
                    //iPuchTotalCount += ivalue0;
                    icheck = ivalue0;

                    for (int iCount = 0; iCount < g_lsCountSlect.Count(); iCount++)
                        itmp += g_lsCountSlect[iCount];

                    ivalue0 += itmp;

                    //這裡需要判斷累加的數量是否超過資料表pof16(入庫量)，先行判斷，2017.04.25，Brian                
                    //累加物料條碼量已超過資料庫額定入庫量
                    g_fbarcodeNum = (float)ivalue0;  //累積入庫量持續累加更新
                    g_fTableNum = float.Parse(sDbCounnt);  //資料庫額定入庫量

                    //持續新增ListBox Item
                    if (g_fbarcodeNum > g_fTableNum)
                    {
                        th1 = new Thread(new ThreadStart(ClearTextSourceBox));
                        th1.Start();

                        MessageBox.Show("已超過入庫額定總 " + g_fTableNum.ToString() + " 量!");
                        //將物料管理欄位清空，2017.04.21，Brian
                       // return;
                    }
                    else
                    {

                        //新增動態變數做儲存
                        g_lsCountGroup.Add(sFindNum);
                        g_lsCountSlect.Add(icheck);
                        g_AddItemNum.Add(fvalue0);
                        //cbx_ItemNum.Items.Add(g_lsCountGroup[iadd]);
                        
                        //紀錄生產日期時間，2017.05.22,Brian
                        g_sProductDate.Add(sDate);

                       // string sListCharAll = g_GroupItem[itemp] + "<- Group 表單項次(" + sPartNum + ") ,掃描物料包裝量(" + sFindNum + " pcs ) 增加扣帳項目";
                        string sListCharAll = "表單項次(" + sPartNum + ") ,掃描物料包裝量(" + sFindNum + " pcs ) 增加扣帳項目";

                        //iPartNO
                        lbx_ItemNum.Items.Add(sListCharAll);

                        //取1/5長度當百分比
                        iCentMaxValue += tbx_SourceText.Text.Length/5;

                        //將物料管理欄位清空，2017.04.21，Brian
                        th1 = new Thread(new ThreadStart(ClearTextSourceBox));
                        th1.Start();

                        //MessageBox.Show("-符號字串在第 " + iFindCount.ToString() + " 個" + "解析字串為:" + sFindNum + "資料庫數量:" + sDbCounnt);
                        // MessageBox.Show("-符號字串在第 " + iFindCount.ToString() + " 個" + "解析字串為:" + sFindNum + "資料庫數量:" + sDbCounnt);

                        if (g_fbarcodeNum == g_fTableNum)
                            MessageBox.Show("已符合入庫額定總 " + g_fTableNum.ToString() + " 量#");
                        else
                            MessageBox.Show(" 已加入OK !");

                        // if (tbx_SourceText.Text.ToString() != null)
                        // tbx_SourceText.Text = string.Empty; 
                        iadd++;

                        if (iadd > 0)
                        {
                            //iMulitCheck = 0;
                            itemp++;                                                 
                        }
                    }                                                    
                }
                else
                {
                    //MessageBox.Show("此物料已經被掃描過了，請掃取別組物料，謝謝!");
                    th1 = new Thread(new ThreadStart(ClearTextSourceBox));
                    th1.Start();

                    MessageBox.Show("此物料不適用此採購入庫單號需求，請掃取別組物料，謝謝!");
                    return;                
                }                                                                                       
            }
                                    
        }

        /*
        private void tbx_SourceText_KeyUp(object sender, KeyEventArgs e)
        {
             //btn_addItem.PerformClick();
            if (e.KeyCode == Keys.Enter)
            {
                g_iEnterPress++;

                if (g_iEnterPress == 1)
                {
                   BtnTestClick();                
                }
                else
                {
                    MessageBox.Show("請等SCAN條碼欄位清空在做掃描，TKS! ");
                    g_iEnterPress = 2;
                }
            
            }

            tbx_SourceText.ReadOnly = false;

            if (tbx_SourceText.Text.ToString() == "")
                g_iEnterPress = 0;

        }
        */
              
        private void btn_ClearItem_Click(object sender, EventArgs e)
        {

            #region  ComboBox
            /*
            //需求1:移除所有項目
            cbx_ItemNum.Items.Clear();

            for (int iItem = 0; iItem < cbx_ItemNum.Items.Count; iItem++)
            {
                //移除所有cbx_ItemNum 
                cbx_ItemNum.Items.Remove(cbx_ItemNum.Items[iItem]);
            }                        
            //需求2:僅移除選取項目(單選)            
            int x = this.cbx_ItemNum.SelectedIndex;
               
            if(x < 0)
            {
                MessageBox.Show("請選擇要刪除的項目，TKS!");
                return;
            }
            else
            {
                cbx_ItemNum.Items.RemoveAt(x);
                MessageBox.Show("已刪除項目:" + x.ToString());
            }
             */ 
            #endregion

            #region  ListBox

             //iListSelect = lbx_ItemNum;
            //lbx_ItemNum.Items.RemoveAt(iListSelect);
            //int[] iId = new int[iSelectCount];
            string ss="";

            ListBox.SelectedIndexCollection sic = lbx_ItemNum.SelectedIndices;//得到選择的Item的數量

            if (sic.Count == 0) 
                return;
            else
            {
                List<int> list = new List<int>();
                for (int i = 0; i < sic.Count; i++) 
                {
                    list.Add(sic[i]);                     
                } 
                
                list.Sort();//對list進行排序（從下到大的排序） 
               

                /*
                for (int i = 0; i < list.Count; i++)
                    ss += list[i]+",";
                 */
                //g_lsCountSlect.Sort(); //對g_lsCountSlect進行排序（從下到大的排序） 
                
                
               // for (int i = 0; i < list.Count; i++)
                
                for (int i = 0; i < list.Count; i++)                
                {
                    g_lsCountSlect.RemoveAt(list[list.Count-i-1]);

                    //移除 ActualStrogeNumber的刪除指定數量
                    g_AddItemNum.RemoveAt(list[list.Count-i-1]);

                    g_sProductDate.RemoveAt(list[list.Count-i-1]);

                }
                
                
                //g_lsCountSlect.Sort(); //對g_lsCountSlect進行排序（從下到大的排序） 
                
                while (list.Count >= 1)
                {
                    //在做刪除的動作也必須同時將動態陣列的位元刪除，否則會有字串刪除長度問題存在，2017.04.25,Brian
                    lbx_ItemNum.Items.RemoveAt(list[list.Count - 1]);
                    list.RemoveAt(list.Count - 1);
                    //g_lsCountSlect.RemoveAt(list[list.Count - 1]); 
                }
            }

            //g_lsCountSlect.Sort();

            int k = 0;

            while (k < g_lsCountSlect.Count)
            {
                ss += g_lsCountSlect[k].ToString() + ",";                 
                k++;
            }

          //  MessageBox.Show("g_lsCountSlect內部的ITEMS為:" + ss);

            MessageBox.Show("@已刪除選取的的ITEMS完畢@");

            
            #endregion
        }

        //SQL Table Column Name Class of all Item
        class TableInfo
        {
            public string SingalListNum { get; set; }
            public string Item { get; set; }
            public string Vender { get; set; }
            public string PartNo { get; set; }
            public string ChObjName { get; set; }
            public string Spec { get; set; }
            public string TotalAmontStorage { get; set; }
            public string PurchOrderNo { get; set; }
            public string CheckConfStatus { get; set; }
            public string ProductDate { get; set; }
            //new add 來源單號和來源項次，2017.06.09，Brian
            public string SourceSingle { get; set; }
            public string SourceItem { get; set; }

        }

        private void cbx_ItemNum_SelectedIndexChanged(object sender, EventArgs e)
        {




        }

        private void lbx_ItemNum_SelectedIndexChanged(object sender, EventArgs e)
        {                       
            /*
            string sSelectItem = lbx_ItemNum.Text.ToString();

            iListSelect = lbx_ItemNum.FindString(sSelectItem);
             */

            //總選取數量
           // iSelectCount = lbx_ItemNum.SelectedItems.Count;

            g_Listcount = new List<int>();

            //將選取數量的ID 存入 動態暫列g_Listcount 做後續刪除參考，2017.04.25，Brian
            foreach (string select in lbx_ItemNum.SelectedItems)
            {
                int ichk = lbx_ItemNum.FindString(select);
                g_Listcount.Add(ichk);
            }       
    
        }

        //更新最後處理總消耗時間,2017.05.05，Brian
        private void TotalCostTimeRecord(int ItemID,string sTotalTime)
        {
            string sresult = string.Empty;
            string sSingleList = tbx_WareSingScan.Text.ToString();
            string strtarpath = string.Format("server={0};database={1};uid={2};pwd={3};Connect Timeout = 180", Server, Database, dbuid, dbpwd);
          
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

                //+ "'" + sConfigFlag[0] + "'" + " WHERE pof01 = " + "'" + sSingleList + "'" + " AND pof02 = " + "'" + sItemNum + "'" + " AND pof20 = " + "'" + sPuch + "'"; 
                //"UPDATE PurchasingWarehousingSummary SET SearchTime = "
                //isc.CommandText = SqlUpdate + "'" + sTotalTime + "'" + " WHERE ChangeTransNumer = " + "'" + sSingleList + "'" + " AND SourceLineItem = " + "'" + ItemID.ToString() + "'";
                //iItemNum  來源項次   
                
                //採購入庫
                if (bPuchWareCheck)
                {
                   isc.CommandText = SqlUpdate + "'" + sTotalTime + "'" + " WHERE FileID = " + "'" + ItemID + "'" + " AND ChangeTransNumer = " + "'" + sSingleList + "'" + " AND SourceLineItem = " + "'" + iItemNum + "'";                                                
                }
                else //工單入庫
                {
                    isc.CommandText = SqlUpdate + "'" + sTotalTime + "'" + " WHERE FileID = " + "'" + ItemID + "'" + " AND ChangeTransNumer = " + "'" + sSingleList + "'" + " AND ChangeTransLine = " + "'" + iItemNum + "'";                                                    
                }


                isc.CommandTimeout = 3000;


                //step(3) streamreader Enable!
                //SqlDataReader:從數據庫獲取行
                 int iack = isc.ExecuteNonQuery();


                 if (iack == 1)
                 {
                    
                 }

                 //Step 6：close SyncObject 與 資料配接器物件來釋放物件所佔用的資源 mean Free not use memory session                                
                 icn.Close();
                 isc.Dispose();                
                 
            }
            catch (Exception k)
            {
                MessageBox.Show("Error link Read!~~");
                throw k;
            }           
        
        
        
        }


        private string UpdatePublicTable()
        {
            string strConnection = "Dsn=ERP-032;uid=dragon;database=erp032;host=10.1.1.2;srvr=c02;serv=50151;pro=onsoctcp;cloc=zh_tw.big5;dloc=zh_tw.big5;vmb=0;curb=0;scur=0;icur=0;oac=1;optofc=0;rkc=0;odtyp=0;fbs=4096;ddfp=0;dnl=0";
            string sFinal = string.Empty;
            
            string sSingleList = tbx_WareSingScan.Text.ToString();

            //pof00,pof01,pof02,pof04,pof11,pof111,pof112,pof16,pof18,pof19,pof20,pofudate,pofbconf 

            //pof18 to pof20( 1代表有值,0代表空值)，2017.06.09，Brian
            //採購入庫( 1,1 ,1)
            //工單入庫( 1,0 ,0)

            //取得生產日期內容
            int iprodate = dgv_SqlTableData.ColumnCount -2;
            string sbuildDate = dgv_SqlTableData.Rows[g_SelectRow].Cells[iprodate].Value.ToString();

            //取得採購單號(含日期)，可做後續追蹤
            int iPuch = dgv_SqlTableData.ColumnCount - 3;
            string sPuch = dgv_SqlTableData.Rows[g_SelectRow].Cells[iPuch].Value.ToString();

            //取得來源單號 pof18
            int iOriSingle = dgv_SqlTableData.ColumnCount - 5;
            string sOriSingle = dgv_SqlTableData.Rows[g_SelectRow].Cells[iOriSingle].Value.ToString();

            //取得項次 pof02
            int iItemNum = dgv_SqlTableData.ColumnCount - 11;
            string sItemNum = dgv_SqlTableData.Rows[g_SelectRow].Cells[iItemNum].Value.ToString();

            //判斷工單或是採購入庫
            //bPuchWareCheck

            string[] sConfigFlag = { "Y", "N" };

            OdbcConnection odbc = new OdbcConnection();
            odbc.ConnectionString = strConnection;

            //stop current 
            if (odbc.State == ConnectionState.Open)
                odbc.Close();

            //open start!
            odbc.Open();

            try
            {

                OdbcCommand odbcmd = new OdbcCommand();
                odbcmd.Connection = odbc;
                //更新採購入庫專用CMD，2017.04.26,Brian
                //測試正常 OK!
                //odbcmd.CommandText = sOdbcUpdateConf + "'" + sConfigFlag[0] + "'" + " WHERE pof01 = " + "'" + sSingleList + "'" + " AND pof02 = " + "'" + iPartNO.ToString() + "'"; 

                //比對異動單號，項次，來源單號
                odbcmd.CommandText = sOdbcUpdateConf + "'" + sConfigFlag[0] + "'" + " WHERE pof01 = " + "'" + sSingleList + "'" + " AND pof02 = " + "'" + sItemNum + "'" + " AND pof18 = " + "'" + sOriSingle + "'"; 

                //NG!
//              odbcmd.CommandText = sOdbcUpdateConf + "'" + sConfigFlag[0] + "'" + " WHERE pof01 = " + "'" + sSingleList + "'" + " AND pof20 = " + "'" + sPuch + "'";  
                                
                int iNUS  = odbcmd.ExecuteNonQuery();

                if(iNUS == 1)
                {
                    //MessageBox.Show("Pofconf 更新Y 正常! ^^");
                    sFinal = "Y";  
                }
                
                odbcmd.Dispose();
                odbc.Close();
                odbc.Dispose();
            }
            catch (Exception k)
            {
                MessageBox.Show("Error No SingleList Exist!");
                throw k;
            }

            return sFinal;
        }


        private void createNode( string sWriteTemp ,XmlWriter writer, List<DataInfo> Patient_List)
        {
            string sTab1, sTab2, sTab3, sTab4;
            string sTabNew1, sTabNew2, sTabNew3, sTabNew4;  
            //寫入XML 做存查
            // sWriteXMLALL = sSingleList + "," + sSerial + "," + sBindLotNumber + "," + sStroagelist;
            string[] sXMLFiled = sWriteTemp.Split(',');
       

            if (bcheckall)
            {
                writer.WriteStartElement("MMS-DataInfo");

                //1.先將舊的先寫入
                for (int k = 0; k < Patient_List.Count(); k++)
                {
                    DataInfo DI = Patient_List[k];
                   
                    sTab1 = DI.ChangeTransNumer.ToString();
                    sTab2 = DI.BindingNumber.ToString();;
                    sTab3 = DI.BindingLotNumber.ToString();
                    sTab4 = DI.ActualStrogeNumber.ToString();

                    //MMS-DataInfo

                    //tabLabel                 
                    writer.WriteStartElement("tabLabel");

                    writer.WriteStartElement("tab_ChangeTransNumer");
                    writer.WriteString(sTab1);
                    writer.WriteEndElement();
                    writer.WriteStartElement("tab_BindingNumber");
                    writer.WriteString(sTab2);
                    writer.WriteEndElement();
                    writer.WriteStartElement("tab_BindingLotNumber");
                    writer.WriteString(sTab3);
                    writer.WriteEndElement();
                    writer.WriteStartElement("tab_ActualStrogeNumber");
                    writer.WriteString(sTab4);
                    writer.WriteEndElement();
                    writer.WriteEndElement();                                                                                     

                }

                //2.再將新的寫入,將各項欄位帶入  
                sTabNew1 = sXMLFiled[0];
                sTabNew2 = sXMLFiled[1];
                sTabNew3 = sXMLFiled[2];
                sTabNew4 = sXMLFiled[3];
            
 
                //tabLabel                 
                //tabLabel                 
                writer.WriteStartElement("tabLabel");

                /*新增ID(依照產生次數)*/
                writer.WriteStartElement("tab_ChangeTransNumer");
                writer.WriteString(sTabNew1);
                writer.WriteEndElement();

                writer.WriteStartElement("tab_BindingNumber");
                writer.WriteString(sTabNew2);
                writer.WriteEndElement();
                writer.WriteStartElement("tab_BindingLotNumber");
                writer.WriteString(sTabNew3);
                writer.WriteEndElement();
                writer.WriteStartElement("tab_ActualStrogeNumber");
                writer.WriteString(sTabNew4);
                writer.WriteEndElement();
                writer.WriteEndElement();

                writer.WriteEndElement();               
                writer.Flush();
                writer.Close();

                //MessageBox.Show("XML File AGAIN### ");
            }
            else
            {
               
                //2.再將新的寫入,將各項欄位帶入                          
                writer.WriteStartElement("tabLabel");

                /*新增ID(依照產生次數)*/
                writer.WriteStartElement("tab_ChangeTransNumer");
                writer.WriteString(sXMLFiled[0]);
                writer.WriteEndElement();

                writer.WriteStartElement("tab_BindingNumber");
                writer.WriteString(sXMLFiled[1]);
                writer.WriteEndElement();
                writer.WriteStartElement("tab_BindingLotNumber");
                writer.WriteString(sXMLFiled[2]);
                writer.WriteEndElement();
                writer.WriteStartElement("tab_ActualStrogeNumber");
                writer.WriteString(sXMLFiled[3]);
                writer.WriteEndElement();
                writer.WriteEndElement();

                writer.WriteEndElement();
                writer.WriteEndDocument();
                writer.Close();

                XmlDataDocument xmldoc = new XmlDataDocument();

                xmldoc.Save(writer);

                //MessageBox.Show("XML File Create first@@ ");

            }
        }


        //寫入XML格式日後作追蹤，2017.05.08，Brian
        private void WriteXMLRecord(string sXmlContent)
        {

            DirectoryInfo dti_LogInfo = null;
            FileInfo fi_logflinfo;
            string sWritePath = string.Empty, strCarryon =string.Empty;


            //sWritePath = sXMLfilePath + +"\\MMS-Data.xml";
            sWritePath = sXMLfilePath + "\\MMS-Data-"+System.DateTime.Today.ToString("MM-dd-yyyy") +".xml";

            //string sWritePath = sXMLfilePath; 
            //+ System.DateTime.Today.ToString("MM-dd-yyyy") + "\\MMS-Data.xml";
            fi_logflinfo = new FileInfo(sWritePath);
            dti_LogInfo = new DirectoryInfo(fi_logflinfo.DirectoryName);


            if (!dti_LogInfo.Exists)
                dti_LogInfo.Create();

            DAList = new List<DataInfo>();

            if (!File.Exists(sWritePath))
            {
                bcheckall = false;

                XmlTextWriter writer = new XmlTextWriter(sWritePath, System.Text.Encoding.UTF8);
                //writer = new XmlTextWriter(sWriteXMLPath, System.Text.Encoding.UTF8);

                writer.WriteStartDocument(true);
                writer.Formatting = Formatting.Indented;
                writer.Indentation = 2;
                writer.WriteStartElement("MMS-DataInfo");

                //建立表單格式，2017.05.08，Brian
                createNode(sXmlContent, writer, DAList);

            
            }
            else
            {
                bcheckall = true;

                XmlDocument xmlDoc = new XmlDocument();

                xmlDoc.Load(sWritePath);

                //讀取NodeList各節點項目
                XmlNodeList nodeList = xmlDoc.DocumentElement.SelectNodes("/MMS-DataInfo/tabLabel");

                //代表READ到最後一個節點
                foreach (XmlNode node in nodeList)
                {
                    DataInfo di = new DataInfo();

                    //1.ChangeTransNumer
                    di.ChangeTransNumer = node.SelectSingleNode("tab_ChangeTransNumer").InnerText;

                    //2.BindingNumber
                    string prosearch = node.SelectSingleNode("tab_BindingNumber").InnerText;
                    di.BindingNumber = prosearch;

                    //3.BindingLotNumber
                    string prodetail = node.SelectSingleNode("tab_BindingLotNumber").InnerText;
                    di.BindingLotNumber  = prodetail;

                    //4.ActualStrogeNumber
                    string proreflash = node.SelectSingleNode("tab_ActualStrogeNumber").InnerText;
                    di.ActualStrogeNumber  = proreflash;

                    //全部儲存在List
                    DAList.Add(di);

                }



                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.IndentChars = "\n";
                settings.NewLineOnAttributes = true;

                strCarryon = sWritePath.ToString();

                XmlWriter xmlWriter = XmlWriter.Create(strCarryon, settings);

                createNode(sXmlContent, xmlWriter, DAList);
            
            
            }
        }

        private void CheckSQLStatus(string[] sColumn, string TextField)
        {
            string strtarpath = string.Format("server={0};database={1};uid={2};pwd={3};Connect Timeout = 180", Server, Database, dbuid, dbpwd);

            //string strtarpath = "Data Source=DRAGONSERVER;Initial Catalog=MMS;Persist Security Info=True;User ID=Brian-SQl";
            string sCheck1 = string.Empty;
            string s_param2 = string.Empty;


            string[] s_field = TextField.Split(',');

            for (int i = 0; i < s_field.Length; i++)
            {
                //s_field[i] = "@" + s_field[i].Trim();
                sCheck1 = IsChina(s_field[i]);

                // s_field[i] = "'" + s_field[i].Trim() + "'";

                s_field[i] = "'" + sCheck1.Trim() + "'";

            }

            s_param2 = string.Join(",", s_field);



            // string stmp = IsChina(TextField[icout]);                                                 
            // s_param2 += "'"+stmp + "'";     

            //參數集合
            //s_param2 = s_param2.Substring(0, s_param2.Length - 1);


            //s_param2 = s_param2.Trim();

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


                /*
                 ex: INSERT INTO MyTable (PriKey, Description) VALUES (123, 'A description of part 123.');
                */

                //isc.CommandText = SqlInsertTry + " (" + sSqlColumn + ") VALUES ('Export');"; 
                 isc.CommandText = SqlInsertTry + " (" + sSqlColumn + ") VALUES(" + s_param2 + ");";
                 isc.CommandTimeout = 3000;
                 isc.CommandType = CommandType.Text;

                /*
                SqlParameter  prm1 = new SqlParameter("@value1",SqlDbType.NVarChar,50);
                prm1.Direction = ParameterDirection.Input;
                isc.Parameters.Add(prm1);
                prm1.Value = (!string.IsNullOrEmpty(s_param2)) ? s_param2.ToString() : "";   
                */

                //int icheck = isc.ExecuteNonQuery();
                
                //if (icheck == 1)
                isc.ExecuteNonQuery();

                
                //Step 6：close SyncObject 與 資料配接器物件來釋放物件所佔用的資源 mean Free not use memory session
                //isc.Close();

                isc.Dispose();
                icn.Close();
                icn.Dispose();
                //return sCheck;

                //MessageBox.Show("執行Insert 成功!");

            }
            catch (Exception k)
            {
                MessageBox.Show("Error link Read!~~");
                throw k;

            }

        }


        public string IsChina(string stmp)
        {
            //bool BoolValue = false;
            char[] cnull = { ' ' };
            char[] cnull2 = { '上', '午' };

            char[] check = new char[stmp.Length];
            int time = 0;

            for (int i = 0; i < stmp.Length; i++)
            {
                //代表非中文字
                if (Convert.ToInt32(Convert.ToChar(stmp.Substring(i, 1))) < Convert.ToInt32(Convert.ToChar(128)))
                {
                    check[i] = stmp[i];
                }
                else //代表中文字
                {

                    time++;

                    /*           
                   if(time%2==0)
                       check[i] = 'M';
                   else
                       check[i] = 'A';                     
                   */

                    if (check[i] != cnull[0])
                    {
                        if (time % 2 == 0)
                            check[i] = 'M';
                        else
                            check[i] = 'A';
                    }
                    else
                        check[i] = cnull[0];
                }
            }

            string sResult = new string(check);

            return sResult;

        }

        //新增消耗工作時間函數 2017.03.17 Brian
        private void DetecTimerTotalCost(int itimes)
        {
            string sCosttime ="cost total : ";

            float ftotalcost = 0, fminsec =0;

            double dbcost1, dbtotalcost = 0;

            int icatch,isec, imin, ihour, iMaxItemID, iminisec,ifloat;  //計算各項時間數據

            icatch = ifloat = isec = imin = ihour = iMaxItemID = iminisec = 0;

            //找出最大項次ID值
            iMaxItemID = DB_FileIdFindMax();

            
            //超過1分鐘          
            if (itimes >= 60000000)
            {
                //此資料量過大，無法負荷，請重整資料量再insert資料        
                MessageBox.Show("此資料量過大，無法負荷，請重整資料量再insert資料");
            }
            else //1分鐘以內
            {
                /*
                ihour = itimes / 3600;
                imin = (itimes % 3600) / 60;
                isec = (itimes % 3600) % 60;
                 */
                //記錄消耗秒與毫秒，2014.05.05，Brian
                imin = itimes / 1000;

                ifloat = itimes % 1000;

                dbcost1 = (double) ifloat *0.001;

                
                //dbcost1 = isec + dbtotalcost;

                dbtotalcost = (double)imin + dbcost1;
                                             
                //將總total 消耗時間記錄在最後一筆，2017.05.05，Brian
               // sCosttime += Convert.ToString(isec) + "秒 " + Convert.ToString(iminisec) + "毫秒";

                sCosttime += Convert.ToString(dbtotalcost) + "秒 ";

                TotalCostTimeRecord(iMaxItemID, sCosttime);
                
            }
        }


        private void InsertMMSTable()
        {
            string strCon = "Dsn=ERP-032;uid=dragon;database=erp032;host=10.1.1.2;srvr=c02;serv=50151;pro=onsoctcp;cloc=zh_tw.big5;dloc=zh_tw.big5;vmb=0;curb=0;scur=0;icur=0;oac=1;optofc=0;rkc=0;odtyp=0;fbs=4096;ddfp=0;dnl=0";
            string sSingleList = tbx_WareSingScan.Text.ToString();
            string sFinalInsert = string.Empty;
            string sSerial =string.Empty;
            string sStroagelist =string.Empty;
            string sWriteXMLALL = string.Empty;

            //pgb_Status.Maximum = iCentMaxValue;

            //取得項次 pof02
            int iItemNum = dgv_SqlTableData.ColumnCount - 11;

            //int iSearchtime = 0;
            string sItemNum = dgv_SqlTableData.Rows[g_SelectRow].Cells[iItemNum].Value.ToString();
                                          
            string sAllOdbcName = string.Empty;
            string sAlltmp = string.Empty;

            //取SQL DB MaxID ，2017.05.02,Brian
            int iMaxID = DB_FileIdFindMaxLog();
            int iLableCount = lbx_ItemNum.Items.Count;

            pgb_Status.Maximum = iLableCount;

            DateTime dtn = DateTime.Now;

            OdbcConnection odbc = new OdbcConnection();
            odbc.ConnectionString = strCon;

            //stop current 
            if (odbc.State == ConnectionState.Open)
                odbc.Close();

            //open start!
            odbc.Open();

            try
            {
                //step(2):
                OdbcCommand odbcmd = new OdbcCommand();
                odbcmd.Connection = odbc;
                odbcmd.CommandText = InforMixCopyToSql + " WHERE pof01 = " + "'" + sSingleList + "'" + " AND pof02 = " + "'" + sItemNum + "'";
                odbcmd.CommandTimeout = 2000; //2 sec time out !

                //step(3) streamreader Enable!
                //SqlDataReader:從數據庫獲取行
                OdbcDataReader odbRead = odbcmd.ExecuteReader();
                // SqlDataReader sqlDr = isc.ExecuteReader();  // sqlComad.ExecuteReader();
                //FieldCount  取得目前資料列中的資料行數目。

                DataTable dt = new DataTable();

                int iDrCount = odbRead.FieldCount; //sqlDr.FieldCount;

                //find colums 
                for (int iCnt = 0; iCnt < iDrCount; iCnt++)
                {
                    string a = odbRead.GetName(iCnt);

                    sAllOdbcName += a + ",";
                    //dt.Columns.Add(a);
                    //dt.Rows.Add((DataRow)sqlDr[iCnt]);
                }

                //將DataReader前進到下一個資料
                while (odbRead.Read())
                {
                    //定義一個數組，便於讀出每一行資料
                    String[] subitems = new String[iDrCount];
                    //用循環讀出每一行資料
                    for (int iCnt = 0; iCnt < iDrCount; iCnt++)
                    {
                        //讀出每一行資料保存到數組中
                        subitems[iCnt] = odbRead[iCnt].ToString();
                        string sValue = odbRead[iCnt].ToString();

                        sAlltmp += subitems[iCnt] + ",";
                    }
                    //將讀出的行資料增表的行中
                    //dt.Rows.Add(subitems);
                }

               
                //if (odbRead.Read())
                odbRead.Close();
                odbcmd.Dispose();
                odbc.Close();
                odbc.Dispose();



                //再跳到SQL作複製後讀取查閱，2017.04.27，Brian
                sAllOdbcName = sAllOdbcName.Substring(0, sAllOdbcName.Length - 1);
                
                sAlltmp = sAlltmp.Substring(0, sAlltmp.Length - 1);

                /*
                //工單入庫要做空值調整，2017.06.06，Brian
                if(bPuchWareCheck == false)
                {
                    string[] sSingleCheck = sAlltmp.Split(',');

                    for (int k = 0; k < sSingleCheck.Length; k++)
                    {
                        if (k == 4 || k == 5)
                        {
                            sSingleCheck[k] = "null";

                        }
                        else
                            sAlltmp += sSingleCheck[k] + ",";
                    }
                }
                */


                //轉成字串陣列
                string[] sArray1 = sAllOdbcName.Split(',');

                //GUID取前五碼
                string sGuid = System.Guid.NewGuid().ToString("N");
                sGuid = sGuid.Substring(0, 4);

                string sClass = dgv_SqlTableData.Rows[g_SelectRow].Cells[0].Value.ToString();
                sClass = sClass.Trim();

                //以下為Insert SQL DB 指定Table，依照ScanTimess,2017.05.02,Brian
                for (int iInset = 1; iInset <= iLableCount; iInset++)
                {

                    lbl_WaitRun.Text = "進行中...";
                    Application.DoEvents();
                    
                    //FileID,ScanCount,BindingNum(日期+iInset),BindingLotNum,checkAorM
                    string sRunId = iMaxID.ToString();                                        
                    string sScanCount = iLableCount.ToString();
                    string sBindNumber = dtn.ToString() + "-SerialNo-" + sRunId;
                                        
                    string sBindLotNumber = dtn.ToString() + "-Lot-" + sGuid;

                    string sStrogeNumber = g_AddItemNum[iInset - 1].ToString();

                    string sProductMakeDate = g_sProductDate[iInset - 1].ToString();

                    string sFloatNumber = sStrogeNumber;

                    string sCost = "cost...";

                    

                    //測試用,OK
                    sID = tbx_IDCheck.Text ;
                    sDepart = tbx_DepartCheck.Text;

                    //sFinalInsert = sRunId + "," + sSingleList + "," + sAlltmp + "," + sID + "," + sDepart + "," + sScanCount + "," + sBindNumber + "," + sCost + "," + sBindLotNumber;
                    //測試OK!
                    sFinalInsert = sRunId + "," + sClass + "," + sSingleList + "," + sAlltmp + "," + sID + "," + sDepart + "," + sScanCount + "," + sBindNumber + "," + sCost + "," + sBindLotNumber + ",";


                    //後續判斷自動或手動                   
                   // sFinalInsert += (g_bAuto == true) ? AutoOrMannul[0] + "," + sStrogeNumber : AutoOrMannul[1] + "," + sStrogeNumber;

                    //後續判斷自動或手動                   
                    sFinalInsert += (g_bAuto == true) ? AutoOrMannul[0] + "," + sStrogeNumber + "," + sFloatNumber : AutoOrMannul[1] + "," + sStrogeNumber + "," + sFloatNumber;

                    sFinalInsert += "," + sProductMakeDate + "," + sIvalidFlag[1];
                    
                    //累加寫入XML字串，2017.05.08，Brian
                    sSerial += sBindNumber +"*";

                    sStroagelist += sStrogeNumber+"*";

                                        
                    CheckSQLStatus(sArray1, sFinalInsert);

                    //持續增加，依照Insert 次數
                    iMaxID++;

                    
                    //if (iInset == pgb_Status.Maximum - 1)
                    if (iInset == pgb_Status.Maximum )
                    {
                        lbl_WaitRun.Text = "完成!";
                        pgb_Status.Value = pgb_Status.Maximum;
                    }
                      

                    if (iInset == iLableCount)
                    {
                        /*
                        ThreadStart th2 = new ThreadStart(StopTimer);
                        Thread tok = new Thread(th2);
                        tok.Start();
                         */

                        sw.Stop();

                        sSerial = sSerial.Substring(0,sSerial.Length-1);
                        sStroagelist = sStroagelist.Substring(0,sStroagelist.Length-1);

                        
                        string sminisecond = sw.Elapsed.TotalMilliseconds.ToString();

                        dbCount = Convert.ToDouble(sminisecond);

                        iSearchtime = Convert.ToInt32(dbCount);   
                        
                        //偵測時間
                        DetecTimerTotalCost(iSearchtime);
                                               
                        //寫入XML 做存查
                        sWriteXMLALL = sSingleList+","+sSerial+","+sBindLotNumber+","+sStroagelist;
                        WriteXMLRecord(sWriteXMLALL);

                        iCaculusSecond = 0;

                        MessageBox.Show("執行更新公用表 && Insert物料管理資料表 成功!");


                        while (pgb_Status.Value != 0)
                        {
                            pgb_Status.Value -=1;                        
                        }

                        lbl_WaitRun.Text = "等待中";
                         
                    }
                }
                                                
            }
            catch (Exception k)
            {
                MessageBox.Show("Error link Read!~~");
                throw k;
            }
        
        }
        
        private bool CheckPofCONG()
        {
            /*
            for (int ival = 0; ival < g_ConfigYN.Count;ival++ )
            {
                if (ival == g_SelectRow && g_ConfigYN[ival]=="Y")
                    return true;
            }
             */

            int iconfig = dgv_SqlTableData.ColumnCount - 1;

            if (dgv_SqlTableData.Rows[g_SelectRow].Cells[iconfig].Value.ToString()=="Y")
                return true;
                                
            return false;
        }

        //確認後將目前入庫量做公用表 ->Update 和物料管理資料表做->Insert ，2017.04.26，Brian
        private void btn_AutoConfirm_Click(object sender, EventArgs e)
        {
            int iVal = 0;
            bool bcheck = false;

            pgb_Status.Value = 0;

            //取得之掃描異動單據之單號pofconf是否為Y，若符合就確認之前已入庫過，否則才繼續執行以下，2017.04.28，Brian
            bcheck = CheckPofCONG();

            if (bcheck)
            {
                MessageBox.Show("此項次物料已被入庫 ,pofbconf = Y ,TKS@!");
                return;
            }


            //listBox無任何紀錄產生
            if (lbx_ItemNum.Items.Count < 1 || g_lsCountSlect.Count < 1)
            {
                MessageBox.Show("無任何物料紀錄入庫量產生，請做掃描紀錄利於扣帳!");
                return;
            }

            for (int iCount = 0; iCount < g_lsCountSlect.Count(); iCount++)
                iVal += g_lsCountSlect[iCount];

            //比對一致OK!
            if ((float)iVal == g_fTableNum)
            {
                btn_AutoConfirm.Enabled = false;

                sw.Reset();//碼表歸零
                sw.Start();//碼表開始計時
     
                //MessageBox.Show("目前數量一致可做入庫!TKS!~^.^");            
                  string sconf = UpdatePublicTable();

                  if (sconf.StartsWith("Y"))
                  {
                      //search cost total time
                      //tm_Caculus.Enabled = true;
                      //tm_Caculus.Interval = 100; //以0.1秒 timrer base 2017.03.16 Brian

                      //MessageBox.Show("Pofconf 更新Y 正常! <-this One");
                      InsertMMSTable();

                      //確定無誤就將當前Row移除
                      DataGridViewRow dt1 = dgv_SqlTableData.Rows[g_SelectRow];
                      dgv_SqlTableData.Rows.Remove(dt1);

                      btn_AutoConfirm.Enabled = true;
                     
                  }
                  else
                  {
                      MessageBox.Show("更新公用表 Conf 失效，維持N !");                                                                                                              
                  }
            }
            else //比對NG...            
            {               
               MessageBox.Show("目前數量尚未到達異動單入庫量，請繼續掃描該項次類別條碼!!!");                                
            }
        }

        private void tm_Caculus_Tick(object sender, EventArgs e)
        {
            iCaculusSecond++;
         
            //dbCount = iCaculusSecond / 10.0;
            lbl_TestTime.Text = (iCaculusSecond/10.0) + "";  
        }

        class DataInfo
        {
            public string ChangeTransNumer { get; set; }
            public string BindingNumber { get; set; }
            public string BindingLotNumber { get; set; }
            public string ActualStrogeNumber { get; set; }
        }

        private void tbx_SourceText_KeyPress(object sender, KeyPressEventArgs e)
        {

            char Key_Char = e.KeyChar;//判斷按鍵的 Keychar 

            //掃入條碼並確認狀況(--Enter鍵按下)，2017.06.01，Brian        
            if (Key_Char == 13)
            {
                g_iEnterPress++;

                if (g_iEnterPress == 1)
                {
                    BtnTestClick();     
                    //g_iEnterPress = 0;
                }
                else
                {                                
                    MessageBox.Show("請等SCAN條碼欄位清空在做掃描，TKS! ");
                    g_iEnterPress = 2;
                }
                //MessageBox.Show("Enter 鍵啟動 !");
            }

            tbx_SourceText.ReadOnly = false;

            if (tbx_SourceText.Text.ToString() == "")
                g_iEnterPress = 0;
        }

        //將此異動單據記錄全刪除，2017.06.14，Brian
        private void btn_DelPuchExist_Click(object sender, EventArgs e)
        {
            //string sIvalid = "Y";
            
            string strtarpath = string.Format("server={0};database={1};uid={2};pwd={3};Connect Timeout = 180", Server, Database, dbuid, dbpwd);
            //string strtarpath = "Data Source=DRAGONSERVER;Initial Catalog=MMS;Persist Security Info=True;User ID=Brian-SQl";

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
                isc.CommandText = SqlUpdatePuchWareTable + "'"+sIvalidFlag[0]+ "'" + " WHERE FileID IN " + "(" + g_sIDCollect + ")";
                isc.CommandTimeout = 3000;

                 //step(3) streamreader Enable!                
                int  icheck = isc.ExecuteNonQuery();

                if (icheck != 0)
                {
                    rbn_AutoScan.Visible = true;
                    lbl_dgvSelectStatus.Visible= rbn_AutoScan.Checked = false;
                    btn_DelPuchExist.Visible = lbl_Invalid.Visible = false;
                                                                             
                    if (g_sLanguage == "en-US")
                    {
                        MessageBox.Show("This material is completed, please continue to do scan storage!");                                        
                    }
                    else if (g_sLanguage == "zh-TW")
                    {
                       MessageBox.Show("此項物料作廢完成，請繼續做掃描入庫!");                    
                    }
                    else if (g_sLanguage == "zh-CN")
                    {
                        MessageBox.Show("此项物料作废完成，请继续做扫描入库!");                    
                    }
                    
                }
                
                //close SyncObject 與 資料配接器物件來釋放物件所佔用的資源 mean Free not use memory session
                isc.Dispose();
                isc.Clone();
                icn.Close();
                icn.Dispose();
               
            }
            catch (Exception k)
            {
                MessageBox.Show("Error Run Update!~~");
                throw k;
            }                                  
        }
    }
}
