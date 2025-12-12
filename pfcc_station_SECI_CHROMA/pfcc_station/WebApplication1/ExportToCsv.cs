using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using System.Data;
using MySql.Data.MySqlClient;
using System.IO;
using System.Text;
using WebApplication1;

namespace WebApplication1
{
    public class ExportToCsv
    {
        public DataTable Export(string connectionString, string query, string filePath )
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                MySqlCommand command = new MySqlCommand(query, connection);
                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);
                

                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    // 寫入列名
                    for (int i = 0; i < dataTable.Columns.Count; i++)
                    {
                        writer.Write(dataTable.Columns[i].ColumnName);
                        if (i < dataTable.Columns.Count - 1)
                            writer.Write(",");
                    }
                    writer.WriteLine();

                    // 寫入數據
                    foreach (DataRow row in dataTable.Rows)
                    {
                        for (int i = 0; i < dataTable.Columns.Count; i++)
                        {
                            writer.Write(row[i].ToString());
                            if (i < dataTable.Columns.Count - 1)
                                writer.Write(",");
                        }
                        writer.WriteLine();
                    }
                }

                return dataTable;
            }
        }

        private string check_datetime_Formate_IsOK(string check_column_val , string reciver_formate) 
        {
            // 檢查是否符合日期時間格式
            DateTime parsedDate;
            bool isValidDate = DateTime.TryParseExact(check_column_val, reciver_formate,
                                                      CultureInfo.InvariantCulture,
                                                      DateTimeStyles.None,
                                                      out parsedDate);

            if (!isValidDate)
            {
                // 將其格式化為 MySQL 可以理解的格式：yyyy-MM-dd HH:mm:ss
                // string formattedDate = parsedDate.ToString("yyyy-MM-dd HH:mm:ss");
                DateTime currentTime = DateTime.Now;
                
                // 構建 SQL 語句中的值
               // string val_AnlaysisDayD = "STR_TO_DATE('" + check_column_val + "', '%Y-%m-%d %H:%i:%s')";
                string formattedDateTime = currentTime.ToString("yyyy/MM/dd HH:mm:ss");

                return formattedDateTime;
                // Console.WriteLine("轉換的 SQL 語句: " + val_AnlaysisDayD);
            }

            return check_column_val;
        
        }

        public bool Merge_existfilter_value(string connectionString, string connect_remote_merge, string allcolumn,  string DB, string tableName)
        {
            // ⭐ ASP.NET Script Timeout
            HttpContext.Current.Server.ScriptTimeout = 900;

            string[] columnNames = null;
            string final_InsertQuery = string.Empty;
            string colList = string.Empty;
            int insert_number = 0;
            string insert_value = string.Empty;
            List<string> row_pfcc_column = new List<string>();             
            List<string> row_pfcc_value  = new List<string>();
            MySqlDataReader dr ;

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {                              
                connection.Open();

                //(1).先將指定欄位name一次撈出
                using (MySqlCommand command = new MySqlCommand(allcolumn, connection)) 
                {
                    command.CommandTimeout = 300;   // ⭐新增 Timeout

                    dr = command.ExecuteReader();
                    if (dr.HasRows)
                    {
                        //使用Read方法把資料讀進Reader，讓Reader一筆一筆順向指向資料列，並回傳是否成功。
                        if (dr.Read())
                        {
                            // 获取 col_list 的值
                            colList = dr["col_list"].ToString();

                            // 如果需要处理 col_list，可以将它按逗号拆分成数组
                            columnNames = colList.Split(',');

                            //模組ID名稱欄位目前不一致
                            if (columnNames[0].ToString() == "ID")
                            {
                                columnNames[0] = "modelId"; // 修改索引0位置的值(ID 改 modeID)
                            }

                            // 打印每个列名                       
                           // Console.WriteLine(columnNames);                            
                        }
                    }
                }
                connection.Close();                              
            }

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string testModel = string.Empty;
                string testvalue = string.Empty;

                string queryExecuteSql = $@"SELECT {colList} FROM {tableName};";
                using (MySqlCommand comm_recivervalue = new MySqlCommand(queryExecuteSql, conn))
                {
                    comm_recivervalue.CommandTimeout = 300;   // ⭐新增 Timeout

                    dr = comm_recivervalue.ExecuteReader();
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            //(2).再將每組Row data 全部指定欄位value 存取長字串

                            string column_value = string.Empty;
                            for (int col = 0; col < columnNames.Length; col++)
                            {
                                if (col < columnNames.Length - 1)
                                {
                                    //第一筆為模組ID, 這邊需要索引欄位ID,不使用modelId,
                                    if (col == 0)
                                    {
                                        string modelId = Convert.ToString(dr["ID"]);
                                        //testModel = modelId.ToString();
                                        insert_value += " '" + modelId + "' ,";
                                    }
                                    else
                                    {
                                      insert_value += " '" + dr[columnNames[col]].ToString() + "' ,";

                                        //測試改變value是否能改變原先modleID,不insert 只做update,
                                        //string number = string.Empty;
                                        //string newtestnumber = string.Empty;
                                        //number = dr[columnNames[col]].ToString();

                                        //if ((testModel.IndexOf("MW2007H18985") != -1 && col == 2) || (testModel == "MW2007H18991" && col == 6) || (testModel == "MW200XA00171" && col == 9))
                                        //{
                                        //    double parsenumber;
                                        //    if ((col == 2 || col == 6) && double.TryParse(number, out parsenumber))
                                        //    {
                                        //        if (col == 2 &&  parsenumber != 1000000) {
                                        //            newtestnumber = "1910.097";
                                        //        } 
                                        //        else  if (col == 6 && parsenumber != 1000000)
                                        //        { 
                                        //                newtestnumber = "-639.22";
                                        //        }

                                        //    }
                                        //    else { 
                                        //        newtestnumber = "test_STR_update";
                                        //    }
                                        //    insert_value += " '" + newtestnumber + "' ,";
                                        //}
                                        //else {
                                        //    insert_value += " '" + dr[columnNames[col]].ToString() + "' ,";
                                        //}                                                                                                                               
                                    }
                                }//最後欄位 columnNames.Length -1
                                else
                                {                               
                                   insert_value += " '" + dr[columnNames[col]].ToString() + "' ";                                                                    
                                }

                            }

                            //將整理完dr.read 每組Row value 存入list
                            row_pfcc_value.Add(insert_value);
                            insert_number++;
                            insert_value = "";
                        }
                    }
                }
                conn.Close();
            }

            // Console.WriteLine("全部row_pfcc_value數據組: " + row_pfcc_value);
            //  Console.Write("insert_number = " + insert_number);

            
            //(3) 將所有取得數據組value 並且重整INSERT的字串總括 
            using (MySqlConnection conn_remote = new MySqlConnection(connect_remote_merge))
            {
                conn_remote.Open();


                string merge_acktable = string.Empty;


                //實際要merge的表單這邊做判斷
                if (tableName.IndexOf("pfprocess001") != -1) 
                    merge_acktable = "testmerge_pf";
                else
                    merge_acktable = "testmerge_CC1orCC2";

                string select_action = $"SELECT * FROM {merge_acktable};";
                using (MySqlCommand comm_merge = new MySqlCommand(select_action, conn_remote))
                {
                    comm_merge.CommandTimeout = 300;   // ⭐新增 Timeout

                    dr = comm_merge.ExecuteReader();
                   /// if (dr.HasRows)                   
                    {
                        //if (dr.Read())
                        {
                            string data = string.Empty;
                            string updatefield = "ON DUPLICATE KEY UPDATE\n";
                            // 使用 string.Join 來將陣列轉換為逗號分隔的字串
                            string Insert_columnAll = string.Join(",", columnNames);

                            StringBuilder insertQuery = new StringBuilder();
                            insertQuery.Append($@"INSERT INTO {merge_acktable} ({Insert_columnAll}) VALUES ");

                            //開36個insert value 組態
                            for (int iFlag = 0; iFlag < insert_number; iFlag++)
                            {
                                if (iFlag < insert_number - 1)
                                {
                                   data = "(" + row_pfcc_value[iFlag].ToString() + ") ,\n";                                    
                                }
                                else
                                {
                                    data = "(" + row_pfcc_value[iFlag].ToString() + ") \n";
                                }
                                insertQuery.Append(data);
                            }

                            //整理update 欄位name = Values(欄位name)
                            for (int col = 0; col < columnNames.Length; col++) {
                                if (col > 1 &&  col < columnNames.Length - 1)
                                     updatefield = updatefield + $"{columnNames[col]} = VALUES({columnNames[col]}),\n";
                                else if(col ==  columnNames.Length - 1)
                                    updatefield = updatefield +  $"{columnNames[col]} = VALUES({columnNames[col]});";
                            }                               
                            insertQuery.Append(updatefield);

                             final_InsertQuery = insertQuery.ToString().Trim('{', '}');
                            //Console.WriteLine(final_InsertQuery);                            
                        }
                    }
                }
                conn_remote.Close();
            }

            // (4) INSERT 指定合併表單 目前表單為(testmerge_pf 或 testmerge_cc1orcc2)
            using (MySqlConnection conn_remote2 = new MySqlConnection(connect_remote_merge))
            {
                //測試用
                //              string final_InsertQuery2 = @"
                //      INSERT INTO targettable (modelId, parameter, VDA,VSA)
                //       VALUES ('MW2007H14053', '023', 69.10 , 'fourcheck'),
                //       ('MW2007H14054', '023', -620.10 , 'Fivedcheck')
                //ON DUPLICATE KEY UPDATE                        
                //VDA = VALUES(VDA),
                //VSA = VALUES(VSA)
                //";  // 更新 VDA和VSA 的值（根據需要修改更新的欄位）


                conn_remote2.Open();

                using (MySqlCommand comm_merge_final = new MySqlCommand(final_InsertQuery, conn_remote2))
                {
                    comm_merge_final.CommandTimeout = 300;   // ⭐新增 Timeout
                    try
                    {
                        comm_merge_final.ExecuteNonQuery(); //insert or update 36筆                                
                        return true;

                    }
                    catch (Exception ex)
                    {
                        return false;
                    }
                    finally
                    {
                        if (conn_remote2.State != ConnectionState.Closed)
                            conn_remote2.Close();
                    }
                }
            }
        }
            
        //執行將判定NG的CSV 存入異常mes紀錄表單做後續追蹤處理
        public bool Insert_Error_convert_Raw(string connecting_mes, string all_columns, List<ErrorRaw> error_rawdata)
        {
            // ⭐ ASP.NET Script Timeout
            HttpContext.Current.Server.ScriptTimeout = 900;

            string[] columnNames_error = null;
            string Error_record_final_InsertQuery = string.Empty;
            string colList = string.Empty;
            int insert_error_number = 0;
            string insert_value_error = string.Empty;
            List<string> row_pfcc_error_info = new List<string>();
            MySqlDataReader dr;
            //忽略不更新欄位
            string[] Ignore_field_Error_Record = new string[] { "machineNumber", "errorDevice", "errorStatus" };
            //判定Insear 狀態
            bool isComplete = false;

            int totla_error_count = error_rawdata.Count();


            //(1) 先將異常表單productionerror_record 所有欄位取出
            using (MySqlConnection connection = new MySqlConnection(connecting_mes))
            {
                connection.Open();

                //(1).先將指定欄位name一次撈出
                using (MySqlCommand command = new MySqlCommand(all_columns, connection))
                {
                    command.CommandTimeout = 300;   // ⭐新增 Timeout

                    dr = command.ExecuteReader();
                    if (dr.HasRows)
                    {
                        //使用Read方法把資料讀進Reader，讓Reader一筆一筆順向指向資料列，並回傳是否成功。
                        if (dr.Read())
                        {
                            // 获取 col_list 的值
                            colList = dr["col_list"].ToString();

                            // 如果需要处理 col_list，可以将它按逗号拆分成数组
                            columnNames_error = colList.Split(',');

                            // 打印每个列名                       
                            Console.WriteLine(columnNames_error);                            
                        }
                    }
                }
                connection.Close();
            }


            //(2) 再將要存入遠端的結構error_rawdata依序排列,目前只存入(station ,machineNumber , errorDevice , errorStatus,created_at)
            for (int er = 0; er < error_rawdata.Count(); er++)
            {
                for (int col = 0; col < columnNames_error.Length; col++)
                {
                    if (col < columnNames_error.Length - 1)
                    {
                        //以下序號為要填入異常資訊的欄位
                        if (col <= 1)
                        {
                            if (col == 0)
                            {
                                string record_station = "Sulting32分選判別";
                                insert_value_error += " '" + record_station + "' ,";
                            }
                            else if (col == 1)
                            {
                                string record_machineNumber = error_rawdata[er].Machine_TrayID.ToString();
                                insert_value_error += " '" + record_machineNumber + "' ,";
                            }
                        }
                        else if (col == 4)
                        {
                            string record_errorDevice = error_rawdata[er].NgFile.ToString();
                            insert_value_error += " '" + record_errorDevice + "' ,";
                        }
                        else if (col == 5)
                        {
                            string record_errorStatus = error_rawdata[er].ErrorStatus.ToString();
                            insert_value_error += " '" + record_errorStatus + "' ,";
                        }
                        else
                        {
                            insert_value_error += " ' ',";
                        }

                    }
                    //最後欄位 columnNames.Length -1
                    else
                    {
                        insert_value_error += " now()";
                    }
                }

                //將整理完dr.read 每組Row value 存入list
                row_pfcc_error_info.Add(insert_value_error);
                insert_error_number++;
                insert_value_error = "";
            }

            Console.WriteLine("row_pfcc_error_info 最後得出insert Value 總匯集 = " + row_pfcc_error_info);


            string data = string.Empty;
            string updatefield_error = "ON DUPLICATE KEY UPDATE\n";
            // 使用 string.Join 來將陣列轉換為逗號分隔的字串
            string Insert_columnAll = string.Join(",", columnNames_error);

            StringBuilder insertQuery = new StringBuilder();
            insertQuery.Append($@"INSERT INTO productionError_Record ({Insert_columnAll}) VALUES ");

            //依據多少 error_rawdata.count insert value 組態
            for (int ierror = 0; ierror < insert_error_number; ierror++)
            {
                if (ierror < insert_error_number - 1)
                {
                    data = "(" + row_pfcc_error_info[ierror].ToString() + ") ,\n";
                }
                else
                {
                    data = "(" + row_pfcc_error_info[ierror].ToString() + ") \n";
                }
                insertQuery.Append(data);
            }

            //整理update 欄位name = Values(欄位name)
            for (int col = 0; col < columnNames_error.Length; col++)
            {
                string error_field = columnNames_error[col].ToString().Trim();

                // 若欄位在忽略列表中 → 跳過
                if (Ignore_field_Error_Record.Contains(error_field))
                {
                    Console.WriteLine($" 忽略欄位{columnNames_error[col]}不加入update! ");
                    continue;
                }

                if (col == columnNames_error.Length - 1)
                    updatefield_error = updatefield_error + $"{columnNames_error[col]} = VALUES({columnNames_error[col]});";
                else
                    updatefield_error = updatefield_error + $"{columnNames_error[col]} = VALUES({columnNames_error[col]}),\n";
            }
            insertQuery.Append(updatefield_error);

            Error_record_final_InsertQuery = insertQuery.ToString().Trim('{', '}');


            //(3) 開啟Meds遠端連線池 ,後續將所有取得error數據組value 並且重整INSERT的字串總括 
            using (MySqlConnection conn_remote = new MySqlConnection(connecting_mes))
            {                               
                conn_remote.Open();

                using (MySqlCommand comm_merge_final = new MySqlCommand(Error_record_final_InsertQuery, conn_remote))
                {
                    comm_merge_final.CommandTimeout = 300;   // ⭐新增 Timeout
                    try
                    {
                        comm_merge_final.ExecuteNonQuery(); //insert or update 36筆                                                        
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($" 存入異常統計表單有異常 檢查錯誤訊息: {ex.Message} ");
                        return false;
                    }
                    finally
                    {
                        if (conn_remote.State != ConnectionState.Closed)
                        {                        
                            conn_remote.Close();
                        }                            
                    }
                }

            }

        }
    }
}