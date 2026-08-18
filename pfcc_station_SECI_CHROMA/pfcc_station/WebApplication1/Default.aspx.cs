using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Data;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;

namespace WebApplication1
{
    public partial class _Default : Page
    {
        List<string> g_csvFile = new List<string>();
        List<string> g_pfcctype = new List<string>();
        List<string> g_ThreadNotOkFile = new List<string>();
        List<string> g_batterycell_number = new List<string>();
        List<string> g_Batt_Classtype;
        List<string> g_NG_PFCC_File;
        List<string> g_ERROR_STATUS;
        List<int>    g_OnlyExist_ModleID_Number;
        List<string> g_Modle_CC_Kvalue;
        

        bool timerunheck = false;
        public static  readonly object _lock = new object();
        public static readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        // public String SourceFolder = @"C:\source_testXXX";
         public String SourceFolder = @"C:\copy_temp\source_pfcc";
        //測試用
       // public String SourceFolder = @"C:\copy_temp\source_pfcc_test";
        public String ResultTaskFolder = @"C:\pf-cc-result";
        public String NGTThread_filepath = @"C:\pf-cc-result\ng_output.txt"; // 自動執行有NG存取指定檔案路徑
        public String NG_file_record = @"C:\pf-cc-result\ng_record.txt"; // 清除既定完成數據清除指定檔案路徑讀取

        //測試NG 存放路徑資料夾
        public String NG_file_Path = @"C:\copy_temp\pf-cc-testNG";

        //錯誤狀態紀錄error_record.txt放置路徑
        public String NG_STARUS_record = @"C:\copy_temp\pf-cc-testNG\error_record.txt";
        public String NG_STARUS_record_copy_Z = @"Z:\pf-cc-result_NG";


        // public String NGTThread_filepath = @"C:\pf-cc\ng_output.txt"; // 自動執行有NG存取指定檔案路徑
        // public String NG_file_record = @"C:\pf-cc\ng_record.txt"; // 清除既定完成數據清除指定檔案路徑讀取

        // public String SourceFolder = @"C:\source_pfcc";
        // public String exec_savepfccbat = @"C:\copy_pfcc_result.bat";

        //MSSQL資料庫各連結參數         
        string MS_Server = "192.168.200.52";
        string MS_Database = "ASRS_HTBI";
        string MS_dbuid = "HTBI_MES";
        string MS_dbpwd = "mes123";

        //計算沒有flag Reached Target voltage 的數量
        int[] step_caculator_value;
         string[] step_abs_value;

        //配方版本
        string sVer = string.Empty;


        // 紀錄沒有K值的索引
        List<int> g_K_serial_NohaveIndexes;

        // 紀錄已經搜尋過的索引
        HashSet<int> g_searcheno_Kclass ;


