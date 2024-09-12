using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

//(+)2016.07.27 Brian ( New Add Lib )
using System.Data.SqlClient;
using Shell32;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Collections;
//加入2.0以後的交易,匯入System.Transactions.dll
using System.Transactions;
using GetFileInfo.Properties;  //(+)2016.07.29 Brian (for FileShowProperty)
using System.Xml.Linq;
using System.Configuration;
using System.Data.SqlServerCe;  //for 本地端主機使用資料庫 
using System.Data.OleDb;        //for Microsoft SQL 3.5
using System.Data.Odbc;
//using Microsoft.Office.Interop.Excel;   //會跟using System.Data ((>.<))產生衝突，所以使用以下方式
using Excel = Microsoft.Office.Interop.Excel; //匯入Export Excel.dll , 2016.08.31 , Brian 

namespace GetFileInfo
{
    public partial class FileCatch : Form
    {
        /**************************Declare Global Variable ***************************/
        //選取當前路徑和全路徑變數宣告
        string g_sPathFile ="" ,g_PathFailString = "";
        string g_sAllFile  ="";
        //全部RowFileInfo 收集
        string g_sFileProperty = "";
                      
        // Table Name,MDF,MDB 各項名稱
        String g_sTableName = "DataTable1";        //  Table資料表名稱: "DataTable1"        
        String g_sTableNameLog = "DataLog";        //  Table資料表名稱: "DataLog"

       // String g_sMDFName = "FileDataTable";   //  SQL SERVER COMPACT 3.5 資料庫Name
        String g_sMdbName = "FileData";        //  Access資料庫Name


        //新增 "[FileRoot]" 2016.08.16 ,By Brian
        String g_sTextAll = "[FileID],[FileName],[FileSize],[FileDate],[FileVersion],[FileRoot],[SaveDate]";

        //新增 "[SaveDate],[FileGroup]" 2016.08.23 ,By Brian
        String g_sText2All = "[FileID],[FileName],[FileSize],[FileDate],[FileVersion],[FileRoot],[SaveDate],[FileGroup]";

        String g_sInsertOne = "FileID,FileName,FileSize,FileDate,FileVersion,FileRoot,SaveDate";

        //新增 "value6" 2016.08.16 ,By Brian        
        String g_sValAll = "value1,value2,value3,value4,value5,value6,value7";
        //新增 g_sVal2All new add "value7,value8" ,2016.08.19,By Brian                
        String g_sVal2All = "value1,value2,value3,value4,value5,value6,value7,value8";
        
        //新增 Table Text Name 2016.08.16 ,By Brian
        String[] g_sTableMdbText = { "ID", "FileName", "Size", "SaveDate", "Version" ,"Root"};
 
        //動態陣列宣告
        List<string> g_lsData, g_lsSearchFile = new List<string>(), g_lsTotalFile = new List<string>(), g_lsSaveFile = new List<string>();

        //動態陣列宣告 for 資料群組 ，2016.08.23，By Brian
        List<string> g_lsDataGroup = new List<string>();

        //加入動態陣列 for selectIndexchange 的ID 值，2016.08.25，By Brian
        List<string> g_lsIdIndexSelect = new List<string>();

        //宣告靜態存取變數，之後有需要做參考(寫入Log)
        static string g_sTaskLog = null;

        //Table Columns Labeld && DataBase Table Name Array   //{ "序列:", "檔案名稱:", "大小(KB):", "存取日期:", "VER:" };
        String[] g_sExcelLabel = { "序列:", "檔案名稱:", "大小(KB):", "檔案日期:", "版本(VER):", "存檔路徑:", "存檔日期:", "群組:" };
      
       
        //新增群組Group 命名ID，2016.08.19，By Brian
        string[] g_sGroupName = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" }; 
                                 
        //系統時間 System.DateTime.Now,全域變數
        string  g_sInsertTime = "";

        //抓取 WinForm 應用程式所在的目錄，傳應用程式設定執行檔輸出目錄的路徑
        // String g_sProgramPath = System.Windows.Forms.Application.StartupPath;

        //抓取 Console 與WPF應用程式所在的目錄可使用的方式
         String g_sProgramPath = System.AppDomain.CurrentDomain.BaseDirectory;
         
        //mdb 存放路徑 與  log 存放路徑
        string g_sBaseMdbFile="",g_sLogBeginPath="";

        //Excel格式 存放路徑 ,2016.08.31,Brian
        string g_sExcelFileList = "";
        
        //字串收集ID
        string g_sIdSelect = "";

        //刪除Row字串 和 主索引鍵ID
        string g_sRowId = "", g_sFileTotal = "";
        //string g_sIndexKey = "ID";
        string g_sIndexKey = "FileID";
        
        //紀錄每個資訊紀錄參數
        int g_iOk = 0,g_iFile = 0 ,g_iOkAll = 0 ,g_iSelect = 0 ,g_sFileNum = 0 ,g_iTimes2 = 0;
        
        //Insert ID 遞增
        int iIdCount = 0, iIdCountAll = 0 , iNewInsert2 = 0, iGetChange = 0;

        //Insert 判斷 次數，2016.08.23，By Brian
        int g_iGetInsert = 0 ;

        //Find 資料庫最大ID
        int g_iIdMaxLog = 0, g_iIdMaxDataTable1 = 0;

        //Excel 工作項數量 ,2016.08.31 ,Brian
        int g_iExcelNum = 0, g_iExcelNum2 = 0;

        //動態字型大小宣告
        private FontStyle g_fsView;
        private FontFamily g_fmInfo;
        private Font g_ftType;

        bool g_bInsertOne = true, g_bDebug = true ,g_bDelRow = true;   //(+) 2016.07.29 Brian 增加insert判斷 
      
       // Excel.Application excel = new Excel.Application();
       /*******************************Declare End ***********************************/
        
        //----Begin Run ----
        public FileCatch()
        {
            InitializeComponent();
         // tbx_FileProperty.Multiline = true;            
            Control.CheckForIllegalCrossThreadCalls = false;  //不使用安全線程,反之則使用      
        }

        private void FileCatch_Load(object sender, EventArgs e)
        {
            //******************Declare List class prepare ******************
            ckx_AllPathFolder.Checked = true;   //default is check here! 
            ckx_SinglePathFolder.Checked = false;
         
            //取得WIN FORM 執行原始Base路徑 , 2016.08.11 ,By Brian
            g_sBaseMdbFile = g_sProgramPath + "MDBData\\";
            g_sLogBeginPath = g_sProgramPath + "Log\\";
            g_sExcelFileList = g_sProgramPath + "ExcelFile\\"; //  Excel->根目錄存放路徑 2016.08.31,Brian
                          
            //tbx_CvtXls.Text = g_sPlit.ToString();            
            tbx_SqlCmd.Text = g_sBaseMdbFile;
            g_sTaskLog = g_sLogBeginPath;

            //一開始進度百分比 為 = 0 ，2016.08.29，Brian
            pgb_CentValue.Value = 0;
            //************************* Declare List End************************
        }

        private void ConvertStringToList(string strlist)
        {           
           //split判斷字串
           //string[] strs = strlist.Split(new string[] { "@.@" }, StringSplitOptions.RemoveEmptyEntries);
            
            //因為有些檔案屬性(ex:版本)有逗號，會有切割問題，所以改成 '^'
            // var Patient_List = sCutstr.Split(';').Select(x => x.Split(',')).ToList();           
            var Patient_List = strlist.Split(';').Select(x => x.Split('^')).ToList();
            
            DataTable dtView = ConvertStringtoTable(Patient_List);

            dgv_dt1.DataSource = dtView;  //將dt1(DataTable) 內容 轉由 dgv (DatGridView)格式顯示                            
         }

         // Convert to DataTable.
         public DataTable ConvertStringtoTable(List<string[]> Patient_List)                 
         {                   
            DataTable dt = new DataTable();
            int iColumnNum = 0;
            int iListCount = 0;
           
            //catch Data attribute (序列、檔案名稱、版本、日期、大小) to DataTable
            dt.Columns.Add("序列", typeof(string));            
            dt.Columns.Add("檔案名稱", typeof(string));
            dt.Columns.Add("檔案大小(KB)", typeof(string));
            dt.Columns.Add("檔案日期", typeof(string));
            dt.Columns.Add("檔案版本", typeof(string));
            //新增路徑 2016.08.16 ，不顯示table
            dt.Columns.Add("檔案路徑", typeof(string));
            dt.Columns.Add("存檔日期", typeof(string));
            //新增檔案群組 2016.08.21 
            dt.Columns.Add("檔案群組", typeof(string));


            //總Column列數量
            iColumnNum = dt.Columns.Count;

           //begin Run !!!
            try
            {                
                foreach (var row in Patient_List)
                {
                    //因為 Patient_List物件經切割Select(x => x.Split('^')).ToList()有數量多一個問題，所以在此判總數量減一，2016.08.19，By Brian
                    if (iListCount < Patient_List.Count-1)
                       dt.Rows.Add(row);

                    iListCount++;
                }                                                      
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "GetFile 警告!!!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                throw ex;
            }            
          
            return dt;
        }

        // (Record Info All of File!),2016.07.27 Brian
        public static void WriteLog(string strLog)
        {
            StreamWriter sw_Log;
            FileStream fs_Flstr = null;
            DirectoryInfo dti_LogInfo = null;
            FileInfo fi_logflinfo;
            var sPath = (string)GetFileInfo.FileCatch.g_sTaskLog;

            //Record *.log File Bellow of Path!
            string s_LogPath = sPath + "LogData-";

            s_LogPath = s_LogPath + System.DateTime.Today.ToString("MM-dd-yyyy") + "." + "txt";
            fi_logflinfo = new FileInfo(s_LogPath);
            dti_LogInfo = new DirectoryInfo(fi_logflinfo.DirectoryName);

            if (!dti_LogInfo.Exists)
                dti_LogInfo.Create();

            if (!fi_logflinfo.Exists)
            {
                fs_Flstr = fi_logflinfo.Create();
            }
            else
            {
                fs_Flstr = new FileStream(s_LogPath, FileMode.Append);
            }
            sw_Log = new StreamWriter(fs_Flstr);
            sw_Log.WriteLine(strLog);
            sw_Log.Close();
        }

        public static DataTable DataGridViewUpdateDataTable(DataGridView dgv,int RowIndex ,string SaveDateCell)
        {
            DataTable dt = new DataTable();

            // Header columns  
            foreach (DataGridViewColumn column in dgv.Columns)
            {
                DataColumn dc = new DataColumn(column.HeaderText.ToString());

                dt.Columns.Add(dc);
            }

            // Data Rows cells  
            for (int i = 0; i < dgv.Rows.Count; i++)
            {                
                DataGridViewRow row = dgv.Rows[i];
                DataRow dr = dt.NewRow();

                //設條件式 i 等於 RowIndex
                if (i == RowIndex)
                {
                    for (int j = 0; j < dgv.Columns.Count; j++)
                    {
                         // 當 執行到 SaveDate 的時候，將 SaveDateCell 存入 此Cells ，2016.08.25，By Brian
                        if (j == 6)
                        {
                            dr[6] = SaveDateCell.ToString();
                        }
                        else
                        {
                           dr[j] = (row.Cells[j].Value == null) ? "" : row.Cells[j].Value.ToString();                        
                        }
                    } 
                }
                else
                {
                    for (int j = 0; j < dgv.Columns.Count; j++)
                    {                
                        dr[j] = (row.Cells[j].Value == null) ? "" : row.Cells[j].Value.ToString();
                    }                
                }                
                dt.Rows.Add(dr);
            }

            return dt;

        }

