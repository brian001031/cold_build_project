using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MySql.Data.MySqlClient;
using System.Data;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Diagnostics;



namespace WebApplication1
{
    public partial class _Default : Page
    {
        List<string> g_csvFile = new List<string>();
        List<string> g_pfcctype = new List<string>();
        List<string> g_ThreadNotOkFile = new List<string>();

        bool timerunheck = false;
        public static  readonly object _lock = new object();
        public static readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

       // public String SourceFolder = @"C:\source_testXXX";
        public String SourceFolder = @"C:\copy_temp\source_pfcc";
        public String ResultTaskFolder = @"C:\pf-cc-result";
        public String NGTThread_filepath = @"C:\pf-cc-result\ng_output.txt"; // 自動執行有NG存取指定檔案路徑
        public String NG_file_record = @"C:\pf-cc-result\ng_record.txt"; // 清除既定完成數據清除指定檔案路徑讀取

        // public String NGTThread_filepath = @"C:\pf-cc\ng_output.txt"; // 自動執行有NG存取指定檔案路徑
        // public String NG_file_record = @"C:\pf-cc\ng_record.txt"; // 清除既定完成數據清除指定檔案路徑讀取

        // public String SourceFolder = @"C:\source_pfcc";
        // public String exec_savepfccbat = @"C:\copy_pfcc_result.bat";

