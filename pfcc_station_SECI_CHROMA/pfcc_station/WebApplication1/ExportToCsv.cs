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
    }
}