        protected void Page_Load(object sender, EventArgs e)
        {
            
            //避免Auto時重複初始化
            if (!Page.IsPostBack)
            {
                Button1.Visible = true;
                Button3.Visible = true;
                Btn_Auto.Visible = true;
                Timer1.Interval = 1000;//設定每秒執行一次
                Timer1.Enabled = false;//先關閉計時
                ViewState["time"] = 0;
                Label1.Text = "";                
                Button3.Visible = false;
                // 隱藏 Btn_Auto 按鈕
                Btn_Auto.Visible = false;
            }
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            //Yuping 本機端MYSQL 設定
            //string connection = "server=localhost;user id=root;password=27763923;database=sakila; pooling=true;";
            
            //目前佈署端local host MYSQL 設定
          //  string connection = "server=localhost;user id=root;password=Xcold@246810;database=sakila; pooling=true;Min Pool Size=0;Max Pool Size=3000;";

            //目前開發本機端MYSQL 設定
           string connection = "server=localhost;user id=root;password=K@admin123456;database=sakila; pooling=true;Min Pool Size=0;Max Pool Size=3000;";


            //遠端remote合併 hr.test_mergepfcc MYSQL 設定
            string connection_merge = "server=192.168.3.100;user id=root;password=Admin0331;database=mes; pooling=true;Min Pool Size=0;Max Pool Size=3000;";


            string STR_MSSQL_ARASHTBI = string.Format("server={0};database={1};uid={2};pwd={3};Connect Timeout = 180", MS_Server, MS_Database, MS_dbuid, MS_dbpwd);


            MySqlConnection conn = new MySqlConnection(connection);

            //pf,cc1,cc2工作數據暫訂upload 位置資料夾(SourceFolder)
            // String SourceFolder = "z:\\\\source_pfcc";
            //String SourceFolder = @"Z:\source_pfcc";
            //因佈署後UNC路徑目前無法透過磁區辨別,只能由源頭IP位置找尋
           //  String SourceFolder = @"\\192.168.3.100\hr_tmp\source_pfcc";
            String DestinationFolder = "c:\\\\tempcsv";
            String Filename = "" , pfcc_tablename ="";
            string schema_DB = "sakila";  // 本機PFCC工具資料庫名稱


            String LoadSql = "";

            String dumpcsv = "";
            String merge_sql_var = "" , merge_table_rowdata ="", All_col_listname = "";
            String fileExtension = "csv";
            bool iscsvexist = false;
            bool IsOverWrite = true;
            bool copy_one = true;
            bool mannulrun = true;
            //load 資料

           
            sVer = this.ver_select.SelectedItem.ToString();
            //PF + CC 


            Filename = TextBox1.Text;  //Filename = "H000003_20230910130027.csv"; //讀路徑下的檔案

            //將 Z:\source_pfcc 資料夾內指定Filename複製到 C:\tempcsv
            CopyDirectory(SourceFolder, DestinationFolder, IsOverWrite, copy_one);


            string[] files = Directory.GetFiles(DestinationFolder, $"*.{fileExtension}");
            string DetecFile = DestinationFolder + "\\" + Filename;

            //只對*.csv檔案格式做判斷
            foreach (string file in files)
            {
                if (DetecFile.Equals(file)) {
                    iscsvexist = true;
                    break;
                }
            }

            if (iscsvexist == false) {
                LResult.Text = SourceFolder + " -> 路徑沒有符合目前輸入之("+ Filename + ")檔案/請執行copy_pfcc.bat";
                return;
            }

            //擷取開頭站點字串( pf:K000008 , CC1:H000014   CC2:H000020)
            //pfprocess001  存pf檔  PRIMARY KEY (`ID`,`StartDateD`)
            //processcc 存cc檔(含cc1, cc2)，PRIMARY KEY(`ID`,`StartDateD`)

            String flitersite = Filename.Substring(0, 3);
            String pf_cctable = "";
            String load_select_table = "";

            //由上搜尋站點字串判斷要清除當前一站暫存table內容
            //for pf 
            if (flitersite.Equals("K00") || flitersite.Equals("PF0")) {
                pf_cctable = "pfprocess001";
 
            }// for cc1 或 cc2
            else if (flitersite.Equals("H00") || flitersite.Equals("CC-") || flitersite.Equals("CC0"))
            {
                pf_cctable = "processcc";
            }
            else{
                LResult.Text = "沒有符合此(pf,cc系列)工作項目csv,請在確認!";
                return;
            }


            LResult.Text = "處理表單("+ Filename + ")進行中......";


            LoadSql = "delete from test_loadpfdata003; ";    //刪除暫存TABLE
            LoadSql = LoadSql + "TRUNCATE TABLE " + pf_cctable +"; ";
            //實際路徑是 C:\ProgramData\MySQL\MySQL Server 8.0\Data\test\
            LoadSql = LoadSql + " load data infile 'c:\\\\tempcsv\\\\" + Filename + "' into table test_loadpfdata003 fields terminated by ',' ;";

            //若有Error Code: 1300. Invalid utf8mb4 character string: ,使用下列
            //LoadSql = LoadSql + " LOAD DATA INFILE  'c:\\\\tempcsv\\\\" + Filename + "' INTO  TABLE test_loadpfdata003 " +
            //    "CHARACTER SET utf8mb4 "+  "FIELDS TERMINATED BY ',' "  +
            //    "OPTIONALLY ENCLOSED BY '\"' "+
            //    "LINES TERMINATED BY '\\r\\n' "+
            //    "IGNORE 1 LINES;";

            // LoadSql = LoadSql + " insert into test_loadpfdata003 (fld1) values('"+ Filename + "') ; ";


            //String testSql = insertSql;

            // MySqlConnection conn = new MySqlConnection(connection);
            string fileResult = "1";

            if (conn.State != ConnectionState.Open)
                conn.Open();

            MySqlCommand cmd = new MySqlCommand(LoadSql, conn);
            
            try
            {
                cmd.ExecuteNonQuery();
                LResult.Text = "已完成載檔";
                fileResult = "1";

            }
            catch (Exception ex)
            {
                cmd.Clone();
                conn.Close();
                //LResult.Text = "資料錯誤" + ex.ToString();
                LResult.Text = "上傳資料檔案錯誤" ;
                fileResult = "err";


            }
            conn.Close();

            

            if (fileResult == "1") 
            {
                //總共要塞的欄位
                // insert into pfprocess001()
                //ID,Start dateEnd date,tary ID,	parameter,State,2.8V,2.8V Ah,3.2V,3.2V Ah,3.5V,3.5V Ah,	file name,process,Anlaysis day

                //取固定值---起始日、終止日
                string sqlQuery = "";
            sqlQuery = sqlQuery + "/*title */";
            //sqlQuery = sqlQuery + " (select fld5 as f_title,1 as sort from test_LoadPFData003 LIMIT 9, 1)  /*start_date */ ";
            sqlQuery = sqlQuery + "( select case when (SUBSTRING(fld5, 2, 1) = '/') or (SUBSTRING(fld5, 3, 1) = '/')  then CONVERT(STR_TO_DATE(fld5, '%m/%d/%Y %T'), DATETIME)  else CONVERT(fld5, DATETIME)  end f_title ,1 as sort  from test_LoadPFData003 LIMIT 9, 1 )  ";
            sqlQuery = sqlQuery + "union all ";
            //sqlQuery = sqlQuery + "(select fld5 as f_title,2 as sort from test_LoadPFData003 order by fld5 desc LIMIT 1, 1) /*end_date */ ";
            //sqlQuery = sqlQuery + "union all ";
            sqlQuery = sqlQuery + "( select max(a.result) as f_title,2 as sort from (SELECT    CASE        WHEN(            SELECT COUNT(*)    FROM test_LoadPFData003  WHERE fld5 <> 'PC Time'  AND((SUBSTRING(fld5, 2, 1) = '/')or(SUBSTRING(fld5, 3, 1) = '/'))        ) > 0 ";
            sqlQuery = sqlQuery + " THEN CONVERT(STR_TO_DATE(fld5, '%m/%d/%Y %T'), DATETIME)        ELSE CONVERT(fld5, DATETIME)    END AS result    from test_LoadPFData003    ) a ) /*end_date */ ";
            sqlQuery = sqlQuery + "union all ";
            sqlQuery = sqlQuery + "(select CONCAT ((select fld2 from test_LoadPFData003 LIMIT 3, 1) , '-' , (select fld2 from test_LoadPFData003 LIMIT 0, 1) ) as f_title,3 as sort ) ";
            sqlQuery = sqlQuery + "/*tray_id*/ ";
            sqlQuery = sqlQuery + "union all ";
            sqlQuery = sqlQuery + "(select fld2 as f_title,4 as sort from test_LoadPFData003 LIMIT 2, 1)  /*parameter*/ ";
            //sqlQuery = sqlQuery + "c  /*parameter*/ ";
            sqlQuery = sqlQuery + "union all ";
            sqlQuery = sqlQuery + "(select fld2 as f_title,5 as sort from test_LoadPFData003 LIMIT 1, 1)  /*process*/ ";
            sqlQuery = sqlQuery + "union all ";
            sqlQuery = sqlQuery + "(select now() as f_title,6 as sort ) /*Anlaysis day*/ ";

            MySqlCommand comm = new MySqlCommand(sqlQuery, conn);
            if (conn.State != ConnectionState.Open)
                    conn.Open();
            MySqlDataReader dr = comm.ExecuteReader();



            string vStart_date = "";
            string vdateEnd_date = "";
            string vtary_ID = "";
            string vparameter = "";
            string vparameter_chg = "";
             

            string vparameter_All = "";
            string vprocess = "";

            string sort_temp = "";

            bool check_cc2_algorithm = false , haveTargetvoltage = true, insertNg_ack = true;

             //title 列
             if (dr.HasRows)
            {
                //使用Read方法把資料讀進Reader，讓Reader一筆一筆順向指向資料列，並回傳是否成功。
                while (dr.Read())
                {

                    //DataReader讀出欄位內資料的方式，通常也可寫Reader[0]、[1]...[N]代表第一個欄位到N個欄位。
                    //ss += Convert.ToString(dr["city_id"].ToString() + " -> " + dr["city"].ToString() + " -> " + dr["country_id"].ToString() + "\r\n");
                    sort_temp = Convert.ToString(dr["sort"].ToString());
                    switch (sort_temp)
                    {
                        case "1":
                            vStart_date = Convert.ToString(dr["f_title"].ToString());
                            break;
                        case "2":
                            vdateEnd_date = Convert.ToString(dr["f_title"].ToString());
                            break;
                        case "3":
                            vtary_ID = Convert.ToString(dr["f_title"].ToString());
                            break;
                        case "4":
                            vparameter_All = Convert.ToString(dr["f_title"].ToString());

                            //for chroma
                            if (flitersite.Equals("PF0") || flitersite.Equals("CC0")) {

                               vparameter = vparameter_All.Substring(2, 3);
                               vparameter_chg = vparameter_All.Substring(vparameter_All.Length-17);

                                // 當vparameter 為017 -> CC2時,目前下面做記號
                                if ( vparameter.StartsWith("017") && vparameter_chg.Contains("CC2")) {
                                    vparameter_chg = vparameter + "-chromaCC2";
                                 } //當vparameter 為010 -> CC1時,目前下面做記號
                                else if ( vparameter.StartsWith("010") && vparameter_chg.Contains("CC1")) {
                                    vparameter_chg = vparameter + "-chromaCC1";
                                }//當vparameter 為010 -> CC1時,目前下面做記號
                                else if (vparameter.StartsWith("023") && vparameter_chg.Contains("PF"))
                                {
                                    vparameter_chg = vparameter + "-chromaPF";
                                }
                                else {
                                 //其他未定義
                                    vparameter_chg = vparameter ;
                                }
                                
                            } else {
                               // for SECI 
                                vparameter = vparameter_All.Substring(0, 3);

                                //若原檔案站點旗標有異常,這邊先行轉換讓程序能run,視實際狀況
                                //vparameter = "023";
                                vparameter_chg = vparameter_All.Substring((vparameter_All.Length - 9), 4);
                                
                                if (vparameter_chg == "2023")
                                {
                                    vparameter_chg = vparameter;
                                }
                                else
                                {
                                    vparameter_chg = vparameter + "2";
                                }
                            }

                            break;
                        case "5":
                            vprocess = Convert.ToString(dr["f_title"].ToString());
                            break;
                        default:

                            break;
                    }

                }

            }//if (dr.HasRows)


                //每一個 Cell ID有7行 第一筆H~N行
                //H:Ch1_V(V)	I:Ch1_I(A)	J:Ch1_PV(V)	k:Ch1_OV(V)	L:Ch1_Capa(mAh) 	M:Ch1_Wh(Wh)	N:Ch1_Remark 每一組7行所以+7


            int vComID = 8; //comid 如MW2005A53693 第一筆H 欄+7 為第二組 Ch1_V(V)
            int vState = 14; //N 欄 如 ok: Ch1_Remark
            //展36筆
            String tempSql = " select ";
            for (int iFlag = 1; iFlag <= 36; iFlag++)
            {
                tempSql = tempSql + "fld" + vComID + ", fld" + vState + ",";
                vComID = vComID + 7;  //第一筆H 行第8行 +7(每組7行)
                vState = vState + 7;  //第一筆N 行第14行 +7(每組7行)
            }

            tempSql = tempSql.Substring(0, tempSql.Length - 1) + " from test_LoadPFData003 LIMIT 7, 1 "; //因為取標頭只有一列
            // tempSql = tempSql  + " from test_LoadPFData003 LIMIT 7, 1 ";




            dr.Close();

            comm = new MySqlCommand(tempSql, conn);
            dr = comm.ExecuteReader();

            int BattaryID = 8, battary_count = 0, insert_num = 0;
            vComID = 8;
            vState = 14;
            String insertSql = "";



            MySqlCommand comm_detail;
            MySqlDataReader dr_detail;

               



            MySqlConnection conn_detail = new MySqlConnection(connection);
            if (conn_detail.State != ConnectionState.Open)
                    conn_detail.Open();
            insertSql = "";
            string[] stepValue, divValue;

            divValue = new string[] { "", "", "" };
            stepValue = new string[] { "", "", "" };

             //宣告7組 
            step_caculator_value = new int[] {0, 0, 0, 0, 0, 0, 0 };
            step_abs_value = new string[] { "", "", "" };


                //TextBox1.Text = stepValue[1];
                switch (vparameter)  //STEP 
            {
                case "023": //pf ==>'023'
                    stepValue = new string[] { "2", "4", "6" };  //step 
                    divValue = new string[] { "2", "4", "6" };
                    step_abs_value = new string[] { "2", "4", "6" };
                    break;
                case "010":  //cc1
                    stepValue = new string[] { "1", "3", "5" };
                    divValue = new string[] { "1", "3", "5" };
                    step_abs_value = new string[] { "1", "3", "5" };
                    break;
                case "017": //cc2
                        if (vparameter_chg == "0172") //cc2-2 2024
                        {
                            stepValue = new string[] { "1", "3", "7" };
                            divValue = new string[] { "1", "3", "7" };
                            step_abs_value = new string[] { "1", "3", "7" };
                        } else if (vparameter_chg == "017-chromaCC2") //cc2 for chroma 2024開始
                        {
                            stepValue = new string[] { "1", "3", "7" };
                            divValue = new string[] { "1", "3", "7" };
                            step_abs_value = new string[] { "1", "3", "7" };
                        }
                        else  //cc2 2023
                        {
                            stepValue = new string[] { "1", "5", "9" };
                            divValue = new string[] { "1", "5", "9" };
                            step_abs_value = new string[] { "1", "5", "9" };
                        }
                    break;
            }


            string detailVD = "" , detailmAH = "" , detailCurent = "" , like_step = "";
             
            string tableTitleSql = "", columnSql = "", valueSql = "" , detailSelect = "" ;

            string cc1SelectSql = "";



            string VD28 = "", VAHD28 = "", VD32 = "", VAHD32 = "", VD35 = "", VAHD35 = "";            
            string VCCcurrent = "", VOCV = "", VaverageV1 = "", VaverageV2 = "", VaverageV3 = "", Vcharge34V = "";
            string Vcharge345V = "", Vcharge35V = "", Vtime50A = "", VV = "", VV1 = "", VV2 = "";
            string VV3 = "", VV4 = "", VmOhm = "",Vpara = "";

            //判讀碼A 位置1 --start--
            string CC2_interpretcode = "", CC2_position = "";
                //---end---

            //K值 
            string Get_K_Value = "";

            if (dr.HasRows)
            {
                //使用Read方法把資料讀進Reader，讓Reader一筆一筆順向指向資料列，並回傳是否成功。
                while (dr.Read())
                { //應該只有一筆

                        //重新清空存取電芯號碼存取列表
                        g_batterycell_number = new List<string>();
                        // Console.WriteLine("Number of rows returned: " + dr.FieldCount);
                        //開36個電芯號碼搜尋 , 先收集所有電芯號碼modle 
                        for (int ibattary = 1; ibattary <= 36; ibattary++)
                        {
                            string cell_Boxbatt = dr["fld" + BattaryID].ToString();

                            //測試如果沒有查到電芯號或是電芯號目前尚未建MSSQL表搜無---test start--------
                            //if (ibattary == 6 || ibattary == 12 || ibattary == 14 || ibattary == 20 || ibattary == 32)
                            //{
                            //    cell_Boxbatt = "MW2007HXXXXXXX".ToString();                            
                            //}
                            //if (ibattary != 100) cell_Boxbatt = "MW2007HXXXXXXX".ToString();
                            //-------end--------


                            g_batterycell_number.Add(cell_Boxbatt);
                            BattaryID += 7;


                            //Debug用,當有電芯號無充放電數據,這邊做修正讓其他電芯號作分析-----start-------------
                            //  if (cell_Boxbatt.Equals("MW2009A50698") || cell_Boxbatt.Equals("MW2009A50697"))
                            // if (cell_Boxbatt.Equals("MW2010A18986") || cell_Boxbatt.Equals("MW2010A18987"))

                            //if(cell_Boxbatt.Equals("MW2010A52709") ||
                            //    cell_Boxbatt.Equals("MW2011A87235") ||                                
                            //    cell_Boxbatt.Equals("MW2011A87237") ||
                            //    cell_Boxbatt.Equals("MW2011A87238") ||
                            //    cell_Boxbatt.Equals("MW2011A87239") ||
                            //    cell_Boxbatt.Equals("MW2011A87241") ||
                            //    cell_Boxbatt.Equals("MW2011A87244") ||
                            //    cell_Boxbatt.Equals("MW2011A87251") ||
                            //    cell_Boxbatt.Equals("MW2011A87252"))
                            //{

                            //if (cell_Boxbatt.Equals("MW2026B00283") || cell_Boxbatt.Equals("MW2026B00289"))
                            //if (cell_Boxbatt.Equals(""))
                            //{
                            //    Console.WriteLine("第" + ibattary + "個電芯號" + cell_Boxbatt + "不加入分析");
                            //    // g_batterycell_number.Add(cell_Boxbatt);
                            //    BattaryID += 7;
                            //}
                            //else
                            //{
                            //    g_batterycell_number.Add(cell_Boxbatt);
                            //    BattaryID += 7;
                            //}
                            // ------------------------end-------------------------------------------------------
                        }

                        //這邊串接HTBI_K_Value_MapperType2_V 找尋 K_Value 所判定為ClassType所屬英文代號
                        if (vparameter != "023")
                            Sync_HTBI_Merge_Classparam(STR_MSSQL_ARASHTBI, g_batterycell_number);


                        //檢視最後g_Batt_Classtype 存取狀態顯示
                        Console.WriteLine("電芯目前全classtype 36組顯示 = " + string.Join(", ", g_Batt_Classtype , g_Modle_CC_Kvalue));

                        int AllInsert;

                        //計算要insert的實際數量,若有?則跳過不計,針對CC分容站
                        if (vparameter != "023")
                        {
                            AllInsert = calculate_insert_currentNumber(g_Batt_Classtype, vparameter);
                        }
                        else {
                          //AllInsert = g_Modle_CC_Kvalue.Count();       
                          // AllInsert = g_batterycell_number.Count()-1;
                            AllInsert = 36;
                        }
                        

                        Console.WriteLine("電芯無K值英文序號 索引位置 = " +  g_K_serial_NohaveIndexes);

                        if (vparameter != "023")
                        {
                            if (g_K_serial_NohaveIndexes.Count() != 0)
                                g_searcheno_Kclass = new HashSet<int>();
                        }
                       

                        //判定是否為整個tray 等同36
                        bool isOnlyValid = (AllInsert != 36);

                        //判定Kvalue 索引總數量
                        int Kpasslen = (vparameter != "023") ?  36 - g_Modle_CC_Kvalue.Count() :0;

                        //Debug 時,Kpasslen不考慮 設定為0
                         Kpasslen = 0;

                        // AllInsert = 26;

                        int add_count = 0; // 14 -1
                        

                    //開36個insert 
                    //當有第一開頭序號有NG,會先忽略不計,但要補償少做的數量,若閃2顆就要加回2顆                 
                    for (int iFlag = 1; iFlag <= AllInsert + Kpasslen ; iFlag++)                
                    {
                        //初始要閃過的個電芯號序號,依實際狀況做調整----debug用----- start--------
                        //if (isOnlyValid && iFlag < Kpasslen + 1)
                        //{
                        //    //當有要跳過的電芯號數列,這邊需要跳出次數以這邊參考,多增加跳躍7個欄位, 在依照實際跳躍的電芯號數量做判定
                        //    vComID = vComID + 1 * 7;
                        //    vState = vState + 1 * 7;
                        //    continue;
                        //}
                        //-----end--------
                        cc1SelectSql = "select max(a.VD28) VD28, max(a.VAHD28) VAHD28, max(a.VD32) VD32, max(a.VAHD32) VAHD32, max(a.VD35) VD35, max(a.VAHD35) VAHD35 ";
                        cc1SelectSql = cc1SelectSql + ",(select fld" + vComID + " as OCV from test_LoadPFData003 LIMIT 10, 1)  OCV  /*fld做變更*/ ";
                        cc1SelectSql = cc1SelectSql + " , max(a.CCcurrent) CCcurrent ";
                        cc1SelectSql = cc1SelectSql + ",((select sum(cast(fld" + (vComID) + " as decimal)) from test_LoadPFData003 b where b.fld7 = '" + divValue[0] + "' and ABS(b.fld" + (vComID + 1) + ") > '10') / ";
                        cc1SelectSql = cc1SelectSql + "((select sum(cast(fld7 as decimal)) from test_LoadPFData003 b where b.fld7 = '" + divValue[0] + "' and ABS(b.fld" + (vComID + 1) + ") > '10')/ " + divValue[0] + ")) averageV1 ";
                        cc1SelectSql = cc1SelectSql + ",((select sum(cast(fld" + (vComID) + " as decimal)) from test_LoadPFData003 b where b.fld7 = '" + divValue[1] + "' and ABS(b.fld" + (vComID + 1) + ") > '10') / ";
                        cc1SelectSql = cc1SelectSql + "((select sum(cast(fld7 as decimal)) from test_LoadPFData003 b where b.fld7 = '" + divValue[1] + "' and ABS(b.fld" + (vComID + 1) + ") > '10')/ " + divValue[1] + ")) averageV2 ";
                        cc1SelectSql = cc1SelectSql + ",((select sum(cast(fld" + (vComID) + " as decimal)) from test_LoadPFData003 b where b.fld7 = '" + divValue[2] + "' and ABS(b.fld" + (vComID + 1) + ") > '10') / ";
                        cc1SelectSql = cc1SelectSql + "((select sum(cast(fld7 as decimal)) from test_LoadPFData003 b where b.fld7 = '" + divValue[2] + "' and ABS(b.fld" + (vComID + 1) + ") > '10')/ " + divValue[2] + ")) averageV3 ";
                        /*fld12 要做+8 (變數)*/
                        cc1SelectSql = cc1SelectSql + ",(select max(cast(fld" + (vComID + 4) + " as decimal))  from test_LoadPFData003 where fld7 = '3' and fld" + (vComID + 1) + "  > '10' and fld" + (vComID) + "  <= '3.4') as 'charge34V' ";
                        cc1SelectSql = cc1SelectSql + ",(select  max(cast(fld" + (vComID + 4) + "  as decimal))  from test_LoadPFData003 where fld7 = '3' and fld" + (vComID + 1) + "  > '10' and fld" + (vComID) + "  <= '3.45') as 'charge345V' ";
                        cc1SelectSql = cc1SelectSql + ",(select  max(cast(fld" + (vComID + 4) + "  as decimal))   from test_LoadPFData003 where fld7 = '3' and fld" + (vComID + 1) + "  > '10' and fld" + (vComID) + "  <= '3.5') as  'charge35V' ";
                            

                            
                        //detailSelect  是用在 VLOOKUP  如VD28=XLOOKUP(1,(G11:G5000(STEP) =2)*(N11:N5000=JK8[Reached Target voltage] ),H11:H5000(n-6),0,0)  //每個parameter 底層都一樣
                        detailSelect = "from( "
                         + "select fld7, fld8, fld9 ,fld12, fld14, case when fld7 = '" + stepValue[0] + "' /*2*/ then  fld" + (vState - 6) + "  end VD28, case when fld7 = '" + stepValue[0] + "'  /*2*/ then  fld" + (vState - 2) + " end VAHD28 "
                            + ", case when fld7 = '" + stepValue[1] + "' /*4*/  then  fld" + (vState - 6) + "  end VD32, case when fld7 = '" + stepValue[1] + "' then  fld" + (vState - 2) + "  end VAHD32 "
                            + ", case when fld7 = '" + stepValue[2] + "'/*6*/ then  fld" + (vState - 6) + "  end VD35, case when fld7 = '" + stepValue[2] + "' then  fld" + (vState - 2) + "  end VAHD35 "
                            + " ,case when fld7 = '1' then fld" + (vState - 5) + "  end  'CCcurrent' " +
                            " from test_LoadPFData003  where fld" + vState + " = 'Reached Target voltage' ) a ";


                         //若沒有充電電壓flag 這邊用試算方式求出



                        switch (vparameter)
                        {
                             case "023": //pf
                                    sqlQuery = "select max(a.VD28) VD28, max(a.VAHD28) VAHD28, max(a.VD32) VD32, max(a.VAHD32) VAHD32, max(a.VD35) VD35, max(a.VAHD35) VAHD35 " + detailSelect;

                                    /* 變成DetailSelectSql 
                                    sqlQuery = sqlQuery + "from( ";
                                    sqlQuery = sqlQuery + "select fld7, fld8, fld9 ,fld12, fld14, case when fld7 = '2' then  fld" + (vState - 6) + "  end VD28, case when fld7 = '2' then  fld" + (vState - 2) + " end VAHD28 ";
                                    sqlQuery = sqlQuery + ", case when fld7 = '4' then  fld" + (vState - 6) + "  end VD32, case when fld7 = '4' then  fld" + (vState - 2) + "  end VAHD32 ";
                                    sqlQuery = sqlQuery + ", case when fld7 = '6' then  fld" + (vState - 6) + "  end VD35, case when fld7 = '6' then  fld" + (vState - 2) + "  end VAHD35 ";
                                    sqlQuery = sqlQuery + " ,case when fld7 = '1' then fld" + (vState - 5) + "  end  'CCcurrent' ";
                                    sqlQuery = sqlQuery + " from test_LoadPFData003  where fld" + vState + " = 'Reached Target voltage' ) a ";
                                    */
                                    break;
                            case "010":  //cc1
                                //vComID = 8;//H欄     vState = 14;//N欄

                                sqlQuery = cc1SelectSql + detailSelect; //+ " ) finalR ";

                                break;

                            case "017":   //cc2
                         // case "010":  //cc1
                         //   case "023":  //pf

                                 if (vparameter_chg == "0172" || vparameter_chg == "017-chromaCC2" || vparameter_chg =="010-chromaCC1" || vparameter_chg == "0232" || vparameter_chg == "023-chromaPF" || vparameter_chg =="010") //cc2-2 2024 , cc2 017-chroma2 2024開始
                                {

                                        //SECI 走這段解析 V , V1 ,V2,V3,V4 ,育平之前定義的各項目count 總數                                        
                                         if (vparameter_chg == "0172" && check_cc2_algorithm) // for 測試正常                                       
                                       // if (vparameter_chg == "0172")
                                        {
                                            //  jj7 5169 ,  jj8 8395    =(@INDIRECT((ADDRESS($JJ$7, JF14)), 1))                           
                                            cc1SelectSql = cc1SelectSql + ",(SELECT COUNT(*)  FROM test_LoadPFData003 WHERE fld7 = '3' and  cast( fld" + (vComID + 1) + "  as decimal) > 20) as time50A ";
                                            cc1SelectSql = cc1SelectSql + ",(select  fld" + vComID + " from test_LoadPFData003 limit 4976,1 ) as V ";
                                            cc1SelectSql = cc1SelectSql + ",(select  fld" + vComID + " from test_LoadPFData003 limit 5168,1) as V1 ";
                                            cc1SelectSql = cc1SelectSql + ",(select  fld" + vComID + " from test_LoadPFData003 limit 8394,1 ) as V2 ";
                                            cc1SelectSql = cc1SelectSql + ",(select  fld" + (vComID + 1) + " from test_LoadPFData003 limit 5114,1) as v3 ";
                                            cc1SelectSql = cc1SelectSql + ",(select  fld" + (vComID + 1) + " from test_LoadPFData003 limit 5178,1) as v4 ";
                                        }
                                        else // Chroma  走這段解析 V , V1 ,V2,V3,V4 ,這邊根據每個step 與 Reached Target voltage' 條件對應位置 算出count
                                        {
                                            //  jj7 5169 ,  jj8 8395    =(@INDIRECT((ADDRESS($JJ$7, JF14)), 1))                           
                                            cc1SelectSql = cc1SelectSql + ",(SELECT COUNT(*)  FROM test_LoadPFData003 WHERE fld7 = '3' and  cast( fld" + (vComID + 1) + "  as decimal) > 20) as time50A ";
                                                                                       
                                            //計算五次
                                            for (int n = 0; n < 5; n++)
                                            {                                               
                                                int cacula_number = Parse_chroma_V_serial_count(n, vComID, vComID + 1, connection);
                                                if (n <= 2)
                                                {
                                                    if (n == 0)
                                                    {
                                                        cc1SelectSql = cc1SelectSql + ",(select  fld" + vComID + " from test_LoadPFData003 limit " + (cacula_number) + ",1 ) as V ";

                                                    }
                                                    else
                                                    {
                                                        //這邊有遇到演算異常,實際計算的count會overflow = 1,這邊透過-1 下面query才會正常,依實際狀況調整(目前遇到為V2計算量)
                                                        //if (n == 2)
                                                        //{
                                                        //    Console.WriteLine($"第{n}個壓段數量:{cacula_number} 第{insert_num}筆");
                                                        //    cacula_number = cacula_number - 2;
                                                        //}

                                                        //向下微調取到合理值 電壓 電流 參數
                                                       // cacula_number = cacula_number - 150;
                                                        cc1SelectSql = cc1SelectSql + ",(select  fld" + vComID + " from test_LoadPFData003 limit " + (cacula_number) + ",1 ) as V" + (n);
                                                    }
                                                }
                                                else
                                                {
                                                    //於實際驗算的count有落差,因充放電有step步數不一致狀態,這邊予以微調降步數才能索引到實際參數值(電流)
                                                    //if (n == 4 || n == 3)
                                                    //if (n == 4)
                                                    //{
                                                    //    Console.WriteLine($"第{n}個壓段數量:{cacula_number} 第{insert_num}筆");

                                                    //    if (cacula_number > 5000)
                                                    //        cacula_number = cacula_number - 5;

                                                    //    微調步數往前推移擷取
                                                    //    cacula_number = cacula_number - 150;
                                                    //}

                                                    //if (iFlag == 19 || iFlag == 20)
                                                    //    cacula_number = cacula_number - 44;


                                                    cc1SelectSql = cc1SelectSql + ",(select  fld" + (vComID + 1) + " from test_LoadPFData003 limit " + (cacula_number) + ",1 ) as V" + (n) + " ";
                                                }
                                            }


                                            //當原始數據沒有Reached Target voltage參考
                                            if (!haveTargetvoltage) {                                                 
                                                for ( int k = 0 ; k < step_caculator_value.Count(); k++) 
                                                {
                                                    //增加判斷充放電週期有無正常數據流limit count 算出
                                                    int stepCount = int.TryParse(step_caculator_value[k].ToString(), out int val) ? val : 0;

                                                    if (k < 6) {

                                                        if (k%2 == 0 || k==0)
                                                        {
                                                            //取VD 2.8, 3.2, 3.5 
                                                            if (k == 0) {
                                                                detailVD = "absVD28";
                                                                like_step = step_abs_value[0];
                                                            } else if (k == 2)
                                                            {
                                                                detailVD = "absVD32";
                                                                like_step = step_abs_value[1];
                                                            } else if (k == 4) {
                                                                detailVD = "absVD35";
                                                                like_step = step_abs_value[2];
                                                            }
                                                                                                                      
                                                            //這邊增加機制防止crash(針對週期有少的情況)
                                                            if ( stepCount < 3)
                                                                step_caculator_value[k] = 3; 

                                                            cc1SelectSql = cc1SelectSql + ",( select abs(fld" + vComID + ")  from test_LoadPFData003 WHERE fld7 LIKE '" + like_step + "' limit " + (step_caculator_value[k]-3) + " ,1 ) as "+ detailVD + "" ;

                                                        }
                                                        else {
                                                            //取mAH 2.8, 3.2, 3.5                                                             
                                                            if (k == 1)
                                                            {
                                                                detailmAH = "absmAH28";
                                                                like_step = step_abs_value[0];
                                                            }
                                                            else if (k == 3)
                                                            {
                                                                detailmAH = "absmAH32";
                                                                like_step = step_abs_value[1];
                                                            }
                                                            else if (k == 5)
                                                            {
                                                                detailmAH = "absmAH35";
                                                                like_step = step_abs_value[2];
                                                            }

                                                            //這邊增加機制防止crash(針對週期有少的情況)
                                                            if (stepCount < 3)
                                                                step_caculator_value[k] = 3;

                                                            cc1SelectSql = cc1SelectSql + ",( select abs(fld" + (vComID+4) + ")  from test_LoadPFData003 WHERE fld7 LIKE '" + like_step + "' limit " + (step_caculator_value[k] - 3) + " ,1 ) as " + detailmAH + "";
                                                        }

                                                    } 
                                                    else {
                                                        //取current 電流                                                          
                                                        detailCurent = "absCurrentmA";

                                                        //這邊增加機制防止crash(針對週期有少的情況)
                                                        if (stepCount == 0)
                                                            step_caculator_value[k] = 1;

                                                        cc1SelectSql = cc1SelectSql + ",( select abs(fld" + (vComID + 1) + ")  from test_LoadPFData003 WHERE fld7 LIKE '1' limit " + (step_caculator_value[k] - 1) + " ,1 ) as " + detailCurent + " " ;
                                                    }
                                                }

                                            }

                                        }


                                        sqlQuery = cc1SelectSql + detailSelect; //+ " ) finalR ";

                                }
                                else  //cc2 2023
                                {
                                    //jj7 4893,jj8 4957  =(@INDIRECT((ADDRESS($JJ$7,JF14)),1))
                                    cc1SelectSql = cc1SelectSql + ",(SELECT COUNT(*)  FROM test_LoadPFData003 WHERE fld7 = '3' and  cast( fld" + (vComID + 1) + "  as decimal) > 20) as time50A ";
                                    cc1SelectSql = cc1SelectSql + ",(select  fld" + vComID + " from test_LoadPFData003 limit 492,1 ) as V ";
                                    cc1SelectSql = cc1SelectSql + ",(select  fld" + vComID + " from test_LoadPFData003 limit 4892,1) as V1 ";
                                    cc1SelectSql = cc1SelectSql + ",(select  fld" + vComID + " from test_LoadPFData003 limit 4956,1 ) as V2 ";
                                    cc1SelectSql = cc1SelectSql + ",(select  fld" + (vComID + 1) + " from test_LoadPFData003 limit 4838,1) as v3 ";
                                    cc1SelectSql = cc1SelectSql + ",(select  fld" + (vComID + 1) + " from test_LoadPFData003 limit 4902,1) as v4 ";

                                    sqlQuery = cc1SelectSql + detailSelect; //+ " ) finalR ";

                                }

                                break;

                        }//end switch


                       
                        comm_detail = new MySqlCommand(sqlQuery, conn_detail);

                        dr_detail = comm_detail.ExecuteReader();

                        if (dr_detail.HasRows)  //找FUNCTION的值
                        {
                            while (dr_detail.Read())
                            {
                                    //PF

                                    /*cc 新增的欄位*/
                                    //,`CCcurrent`,`OCV`,`averageV1`,`averageV2`,`averageV3`
                                    //,`charge34V`,`charge345V`,`charge35V`,`time50A`,`v`
                                    //,`v1`,`v2`,`v3`,`v4`,`Para`
                                    //mOhm 欄位 ABS(KD16-KE16)/ABS(KF16-KG16)*1000 取得欄位後     Math.Abs();


                                    //string svd28 = Convert.ToString(dr_detail["absVD28"].ToString());
                                    //string smaH28 = Convert.ToString(dr_detail["absmAH28"].ToString());
                                    //string svd32 = Convert.ToString(dr_detail["absVD32"].ToString());
                                    //string smaH32 = Convert.ToString(dr_detail["absmAH32"].ToString());
                                    //string svd35 = Convert.ToString(dr_detail["absVD35"].ToString());
                                    //string smaH35 = Convert.ToString(dr_detail["absmAH35"].ToString());


                                    if (!haveTargetvoltage  && (vparameter == "017" || vparameter == "023"))
                                    {
                                        //不存入NG電芯
                                        if (!insertNg_ack)
                                        {
                                            VD28 = Convert.ToString(dr_detail["absVD28"].ToString());
                                            VAHD28 = Convert.ToString(dr_detail["absmAH28"].ToString());
                                            VD32 = Convert.ToString(dr_detail["absVD32"].ToString());
                                            VAHD32 = Convert.ToString(dr_detail["absmAH32"].ToString());
                                            VD35 = Convert.ToString(dr_detail["absVD35"].ToString());
                                            VAHD35 = Convert.ToString(dr_detail["absmAH35"].ToString());
                                        }
                                        else {
                                            VD28 = ToNullORVALUE_CheckString(dr_detail["absVD28"]);
                                            VAHD28 = ToNullORVALUE_CheckString(dr_detail["absmAH28"]);
                                            VD32 = ToNullORVALUE_CheckString(dr_detail["absVD32"]);
                                            VAHD32 = ToNullORVALUE_CheckString(dr_detail["absmAH32"]);
                                            VD35 = ToNullORVALUE_CheckString(dr_detail["absVD35"]);
                                            VAHD35 = ToNullORVALUE_CheckString(dr_detail["absmAH35"]);
                                        }
                                    }
                                    else {
                                        //不存入NG電芯
                                        if (!insertNg_ack) {
                                            VD28 = Convert.ToString(dr_detail["VD28"].ToString());
                                            VAHD28 = Convert.ToString(dr_detail["VAHD28"].ToString());
                                            VD32 = Convert.ToString(dr_detail["VD32"].ToString());
                                            VAHD32 = Convert.ToString(dr_detail["VAHD32"].ToString());
                                            VD35 = Convert.ToString(dr_detail["VD35"].ToString());
                                            VAHD35 = Convert.ToString(dr_detail["VAHD35"].ToString());
                                        }                                       
                                        else {
                                            VD28 = ToNullORVALUE_CheckString(dr_detail["VD28"]);
                                            VAHD28 = ToNullORVALUE_CheckString(dr_detail["VAHD28"]);
                                            VD32 = ToNullORVALUE_CheckString(dr_detail["VD32"]);
                                            VAHD32 = ToNullORVALUE_CheckString(dr_detail["VAHD32"]);
                                            VD35 = ToNullORVALUE_CheckString(dr_detail["VD35"]);
                                            VAHD35 = ToNullORVALUE_CheckString(dr_detail["VAHD35"]);
                                        }                                                                               
                                    }



                                Console.WriteLine($"3.5-2.8V Ah 電容量 =  { VAHD35}");

                                switch (vparameter)
                                {
                                    case "023": //pf
                                        if (g_Modle_CC_Kvalue != null &&  g_Modle_CC_Kvalue.Count() != 0)
                                        {
                                            if (AllInsert != 0 && iFlag - 1 < AllInsert )
                                            {
                                                    //有空Kvalue 序號從0開始
                                                    //if (iFlag >  Kpasslen)
                                                    //    Get_K_Value = g_Modle_CC_Kvalue[iFlag - Kpasslen - 1].ToString();
                                                    //else
                                                    //    Get_K_Value = "";

                                                    //有空Kvalue 從g_Modle_CC_Kvalue.Count()+1 序號開始
                                                    //if (iFlag > g_Modle_CC_Kvalue.Count())
                                                    //    Get_K_Value = "";
                                                    //else
                                                    //    Get_K_Value = g_Modle_CC_Kvalue[iFlag - 1].ToString();

                                                    // 有少classtype 字元 , Kpasslen加指定位置計算
                                                    //if (iFlag > AllInsert - Kpasslen )
                                                    //    Get_K_Value = "";
                                                    //else
                                                    //    Get_K_Value = g_Modle_CC_Kvalue[iFlag - 1].ToString();

                                                    //正常INSERT                                                  
                                                    // Get_K_Value = g_Modle_CC_Kvalue[iFlag - 1].ToString();                                                   
                                             }
                                             else
                                                  Get_K_Value = "";
                                        }
                                        else
                                             Get_K_Value = "";

                                            break;
                                    case "010": //cc1                                     
                                        VCCcurrent = Convert.ToString(dr_detail["CCcurrent"].ToString());                                                                                       
                                        //VOCV = Convert.ToString(dr_detail["OCV"].ToString());
                                        //VaverageV1 = Convert.ToString(dr_detail["averageV1"].ToString());
                                        //VaverageV2 = Convert.ToString(dr_detail["averageV2"].ToString());
                                        //VaverageV3 = Convert.ToString(dr_detail["averageV3"].ToString());
                                        //Vcharge34V = Convert.ToString(dr_detail["charge34V"].ToString());
                                        //Vcharge345V = Convert.ToString(dr_detail["charge345V"].ToString());
                                        //Vcharge35V = Convert.ToString(dr_detail["charge35V"].ToString());

                                        //不存入NG電芯
                                        if (!insertNg_ack)
                                        {
                                            VOCV = Convert.ToString(dr_detail["OCV"].ToString());
                                            VaverageV1 = Convert.ToString(dr_detail["averageV1"].ToString());
                                            VaverageV2 = Convert.ToString(dr_detail["averageV2"].ToString());
                                            VaverageV3 = Convert.ToString(dr_detail["averageV3"].ToString());
                                            Vcharge34V = Convert.ToString(dr_detail["charge34V"].ToString());
                                            Vcharge345V = Convert.ToString(dr_detail["charge345V"].ToString());
                                            Vcharge35V = Convert.ToString(dr_detail["charge35V"].ToString());
                                        }
                                        else
                                        {
                                            VOCV = ToNullORVALUE_CheckString(dr_detail["OCV"]);
                                            VaverageV1 = ToNullORVALUE_CheckString(dr_detail["averageV1"]);
                                            VaverageV2 = ToNullORVALUE_CheckString(dr_detail["averageV2"]);
                                            VaverageV3 = ToNullORVALUE_CheckString(dr_detail["averageV3"]);
                                            Vcharge34V = ToNullORVALUE_CheckString(dr_detail["charge34V"]);
                                            Vcharge345V = ToNullORVALUE_CheckString(dr_detail["charge345V"]);
                                            Vcharge35V = ToNullORVALUE_CheckString(dr_detail["charge35V"]);
                                        }

                                        //if (iFlag - 13 <= g_Modle_CC_Kvalue.Count())
                                        if (g_Modle_CC_Kvalue.Count() != 0)
                                        {
                                                if (AllInsert != 0 && iFlag - 1 < g_Modle_CC_Kvalue.Count() + Kpasslen)
                                                    Get_K_Value = g_Modle_CC_Kvalue[iFlag - Kpasslen - 1].ToString();
                                                else
                                                    Get_K_Value = "";
                                        }

                                        //目前CHROMA 數據有問題  CHX_I(A) 都是負值,條件式需要大於10 , Current 目前因 Reached Target voltage無故無法收驗找到相對應值                           
                                        if (VCCcurrent.ToString() == "" || string.IsNullOrEmpty(VaverageV1) || string.IsNullOrEmpty(VaverageV3))
                                        {
                                            VCCcurrent = VaverageV1 = VaverageV3 = "0.0";
                                        }

                                        Vtime50A = "0";
                                        VV = "0";
                                        VV1 = "0";
                                        VV2 = "0";
                                        VV3 = "0";
                                        VV4 = "0";
                                        VmOhm = "0";
                                        Vpara = "CC1";


                                        break;
                                    case "017":  //cc2 or cc2-2 or cc2-chroma2
                                        bool check_K_nohave = false;

                                        if (!haveTargetvoltage)
                                        {
                                            VCCcurrent = Convert.ToString(dr_detail["absCurrentmA"].ToString());
                                        }
                                        else
                                        {
                                            VCCcurrent = Convert.ToString(dr_detail["CCcurrent"].ToString());
                                        }

                                        //不存入NG電芯
                                        if (!insertNg_ack)
                                        {
                                            VOCV = Convert.ToString(dr_detail["OCV"].ToString());
                                            VaverageV1 = Convert.ToString(dr_detail["averageV1"].ToString());
                                            VaverageV2 = Convert.ToString(dr_detail["averageV2"].ToString());
                                            VaverageV3 = Convert.ToString(dr_detail["averageV3"].ToString());
                                            Vcharge34V = Convert.ToString(dr_detail["charge34V"].ToString());
                                            Vcharge345V = Convert.ToString(dr_detail["charge345V"].ToString());
                                            Vcharge35V = Convert.ToString(dr_detail["charge35V"].ToString());
                                        }
                                        else {
                                            VOCV = ToNullORVALUE_CheckString(dr_detail["OCV"]);
                                            VaverageV1 = ToNullORVALUE_CheckString(dr_detail["averageV1"]);
                                            VaverageV2 = ToNullORVALUE_CheckString(dr_detail["averageV2"]);
                                            VaverageV3 = ToNullORVALUE_CheckString(dr_detail["averageV3"]);
                                            Vcharge34V = ToNullORVALUE_CheckString(dr_detail["charge34V"]);
                                            Vcharge345V = ToNullORVALUE_CheckString(dr_detail["charge345V"]);
                                            Vcharge35V = ToNullORVALUE_CheckString(dr_detail["charge35V"]);
                                        }
                                       
                                        int cap_type = Assign_Cap_mAH_Type(VAHD35);
                                        
                                        insert_num = isOnlyValid ? g_OnlyExist_ModleID_Number[iFlag - Kpasslen -1 ] : insert_num;

                                        //若遇到K值無英文代碼目前做法是忽略,其餘照舊判斷
                                        //if ( iFlag >= 14 && iFlag <= 16)
                                        //{
                                        //    Get_K_Value = "";
                                        //}                                           
                                        //else
                                        //{
                                        //        string check_kval = "";
                                        //        //if (iFlag >= 8 && iFlag <= 19)
                                        //        //{
                                        //        //     check_kval = g_Modle_CC_Kvalue[iFlag- 8].ToString();

                                        //        //} else if (iFlag >=25 ) {
                                        //        //    add_count++;
                                        //        //    check_kval = g_Modle_CC_Kvalue[add_count].ToString();
                                        //        //}

                                        //        if (iFlag < 14 )
                                        //        {
                                        //            check_kval = g_Modle_CC_Kvalue[iFlag -1].ToString();

                                        //        }
                                        //        else if (iFlag >=17)
                                        //        {
                                        //            add_count++;
                                        //            check_kval = g_Modle_CC_Kvalue[add_count].ToString();
                                        //        }

                                        //        Get_K_Value = check_kval;
                                        //        Console.WriteLine("check_kval:" + Get_K_Value);
                                        //        //Get_K_Value = isOnlyValid ? g_Modle_CC_Kvalue[iFlag - Kpasslen - 1].ToString() : g_Modle_CC_Kvalue[iFlag - 1].ToString();
                                        //}

                                        //有K值找無NotFind狀況已下判斷                                    
                                        for (int idx = 0; idx < g_K_serial_NohaveIndexes.Count(); idx++) {

                                            // 跳過已經搜尋過無K英文別名稱的索引位置
                                            if (g_searcheno_Kclass.Contains(g_K_serial_NohaveIndexes[idx]))
                                            {
                                                continue;
                                            }

                                            //當有符合序號(未找到K 序號),0要額外再加判斷
                                            if (iFlag-1  == g_K_serial_NohaveIndexes[idx] ) {
                                                // 記錄已經搜尋過的
                                                g_searcheno_Kclass.Add(g_K_serial_NohaveIndexes[idx]);
                                                Get_K_Value = "";
                                                add_count++;
                                                check_K_nohave = true;
                                                break;
                                            }
                                        }
                                            
                                        //當狀態為其他有序號
                                        if (g_K_serial_NohaveIndexes.Count() != 0 && !check_K_nohave) {                                                
                                                Get_K_Value = g_Modle_CC_Kvalue[iFlag- add_count-1].ToString();
                                        }

                                       //正常36顆電芯走下方     
                                        if (g_K_serial_NohaveIndexes.Count() == 0) {                                                                                 
                                            Get_K_Value = isOnlyValid ? g_Modle_CC_Kvalue[iFlag - Kpasslen - 1].ToString() : g_Modle_CC_Kvalue[iFlag - 1].ToString();
                                        }

                                        CC2_interpretcode = Convert.ToString(g_Batt_Classtype[insert_num]) + cap_type.ToString("D2");

                                        int check_position = Determination_Type_Position(g_Batt_Classtype[insert_num], cap_type);

                                        CC2_position = Convert.ToString(check_position);

                                        //目前CHROMA 數據有問題  CHX_I(A) 都是負值,條件式需要大於10 , Current 目前因 Reached Target voltage無故無法收驗找到相對應值
                                        if (VCCcurrent.ToString() == "" || string.IsNullOrEmpty(VaverageV1) || string.IsNullOrEmpty(VaverageV3))
                                        {
                                            VCCcurrent = VaverageV1 = VaverageV3 = "0.0";
                                        }

                                        //不存入NG電芯
                                        if (!insertNg_ack)
                                        {
                                            Vtime50A = Convert.ToString(dr_detail["time50A"].ToString());
                                            VV = Convert.ToString(dr_detail["V"].ToString());
                                            VV1 = Convert.ToString(dr_detail["V1"].ToString());
                                            VV2 = Convert.ToString(dr_detail["V2"].ToString());
                                            VV3 = Convert.ToString(dr_detail["V3"].ToString());
                                            VV4 = Convert.ToString(dr_detail["V4"].ToString());
                                        }
                                        else {
                                            Vtime50A = Convert.ToString(dr_detail["time50A"].ToString());
                                            VV = ToNullORVALUE_CheckString(dr_detail["V"]);
                                            VV1 = ToNullORVALUE_CheckString(dr_detail["V1"]);
                                            VV2 = ToNullORVALUE_CheckString(dr_detail["V2"]);
                                            VV3 = Convert.ToString(dr_detail["V3"].ToString());                                            
                                            VV4 = ToNullORVALUE_CheckString(dr_detail["V4"]);
                                        }

                                            //if (iFlag == 28 || iFlag == 29 || iFlag == 27 || iFlag == 25  || iFlag == 30 || iFlag == 31 || iFlag == 34)
                                            //{
                                            //    string modle_ID = g_batterycell_number[iFlag - 1].ToString();
                                            //    Console.WriteLine("模組ID:" + modle_ID);
                                            //    Console.WriteLine("VV1 =" + VV1);
                                            //    Console.WriteLine("VV2 =" + VV2);
                                            //    Console.WriteLine("VV3 =" + VV3);
                                            //    Console.WriteLine("VV4 =" + VV4);
                                            //}


                                            //=ABS(KD14-KE14)/ABS(KF14-KG14)*1000

                                            //當擷取V3數值為空
                                            if (VV3=="" ||  VV3 !="0.0") {
                                                VV3 = "0.0";
                                        }

                                        //安全浮點判斷
                                        if (double.TryParse(VV4, out double v4Value))
                                        {
                                            if (Math.Abs(v4Value) < 1e-6) // 代表幾乎是 0
                                            {
                                                VV4 = null;
                                            }
                                        }

                                        Vpara = "CC2";
                                            //Decimal divisor = Math.Abs(Convert.ToDecimal(VV3) - Convert.ToDecimal(VV4));
                                            //if (divisor == 0) divisor = 0.0039M;
                                            //VmOhm = Convert.ToString( Math.Abs(Convert.ToDecimal(VV1) - Convert.ToDecimal(VV2)) / divisor);

                                     if (g_Batt_Classtype[insert_num] !="?" && VV4 != null && VV4 != "0.0")
                                   //  if (g_Batt_Classtype[insert_num] != "?")
                                          VmOhm = Convert.ToString(Math.Abs(Convert.ToDecimal(VV1) - Convert.ToDecimal(VV2)) / Math.Abs(Convert.ToDecimal(VV3) - Convert.ToDecimal(VV4)));
                                     else
                                          VmOhm = "0.000";
                                     break;



                                }
                            }

                                //vStart_date //設定測試的日期(因為是key，所以手動輸 0755  測試值上線要拿掉
                                //vStart_date = "2024/01/01 01:11:44";
                                //'2024/07/01 02:13:44'


                                //string tableTileSql = "", columnSql = "", valueSql = "";

                                ///insertSql = insertSql + " INSERT INTO pfprocess001  ";
                                ///insertSql = insertSql + " INSERT INTO pfprocess001  ";
                                DateTime now_str = DateTime.Now;

                                // 寫入資料庫的格式（正確）
                                string dbTimeStr = now_str.ToString("yyyy-MM-dd HH:mm:ss");
                                //  string dateStr = now_str.ToString("yyyy-MM-dd HH:mm:ss", new CultureInfo("zh-TW")); // 格式與 MySQL 相符

                                // 顯示用格式（含上午/下午）
                                CultureInfo taiwanCulture = new CultureInfo("zh-TW");
                                string displayTimeStr = now_str.ToString("yyyy/M/d tt hh:mm:ss", taiwanCulture);

                                // 先解析時間
                                DateTime dt_start = DateTime.ParseExact(vStart_date, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                                DateTime dt_end = DateTime.ParseExact(vdateEnd_date, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);


                                // 格式  yyyy/MM/dd 上午/下午 hh:mm:ss
                                string cvt_startdate = dt_start.ToString("yyyy/MM/dd tt hh:mm:ss", taiwanCulture);
                                string cvt_enddate = dt_end.ToString("yyyy/MM/dd tt hh:mm:ss", taiwanCulture);
                
                                valueSql = "VALUES ( '" + dr["fld" + vComID].ToString() + "',  '" + vStart_date + "','" + cvt_enddate + "','" + vtary_ID + "','" + vparameter + "',";
                                valueSql = valueSql + " '" + dr["fld" + vState].ToString() + "' ,'" + (VD28 == null ? "NULL" :  $"{VD28}") + "','" + (VD28 == null ? "NULL" : $"{VD28}") + "','" + (VAHD28 == null ? "NULL" : $"{VAHD28}") + "','" + (VAHD28 == null ? "NULL" : $"{VAHD28}") + "',";
                                valueSql = valueSql + " '" + (VD32 == null ? "NULL" : $"{VD32}") + "' ,'" + (VD32 == null ? "NULL" : $"{VD32}") + "','" + (VAHD32 == null ? "NULL" : $"{VAHD32}") + "','" + (VAHD32 == null ? "NULL" : $"{VAHD32}") + "','" + (VD35 == null ? "NULL" : $"{VD35}") + "',";
                                //valueSql = valueSql + " '" + VD35 + "' ,'" + VAHD35 + "','" + VAHD35 + "','" + Filename + "','" + vprocess + "',now()";
                                valueSql = valueSql + " '" + (VD35 == null ? "NULL" : $"{VD35}") + "' ,'" + (VAHD35 == null ? "NULL" : $"{VAHD35}") + "','" + (VAHD35 == null ? "NULL" : $"{VAHD35}") + "','" + Filename + "','" + vprocess + "','" + displayTimeStr + "'";

                                //select CCcurrent, OCV, averageV1, averageV2, averageV3, charge34V, charge345V, charge35V
                                //  , time50A, v, v1, v2, v3, v4, mOhm from processcc
                            switch (vparameter)
                            {
                                case "023": //pf
                                    tableTitleSql = " INSERT INTO pfprocess001  ";
                                    tableTitleSql = tableTitleSql +  " (ID,StartDateD,EnddateD,trayID,parameter ";
                                    tableTitleSql = tableTitleSql + ",State,VD28,VS28,VAHD28,VAHS28 ";
                                    tableTitleSql = tableTitleSql + " ,VD32 ,VS32 ,VAHD32,VAHS32 ,VD35  ";
                                    tableTitleSql = tableTitleSql + " ,VS35,VAHD35 ,VAHS35,FileName,Process,AnlaysisDayD,interpretcode,position,K_Value";

                                    valueSql = valueSql + " ,'" + CC2_interpretcode + "','" + CC2_position + "','" + Get_K_Value + "'";

                                    insertSql = insertSql + tableTitleSql +  " ) " + valueSql  + ");";
                                    break;
                                default: //cc2>cc1 所以有7個欄位 寫0

                                    /*cc 新增的欄位*/
                                    //,`Para`,`CCcurrent`,`OCV`,`averageV1`,`averageV2`,`averageV3`,`charge34V`,`charge345V`,`charge35V`
                                    //,`time50A`,`v`
                                    //,`v1`,`v2`,`v3`,`v4`
                                    //mOhm 欄位 ABS(KD16-KE16)/ABS(KF16-KG16)*1000 取得欄位後     Math.Abs();

                                    tableTitleSql = " INSERT INTO processcc";
                                    columnSql = "(ID,StartDateD,EnddateD,trayID,parameter ";
                                    columnSql = columnSql + ",State,VDA,VSA,VAHDA,VAHSA ";
                                    columnSql = columnSql + " ,VDB ,VSB ,VAHDB,VAHSB ,VDC  ";
                                    columnSql = columnSql + " ,VSC,VAHDC ,VAHSC,FileName,Process,AnlaysisDayD ";
                                    columnSql = columnSql + ",Para ";
                                    columnSql = columnSql + ",CCcurrent,OCV,averageV1,averageV2,averageV3,charge34V,charge345V,charge35V"; //cc1有的，
                                    columnSql = columnSql + ",time50A,v,v1,v2,v3,v4,mOhm,interpretcode,position,K_Value,analysisDT"; //cc2才有的，cc1要塞的話，值均為0 mOhm 是用算值出來的
                                    
                                    valueSql = valueSql + ",'" + Vpara + "'";  //para
                                    valueSql = valueSql + ", "+ VCCcurrent + "," + VOCV + "," + (VaverageV1 == null ? "NULL" : $"{VaverageV1}") + ","  + (VaverageV2 == null ? "NULL" : $"{VaverageV2}") + ","  + (VaverageV3 == null ? "NULL" : $"{VaverageV3}") + ","  + (Vcharge34V == null ? "NULL" : $"{Vcharge34V}") + ","  + (Vcharge345V == null ? "NULL" : $"{Vcharge345V}") + ","  + (Vcharge35V == null ? "NULL" : $"{Vcharge35V}");
                                    valueSql = valueSql + ", " + Vtime50A + ", " + (VV == null ? "NULL" : $"{VV}") + ", " + (VV1 == null ? "NULL" : $"{VV1}") + ", " + (VV2 == null ? "NULL" : $"{VV2}") + ", " + VV3 + ", " + (VV4 == null ? "NULL": VV4) + ", " + VmOhm + ", " + "'" + CC2_interpretcode + "'" + ", " + "'" + CC2_position + "'" + ", " + "'" + Get_K_Value + "'" + ", now()" + ") ";
                                        //新增vvalueSql//增加value(
                                        insertSql = insertSql + tableTitleSql + columnSql + " ) " + valueSql +";";
                                    break;

                            }

                                //alueSql = valueSql + ") ; ";

                                vComID = vComID + 7;
                                vState = vState + 7;

                                //DEBUG用
                                //if (iFlag == 13 || iFlag == 18 || iFlag == 24 || iFlag == 25)
                                //{
                                //     當有要跳過的電芯號數列,這邊需要跳出次數以這邊參考,多增加跳躍7個欄位, 在依照實際跳躍的電芯號數量做判定
                                //    vComID = vComID + 21;
                                //    vState = vState + 21;
                                //}

                                //if (iFlag == 14)
                                //{
                                //    // 當有要跳過的電芯號數列,這邊需要跳出次數以這邊參考,多增加跳躍7個欄位, 在依照實際跳躍的電芯號數量做判定
                                //    vComID = vComID + 7 * (1 + 8);
                                //    vState = vState + 7 * (1 + 8);
                                //}
                                //else
                                //{
                                //    vComID = vComID + 7;
                                //    vState = vState + 7;
                                //}

                                //int vComID = 8; //comid 如MW2005A53693 第一筆H 欄+7 為第二組 Ch1_V(V)
                                //int vState = 14; //N 欄 如 ok: Ch1_Remark
                                //if (vComID >= 264 || vState >=264) 
                                //{
                                //    string modle_ID = g_batterycell_number[iFlag - 1].ToString();
                                //    Console.WriteLine("模組ID:" + modle_ID);                       
                                //    Console.WriteLine($" iFlag為{iFlag}項:,  第vComID:{vComID}個:欄位 , 第vState:{vState}筆");
                                //    break;
                                //}


                                insert_num++;

                            dr_detail.Close();
                        }

                    }
                }


                //LResult.Text = insertSql;
                conn_detail.Close();


                //String testSql = "insert INTO pfprocess001  (ID,StartDateD,EnddateD,trayID,parameter ,State,VD28,VS28,VAHD28,VAHS28  ,VD32 ,VS32 ,VAHD32,VAHS32 ,VD35   ,VS35,VAHD35 ,VAHS35,FileName,Process,AnlaysisDayD)VALUES ( 'MW2007A05101',  '2024/01/01 02:17:02','2024/01/01 07:15:14','PF-03-K000001','023', 'OK' ,'2.8000','2.8000','2627.0','2627.0', '3.3000' ,'3.3000','13802.2','13802.2','3.4000', '3.4000' ,'30400.0','30400.0','0000001.txt','00:Pressure Formation',now()) ; ";
                //testSql = testSql + "insert INTO pfprocess001(ID, StartDateD, EnddateD, trayID, parameter, State, VD28, VS28, VAHD28, VAHS28, VD32, VS32, VAHD32, VAHS32, VD35, VS35, VAHD35, VAHS35, FileName, Process, AnlaysisDayD)VALUES('MW2007A05101', '2024/01/01 02:18:02', '2024/01/01 07:16:14', 'PF-03-K000001', '023', 'OK', '2.8000', '2.8000', '2627.0', '2627.0', '3.3000', '3.3000', '13802.2', '13802.2', '3.4000', '3.4000', '30400.0', '30400.0', '0000001.txt', '00:Pressure Formation', now()); ";

                String testSql = insertSql;

                MySqlConnection conn_exec = new MySqlConnection(connection);

                if (conn_exec.State != ConnectionState.Open)
                        conn_exec.Open();
                //MySqlCommand cmd = new MySqlCommand(testSql, conn_exec);
                 cmd = new MySqlCommand(testSql, conn_exec);
                try
                {
                    cmd.ExecuteNonQuery(); //insert 36筆
                    LResult.Text = "已完成";

                }
                catch (Exception ex)
                {
                    LResult.Text = "資料錯誤" + ex.ToString();
                }

                conn_exec.Close();


                
            }

                if (conn.State != ConnectionState.Closed)
                    conn.Close();


                string originalfile = Filename;

                //將重新解析的(PF or CC1 or CC2)存成csv,並呈現table含數據於頁面上
                //只取檔案名稱,忽略副檔名
                Filename = Path.GetFileNameWithoutExtension(Filename);

                switch (vparameter)  //STEP 
                {
                    case "023": //pf ==>'023'
                        dumpcsv = "SELECT * FROM sakila.pfprocess001;";
                        Filename = Filename + "-pfprocess001.csv";
                        pfcc_tablename = "pfprocess001";
                        break;
                    case "010":  //cc1
                        dumpcsv = "SELECT * FROM sakila.processcc;";
                        Filename = Filename + "-process-cc1.csv";
                        pfcc_tablename = "processcc";
                        break;
                    case "017": //cc2
                        dumpcsv = "SELECT * FROM sakila.processcc;";

                        if (vparameter_chg == "0172") //cc2-2 2024
                        {
                            Filename = Filename + "-process-cc2-2.csv";
                        } else if (vparameter_chg == "017-chromaCC2") {
                            Filename = Filename + "-process_chroma-cc2.csv";
                        } 
                        else {
                            Filename = Filename + "-process-cc2.csv";
                        }
                        pfcc_tablename = "processcc";
                        break;
                }



                //若路徑資料夾(C:\\pf-cc)沒有則這邊建立,for MYSQL LOAD REQUIRE
                if (!Directory.Exists(ResultTaskFolder))
                    Directory.CreateDirectory(ResultTaskFolder);

                ExportToCsv resultcsv = new ExportToCsv();

                //  string pfccPath_File = Server.MapPath("~/" + "pf-cc" + "/")+ Filename;
                //(1)先將分析數據產生export csv格式檔
                string pfccPath_File = Path.Combine(ResultTaskFolder, Filename);
                DataTable dtView = resultcsv.Export(connection, dumpcsv, pfccPath_File);                      
                csvview.DataSource = dtView;
                csvview.DataBind();

                string sDayD_value = string.Empty;

                if (vparameter == "023")
                {
                    sDayD_value = "XXXXXX";
                }
                else {
                    sDayD_value = "AnlaysisDayD";
                }

                //(2)再將分析完的數據合併預先遠端建置之的table (這邊目前使用遠端 hr.test_mergepfcc)
                //目前所有(pc,cc1,cc2,cc2-2)都忽略以下欄位
                All_col_listname = $@"SELECT GROUP_CONCAT(CASE
                       WHEN COLUMN_NAME NOT IN('State', 'Process','{sDayD_value}') THEN COLUMN_NAME
                        ELSE NULL
                        END  ORDER BY ORDINAL_POSITION) AS col_list
                        FROM INFORMATION_SCHEMA.COLUMNS
                        WHERE TABLE_NAME = '{pfcc_tablename}'
                        AND TABLE_SCHEMA = '{schema_DB}'; ";


                if (resultcsv.Merge_existfilter_value(connection, connection_merge, All_col_listname, schema_DB, pfcc_tablename) == true)
                    LResult.Text = "分析完篩選型號及合併資料完畢!";
                else
                    LResult.Text = "資料合併異常,NG,請確認分析完PF_CC系列數據格式!";


                DirectoryInfo tempDir = new DirectoryInfo(DestinationFolder);
                foreach (FileInfo fi in tempDir.EnumerateFiles())
                {
                    // 目錄下C:\\tempcsv 內檔案全部刪除
                    File.Delete(DestinationFolder + Path.DirectorySeparatorChar + fi.Name);
                }


                List<string> recordsucessful = new List<string>();
                recordsucessful.Add(originalfile);


                //刪除已經完成之數據原始檔案
                DeleteTHreadOKFiles(SourceFolder, recordsucessful, mannulrun);

                //透過C:\\copy_pfcc_result.bat 將產出pf cc1 cc2 等數據csv 回存到 網路工作磁碟(ex:\\192.168.3.100\pfcc_result)
                // PFCC_result_SaveExecuteBatFile();
               // EXEC_Save_PFCCbat();


            } //end if (讀檔錯誤判斷====>)

        }