        protected void Page_Load(object sender, EventArgs e)
        {
            //避免Auto時重複初始化
            if(!Page.IsPostBack)
            {
                Button1.Visible = true;
                Button3.Visible = true;
                Btn_Auto.Visible = true;
                Timer1.Interval = 1000;//設定每秒執行一次
                Timer1.Enabled = false;//先關閉計時
                ViewState["time"] = 0;
                Label1.Text = "";
            }
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            //Yuping 本機端MYSQL 設定
            //string connection = "server=localhost;user id=root;password=27763923;database=sakila; pooling=true;";
            //目前開發本機端MYSQL 設定
          //  string connection = "server=localhost;user id=root;password=K@admin123456;database=sakila; pooling=true;";
            //目前佈署端local host MYSQL 設定
            string connection = "server=localhost;user id=root;password=Xcold@246810;database=sakila; pooling=true;Min Pool Size=0;Max Pool Size=3000;";

            MySqlConnection conn = new MySqlConnection(connection);

            //pf,cc1,cc2工作數據暫訂upload 位置資料夾(SourceFolder)
            // String SourceFolder = "z:\\\\source_pfcc";
            //String SourceFolder = @"Z:\source_pfcc";
            //因佈署後UNC路徑目前無法透過磁區辨別,只能由源頭IP位置找尋
           //  String SourceFolder = @"\\192.168.3.100\hr_tmp\source_pfcc";
            String DestinationFolder = "c:\\\\tempcsv";
            String Filename = "";

            String LoadSql = "";

            String dumpcsv = "";
            String fileExtension = "csv";
            bool iscsvexist = false;
            bool IsOverWrite = true;
            bool copy_one = true;
            bool mannulrun = true;
            //load 資料


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

            //由上搜尋站點字串判斷要清除當前一站暫存table內容
            //for pf 
            if (flitersite.Equals("K00")) {
                pf_cctable = "pfprocess001";
            }// for cc1 或 cc2
            else if (flitersite.Equals("H00") || flitersite.Equals("CC-"))
            {
                pf_cctable = "processcc";
            }
            else{
                LResult.Text = "沒有符合此(pf,cc系列)工作項目csv,請在確認!";
                return;
            }


            LResult.Text = "處理表單("+ Filename + ")進行中......";


            LoadSql = "delete from test_LoadPFData003; ";    //刪除暫存TABLE
            LoadSql = LoadSql + "TRUNCATE TABLE " + pf_cctable +"; ";
            //實際路徑是 C:\ProgramData\MySQL\MySQL Server 8.0\Data\test\
            LoadSql = LoadSql + " load data infile 'c:\\\\tempcsv\\\\" + Filename + " ' into table test_loadpfdata003 fields terminated by ',' ;";
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

            //TextBox1.Text = stepValue[1];
            switch (vparameter)  //STEP 
            {
                case "023": //pf ==>'023'
                    stepValue = new string[] { "2", "4", "6" };  //step 
                    divValue = new string[] { "2", "4", "6" };
                    break;
                case "010":  //cc1
                    stepValue = new string[] { "1", "3", "5" };
                    divValue = new string[] { "1", "3", "5" };

                    break;
                case "017": //cc2
                    if (vparameter_chg == "0172") //cc2-2 2024
                    {
                            stepValue = new string[] { "1", "3", "7" };
                            divValue = new string[] { "1", "3", "7" };
                    }else  //cc2 2023
                    {
                            stepValue = new string[] { "1", "5", "9" };
                            divValue = new string[] { "1", "5", "9" };

                    }
                    break;


            }

             
            string tableTitleSql = "", columnSql = "", valueSql = "" , detailSelect = "";

            string cc1SelectSql = "";



            string VD28 = "", VAHD28 = "", VD32 = "", VAHD32 = "", VD35 = "", VAHD35 = "";
            string VCCcurrent = "", VOCV = "", VaverageV1 = "", VaverageV2 = "", VaverageV3 = "", Vcharge34V = "";
            string Vcharge345V = "", Vcharge35V = "", Vtime50A = "", VV = "", VV1 = "", VV2 = "";
            string VV3 = "", VV4 = "", VmOhm = "",Vpara = "";


            if (dr.HasRows)
            {
                //使用Read方法把資料讀進Reader，讓Reader一筆一筆順向指向資料列，並回傳是否成功。
                while (dr.Read())
                { //應該只有一筆

                    //開36個insert 
                    for (int iFlag = 1; iFlag <= 36; iFlag++)
                    {


                        cc1SelectSql = "select max(a.VD28) VD28, max(a.VAHD28) VAHD28, max(a.VD32) VD32, max(a.VAHD32) VAHD32, max(a.VD35) VD35, max(a.VAHD35) VAHD35 ";
                        cc1SelectSql = cc1SelectSql + ",(select fld" + vComID + " as OCV from test_LoadPFData003 LIMIT 10, 1)  OCV  /*fld做變更*/ ";
                        cc1SelectSql = cc1SelectSql + " , max(a.CCcurrent) CCcurrent ";
                        cc1SelectSql = cc1SelectSql + ",((select sum(cast(fld" + (vComID) + " as decimal)) from test_LoadPFData003 b where b.fld7 = '" + divValue[0] + "' and b.fld" + (vComID + 1) + " > '10') / ";
                        cc1SelectSql = cc1SelectSql + "((select sum(cast(fld7 as decimal)) from test_LoadPFData003 b where b.fld7 = '" + divValue[0] + "' and b.fld" + (vComID + 1) + " > '10')/ " + divValue[0] + ")) averageV1 ";
                        cc1SelectSql = cc1SelectSql + ",((select sum(cast(fld" + (vComID) + " as decimal)) from test_LoadPFData003 b where b.fld7 = '" + divValue[1] + "' and b.fld" + (vComID + 1) + " > '10') / ";
                        cc1SelectSql = cc1SelectSql + "((select sum(cast(fld7 as decimal)) from test_LoadPFData003 b where b.fld7 = '" + divValue[1] + "' and b.fld" + (vComID + 1) + " > '10')/ " + divValue[1] + ")) averageV2 ";
                        cc1SelectSql = cc1SelectSql + ",((select sum(cast(fld" + (vComID) + " as decimal)) from test_LoadPFData003 b where b.fld7 = '" + divValue[2] + "' and b.fld" + (vComID + 1) + " > '10') / ";
                        cc1SelectSql = cc1SelectSql + "((select sum(cast(fld7 as decimal)) from test_LoadPFData003 b where b.fld7 = '" + divValue[2] + "' and b.fld" + (vComID + 1) + " > '10')/ " + divValue[2] + ")) averageV3 ";
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
                                if (vparameter_chg == "0172") //cc2-2 2024
                                {
                                    //  jj7 5169 ,  jj8 8395    =(@INDIRECT((ADDRESS($JJ$7, JF14)), 1))                           
                                    cc1SelectSql = cc1SelectSql + ",(SELECT COUNT(*)  FROM test_LoadPFData003 WHERE fld7 = '3' and  cast( fld" + (vComID + 1) + "  as decimal) > 20) as time50A ";
                                    cc1SelectSql = cc1SelectSql + ",(select  fld" + vComID + " from test_LoadPFData003 limit 4976,1 ) as V ";
                                    cc1SelectSql = cc1SelectSql + ",(select  fld" + vComID + " from test_LoadPFData003 limit 5168,1) as V1 ";
                                    cc1SelectSql = cc1SelectSql + ",(select  fld" + vComID + " from test_LoadPFData003 limit 8394,1 ) as V2 ";
                                    cc1SelectSql = cc1SelectSql + ",(select  fld" + (vComID + 1) + " from test_LoadPFData003 limit 5114,1) as v3 ";
                                    cc1SelectSql = cc1SelectSql + ",(select  fld" + (vComID + 1) + " from test_LoadPFData003 limit 5178,1) as v4 ";

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

                                VD28 = Convert.ToString(dr_detail["VD28"].ToString());
                                VAHD28 = Convert.ToString(dr_detail["VAHD28"].ToString());
                                VD32 = Convert.ToString(dr_detail["VD32"].ToString());
                                VAHD32 = Convert.ToString(dr_detail["VAHD32"].ToString());
                                VD35 = Convert.ToString(dr_detail["VD35"].ToString());
                                VAHD35 = Convert.ToString(dr_detail["VAHD35"].ToString());




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

                                        VCCcurrent = Convert.ToString(dr_detail["CCcurrent"].ToString());
                                        VOCV = Convert.ToString(dr_detail["OCV"].ToString());
                                        VaverageV1 = Convert.ToString(dr_detail["averageV1"].ToString());
                                        VaverageV2 = Convert.ToString(dr_detail["averageV2"].ToString());
                                        VaverageV3 = Convert.ToString(dr_detail["averageV3"].ToString());
                                        Vcharge34V = Convert.ToString(dr_detail["charge34V"].ToString());
                                        Vcharge345V = Convert.ToString(dr_detail["charge345V"].ToString());
                                        Vcharge35V = Convert.ToString(dr_detail["charge35V"].ToString());

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
                                            VmOhm = Convert.ToString( Math.Abs(Convert.ToDecimal(VV1) - Convert.ToDecimal(VV2)) / Math.Abs(Convert.ToDecimal(VV3) - Convert.ToDecimal(VV4)));
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
                                    tableTitleSql = tableTitleSql +  " (ID,StartDateD,EnddateD,trayID,parameter ";
                                    tableTitleSql = tableTitleSql + ",State,VD28,VS28,VAHD28,VAHS28 ";
                                    tableTitleSql = tableTitleSql + " ,VD32 ,VS32 ,VAHD32,VAHS32 ,VD35  ";
                                    tableTitleSql = tableTitleSql + " ,VS35,VAHD35 ,VAHS35,FileName,Process,AnlaysisDayD ";

                                    insertSql = insertSql + tableTitleSql +  " ) " + valueSql + ");";
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
                                    columnSql = columnSql + ",time50A,v,v1,v2,v3,v4,mOhm"; //cc2才有的，cc1要塞的話，值均為0 mOhm 是用算值出來的
                                    
                                    valueSql = valueSql + ",'" + Vpara + "'";  //para
                                    valueSql = valueSql + ", "+ VCCcurrent + "," + VOCV + "," + VaverageV1 + ","  + VaverageV2 + ","  + VaverageV3 + ","  + Vcharge34V + ","  + Vcharge345V + ","  + Vcharge35V ;
                                    valueSql = valueSql + ", " + Vtime50A + ", " + VV + ", " + VV1 + ", " + VV2 + ", " + VV3 + ", " + VV4 + ", " + VmOhm + ") ";
                                    //新增vvalueSql//增加value(
                                    insertSql = insertSql + tableTitleSql + columnSql + " ) " + valueSql +";";
                                    break;

                            }

                            //alueSql = valueSql + ") ; ";

                            vComID = vComID + 7;
                            vState = vState + 7;

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
                        break;
                    case "010":  //cc1
                        dumpcsv = "SELECT * FROM sakila.processcc;";
                        Filename = Filename + "-process-cc1.csv";

                        break;
                    case "017": //cc2
                        dumpcsv = "SELECT * FROM sakila.processcc;";

                        if (vparameter_chg == "0172") //cc2-2 2024
                        {
                            Filename = Filename + "-process-cc2-2.csv";
                        }
                        else {
                            Filename = Filename + "-process-cc2.csv";
                        } 
                        break;
                }
            

                //若路徑資料夾(C:\\pf-cc)沒有則這邊建立,for MYSQL LOAD REQUIRE
                if (!Directory.Exists(ResultTaskFolder))
                    Directory.CreateDirectory(ResultTaskFolder);

                ExportToCsv resultcsv = new ExportToCsv();

                //  string pfccPath_File = Server.MapPath("~/" + "pf-cc" + "/")+ Filename;
                string pfccPath_File = Path.Combine(ResultTaskFolder, Filename);
                DataTable dtView = resultcsv.Export(connection, dumpcsv, pfccPath_File);

                csvview.DataSource = dtView;
                csvview.DataBind();

                LResult.Text = "篩選型號資料完畢!";

               
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
                EXEC_Save_PFCCbat();


            } //end if (讀檔錯誤判斷====>)

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
                if (flitersite.Equals("K00"))
                {
                    pf_cctable = "pfprocess001";
                }// for cc1 或 cc2
                else if (flitersite.Equals("H00") || flitersite.Equals("CC-"))
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


            EXEC_Save_PFCCbat();

            LResult.Text = "所有pf,cc1,cc2轉換任務已完成 / 請執行copy_pfcc_result.bat將數據結果存到 pfcc_result";

        }
        protected async Task  ExecuteTask(string taskId, string pfcctype, int tnum,CancellationToken token)
        {
            //Yuping 本機端MYSQL 設定
            //string connection = "server=localhost;user id=root;password=27763923;database=sakila; pooling=true;";
            //目前開發本機端MYSQL 設定
          //  string connection = "server=localhost;user id=root;password=K@admin123456;database=sakila; pooling=true;";
           

            //目前佈署端local host MYSQL 設定
            //string connection = "server=localhost;user id=root;password=Xcold@246810;database=sakila; pooling=true;";
            string connection = "server=localhost;user id=root;password=Xcold@246810;database=sakila; pooling=true;Min Pool Size=0;Max Pool Size=3000;";
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
            String pf_cctable = "";

            String LoadSql = "";

            String dumpcsv = "";
            String fileExtension = "csv";
            bool iscsvexist = false;
            bool IsOverWrite = true;
            bool copy_one = true;
            //load 資料


            //PF + CC 

            //--start 這邊為(pf,cc1,cc2)檔案名稱和檔案型態,實際擷取的狀態依照前收集的名稱列--
            //Filename = taskId;
            Filename = Path.GetFileName(taskId);
            pf_cctable = pfcctype;
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

                //TextBox1.Text = stepValue[1];
                switch (vparameter)  //STEP 
                {
                    case "023": //pf ==>'023'
                        stepValue = new string[] { "2", "4", "6" };  //step 
                        divValue = new string[] { "2", "4", "6" };
                        break;
                    case "010":  //cc1
                        stepValue = new string[] { "1", "3", "5" };
                        divValue = new string[] { "1", "3", "5" };

                        break;
                    case "017": //cc2
                        if (vparameter_chg == "0172") //cc2-2 2024
                        {
                            stepValue = new string[] { "1", "3", "7" };
                            divValue = new string[] { "1", "3", "7" };
                        }
                        else  //cc2 2023
                        {
                            stepValue = new string[] { "1", "5", "9" };
                            divValue = new string[] { "1", "5", "9" };

                        }
                        break;


                }


                string tableTitleSql = "", columnSql = "", valueSql = "", detailSelect = "";

                string cc1SelectSql = "";



                string VD28 = "", VAHD28 = "", VD32 = "", VAHD32 = "", VD35 = "", VAHD35 = "";
                string VCCcurrent = "", VOCV = "", VaverageV1 = "", VaverageV2 = "", VaverageV3 = "", Vcharge34V = "";
                string Vcharge345V = "", Vcharge35V = "", Vtime50A = "", VV = "", VV1 = "", VV2 = "";
                string VV3 = "", VV4 = "", VmOhm = "", Vpara = "";


                if (dr.HasRows)
                {
                    //使用Read方法把資料讀進Reader，讓Reader一筆一筆順向指向資料列，並回傳是否成功。
                    while (dr.Read())
                    { //應該只有一筆

                        //開36個insert 
                        for (int iFlag = 1; iFlag <= 36; iFlag++)
                        {


                            cc1SelectSql = "select max(a.VD28) VD28, max(a.VAHD28) VAHD28, max(a.VD32) VD32, max(a.VAHD32) VAHD32, max(a.VD35) VD35, max(a.VAHD35) VAHD35 ";
                            cc1SelectSql = cc1SelectSql + ",(select fld" + vComID + " as OCV from test_LoadPFData003 LIMIT 10, 1)  OCV  /*fld做變更*/ ";
                            cc1SelectSql = cc1SelectSql + " , max(a.CCcurrent) CCcurrent ";
                            cc1SelectSql = cc1SelectSql + ",((select sum(cast(fld" + (vComID) + " as decimal)) from test_LoadPFData003 b where b.fld7 = '" + divValue[0] + "' and b.fld" + (vComID + 1) + " > '10') / ";
                            cc1SelectSql = cc1SelectSql + "((select sum(cast(fld7 as decimal)) from test_LoadPFData003 b where b.fld7 = '" + divValue[0] + "' and b.fld" + (vComID + 1) + " > '10')/ " + divValue[0] + ")) averageV1 ";
                            cc1SelectSql = cc1SelectSql + ",((select sum(cast(fld" + (vComID) + " as decimal)) from test_LoadPFData003 b where b.fld7 = '" + divValue[1] + "' and b.fld" + (vComID + 1) + " > '10') / ";
                            cc1SelectSql = cc1SelectSql + "((select sum(cast(fld7 as decimal)) from test_LoadPFData003 b where b.fld7 = '" + divValue[1] + "' and b.fld" + (vComID + 1) + " > '10')/ " + divValue[1] + ")) averageV2 ";
                            cc1SelectSql = cc1SelectSql + ",((select sum(cast(fld" + (vComID) + " as decimal)) from test_LoadPFData003 b where b.fld7 = '" + divValue[2] + "' and b.fld" + (vComID + 1) + " > '10') / ";
                            cc1SelectSql = cc1SelectSql + "((select sum(cast(fld7 as decimal)) from test_LoadPFData003 b where b.fld7 = '" + divValue[2] + "' and b.fld" + (vComID + 1) + " > '10')/ " + divValue[2] + ")) averageV3 ";
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
                                    if (vparameter_chg == "0172") //cc2-2 2024
                                    {
                                        //  jj7 5169 ,  jj8 8395    =(@INDIRECT((ADDRESS($JJ$7, JF14)), 1))                           
                                        cc1SelectSql = cc1SelectSql + ",(SELECT COUNT(*)  FROM test_LoadPFData003 WHERE fld7 = '3' and  cast( fld" + (vComID + 1) + "  as decimal) > 20) as time50A ";
                                        cc1SelectSql = cc1SelectSql + ",(select  fld" + vComID + " from test_LoadPFData003 limit 4976,1 ) as V ";
                                        cc1SelectSql = cc1SelectSql + ",(select  fld" + vComID + " from test_LoadPFData003 limit 5168,1) as V1 ";
                                        cc1SelectSql = cc1SelectSql + ",(select  fld" + vComID + " from test_LoadPFData003 limit 8394,1 ) as V2 ";
                                        cc1SelectSql = cc1SelectSql + ",(select  fld" + (vComID + 1) + " from test_LoadPFData003 limit 5114,1) as v3 ";
                                        cc1SelectSql = cc1SelectSql + ",(select  fld" + (vComID + 1) + " from test_LoadPFData003 limit 5178,1) as v4 ";

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

                                    VD28 = Convert.ToString(dr_detail["VD28"].ToString());
                                    VAHD28 = Convert.ToString(dr_detail["VAHD28"].ToString());
                                    VD32 = Convert.ToString(dr_detail["VD32"].ToString());
                                    VAHD32 = Convert.ToString(dr_detail["VAHD32"].ToString());
                                    VD35 = Convert.ToString(dr_detail["VD35"].ToString());
                                    VAHD35 = Convert.ToString(dr_detail["VAHD35"].ToString());




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

                                            VCCcurrent = Convert.ToString(dr_detail["CCcurrent"].ToString());
                                            VOCV = Convert.ToString(dr_detail["OCV"].ToString());
                                            VaverageV1 = Convert.ToString(dr_detail["averageV1"].ToString());
                                            VaverageV2 = Convert.ToString(dr_detail["averageV2"].ToString());
                                            VaverageV3 = Convert.ToString(dr_detail["averageV3"].ToString());
                                            Vcharge34V = Convert.ToString(dr_detail["charge34V"].ToString());
                                            Vcharge345V = Convert.ToString(dr_detail["charge345V"].ToString());
                                            Vcharge35V = Convert.ToString(dr_detail["charge35V"].ToString());

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
                                            VmOhm = Convert.ToString(Math.Abs(Convert.ToDecimal(VV1) - Convert.ToDecimal(VV2)) / Math.Abs(Convert.ToDecimal(VV3) - Convert.ToDecimal(VV4)));
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
                                        tableTitleSql = tableTitleSql + " ,VS35,VAHD35 ,VAHS35,FileName,Process,AnlaysisDayD ";

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
                                        columnSql = columnSql + ",time50A,v,v1,v2,v3,v4,mOhm"; //cc2才有的，cc1要塞的話，值均為0 mOhm 是用算值出來的

                                        valueSql = valueSql + ",'" + Vpara + "'";  //para
                                        valueSql = valueSql + ", " + VCCcurrent + "," + VOCV + "," + VaverageV1 + "," + VaverageV2 + "," + VaverageV3 + "," + Vcharge34V + "," + Vcharge345V + "," + Vcharge35V;
                                        valueSql = valueSql + ", " + Vtime50A + ", " + VV + ", " + VV1 + ", " + VV2 + ", " + VV3 + ", " + VV4 + ", " + VmOhm + ") ";
                                        //新增vvalueSql//增加value(
                                        insertSql = insertSql + tableTitleSql + columnSql + " ) " + valueSql + ";";
                                        break;

                                }

                                //alueSql = valueSql + ") ; ";

                                vComID = vComID + 7;
                                vState = vState + 7;

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
                        break;
                    case "010":  //cc1
                        dumpcsv = "SELECT * FROM sakila.processcc;";
                        Filename = Filename + "-process-cc1.csv";

                        break;
                    case "017": //cc2
                        dumpcsv = "SELECT * FROM sakila.processcc;";

                        if (vparameter_chg == "0172") //cc2-2 2024
                        {
                            Filename = Filename + "-process-cc2-2.csv";
                        }
                        else
                        {
                            Filename = Filename + "-process-cc2.csv";
                        }
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

                    //  string pfccPath_File = Server.MapPath("~/" + "pf-cc" + "/")+ Filename;
                    string pfccPath_File = Path.Combine(ResultTaskFolder, Filename);

                    DataTable dtView = resultcsv.Export(connection, dumpcsv, pfccPath_File);

                    //csvview.DataSource = dtView;
                    //csvview.DataBind();
                    // LResult.Text = "篩選型號資料完畢!";

                    List<string>  Del_ComplateFile= new List<string>();
                    Del_ComplateFile.Add(OriginallFile);


                    //刪除已經完成之數據原始檔案
                    DeleteTHreadOKFiles(SourceFolder, Del_ComplateFile, true);
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

        public void RewriteAndAppendToFile(string filePath, List<string> newLines)
        {
            // 讀取舊檔案內容
            List<string> existingLines = new List<string>();
            DateTime currentDateTime = DateTime.Now;
            string formattedFull = currentDateTime.ToString("yyyy-MM-dd HH:mm:ss");

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
    }
}