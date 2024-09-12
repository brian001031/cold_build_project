using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MySql.Data.MySqlClient;
using System.Data;
using System.Text;

namespace WebApplication1
{
    public class DBmanager
    {

        public String Data
        {
            get
            {
                String ss = "";
               // var courseNameText = new StringBuilder();
                string connection = " server=192.168.3.100;user id=root;password=Admin0331;database=hr; pooling=true; ";
                MySqlConnection conn = new MySqlConnection(connection);
                string sqlQuery = "SELECT * FROM hr.frmleave";

                MySqlCommand comm = new MySqlCommand(sqlQuery, conn);
                conn.Open();
                MySqlDataReader dr = comm.ExecuteReader();

                //檢查是否有資料列
                if (dr.HasRows)
                {
                    //使用Read方法把資料讀進Reader，讓Reader一筆一筆順向指向資料列，並回傳是否成功。
                    while (dr.Read())
                    {

                        //DataReader讀出欄位內資料的方式，通常也可寫Reader[0]、[1]...[N]代表第一個欄位到N個欄位。
                        ss += Convert.ToString(dr["frmname"].ToString()+ " -> " + dr["memDep"].ToString() + " -> " + dr["memCorp"].ToString() + "\r\n")  ;
                        //ss = dr["memID"].getString() + " -> " + dr["card_time"].ToString() + " -> " + dr["card_name"].ToString();
                        // ss += dr.GetString(0);




                    }
                }
                //關閉與資料庫連接的通道
                conn.Close();
                return ss;

            }
        }
    }
}