        //透過以下範圍參數建立電芯電容量欄位 (英文+數字(2位元))
        // TYPE 電容量
        //0 < 42
        //1   45~42
        //2   45 - 46
        //3   46 - 47
        //4   47 - 48
        //5   48 - 49
        //6   49 - 50
        //7   50 - 51
        //8   51~53
        //9 > 53
        private int Assign_Cap_mAH_Type(string sCap_mAH)
        {
            int require_mAH = 0 , final_code_number = 1 , diff_range=0;

            if (!string.IsNullOrEmpty(sCap_mAH))
            {
                double cap_mAH = Convert.ToDouble(sCap_mAH); // 嘗試轉換字串為數值格式
                //需要先將除1000取商 mAH 電芯容量單位為(1C = 1000mAH) 
                require_mAH = (int)cap_mAH / 1000;
            }
            else
            {
                // 這裡可以加入錯誤處理的邏輯，比如設為預設值或拋出錯誤
                Console.WriteLine("sCap_mAH 是空字串或 null");
            }

            //初版第一次32分選辨識碼
            //if (require_mAH < 42) return 0;
            //else if (require_mAH >= 42 && require_mAH < 45) return 1;
            //else if (require_mAH >= 45 && require_mAH < 46) return 2;
            //else if (require_mAH >= 46 && require_mAH < 47) return 3;
            //else if (require_mAH >= 47 && require_mAH < 48) return 4;
            //else if (require_mAH >= 48 && require_mAH < 49) return 5;
            //else if (require_mAH >= 49 && require_mAH < 50) return 6;
            //else if (require_mAH >= 50 && require_mAH < 51) return 7;
            //else if (require_mAH >= 51 && require_mAH < 53) return 8;
            //else if (require_mAH >= 53) return 9;
            //return 0;

            // 第一次32分選辨識碼 (區分低電容量 50000 mAH含以下 , 高電容量 以上)
            if (require_mAH < 25)
            {
                return 1;
            } //1.級距為 5000mAH = 5c ,  5 <= grade_span_Quo < 9
            else if (require_mAH >= 25 && require_mAH < 45)
            {
                int grade_span_Quo = (int)require_mAH / 5;
                int grade_span_Div = (int)require_mAH % 5;
                diff_range = grade_span_Quo - 5;

                final_code_number += 1; //初始為2             

            } //2.級距為 1000mAH = 1c 
            else if (require_mAH >= 45 && require_mAH < 64)
            {
                final_code_number += 5; //初始為6
                diff_range = require_mAH - 45;
            }

            //初始化回傳位置
            if (diff_range == 0)
                return final_code_number;

            //級距計算累加
            while (diff_range != 0)
            {
                final_code_number += 1;
                diff_range--;
            }
            return final_code_number; //若都找無結果預設1 最低容量, 或是經過計算的位置
        }

        // 判斷邏輯	00	優先去15
        //          09	優先去31
        //           G   非00與09 去16
        //           H   非00與09 去32
        private int Determination_Type_Position(string char_En, int Assign_number)
        {
            //配方版本: 例如 Ver.001
            if (sVer.EndsWith("001"))
            {               
                if (Assign_number <= 1)
                {
                    //E00 重新定位 '31' ,其他00維持15
                    if (Assign_number == 0 && char_En[0] == 'E') {
                        return 31;
                    }
                    else {
                        return 15;
                    }                    
                }
                
                if (Assign_number == 8 || Assign_number == 9) return 1;

                if (char_En[0] == 'G') //G判斷
                {
                    if (Assign_number <= 1 || Assign_number >= 8)
                    {

                    }
                    else //G 非00,01,08,09
                    {
                        return 16;
                    }
                }

                else if (char_En[0] == 'H') //H判斷
                {
                    if (Assign_number <= 1 || Assign_number >= 8)
                    {

                    }
                    else //H 非00,01,08,09
                    {
                        return 32;
                    }
                }

                else if (char_En[0] == 'A')  //A判斷
                {
                    //偶數
                    if (Assign_number % 2 == 0)
                    {
                        if (Assign_number >= 2 && Assign_number <= 6)
                        {
                            //A02 重新定位 '13' ,其他維持/=2
                            if (Assign_number == 2) {
                                return 13;
                            }
                            else {
                                return Assign_number / 2;
                            }                            
                        }
                    }
                    else
                    {   //奇數
                        if (Assign_number >= 3 && Assign_number <= 7)
                        {
                            int divnum = Assign_number / 2;
                            return 16 + divnum;
                        }
                    }
                }
                else if (char_En[0] == 'B')  //B判斷
                {
                    //偶數
                    if (Assign_number % 2 == 0)
                    {
                        if (Assign_number >= 2 && Assign_number <= 6)
                        {
                            int divnum = Assign_number / 2;
                            return 3 + divnum;
                        }
                    }
                    else
                    {
                        //奇數
                        if (Assign_number >= 3 && Assign_number <= 7)
                        {
                            int divnum = Assign_number / 2;
                            int remainder = Assign_number % 2;
                            return 20 + (divnum - remainder);
                        }
                    }
                }
                else if (char_En[0] == 'C')  //C判斷
                {
                    //偶數
                    if (Assign_number % 2 == 0)
                    {
                        if (Assign_number >= 2 && Assign_number <= 6)
                        {
                            int divnum = Assign_number / 2;
                            return 6 + divnum;
                        }
                    }
                    else
                    {
                        //奇數
                        if (Assign_number >= 3 && Assign_number <= 7)
                        {
                            int divnum = Assign_number / 2;
                            int remainder = Assign_number % 2;
                            return 21 + (divnum + remainder);
                        }

                    }

                }
                else if (char_En[0] == 'D')  //D判斷
                {
                    //偶數
                    if (Assign_number % 2 == 0)
                    {
                        if (Assign_number >= 2 && Assign_number <= 6)
                        {
                            int divnum = Assign_number / 2;
                            return 9 + divnum;
                        }
                    }
                    else
                    {
                        //奇數
                        if (Assign_number >= 3 && Assign_number <= 7)
                        {
                            int divnum = Assign_number / 2;
                            int remainder = Assign_number % 2;
                            return 24 + (divnum + remainder);
                        }
                    }
                }
                else //當搜尋到未知的符號,目前若電芯號碼串接無資訊回傳,預設 char_En[0] -> '?'
                {
                    return 32;
                }

            }
            else if (sVer.EndsWith("002"))
            {

            }

            return 32;
        }

        private int calculate_insert_currentNumber(List<string> all_battery_class,String pfcc_param) 
        {            
            int count = 0;
            g_K_serial_NohaveIndexes = new List<int>();
            if (all_battery_class.Count == 0)
                count = all_battery_class.Count;
            else
            {
                for (int modle = 0; modle < all_battery_class.Count; modle++)
                {
                    // CC2需要sync 有電芯K值數據才有意義,CC1目前不需要,以下做區分
                    if (pfcc_param.StartsWith("017") && all_battery_class[modle] != "" || pfcc_param.StartsWith("010"))
                        count++;

                    if (all_battery_class[modle] == "?") { 
                        //存取無K值索引位置
                        g_K_serial_NohaveIndexes.Add(modle);
                    }

                }
            }

            return count;
        }

        private void Sync_HTBI_Merge_Classparam(string MS_dbcon, List<string> all_batterycell)
        {
            Console.WriteLine($" MSSQL connecting string =  {MS_dbcon} ");
            string scmdAll = "", s_cmd2 = "";

            //實際透過MSSQL query 找到的電芯號
            List<string> actual_find_model = new List<string>();

            //實際透過MSSQL query 英文 classType 代號
            List<string> actual_find_classtype = new List<string>();

            //實際透過MSSQL query 英文 K_Value 數值
            List<string> actual_find_kvalue = new List<string>();

            //找尋同電芯號的位置index
            //List<int> matchingIndexes = new List< int>();

            // 儲存匹配的 index 和對應的 classType
            List<Tuple<int, string>> matchingIndexes = new List<Tuple<int, string>>();

            // 紀錄已經搜尋過的索引
            HashSet<int> searchedIndexes = new HashSet<int>();

            // 紀錄已經統計的索引
            HashSet<int> runfilter = new HashSet<int>();

            //全部要搜尋的電芯號列表
            StringBuilder All_battarycell = new StringBuilder();

            //全部CASE 電芯號列表描述語法 (排序依原先)
            StringBuilder All_battaryCase = new StringBuilder();

            //確認找到電芯號旗標 flag
            bool check_classtype, findBox_Batt;

            //  select BOX_BATT,ClassType from HTBI_K_Value_MapperType2_V where BOX_BATT IN ( 'MW2007H62787', 'MW2007H62788','MW2007H62789','MW2007H62790','MW2007H62791','MW2007H62955');
            for (int modle = 0; modle < all_batterycell.Count; modle++)
            {
                // for s_cmd  -----start------
                All_battarycell.Append("'").Append(Convert.ToString(all_batterycell[modle])).Append("'");

                // 如果不是最後一個元素，則加逗號
                if (modle < all_batterycell.Count - 1)
                {
                    All_battarycell.Append(", ");
                }
                //  -----end------

                // for s_cmd2---- - start------
                All_battaryCase.Append(" WHEN '")
               .Append(Convert.ToString(all_batterycell[modle]))
               .Append("' THEN ")
               .Append(modle + 1)
               .Append(Environment.NewLine);

                if (modle == all_batterycell.Count - 1)
                {
                    All_battaryCase.Append(Environment.NewLine).Append("ELSE ").Append(modle + 2).Append(" END;");
                }
                // -----end------
            }

            // Console.WriteLine($" All_battarycell CASE  =  {All_battarycell} ");

            //原先modelID 編號唯一 不會有重複狀況----------start-------------------
            //ORDER BY 子句中使用 CASE，將每個 BOX_BATT 的值映射到一個固定的排序順序
           // scmdAll = "select BOX_BATT,ClassType from HTBI_K_Value_MapperType2_V where BOX_BATT IN ( " + All_battarycell + " )";            
           // s_cmd2 = " ORDER BY CASE BOX_BATT " + All_battaryCase;
            //----------------------end--------------------------------------------


            //目前modelID 編號會有重複狀況 (以最新ID 鎖定電芯號優先 降冪)----------start-------------------
            scmdAll = "WITH RankedBox_Batt AS (SELECT *,  ROW_NUMBER() OVER(PARTITION BY BOX_BATT ORDER BY ID DESC) AS rn FROM HTBI_K_Value_MapperType2_V  WHERE BOX_BATT IN( " + All_battarycell + " ) ) ";            
            s_cmd2 = " SELECT * FROM RankedBox_Batt WHERE rn = 1 ORDER BY CASE BOX_BATT " + All_battaryCase;
            //----------------------end--------------------------------------------


            scmdAll = $"{scmdAll}{s_cmd2}";


            Console.WriteLine($" All_battarycell Query CMD =  {scmdAll} ");


            SqlConnection icn = new SqlConnection();
            icn.ConnectionString = MS_dbcon;

            //stop current 
            if (icn.State == ConnectionState.Open) icn.Close();

            try
            {
                //open start!
                icn.Open();

                SqlCommand ack = new SqlCommand(scmdAll, icn);
                ack.CommandText = scmdAll;
                ack.CommandTimeout = 3000;
                ack.CommandType = CommandType.Text;

                //SqlDataReader:從數據庫獲取行
                SqlDataReader Batt_box = ack.ExecuteReader();
                //int Count = Batt_box.FieldCount;
                int number = 0, dynic_num = 0;
                int All_Batt_Length = all_batterycell.Count;

                //宣告36組空字串空間
                g_Batt_Classtype = new List<string>(new string[All_Batt_Length]);
                g_Modle_CC_Kvalue = new List<string>(new string[All_Batt_Length]);


                //宣告Map classtype 儲存最終確認陣列
                List<string> final_classtype_list = new List<string>(new string[All_Batt_Length]);

                //  ClassType 為K值 英文代號 ()
                //Type 範圍
                //A - 0.03
                //B   0
                // C - 0.06
                // D   0.03
                //E - 0.1
                //F   0.06
                // G < -0.1
                // H > 0.1

                while (Batt_box.Read())
                {
                    number++;
                    string classType = Batt_box["ClassType"].ToString();
                    string box_Battary = Batt_box["BOX_BATT"].ToString();
                    string modle_Kvalue = Batt_box["K_Value"].ToString();
                    //將找到的電芯號存入
                    actual_find_model.Add(box_Battary);
                    //將找到的classtype存入
                    actual_find_classtype.Add(classType);
                    //將找到的K_Value存入
                    actual_find_kvalue.Add(modle_Kvalue);

                }

                Console.WriteLine($" total classtype 總數量 =  {number} ");

                //當全部電芯號都有找到 目前是36組為一個group
                if (number >= 36)
                {
                    int cut_fit = 0;

                    while (cut_fit < number)
                    {
                        //再次確定有電芯號同步串接成功
                        if (actual_find_model[cut_fit].Equals(all_batterycell[cut_fit].ToString()))
                        {
                            //當classtype 尚未產生,預設 ?
                            if (actual_find_classtype[cut_fit].ToString() == "")
                            {
                                g_Batt_Classtype[cut_fit] = "?";
                                g_Modle_CC_Kvalue[cut_fit] = "";
                            }
                            else
                            {
                                g_Batt_Classtype[cut_fit] = actual_find_classtype[cut_fit].ToString();
                                g_Modle_CC_Kvalue[cut_fit] = actual_find_kvalue[cut_fit].ToString();
                            }
                        }

                        cut_fit++;
                    }

                }
                else if (number >= 1 && number < 36) //查沒有36組, 36組以內 
                {                    
                    g_OnlyExist_ModleID_Number = new List<int>();
                    g_Modle_CC_Kvalue = new List<string>();                    
                    //紀錄當前modle 在all_batterycell搜尋列的index 位置
                    for (int find = 0; find < actual_find_model.Count; find++)
                    {
                        for (int search = 0; search < all_batterycell.Count; search++)
                        {
                            // 跳過已經搜尋過的索引
                            if (searchedIndexes.Contains(search))
                            {
                                continue;
                            }

                            // 當有找到同樣電芯號碼, 紀錄當前搜尋列的 index 位置
                            if (actual_find_model[find].ToString() == all_batterycell[search].ToString())
                            {
                                // 將 index 和對應的 classType 存入 matchingIndexes
                                matchingIndexes.Add(new Tuple<int, string>(Convert.ToInt32(search), actual_find_classtype[find]));
                                searchedIndexes.Add(search);  // 記錄已經搜尋過的 index
                                g_OnlyExist_ModleID_Number.Add(search); //啟動僅存有找到電芯ID號碼
                                g_Modle_CC_Kvalue.Add(actual_find_kvalue[find].ToString()); //儲存搜尋到的電芯K值   
                            }
                        }
                    }

                    //原先查詢電芯總數量
                    for (int run = 0; run < all_batterycell.Count; run++)
                    {
                        //預設false
                        findBox_Batt = false;

                        var matchingResult = matchingIndexes.FirstOrDefault(item => item.Item1 == run);

                        if (matchingResult != null)
                        {
                            findBox_Batt = !findBox_Batt; //切為 true
                            // 記錄處理統計過的 index
                            //當classtype 尚未產生,預設 ?
                            if (matchingResult.Item2.ToString() == "")
                            {
                                g_Batt_Classtype[run] = "?";                            
                            }
                            else
                            {
                                g_Batt_Classtype[run] = matchingResult.Item2.ToString();                              
                            }

                            // 找到對應的索引
                            //Console.WriteLine($"Found  BoX_Batt index = {run}: classType = {matchingResult.Item2}");
                        }

                        //沒有找到電芯號
                        if (!findBox_Batt)
                        {
                            g_Batt_Classtype[run] = "?";                     
                        }

                        ////實際query找到目前電芯的位置號碼
                        //for (int id = 0; id < matchingIndexes.Count; id++)
                        //{
                        //    if (runfilter.Contains(id))
                        //    {
                        //        continue;
                        //    }
                        //int index = matchingIndexes[id].Item1;   // 取得匹配的索引
                        //string classType = matchingIndexes[id].Item2;  // 取得對應的 classType
                        //    //有找到電芯號位置                         
                        //     if( run == index)
                        //    {
                        //        findBox_Batt = !findBox_Batt; //切為 true
                        //        runfilter.Add(run);  // 記錄處理統計過的 index
                        //        g_Batt_Classtype[run] = actual_find_classtype[].ToString();
                        //    }
                        //}

                        //沒有找到電芯號
                        //if (!findBox_Batt)
                        //{
                        //    g_Batt_Classtype[run] = "?";
                        //}

                    }
                }
                else //查無任何電芯號 
                {                
                    for (int modle = 0; modle < all_batterycell.Count; modle++)
                    {
                        g_Batt_Classtype[modle] = "?";
                        g_Modle_CC_Kvalue[modle] = "";                 
                    }
                }



                //close SyncObject 與 資料配接器物件來釋放物件所佔用的資源 mean Free not use memory session                                
                Batt_box.Close();
                ack.Dispose();
                icn.Close();
            }
            catch (Exception k)
            {
                Console.WriteLine("Error link HTBI Read!");
                throw k;
            }
        }



        private string GetRelativePath(string rootPath, string filePath)
        {
            Uri rootUri = new Uri(rootPath);
            Uri fileUri = new Uri(filePath);

            // 確保 URI 是相同的基礎
            if (rootUri.Scheme != fileUri.Scheme)
            {
                throw new InvalidOperationException("Cannot create relative path from different root types.");
            }

            // 返回相對路徑
            return Uri.UnescapeDataString(rootUri.MakeRelativeUri(fileUri).ToString());
        }