        public static DataTable DataGridViewConvertDataTable(DataGridView dgv)
        {
            DataTable dt = new DataTable();
            // Header columns  
            foreach (DataGridViewColumn column in dgv.Columns)        
            {
                DataColumn dc = new DataColumn(column.HeaderText.ToString());

                dt.Columns.Add(dc);
            }
            // Data Rows cells  
            for (int i = 0; i < dgv.Rows.Count; i++)
            {
                DataGridViewRow row = dgv.Rows[i];
                DataRow dr = dt.NewRow();
                for (int j = 0; j < dgv.Columns.Count; j++)
                {
                    dr[j] = (row.Cells[j].Value == null) ? "" : row.Cells[j].Value.ToString();
                }
                dt.Rows.Add(dr);
            }
            return dt;
        }


        public bool DB_Debug_Read(string sFile,string sDate, int iNumber)
        {
            string s_cmd = "";
            string s_sqlcon = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + g_sBaseMdbFile + g_sMdbName + ".mdb;Persist Security Info=True";
            bool bDebug;

            if (g_bDebug)
            {
                 s_cmd = "SELECT * FROM " + g_sTableNameLog + " WHERE FileName = " + "'" + sFile + "'" + " AND SaveDate = " + "'" + sDate + "'";            
                //測試OK!
                // s_cmd = "SELECT FileID FROM " + g_sTableNameLog+ " WHERE FileID IN "+ " ("+"0,1,2"+") ";
                //  s_cmd = "SELECT * FROM " + g_sTableNameLog + " WHERE FileName = " + "'gsrvctr.ini'";         
            }
            else
            {
                s_cmd = "SELECT * FROM " + g_sTableName + " WHERE FileName = " + "'" + sFile + "'" + " AND SaveDate = " + "'" + sDate + "'";             
            }

            OleDbConnection icn = new OleDbConnection();
            icn.ConnectionString = s_sqlcon;

            if (icn.State == ConnectionState.Open) icn.Close();

            try
            {
                icn.Open();

                OleDbCommand cmd = new OleDbCommand(s_cmd, icn);
                cmd.CommandText = s_cmd;
                cmd.CommandTimeout = 600;
                cmd.CommandType = CommandType.Text;

                OleDbDataReader oleDr = cmd.ExecuteReader(); //從ole讀取數據
                int OleCount = oleDr.FieldCount;

                //create a DataTable(new)
                DataTable dt = new DataTable();

                //find colums 
                for (int iCnt = 0; iCnt < OleCount; iCnt++)
                {
                    string a = oleDr.GetName(iCnt);
                    dt.Columns.Add(a);
                }

                //當搜尋到符合的，則回傳true，2016.08.25，By Brian
                if (oleDr.Read())
                {
                    bDebug = true;
                    return bDebug;
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
                     // string sValue = sqlDr[iCnt].ToString();                       
                    }
                    //將讀出的行資料增表的行中
                    dt.Rows.Add(s_subitems);
                }

                //Close SyncObject 與 資料配接器物件來釋放物件所佔用的資源 mean Free not use memory session
                oleDr.Close();
                cmd.Dispose();
                icn.Close();
                           
                return false;
            }
            catch (Exception cp)
            {
                //抓錯誤訊息
                MessageBox.Show(cp.Message, "DB_Debug_Read 警告!!!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                throw cp;
            }        
        }
          
        public void DB_ReadLog(string fileds)
        {
            string s_cmd, s_param;
            string s_sqlcon = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + g_sBaseMdbFile + g_sMdbName + ".mdb;Persist Security Info=True";
                       
            string[] s_field = fileds.Split(',');

            for (int i = 0; i < s_field.Length; i++)
            {
                s_field[i] = "@" + s_field[i].Trim();
            }

            s_param = string.Join(",", s_field);

            s_cmd = "SELECT * FROM " + g_sTableNameLog;
                       
            tbx_SqlCmd.Text = s_cmd.ToString();

            OleDbConnection icn = new OleDbConnection();
            icn.ConnectionString = s_sqlcon;

            if (icn.State == ConnectionState.Open) icn.Close();

            try
            {
                icn.Open();

                OleDbCommand cmd = new OleDbCommand(s_cmd, icn);
                cmd.CommandText = s_cmd;
                cmd.CommandTimeout = 600;
                cmd.CommandType = CommandType.Text;

                OleDbDataReader oleDr = cmd.ExecuteReader(); //從ole讀取數據
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
                    }
                    //將讀出的行資料增表的行中
                    dt.Rows.Add(s_subitems);
                }                
                // 總欄數 Rows * Coluns
                int iTexTotalNum = dt.Rows.Count * OleCount;

                dgv_dt1.DataSource = dt;

                if (dgv_dt1.DataSource != null)
                {                                 
                    lbl_LogTest.Text = "已讀取資料庫!資料行(數):" + OleCount.ToString() + Environment.NewLine + "總欄數(Rows*Columns):" + Convert.ToString(iTexTotalNum);
                }

                //Close SyncObject 與 資料配接器物件來釋放物件所佔用的資源 mean Free not use memory session
                oleDr.Close();
                cmd.Dispose();
                icn.Close();
            }
            catch (Exception cp)
            {
                //抓錯誤訊息
                MessageBox.Show(cp.Message, "DB_ReadLog 警告!!!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                throw cp;
            }

        }

        //Table轉換 以此下函式可以做到(當有Insert)，2016.08.26，By Brian
        public void DataLogConvertDataTable1(string[] sRowsArray)
        {
            string s_cmd="", s_param = "";
        
            string s_sqlcon = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + g_sBaseMdbFile + g_sMdbName + ".mdb;Persist Security Info=True";
                                 
            for (int i = 0; i < sRowsArray.Count(); i++)
            {
                sRowsArray[i] = "'" + sRowsArray[i] + "'";
            }

            s_param = string.Join(",", sRowsArray);

            s_cmd = "INSERT INTO " + g_sTableName + "(" + g_sInsertOne + ") values(" + s_param + ")";          
          
            OleDbConnection icn = new OleDbConnection();
            icn.ConnectionString = s_sqlcon;

            if (icn.State == ConnectionState.Open) icn.Close();

            try
            {
                icn.Open();

                OleDbCommand cmd = new OleDbCommand(s_cmd, icn);
                cmd.CommandText = s_cmd;
                cmd.CommandTimeout = 600;
                cmd.CommandType = CommandType.Text;

           
                int iresult = cmd.ExecuteNonQuery();

                if (iresult ==1)
                {
                   // MessageBox.Show("已將Rows Data INSERT INTO  -DataTable1-");                
                }
                else
                {
                    MessageBox.Show(" DataLog Convert DataTable1 : 轉換失敗 >.<  !!!");
                    //return;          
                }
                
             // oleDr.Close();
                cmd.Dispose();
                icn.Close();                
           }
            catch (Exception cp)
            {
                //抓錯誤訊息
                MessageBox.Show(cp.Message, " DataLogConvertDataTable1 警告!!!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                throw cp;
            }                
        }

        //連結本機端資料庫並做資料讀取 !!! 2016.8.2 ,by Brian 
        public void DB_Read_Table1(string fileds)
        {
            string s_cmd, s_param;
            string s_sqlcon = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + g_sBaseMdbFile + g_sMdbName + ".mdb;Persist Security Info=True";
            
            string[] s_field = fileds.Split(',');

            for (int i = 0; i < s_field.Length; i++)
            {
                s_field[i] = "@" + s_field[i].Trim();
            }

            s_param = string.Join(",", s_field);

            s_cmd = "SELECT * FROM " + g_sTableName;

            tbx_SqlCmd.Text = s_cmd.ToString();

            OleDbConnection icn = new OleDbConnection();
            icn.ConnectionString = s_sqlcon;

            if (icn.State == ConnectionState.Open) icn.Close();

            try
            {              
                icn.Open();

                OleDbCommand cmd = new OleDbCommand(s_cmd, icn);
                cmd.CommandText = s_cmd;
                cmd.CommandTimeout = 600;
                cmd.CommandType = CommandType.Text;

                OleDbDataReader oleDr = cmd.ExecuteReader(); //從ole讀取數據
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
                    }
                    //將讀出的行資料增表的行中
                    dt.Rows.Add(s_subitems);
                }
                
                // 總欄數 Rows * Coluns
                int iTextAllNum = dt.Rows.Count * OleCount;
              
                dgv_dt1.DataSource = dt;
            
                if (dgv_dt1.DataSource != null)
                {
                    //顯示動態Label
                    g_fsView = lbl_rdstatus.Font.Style;
                    g_fmInfo = new FontFamily(lbl_rdstatus.Font.Name);
                    g_ftType = new Font(g_fmInfo, 18, g_fsView);
                    lbl_rdstatus.Font = g_ftType;
                    lbl_rdstatus.ForeColor = Color.Green;

                    lbl_rdstatus.Text = "已讀取資料庫!" + Environment.NewLine + "資料行(數):" + OleCount.ToString() + Environment.NewLine + "總欄數(Rows*Columns):" + Environment.NewLine + Convert.ToString(iTextAllNum);
                }

                //Close SyncObject 與 資料配接器物件來釋放物件所佔用的資源 mean Free not use memory session
                oleDr.Close();
                cmd.Dispose();
                icn.Close();
            }
            catch (Exception cp)
            {
                //抓錯誤訊息
                MessageBox.Show(cp.Message, "DB_Read_Table1 警告!!!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                throw cp;
            }          
        }

