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
                //--------------------------------------------------
                /*
                string connection = "server=localhost;user id=root;password=27763923;database=sakila; pooling=true;";
                MySqlConnection conn = new MySqlConnection(connection);
                //string sqlQuery = "SELECT * FROM hr.frmleave";--測試ok
                string sqlQuery = "insert into test_input0822_01(other_column) value('500')";
                MySqlCommand comm = new MySqlCommand(sqlQuery, conn);
                conn.Open();
                comm.ExecuteNonQuery();
                //MySqlDataReader dr = comm.ExecuteReader();
                */

                //------------------------------------------------
                

                // var courseNameText = new StringBuilder();
                //string connection = " server=192.168.3.100;user id=root;password=Admin0331;database=hr; pooling=true; ";  //--測試ok
                string connection = "server=localhost;user id=root;password=27763923;database=sakila; pooling=true;";
                MySqlConnection conn = new MySqlConnection(connection);
                //string sqlQuery = "SELECT * FROM hr.frmleave";--測試ok
                int row_i = 8;
                string sqlQuery = "select fld"+ row_i + " from test_LoadPFData003 LIMIT 7, 1";
                //---string sqlQuery = "select city_id,city,country_id from sakila.city limit 5";
                //test 欄位是否可以變數 


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
                        //ss += Convert.ToString(dr["city_id"].ToString() + " -> " + dr["city"].ToString() + " -> " + dr["country_id"].ToString() + "\r\n");
                        //測試變數是否可以
                        ss += Convert.ToString(dr["fld"+ row_i].ToString() + " -> -----------------"  + "\r\n");

                        //ss = dr["memID"].getString() + " -> " + dr["card_time"].ToString() + " -> " + dr["card_name"].ToString();
                        // ss += dr.GetString(0);




                    }
                }

                //test 第2個sql 

                 row_i = 15;
                 sqlQuery = "select fld" + row_i + " from test_LoadPFData003 LIMIT 7, 1";
                //---string sqlQuery = "select city_id,city,country_id from sakila.city limit 5";
                //test 欄位是否可以變數 


                 comm = new MySqlCommand(sqlQuery, conn);
                //conn.Open();
                dr.Close();
                 dr = comm.ExecuteReader();

                //檢查是否有資料列
                if (dr.HasRows)
                {
                    //使用Read方法把資料讀進Reader，讓Reader一筆一筆順向指向資料列，並回傳是否成功。
                    while (dr.Read())
                    {

                        //DataReader讀出欄位內資料的方式，通常也可寫Reader[0]、[1]...[N]代表第一個欄位到N個欄位。
                        //ss += Convert.ToString(dr["city_id"].ToString() + " -> " + dr["city"].ToString() + " -> " + dr["country_id"].ToString() + "\r\n");
                        //測試變數是否可以
                        ss += Convert.ToString(dr["fld" + row_i].ToString() + " -> -----------------" + "\r\n");

                        //ss = dr["memID"].getString() + " -> " + dr["card_time"].ToString() + " -> " + dr["card_name"].ToString();
                        // ss += dr.GetString(0);




                    }
                }
                //=======================================================================








                //關閉與資料庫連接的通道                

                conn.Close();
                return ss;

            }
        }
    }
}