        private int Parse_chroma_V_serial_count(int caculatorNumber, int chV_step, int chI_step, string constring)
        {
            MySqlCommand comm_V;
            MySqlDataReader dr_V_Read;
            MySqlConnection conn_parse = new MySqlConnection(constring);
            string sqlQuery_V = "" , sqlQuery_Vabs = "" , sqlQuery_maHabs = "" , sqlQuery_Current = "", sql_totall="";
            int dr_result , dr_V = 0, count = 0;


            // 有考慮到 Reached Target voltage 的充電到達次數 chI_step != null , 反之則 chI_step != ''
            //V 數量
            if (caculatorNumber == 0)
            {
                sqlQuery_V = "select count(*)+10 from test_LoadPFData003  where (fld7 >= '1' AND fld7 <= '2'  OR fld7 = '3' AND fld" + chV_step + " != '0' AND(fld" + chI_step + " != '0' OR  fld" + chI_step + " != null))";
                sqlQuery_Vabs = " union all  SELECT count(fld" + chV_step + ") FROM test_loadpfdata003 WHERE fld7 LIKE '"+step_abs_value[0]+ "' and fld" + chV_step + " not like '0' ";
                sqlQuery_maHabs = " union all  SELECT count(fld" + (chV_step+4) + ") FROM test_loadpfdata003 WHERE fld7 LIKE '" + step_abs_value[0] + "' and fld" + (chV_step+4) + " not like '0' ;";
            } //V1 數量
            else if (caculatorNumber == 1)
            {

                sqlQuery_V = "select count(*)+10 from test_LoadPFData003  where (fld7 >= '1' AND fld7 <= '6')";
                sqlQuery_Vabs = " union all  SELECT count(fld" + chV_step + ") FROM test_loadpfdata003 WHERE fld7 LIKE '" + step_abs_value[1] + "' and fld" + chV_step + " not like '0' ";
                sqlQuery_maHabs = " union all  SELECT count(fld" + (chV_step + 4) + ") FROM test_loadpfdata003 WHERE fld7 LIKE '" + step_abs_value[1] + "' and fld" + (chV_step + 4) + " not like '0' ;";

            } //V2 數量
            else if (caculatorNumber == 2)
            {
                sqlQuery_V = "select count(*)+10 from test_LoadPFData003  where (fld7 >= '1' AND fld7 <= '6'  OR fld7 = '7' AND fld" + chV_step + " != '0' AND(fld" + chI_step + " != '0' OR  fld" + chI_step + " != null))";
                sqlQuery_Vabs = " union all  SELECT count(fld" + chV_step + ") FROM test_loadpfdata003 WHERE fld7 LIKE '" + step_abs_value[2] + "' and fld" + chV_step + " not like '0' ";
                sqlQuery_maHabs = " union all  SELECT count(fld" + (chV_step + 4) + ") FROM test_loadpfdata003 WHERE fld7 LIKE '" + step_abs_value[2] + "' and fld" + (chV_step + 4) + " not like '0' ;";
            
            } //V3 數量
            else if (caculatorNumber == 3)
            {
                sqlQuery_V = "select count(*)+20 from test_LoadPFData003  where (fld7 >= '1' AND fld7 <= '5')";
                sqlQuery_Current = " union all  SELECT count(fld" + chI_step + ") FROM test_loadpfdata003 WHERE fld7 LIKE '" + step_abs_value[0] + "' and fld" + chI_step + " not like '0' ";
            } //V4 數量
            else if (caculatorNumber == 4)
            {
                sqlQuery_V = "select count(*)+20 from test_LoadPFData003  where (fld7 >= '1' AND fld7 <= '6')";
            }

            sql_totall = sql_totall + sqlQuery_V + sqlQuery_Vabs + sqlQuery_maHabs+ sqlQuery_Current;

            comm_V = new MySqlCommand(sql_totall, conn_parse);


            {
                if (conn_parse.State != ConnectionState.Open)
                    conn_parse.Open();
            }

            dr_V_Read = comm_V.ExecuteReader();

            if (dr_V_Read.HasRows)
            {
                //使用Read方法把資料讀進Reader，讓Reader一筆一筆順向指向資料列，並回傳是否成功。
                while (dr_V_Read.Read())
                {
                    
                    dr_result = dr_V_Read.GetInt32(0);  // 這裡 0 是指第一列的資料，假設只有一個結果

                    //存取v,v1,v2
                    if (count == 0) {
                        dr_V = dr_result;
                    } //存取 ch_v
                    else if (count == 1 && dr_result != 0) {

                        if (caculatorNumber == 0) {
                            step_caculator_value[0] = dr_result;
                        }
                        else if (caculatorNumber == 1)
                        {
                            step_caculator_value[2] = dr_result;
                        }
                        else if (caculatorNumber == 2)
                        {
                            step_caculator_value[4] = dr_result;
                        }//存取  CC-current (A)
                        else if (caculatorNumber == 3)
                        {
                            step_caculator_value[6] = dr_result;

                        }

                    }//存取 mAH
                    else if (count == 2 && dr_result != 0) {
                        if (caculatorNumber == 0) {
                            step_caculator_value[1] = dr_result;
                        }
                        else if (caculatorNumber == 1)
                        {
                            step_caculator_value[3] = dr_result;
                        }
                        else if (caculatorNumber == 2)
                        {
                            step_caculator_value[5] = dr_result;
                        }
                    }
                   
                    if ( caculatorNumber <=2  &&  count == 2 || caculatorNumber == 3 && count == 1 || caculatorNumber == 4) {
                        conn_parse.Close();
                        return dr_V;
                    }

                    count++;
                }               
            }

            throw new NotImplementedException();
        }

        private void PFCC_result_SaveExecuteBatFile()
        {
            try
            {
                // 設定 .bat 檔案的路徑
               // @"C:\copy_pfcc_result.bat";
                // 使用虛擬路徑
               // string batFilePath = "~/C:/copy_pfcc_result.bat"; // 虛擬路徑
               // string physicalPath = Server.MapPath(batFilePath); // 轉換為物理路徑

                string physicalPath = @"C:\copy_pfcc_result.bat"; // 既有的實體路徑

                // 1. 取得網站的根目錄
                string rootPath = Server.MapPath("~/");

                
                // 2. 取得虛擬路徑
                string relativePath = GetRelativePath(rootPath, physicalPath); // 計算相對路徑

                // 3. 建立虛擬路徑 (根據你的需求)
                string virtualPath = "~/" + relativePath.Replace("\\", "/"); // 替換反斜線為斜線

                // 4. 轉換虛擬路徑為物理路徑
                string convertedPhysicalPath = Server.MapPath(virtualPath);

                // 設定 ProcessStartInfo
                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    FileName = convertedPhysicalPath,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true // 不顯示命令提示字元視窗
                };

                // 執行檔案
                using (Process process = Process.Start(processStartInfo))
                {
                    // 可選：讀取輸出
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    process.WaitForExit();

                    // 可選：處理輸出和錯誤
                    if (process.ExitCode == 0)
                    {
                        // 執行成功
                        Response.Write("執行成功：" + output);
                    }
                    else
                    {
                        // 執行失敗
                        Response.Write("執行失敗：" + error);
                    }
                }
            }
            catch (Exception ex)
            {
                // 錯誤處理
                Response.Write("錯誤：" + ex.Message);
            }
        }

        private void EXEC_Save_PFCCbat() {

            System.IO.StreamReader strm = System.IO.File.OpenText(@"C:\\inetpub\\wwwroot\\copy_pfcc_result.bat"); //讀bat 檔案
            // Create the ProcessInfo object
            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo("cmd.exe");
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            //psi.RedirectStandardInput = true;
            psi.RedirectStandardError = true;

            psi.Arguments = "/K C:\\inetpub\\wwwroot\\copy_pfcc_result.bat";
            psi.WorkingDirectory = "C:\\inetpub\\wwwroot\\";

            // Start the process
            System.Diagnostics.Process proc = System.Diagnostics.Process.Start(psi);

            // Attach the output for reading
            System.IO.StreamReader sOut = proc.StandardOutput;
            //proc.Close();

            // Read the sOut to a string.
            // string results = sOut.ReadToEnd().Trim();
            // sOut.Close();

            // Attach the in for writing
            //System.IO.StreamWriter sIn = proc.StandardInput;
            //// Write each line of the batch file to standard input
            //while (strm.Peek() != -1)
            //{
            //    sIn.WriteLine(strm.ReadLine());  //寫入
            //}
            //strm.Close();

            // Close the process
            //proc.WaitForExit();
            proc.Close();

            // Read the sOut to a string.
            //string results = sOut.ReadToEnd().Trim();           
            //// Close the io Streams;
            //sOut.Close();
            

            //// Close the io Streams;
            //// sIn.Close();
            //string fmtStdOut = "{0}";
            //this.Response.Write(String.Format(fmtStdOut, results.Replace(System.Environment.NewLine, "")));

            LResult.Text += "  -> 請執行copy_pfcc_result.bat將數據結果存到pfcc_result";
        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            //分析cc
            // string connection = "server=localhost;user id=root;password=27763923;database=sakila; pooling=true;";

            string connection = "server=localhost;user id=root;password=Xcold@246810;database=sakila; pooling=true;";

            MySqlConnection conn = new MySqlConnection(connection);


            //int row_i = 8;
            //總共要塞的欄位
            // insert into pfprocess001()
            //ID,Start dateEnd date,tary ID,	parameter,State,2.8V,2.8V Ah,3.2V,3.2V Ah,3.5V,3.5V Ah,	file name,process,Anlaysis day

            //取固定值---
            string sqlQuery = "";
            sqlQuery = sqlQuery + "/*title */";
            sqlQuery = sqlQuery + " (select fld5 as f_title,1 as sort from test_LoadPFData003 LIMIT 9, 1)  /*start_date */ ";
            sqlQuery = sqlQuery + "union all ";
            sqlQuery = sqlQuery + "(select fld5 as f_title,2 as sort from test_LoadPFData003 order by fld5 desc LIMIT 1, 1) /*end_date */ ";
            sqlQuery = sqlQuery + "union all ";
            sqlQuery = sqlQuery + "(select CONCAT ((select fld2 from test_LoadPFData003 LIMIT 3, 1) , '-' , (select fld2 from test_LoadPFData003 LIMIT 0, 1) ) as f_title,3 as sort ) ";
            sqlQuery = sqlQuery + "/*tray_id*/ ";
            sqlQuery = sqlQuery + "union all ";
            sqlQuery = sqlQuery + "(select left(fld2,3) as f_title,4 as sort from test_LoadPFData003 LIMIT 2, 1)  /*parameter*/ ";
            sqlQuery = sqlQuery + "union all ";
            sqlQuery = sqlQuery + "(select fld2 as f_title,5 as sort from test_LoadPFData003 LIMIT 1, 1)  /*process*/ ";
            sqlQuery = sqlQuery + "union all ";
            sqlQuery = sqlQuery + "(select now() as f_title,6 as sort ) /*Anlaysis day*/ ";


            MySqlCommand comm = new MySqlCommand(sqlQuery, conn);
            conn.Open();
            MySqlDataReader dr = comm.ExecuteReader();
            String returnValue = "";


            //ID,Start dateEnd date,tary ID,	parameter,State,2.8V,2.8V Ah,3.2V,3.2V Ah,3.5V,3.5V Ah,	file name,process,Anlaysis day
            string vStart_date = "";
            string vdateEnd_date = "";
            string vtary_ID = "";
            string vparameter = "";
            string vprocess = "";

            string sort_temp = "";


            //檢查是否有資料列
            if (dr.HasRows)
            {
                //使用Read方法把資料讀進Reader，讓Reader一筆一筆順向指向資料列，並回傳是否成功。
                while (dr.Read())
                {

                    //DataReader讀出欄位內資料的方式，通常也可寫Reader[0]、[1]...[N]代表第一個欄位到N個欄位。
                    //ss += Convert.ToString(dr["city_id"].ToString() + " -> " + dr["city"].ToString() + " -> " + dr["country_id"].ToString() + "\r\n");
                    sort_temp = Convert.ToString(dr["sort"].ToString());
                    switch (sort_temp)
                    {
                        case "1":
                            vStart_date = Convert.ToString(dr["f_title"].ToString());
                            break;
                        case "2":
                            vdateEnd_date = Convert.ToString(dr["f_title"].ToString());
                            break;
                        case "3":
                            vtary_ID = Convert.ToString(dr["f_title"].ToString());
                            break;
                        case "4":
                            vparameter = Convert.ToString(dr["f_title"].ToString());
                            break;
                        case "5":
                            vprocess = Convert.ToString(dr["f_title"].ToString());
                            break;
                        default:

                            break;
                    }




                    //---returnValue += Convert.ToString(dr["f_title"].ToString() + " -> -----------------" + dr["sort"].ToString() + " -> -----------------" + "<br>");
                    //returnValue += Convert.ToString(dr["fld" + row_i].ToString() + " -> -----------------" + "<br>");





                }

            }//if (dr.HasRows)


            conn.Close();
        }

        protected void Button3_Click(object sender, EventArgs e)
        {
            BindSourceFileList("source");
            BindSourceFileList("object");


            //若自動化提早結束,但沒有清理已經完成數據轉換的原始csv,這裡手動清除
            // 讀取紀錄檔案內容(NG未完成檔案)
            //List<string> recordLines = new List<string>();

            //if (File.Exists(NG_file_record))
            //{
            //    recordLines.AddRange(File.ReadAllLines(NG_file_record)); // 讀取所有行
            //}

            ////只留下未完成,刪除已經完成之數據原始檔案
            //DeleteTHreadOKFiles(SourceFolder, recordLines,false);

            //LResult.Text = "已清除轉換完成原始csv,剩餘為未完成請再確認-> " + SourceFolder;
        }

        protected void Button2_Click1(object sender, EventArgs e)
        {
 
        }

        private void BindSourceFileList(string filepath)
        {
            string directoryPath = Server.MapPath("~/"+ filepath + "/");
            DataTable dt = new DataTable();
            dt.Columns.Add("FileName");
            dt.Columns.Add("FileSize (bytes)");
            dt.Columns.Add("Creation Date");

            if (Directory.Exists(directoryPath))
            {
                string[] files = Directory.GetFiles(directoryPath);

                foreach (string file in files)
                {
                    FileInfo fileInfo = new FileInfo(file);
                    DataRow row = dt.NewRow();
                    row["FileName"] = fileInfo.Name;
                    row["FileSize (bytes)"] = fileInfo.Length;
                    row["Creation Date"] = fileInfo.CreationTime;
                    dt.Rows.Add(row);
                }


                switch (filepath)
                {
                    case "source":
                        GridView1.DataSource = dt;
                        GridView1.DataBind();
                        break;
                    case "object":
                        GridView2.DataSource = dt;
                        GridView2.DataBind();
                        break;
                     
                }

                }
            else
            {
                // Handle directory not found
                Response.Write("Directory does not exist.");
            }
        }

        public void COPY_NG_Directionary(string Source, string NG_Destination , List<string> gloal_Ng_total ) 
        {

            DirectoryInfo srcDir = new DirectoryInfo(Source);


            try {
                foreach (FileInfo fi in srcDir.EnumerateFiles())
                {

                    for (int ng_fi = 0; ng_fi < gloal_Ng_total.Count; ng_fi++) {

                        string InputNGFile = gloal_Ng_total[ng_fi].ToString();

                        //if (fi.Name.Equals(InputNGFile))
                        //{
                        //    File.Copy(fi.FullName, NG_Destination + Path.DirectorySeparatorChar + fi.Name);
                        //}

                        if (fi.Name.Equals(InputNGFile))
                        {
                            string destinationPath = NG_Destination + Path.DirectorySeparatorChar + fi.Name;

                            // 檢查檔案是否已經存在，並選擇覆蓋或跳過
                            if (File.Exists(destinationPath))
                            {
                                // 若檔案已經存在，你可以選擇覆蓋或跳過檔案
                                // 覆蓋檔案
                                File.Copy(fi.FullName, destinationPath, overwrite: true);
                                // 如果不希望覆蓋檔案，可以跳過：
                                // continue; // 這行會跳過當前檔案，繼續處理下個檔案
                            }
                            else
                            {
                                // 檔案不存在，直接複製
                                File.Copy(fi.FullName, destinationPath);
                            }
                        }
                    }                                            
                }

            }
            catch (Exception err) {

                throw;
            }


        }

        public void CopyDirectory(string Source, string Destination, bool IsOverWrite = true , bool signlecopy = false)
        {
            String DestinationFolder = "c:\\\\tempcsv";
            //目標目錄不存在則新建
            if (!Directory.Exists(Destination))
                Directory.CreateDirectory(Destination);

            string InputFile = TextBox1.Text.ToString();

            DirectoryInfo srcDir = new DirectoryInfo(Source);
            bool checkfile = false;

            try
            {
                //步驟1:先找出所有目錄
                //foreach (string direc in Directory.GetDirectories(Source))
                //{
                //    DirectoryInfo Dir = new DirectoryInfo(direc);
                //    //先針對目前目錄的檔案做處理
                //    foreach (FileInfo fi in Dir.EnumerateFiles())
                //    {
                //        if (signlecopy)
                //        {
                //            if (fi.Name.Equals(InputFile))
                //            {
                //                File.Copy(fi.FullName, Destination + Path.DirectorySeparatorChar + fi.Name, IsOverWrite);
                //                checkfile = true;
                //                break;
                //            }
                //        }
                //        else
                //        {
                //            File.Copy(fi.FullName, Destination + Path.DirectorySeparatorChar + fi.Name, IsOverWrite);
                //        }
                //    }
                //    //遞迴搜尋下一個子目錄
                //    if (signlecopy)
                //    {
                //        if (!checkfile)
                //            CopyDirectory(direc, DestinationFolder, IsOverWrite, signlecopy);
                //        else
                //            break;
                //    }

                //}


                //步驟2: 再針對該source層內的所有檔案
                foreach (FileInfo fi in srcDir.EnumerateFiles()) 
                {
                   if (signlecopy)
                    {
                        if (checkfile)
                            break;

                        if (fi.Name.Equals(InputFile)) {
                            File.Copy(fi.FullName, Destination + Path.DirectorySeparatorChar + fi.Name, IsOverWrite);
                            break;
                        }
                    }
                    else {
                        File.Copy(fi.FullName, Destination + Path.DirectorySeparatorChar + fi.Name, IsOverWrite);

                    }
                }

                //foreach (FileInfo fi in srcDir.EnumerateFiles())
                //{
                //    //目錄下的目錄全部刪除
                //    File.Delete(Destination + Path.DirectorySeparatorChar + fi.Name);
                //}

                //目錄下的目錄再用遞迴方式複製
                //foreach (DirectoryInfo di in srcDir.EnumerateDirectories())
                //    CopyDirectory(di.FullName, DestinationFolder + Path.DirectorySeparatorChar + di.Name);
            }
            catch
            {
                throw;
            }
        }

        public void DeleteTHreadOKFiles(string directory, List<string> ThreadNgFile, bool mannul)
        {
            // 取得所有檔案，包括子資料夾中的檔案
            var allFiles = Directory.GetFiles(directory, "*.*", SearchOption.TopDirectoryOnly);

            foreach (var file in allFiles)
            {
                // 取得檔案名稱
                string fileName = Path.GetFileName(file);

                if (mannul) {

                    // 如果檔案在指定清單中，則刪除
                    if (ThreadNgFile.Contains(fileName))
                    {
                        try
                        {
                            //刪除已經完成運行轉換的數據檔案
                            File.Delete(file);
                            break;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error deleting file {file}: {ex.Message}");
                        }
                    }

                }
                else {

                    // 如果檔案不在指定清單中，則刪除
                    if (!ThreadNgFile.Contains(fileName))
                    {
                        try
                        {
                            //刪除已經完成運行轉換的數據檔案
                            File.Delete(file);
                            // Console.WriteLine($"Deleted: {file}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error deleting file {file}: {ex.Message}");
                        }
                    }


                }

               
            }
        }

        private string ToNullORVALUE_CheckString(object value)
        {
          if (value == null || value == DBNull.Value) return null;
            string s = Convert.ToString(value.ToString().Trim());
            return s == "" ? null : s;
        }