        public void SelParamText(int isort, SqlCommand SqlCmd, OleDbCommand OleCmd, int iRowsCount, DataTable dt_view)
        {
            string  g_sRandId = System.Guid.NewGuid().ToString("N");
           
            switch (isort)
            {                
                //序列
                case 0:
                {                   
                    OleDbParameter prm1 = new OleDbParameter("@value1", OleDbType.Numeric, 100000);

                    prm1.Direction = ParameterDirection.Input;

                    if (g_bInsertOne)  //Insert One
                    {                      
                       OleCmd.Parameters.Add(prm1);

                       if (tbx_Id.Text != g_sRandId.ToString())
                       {
                           if (iIdCount < iIdCountAll )                           
                               iIdCount = iIdCountAll;                              
                                                      
                           tbx_Id.Text = Convert.ToString(iIdCount);
                           iIdCount++;
                       }
                        
                        prm1.Value = (!string.IsNullOrEmpty(tbx_Id.Text.ToString())) ? tbx_Id.Text.ToString() : "";                                                                                    
                    }
                    else    //Insert 全部              
                    {
                        MessageBox.Show(prm1 + "無法加入table");
                        return;
                    }
                   break;
                }
                //檔名
                case 1:
                {
                    OleDbParameter prm2 = new OleDbParameter("@value2", OleDbType.VarChar, 255);
                    prm2.Direction = ParameterDirection.Input;

                    if (g_bInsertOne ) //Insert One
                    {
                        OleCmd.Parameters.Add(prm2);
                        prm2.Value = (!string.IsNullOrEmpty(tbx_FileName.Text.ToString())) ? tbx_FileName.Text.ToString() : "";                                                 
                    }
                    else      //Insert 全部              
                    {
                        MessageBox.Show(prm2 + "無法加入table");
                        return;
                    }
                     break;
                }
                //大小
                case 2:
                {
                    OleDbParameter prm3 = new OleDbParameter("@value3", OleDbType.Numeric, 50);
                    prm3.Direction = ParameterDirection.Input;
                    if (g_bInsertOne) //Insert One
                    {
                        OleCmd.Parameters.Add(prm3);                 
                        prm3.Value = (!string.IsNullOrEmpty(tbx_Size.Text.ToString())) ? tbx_Size.Text.ToString() : "";                                                         
                    }
                    else      //Insert 全部              
                    {
                        MessageBox.Show(prm3 + "無法加入table");
                        return;                                         
                    }
                   break;
                }
                //存取日期
                case 3:
                {
                    OleDbParameter prm4 = new OleDbParameter("@value4", OleDbType.VarChar, 255);
                    prm4.Direction = ParameterDirection.Input;

                    if (g_bInsertOne ) //Insert One
                    {
                        OleCmd.Parameters.Add(prm4);                
                        prm4.Value = (!string.IsNullOrEmpty(tbx_Date.Text.ToString())) ? tbx_Date.Text.ToString() : "";                                                                 
                    }
                    else      //Insert 全部              
                    {
                        MessageBox.Show(prm4 + "無法加入table");
                        return;
                    }
                  break;
                }
                //Ver
                case 4:
                {
                    OleDbParameter prm5 = new OleDbParameter("@value5", OleDbType.VarChar, 255);
                    prm5.Direction = ParameterDirection.Input;
                    if (g_bInsertOne) //Insert One
                    {
                        OleCmd.Parameters.Add(prm5);                
                        prm5.Value = (!string.IsNullOrEmpty(tbx_Version.Text.ToString())) ? tbx_Version.Text.ToString() : "";                                                                              
                    }
                    else      //Insert 全部              
                    {
                        MessageBox.Show(prm5 + "無法加入table");
                        return;              
                    }
                    break;
                }
                //後續依需求新增-----------------
            }                                                                           
        }