        protected  void Btn_Auto_Click(object sender, EventArgs e)
        {

            // this.Button1_Click(sender, e);
            //恢復開始Page_Load 元件初始狀態
            //this.Button1.Visible = false;
            //this.Button3.Visible = false;
            //this.Btn_Auto.Visible = true;
            // this.Btn_Auto.Enabled = false;
            this.Button1.Visible = false;
            this.Button3.Visible = false;
            bool mannulrun = false;
            // 隱藏其他元件
            this.ClientScript.RegisterStartupScript(this.GetType(), "hide", "hideElements();", true);
            Timer1.Interval = 1000;//設定每秒執行一次
            Timer1.Enabled = true;//啟動計時
            //ViewState["time"] = 0;

            // 設定執行緒數量初始為0
            int totalTasks = 0;
            g_csvFile = new List<string>();
            g_pfcctype = new List<string>();
            g_ThreadNotOkFile = new List<string>();
            List<Task> tasks = new List<Task>();
            using (StreamWriter writer = new StreamWriter(NGTThread_filepath, true)) // 設定為 true 以啟用附加模式
            {
                //  foreach (string line in lines)
                {
                    writer.WriteLine("開始"); // 寫入每一行
                }
            } // 此處自動關閉 StreamWriter 和檔案


            CancellationTokenSource cts = new CancellationTokenSource();

           // String SourceFolder = "z:\\\\source_pfcc";
           // String SourceFolder = @"Z:\source_pfcc";
          //  String SourceFolder = @"\\192.168.3.100\hr_tmp\source_pfcc";
          //  String SourceFolder = @"C:\copy_temp\source_pfcc";
            //String SourceFolder = @"C:\source_pfcc";
            String DestinationFolder = "c:\\\\tempcsv";
            String fileExtension = "csv";
            string pf_cctable = "";
            bool IsOverWrite = true;
            bool copy_one = false; //false -> 複製全部 / true -> 複製單項
            //load 資料


            //PF + CC 

            //將 Z:\source_pfcc 資料夾內全部複製到 C:\tempcsv
            CopyDirectory(SourceFolder, DestinationFolder, IsOverWrite, copy_one);

            string[] files = Directory.GetFiles(DestinationFolder, $"*.{fileExtension}");
          
            //只對*.csv檔案格式做工作序列
            foreach (string file in files)
            {
                //擷取開頭站點字串( pf:K000008 , CC1:H000014   CC2:H000020)
                //pfprocess001  存pf檔  PRIMARY KEY (`ID`,`StartDateD`)
                //processcc 存cc檔(含cc1, cc2)，PRIMARY KEY(`ID`,`StartDateD`)

                String flitersite =  Path.GetFileName(file).Substring(0, 3);

                //由上搜尋站點字串判斷要清除當前一站暫存table內容
                //for pf 
                if (flitersite.Equals("K00") || flitersite.Equals("PF0"))
                {
                    pf_cctable = "pfprocess001";
                }// for cc1 或 cc2
                else if (flitersite.Equals("H00") || flitersite.Equals("CC-") || flitersite.Equals("CC0"))
                {
                    pf_cctable = "processcc";
                }
                else
                {
                   // LResult.Text = file+"->沒有符合此(pf,cc系列)工作項目csv!";
                    continue;
                   // return;
                }

                //將檔案名稱/ pfcctype做存取
                g_csvFile.Add(file);
                g_pfcctype.Add(pf_cctable);
                totalTasks++;
            }

            if (totalTasks == 0)
            {
                LResult.Text = "目前全沒有符合此(pf,cc系列)工作項目csv! / 請執行copy_pfcc.bat";
                return;
            }

            for (int i = 0; i < totalTasks; i++)
            {
                string  taskId = g_csvFile[i].ToString();
                string  tasktype = g_pfcctype[i].ToString();

                LResult.Text = $"處理表單{taskId}進行中.....";


                // 每個執行緒都在新的任務中運行
                tasks.Add(Task.Run(async () => await ExecuteTask(taskId, tasktype, i, cts.Token)));
                //tasks.Add(Task.Run(() => ExecuteTask(taskId, tasktype, i, cts.Token)));
                
                //task為當前處理之工作項目
                var tasknow = ExecuteTask(taskId, tasktype, i, cts.Token);

                // 等待任務完成或超過 60 秒
                 if ( Task.WhenAny(tasknow, Task.Delay(20000)) == tasknow)
              // if (tasknow.Wait(TimeSpan.FromSeconds(20)))
                {
                    // 任務在 20 秒內完成
                    //await tasknow; // 確保捕獲任務結果
                    lock (_lock) // 確保同一時間只有一個執行緒執行
                    {
                        // await tasknow; // 確保捕獲任務結果
                        //tasknow.Wait();
                       // LResult.Text = tasknow.ToString();
                        LResult.Text = $"taskId: {taskId}" + "->任務進行中.....!";
                        timerunheck = true;
                    }
                }
                else
                {
                    // 任務超時，取消該任務
                    cts.Cancel();
                    // Console.WriteLine("任務超時，已取消！");
                    LResult.Text = taskId + "->總處理過程超過60秒，結束!";
                    timerunheck = false;

                   
                    cts.Cancel(); // 暫停該任務

                    // 等待下一個任務完成
                    semaphore.Wait();
                    try
                    {
                        // 等待 3 秒後重新執行
                        Thread.Sleep(3000);
                        //tasknow.Wait(); // 確保之前的任務可以繼續
                        string test = Convert.ToString(tasknow.Status);
                        Console.WriteLine($"目前執行緒狀態: {test} ");
                        
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }

                // 等待當前執行緒完成
                  var status =  tasks[tasks.Count - 1];

                // 完成後等待 5 秒
                if (i < g_csvFile.Count())
                {
                    Console.WriteLine($"等待任務 {status} 執行");
                    Console.WriteLine("等待 2 秒...");
                  //  Task.Delay(2000);
                }
            }

            //恢復開始Page_Load 元件初始狀態
            this.Button1.Visible = true;
            this.Button3.Visible = true;
            this.Btn_Auto.Visible = true;
            this.Btn_Auto.Enabled = true;

        
            DirectoryInfo tempDir = new DirectoryInfo(DestinationFolder);
            foreach (FileInfo fi in tempDir.EnumerateFiles())
            {
                // 目錄下C:\\tempcsv 內檔案全部刪除
                File.Delete(DestinationFolder + Path.DirectorySeparatorChar + fi.Name);
            }

            RewriteAndAppendToFile(NGTThread_filepath, g_ThreadNotOkFile);


            // 針對 pf,cc1,cc2來源檔案做後續處理
           // DeleteTHreadOKFiles(SourceFolder, g_ThreadNotOkFile, mannulrun);

            // 任務完成後恢復元件
            this.ClientScript.RegisterStartupScript(this.GetType(), "show", "showElements();", true);

           //Console.WriteLine("所有pf,cc1,cc2轉換任務已完成。");


          //  EXEC_Save_PFCCbat();

            LResult.Text = "所有pf,cc1,cc2轉換任務已完成 / 請執行copy_pfcc_result.bat將數據結果存到 pfcc_result";

        }
        protected async Task  ExecuteTask(string taskId, string pfcctype, int tnum,CancellationToken token)
        {
            //Yuping 本機端MYSQL 設定
            //string connection = "server=localhost;user id=root;password=27763923;database=sakila; pooling=true;";
            //目前開發本機端MYSQL 設定
            string connection = "server=localhost;user id=root;password=K@admin123456;database=sakila; pooling=true;Min Pool Size=0;Max Pool Size=3000;";


            //目前佈署端local host MYSQL 設定
            //string connection = "server=localhost;user id=root;password=Xcold@246810;database=sakila; pooling=true;";
            // string connection = "server=localhost;user id=root;password=Xcold@246810;database=sakila; pooling=true;Min Pool Size=0;Max Pool Size=3000;";


            // 遠端remote合併 hr.test_mergepfcc MYSQL 設定
            string connection_merge = "server=192.168.3.100;user id=root;password=Admin0331;database=mes; pooling=true;Min Pool Size=0;Max Pool Size=3000;";

            string STR_MSSQL_ARASHTBI = string.Format("server={0};database={1};uid={2};pwd={3};Connect Timeout = 180", MS_Server, MS_Database, MS_dbuid, MS_dbpwd);

            MySqlConnection conn = new MySqlConnection(connection);
            string tasknum = Convert.ToString(tnum+1);
           // Label2.Text = "處理狀態:"+ $"第{tasknum}個. 工作項目-> {taskId} 開始";
            LResult.Text = "處理狀態:" + $"第{tasknum}個. 工作項目-> {taskId} 開始";
            //Console.WriteLine($"第{tasknum}個. 工作項-> {taskId} 開始");

            //ViewState["time"] = 0;
            //Label1.Text = "";

            //Button1.Visible = false;
            // Button3.Visible = false;
            // Btn_Auto.Visible = true;
            // Btn_Auto.Enabled = false;
            // Timer1.Interval = 1000;//設定每秒執行一次
            // Timer1.Enabled = true;//啟動計時
            // ViewState["time"] = 0;



            //int maxExecutionTime = 20 * 1000; // 最大執行時間 20 秒
            //int elapsed = 0;
            //bool taskPaused = false;

            //// 模擬工作
            //while (elapsed < maxExecutionTime)
            //{
            //    // 模擬工作進行中
            //    Console.WriteLine($"執行緒 {taskId} 正在執行...");
            //    await Task.Delay(5000); // 模擬 5 秒的工作

            //    elapsed += 5000;

            //    if (elapsed >= maxExecutionTime && !taskPaused)
            //    {
            //        Console.WriteLine($"執行緒 {taskId} 超過最大執行時間，暫停...");
            //        taskPaused = true;

            //        // 暫停當前執行緒，並等待其他執行緒完成
            //        while (elapsed >= maxExecutionTime)
            //        {
            //            // 檢查是否有其他任務正在執行
            //            if (token.IsCancellationRequested)
            //            {
            //                Console.WriteLine($"執行緒 {taskId} 被取消。");
            //                return;
            //            }
            //            await Task.Delay(1000); // 暫停 1 秒
            //        }
            //    }
            //}

            //Console.WriteLine($"執行緒 {taskId} 完成");

            //Yuping 本機端MYSQL 設定
            //string connection = "server=localhost;user id=root;password=27763923;database=sakila; pooling=true;";
            //目前測試本機端MYSQL 設定
            // string connection = "server=localhost;user id=root;password=K@admin123456;database=sakila; pooling=true;";
            // MySqlConnection conn = new MySqlConnection(connection);

            //pf,cc1,cc2工作數據暫訂upload 位置資料夾(SourceFolder)
            //String SourceFolder = "z:\\\\source_pfcc";
            //   String SourceFolder = @"Z:\source_pfcc";
            //   String SourceFolder = @"\\192.168.3.100\hr_tmp\source_pfcc";
            //  String SourceFolder = @"C:\copy_temp\source_pfcc";
            // String SourceFolder = @"C:\source_pfcc";
            String DestinationFolder = "c:\\\\tempcsv";
            String Filename = "";
            String pf_cctable = "", pfcc_tablename = "";
            string schema_DB = "sakila";  // 本機PFCC工具資料庫名稱

            String LoadSql = "";

            String dumpcsv = "";
            String merge_sql_var = "", merge_table_rowdata = "", All_col_listname = "";
            String fileExtension = "csv";
            bool iscsvexist = false;
            bool IsOverWrite = true;
            bool copy_one = true;
            bool check_cc2_algorithm = false, haveTargetvoltage = true;
            //load 資料

            sVer = this.ver_select.SelectedItem.ToString();
            //PF + CC 

            //--start 這邊為(pf,cc1,cc2)檔案名稱和檔案型態,實際擷取的狀態依照前收集的名稱列--
            //Filename = taskId;
            Filename = Path.GetFileName(taskId);
            pf_cctable = pfcctype;

            String flitersite = Filename.Substring(0, 3);
            //---end----

            LoadSql = "use sakila; ";
            LoadSql = LoadSql + "delete from test_LoadPFData003; ";    //刪除暫存TABLE
            LoadSql = LoadSql + "TRUNCATE TABLE " + pf_cctable + "; ";
            //實際路徑是 C:\ProgramData\MySQL\MySQL Server 8.0\Data\test\
           // LoadSql = LoadSql + $" load data infile '{Filename}' into table test_loadpfdata003 fields terminated by ',' ;";
            LoadSql = LoadSql + " load data infile 'c:\\\\tempcsv\\\\" + Filename + "' into table test_loadpfdata003 fields terminated by ',' ;";
            //String testSql = insertSql;

            // MySqlConnection conn = new MySqlConnection(connection);
            string fileResult = "1";

            if (conn.State != ConnectionState.Open)
                conn.Open();


            MySqlCommand cmd = new MySqlCommand(LoadSql, conn);
            // conn.Open();

           
            try
            {
                
                //Console.WriteLine("連接成功");
                cmd.ExecuteNonQuery();
                LResult.Text = "已完成載檔";
                fileResult = "1";
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"連接失敗: {ex.Message}");
                cmd.Clone();
                conn.Close();
                //LResult.Text = "資料錯誤" + ex.ToString();
                LResult.Text = "上傳資料檔案錯誤";
                fileResult = "err";
            }
     
            conn.Close();

            if (fileResult == "1")
            {

                //總共要塞的欄位
                // insert into pfprocess001()
                //ID,Start dateEnd date,tary ID,	parameter,State,2.8V,2.8V Ah,3.2V,3.2V Ah,3.5V,3.5V Ah,	file name,process,Anlaysis day

                //取固定值---起始日、終止日
                string sqlQuery = "";
                sqlQuery = sqlQuery + "/*title */";
                //sqlQuery = sqlQuery + " (select fld5 as f_title,1 as sort from test_LoadPFData003 LIMIT 9, 1)  /*start_date */ ";
                sqlQuery = sqlQuery + "( select case when (SUBSTRING(fld5, 2, 1) = '/') or (SUBSTRING(fld5, 3, 1) = '/')  then CONVERT(STR_TO_DATE(fld5, '%m/%d/%Y %T'), DATETIME)  else CONVERT(fld5, DATETIME)  end f_title ,1 as sort  from test_LoadPFData003 LIMIT 9, 1 )  ";
                sqlQuery = sqlQuery + "union all ";
                //sqlQuery = sqlQuery + "(select fld5 as f_title,2 as sort from test_LoadPFData003 order by fld5 desc LIMIT 1, 1) /*end_date */ ";
                //sqlQuery = sqlQuery + "union all ";
                sqlQuery = sqlQuery + "( select max(a.result) as f_title,2 as sort from (SELECT    CASE        WHEN(            SELECT COUNT(*)    FROM test_LoadPFData003  WHERE fld5 <> 'PC Time'  AND((SUBSTRING(fld5, 2, 1) = '/')or(SUBSTRING(fld5, 3, 1) = '/'))        ) > 0 ";
                sqlQuery = sqlQuery + " THEN CONVERT(STR_TO_DATE(fld5, '%m/%d/%Y %T'), DATETIME)        ELSE CONVERT(fld5, DATETIME)    END AS result    from test_LoadPFData003    ) a ) /*end_date */ ";
                sqlQuery = sqlQuery + "union all ";
                sqlQuery = sqlQuery + "(select CONCAT ((select fld2 from test_LoadPFData003 LIMIT 3, 1) , '-' , (select fld2 from test_LoadPFData003 LIMIT 0, 1) ) as f_title,3 as sort ) ";
                sqlQuery = sqlQuery + "/*tray_id*/ ";
                sqlQuery = sqlQuery + "union all ";
                sqlQuery = sqlQuery + "(select fld2 as f_title,4 as sort from test_LoadPFData003 LIMIT 2, 1)  /*parameter*/ ";
                //sqlQuery = sqlQuery + "c  /*parameter*/ ";
                sqlQuery = sqlQuery + "union all ";
                sqlQuery = sqlQuery + "(select fld2 as f_title,5 as sort from test_LoadPFData003 LIMIT 1, 1)  /*process*/ ";
                sqlQuery = sqlQuery + "union all ";
                sqlQuery = sqlQuery + "(select now() as f_title,6 as sort ) /*Anlaysis day*/ ";

                MySqlCommand comm = new MySqlCommand(sqlQuery, conn);

                if (conn.State != ConnectionState.Open)
                    conn.Open();
                MySqlDataReader dr = comm.ExecuteReader();



                string vStart_date = "";
                string vdateEnd_date = "";
                string vtary_ID = "";
                string vparameter = "";
                string vparameter_chg = "";

                string vparameter_All = "";
                string vprocess = "";

                string sort_temp = "";


                //title 列
                if (dr.HasRows)
                {
                    //使用Read方法把資料讀進Reader，讓Reader一筆一筆順向指向資料列，並回傳是否成功。
                    while (dr.Read())
                    {

                        //DataReader讀出欄位內資料的方式，通常也可寫Reader[0]、[1]...[N]代表第一個欄位到N個欄位。
                        //ss += Convert.ToString(dr["city_id"].ToString() + " -> " + dr["city"].ToString() + " -> " + dr["country_id"].ToString() + "\r\n");
                        sort_temp = Convert.ToString(dr["sort"].ToString());
                        switch (sort_temp)
                        {
                            case "1":
                                vStart_date = Convert.ToString(dr["f_title"].ToString());
                                break;
                            case "2":
                                vdateEnd_date = Convert.ToString(dr["f_title"].ToString());
                                break;
                            case "3":
                                vtary_ID = Convert.ToString(dr["f_title"].ToString());
                                break;
                            case "4":
                                vparameter_All = Convert.ToString(dr["f_title"].ToString());

                                //for chroma
                                if (flitersite.Equals("PF0") || flitersite.Equals("CC0"))
                                {

                                    vparameter = vparameter_All.Substring(2, 3);
                                    vparameter_chg = vparameter_All.Substring(vparameter_All.Length-17 );

                                    // 當vparameter 為017 -> CC2時,目前下面做記號
                                    if (vparameter.StartsWith("017") && vparameter_chg.Contains("CC2"))
                                    {
                                        vparameter_chg = vparameter + "-chromaCC2";
                                    } //當vparameter 為010 -> CC1時,目前下面做記號
                                    else if (vparameter.StartsWith("010") && vparameter_chg.Contains("CC1"))
                                    {
                                        vparameter_chg = vparameter + "-chromaCC1";
                                    }//當vparameter 為010 -> CC1時,目前下面做記號
                                    else if (vparameter.StartsWith("023") && vparameter_chg.Contains("PF"))
                                    {
                                        vparameter_chg = vparameter + "-chromaPF";
                                    }
                                    else
                                    {
                                        //其他未定義
                                        vparameter_chg = vparameter;
                                    }

                                }
                                else
                                {
                                    // for SECI 
                                    vparameter = vparameter_All.Substring(0, 3);
                                    vparameter_chg = vparameter_All.Substring((vparameter_All.Length - 9), 4);
                                    if (vparameter_chg == "2023")
                                    {
                                        vparameter_chg = vparameter;
                                    }
                                    else
                                    {
                                        vparameter_chg = vparameter + "2";
                                    }
                                }

                                break;
                            case "5":
                                vprocess = Convert.ToString(dr["f_title"].ToString());
                                break;
                            default:

                                break;
                        }

                    }

                }//if (dr.HasRows)


                //每一個 Cell ID有7行 第一筆H~N行
                //H:Ch1_V(V)	I:Ch1_I(A)	J:Ch1_PV(V)	k:Ch1_OV(V)	L:Ch1_Capa(mAh) 	M:Ch1_Wh(Wh)	N:Ch1_Remark 每一組7行所以+7


                int vComID = 8; //comid 如MW2005A53693 第一筆H 欄+7 為第二組 Ch1_V(V)
                int vState = 14; //N 欄 如 ok: Ch1_Remark
                                 //展36筆
                String tempSql = " select ";
                for (int iFlag = 1; iFlag <= 36; iFlag++)
                {

                    tempSql = tempSql + "fld" + vComID + ", fld" + vState + ",";
                    vComID = vComID + 7;  //第一筆H 行第8行 +7(每組7行)
                    vState = vState + 7;  //第一筆N 行第14行 +7(每組7行)
                }
                tempSql = tempSql.Substring(0, tempSql.Length - 1) + " from test_LoadPFData003 LIMIT 7, 1 "; //因為取標頭只有一列

                dr.Close();

                comm = new MySqlCommand(tempSql, conn);
                dr = comm.ExecuteReader();

                int BattaryID = 8, battary_count = 0, insert_num = 0;
                vComID = 8;
                vState = 14;
                String insertSql = "";



                MySqlCommand comm_detail;
                MySqlDataReader dr_detail;





                MySqlConnection conn_detail = new MySqlConnection(connection);
                if (conn_detail.State != ConnectionState.Open)
                    conn_detail.Open();
                insertSql = "";
                string[] stepValue, divValue;
               

                divValue = new string[] { "", "", "" };
                stepValue = new string[] { "", "", "" };
                //宣告7組 
                step_caculator_value = new int[] { 0, 0, 0, 0, 0, 0, 0 };
                step_abs_value = new string[] { "", "", "" };

                //TextBox1.Text = stepValue[1];
                switch (vparameter)  //STEP 
                {
                    case "023": //pf ==>'023'
                        stepValue = new string[] { "2", "4", "6" };  //step 
                        divValue = new string[] { "2", "4", "6" };
                        step_abs_value = new string[] { "2", "4", "6" };
                        break;
                    case "010":  //cc1
                        stepValue = new string[] { "1", "3", "5" };
                        divValue = new string[] { "1", "3", "5" };
                        step_abs_value = new string[] { "1", "3", "5" };
                        break;
                    case "017": //cc2
                        if (vparameter_chg == "0172") //cc2-2 2024
                        {
                            stepValue = new string[] { "1", "3", "7" };
                            divValue = new string[] { "1", "3", "7" };
                            step_abs_value = new string[] { "1", "3", "7" };
                        }
                        else if (vparameter_chg == "017-chromaCC2") //cc2 for chroma 2024開始
                        {
                            stepValue = new string[] { "1", "3", "7" };
                            divValue = new string[] { "1", "3", "7" };
                            step_abs_value = new string[] { "1", "3", "7" };
                        }
                        else  //cc2 2023
                        {
                            stepValue = new string[] { "1", "5", "9" };
                            divValue = new string[] { "1", "5", "9" };
                            step_abs_value = new string[] { "1", "5", "9" };
                        }
                        break;


                }


             

                string detailVD = "", detailmAH = "", detailCurent = "", like_step = "";

                string tableTitleSql = "", columnSql = "", valueSql = "", detailSelect = "";

                string cc1SelectSql = "";



                string VD28 = "", VAHD28 = "", VD32 = "", VAHD32 = "", VD35 = "", VAHD35 = "";
                string VCCcurrent = "", VOCV = "", VaverageV1 = "", VaverageV2 = "", VaverageV3 = "", Vcharge34V = "";
                string Vcharge345V = "", Vcharge35V = "", Vtime50A = "", VV = "", VV1 = "", VV2 = "";
                string VV3 = "", VV4 = "", VmOhm = "", Vpara = "";

                //判讀碼A 位置1 --start--
                string CC2_interpretcode = "", CC2_position = "";
                //---end---


                if (dr.HasRows)
                {
                    //使用Read方法把資料讀進Reader，讓Reader一筆一筆順向指向資料列，並回傳是否成功。
                    while (dr.Read())
                    { //應該只有一筆

                        //重新清空存取電芯號碼存取列表
                        g_batterycell_number = new List<string>();
                        // Console.WriteLine("Number of rows returned: " + dr.FieldCount);
                        //開36個電芯號碼搜尋 , 先收集所有電芯號碼modle 
                        for (int ibattary = 1; ibattary <= 36; ibattary++)
                        {
                            string cell_Boxbatt = dr["fld" + BattaryID].ToString();

                            //測試如果沒有查到電芯號或是電芯號目前尚未建MSSQL表搜無---test start--------
                            //if (ibattary == 6 || ibattary == 12 || ibattary == 14 || ibattary == 20 || ibattary == 32)
                            //{
                            //    cell_Boxbatt = "MW2007HXXXXXXX".ToString();                            
                            //}
                            //if (ibattary != 100) cell_Boxbatt = "MW2007HXXXXXXX".ToString();
                            //-------end--------
                            g_batterycell_number.Add(cell_Boxbatt);
                            BattaryID += 7;
                        }

                        //這邊串接HTBI_K_Value_MapperType2_V 找尋 K_Value 所判定為ClassType所屬英文代號
                        Sync_HTBI_Merge_Classparam(STR_MSSQL_ARASHTBI, g_batterycell_number);


                        //檢視最後g_Batt_Classtype 存取狀態顯示
                        Console.WriteLine("電芯目前全classtype 36組顯示 = " + string.Join(", ", g_Batt_Classtype, g_Modle_CC_Kvalue));


                        //開36個insert 
                        for (int iFlag = 1; iFlag <= 36; iFlag++)
                        {
                            cc1SelectSql = "select max(a.VD28) VD28, max(a.VAHD28) VAHD28, max(a.VD32) VD32, max(a.VAHD32) VAHD32, max(a.VD35) VD35, max(a.VAHD35) VAHD35 ";
                            cc1SelectSql = cc1SelectSql + ",(select fld" + vComID + " as OCV from test_LoadPFData003 LIMIT 10, 1)  OCV  /*fld做變更*/ ";
                            cc1SelectSql = cc1SelectSql + " , max(a.CCcurrent) CCcurrent ";
                            cc1SelectSql = cc1SelectSql + ",((select sum(cast(fld" + (vComID) + " as decimal)) from test_LoadPFData003 b where b.fld7 = '" + divValue[0] + "' and ABS(b.fld" + (vComID + 1) + ") > '10') / ";
                            cc1SelectSql = cc1SelectSql + "((select sum(cast(fld7 as decimal)) from test_LoadPFData003 b where b.fld7 = '" + divValue[0] + "' and ABS(b.fld" + (vComID + 1) + ") > '10')/ " + divValue[0] + ")) averageV1 ";
                            cc1SelectSql = cc1SelectSql + ",((select sum(cast(fld" + (vComID) + " as decimal)) from test_LoadPFData003 b where b.fld7 = '" + divValue[1] + "' and ABS(b.fld" + (vComID + 1) + ") > '10') / ";
                            cc1SelectSql = cc1SelectSql + "((select sum(cast(fld7 as decimal)) from test_LoadPFData003 b where b.fld7 = '" + divValue[1] + "' and ABS(b.fld" + (vComID + 1) + ") > '10')/ " + divValue[1] + ")) averageV2 ";
                            cc1SelectSql = cc1SelectSql + ",((select sum(cast(fld" + (vComID) + " as decimal)) from test_LoadPFData003 b where b.fld7 = '" + divValue[2] + "' and ABS(b.fld" + (vComID + 1) + ") > '10') / ";
                            cc1SelectSql = cc1SelectSql + "((select sum(cast(fld7 as decimal)) from test_LoadPFData003 b where b.fld7 = '" + divValue[2] + "' and ABS(b.fld" + (vComID + 1) + ") > '10')/ " + divValue[2] + ")) averageV3 ";

                            /*fld12 要做+8 (變數)*/
                            cc1SelectSql = cc1SelectSql + ",(select max(cast(fld" + (vComID + 4) + " as decimal))  from test_LoadPFData003 where fld7 = '3' and fld" + (vComID + 1) + "  > '10' and fld" + (vComID) + "  <= '3.4') as 'charge34V' ";
                            cc1SelectSql = cc1SelectSql + ",(select  max(cast(fld" + (vComID + 4) + "  as decimal))  from test_LoadPFData003 where fld7 = '3' and fld" + (vComID + 1) + "  > '10' and fld" + (vComID) + "  <= '3.45') as 'charge345V' ";
                            cc1SelectSql = cc1SelectSql + ",(select  max(cast(fld" + (vComID + 4) + "  as decimal))   from test_LoadPFData003 where fld7 = '3' and fld" + (vComID + 1) + "  > '10' and fld" + (vComID) + "  <= '3.5') as  'charge35V' ";
                            //detailSelect  是用在 VLOOKUP  如VD28=XLOOKUP(1,(G11:G5000(STEP) =2)*(N11:N5000=JK8[Reached Target voltage] ),H11:H5000(n-6),0,0)  //每個parameter 底層都一樣
                            detailSelect = "from( "
                         + "select fld7, fld8, fld9 ,fld12, fld14, case when fld7 = '" + stepValue[0] + "' /*2*/ then  fld" + (vState - 6) + "  end VD28, case when fld7 = '" + stepValue[0] + "'  /*2*/ then  fld" + (vState - 2) + " end VAHD28 "
                            + ", case when fld7 = '" + stepValue[1] + "' /*4*/  then  fld" + (vState - 6) + "  end VD32, case when fld7 = '" + stepValue[1] + "' then  fld" + (vState - 2) + "  end VAHD32 "
                            + ", case when fld7 = '" + stepValue[2] + "'/*6*/ then  fld" + (vState - 6) + "  end VD35, case when fld7 = '" + stepValue[2] + "' then  fld" + (vState - 2) + "  end VAHD35 "
                            + " ,case when fld7 = '1' then fld" + (vState - 5) + "  end  'CCcurrent' " +
                            " from test_LoadPFData003  where fld" + vState + " = 'Reached Target voltage' ) a ";


                            //若沒有充電電壓flag 這邊用試算方式求出

                            switch (vparameter)
                            {
                                case "023": //pf
                                    sqlQuery = "select max(a.VD28) VD28, max(a.VAHD28) VAHD28, max(a.VD32) VD32, max(a.VAHD32) VAHD32, max(a.VD35) VD35, max(a.VAHD35) VAHD35 " + detailSelect;

                                    /* 變成DetailSelectSql 
                                    sqlQuery = sqlQuery + "from( ";
                                    sqlQuery = sqlQuery + "select fld7, fld8, fld9 ,fld12, fld14, case when fld7 = '2' then  fld" + (vState - 6) + "  end VD28, case when fld7 = '2' then  fld" + (vState - 2) + " end VAHD28 ";
                                    sqlQuery = sqlQuery + ", case when fld7 = '4' then  fld" + (vState - 6) + "  end VD32, case when fld7 = '4' then  fld" + (vState - 2) + "  end VAHD32 ";
                                    sqlQuery = sqlQuery + ", case when fld7 = '6' then  fld" + (vState - 6) + "  end VD35, case when fld7 = '6' then  fld" + (vState - 2) + "  end VAHD35 ";
                                    sqlQuery = sqlQuery + " ,case when fld7 = '1' then fld" + (vState - 5) + "  end  'CCcurrent' ";
                                    sqlQuery = sqlQuery + " from test_LoadPFData003  where fld" + vState + " = 'Reached Target voltage' ) a ";
                                    */
                                    break;
                                case "010":  //cc1
                                             //vComID = 8;//H欄     vState = 14;//N欄

                                    sqlQuery = cc1SelectSql + detailSelect; //+ " ) finalR ";

                                    break;

                                case "017":
                                    if (vparameter_chg == "0172" || vparameter_chg == "017-chromaCC2") //cc2-2 2024 , cc2 017-chroma2 2024開始
                                    {
                                        //SECI 走這段解析 V , V1 ,V2,V3,V4 ,育平之前定義的各項目count 總數
                                        if (vparameter_chg == "0172" && check_cc2_algorithm) // for 測試正常
                                         //   if (vparameter_chg == "0172")
                                        {
                                            //  jj7 5169 ,  jj8 8395    =(@INDIRECT((ADDRESS($JJ$7, JF14)), 1))                           
                                            cc1SelectSql = cc1SelectSql + ",(SELECT COUNT(*)  FROM test_LoadPFData003 WHERE fld7 = '3' and  cast( fld" + (vComID + 1) + "  as decimal) > 20) as time50A ";
                                            cc1SelectSql = cc1SelectSql + ",(select  fld" + vComID + " from test_LoadPFData003 limit 4976,1 ) as V ";
                                            cc1SelectSql = cc1SelectSql + ",(select  fld" + vComID + " from test_LoadPFData003 limit 5168,1) as V1 ";
                                            cc1SelectSql = cc1SelectSql + ",(select  fld" + vComID + " from test_LoadPFData003 limit 8394,1 ) as V2 ";
                                            cc1SelectSql = cc1SelectSql + ",(select  fld" + (vComID + 1) + " from test_LoadPFData003 limit 5114,1) as v3 ";
                                            cc1SelectSql = cc1SelectSql + ",(select  fld" + (vComID + 1) + " from test_LoadPFData003 limit 5178,1) as v4 ";
                                        }
                                        else // Chroma  走這段解析 V , V1 ,V2,V3,V4 ,這邊根據每個step 與 Reached Target voltage' 條件對應位置 算出count
                                        {
                                            //  jj7 5169 ,  jj8 8395    =(@INDIRECT((ADDRESS($JJ$7, JF14)), 1))                           
                                            cc1SelectSql = cc1SelectSql + ",(SELECT COUNT(*)  FROM test_LoadPFData003 WHERE fld7 = '3' and  cast( fld" + (vComID + 1) + "  as decimal) > 20) as time50A ";
                                            //計算五次
                                            for (int n = 0; n < 5; n++)
                                            {

                                                int cacula_number = Parse_chroma_V_serial_count(n, vComID, vComID + 1, connection);
                                                if (n <= 2)
                                                {
                                                    if (n == 0)
                                                    {
                                                        cc1SelectSql = cc1SelectSql + ",(select  fld" + vComID + " from test_LoadPFData003 limit " + (cacula_number) + ",1 ) as V ";
                                                    }
                                                    else
                                                    {
                                                        cc1SelectSql = cc1SelectSql + ",(select  fld" + vComID + " from test_LoadPFData003 limit " + (cacula_number) + ",1 ) as V" + (n);
                                                    }
                                                }
                                                else
                                                {

                                                    cc1SelectSql = cc1SelectSql + ",(select  fld" + (vComID + 1) + " from test_LoadPFData003 limit " + (cacula_number) + ",1 ) as V" + (n) + " ";
                                                }
                                            }

                                            //當原始數據沒有Reached Target voltage參考
                                            if (!haveTargetvoltage)
                                            {
                                                for (int k = 0; k < step_caculator_value.Count(); k++)
                                                {
                                                    if (k < 6)
                                                    {

                                                        if (k % 2 == 0 || k == 0)
                                                        {
                                                            //取VD 2.8, 3.2, 3.5 
                                                            if (k == 0)
                                                            {
                                                                detailVD = "absVD28";
                                                                like_step = step_abs_value[0];
                                                            }
                                                            else if (k == 2)
                                                            {
                                                                detailVD = "absVD32";
                                                                like_step = step_abs_value[1];
                                                            }
                                                            else if (k == 4)
                                                            {
                                                                detailVD = "absVD35";
                                                                like_step = step_abs_value[2];
                                                            }

                                                            cc1SelectSql = cc1SelectSql + ",( select abs(fld" + vComID + ")  from test_LoadPFData003 WHERE fld7 LIKE '" + like_step + "' limit " + (step_caculator_value[k] - 3) + " ,1 ) as " + detailVD + "";

                                                        }
                                                        else
                                                        {
                                                            //取mAH 2.8, 3.2, 3.5                                                             
                                                            if (k == 1)
                                                            {
                                                                detailmAH = "absmAH28";
                                                                like_step = step_abs_value[0];
                                                            }
                                                            else if (k == 3)
                                                            {
                                                                detailmAH = "absmAH32";
                                                                like_step = step_abs_value[1];
                                                            }
                                                            else if (k == 5)
                                                            {
                                                                detailmAH = "absmAH35";
                                                                like_step = step_abs_value[2];
                                                            }

                                                            cc1SelectSql = cc1SelectSql + ",( select abs(fld" + (vComID + 4) + ")  from test_LoadPFData003 WHERE fld7 LIKE '" + like_step + "' limit " + (step_caculator_value[k] - 3) + " ,1 ) as " + detailmAH + "";
                                                        }

                                                    }
                                                    else
                                                    {
                                                        //取current 電流                                                          
                                                        detailCurent = "absCurrentmA";
                                                        cc1SelectSql = cc1SelectSql + ",( select abs(fld" + (vComID + 1) + ")  from test_LoadPFData003 WHERE fld7 LIKE '1' limit " + (step_caculator_value[k] - 1) + " ,1 ) as " + detailCurent + " ";
                                                    }
                                                }

                                            }
                                        }


                                        sqlQuery = cc1SelectSql + detailSelect; //+ " ) finalR ";

                                    }
                                    else  //cc2 2023
                                    {
                                        //jj7 4893,jj8 4957  =(@INDIRECT((ADDRESS($JJ$7,JF14)),1))
                                        cc1SelectSql = cc1SelectSql + ",(SELECT COUNT(*)  FROM test_LoadPFData003 WHERE fld7 = '3' and  cast( fld" + (vComID + 1) + "  as decimal) > 20) as time50A ";
                                        cc1SelectSql = cc1SelectSql + ",(select  fld" + vComID + " from test_LoadPFData003 limit 492,1 ) as V ";
                                        cc1SelectSql = cc1SelectSql + ",(select  fld" + vComID + " from test_LoadPFData003 limit 4892,1) as V1 ";
                                        cc1SelectSql = cc1SelectSql + ",(select  fld" + vComID + " from test_LoadPFData003 limit 4956,1 ) as V2 ";
                                        cc1SelectSql = cc1SelectSql + ",(select  fld" + (vComID + 1) + " from test_LoadPFData003 limit 4838,1) as v3 ";
                                        cc1SelectSql = cc1SelectSql + ",(select  fld" + (vComID + 1) + " from test_LoadPFData003 limit 4902,1) as v4 ";

                                        sqlQuery = cc1SelectSql + detailSelect; //+ " ) finalR ";

                                    }

                                    break;
                            }//end switch

                            comm_detail = new MySqlCommand(sqlQuery, conn_detail);

                            dr_detail = comm_detail.ExecuteReader();

                            if (dr_detail.HasRows)  //找FUNCTION的值
                            {
                                while (dr_detail.Read())
                                {
                                    //PF

                                    /*cc 新增的欄位*/
                                    //,`CCcurrent`,`OCV`,`averageV1`,`averageV2`,`averageV3`
                                    //,`charge34V`,`charge345V`,`charge35V`,`time50A`,`v`
                                    //,`v1`,`v2`,`v3`,`v4`,`Para`
                                    //mOhm 欄位 ABS(KD16-KE16)/ABS(KF16-KG16)*1000 取得欄位後     Math.Abs();

                                    //string svd28 = Convert.ToString(dr_detail["absVD28"].ToString());
                                    //string smaH28 = Convert.ToString(dr_detail["absmAH28"].ToString());
                                    //string svd32 = Convert.ToString(dr_detail["absVD32"].ToString());
                                    //string smaH32 = Convert.ToString(dr_detail["absmAH32"].ToString());
                                    //string svd35 = Convert.ToString(dr_detail["absVD35"].ToString());
                                    //string smaH35 = Convert.ToString(dr_detail["absmAH35"].ToString());


                                    if (!haveTargetvoltage && vparameter == "017")
                                    {
                                        VD28 = Convert.ToString(dr_detail["absVD28"].ToString());
                                        VAHD28 = Convert.ToString(dr_detail["absmAH28"].ToString());
                                        VD32 = Convert.ToString(dr_detail["absVD32"].ToString());
                                        VAHD32 = Convert.ToString(dr_detail["absmAH32"].ToString());
                                        VD35 = Convert.ToString(dr_detail["absVD35"].ToString());
                                        VAHD35 = Convert.ToString(dr_detail["absmAH35"].ToString());
                                    }
                                    else
                                    {
                                        VD28 = Convert.ToString(dr_detail["VD28"].ToString());
                                        VAHD28 = Convert.ToString(dr_detail["VAHD28"].ToString());
                                        VD32 = Convert.ToString(dr_detail["VD32"].ToString());
                                        VAHD32 = Convert.ToString(dr_detail["VAHD32"].ToString());
                                        VD35 = Convert.ToString(dr_detail["VD35"].ToString());
                                        VAHD35 = Convert.ToString(dr_detail["VAHD35"].ToString());
                                    }


                                    Console.WriteLine($"3.5-2.8V Ah 電容量 =  { VAHD35}");

                                    switch (vparameter)
                                    {


                                        case "010": //cc1
                                             
                                            VCCcurrent = Convert.ToString(dr_detail["CCcurrent"].ToString());                                            
                                            VOCV = Convert.ToString(dr_detail["OCV"].ToString());
                                            VaverageV1 = Convert.ToString(dr_detail["averageV1"].ToString());
                                            VaverageV2 = Convert.ToString(dr_detail["averageV2"].ToString());
                                            VaverageV3 = Convert.ToString(dr_detail["averageV3"].ToString());
                                            Vcharge34V = Convert.ToString(dr_detail["charge34V"].ToString());
                                            Vcharge345V = Convert.ToString(dr_detail["charge345V"].ToString());
                                            Vcharge35V = Convert.ToString(dr_detail["charge35V"].ToString());

                                            //目前CHROMA 數據有問題  CHX_I(A) 都是負值,條件式需要大於10 , Current 目前因 Reached Target voltage無故無法收驗找到相對應值
                                            if (VCCcurrent.ToString() == "" || VaverageV1.ToString() == "" || VaverageV3.ToString() == "")
                                            {
                                                VCCcurrent = VaverageV1 = VaverageV3 = "0.0";
                                            }


                                            Vtime50A = "0";
                                            VV = "0";
                                            VV1 = "0";
                                            VV2 = "0";
                                            VV3 = "0";
                                            VV4 = "0";
                                            VmOhm = "0";
                                            Vpara = "CC1";


                                            break;
                                        case "017":  //cc2 or cc2-2

                                            if (!haveTargetvoltage)
                                            {
                                                VCCcurrent = Convert.ToString(dr_detail["absCurrentmA"].ToString());
                                            }
                                            else
                                            {
                                                VCCcurrent = Convert.ToString(dr_detail["CCcurrent"].ToString());
                                            }

                                            VOCV = Convert.ToString(dr_detail["OCV"].ToString());
                                            VaverageV1 = Convert.ToString(dr_detail["averageV1"].ToString());
                                            VaverageV2 = Convert.ToString(dr_detail["averageV2"].ToString());
                                            VaverageV3 = Convert.ToString(dr_detail["averageV3"].ToString());
                                            Vcharge34V = Convert.ToString(dr_detail["charge34V"].ToString());
                                            Vcharge345V = Convert.ToString(dr_detail["charge345V"].ToString());
                                            Vcharge35V = Convert.ToString(dr_detail["charge35V"].ToString());

                                            int cap_type = Assign_Cap_mAH_Type(VAHD35);
                                            
                                            CC2_interpretcode = Convert.ToString(g_Batt_Classtype[insert_num]) + cap_type.ToString("D2");
                                          
                                            int check_position = Determination_Type_Position(g_Batt_Classtype[insert_num], cap_type);

                                            CC2_position = Convert.ToString(check_position);

                                            //目前CHROMA 數據有問題  CHX_I(A) 都是負值,條件式需要大於10 , Current 目前因 Reached Target voltage無故無法收驗找到相對應值
                                            if (VCCcurrent.ToString() == "" || VaverageV1.ToString() == "" || VaverageV3.ToString() == "")
                                            {
                                                VCCcurrent = VaverageV1 = VaverageV3 = "0.0";
                                            }

                                    

                                            Vtime50A = Convert.ToString(dr_detail["time50A"].ToString());
                                            VV = Convert.ToString(dr_detail["V"].ToString());
                                            VV1 = Convert.ToString(dr_detail["V1"].ToString());
                                            VV2 = Convert.ToString(dr_detail["V2"].ToString());
                                            VV3 = Convert.ToString(dr_detail["V3"].ToString());
                                            VV4 = Convert.ToString(dr_detail["V4"].ToString());
                                            //=ABS(KD14-KE14)/ABS(KF14-KG14)*1000
                                            Vpara = "CC2";
                                            //Decimal divisor = Math.Abs(Convert.ToDecimal(VV3) - Convert.ToDecimal(VV4));
                                            //if (divisor == 0) divisor = 0.0039M;
                                           // VmOhm = Convert.ToString(Math.Abs(Convert.ToDecimal(VV1) - Convert.ToDecimal(VV2)) / divisor);
                                           // VmOhm = Convert.ToString(Math.Abs(Convert.ToDecimal(VV1) - Convert.ToDecimal(VV2)) / Math.Abs(Convert.ToDecimal(VV3) - Convert.ToDecimal(VV4)));

                                            if (g_Batt_Classtype[insert_num] != "?")
                                                VmOhm = Convert.ToString(Math.Abs(Convert.ToDecimal(VV1) - Convert.ToDecimal(VV2)) / Math.Abs(Convert.ToDecimal(VV3) - Convert.ToDecimal(VV4)));
                                            else
                                                VmOhm = "0.000";

                                            break;

                                    }
                                }

                                //vStart_date //設定測試的日期(因為是key，所以手動輸 0755  測試值上線要拿掉
                                //vStart_date = "2024/01/01 01:11:44";
                                //'2024/07/01 02:13:44'


                                //string tableTileSql = "", columnSql = "", valueSql = "";

                                ///insertSql = insertSql + " INSERT INTO pfprocess001  ";



                                valueSql = "VALUES ( '" + dr["fld" + vComID].ToString() + "',  '" + vStart_date + "','" + vdateEnd_date + "','" + vtary_ID + "','" + vparameter + "',";
                                valueSql = valueSql + " '" + dr["fld" + vState].ToString() + "' ,'" + VD28 + "','" + VD28 + "','" + VAHD28 + "','" + VAHD28 + "',";
                                valueSql = valueSql + " '" + VD32 + "' ,'" + VD32 + "','" + VAHD32 + "','" + VAHD32 + "','" + VD35 + "',";
                                valueSql = valueSql + " '" + VD35 + "' ,'" + VAHD35 + "','" + VAHD35 + "','" + Filename + "','" + vprocess + "',now()";

                                //select CCcurrent, OCV, averageV1, averageV2, averageV3, charge34V, charge345V, charge35V
                                //  , time50A, v, v1, v2, v3, v4, mOhm from processcc
                                switch (vparameter)
                                {
                                    case "023": //pf
                                        tableTitleSql = " INSERT INTO pfprocess001  ";
                                        tableTitleSql = tableTitleSql + " (ID,StartDateD,EnddateD,trayID,parameter ";
                                        tableTitleSql = tableTitleSql + ",State,VD28,VS28,VAHD28,VAHS28 ";
                                        tableTitleSql = tableTitleSql + " ,VD32 ,VS32 ,VAHD32,VAHS32 ,VD35  ";
                                        tableTitleSql = tableTitleSql + " ,VS35,VAHD35 ,VAHS35,FileName,Process,AnlaysisDayD,interpretcode,position ";

                                        //PF 這邊後續電芯判讀號和位置新增計算結果值加入---start-----
                                        valueSql = valueSql + " ,'" + CC2_interpretcode + "','" + CC2_position + "'";
                                        //------stop----------------
                                        insertSql = insertSql + tableTitleSql + " ) " + valueSql + ");";
                                        break;
                                    default: //cc2>cc1 所以有7個欄位 寫0

                                        /*cc 新增的欄位*/
                                        //,`Para`,`CCcurrent`,`OCV`,`averageV1`,`averageV2`,`averageV3`,`charge34V`,`charge345V`,`charge35V`
                                        //,`time50A`,`v`
                                        //,`v1`,`v2`,`v3`,`v4`
                                        //mOhm 欄位 ABS(KD16-KE16)/ABS(KF16-KG16)*1000 取得欄位後     Math.Abs();

                                        tableTitleSql = " INSERT INTO processcc";
                                        columnSql = "(ID,StartDateD,EnddateD,trayID,parameter ";
                                        columnSql = columnSql + ",State,VDA,VSA,VAHDA,VAHSA ";
                                        columnSql = columnSql + " ,VDB ,VSB ,VAHDB,VAHSB ,VDC  ";
                                        columnSql = columnSql + " ,VSC,VAHDC ,VAHSC,FileName,Process,AnlaysisDayD ";
                                        columnSql = columnSql + ",Para ";
                                        columnSql = columnSql + ",CCcurrent,OCV,averageV1,averageV2,averageV3,charge34V,charge345V,charge35V"; //cc1有的，
                                        columnSql = columnSql + ",time50A,v,v1,v2,v3,v4,mOhm,interpretcode,position,analysisDT"; //cc2才有的，cc1要塞的話，值均為0 mOhm 是用算值出來的

                                        valueSql = valueSql + ",'" + Vpara + "'";  //para
                                        valueSql = valueSql + ", " + VCCcurrent + "," + VOCV + "," + VaverageV1 + "," + VaverageV2 + "," + VaverageV3 + "," + Vcharge34V + "," + Vcharge345V + "," + Vcharge35V;
                                        valueSql = valueSql + ", " + Vtime50A + ", " + VV + ", " + VV1 + ", " + VV2 + ", " + VV3 + ", " + VV4 + ", " + VmOhm + ", " + "'" + CC2_interpretcode + "'" + ", " + "'" + CC2_position + "'" + ", now()" + ") ";
                                        //新增vvalueSql//增加value(
                                        insertSql = insertSql + tableTitleSql + columnSql + " ) " + valueSql + ";";
                                        break;

                                }

                                //alueSql = valueSql + ") ; ";

                                vComID = vComID + 7;
                                vState = vState + 7;
                                insert_num++;

                                dr_detail.Close();
                            }

                        }
                    }
                    if (conn_detail.State != ConnectionState.Closed)
                        conn_detail.Close();

                    //LResult.Text = insertSql;



                    //String testSql = "insert INTO pfprocess001  (ID,StartDateD,EnddateD,trayID,parameter ,State,VD28,VS28,VAHD28,VAHS28  ,VD32 ,VS32 ,VAHD32,VAHS32 ,VD35   ,VS35,VAHD35 ,VAHS35,FileName,Process,AnlaysisDayD)VALUES ( 'MW2007A05101',  '2024/01/01 02:17:02','2024/01/01 07:15:14','PF-03-K000001','023', 'OK' ,'2.8000','2.8000','2627.0','2627.0', '3.3000' ,'3.3000','13802.2','13802.2','3.4000', '3.4000' ,'30400.0','30400.0','0000001.txt','00:Pressure Formation',now()) ; ";
                    //testSql = testSql + "insert INTO pfprocess001(ID, StartDateD, EnddateD, trayID, parameter, State, VD28, VS28, VAHD28, VAHS28, VD32, VS32, VAHD32, VAHS32, VD35, VS35, VAHD35, VAHS35, FileName, Process, AnlaysisDayD)VALUES('MW2007A05101', '2024/01/01 02:18:02', '2024/01/01 07:16:14', 'PF-03-K000001', '023', 'OK', '2.8000', '2.8000', '2627.0', '2627.0', '3.3000', '3.3000', '13802.2', '13802.2', '3.4000', '3.4000', '30400.0', '30400.0', '0000001.txt', '00:Pressure Formation', now()); ";

                    String testSql = insertSql;

                    MySqlConnection conn_exec = new MySqlConnection(connection);

                    if (conn_exec.State != ConnectionState.Open)
                        conn_exec.Open();
                    //MySqlCommand cmd = new MySqlCommand(testSql, conn_exec);
                    cmd = new MySqlCommand(testSql, conn_exec);
                    try
                    {
                        cmd.ExecuteNonQuery(); //insert 36筆
                        LResult.Text = "已完成";

                    }
                    catch (Exception ex)
                    {
                        LResult.Text = "資料錯誤" + ex.ToString();
                    }


                    if (conn_exec.State != ConnectionState.Closed)
                        conn_exec.Close();

                }

                if (conn.State != ConnectionState.Closed)
                    conn.Close();

                string OriginallFile = Filename;

                //將重新解析的(PF or CC1 or CC2)存成csv,並呈現table含數據於頁面上
                //只取檔案名稱,忽略副檔名
                Filename = Path.GetFileNameWithoutExtension(Filename);

                switch (vparameter)  //STEP 
                {
                    case "023": //pf ==>'023'
                        dumpcsv = "SELECT * FROM sakila.pfprocess001;";
                        Filename = Filename + "-pfprocess001.csv";
                        pfcc_tablename = "pfprocess001";
                        break;
                    case "010":  //cc1
                        dumpcsv = "SELECT * FROM sakila.processcc;";
                        Filename = Filename + "-process-cc1.csv";
                        pfcc_tablename = "processcc";
                        break;
                    case "017": //cc2
                        dumpcsv = "SELECT * FROM sakila.processcc;";

                        if (vparameter_chg == "0172") //cc2-2 2024
                        {
                            Filename = Filename + "-process-cc2-2.csv";
                        }
                        else if (vparameter_chg == "017-chromaCC2")
                        {
                            Filename = Filename + "-process_chroma-cc2.csv";
                        }
                        else
                        {
                            Filename = Filename + "-process-cc2.csv";
                        }
                        pfcc_tablename = "processcc";
                        break;
                }

                //目標目錄(C:\\pf-cc)不存在則新建
                if (!Directory.Exists(ResultTaskFolder))
                    Directory.CreateDirectory(ResultTaskFolder);

                //先行判斷執行續是否有正常處理36組row data
                int ExistRowNumber = GetRowCount(connection, dumpcsv);

                //正常處理數據量(目前依照制定為36組compnet)
                if (ExistRowNumber >=36) {
                    ExportToCsv resultcsv = new ExportToCsv();

                    //(1)先將分析數據產生export csv格式檔
                    //  string pfccPath_File = Server.MapPath("~/" + "pf-cc" + "/")+ Filename;
                    string pfccPath_File = Path.Combine(ResultTaskFolder, Filename);

                    //(1)先將分析數據產生export csv格式檔
                    DataTable dtView = resultcsv.Export(connection, dumpcsv, pfccPath_File);

                    //csvview.DataSource = dtView;
                    //csvview.DataBind();
                    // LResult.Text = "篩選型號資料完畢!";


                    string sDayD_value = string.Empty;
                    
                   
                    if (vparameter == "023")
                    {
                        //PF站不需要的欄位過濾
                        sDayD_value = "XXXXXX";
                    }
                    else
                    {
                       //CC1&2站不需要的欄位過濾
                       sDayD_value = "AnlaysisDayD";
                    }


                    //(2)再將分析完的數據合併預先遠端建置之的table (這邊目前使用遠端表單為  testmerge_pf 和 testmerge_cc1orcc2)
                    //目前所有(pc,cc1,cc2,cc2-2)都忽略以下欄位
                    All_col_listname = $@"SELECT GROUP_CONCAT(CASE
                       WHEN COLUMN_NAME NOT IN( 'State', 'Process','{sDayD_value}') THEN COLUMN_NAME
                        ELSE NULL
                        END  ORDER BY ORDINAL_POSITION) AS col_list
                        FROM INFORMATION_SCHEMA.COLUMNS
                        WHERE TABLE_NAME = '{pfcc_tablename}'
                        AND TABLE_SCHEMA = '{schema_DB}'; ";

                    List<string> Del_ComplateFile = new List<string>();
                    Del_ComplateFile.Add(OriginallFile);

                    //若合併指定遠端table 成功則往繼續執行下方最後作業!
                    if (resultcsv.Merge_existfilter_value(connection, connection_merge, All_col_listname, schema_DB, pfcc_tablename) == true)
                    {
                        //刪除已經完成之數據原始檔案
                        DeleteTHreadOKFiles(SourceFolder, Del_ComplateFile, true);
                    }
                }
                else {
                    //轉換過程row data 少於36組
                    //收集執行緒異常的檔案名稱!
                    g_ThreadNotOkFile.Add(OriginallFile);

                    //將NG未處理完畢之檔案名稱紀錄後續追蹤
                    WriteNGToFile(NG_file_record, OriginallFile);

                }
                

            } //end if (讀檔錯誤判斷====>)
        }

        public string SqlVal_Define(string v)
        {          
            if (v == "") return "''";
            return v;
        }

        public void RewriteAndAppendToFile(string filePath, List<string> newLines)
        {
            // 讀取舊檔案內容
            List<string> existingLines = new List<string>();
            DateTime currentDateTime = DateTime.Now;
            string formattedFull = currentDateTime.ToString("yyyy-MM-dd HH:mm:ss");
          
            if (!File.Exists(filePath))
            {
                // 如果檔案不存在，創建並寫入初始內容
                File.WriteAllText(filePath, "=== PFCC電化學的分析錯誤記錄 ===\n");
            }

            if (File.Exists(filePath))
            {
                existingLines.AddRange(File.ReadAllLines(filePath)); // 讀取所有行
            }

            //新增一組系統當前日期時間
            existingLines.Add(formattedFull);

            // 在舊內容後添加新內容
            using (StreamWriter writer = new StreamWriter(filePath, false)) // 設定為 false 以覆蓋檔案
            {
                foreach (string line in existingLines)
                {
                    writer.WriteLine(line); // 寫入舊內容
                }
                
                writer.WriteLine("----------------------新NG紀錄▽-------------------------"+ Environment.NewLine);

                foreach (string newLine in newLines)
                {
                    writer.WriteLine(newLine); // 寫入新內容
                }

                writer.WriteLine("----------------------結束-------------------------" + Environment.NewLine); // 寫入新內容
            } // 自動關閉 StreamWriter
        }


        public void CopyErrorRecordWith_Z_Backup()
        {
            string sourceFile = NG_STARUS_record;
            string primaryFolder = NG_STARUS_record_copy_Z;  // 主要目標 (Z碟)
            string fileName = Path.GetFileName(sourceFile);

            try
            {
                // 若無Z碟目的資料夾不存在則建立
                if (!Directory.Exists(primaryFolder))
                {
                    Directory.CreateDirectory(primaryFolder);
                }

                // 🕒 生成帶時間戳記的檔名（避免覆蓋）
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(sourceFile);
                string extension = Path.GetExtension(sourceFile);
                string newFileName = $"{fileNameWithoutExt}_localhost{extension}";
              //  string destinationFile = Path.Combine(primaryFolder, newFileName);

                string destinationPath = primaryFolder + Path.DirectorySeparatorChar + newFileName;
               
                if (File.Exists(destinationPath))
                {
                    // 若檔案已經存在，你可以選擇覆蓋或跳過檔案
                    // 覆蓋檔案
                    // 覆蓋寫入檔案
                    File.Copy(sourceFile, destinationPath, overwrite: true);
                }
                else
                {
                    // 覆蓋寫入檔案
                    File.Copy(sourceFile, destinationPath);                
                }

                //  Console.WriteLine($"✅ 檔案已成功複製至：{destinationFile}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 複製至{primaryFolder}失敗：{ex.Message}");
            }
        }

        public void WriteNGToFile(string filePath, string lines)
        {

            // 使用 using 語句確保 StreamWriter 會正確關閉
            using (StreamWriter writer = new StreamWriter(filePath, true)) // 設定為 true 以啟用附加模式
            {
              //  foreach (string line in lines)
                {
                    writer.WriteLine(lines); // 寫入每一行
                }
            } // 此處自動關閉 StreamWriter 和檔案
        }

        public int GetRowCount(string connectionString, string query)
        {
            int rowCount = 0;

            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new MySqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        rowCount++;
                    }
                }
            }

            return rowCount;
        }

       

        protected void Timer1_Tick(object sender, EventArgs e)
        {            
            int time = (int)ViewState["time"];

            if (time ==20 || timerunheck ==false) {
                ViewState["time"] = 0;
                //Label1.Text = "重編結束";
                this.Label2.Text = "工作限制60秒已經結束";
            }
            else {
                
                ViewState["time"] = time + 1;
                this.Label1.Text = "等待(" + Convert.ToString(time) + ")秒";
            }

           
        }

        protected void ver_select_SelectedIndexChanged(object sender, EventArgs e)
        {
            sVer = this.ver_select.SelectedItem.ToString();
        }

        protected void Button_Loop_Click(object sender, EventArgs e)
        {
            int totalTasks = 0, succesfulnum = 0;
            g_csvFile = new List<string>();
            g_pfcctype = new List<string>();
            g_ThreadNotOkFile = new List<string>();

            String DestinationFolder = "c:\\\\tempcsv";
            String fileExtension = "csv";
            string pf_cctable = "";
            bool IsOverWrite = true;
            bool copy_one = false; //false -> 複製全部 / true -> 複製單項
            bool check_have_ng = false; //確認file分析 flag ,預設false
            bool check_modlename_nodata = false; //預設電芯號都無搜尋 false
            int  check_index_overflow_number ; //預設查詢電芯序號序號為正常沒有溢位,0
            int  check_ng_num ; //NG file 偵測電芯號 數量 ,預設為0

            // 創建NG資料夾（如果不存在）
            if (!Directory.Exists(NG_file_Path))
            {
                Directory.CreateDirectory(NG_file_Path);               
            }

            //將 C:\copy_temp\source_pfcc 資料夾內csv全部複製到 C:\tempcsv
            CopyDirectory(SourceFolder, DestinationFolder, IsOverWrite, copy_one);

            string[] files = Directory.GetFiles(DestinationFolder, $"*.{fileExtension}");


            //只對*.csv檔案格式做工作序列
            foreach (string file in files)
            {
                //擷取開頭站點字串( pf:K000008 , CC1:H000014   CC2:H000020)
                //pfprocess001  存pf檔  PRIMARY KEY (`ID`,`StartDateD`)
                //processcc 存cc檔(含cc1, cc2)，PRIMARY KEY(`ID`,`StartDateD`)

                String flitersite = Path.GetFileName(file).Substring(0, 3);

                //由上搜尋站點字串判斷要清除當前一站暫存table內容
                //for pf 
                if (flitersite.Equals("K00") || flitersite.Equals("PF0"))
                {
                    pf_cctable = "pfprocess001";
                }// for cc1 或 cc2
                else if (flitersite.Equals("H00") || flitersite.Equals("CC-") || flitersite.Equals("CC0"))
                {
                    pf_cctable = "processcc";
                }
                else
                {
                    // LResult.Text = file+"->沒有符合此(pf,cc系列)工作項目csv!";
                    continue;
                    // return;
                }

                //將檔案名稱/ pfcctype做存取
                g_csvFile.Add(file);
                g_pfcctype.Add(pf_cctable);
                totalTasks++;
            }


            if (totalTasks == 0)
            {
                LResult.Text = "目前全沒有符合此(pf,cc系列)工作項目csv! / 請執行copy_pfcc.bat";
                return;
            }

            //宣告NG 可能性
            g_NG_PFCC_File = new List<string>();
            //錯誤狀態清空重新記錄
            g_ERROR_STATUS = new List<string>();

            //宣告總收集error raw data 後續insert
            List<ErrorRaw> g_total_error_raw = new List<ErrorRaw>();

            //宣告global MES 連線池
            String g_Mes_connectpool = "" , g_remote_connect="";


            for (int i = 0; i < totalTasks; i++)
            {
                bool skipCurrentFile = false;
                string taskId = g_csvFile[i].ToString();
                string tasktype = g_pfcctype[i].ToString();


                //------增加 NG檔案 判斷-----start--------
                check_have_ng = false;
                check_ng_num = 0;
                check_index_overflow_number = 0;
                check_modlename_nodata = false;                
                //------end-------- 

                LResult.Text = $"處理表單{taskId}進行中.....";

                //Yuping 本機端MYSQL 設定
                //string connection = "server=localhost;user id=root;password=27763923;database=sakila; pooling=true;";

                //目前佈署端local host MYSQL 設定
               // string connection = "server=localhost;user id=root;password=Xcold@246810;database=sakila; pooling=true;Min Pool Size=0;Max Pool Size=3000;";

                //目前開發本機端MYSQL 設定
                string connection = "server=localhost;user id=root;password=K@admin123456;database=sakila; pooling=true;Min Pool Size=0;Max Pool Size=3000;";


                //遠端remote合併 hr.test_mergepfcc MYSQL 設定
                string connection_merge = "server=192.168.3.100;user id=root;password=Admin0331;database=mes; pooling=true;Min Pool Size=0;Max Pool Size=3000;";
                g_Mes_connectpool = connection_merge;

                string STR_MSSQL_ARASHTBI = string.Format("server={0};database={1};uid={2};pwd={3};Connect Timeout = 180", MS_Server, MS_Database, MS_dbuid, MS_dbpwd);


                MySqlConnection conn = new MySqlConnection(connection);

                //pf,cc1,cc2工作數據暫訂upload 位置資料夾(SourceFolder)
                // String SourceFolder = "z:\\\\source_pfcc";
                //String SourceFolder = @"Z:\source_pfcc";
                //因佈署後UNC路徑目前無法透過磁區辨別,只能由源頭IP位置找尋
                //  String SourceFolder = @"\\192.168.3.100\hr_tmp\source_pfcc";
                String Filename = "", pfcc_tablename = "";
                string schema_DB = "sakila";  // 本機PFCC工具資料庫名稱


                String LoadSql = "";

                String dumpcsv = "";
                String merge_sql_var = "", merge_table_rowdata = "", All_col_listname = "" ;                
                bool iscsvexist = false;               
                bool mannulrun = true;
                //load 資料

                sVer = this.ver_select.SelectedItem.ToString();
                //PF + CC 

                pfcc_tablename = tasktype.ToString();
                Filename = taskId.ToString();  //Filename = "H000003_20230910130027.csv"; //讀路徑下的檔案

                String loadcsvFile = Path.GetFileName(Filename);

                LoadSql = "delete from test_loadpfdata003; ";    //刪除暫存TABLE
                LoadSql = LoadSql + "TRUNCATE TABLE " + pfcc_tablename + "; ";
                //實際路徑是 C:\ProgramData\MySQL\MySQL Server 8.0\Data\test\
                LoadSql = LoadSql + " load data infile 'c:\\\\tempcsv\\\\" + loadcsvFile + "' into table test_loadpfdata003 fields terminated by ',' ;";
                // LoadSql = LoadSql + " insert into test_loadpfdata003 (fld1) values('"+ Filename + "') ; ";

                // MySqlConnection conn = new MySqlConnection(connection);
                string fileResult = "1";

                if (conn.State != ConnectionState.Open)
                    conn.Open();

                MySqlCommand cmd = new MySqlCommand(LoadSql, conn);

                try
                {
                    cmd.ExecuteNonQuery();
                    LResult.Text = "已完成載檔";
                    fileResult = "1";

                }
                catch (Exception ex)
                {
                    cmd.Clone();
                    conn.Close();
                    //LResult.Text = "資料錯誤" + ex.ToString();
                    LResult.Text = "上傳資料檔案錯誤";
                    fileResult = "err";
                }
                conn.Close();


                if (fileResult == "1" && !skipCurrentFile)
                {
                    //總共要塞的欄位
                    // insert into pfprocess001()
                    //ID,Start dateEnd date,tary ID,	parameter,State,2.8V,2.8V Ah,3.2V,3.2V Ah,3.5V,3.5V Ah,	file name,process,Anlaysis day

                    //取固定值---起始日、終止日
                    string sqlQuery = "";
                    sqlQuery = sqlQuery + "/*title */";
                    //sqlQuery = sqlQuery + " (select fld5 as f_title,1 as sort from test_LoadPFData003 LIMIT 9, 1)  /*start_date */ ";
                    sqlQuery = sqlQuery + "( select case when (SUBSTRING(fld5, 2, 1) = '/') or (SUBSTRING(fld5, 3, 1) = '/')  then CONVERT(STR_TO_DATE(fld5, '%m/%d/%Y %T'), DATETIME)  else CONVERT(fld5, DATETIME)  end f_title ,1 as sort  from test_LoadPFData003 LIMIT 9, 1 )  ";
                    sqlQuery = sqlQuery + "union all ";
                    //sqlQuery = sqlQuery + "(select fld5 as f_title,2 as sort from test_LoadPFData003 order by fld5 desc LIMIT 1, 1) /*end_date */ ";
                    //sqlQuery = sqlQuery + "union all ";
                    sqlQuery = sqlQuery + "( select max(a.result) as f_title,2 as sort from (SELECT    CASE        WHEN(            SELECT COUNT(*)    FROM test_LoadPFData003  WHERE fld5 <> 'PC Time'  AND((SUBSTRING(fld5, 2, 1) = '/')or(SUBSTRING(fld5, 3, 1) = '/'))        ) > 0 ";
                    sqlQuery = sqlQuery + " THEN CONVERT(STR_TO_DATE(fld5, '%m/%d/%Y %T'), DATETIME)        ELSE CONVERT(fld5, DATETIME)    END AS result    from test_LoadPFData003    ) a ) /*end_date */ ";
                    sqlQuery = sqlQuery + "union all ";
                    sqlQuery = sqlQuery + "(select CONCAT ((select fld2 from test_LoadPFData003 LIMIT 3, 1) , '-' , (select fld2 from test_LoadPFData003 LIMIT 0, 1) ) as f_title,3 as sort ) ";
                    sqlQuery = sqlQuery + "/*tray_id*/ ";
                    sqlQuery = sqlQuery + "union all ";
                    sqlQuery = sqlQuery + "(select fld2 as f_title,4 as sort from test_LoadPFData003 LIMIT 2, 1)  /*parameter*/ ";
                    //sqlQuery = sqlQuery + "c  /*parameter*/ ";
                    sqlQuery = sqlQuery + "union all ";
                    sqlQuery = sqlQuery + "(select fld2 as f_title,5 as sort from test_LoadPFData003 LIMIT 1, 1)  /*process*/ ";
                    sqlQuery = sqlQuery + "union all ";
                    sqlQuery = sqlQuery + "(select now() as f_title,6 as sort ) /*Anlaysis day*/ ";

                    MySqlCommand comm = new MySqlCommand(sqlQuery, conn);
                    if (conn.State != ConnectionState.Open)
                        conn.Open();
                    MySqlDataReader dr = comm.ExecuteReader();



                    string vStart_date = "";
                    string vdateEnd_date = "";
                    string vtary_ID = "";
                    string vparameter = "";
                    string vparameter_chg = "";


                    string vparameter_All = "";
                    string vprocess = "";

                    string sort_temp = "";

                    bool check_cc2_algorithm = false, haveTargetvoltage = true;



                    //title 列
                    if (dr.HasRows)
                    {
                        //使用Read方法把資料讀進Reader，讓Reader一筆一筆順向指向資料列，並回傳是否成功。
                        while (dr.Read())
                        {

                            String flitersite = Path.GetFileName(Filename).Substring(0, 3);

                            //DataReader讀出欄位內資料的方式，通常也可寫Reader[0]、[1]...[N]代表第一個欄位到N個欄位。
                            //ss += Convert.ToString(dr["city_id"].ToString() + " -> " + dr["city"].ToString() + " -> " + dr["country_id"].ToString() + "\r\n");
                            sort_temp = Convert.ToString(dr["sort"].ToString());
                            switch (sort_temp)
                            {
                                case "1":
                                    vStart_date = Convert.ToString(dr["f_title"].ToString());
                                    break;
                                case "2":
                                    vdateEnd_date = Convert.ToString(dr["f_title"].ToString());
                                    break;
                                case "3":
                                    vtary_ID = Convert.ToString(dr["f_title"].ToString());
                                    break;
                                case "4":
                                    vparameter_All = Convert.ToString(dr["f_title"].ToString());

                                    //for chroma
                                    if (flitersite.Equals("PF0") || flitersite.Equals("CC0"))
                                    {

                                        vparameter = vparameter_All.Substring(2, 3);
                                        vparameter_chg = vparameter_All.Substring(vparameter_All.Length - 17);

                                        // 當vparameter 為017 -> CC2時,目前下面做記號
                                        if (vparameter.StartsWith("017") && vparameter_chg.Contains("CC2"))
                                        {
                                            vparameter_chg = vparameter + "-chromaCC2";
                                        } //當vparameter 為010 -> CC1時,目前下面做記號
                                        else if (vparameter.StartsWith("010") && vparameter_chg.Contains("CC1"))
                                        {
                                            vparameter_chg = vparameter + "-chromaCC1";
                                        }//當vparameter 為010 -> CC1時,目前下面做記號
                                        else if (vparameter.StartsWith("023") && vparameter_chg.Contains("PF"))
                                        {
                                            vparameter_chg = vparameter + "-chromaPF";
                                        }
                                        else
                                        {
                                            //其他未定義
                                            vparameter_chg = vparameter;
                                        }

                                    }
                                    else
                                    {
                                        // for SECI 
                                        vparameter = vparameter_All.Substring(0, 3);
                                        vparameter_chg = vparameter_All.Substring((vparameter_All.Length - 9), 4);
                                        if (vparameter_chg == "2023")
                                        {
                                            vparameter_chg = vparameter;
                                        }
                                        else
                                        {
                                            vparameter_chg = vparameter + "2";
                                        }
                                    }

                                    break;
                                case "5":
                                    vprocess = Convert.ToString(dr["f_title"].ToString());
                                    break;
                                default:

                                    break;
                            }

                        }

                    }//if (dr.HasRows)


                    //每一個 Cell ID有7行 第一筆H~N行
                    //H:Ch1_V(V)	I:Ch1_I(A)	J:Ch1_PV(V)	k:Ch1_OV(V)	L:Ch1_Capa(mAh) 	M:Ch1_Wh(Wh)	N:Ch1_Remark 每一組7行所以+7


                    int vComID = 8; //comid 如MW2005A53693 第一筆H 欄+7 為第二組 Ch1_V(V)
                    int vState = 14; //N 欄 如 ok: Ch1_Remark
                                     //展36筆
                    String tempSql = " select ";
                    for (int iFlag = 1; iFlag <= 36; iFlag++)
                    {

                        tempSql = tempSql + "fld" + vComID + ", fld" + vState + ",";
                        vComID = vComID + 7;  //第一筆H 行第8行 +7(每組7行)
                        vState = vState + 7;  //第一筆N 行第14行 +7(每組7行)
                    }
                    tempSql = tempSql.Substring(0, tempSql.Length - 1) + " from test_LoadPFData003 LIMIT 7, 1 "; //因為取標頭只有一列
                                                                                                                 // tempSql = tempSql  + " from test_LoadPFData003 LIMIT 7, 1 ";

                    dr.Close();

                    comm = new MySqlCommand(tempSql, conn);
                    dr = comm.ExecuteReader();

                    int BattaryID = 8, battary_count = 0, insert_num = 0;
                    vComID = 8;
                    vState = 14;
                    String insertSql = "";



                    MySqlCommand comm_detail;
                    MySqlDataReader dr_detail;





                    MySqlConnection conn_detail = new MySqlConnection(connection);
                    if (conn_detail.State != ConnectionState.Open)
                        conn_detail.Open();
                    insertSql = "";
                    string[] stepValue, divValue;

                    divValue = new string[] { "", "", "" };
                    stepValue = new string[] { "", "", "" };

                    //宣告7組 
                    step_caculator_value = new int[] { 0, 0, 0, 0, 0, 0, 0 };
                    step_abs_value = new string[] { "", "", "" };


                    //TextBox1.Text = stepValue[1];
                    switch (vparameter)  //STEP 
                    {
                        case "023": //pf ==>'023'
                            stepValue = new string[] { "2", "4", "6" };  //step 
                            divValue = new string[] { "2", "4", "6" };
                            step_abs_value = new string[] { "2", "4", "6" };
                            break;
                        case "010":  //cc1
                            stepValue = new string[] { "1", "3", "5" };
                            divValue = new string[] { "1", "3", "5" };
                            step_abs_value = new string[] { "1", "3", "5" };
                            break;
                        case "017": //cc2
                            if (vparameter_chg == "0172") //cc2-2 2024
                            {
                                stepValue = new string[] { "1", "3", "7" };
                                divValue = new string[] { "1", "3", "7" };
                                step_abs_value = new string[] { "1", "3", "7" };
                            }
                            else if (vparameter_chg == "017-chromaCC2") //cc2 for chroma 2024開始
                            {
                                stepValue = new string[] { "1", "3", "7" };
                                divValue = new string[] { "1", "3", "7" };
                                step_abs_value = new string[] { "1", "3", "7" };
                            }
                            else  //cc2 2023
                            {
                                stepValue = new string[] { "1", "5", "9" };
                                divValue = new string[] { "1", "5", "9" };
                                step_abs_value = new string[] { "1", "5", "9" };
                            }
                            break;
                    }


                    string detailVD = "", detailmAH = "", detailCurent = "", like_step = "";

                    string tableTitleSql = "", columnSql = "", valueSql = "", detailSelect = "";

                    string cc1SelectSql = "";



                    string VD28 = "", VAHD28 = "", VD32 = "", VAHD32 = "", VD35 = "", VAHD35 = "";
                    string VCCcurrent = "", VOCV = "", VaverageV1 = "", VaverageV2 = "", VaverageV3 = "", Vcharge34V = "";
                    string Vcharge345V = "", Vcharge35V = "", Vtime50A = "", VV = "", VV1 = "", VV2 = "";
                    string VV3 = "", VV4 = "", VmOhm = "", Vpara = "";

                    //判讀碼A 位置1 --start--
                    string CC2_interpretcode = "", CC2_position = "";
                    //---end---

                    //K值 
                    string Get_K_Value = "";

                    if (dr.HasRows)
                    {
                        //使用Read方法把資料讀進Reader，讓Reader一筆一筆順向指向資料列，並回傳是否成功。
                        while (dr.Read())
                        { //應該只有一筆

                            //重新清空存取電芯號碼存取列表
                            g_batterycell_number = new List<string>();
                            // Console.WriteLine("Number of rows returned: " + dr.FieldCount);
                            //開36個電芯號碼搜尋 , 先收集所有電芯號碼modle 
                            for (int ibattary = 1; ibattary <= 36; ibattary++)
                            {
                                string cell_Boxbatt = dr["fld" + BattaryID].ToString();

                                //測試如果沒有查到電芯號或是電芯號目前尚未建MSSQL表搜無---test start--------
                                //if (ibattary == 6 || ibattary == 12 || ibattary == 14 || ibattary == 20 || ibattary == 32)
                                //{
                                //    cell_Boxbatt = "MW2007HXXXXXXX".ToString();                            
                                //}
                                //if (ibattary != 100) cell_Boxbatt = "MW2007HXXXXXXX".ToString();
                                //-------end--------                                
                                if (cell_Boxbatt.Equals("") )
                                {

                                  //  Console.WriteLine("第" + ibattary + "個電芯號" + cell_Boxbatt + "不加入分析");
                                    BattaryID += 7;
                                }
                                else
                                {
                                    g_batterycell_number.Add(cell_Boxbatt);
                                    BattaryID += 7;
                                }
                            }

                            if (g_batterycell_number.Count() == 0)
                            {
                                check_ng_num++;
                                check_modlename_nodata = true;
                                if (check_ng_num == 1 && check_modlename_nodata)
                                {
                                    g_NG_PFCC_File.Add(loadcsvFile);
                                    g_ERROR_STATUS.Add(loadcsvFile + " 搜尋電芯號全無(空)");

                                    g_total_error_raw.Add(new ErrorRaw
                                    {
                                        NgFile = loadcsvFile,
                                        ErrorStatus = "搜尋電芯號全無(空)",
                                        Machine_TrayID = vtary_ID
                                    });

                                    skipCurrentFile = true;  
                                    break;  // 跳出 while
                                }
                            }


                            //這邊串接HTBI_K_Value_MapperType2_V 找尋 K_Value 所判定為ClassType所屬英文代號
                            Sync_HTBI_Merge_Classparam(STR_MSSQL_ARASHTBI, g_batterycell_number);


                            //檢視最後g_Batt_Classtype 存取狀態顯示
                           // Console.WriteLine("電芯目前全classtype 36組顯示 = " + string.Join(", ", g_Batt_Classtype));
                            Console.WriteLine("電芯目前全classtype 36組顯示 = " + string.Join(", ", g_Batt_Classtype, g_Modle_CC_Kvalue));


                            int AllInsert;

                            //計算要insert的實際數量,若有?則跳過不計,針對CC分容站
                            if (vparameter != "023")
                            {
                                AllInsert = calculate_insert_currentNumber(g_Batt_Classtype, vparameter);
                            }
                            else
                            {
                                AllInsert = g_Modle_CC_Kvalue.Count();
                            }

                            //這邊目前可能為電芯目前為(全部?)產生導致,原因流程尚未建立資料庫搜尋無著落
                            if (AllInsert == 0 ) {                                
                                check_ng_num++;
                                check_modlename_nodata = true;
                                if (check_ng_num == 1 && check_modlename_nodata)
                                {
                                    g_NG_PFCC_File.Add(loadcsvFile);                                    
                                    g_ERROR_STATUS.Add(loadcsvFile+" 搜尋電芯號全無(英文類碼無)");
                                    g_total_error_raw.Add(new ErrorRaw
                                    {
                                        NgFile = loadcsvFile,
                                        ErrorStatus = "搜尋電芯號全無(英文類碼無)",
                                        Machine_TrayID = vtary_ID
                                    });
                                    continue;
                                }
                            }

                        
                            //判定是否為整個tray 等同36
                            bool isOnlyValid = (AllInsert != 36);

                            //判定Kvalue 索引總數量
                            int Kpasslen = 36 - g_Modle_CC_Kvalue.Count();

                            Kpasslen = 0;
                            //有些數據可能沒有同步K_VALUE
                            //if (AllInsert == 36)
                            //    Kpasslen = 0;

                            //開36個insert 
                            //當有第一開頭序號有NG,會先忽略不計,但要補償少做的數量,若閃2顆就要加回2顆                 
                            for (int iFlag = 1; iFlag <= AllInsert + Kpasslen; iFlag++)
                            {
                                //初始要閃過的個電芯號序號,依實際狀況做調整----debug用----- start--------
                                //if (isOnlyValid && iFlag < Kpasslen + 1)
                                //{
                                //    //當有要跳過的電芯號數列,這邊需要跳出次數以這邊參考,多增加跳躍7個欄位, 在依照實際跳躍的電芯號數量做判定
                                //    vComID = vComID + 7;
                                //    vState = vState + 7;
                                //    continue;
                                //}
                                //-----end--------
                                
                                cc1SelectSql = "select max(a.VD28) VD28, max(a.VAHD28) VAHD28, max(a.VD32) VD32, max(a.VAHD32) VAHD32, max(a.VD35) VD35, max(a.VAHD35) VAHD35 ";
                                cc1SelectSql = cc1SelectSql + ",(select fld" + vComID + " as OCV from test_LoadPFData003 LIMIT 10, 1)  OCV  /*fld做變更*/ ";
                                cc1SelectSql = cc1SelectSql + " , max(a.CCcurrent) CCcurrent ";
                                cc1SelectSql = cc1SelectSql + ",((select sum(cast(fld" + (vComID) + " as decimal)) from test_LoadPFData003 b where b.fld7 = '" + divValue[0] + "' and ABS(b.fld" + (vComID + 1) + ") > '10') / ";
                                cc1SelectSql = cc1SelectSql + "((select sum(cast(fld7 as decimal)) from test_LoadPFData003 b where b.fld7 = '" + divValue[0] + "' and ABS(b.fld" + (vComID + 1) + ") > '10')/ " + divValue[0] + ")) averageV1 ";
                                cc1SelectSql = cc1SelectSql + ",((select sum(cast(fld" + (vComID) + " as decimal)) from test_LoadPFData003 b where b.fld7 = '" + divValue[1] + "' and ABS(b.fld" + (vComID + 1) + ") > '10') / ";
                                cc1SelectSql = cc1SelectSql + "((select sum(cast(fld7 as decimal)) from test_LoadPFData003 b where b.fld7 = '" + divValue[1] + "' and ABS(b.fld" + (vComID + 1) + ") > '10')/ " + divValue[1] + ")) averageV2 ";
                                cc1SelectSql = cc1SelectSql + ",((select sum(cast(fld" + (vComID) + " as decimal)) from test_LoadPFData003 b where b.fld7 = '" + divValue[2] + "' and ABS(b.fld" + (vComID + 1) + ") > '10') / ";
                                cc1SelectSql = cc1SelectSql + "((select sum(cast(fld7 as decimal)) from test_LoadPFData003 b where b.fld7 = '" + divValue[2] + "' and ABS(b.fld" + (vComID + 1) + ") > '10')/ " + divValue[2] + ")) averageV3 ";
                                /*fld12 要做+8 (變數)*/
                                cc1SelectSql = cc1SelectSql + ",(select max(cast(fld" + (vComID + 4) + " as decimal))  from test_LoadPFData003 where fld7 = '3' and fld" + (vComID + 1) + "  > '10' and fld" + (vComID) + "  <= '3.4') as 'charge34V' ";
                                cc1SelectSql = cc1SelectSql + ",(select  max(cast(fld" + (vComID + 4) + "  as decimal))  from test_LoadPFData003 where fld7 = '3' and fld" + (vComID + 1) + "  > '10' and fld" + (vComID) + "  <= '3.45') as 'charge345V' ";
                                cc1SelectSql = cc1SelectSql + ",(select  max(cast(fld" + (vComID + 4) + "  as decimal))   from test_LoadPFData003 where fld7 = '3' and fld" + (vComID + 1) + "  > '10' and fld" + (vComID) + "  <= '3.5') as  'charge35V' ";



                                //detailSelect  是用在 VLOOKUP  如VD28=XLOOKUP(1,(G11:G5000(STEP) =2)*(N11:N5000=JK8[Reached Target voltage] ),H11:H5000(n-6),0,0)  //每個parameter 底層都一樣
                                detailSelect = "from( "
                             + "select fld7, fld8, fld9 ,fld12, fld14, case when fld7 = '" + stepValue[0] + "' /*2*/ then  fld" + (vState - 6) + "  end VD28, case when fld7 = '" + stepValue[0] + "'  /*2*/ then  fld" + (vState - 2) + " end VAHD28 "
                                + ", case when fld7 = '" + stepValue[1] + "' /*4*/  then  fld" + (vState - 6) + "  end VD32, case when fld7 = '" + stepValue[1] + "' then  fld" + (vState - 2) + "  end VAHD32 "
                                + ", case when fld7 = '" + stepValue[2] + "'/*6*/ then  fld" + (vState - 6) + "  end VD35, case when fld7 = '" + stepValue[2] + "' then  fld" + (vState - 2) + "  end VAHD35 "
                                + " ,case when fld7 = '1' then fld" + (vState - 5) + "  end  'CCcurrent' " +
                                " from test_LoadPFData003  where fld" + vState + " = 'Reached Target voltage' ) a ";


                                //若沒有充電電壓flag 這邊用試算方式求出



                                switch (vparameter)
                                {
                                    case "023": //pf
                                        sqlQuery = "select max(a.VD28) VD28, max(a.VAHD28) VAHD28, max(a.VD32) VD32, max(a.VAHD32) VAHD32, max(a.VD35) VD35, max(a.VAHD35) VAHD35 " + detailSelect;

                                        /* 變成DetailSelectSql 
                                        sqlQuery = sqlQuery + "from( ";
                                        sqlQuery = sqlQuery + "select fld7, fld8, fld9 ,fld12, fld14, case when fld7 = '2' then  fld" + (vState - 6) + "  end VD28, case when fld7 = '2' then  fld" + (vState - 2) + " end VAHD28 ";
                                        sqlQuery = sqlQuery + ", case when fld7 = '4' then  fld" + (vState - 6) + "  end VD32, case when fld7 = '4' then  fld" + (vState - 2) + "  end VAHD32 ";
                                        sqlQuery = sqlQuery + ", case when fld7 = '6' then  fld" + (vState - 6) + "  end VD35, case when fld7 = '6' then  fld" + (vState - 2) + "  end VAHD35 ";
                                        sqlQuery = sqlQuery + " ,case when fld7 = '1' then fld" + (vState - 5) + "  end  'CCcurrent' ";
                                        sqlQuery = sqlQuery + " from test_LoadPFData003  where fld" + vState + " = 'Reached Target voltage' ) a ";
                                        */
                                        break;
                                    case "010":  //cc1
                                                 //vComID = 8;//H欄     vState = 14;//N欄
                                        sqlQuery = cc1SelectSql + detailSelect; //+ " ) finalR ";

                                        break;

                                    case "017":                                    
                                        if (vparameter_chg == "0172" || vparameter_chg == "017-chromaCC2" /*|| vparameter =="010"*/) //cc2-2 2024 , cc2 017-chroma2 2024開始
                                        {

                                            //SECI 走這段解析 V , V1 ,V2,V3,V4 ,育平之前定義的各項目count 總數                                        
                                            if (vparameter_chg == "0172" && check_cc2_algorithm) // for 測試正常                                       
                                            // if (vparameter_chg == "0172")
                                            {
                                                //  jj7 5169 ,  jj8 8395    =(@INDIRECT((ADDRESS($JJ$7, JF14)), 1))                           
                                                cc1SelectSql = cc1SelectSql + ",(SELECT COUNT(*)  FROM test_LoadPFData003 WHERE fld7 = '3' and  cast( fld" + (vComID + 1) + "  as decimal) > 20) as time50A ";
                                                cc1SelectSql = cc1SelectSql + ",(select  fld" + vComID + " from test_LoadPFData003 limit 4976,1 ) as V ";
                                                cc1SelectSql = cc1SelectSql + ",(select  fld" + vComID + " from test_LoadPFData003 limit 5168,1) as V1 ";
                                                cc1SelectSql = cc1SelectSql + ",(select  fld" + vComID + " from test_LoadPFData003 limit 8394,1 ) as V2 ";
                                                cc1SelectSql = cc1SelectSql + ",(select  fld" + (vComID + 1) + " from test_LoadPFData003 limit 5114,1) as v3 ";
                                                cc1SelectSql = cc1SelectSql + ",(select  fld" + (vComID + 1) + " from test_LoadPFData003 limit 5178,1) as v4 ";
                                            }
                                            else // Chroma  走這段解析 V , V1 ,V2,V3,V4 ,這邊根據每個step 與 Reached Target voltage' 條件對應位置 算出count
                                            {
                                                //  jj7 5169 ,  jj8 8395    =(@INDIRECT((ADDRESS($JJ$7, JF14)), 1))                           
                                                cc1SelectSql = cc1SelectSql + ",(SELECT COUNT(*)  FROM test_LoadPFData003 WHERE fld7 = '3' and  cast( fld" + (vComID + 1) + "  as decimal) > 20) as time50A ";
                                                //計算五次
                                                for (int n = 0; n < 5; n++)
                                                {

                                                    int cacula_number = Parse_chroma_V_serial_count(n, vComID, vComID + 1, connection);
                                                    if (n <= 2)
                                                    {
                                                        if (n == 0)
                                                        {
                                                            cc1SelectSql = cc1SelectSql + ",(select  fld" + vComID + " from test_LoadPFData003 limit " + (cacula_number) + ",1 ) as V ";

                                                        }
                                                        else
                                                        {
                                                            cc1SelectSql = cc1SelectSql + ",(select  fld" + vComID + " from test_LoadPFData003 limit " + (cacula_number) + ",1 ) as V" + (n);
                                                        }
                                                    }
                                                    else
                                                    {

                                                        cc1SelectSql = cc1SelectSql + ",(select  fld" + (vComID + 1) + " from test_LoadPFData003 limit " + (cacula_number) + ",1 ) as V" + (n) + " ";
                                                    }
                                                }


                                                //當原始數據沒有Reached Target voltage參考
                                                if (!haveTargetvoltage)
                                                {
                                                    for (int k = 0; k < step_caculator_value.Count(); k++)
                                                    {
                                                        if (k < 6)
                                                        {

                                                            if (k % 2 == 0 || k == 0)
                                                            {
                                                                //取VD 2.8, 3.2, 3.5 
                                                                if (k == 0)
                                                                {
                                                                    detailVD = "absVD28";
                                                                    like_step = step_abs_value[0];
                                                                }
                                                                else if (k == 2)
                                                                {
                                                                    detailVD = "absVD32";
                                                                    like_step = step_abs_value[1];
                                                                }
                                                                else if (k == 4)
                                                                {
                                                                    detailVD = "absVD35";
                                                                    like_step = step_abs_value[2];
                                                                }

                                                                cc1SelectSql = cc1SelectSql + ",( select abs(fld" + vComID + ")  from test_LoadPFData003 WHERE fld7 LIKE '" + like_step + "' limit " + (step_caculator_value[k] - 3) + " ,1 ) as " + detailVD + "";

                                                            }
                                                            else
                                                            {
                                                                //取mAH 2.8, 3.2, 3.5                                                             
                                                                if (k == 1)
                                                                {
                                                                    detailmAH = "absmAH28";
                                                                    like_step = step_abs_value[0];
                                                                }
                                                                else if (k == 3)
                                                                {
                                                                    detailmAH = "absmAH32";
                                                                    like_step = step_abs_value[1];
                                                                }
                                                                else if (k == 5)
                                                                {
                                                                    detailmAH = "absmAH35";
                                                                    like_step = step_abs_value[2];
                                                                }

                                                                cc1SelectSql = cc1SelectSql + ",( select abs(fld" + (vComID + 4) + ")  from test_LoadPFData003 WHERE fld7 LIKE '" + like_step + "' limit " + (step_caculator_value[k] - 3) + " ,1 ) as " + detailmAH + "";
                                                            }

                                                        }
                                                        else
                                                        {
                                                            //取current 電流                                                          
                                                            detailCurent = "absCurrentmA";
                                                            cc1SelectSql = cc1SelectSql + ",( select abs(fld" + (vComID + 1) + ")  from test_LoadPFData003 WHERE fld7 LIKE '1' limit " + (step_caculator_value[k] - 1) + " ,1 ) as " + detailCurent + " ";
                                                        }
                                                    }

                                                }

                                            }


                                            sqlQuery = cc1SelectSql + detailSelect; //+ " ) finalR ";

                                        }
                                        else  //cc2 2023
                                        {
                                            //jj7 4893,jj8 4957  =(@INDIRECT((ADDRESS($JJ$7,JF14)),1))
                                            cc1SelectSql = cc1SelectSql + ",(SELECT COUNT(*)  FROM test_LoadPFData003 WHERE fld7 = '3' and  cast( fld" + (vComID + 1) + "  as decimal) > 20) as time50A ";
                                            cc1SelectSql = cc1SelectSql + ",(select  fld" + vComID + " from test_LoadPFData003 limit 492,1 ) as V ";
                                            cc1SelectSql = cc1SelectSql + ",(select  fld" + vComID + " from test_LoadPFData003 limit 4892,1) as V1 ";
                                            cc1SelectSql = cc1SelectSql + ",(select  fld" + vComID + " from test_LoadPFData003 limit 4956,1 ) as V2 ";
                                            cc1SelectSql = cc1SelectSql + ",(select  fld" + (vComID + 1) + " from test_LoadPFData003 limit 4838,1) as v3 ";
                                            cc1SelectSql = cc1SelectSql + ",(select  fld" + (vComID + 1) + " from test_LoadPFData003 limit 4902,1) as v4 ";

                                            sqlQuery = cc1SelectSql + detailSelect; //+ " ) finalR ";

                                        }

                                        break;

                                }//end switch



                                comm_detail = new MySqlCommand(sqlQuery, conn_detail);

                                dr_detail = comm_detail.ExecuteReader();

                                if (dr_detail.HasRows)  //找FUNCTION的值
                                {
                                    while (dr_detail.Read())
                                    {
                                        //PF

                                        /*cc 新增的欄位*/
                                        //,`CCcurrent`,`OCV`,`averageV1`,`averageV2`,`averageV3`
                                        //,`charge34V`,`charge345V`,`charge35V`,`time50A`,`v`
                                        //,`v1`,`v2`,`v3`,`v4`,`Para`
                                        //mOhm 欄位 ABS(KD16-KE16)/ABS(KF16-KG16)*1000 取得欄位後     Math.Abs();


                                        //string svd28 = Convert.ToString(dr_detail["absVD28"].ToString());
                                        //string smaH28 = Convert.ToString(dr_detail["absmAH28"].ToString());
                                        //string svd32 = Convert.ToString(dr_detail["absVD32"].ToString());
                                        //string smaH32 = Convert.ToString(dr_detail["absmAH32"].ToString());
                                        //string svd35 = Convert.ToString(dr_detail["absVD35"].ToString());
                                        //string smaH35 = Convert.ToString(dr_detail["absmAH35"].ToString());


                                        try
                                        {

                                            if (!haveTargetvoltage && vparameter == "017")
                                            {
                                                VD28 = Convert.ToString(dr_detail["absVD28"].ToString());
                                                VAHD28 = Convert.ToString(dr_detail["absmAH28"].ToString());
                                                VD32 = Convert.ToString(dr_detail["absVD32"].ToString());
                                                VAHD32 = Convert.ToString(dr_detail["absmAH32"].ToString());
                                                VD35 = Convert.ToString(dr_detail["absVD35"].ToString());
                                                VAHD35 = Convert.ToString(dr_detail["absmAH35"].ToString());
                                            }
                                            else
                                            {
                                                VD28 = Convert.ToString(dr_detail["VD28"].ToString());
                                                VAHD28 = Convert.ToString(dr_detail["VAHD28"].ToString());
                                                VD32 = Convert.ToString(dr_detail["VD32"].ToString());
                                                VAHD32 = Convert.ToString(dr_detail["VAHD32"].ToString());
                                                VD35 = Convert.ToString(dr_detail["VD35"].ToString());
                                                VAHD35 = Convert.ToString(dr_detail["VAHD35"].ToString());
                                            }

                                            // ✅ 空值檢查 (主動丟出 Exception)
                                            if (string.IsNullOrWhiteSpace(VD28) ||
                                                string.IsNullOrWhiteSpace(VAHD28) ||
                                                string.IsNullOrWhiteSpace(VD32) ||
                                                string.IsNullOrWhiteSpace(VAHD32) ||
                                                string.IsNullOrWhiteSpace(VD35) ||
                                                string.IsNullOrWhiteSpace(VAHD35))
                                            {
                                                throw new Exception("VD28~VAHD35 欄位有空值");
                                            }


                                        }
                                        catch (FormatException ex)
                                        {
                                            // 捕捉並處理 FormatException

                                            // 你可以記錄日誌，或將錯誤訊息發送給管理員
                                            // Log.Error("FormatException: " + ex.Message);
                                        }
                                        catch (Exception ex)
                                        {
                                            // 捕捉其他類型的錯誤
                                            Console.WriteLine("ValueNullException: " + ex.Message);                                         
                                            check_ng_num++;
                                            check_have_ng = true;                                           
                                            // VD28 = VD32 = VAHD32 = VD35 = VAHD35 = "電壓值空值判定異常";


                                            if (check_ng_num == 1 && check_have_ng)
                                            {
                                                g_NG_PFCC_File.Add(loadcsvFile);
                                                g_ERROR_STATUS.Add(loadcsvFile + " VD28 ~VAHD35電壓值範圍偵測有空,異常");                                                
                                                g_total_error_raw.Add(new ErrorRaw
                                                {
                                                    NgFile = loadcsvFile,
                                                    ErrorStatus = "VD28 ~VAHD35電壓值範圍偵測有空,異常",
                                                    Machine_TrayID = vtary_ID
                                                });
                                                continue;
                                            }

                                            // Log.Error("Unexpected error: " + ex.Message);
                                        }



                                        Console.WriteLine($"3.5-2.8V Ah 電容量 =  { VAHD35}");

                                        switch (vparameter)
                                        {
                                            case "023": //pf
                                                if (g_Modle_CC_Kvalue.Count() != 0)
                                                {
                                                    if (AllInsert != 0 && iFlag - 1 < g_Modle_CC_Kvalue.Count()+ Kpasslen)
                                                        Get_K_Value = g_Modle_CC_Kvalue[iFlag - Kpasslen - 1].ToString();
                                                    else
                                                        Get_K_Value = "";
                                                }
                                                break;

                                            case "010": //cc1                                     

                                                //if (!haveTargetvoltage)
                                                //{
                                                //    VCCcurrent = Convert.ToString(dr_detail["absCurrentmA"].ToString());
                                                //}
                                                //else
                                                //{
                                                //    VCCcurrent = Convert.ToString(dr_detail["CCcurrent"].ToString());
                                                //}

                                                VCCcurrent = Convert.ToString(dr_detail["CCcurrent"].ToString());

                                                VOCV = Convert.ToString(dr_detail["OCV"].ToString());
                                                VaverageV1 = Convert.ToString(dr_detail["averageV1"].ToString());
                                                VaverageV2 = Convert.ToString(dr_detail["averageV2"].ToString());
                                                VaverageV3 = Convert.ToString(dr_detail["averageV3"].ToString());
                                                Vcharge34V = Convert.ToString(dr_detail["charge34V"].ToString());
                                                Vcharge345V = Convert.ToString(dr_detail["charge345V"].ToString());
                                                Vcharge35V = Convert.ToString(dr_detail["charge35V"].ToString());

                                                //目前CHROMA 數據有問題  CHX_I(A) 都是負值,條件式需要大於10 , Current 目前因 Reached Target voltage無故無法收驗找到相對應值
                                                if (VCCcurrent.ToString() == "" || VaverageV1.ToString() == "" || VaverageV3.ToString() == "")
                                                {
                                                    VCCcurrent = VaverageV1 = VaverageV3 = "0.0";
                                                }

                                                //if (iFlag - 13 <= g_Modle_CC_Kvalue.Count())
                                                if (g_Modle_CC_Kvalue.Count() != 0)
                                                {
                                                    if (AllInsert != 0 && iFlag - 1 < g_Modle_CC_Kvalue.Count() + Kpasslen)
                                                        Get_K_Value = g_Modle_CC_Kvalue[iFlag - Kpasslen - 1].ToString();
                                                    else
                                                        Get_K_Value = "";
                                                }

                                                Vtime50A = "0";
                                                VV = "0";
                                                VV1 = "0";
                                                VV2 = "0";
                                                VV3 = "0";
                                                VV4 = "0";
                                                VmOhm = "0";
                                                Vpara = "CC1";


                                                break;
                                            case "017":  //cc2 or cc2-2 or cc2-chroma2

                                                if (!haveTargetvoltage)
                                                {
                                                    VCCcurrent = Convert.ToString(dr_detail["absCurrentmA"].ToString());
                                                }
                                                else
                                                {
                                                    VCCcurrent = Convert.ToString(dr_detail["CCcurrent"].ToString());
                                                }

                                                VOCV = Convert.ToString(dr_detail["OCV"].ToString());
                                                VaverageV1 = Convert.ToString(dr_detail["averageV1"].ToString());
                                                VaverageV2 = Convert.ToString(dr_detail["averageV2"].ToString());
                                                VaverageV3 = Convert.ToString(dr_detail["averageV3"].ToString());
                                                Vcharge34V = Convert.ToString(dr_detail["charge34V"].ToString());
                                                Vcharge345V = Convert.ToString(dr_detail["charge345V"].ToString());
                                                Vcharge35V = Convert.ToString(dr_detail["charge35V"].ToString());

                                                int cap_type = Assign_Cap_mAH_Type(VAHD35);
                                               
                                                try
                                                {
                                                    insert_num = isOnlyValid ? g_OnlyExist_ModleID_Number[iFlag - Kpasslen - 1] : insert_num;
                                                }
                                                catch (Exception ex)
                                                {
                                                    // 捕捉的錯誤(索引的序號有overflow)
                                                    check_ng_num++;
                                                    check_modlename_nodata = true;
                                                    if (check_ng_num == 1 && check_modlename_nodata)
                                                    {
                                                        g_NG_PFCC_File.Add(loadcsvFile);
                                                        g_ERROR_STATUS.Add(loadcsvFile + " 充放電週期無完整");
                                                        g_total_error_raw.Add(new ErrorRaw
                                                        {
                                                            NgFile = loadcsvFile,
                                                            ErrorStatus = "充放電週期無完整",
                                                            Machine_TrayID = vtary_ID
                                                        });
                                                        continue;
                                                    }
                                                }

                                                try
                                                {
                                                    Get_K_Value = isOnlyValid ? g_Modle_CC_Kvalue[iFlag - Kpasslen - 1].ToString() : g_Modle_CC_Kvalue[iFlag - 1].ToString();
                                                }
                                                catch (Exception ex)
                                                {
                                                    // 捕捉的錯誤(K值沒有mapping)
                                                    check_ng_num++;
                                                    check_modlename_nodata = true;                                                    
                                                    if (check_ng_num ==1  && check_modlename_nodata)
                                                    {
                                                        g_NG_PFCC_File.Add(loadcsvFile);
                                                        g_ERROR_STATUS.Add(loadcsvFile + " 搜尋電芯號全無(英文類碼無)");
                                                        g_total_error_raw.Add(new ErrorRaw
                                                        {
                                                            NgFile = loadcsvFile,
                                                            ErrorStatus = "搜尋電芯號全無(英文類碼無)",
                                                            Machine_TrayID = vtary_ID
                                                        });
                                                        continue;
                                                    }                                                   
                                                }


                                                try
                                                {
                                                    CC2_interpretcode = Convert.ToString(g_Batt_Classtype[insert_num]) + cap_type.ToString("D2");

                                                    int check_position = Determination_Type_Position(g_Batt_Classtype[insert_num], cap_type);

                                                    CC2_position = Convert.ToString(check_position);
                                                }
                                                catch (Exception ex)
                                                {
                                                    // 捕捉的錯誤(判別碼溢位導致Crash)
                                                    check_ng_num++;
                                                    check_index_overflow_number++;
                                                    check_modlename_nodata  = true;
                                                    if ((check_ng_num == 1 && check_modlename_nodata) || check_index_overflow_number ==1)
                                                    {
                                                        g_NG_PFCC_File.Add(loadcsvFile);
                                                        g_ERROR_STATUS.Add(loadcsvFile + " 判別碼序號讀取超出範圍 ");
                                                        g_total_error_raw.Add(new ErrorRaw
                                                        {
                                                            NgFile = loadcsvFile,
                                                            ErrorStatus = "判別碼序號讀取超出範圍",
                                                            Machine_TrayID = vtary_ID
                                                        });
                                                        continue;
                                                    }
                                                }

                                                //目前CHROMA 數據有問題  CHX_I(A) 都是負值,條件式需要大於10 , Current 目前因 Reached Target voltage無故無法收驗找到相對應值
                                                if (VCCcurrent.ToString() == "" || VaverageV1.ToString() == "" || VaverageV3.ToString() == "")
                                                {
                                                    VCCcurrent = VaverageV1 = VaverageV3 = "0.0";
                                                }

                                                Vtime50A = Convert.ToString(dr_detail["time50A"].ToString());
                                                VV = Convert.ToString(dr_detail["V"].ToString());
                                                VV1 = Convert.ToString(dr_detail["V1"].ToString());
                                                VV2 = Convert.ToString(dr_detail["V2"].ToString());
                                                VV3 = Convert.ToString(dr_detail["V3"].ToString());
                                                VV4 = Convert.ToString(dr_detail["V4"].ToString());
                                                //=ABS(KD14-KE14)/ABS(KF14-KG14)*1000

                                                Vpara = "CC2";
                                                //Decimal divisor = Math.Abs(Convert.ToDecimal(VV3) - Convert.ToDecimal(VV4));
                                                //if (divisor == 0) divisor = 0.0039M;
                                                //VmOhm = Convert.ToString( Math.Abs(Convert.ToDecimal(VV1) - Convert.ToDecimal(VV2)) / divisor);

                                                decimal VV1_Vrms, VV2_Vrms, VV3_Vrms, VV4_Vrms;

                                                try {
                                                    
                                                    if (decimal.TryParse(VV1, out VV1_Vrms) && decimal.TryParse(VV2, out VV2_Vrms) &&
                                                        decimal.TryParse(VV3, out VV3_Vrms) && decimal.TryParse(VV4, out VV4_Vrms))
                                                    {
                                                        if (g_Batt_Classtype[insert_num] != "?")
                                                            VmOhm = Convert.ToString(Math.Abs(Convert.ToDecimal(VV1_Vrms) - Convert.ToDecimal(VV2_Vrms)) / Math.Abs(Convert.ToDecimal(VV3_Vrms) - Convert.ToDecimal(VV4_Vrms)));
                                                        else
                                                            VmOhm = "0.000";
                                                    }                                                       
                                                }
                                                catch (FormatException ex)
                                                {
                                                    // 捕捉並處理 FormatException
                                                    VmOhm = "錯誤：輸入的數字格式不正確";
                                                    // 你可以記錄日誌，或將錯誤訊息發送給管理員
                                                    // Log.Error("FormatException: " + ex.Message);
                                                }
                                                catch (Exception ex)
                                                {
                                                    // 捕捉其他類型的錯誤
                                                    check_ng_num++;
                                                    check_have_ng = true;
                                                    VmOhm = "除法計算異常";

                                                    if (check_ng_num == 1 && check_have_ng)
                                                    {
                                                        g_NG_PFCC_File.Add(loadcsvFile);
                                                        g_ERROR_STATUS.Add(loadcsvFile + $" 阻值計算失敗,有空值導致 VV3 ={VV3} VV4 ={VV4}");
                                                        g_total_error_raw.Add(new ErrorRaw
                                                        {
                                                            NgFile = loadcsvFile,
                                                            ErrorStatus = $"阻值計算失敗,有空值導致 VV3 ={VV3} VV4 ={VV4}",
                                                            Machine_TrayID = vtary_ID
                                                        });
                                                        continue;
                                                    }

                                                    // Log.Error("Unexpected error: " + ex.Message);
                                                }

                                                if (string.IsNullOrEmpty(VV1) && string.IsNullOrEmpty(VV2) && string.IsNullOrEmpty(VV3) && string.IsNullOrEmpty(VV4))                                                
                                                {
                                                    check_ng_num++;
                                                    check_have_ng = true;
                                                    VmOhm = "VV系列值都空狀態,異常";
                                                    if (check_ng_num == 1 && check_have_ng)
                                                    {
                                                        g_NG_PFCC_File.Add(loadcsvFile);
                                                        g_ERROR_STATUS.Add(loadcsvFile + " V1到V4電壓值都空狀態,異常");                                                       
                                                        g_total_error_raw.Add(new ErrorRaw
                                                        {
                                                            NgFile = loadcsvFile,
                                                            ErrorStatus = "V1到V4電壓值都空狀態,異常",
                                                            Machine_TrayID = vtary_ID
                                                        });
                                                        continue;
                                                    }
                                                }
                                                break;
                                        }
                                    }

                                    //vStart_date //設定測試的日期(因為是key，所以手動輸 0755  測試值上線要拿掉
                                    //vStart_date = "2024/01/01 01:11:44";
                                    //'2024/07/01 02:13:44'


                                    //string tableTileSql = "", columnSql = "", valueSql = "";

                                    ///insertSql = insertSql + " INSERT INTO pfprocess001  ";
                                    DateTime now_str = DateTime.Now;

                                    // 寫入資料庫的格式（正確）
                                    string dbTimeStr = now_str.ToString("yyyy-MM-dd HH:mm:ss");
                                    //  string dateStr = now_str.ToString("yyyy-MM-dd HH:mm:ss", new CultureInfo("zh-TW")); // 格式與 MySQL 相符

                                    // 顯示用格式（含上午/下午）
                                    CultureInfo taiwanCulture = new CultureInfo("zh-TW");
                                    string displayTimeStr = now_str.ToString("yyyy/M/d tt hh:mm:ss", taiwanCulture);

                                    // 先解析時間
                                    DateTime dt_start = DateTime.ParseExact(vStart_date, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                                    DateTime dt_end = DateTime.ParseExact(vdateEnd_date, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);


                                    // 格式  yyyy/MM/dd 上午/下午 hh:mm:ss
                                    string cvt_startdate = dt_start.ToString("yyyy/MM/dd tt hh:mm:ss", taiwanCulture);
                                    string cvt_enddate = dt_end.ToString("yyyy/MM/dd tt hh:mm:ss", taiwanCulture);


                                    valueSql = "VALUES ( '" + dr["fld" + vComID].ToString() + "',  '" + vStart_date + "','" + cvt_enddate + "','" + vtary_ID + "','" + vparameter + "',";
                                    valueSql = valueSql + " '" + dr["fld" + vState].ToString() + "' ,'" + VD28 + "','" + VD28 + "','" + VAHD28 + "','" + VAHD28 + "',";
                                    valueSql = valueSql + " '" + VD32 + "' ,'" + VD32 + "','" + VAHD32 + "','" + VAHD32 + "','" + VD35 + "',";
                                    // valueSql = valueSql + " '" + VD35 + "' ,'" + VAHD35 + "','" + VAHD35 + "','" + loadcsvFile + "','" + vprocess + "',now()";
                                    valueSql = valueSql + " '" + VD35 + "' ,'" + VAHD35 + "','" + VAHD35 + "','" + loadcsvFile + "','" + vprocess + "','" + displayTimeStr + "'";

                                    //select CCcurrent, OCV, averageV1, averageV2, averageV3, charge34V, charge345V, charge35V
                                    //  , time50A, v, v1, v2, v3, v4, mOhm from processcc
                                    switch (vparameter)
                                    {
                                        case "023": //pf
                                            tableTitleSql = " INSERT INTO pfprocess001  ";
                                            tableTitleSql = tableTitleSql + " (ID,StartDateD,EnddateD,trayID,parameter ";
                                            tableTitleSql = tableTitleSql + ",State,VD28,VS28,VAHD28,VAHS28 ";
                                            tableTitleSql = tableTitleSql + " ,VD32 ,VS32 ,VAHD32,VAHS32 ,VD35  ";
                                            tableTitleSql = tableTitleSql + " ,VS35,VAHD35 ,VAHS35,FileName,Process,AnlaysisDayD,interpretcode,position,K_Value ";

                                            valueSql = valueSql + " ,'" + CC2_interpretcode + "','" + CC2_position + "','" + Get_K_Value + "'";

                                            insertSql = insertSql + tableTitleSql + " ) " + valueSql + ");";
                                            break;
                                        default: //cc2>cc1 所以有7個欄位 寫0

                                            /*cc 新增的欄位*/
                                            //,`Para`,`CCcurrent`,`OCV`,`averageV1`,`averageV2`,`averageV3`,`charge34V`,`charge345V`,`charge35V`
                                            //,`time50A`,`v`
                                            //,`v1`,`v2`,`v3`,`v4`
                                            //mOhm 欄位 ABS(KD16-KE16)/ABS(KF16-KG16)*1000 取得欄位後     Math.Abs();

                                            tableTitleSql = " INSERT INTO processcc";
                                            columnSql = "(ID,StartDateD,EnddateD,trayID,parameter ";
                                            columnSql = columnSql + ",State,VDA,VSA,VAHDA,VAHSA ";
                                            columnSql = columnSql + " ,VDB ,VSB ,VAHDB,VAHSB ,VDC  ";
                                            columnSql = columnSql + " ,VSC,VAHDC ,VAHSC,FileName,Process,AnlaysisDayD ";
                                            columnSql = columnSql + ",Para ";
                                            columnSql = columnSql + ",CCcurrent,OCV,averageV1,averageV2,averageV3,charge34V,charge345V,charge35V"; //cc1有的，
                                            columnSql = columnSql + ",time50A,v,v1,v2,v3,v4,mOhm,interpretcode,position,K_Value,analysisDT"; //cc2才有的，cc1要塞的話，值均為0 mOhm 是用算值出來的

                                            valueSql = valueSql + ",'" + Vpara + "'";  //para
                                            valueSql = valueSql + ", " + VCCcurrent + "," + VOCV + "," + VaverageV1 + "," + VaverageV2 + "," + VaverageV3 + "," + Vcharge34V + "," + Vcharge345V + "," + Vcharge35V;
                                            valueSql = valueSql + ", " + Vtime50A + ", " + VV + ", " + VV1 + ", " + VV2 + ", " + VV3 + ", " + VV4 + ", " + VmOhm + ", " + "'" + CC2_interpretcode + "'" + ", " + "'" + CC2_position + "'" + ", " + "'" + Get_K_Value + "'" + ", now()" + ") ";
                                            //新增vvalueSql//增加value(
                                            insertSql = insertSql + tableTitleSql + columnSql + " ) " + valueSql + ";";
                                            break;

                                    }

                                    //alueSql = valueSql + ") ; ";

                                    vComID = vComID + 7;
                                    vState = vState + 7;

                                    insert_num++;

                                    dr_detail.Close();
                                }

                            }
                        }


                        //LResult.Text = insertSql;
                        conn_detail.Close();

                        if (skipCurrentFile)
                            continue;


                        //String testSql = "insert INTO pfprocess001  (ID,StartDateD,EnddateD,trayID,parameter ,State,VD28,VS28,VAHD28,VAHS28  ,VD32 ,VS32 ,VAHD32,VAHS32 ,VD35   ,VS35,VAHD35 ,VAHS35,FileName,Process,AnlaysisDayD)VALUES ( 'MW2007A05101',  '2024/01/01 02:17:02','2024/01/01 07:15:14','PF-03-K000001','023', 'OK' ,'2.8000','2.8000','2627.0','2627.0', '3.3000' ,'3.3000','13802.2','13802.2','3.4000', '3.4000' ,'30400.0','30400.0','0000001.txt','00:Pressure Formation',now()) ; ";
                        //testSql = testSql + "insert INTO pfprocess001(ID, StartDateD, EnddateD, trayID, parameter, State, VD28, VS28, VAHD28, VAHS28, VD32, VS32, VAHD32, VAHS32, VD35, VS35, VAHD35, VAHS35, FileName, Process, AnlaysisDayD)VALUES('MW2007A05101', '2024/01/01 02:18:02', '2024/01/01 07:16:14', 'PF-03-K000001', '023', 'OK', '2.8000', '2.8000', '2627.0', '2627.0', '3.3000', '3.3000', '13802.2', '13802.2', '3.4000', '3.4000', '30400.0', '30400.0', '0000001.txt', '00:Pressure Formation', now()); ";

                        String testSql = insertSql;

                        MySqlConnection conn_exec = new MySqlConnection(connection);

                        if (conn_exec.State != ConnectionState.Open)
                            conn_exec.Open();
                        //MySqlCommand cmd = new MySqlCommand(testSql, conn_exec);
                        cmd = new MySqlCommand(testSql, conn_exec);
                        try
                        {
                            cmd.ExecuteNonQuery(); //insert 36筆(正常)或實際的電芯測試托盤筆數
                            LResult.Text = "已完成";
                        }
                        catch (Exception ex)
                        {
                            LResult.Text = "資料錯誤" + ex.ToString();
                           
                        }

                        conn_exec.Close();

                    }

                    if (conn.State != ConnectionState.Closed)
                        conn.Close();


                    string originalfile = loadcsvFile;

                    //將重新解析的(PF or CC1 or CC2)存成csv,並呈現table含數據於頁面上
                    //只取檔案名稱,忽略副檔名
                    Filename = Path.GetFileNameWithoutExtension(Filename);

                    switch (vparameter)  //STEP 
                    {
                        case "023": //pf ==>'023'
                            dumpcsv = "SELECT * FROM sakila.pfprocess001;";
                            Filename = Filename + "-pfprocess001.csv";
                            pfcc_tablename = "pfprocess001";
                            break;
                        case "010":  //cc1
                            dumpcsv = "SELECT * FROM sakila.processcc;";
                            Filename = Filename + "-process-cc1.csv";
                            pfcc_tablename = "processcc";
                            break;
                        case "017": //cc2
                            dumpcsv = "SELECT * FROM sakila.processcc;";

                            if (vparameter_chg == "0172") //cc2-2 2024
                            {
                                Filename = Filename + "-process-cc2-2.csv";
                            }
                            else if (vparameter_chg == "017-chromaCC2")
                            {
                                Filename = Filename + "-process_chroma-cc2.csv";
                            }
                            else
                            {
                                Filename = Filename + "-process-cc2.csv";
                            }
                            pfcc_tablename = "processcc";
                            break;
                    }



                    //若路徑資料夾(C:\\pf-cc)沒有則這邊建立,for MYSQL LOAD REQUIRE
                    if (!Directory.Exists(ResultTaskFolder))
                        Directory.CreateDirectory(ResultTaskFolder);

                    ExportToCsv resultcsv = new ExportToCsv();

                    //  string pfccPath_File = Server.MapPath("~/" + "pf-cc" + "/")+ Filename;
                    //(1)先將分析數據產生export csv格式檔

                    //當沒有NG或電芯號不為0才產出csv 表單 
                    if (!check_have_ng && !check_modlename_nodata) {
                        string pfccPath_File = Path.Combine(ResultTaskFolder, Filename);
                        DataTable dtView = resultcsv.Export(connection, dumpcsv, pfccPath_File);
                        csvview.DataSource = dtView;
                        csvview.DataBind();
                    }
                   
                    string sDayD_value = string.Empty;

                    if (vparameter == "023")
                    {
                        sDayD_value = "XXXXXX";
                    }
                    else
                    {
                        sDayD_value = "AnlaysisDayD";
                    }

                    //(2)再將分析完的數據合併預先遠端建置之的table (這邊目前使用遠端 hr.test_mergepfcc)
                    //目前所有(pc,cc1,cc2,cc2-2)都忽略以下欄位
                    All_col_listname = $@"SELECT GROUP_CONCAT(CASE
                       WHEN COLUMN_NAME NOT IN( 'State', 'Process','{sDayD_value}') THEN COLUMN_NAME
                        ELSE NULL
                        END  ORDER BY ORDINAL_POSITION) AS col_list
                        FROM INFORMATION_SCHEMA.COLUMNS
                        WHERE TABLE_NAME = '{pfcc_tablename}'
                        AND TABLE_SCHEMA = '{schema_DB}'; ";


                    if (!check_modlename_nodata && resultcsv.Merge_existfilter_value(connection, connection_merge, All_col_listname, schema_DB, pfcc_tablename) == true)
                    {
                       //確定沒有NG的情況下,將原始測試數據刪除
                        if (!check_have_ng) {
                            List<string> recordsucessful = new List<string>();
                            recordsucessful.Add(originalfile);
                            //刪除已經完成之數據原始檔案
                            DeleteTHreadOKFiles(SourceFolder, recordsucessful, mannulrun);
                            //成功轉換數量累加1
                            succesfulnum++;
                        }                            
                    }                        
                    
                    //當執行完畢到最後一筆
                   if (i == totalTasks-1) 
                   {
                        //DirectoryInfo tempDir1 = new DirectoryInfo(DestinationFolder);
                        //foreach (FileInfo fi in tempDir1.EnumerateFiles())
                        //{
                        //    // 目錄下C:\\tempcsv 內檔案全部刪除
                        //    File.Delete(DestinationFolder + Path.DirectorySeparatorChar + fi.Name);
                        //}

                        //if (succesfulnum == totalTasks)
                        //    LResult.Text = "分析完篩選型號及合併資料完畢!";
                        //else
                        //{
                        //    LResult.Text = "資料合併異常,NG {";
                        //    for (int ng = 0; ng < g_NG_PFCC_File.Count; ng++)
                        //    {
                        //        LResult.Text += g_NG_PFCC_File[ng].ToString()+" ";

                        //        if (ng == g_NG_PFCC_File.Count - 1)
                        //            LResult.Text += " 錯誤狀態寫入於error_record.txt}";
                        //    }

                        //    //將分析NG原始數據檔案放置 既定 C:\copy_temp\pf-cc-testNG
                        //    COPY_NG_Directionary(SourceFolder, NG_file_Path, g_NG_PFCC_File);

                        //    //將原始數據分析後錯誤狀態寫入待後續追蹤
                        //    RewriteAndAppendToFile(NG_STARUS_record, g_ERROR_STATUS);


                        //    DirectoryInfo csvDir = new DirectoryInfo(SourceFolder);
                        //    foreach (FileInfo fi in csvDir.EnumerateFiles())
                        //    {
                        //        // 目錄下C:\copy_temp\source_pfcc 內檔案全部刪除,確定都NG
                        //        File.Delete(SourceFolder + Path.DirectorySeparatorChar + fi.Name);
                        //    }
                        //}

                        //LResult.Text += ",請確認分析完PF_CC系列數據格式!";
                    }

                    //透過C:\\copy_pfcc_result.bat 將產出pf cc1 cc2 等數據csv 回存到 網路工作磁碟(ex:\\192.168.3.100\pfcc_result)
                    // PFCC_result_SaveExecuteBatFile();
                   // EXEC_Save_PFCCbat();


                } //end if (讀檔錯誤判斷====>)

            }

            // ✅ for 迴圈執行完畢後，進行收尾處理
            DirectoryInfo tempDir = new DirectoryInfo(DestinationFolder);
            foreach (FileInfo fi in tempDir.EnumerateFiles())
            {
                // 目錄下C:\\tempcsv 內檔案全部刪除
                File.Delete(DestinationFolder + Path.DirectorySeparatorChar + fi.Name);
            }

            if (succesfulnum == totalTasks)
                LResult.Text = "分析完篩選型號及合併資料完畢!";
            else
            {
                LResult.Text = "資料合併異常,NG {";
                for (int ng = 0; ng < g_NG_PFCC_File.Count; ng++)
                {
                    LResult.Text += g_NG_PFCC_File[ng].ToString() + " ";

                    if (ng == g_NG_PFCC_File.Count - 1)
                        LResult.Text += " 錯誤狀態寫入於error_record.txt}";
                }


                //將異常之PFCC原始數據(檔案名稱,異常狀態,所屬機器ID)存入異常表單--------start----
               String All_mes_error_record_columns = $@"SELECT GROUP_CONCAT(CASE
                       WHEN COLUMN_NAME NOT IN('id') THEN COLUMN_NAME
                        ELSE NULL
                        END  ORDER BY ORDINAL_POSITION) AS col_list
                        FROM INFORMATION_SCHEMA.COLUMNS
                        WHERE TABLE_NAME = 'productionerror_record'
                        AND TABLE_SCHEMA = 'mes'; ";


                //宣告最後要執行csv應用類別物件
                ExportToCsv resultcsv_2 = new ExportToCsv();


                if (resultcsv_2.Insert_Error_convert_Raw(g_Mes_connectpool, All_mes_error_record_columns, g_total_error_raw) == true)
                    Console.WriteLine("存入異常表單完成!");                   
                else
                    Console.WriteLine("存入異常表單失敗 --冏--");
                //---------------------------------end------------------------------------------

                //將分析NG原始數據檔案放置 既定 C:\copy_temp\pf-cc-testNG
                COPY_NG_Directionary(SourceFolder, NG_file_Path, g_NG_PFCC_File);

                //將原始數據分析後錯誤狀態寫入待後續追蹤
                RewriteAndAppendToFile(NG_STARUS_record, g_ERROR_STATUS);


                //將檔案備份複製Z 後續react追蹤
                CopyErrorRecordWith_Z_Backup();


                DirectoryInfo csvDir = new DirectoryInfo(SourceFolder);
                foreach (FileInfo fi in csvDir.EnumerateFiles())
                {
                    // 目錄下C:\copy_temp\source_pfcc 內檔案全部刪除,確定都NG
                    File.Delete(SourceFolder + Path.DirectorySeparatorChar + fi.Name);
                }
            }

            LResult.Text += ",請確認分析完PF_CC系列數據格式!";
        }
    }
}