        public void DB_SelectInsertf2(string fileds2)
        {
            string s_cmd2 = "", s_param2 = "", s_logrecord, sContent = "";

            string s_sqlcon = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + g_sBaseMdbFile + g_sMdbName + ".mdb;Persist Security Info=True";

            int iIDcount = 0, iInsertNum2 = 0;
            int[] iIDTatal = new int[g_iFile];

              // 亂數ID 陣列 ，2016.08.22，By Brian
            int[] iNewIdAll = new int[g_iFile];

            //進度百分比表MAX數值,2016.08.30,Brian
            int  iCentMaxValue = 0;

            //判斷檔案名稱和存取日期
            string sfile1, sdate1,sID;

            string sIdSave = "";

            string sAllSaveFileName = "";

            //參數亂數
            string sParamRand = "";

            bool bcheck = false;
            g_sFileProperty = "";

            //刪除之前的ID，因有重複到2016.08.26，By Brian
            string sLogDelId ="",sDataTable1DelId = "";

            tbx_SqlCmd.Text = s_sqlcon.ToString();

            string[] s_field = fileds2.Split(',');

            for (int i = 0; i < s_field.Length; i++)
            {
                s_field[i] = "@" + s_field[i].Trim();
            }

            s_param2 = string.Join(",", s_field);

            //ex here:
            // string strMsgInsert = @"INSERT INTO DataTable1([ID],[FileName],[Size],[SaveDate],[Version],[Root],[SaveDate],[Group])VALUES(@value1, @value2, @value3,@value4, @value5, @value6,@value7,@value8)";           
            s_cmd2 = "INSERT INTO " + g_sTableNameLog + "(" + g_sText2All + ") values(" + s_param2 + ")";        
          
            tbx_SqlCmd.Text = s_cmd2.ToString();

            OleDbConnection icn = new OleDbConnection();
            icn.ConnectionString = s_sqlcon;

            if (icn.State == ConnectionState.Open) icn.Close();

            try
            {

                //紀錄NewInsert 次數,2016.08.17,By Brian
                iNewInsert2++;

                //pgb_CentValue初始化,2016.08.30,Brian                
                pgb_CentValue.Value = 0;

                icn.Open();
                          
                OleDbCommand cmd = new OleDbCommand(s_cmd2, icn);
                cmd.CommandText = s_cmd2;

                cmd.CommandTimeout = 600;
                cmd.CommandType = CommandType.Text;


                //當Row全部都被Delete完，則顯示已刪除完請再次GetFile 2016.08.30,Brian
                if (dgv_dt1.RowCount <= 0)
                {
                    MessageBox.Show("已刪除完請再次GetFile !!");
                    return;
                }

                string[] sIDinfo = g_sIdSelect.Split('-');

                foreach (var si in sIDinfo)
                {
                    /* 取得要索引的ID值 ,2016.08.17 ,By Brian */
                    if (iIDcount < g_iFile)
                        iIDTatal[iIDcount] = int.Parse(si);

                    iIDcount++;
                }

                //Insert Slect Rows Count ,iIDcount 為所選取的Id總數
                for (int i = 0; i < iIDcount; i++)
                {
                    cmd.Parameters.Clear();//清除掉目前宣告出來的Parameters

                   //sInsertTime = System.DateTime.Today.ToString(); 今天日期

                    DateTime dtn = DateTime.Now;

                    g_sInsertTime = dtn.ToString();

                    for (int col = 0; col < dgv_dt1.Columns.Count; col++)
                    {
                        //將目前DataTable的內容
                        DataTable dt_send = DataGridViewConvertDataTable(dgv_dt1);
                                               
                        /* 取得要Insert iIDdata ,2016.08.17 ,By Brian */
                        sContent = dgv_dt1.Rows[iIDTatal[i]].Cells[col].Value.ToString();

                        //FileID :序列ID
                        if (col == 0)
                        {
                            //用亂數ID
                            string sNewRand = System.Guid.NewGuid().ToString("N");
                            //用系統當前時間                            
                            string sCurrentDataTime = g_sInsertTime;

                            tbx_Id.Text = sContent.ToString();

                            //將 FileName  和 SaveDate 做比對 2016.08.25,By Brian
                            sfile1 = dgv_dt1.Rows[iIDTatal[i]].Cells[1].Value.ToString();
                            sdate1 = dgv_dt1.Rows[iIDTatal[i]].Cells[6].Value.ToString();
                            sID = Convert.ToString(g_iIdMaxLog);

                            sLogDelId += Convert.ToString(g_iIdMaxLog)+",";
                            sDataTable1DelId += Convert.ToString(g_iIdMaxDataTable1)+",";

                            g_bDebug = true;
                           //讀資料庫並搜尋是否有重複 2016.08.23，By Brian
                            bcheck = DB_Debug_Read(sfile1, sdate1, g_iIdMaxLog);

                            if (bcheck)
                            {
                                //方式1: 有重複就停止，之前都會INSERT成功                                 
                                MessageBox.Show("DataLog  (檔案):" + sfile1 + " (存取日期):" + sdate1 + " NG 重複!!! ");                                
                                return;
                                
                                //方式2: 有重複就停止，之前INSERT成功都會全部DELETE ALL,2016.08.26，By Brian
                                /*
                                  sLogDelId = sLogDelId.Substring(0,sLogDelId.Length-1);
                                  sDataTable1DelId = sDataTable1DelId.Substring(0,sDataTable1DelId.Length-1);

                                  MessageBox.Show("Data 檔案:" +sfile1 + " NG 重複!!! ");
                                  DB_DeleteRepeatID(g_sIndexKey, sLogDelId, sDataTable1DelId);                                
                                  return;
                                */
                                
                            }
                            else
                            {                                                           
                                int iRowIndex =  iIDTatal[i];
                                //將更新Insert DateTime 存入DataGrid 2016.08.25，Brian 
                                DataTable dtUpdate = DataGridViewUpdateDataTable(dgv_dt1, iRowIndex, sCurrentDataTime);

                                dgv_dt1.DataSource = dtUpdate;                                                                                  
                            }
                            
                            OleDbParameter prm1 = new OleDbParameter("@value1", OleDbType.Numeric, 100000);
                            prm1.Direction = ParameterDirection.Input;
                            cmd.Parameters.Add(prm1);
                            //用sNewID 當 prm1的Value,2016.08.17,By Brian
                            prm1.Value = (!string.IsNullOrEmpty(sID)) ? sID.ToString(): "";                            
                        }
                        else if (col == 1) //FileName :檔名                        
                        {
                            tbx_FileName.Text = sContent.ToString();
                                      
                            //累加檔案名稱的長度作為最大百分比的數值,2016.08.30,Brian
                            iCentMaxValue += tbx_FileName.Text.Length; 
                            
                            OleDbParameter prm2 = new OleDbParameter("@value2", OleDbType.VarChar, 255);
                            prm2.Direction = ParameterDirection.Input;
                            cmd.Parameters.Add(prm2);
                            prm2.Value = (!string.IsNullOrEmpty(tbx_FileName.Text.ToString())) ? tbx_FileName.Text.ToString() : "";                     
                        }
                        else if (col == 2) //FileSize :檔案大小  
                        {
                            tbx_Size.Text = sContent.ToString();

                            OleDbParameter prm3 = new OleDbParameter("@value3", OleDbType.Numeric, 50);
                            prm3.Direction = ParameterDirection.Input;
                            cmd.Parameters.Add(prm3);
                            prm3.Value = (!string.IsNullOrEmpty(tbx_Size.Text.ToString())) ? tbx_Size.Text.ToString() : "";
                        }
                        else if (col == 3) //FileDate :檔案日期  
                        {
                            tbx_Date.Text = sContent.ToString();

                            OleDbParameter prm4 = new OleDbParameter("@value4", OleDbType.VarChar, 255);
                            prm4.Direction = ParameterDirection.Input;
                            cmd.Parameters.Add(prm4);
                            prm4.Value = (!string.IsNullOrEmpty(tbx_Date.Text.ToString())) ? tbx_Date.Text.ToString() : "";
                        }
                        else if (col == 4) //FileVersion :檔案版本  
                        {
                            tbx_Version.Text = sContent.ToString();

                            OleDbParameter prm5 = new OleDbParameter("@value5", OleDbType.VarChar, 255);
                            prm5.Direction = ParameterDirection.Input;
                            cmd.Parameters.Add(prm5);
                            prm5.Value = (!string.IsNullOrEmpty(tbx_Version.Text.ToString())) ? tbx_Version.Text.ToString() : "";
                        }
                        else if (col == 5) // FileRoot,檔案路徑 
                        {
                            tbx_dirpath.Text = sContent.ToString();
                            OleDbParameter prm6 = new OleDbParameter("@value6", OleDbType.VarChar, 255);
                            prm6.Direction = ParameterDirection.Input;
                            cmd.Parameters.Add(prm6);
                            prm6.Value = (!string.IsNullOrEmpty(tbx_dirpath.Text.ToString())) ? tbx_dirpath.Text.ToString() : "";                        

                           
                        }
                        else if (col == 6)  //SaveDate,存檔日期
                        {
                            //tbx_SaveDate.Text = sContent.ToString();
                            tbx_SaveDate.Text = g_sInsertTime;   //改成SYSTEM DATETIME 現在時間
                            OleDbParameter prm7 = new OleDbParameter("@value7", OleDbType.VarChar, 255);
                            prm7.Direction = ParameterDirection.Input;
                            cmd.Parameters.Add(prm7);
                            prm7.Value = (!string.IsNullOrEmpty(tbx_SaveDate.Text.ToString())) ? tbx_SaveDate.Text.ToString() : "";
                        }
                        else if (col == 7)  //FileGroup,存檔群組
                        {         
                            OleDbParameter prm8 = new OleDbParameter("@value8", OleDbType.VarChar, 255);
                            prm8.Direction = ParameterDirection.Input;
                            cmd.Parameters.Add(prm8);

                            //table 欄位 "Group" 新增機制，2016.08.19，By Brian
                            //如果insert次數 在26次以內
                            if (g_iTimes2 < g_sGroupName.Count())
                            {
                                sParamRand = "Group:(" + g_sGroupName[g_iTimes2].ToString() + ") - ";
                                prm8.Value = (!string.IsNullOrEmpty(sParamRand)) ? sParamRand.ToString() : "";
                            }
                            else  //26次(含26次)以後，用商和餘數的乘積
                            {
                                //ex 28 次 ->    iDivid = 28%26   iDivid=2延伸
                                int iMulix = g_iTimes2 / (g_sGroupName.Count());
                                int iDivid = g_iTimes2 % (g_sGroupName.Count());
                           
                                sParamRand = "Group:(" + g_sGroupName[iDivid] + ") - " + Convert.ToString(iMulix * g_sGroupName.Count() + iDivid) + "-" ; //修正成陣列的才不會被停止攔截,不加時間(因每筆至少有1秒誤差)
                                prm8.Value = (!string.IsNullOrEmpty(sParamRand)) ? sParamRand.ToString() : "";
                            }
                        }
                    }

                    //  新增Row 到 DataTable1，2016.08.26,Brian
                    string sIDNEWR = Convert.ToString(g_iIdMaxDataTable1);

                    string[] LogRow = new string[] { sIDNEWR, tbx_FileName.Text.ToString() , tbx_Size.Text.ToString(), tbx_Date.Text.ToString(), tbx_Version.Text.ToString(), tbx_dirpath.Text.ToString(), tbx_SaveDate.Text.ToString() };

                    //將陣列加入物件收集方便Insert
                    DataLogConvertDataTable1(LogRow);

              
                    g_sFileProperty += Convert.ToString(iInsertNum2) + '^' + tbx_FileName.Text.ToString() + '^' + tbx_Size.Text.ToString() + '^' + tbx_Date.Text.ToString() + '^' + tbx_Version.Text.ToString() + '^' + tbx_dirpath.Text.ToString() + ";";

                    g_lsSearchFile.Add(tbx_FileName.Text.ToString() + '^' + tbx_Size.Text.ToString() + '^' + tbx_Date.Text.ToString());

                    //新增寫入log檔的陣(PS:改成用字串)列，2016.08.30，By Brian
                    sAllSaveFileName += tbx_FileName.Text +'*';
                    //g_lsTotalFile.Add(tbx_FileName.Text.ToString() + '*');
              
                    //收集 g_iIdMaxLog 的字串集合
                    sIdSave += Convert.ToString(g_iIdMaxLog)+",";

                    // DataLog 和 DataTable1 執行完就累加1，做下次ID 的準備
                    g_iIdMaxLog++;
                    g_iIdMaxDataTable1++;
                                   
                    iInsertNum2++;

                    cmd.ExecuteNonQuery();

                    pgb_CentValue.Maximum = iCentMaxValue;

                    for (int ilen = 0; ilen < (int)pgb_CentValue.Maximum; ilen++)
                    {
                        // 進度百分比Run here,2016.08.30,Brian
                        pgb_CentValue.Value = ilen;
                        if (ilen % 25 == 0) //取25%就Delay 5 minin second
                            System.Threading.Thread.Sleep(5);

                        lbl_ColorResult.Text = "Insert 進行中......";
                        Application.DoEvents();

                        if (ilen == pgb_CentValue.Maximum - 1)
                        {
                            lbl_ColorResult.Text = "Insert 完成 #";
                            pgb_CentValue.Value = pgb_CentValue.Maximum;
                        } 
                    }  

                    //將INSERT 成功 Rows就把Rows Delete，2016.08.26，Brian
                    DataGridViewRow r1 = dgv_dt1.Rows[iIDTatal[i]];
                    dgv_dt1.Rows.Remove(r1);                    
                }

                //增加索取陣列，2016.08.18，By Btian
                g_lsSaveFile.Add(g_sFileProperty);
               
                //將字串寫入log做紀錄 ，2016.08.18，By Brian     
                 s_logrecord = WirteFileProperty(sAllSaveFileName, iInsertNum2);

                //將資料資訊集中Log File 
                WriteLog(s_logrecord);
                
                //新增資料群組收集，2016.08.23，By Brian
                g_lsDataGroup.Add(sParamRand);

                //存取iSort ID ，2016.08.25，By Brian
                sIdSave = sIdSave.Substring(0, sIdSave.Length - 1);
                g_lsIdIndexSelect.Add(sIdSave);

                //增加Insert times                
                g_iTimes2++;

                //清除ack mode to free !
                cmd.Cancel();
                cmd.Dispose();
                icn.Close();
                icn.Dispose();

                MessageBox.Show("INSERT 資料表.(DataTable1) (DataLog) OK!!!");
            }
            catch (Exception ex)
            {
                //抓錯誤訊息
                MessageBox.Show(ex.Message, "DB_SelectInsertf2 警告!!!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        //連結本機端資料庫並做資料新增 INSERT !!! 2016.08.03 ,by Brian 
        public void DB_Insert(string fileds)
        {
           string s_cmd, s_param,sInsertID,ss="";
          
            //SQL 本地端 Connecting
           // string s_sqlcon = "Data Source=.\\SQLEXPRESS;AttachDbFilename=C:\\Users\\IRIS\\Desktop\\GetFileInfo\\GetFileInfo\\FileDataTable.mdf;Integrated Security=True;Connect Timeout=30;User Instance=True";
           //ACCESS 本地端 Connecting
            string s_sqlcon = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + g_sBaseMdbFile + g_sMdbName + ".mdb;Persist Security Info=True";
          
            tbx_SqlCmd.Text = s_sqlcon.ToString();

            string[] s_field = fileds.Split(',');

            for (int i = 0; i < s_field.Length; i++)
            {
                s_field[i] = "@" + s_field[i].Trim();
            }
         
            s_param = string.Join(",", s_field);

            //ex here:
            // string strMsgInsert = @"INSERT INTO [test].[dbo].[symbol_stock_info] 
            //([exchange_id],[exchange_name],[registration_date])VALUES(@value1, @value2, @value3) ";
            s_cmd = "INSERT INTO " + g_sTableName + "(" + g_sTextAll + ") values(" + s_param + ")";

            tbx_SqlCmd.Text = s_cmd.ToString();

            OleDbConnection icn = new OleDbConnection();
            icn.ConnectionString = s_sqlcon;

            if (icn.State == ConnectionState.Open) icn.Close();

            try             
            {
                icn.Open();

                SqlCommand mySqlCmd = new SqlCommand();
                OleDbCommand cmd = new OleDbCommand(s_cmd, icn);
                cmd.CommandText = s_cmd;
                cmd.CommandTimeout = 600;
                cmd.CommandType = CommandType.Text;

                //將目前DataTable的內容
                DataTable dt_send = DataGridViewConvertDataTable(dgv_dt1);
                
                 //~Insert One ~
                if (g_bInsertOne)
                {              
                   for (int col = 0; col < dt_send.Columns.Count; col++)
                    {
                        SelParamText(col, mySqlCmd, cmd, g_iSelect, dt_send);
                    }
                    //執行SQL CMD                
                    int recordsaffected = cmd.ExecuteNonQuery(); //insertcmd.executenonquery(); 


                    if (recordsaffected == 1)     // response.write("< script>alert(" + "插入成功" + ");< /script>");  
                    {
                        g_iOk++;            
                    }
                }
                else   //~Insert All ~
                {                                                      
                  int iOkAll = 0;
                    
                    for (int i = 0; i < g_iFile; i++)
                    {
                       cmd.Parameters.Clear();//清除掉目前宣告出來的Parameters

                        for (int col = 0; col < dgv_dt1.Columns.Count; col++)
                        {                          
                            ss = dgv_dt1.Rows[i].Cells[col].Value.ToString();

                            if (col == 0)
                            {
                                sInsertID = System.Guid.NewGuid().ToString("N");
                                       
                                tbx_Id.Text = ss.ToString();

                                if (tbx_Id.Text != sInsertID.ToString())
                                {
                                    //儲存 INSERT 次數
                                    if (iIdCountAll < iIdCount)                                    
                                        iIdCountAll = iIdCount;
                                                                                                                                            
                                    tbx_Id.Text = Convert.ToString(iIdCountAll);
                                    iIdCountAll++;
                                }

                                OleDbParameter prm1 = new OleDbParameter("@value1", OleDbType.Numeric, 100000);
                                prm1.Direction = ParameterDirection.Input;
                                cmd.Parameters.Add(prm1);
                                prm1.Value = (!string.IsNullOrEmpty(tbx_Id.Text.ToString())) ? tbx_Id.Text.ToString() : "";                                
                            }
                               
                            else if (col == 1)
                            {
                                tbx_FileName.Text = ss.ToString();

                                OleDbParameter prm2 = new OleDbParameter("@value2", OleDbType.VarChar, 255);
                                prm2.Direction = ParameterDirection.Input;                              
                                cmd.Parameters.Add(prm2);
                                prm2.Value = (!string.IsNullOrEmpty(tbx_FileName.Text.ToString())) ? tbx_FileName.Text.ToString() : "";                            
                            }
                           else if (col == 2)
                            {
                                tbx_Size.Text = ss.ToString();

                                OleDbParameter prm3 = new OleDbParameter("@value3", OleDbType.Numeric, 50);
                                prm3.Direction = ParameterDirection.Input;                           
                                cmd.Parameters.Add(prm3);
                                prm3.Value = (!string.IsNullOrEmpty(tbx_Size.Text.ToString())) ? tbx_Size.Text.ToString() : "";                                                                                
                            }
                            else if (col == 3)
                            {
                                tbx_Date.Text = ss.ToString();

                                OleDbParameter prm4 = new OleDbParameter("@value4", OleDbType.VarChar, 255);
                                prm4.Direction = ParameterDirection.Input;                            
                                cmd.Parameters.Add(prm4);
                                prm4.Value = (!string.IsNullOrEmpty(tbx_Date.Text.ToString())) ? tbx_Date.Text.ToString() : "";                               
                            }
                            else if(col == 4)
                            {
                                tbx_Version.Text = ss.ToString();

                                OleDbParameter prm5 = new OleDbParameter("@value5", OleDbType.VarChar, 255);
                                prm5.Direction = ParameterDirection.Input;                               
                                cmd.Parameters.Add(prm5);
                                prm5.Value = (!string.IsNullOrEmpty(tbx_Version.Text.ToString())) ? tbx_Version.Text.ToString() : "";                                                                                                  
                            }
                                                                            
                        }

                        iOkAll++;
                        g_iOkAll = iOkAll;
                        cmd.ExecuteNonQuery();                       
                    }
                   // tbx_InsertCount.Text = "寫入ALL完成 ->" + Convert.ToString(g_iOkAll) + "筆)";                             
                }             
                
                //清除ack mode to free !
                cmd.Cancel();
                cmd.Dispose();
                icn.Close();
                icn.Dispose();
                //mySqlCmd.Cancel();
                //mySqlCmd.Dispose();
               
            }
            catch (Exception ex)
            {
                //抓錯誤訊息
                 MessageBox.Show(ex.Message, "Insert 警告!!!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        //有重複就 DELETE 之前成功的ID ，也就是不給正常INSERT !!! 2016.08.26 ,by Brian 
        public void DB_DeleteRepeatID(string sID, string sLogRow, string sTable1Row)
        {
            string s_cmd = "";
            string s_sqlcon = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + g_sBaseMdbFile + g_sMdbName + ".mdb;Persist Security Info=True";

            //ex here:            
            //s_cmd ="DELETE FROM Table1 WHERE Id IN (1,3,4,5,7,8)";   <-SQL 格式     
            //s_cmd ="delete from tblusers1 where UserID = 'user1'";   <-ACCESS 格式     

            //刪除2個 table ,做2次-2016.08.26,Brian
            for (int iDelet = 0; iDelet < 2; iDelet++)
            {
                if(iDelet == 0)  //刪除DataLog ID
                {
                    s_cmd = " DELETE  FROM " + g_sTableNameLog + " WHERE " + sID + " IN " + "(" + sLogRow + ")";

                    OleDbConnection icn = new OleDbConnection();
                    icn.ConnectionString = s_sqlcon;

                    if (icn.State == ConnectionState.Open) icn.Close();

                    try
                    {
                        icn.Open();
                        OleDbDataAdapter OleaDapter = new OleDbDataAdapter();
                  
                        OleaDapter.DeleteCommand = icn.CreateCommand();
                        OleaDapter.DeleteCommand.CommandText = s_cmd;

                        int Deleteresult = OleaDapter.DeleteCommand.ExecuteNonQuery(); //insertcmd.executenonquery(); 

                        if (Deleteresult == 1)     // response.write("< script>alert(" + "刪除成功" + ");< /script>");  
                        {
                            MessageBox.Show("@ DataLog @ 此次Insert 的ID 已全刪除 ! ");                   
                        }

                    }
                    catch (Exception ex)
                    {
                        //抓錯誤訊息
                        MessageBox.Show(ex.Message, "iDelet DataLog 警告!!!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }                               
                }
                else if (iDelet == 1) //刪除DataTable1 ID
                {
                    s_cmd = " DELETE  FROM " + g_sTableName + " WHERE " + sID + " IN " + "(" + sTable1Row + ")";

                    OleDbConnection icn = new OleDbConnection();
                    icn.ConnectionString = s_sqlcon;

                    if (icn.State == ConnectionState.Open) icn.Close();

                    try
                    {
                        icn.Open();
                        OleDbDataAdapter OleaDapter = new OleDbDataAdapter();
               
                        OleaDapter.DeleteCommand = icn.CreateCommand();
                        OleaDapter.DeleteCommand.CommandText = s_cmd;

                        int Deleteresult = OleaDapter.DeleteCommand.ExecuteNonQuery(); //insertcmd.executenonquery(); 

                        if (Deleteresult == 1)     // response.write("< script>alert(" + "刪除成功" + ");< /script>");  
                        {
                            MessageBox.Show("~ DataTable1~ 此次Insert 的ID 已全刪除 ! ");
                        }

                    }
                    catch (Exception ex)
                    {
                        //抓錯誤訊息
                        MessageBox.Show(ex.Message, "iDelet DataTable1 警告!!!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        //連接資料庫並將DataRow 刪除 DELETE !!! 2016.08.11 ,by Brian 
        public void DB_Delete(string sID, string sRowNum)
        {
            string s_cmd = "", sdelline = "";
            int iselect = 0 , iDeleteCount = 0;
            int[] iDetIDTotal=new int[3];
            string s_sqlcon = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + g_sBaseMdbFile + g_sMdbName + ".mdb;Persist Security Info=True";
            string sIdCollect = "";

            tbx_SqlCmd.Text = s_sqlcon.ToString();

            //ex here:            
            //s_cmd ="DELETE FROM Table1 WHERE Id IN (1,3,4,5,7,8)";   <-SQL 格式     
            //s_cmd ="delete from tblusers1 where UserID = 'user1'";   <-ACCESS 格式   

            if (g_bDelRow)
            {
               s_cmd = " DELETE  FROM " + g_sTableName + " WHERE " + sID + " IN " + "(" + sRowNum + ")";
            }
            else
            {
                s_cmd = " DELETE  FROM " + g_sTableNameLog + " WHERE " + sID + " IN " + "(" + sRowNum + ")";                        
            }
            
            tbx_SqlCmd.Text = s_cmd.ToString();

            OleDbConnection icn = new OleDbConnection();
            icn.ConnectionString = s_sqlcon;

            if (icn.State == ConnectionState.Open) icn.Close();

            try
            {
                icn.Open();
                OleDbDataAdapter OleaDapter = new OleDbDataAdapter();
                /*
                OleDbCommand cmd = new OleDbCommand(s_cmd, icn);
                cmd.CommandText = s_cmd;
                cmd.CommandTimeout = 600;
                cmd.CommandType = CommandType.Text;
                */
                OleaDapter.DeleteCommand = icn.CreateCommand();
                OleaDapter.DeleteCommand.CommandText = s_cmd;

                if (dgv_dt1.RowCount <= 0)
                {
                    if (g_bDelRow)
                    {
                        MessageBox.Show("DataTable1 已經刪除完畢了 ! ");
                    }
                    else
                    {
                        MessageBox.Show("DataLog 已經刪除完畢了 ! ");                    
                    }

                    return;
                }

                int Deleteresult = OleaDapter.DeleteCommand.ExecuteNonQuery(); //insertcmd.executenonquery(); 

                //只有執行成功一次才會跑if(Deleteresult == 1),這邊先MASK不設條件式，2016.08.31，Brian
               // if (Deleteresult == 1)     // response.write("< script>alert(" + "刪除成功" + ");< /script>");  
                {
                    foreach (DataGridViewRow row in dgv_dt1.SelectedRows)
                    {
                        iselect = row.Index;
           
                        if (!row.IsNewRow)
                        {
                            dgv_dt1.Rows.Remove(row);

                            //只取前6筆作參考即可，2016.08.31，Brian
                            if (iDeleteCount <=2)
                            {
                              //  iselect = row.Index;
                                iDetIDTotal[iDeleteCount] = iselect;
                            } 
                            
                            iDeleteCount++;
                        }

                    }

                    sIdCollect = string.Join("-", iDetIDTotal);

                    sdelline = "已刪除檔案: (" + g_sFileTotal.ToString() + "...)等  (" + Convert.ToString(iDeleteCount) + ") 筆資料" + "- " + System.DateTime.Now.ToShortDateString() + System.DateTime.Now.ToShortTimeString() + " -";
                
                    //紀錄Log
                    WriteLog(sdelline);

                    if (g_bDelRow)
                    {
                        MessageBox.Show(" DataTable1 -> 已刪除 (" + Convert.ToString(iDeleteCount) + ") 筆資料");               
                    }
                    else
                    {
                        MessageBox.Show(" DataLog ->  已刪除 (" + Convert.ToString(iDeleteCount) + ") 筆資料");                                        
                    }

               }

            }
            catch (Exception ex)
            {
                //抓錯誤訊息
                MessageBox.Show(ex.Message, "DB_Delete 警告!!!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        //open Auto Form function!
        private void OpenLocalDb_ConnnectStatus(object obj)
        {
            Application.Run(new ExcelTask());
        }
            
        //開啟瀏覽資料夾(含路徑)
        private void btn_OpenFolder_Click(object sender, EventArgs e)
        {
            lbl_Status.Text = lbl_rdstatus.Text = "狀態";
            lbl_ROwNum.Text = Convert.ToString(0);
            lbl_ROwNum.ForeColor = Color.Black;
            lbl_Status.ForeColor = lbl_rdstatus.ForeColor = Color.Black;

            //預設動作 ckx_selectAll 取消,2016.08.31,Brian
            ckx_selectAll.Checked = false;

            //clear g_PathFailString 空值=""
            g_PathFailString = "";

            //如果兩個路徑選取都空值,顯示訊息請選取一個以便搜尋，2016.08.22，By Brian
            if (!ckx_AllPathFolder.Checked && !ckx_SinglePathFolder.Checked)
            {
                MessageBox.Show("~請選取其中一個路徑選取方式，不可全空選~");
                return;
            }

            //確認開啟               
            if (fbd_OpenPathFolder.ShowDialog() == DialogResult.OK)
            {                   
                //Show Select Path  
                tbx_FilePath.Text = fbd_OpenPathFolder.SelectedPath;

                // RootPath
                g_sPathFile = tbx_FilePath.Text.ToString();                
            }
            else // 開啟Fail 
            {
                this.tbx_FilePath.Text = "Open  FolderBrowserDialog NG !";
                g_PathFailString = tbx_FilePath.Text;
            }
        }

        //開啟全資料夾
        private void ckx_AllPathFolder_CheckedChanged(object sender, EventArgs e)
        {
            if (ckx_AllPathFolder.Checked)
            {
                ckx_AllPathFolder.Checked = true;
                ckx_SinglePathFolder.Checked = false;
            }
        }

        //開啟單一資料夾
        private void ckx_SinglePathFolder_CheckedChanged(object sender, EventArgs e)
        {
            if(ckx_SinglePathFolder.Checked)
            {
               ckx_SinglePathFolder.Checked = true;
               ckx_AllPathFolder.Checked = false;            
            }
        }

        //Find DB NAX ID AND RETURN CODE,2016.08.24,By Brian
        public int  DB_FileIdFindMaxLog()
        {
            string s_cmd = "";
            string s_sqlcon = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + g_sBaseMdbFile + g_sMdbName + ".mdb;Persist Security Info=True";
            int iIdZero= 0;

            s_cmd = " SELECT MAX(FileID) FROM " + g_sTableNameLog;
        
            OleDbConnection icn = new OleDbConnection();
            icn.ConnectionString = s_sqlcon;

            if (icn.State == ConnectionState.Open) icn.Close();

            try
            {
                icn.Open();

                OleDbCommand cmd = new OleDbCommand(s_cmd, icn);
                cmd.CommandText = s_cmd;
                cmd.CommandTimeout = 600;
                cmd.CommandType = CommandType.Text;

                OleDbDataReader oleDr = cmd.ExecuteReader(); //從ole讀取數據
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

                        if( s_subitems[iCnt] =="")
                            return iIdZero;
                    }
                    //將讀出的行資料增表的行中
                    dt.Rows.Add(s_subitems);
                }
         
                dgv_dt1.DataSource = dt;

                g_iIdMaxLog = int.Parse(dgv_dt1.Rows[0].Cells[0].Value.ToString());

                if (g_iIdMaxLog < 0)
                {
                    g_iIdMaxLog = 0;                  
                }
                else
                {
                    g_iIdMaxLog = g_iIdMaxLog + 1;                 
                }


                dgv_dt1.DataSource = null;

                oleDr.Close();
                cmd.Dispose();
                icn.Close();

                return g_iIdMaxLog;
            }
            catch (Exception cp)
            {
                //抓錯誤訊息
                MessageBox.Show(cp.Message, " DB_FileIdFindMaxLog 警告!!!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                throw cp;
            }

        }


        public int DB_FileIdFindMaxDataTable11()
        {
            string s_cmd = "";
            string s_sqlcon = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + g_sBaseMdbFile + g_sMdbName + ".mdb;Persist Security Info=True";

            int iIdZero = 0;

            s_cmd = " SELECT MAX(FileID) FROM " + g_sTableName;
            
            OleDbConnection icn = new OleDbConnection();
            icn.ConnectionString = s_sqlcon;

            if (icn.State == ConnectionState.Open) icn.Close();

            try
            {
                icn.Open();

                OleDbCommand cmd = new OleDbCommand(s_cmd, icn);
                cmd.CommandText = s_cmd;
                cmd.CommandTimeout = 600;
                cmd.CommandType = CommandType.Text;

                OleDbDataReader oleDr = cmd.ExecuteReader(); //從ole讀取數據
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
                        if (s_subitems[iCnt] == "")
                            return iIdZero;

                    }
                    //將讀出的行資料增表的行中
                    dt.Rows.Add(s_subitems);
                }
      
                dgv_dt1.DataSource = dt;

                g_iIdMaxDataTable1 = int.Parse(dgv_dt1.Rows[0].Cells[0].Value.ToString());

                if (g_iIdMaxDataTable1 < 0)
                {
                    g_iIdMaxDataTable1 = 0;
                }
                else
                {
                    g_iIdMaxDataTable1 = g_iIdMaxDataTable1 + 1;
                }

       
                dgv_dt1.DataSource = null;


                oleDr.Close();
                cmd.Dispose();
                icn.Close();

                return g_iIdMaxDataTable1;
            }
            catch (Exception cp)
            {
                //抓錯誤訊息
                MessageBox.Show(cp.Message, " DB_FileIdFindMaxDataTable11 警告!!!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                throw cp;
            }
        }

        //取得資料夾內檔案所有屬性(檔名，大小，日期，版本)
        private void btn_GetFile_Click(object sender, EventArgs e)
        {
            //一開始 g_iGetInsert提供0        
            if (g_sPathFile.ToString() == "" || g_PathFailString.ToString() == "Open  FolderBrowserDialog NG !")
            {
                MessageBox.Show("請Open路徑 !");
                return;            
            }               
            //按鈕功能執行不讓再次干擾，所以Enable = False，2016.08.30，Brian    
            btn_GetFile.Enabled = false;

            //retry 初始化 ,2016.08.31,Brian
            dgv_dt1.DataSource = null;
            lbl_Status.Text ="狀態";
            lbl_Status.ForeColor = Color.Black;
           
            //取得資料庫最大值當之後要INSERT則從最大值得再加一筆計算，2016.08.24，By Brian
            g_iIdMaxLog = DB_FileIdFindMaxLog();
            g_iIdMaxDataTable1 = DB_FileIdFindMaxDataTable11();
            
           //begin start path search File
            FileInfo[] filesInfo = null;
            DirectoryInfo di = new DirectoryInfo(g_sPathFile);
            FileVersionInfo fVInFoContent;
            int ilimitHigh = 0 , iFileInfoLength = 0;
            string sVersion ="";
            // 2016.08.09 ,清空全域變數 g_sAllFile 
            g_sAllFile = "";

            string sPattern = "*.*";

            
            //step 1-> search RootPath
            try
            {
                //pgb_CentValue初始化,2016.08.30,Brian
                pgb_CentValue.Value = 0;

                //clear all listBox item! 
                if (lbx_FileName.Items.Count > 0)
                {
                    //2016.08.09 移除listbox item all
                    lbx_FileName.Items.Clear();
                    lbx_FileName.Refresh();
                }

                //全部搜尋
                if (ckx_AllPathFolder.Checked == true)  
                {
                    //全部目錄
                    filesInfo = di.GetFiles(sPattern, SearchOption.AllDirectories);                                       
                }
                //單一資料夾搜尋
                else if (ckx_SinglePathFolder.Checked == true)          
                {
                   //只搜尋當前目錄
                    filesInfo = di.GetFiles(sPattern, SearchOption.TopDirectoryOnly);                         
                }    
            }
            catch
            {
                lbl_Status.Text = "NG!";
                lbl_Status.ForeColor = Color.Red;
                dgv_dt1.DataSource = null;
                return;
            }

            //step 2-> 資料夾有內存檔案          
            if (filesInfo != null && filesInfo.Count() != 0)
            {

                g_lsData = new List<string>();

                //做第一筆紀錄，永遠為做New 1次
                g_iGetInsert++;
                            
                //增加檔案名稱至listbox1的表單
                foreach (FileInfo Fileinfolist in filesInfo)
                {                   
                    //取得檔案資訊內容(長度),2016.08.30,Brian
                    iFileInfoLength += Fileinfolist.FullName.Length;
                    pgb_CentValue.Maximum = iFileInfoLength;

                    fVInFoContent = System.Diagnostics.FileVersionInfo.GetVersionInfo(Fileinfolist.FullName.ToString());
                    sVersion = fVInFoContent.ProductVersion;
            
                    g_lsData.Add(Convert.ToString(ilimitHigh) + "@@" + Fileinfolist.Name.ToString() + "@@" + Fileinfolist.Length + "@@" + Fileinfolist.LastAccessTime + "@@" + sVersion + "@@" + Fileinfolist.DirectoryName + "@@" + Fileinfolist.LastWriteTime+"@@");
                    g_sAllFile += Convert.ToString(ilimitHigh) + '^' + Fileinfolist.Name.ToString() + '^' + +Fileinfolist.Length + '^' + Fileinfolist.LastAccessTime + '^' + sVersion + '^' + Fileinfolist.DirectoryName + '^' +"   "+ ";";

                    lbx_FileName.Items.Add(Fileinfolist);  

                    ilimitHigh++;  //進入下一個搜尋序列                                   
                }

                for (int ilen = 0; ilen < pgb_CentValue.Maximum; ilen++)
                {
                    // 進度百分比Run here,2016.08.30,Brian
                    pgb_CentValue.Value = ilen;
                    if (ilen % 25 == 0) //取25%就Delay 2 minin second
                        System.Threading.Thread.Sleep(2);

                    lbl_ColorResult.Text = "Get 進行中......";
                    Application.DoEvents();

                    if (ilen == pgb_CentValue.Maximum-1)
                    {
                        lbl_ColorResult.Text = "GetFile 完成(@.@)";                       
                        pgb_CentValue.Value = pgb_CentValue.Maximum;
                    } 
                }  

                //Rows總數量存放g_sFileNum
                g_sFileNum = ilimitHigh;
              
                //catch Data attribute (檔案名稱、版本、日期、大小) to DataTable
                //將listBox item convert to DataTable              
                this.ConvertStringToList(g_sAllFile);

                //紀錄Getchange 次數,2016.08.17,By Brian
                iGetChange++;

                lbl_Status.Text = "*OK*";
                lbl_Status.ForeColor = Color.Green;

                //按鈕功能重新執行，所以Enable = True，2016.08.30，Brian    
                btn_GetFile.Enabled = true;

                //預設動作 ckx_selectAll 取消,2016.08.31,Brian
                if (ckx_selectAll.Checked == true)
                    ckx_selectAll.Checked = false;
            }            
            else //沒有任何檔案存在 !
            {
                lbl_Status.Text = "~NO Any File ~";
                lbl_Status.ForeColor = Color.Red;
                dgv_dt1.DataSource = null;           
                btn_GetFile.Enabled = true;
                return;
            }
        }

        public void  SelectNumValue(int iSelext , String s)
        {
              switch(iSelext)
              {     
                //序列 
                case 0:
                {
                    tbx_Id.Text = s.ToString();         
                    break;
                } 
                //檔名
                case 1:
                {
                    tbx_FileName.Text = s.ToString();
                    break;
                } 
                //大小
                case 2:
                {
                    tbx_Size.Text = s.ToString();
                    break;
                } 
                //存取日期
                case 3:
                {
                    tbx_Date.Text = s.ToString();
                    break;
                } 
                //Ver版本
                case 4:
                {
                    tbx_Version.Text = s.ToString(); 
                    break;
                }

                //所屬路徑
                case 5:
                {
                    tbx_dirpath.Text = s.ToString();
                    break;
                } 
                //..........其餘新增如有需求
              }    
        }

        //選擇其他檔案動作顯示
        private void lbx_FileName_SelectedIndexChanged(object sender, EventArgs e)
        {
            int iNum = 0;
            tbx_Id.Text ="";
            tbx_FileName.Text="";
            tbx_Size.Text="";
            tbx_Date.Text="";
            tbx_Version.Text="";
            tbx_dirpath.Text = "";

            //選到的檔名
            string  s_Select = lbx_FileName.Text.ToString();

            for (int i = 0; i < lbx_FileName.Items.Count; i++)
            {
                if (g_lsData[i].IndexOf(s_Select, StringComparison.OrdinalIgnoreCase) >= 0)  //找到要的檔名*.xx                
                {
                    //選擇第i個Rows
                    g_iSelect = i;

                    //string[] sInfoAll = g_lsData[i].Split(','); 
                    //Split 字串分割，不使用字元分割，因有些檔案屬性會有衝突，故使用之 2016.08.10 ，By Brian
                    //  string[] sInfoAll = g_lsData[i].Split(new string[] {"@@"}, StringSplitOptions.RemoveEmptyEntries);
                    string[] sInfoAll = g_lsData[i].Split(new string[] { "@@" }, StringSplitOptions.None);

                    foreach (var si in sInfoAll)
                    {
                        //顯示在tbx_FileProperty                  
                        //將要insert資料填入欄位 2016.07.29 New Add  
                        SelectNumValue(iNum, si);

                        iNum++;
                    }

                }
            }
        }
   
        //寫入本機資料庫(單筆資料)
        private void btn_InsertSingle_Click(object sender, EventArgs e)
        {
            g_bInsertOne = true;

            DB_Insert(g_sValAll);
       
            // this.Close();
            /* 
              th = new Thread(OpenLocalDb_ConnnectStatus);
              th.SetApartmentState(ApartmentState.STA);
              th.Start();  
            */           
        }
   
        //轉換Excel格式 ,2016.09.01,Brian
        private void btn_CvtXls_Click(object sender, EventArgs e)
        {
            //方法一:會秀之前已Insert過的紀錄，需要做從重存，一樣OK !
           //  ExportExcelToOne();

            //方法二:會先指定存取檔名，接續在執行轉換Excel，會比較快一些！
            ExportExcelToTwo(dgv_dt1);           

           /*
           thd1 = new Thread(OpenExcelForm);
           thd1.SetApartmentState(ApartmentState.STA);
           thd1.Start();
           */
        }

        //轉換Excel 方法1，2016.09.01，Brian
        private void ExportExcelToOne()
        {
            int iSortId = 0;  //excel列址
            string sSvaDateTime = "", sFinalExcelName = "", sRand = "";

            if (dgv_dt1.RowCount <= 0)
            {
                MessageBox.Show("~ RowData 無資料 ~ ");
                return;
            }

            Cursor = Cursors.WaitCursor;

            //建立Excel 的物件準備做轉換
            Excel.Application xls = new Excel.Application();

            if (xls == null)
            {
                MessageBox.Show("無法建立Excel，可能您的電腦未安装Excel");
                return;
            }

            Excel.Workbook xlsWookBook;
            Excel.Worksheet xlsWookSheet;
            //增加錯誤轉換的通知值
            object misValue = System.Reflection.Missing.Value;
            xlsWookBook = xls.Workbooks.Add(misValue);

            g_iExcelNum++;
            //工作項 從第1~N筆作業,2016.08.31,Brian
            xlsWookSheet = (Excel.Worksheet)xlsWookBook.Worksheets.get_Item(g_iExcelNum);

            //啟動Excel 轉換
            xls.Application.Workbooks.Add(true); //引用Excel工作簿 
            xls.Visible = false;

            /*
             //用另外的Excel Columns Label
            for (int i = 0; i < g_sExcelLabel.Count(); i++)
            {
                excel.Cells[1, i + 1] = g_sExcelLabel[i].ToString();            
            }
            */
        
            iSortId++; //遞增為 1

            //——欄位———————–
            //將資料表 轉至 Excel        
            for (int i = 0; i < dgv_dt1.ColumnCount; i++)
            {
                //在Excel 第1行(iSortId == 1) 將Columns Name 填入
                xls.Cells[iSortId, i + 1] = dgv_dt1.Columns[i].HeaderText;
            }

            //——內容———————–
            //在Excel 第2行(iSortId == 2)開始寫入Excel
            iSortId++;

            foreach (DataGridViewRow row in dgv_dt1.Rows)
            {
                for (int i = 0; i < row.Cells.Count; i++)  //row.Cells.Count = 7筆
                {
                    xls.Cells[iSortId, i + 1] = row.Cells[i].Value.ToString();
                }
                iSortId++; //持續累加iSortId,直到最後一筆dgv_dt1.RowCount-1
            }

            //—–結尾————————                     
            Cursor = Cursors.Default;

            //儲存Excel 路徑確認並做儲存
            if (g_sExcelFileList != null)
            {
                try
                {
                    DateTime dtn = DateTime.Now;

                    sSvaDateTime = dtn.ToString();

                    sRand = System.Guid.NewGuid().ToString("N");

                    //取亂數前3筆字元
                    sRand = sRand.Substring(0, 5);
                  //MessageBox.Show("取亂數前5筆字元:"+sRand.ToString());
                    //ex: sFinalExcelName = C:\Users\IRIS\Desktop\GetFileInfo\bin\Debug\ExcelFile\Excel-Log-(7)-2016.08.31 23:27:35
                    sFinalExcelName = g_sExcelFileList + "Excel-Work-" + sRand;

                    xlsWookBook.SaveAs(sFinalExcelName, Excel.XlFileFormat.xlWorkbookNormal, misValue, misValue, misValue, misValue, Excel.XlSaveAsAccessMode.xlExclusive, misValue, misValue, misValue, misValue, misValue);

                    xlsWookBook.Close(true, misValue, misValue);
                    xls.Quit();
                    xlsWookSheet = null;
                    xlsWookBook = null;
                    xls = null;
                    MessageBox.Show("* 資料轉至Excel作業結束，接續選擇儲存路徑...*");

                }
                catch (Exception ex)
                {
                    MessageBox.Show("匯出文件時出錯,EXCEL文件可能正在使用中！\n" + ex.Message);
                }

            }
        }

        //轉換Excel 方法2，2016.09.01，Brian        
        private void ExportExcelToTwo(DataGridView myDGV)
        {
            string sSaveFileName = "", sLogRecord = "" ,sSvaDateTime="";

            if (dgv_dt1.RowCount <= 0)
            {
                MessageBox.Show("~ RowData 無資料 ~ ");
                return;
            }

            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.DefaultExt = "xlsx";
            saveDialog.Filter = "Excel文件|*.xlsx";
            saveDialog.ShowDialog();
            sSaveFileName = saveDialog.FileName;
            if (sSaveFileName.IndexOf(":") < 0)
                return;
           
            Excel.Workbook xlsWookBook;
            Excel.Workbooks xlsWookBooks;
            Excel.Worksheet xlsWookSheet;
            Excel.Application xls = new Excel.Application();

            int iSortId = 0 ;  //excel列址

            if (xls == null)
            {
                MessageBox.Show("無法建立Excel，可能您的電腦未安装Excel");
                return;
            }

            g_iExcelNum2 = 1;
            xlsWookBooks = xls.Workbooks;
            xlsWookBook = xlsWookBooks.Add(Excel.XlWBATemplate.xlWBATWorksheet);
            xlsWookSheet = (Excel.Worksheet)xlsWookBook.Worksheets[g_iExcelNum2]; //取得sheet1

            iSortId++;

            for (int i = 0; i < myDGV.ColumnCount; i++)
            {
                // 依照上面合併使用過的列數(最大值+1) ，從第一列開始寫欄位名稱
                xls.Cells[iSortId, i + 1] = myDGV.Columns[i].HeaderText;          
            }
            //從2行開始Insert
            iSortId++;
  
            foreach (DataGridViewRow row in myDGV.Rows)            
            {
                for (int i = 0; i < row.Cells.Count; i++)  //row.Cells.Count = DataTable1 (7筆),DataLog (8筆)
                {                
                    xls.Cells[iSortId, i + 1] = row.Cells[i].Value.ToString();
                }
                iSortId++; //持續累加iSortId,直到最後一筆dgv_dt1.RowCount-1
                System.Windows.Forms.Application.DoEvents();
            }

            //自動調整欄位
            xlsWookSheet.Columns.EntireColumn.AutoFit();

            if (sSaveFileName != "")
            {
                try
                {
                    DateTime dtn = DateTime.Now;
                    sSvaDateTime = dtn.ToString();

                    xlsWookBook.Saved = true;
                    xlsWookBook.SaveCopyAs(sSaveFileName);                   
                }
                catch (Exception ex)
                {                    
                    MessageBox.Show("匯出文件時出錯,EXCEL文件可能正在使用中！\n" + ex.Message);
                }
            }
          
            xls.Quit();
            GC.Collect();  //回收釋放

            sLogRecord = "以將DataGridView資料轉換Excel 檔名:(" + sSaveFileName + ") 一份" + sSvaDateTime;

            //將資料資訊集中Log File 
            WriteLog(sLogRecord);
                
            MessageBox.Show(sSaveFileName + "的資料匯出成功", "提示", MessageBoxButtons.OK);
            g_iExcelNum2++; //第 1 筆 Excel,之後累加 "必須因為下次在執行時是用此索引，不然只會做一次成功而已"，2016.09.01，Brian
        }
       
        private void OpenExcelForm(object obj)
        {
            Application.Run(new ExcelTask());
        }

        public void Clear_DataTable()
        {
            try
            {          
                foreach (DataGridViewRow row in dgv_dt1.Rows)
                {
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        cell.Value = "";
                    }
                }
            }
            catch (Exception th)
            {
                //抓錯誤訊息
                MessageBox.Show(th.Message, "-Table Clear NG XX !-", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }                 
        }

        //清除 DataGridTable ALL 內容
        private void btn_ClrTable_Click(object sender, EventArgs e)
        {            
            Clear_DataTable();

            lbl_Status.Text = lbl_rdstatus.Text = lbl_rdstatus.Text = lbl_LogTest.Text= "狀態";
            lbl_Status.ForeColor = lbl_rdstatus.ForeColor = Color.Black;
        }
      
        private void btn_linkDB_Click(object sender, EventArgs e)
        {
            lbl_Status.Text = lbl_rdstatus.Text = "狀態";
            lbl_Status.ForeColor = Color.Black;
            
            lbl_listName.Text = "List狀態";
            lbl_listName.ForeColor = Color.Black;

            //預設動作 ckx_selectAll 取消,2016.08.31,Brian
            ckx_selectAll.Checked = false;

            btn_delRow.Enabled = true;
            btn_DeletLog.Enabled = false;
           
            DB_Read_Table1(g_sValAll);

            //預設動作 ckx_selectAll 取消,2016.08.31,Brian
            if (ckx_selectAll.Checked == true)
                ckx_selectAll.Checked = false;
        }

        //刪除本機資料庫(單或多筆資料) 2016.08.03,By Brian
        private void btn_delRow_Click(object sender, EventArgs e)
        {
            if (lbl_listName.Text != "List狀態")
            {
                MessageBox.Show("請讀取DataTable1總表 再選取刪除，TKS!");
                return;
            }
            
            //尋找要刪除的RowID 
            Delete_RowsNumber();            
            // 執行Delete CMD
            g_bDelRow = true;
            DB_Delete(g_sIndexKey, g_sRowId);

            //預設動作 ckx_selectAll 取消,2016.08.31,Brian
            if (ckx_selectAll.Checked == true)
                ckx_selectAll.Checked = false;
        }

        private string WirteFileProperty(string sProperty  ,int count)
        {
            string sWriteLine = "",sEmpty = "~輸入空白值~";
            int iCount = 0; 
          
            // if (g_lsData[g_iSelect].Length > 0)  //判斷資料欄位是有值
            //if (g_lsData[g_iSelect].ToString() != "")  //判斷資料欄位是有值
            if (sProperty .ToString() != "")  //判斷資料欄位是有值            
            {                
                sProperty  = sProperty .Substring(0, sProperty .Length - 1);

                string[] sInfoAll = sProperty .Split('*');
             
                sWriteLine += "已寫入檔案:";

                foreach (var sFile in sInfoAll)
                {        
                    //最多顯示顯示3筆，其餘...2016.08.30,Brian
                    if (iCount <= 1)
                    {
                        sWriteLine += sFile + ',';                    
                    }
                   
                    iCount++;                                    
                }
                sWriteLine = sWriteLine.Substring(0, sWriteLine.Length - 1);
                sWriteLine += " ..等" + Convert.ToString(count) + "筆 (" + System.DateTime.Now.ToShortDateString() + System.DateTime.Now.ToShortTimeString() + ")";

                lbx_AddRowInfo.Items.Add(sWriteLine);

                return sWriteLine;
            }            
            return sEmpty;               
        }

        private void Delete_RowsNumber()
        {
            char[] cSpace = { ' ' };
            string sId1 = "", sFile = "";
            int iSelectNum = 0;  
            
            foreach (DataGridViewRow row in dgv_dt1.SelectedRows)
            {
                //刪除所選取Row
                try
                {
                    if (row.Cells[0].Value != null && row.Cells[1].Value != null && row.Index >= 0)
                    {
                        sId1 += row.Cells[0].Value.ToString() + ",";

                        //只顯示已刪除兩筆資料,2016.08.31,Brian
                        if (iSelectNum <= 1)
                        {
                            sFile += row.Cells[1].Value.ToString() + ",";
                        }

                        iSelectNum++;
                    }
                    g_sRowId = sId1.Substring(0, sId1.Length - 1);

                    g_sFileTotal = sFile.Substring(0, sFile.Length - 1);                  
                  
                    //sId1.TrimEnd(cSpace);        
                }
                catch (Exception ex)
                {
                    //抓錯誤訊息
                    MessageBox.Show(ex.Message, "Delete_RowsNumber 警告!!!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }
		     
        private void btn_InsertDB_Click(object sender, EventArgs e)
        {       
            if (lbl_Status.Text == "*OK*")
            {                
                if (dgv_dt1.RowCount <= 0)
                {
                    MessageBox.Show("DataTable已經無Row資料,請再次GetFile !!!");
                    return;
                }
                //新增DataLog 的Insert CMD ，2016.08.19，By Brian    
                btn_InsertDB.Enabled = false;   //按鈕功能執行不讓再次干擾，所以Enable = False，2016.08.30，Brian                
                DB_SelectInsertf2(g_sVal2All);
            }
            else
            {
                MessageBox.Show("請GetFile !!!");
                return;
            }
            //按鈕功能重新執行，所以Enable = True，2016.08.30，Brian               
            btn_InsertDB.Enabled = true;

            //預設動作 ckx_selectAll 取消,2016.08.31,Brian
            if (ckx_selectAll.Checked == true)
                ckx_selectAll.Checked = false;
        }

        //當datatable 選取Row 時的事件
        private void dgv_dt1_SelectionChanged(object sender, EventArgs e)
        {                  
            tbx_dirpath.Text = "";
            int iSelectRCount = dgv_dt1.Rows.GetRowCount(DataGridViewElementStates.Selected) , iNum = 0 ;
            int iIndex;
            int[] iId = new int[iSelectRCount];
            string sParam="";
            g_sIdSelect = "";
           
            g_iFile = iSelectRCount;

            foreach (DataGridViewRow row in dgv_dt1.SelectedRows)
            {                
                iIndex = row.Index;
              
                row.Selected = true;

                try
                {

                    if (dgv_dt1.Rows[iIndex].Cells[0].Value.ToString() != null)
                    {

                       iId[iNum] = iIndex;

                        //透過 g_sIdSelect 收集 ID 數字,2016.08.17,By Brian
                       sParam += Convert.ToString(iId[iNum]) + '-';
                       g_sIdSelect = sParam.Substring(0, sParam.Length - 1);
                       iNum++;
                    }                  
                }
                catch (Exception ex)
                {
                    //抓錯誤訊息
                    MessageBox.Show(ex.Message, "Rows行數範圍選取超過!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }                        
            }

            for (int iInser = 0; iInser < iSelectRCount; iInser++)
            {
                    for (int col = 0; col < dgv_dt1.ColumnCount; col++)
                    {
                        //索引每行 Row 的 對應 Column 值 

                        sParam = dgv_dt1.Rows[iId[iInser]].Cells[col].Value.ToString();
                        
                        //"序列:", "檔案名稱:", "大小(KB):", "存取日期:", "VER:" ,"路徑"
                        SelectNumValue(col, sParam);
                    }                            
            }


            g_fsView = lbl_ROwNum.Font.Style;
            g_fmInfo = new FontFamily(lbl_ROwNum.Font.Name);
            g_ftType = new Font(g_fmInfo,14, g_fsView);
            lbl_ROwNum.Font = g_ftType;
            lbl_ROwNum.ForeColor = Color.DarkSlateBlue;

            lbl_ROwNum.Text =  Convert.ToString(iSelectRCount) ;
             
        }

        private void ckx_selectAll_CheckedChanged(object sender, EventArgs e)
        {                             
            if (ckx_selectAll.Checked)
            {               
                dgv_dt1.SelectAll();               
            }
            else
            {
                //clear 所有 選取Rows 
                dgv_dt1.ClearSelection();
            }
            
        }

        //Read Item convert to Datatable,2016.08.17 ,Brian
        private void lbx_AddRowInfo_SelectedIndexChanged(object sender, EventArgs e)
        {
           //選到的檔名
           string s_Select = lbx_AddRowInfo.Text.ToString();

           // Find the string in ListBox2.
           int iIndex = lbx_AddRowInfo.FindString(s_Select);

            string[] sSFile = new string[g_iTimes2];
            string[] sInsertLog = new string[g_iTimes2];

            string[] sSortLog = new string[g_iTimes2];
            
            for (int k = 0; k < sSFile.Count(); k++)   
            {
                sSFile[k] = g_lsSaveFile[k];                           
            }

            
            for (int t = 0; t < sInsertLog.Count(); t++)   
            {
                sInsertLog[t] = g_lsDataGroup[t];                           
            }


            for (int iNum = 0; iNum < lbx_AddRowInfo.Items.Count; iNum++)
            {               
                //方法:讀取資料庫透過群組條件式來做顯示，2016.08.23，By Brian
                if (sInsertLog[iNum].StartsWith("Group:") == true && iNum == iIndex)  //找到要的檔名*.xx                                     
                {                 
                    string sGroupNName = sInsertLog[iNum].ToString();

                    string sSortNewID = g_lsIdIndexSelect[iNum].ToString();

                    //從資料庫搜尋符合-> "sGroupNName"並顯示DataTable
                    ListBox_ChangeIndex_GetTable(sGroupNName, sSortNewID);

                    //新增標記:2016.08.30,Brian
                    lbl_listName.ForeColor = Color.DarkGreen;
                    lbl_listName.Text = "List: (" + Convert.ToString(iNum)+")";
                }                            
            }                      
        }

        //當IndexChange就執行以下函式去做取值的動作,2016.08.26,Brian
        public void ListBox_ChangeIndex_GetTable(string sGroupFind,string sSortID)
        {
            string s_cmd;
            string s_sqlcon = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + g_sBaseMdbFile + g_sMdbName + ".mdb;Persist Security Info=True";
         
            //測試OK!
            s_cmd = "SELECT * FROM " + g_sTableNameLog + " WHERE  FileGroup = " + "'" + sGroupFind + "'" + " AND FileID IN " + "(" + sSortID +")";
          
            OleDbConnection icn = new OleDbConnection();
            icn.ConnectionString = s_sqlcon;

            if (icn.State == ConnectionState.Open) icn.Close();

            try
            {
                icn.Open();

                OleDbCommand cmd = new OleDbCommand(s_cmd, icn);
                cmd.CommandText = s_cmd;
                cmd.CommandTimeout = 600;
                cmd.CommandType = CommandType.Text;

                OleDbDataReader oleDr = cmd.ExecuteReader(); //從ole讀取數據
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
                    }
                    //將讀出的行資料增表的行中
                    dt.Rows.Add(s_subitems);
                }


                dgv_dt1.DataSource = dt;
          
                oleDr.Close();
                cmd.Dispose();
                icn.Close();               
            }
            catch (Exception cp)
            {
                //抓錯誤訊息
                MessageBox.Show(cp.Message, "ListBox_ChangeIndex_GetTable 警告!!!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                throw cp;
            }
        }

        private void btn_readLog_Click(object sender, EventArgs e)
        {
            lbl_listName.Text = "List狀態";
            lbl_listName.ForeColor = Color.Black;

            //預設動作 ckx_selectAll 取消,2016.08.31,Brian
            ckx_selectAll.Checked = false;

            btn_DeletLog.Enabled = true;
            btn_delRow.Enabled = false;

            DB_ReadLog(g_sVal2All);

            //預設動作 ckx_selectAll 取消,2016.08.31,Brian
            if (ckx_selectAll.Checked == true)
                ckx_selectAll.Checked = false;
        }

        //新增->刪除DataLog按鈕,2016.08.30,Brian
        private void btn_DeletLog_Click(object sender, EventArgs e)
        {

            if (lbl_listName.Text != "List狀態")
            {
                MessageBox.Show("請讀取DataLog總表 再選取刪除，TKS!");
                return;
            }

            //尋找要刪除的RowID 
            Delete_RowsNumber();
            // 執行Delete CMD
            g_bDelRow = false;
            DB_Delete(g_sIndexKey, g_sRowId);

            //使用迭代做兩次刪除才可真正刪除ListBox的Item,2016.09.01,Brian
            IterationRemoveItem(lbx_AddRowInfo);

            //將群組和ID 從新歸零,2016.09.01,Brian
            g_lsDataGroup = new List<string>();
            g_lsIdIndexSelect = new List<string>();

            //再次Insert 資料庫 重 0 開始,2016.09.01,Brian
            g_iTimes2 = 0;

            //預設動作 ckx_selectAll 取消,2016.08.31,Brian
            if (ckx_selectAll.Checked == true)
                ckx_selectAll.Checked = false;
        }

        //使用迭代做兩次刪除,2016.09.01,Brian
        private void IterationRemoveItem(ListBox listbox)
        {
            for (int i = 0; i < listbox.Items.Count; i++)
            {
                lbx_AddRowInfo.Items.RemoveAt(i);
            }
            for (int j = 0; j < listbox.Items.Count; j++)
            {
                IterationRemoveItem(listbox);
            }
        }

        //登入函式 2016.08.31,Brian
        private void lbl_ProjectTitle_DoubleClick(object sender, EventArgs e)
        {
            //全部顯示出
            lbl_Password.Visible = tbx_Password.Visible = btn_Confirm.Visible = true;
            tbx_Password.BackColor = Color.GreenYellow;

            tbx_Password.MaxLength = 5;
                     
        }
 
        private void btn_Confirm_Click(object sender, EventArgs e)
        {            
            //新增密碼機制 default ="13579"
            if (tbx_Password.Text == "13579")
            {
                lbl_DataTable1All.Visible = btn_linkDB.Visible = lbl_rdstatus.Visible = btn_delRow.Visible = btn_DeletLog.Visible = true;
                tbx_Password.ReadOnly = true;                
                MessageBox.Show("@PASS -> DataTable1權限開放");  
            }
            else
            {
                MessageBox.Show(" 登入NG Fail !");            
            }              
        }
    }             
}
  

