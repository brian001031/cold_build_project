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

namespace WebApplication1
{
    public partial class Search : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void GridView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        protected void Button3_Click(object sender, EventArgs e)
        {
            string connection = "server=localhost;user id=root;password=27763923;database=sakila; pooling=true;";
            MySqlConnection conn = new MySqlConnection(connection);
            String sSQLSearch = "";

            String sSQLTitle = "" ;

            string selectedOption = string.Empty;

            if (rbOption1.Checked)
            {
                selectedOption = "PF";
                sSQLTitle = "SELECT ID    ,StartDateD    , EnddateD    , trayID    , parameter    , State   " +
                    ", VD28  as '2.8V'    ,VAHD28 as '2.8V AH'    ,VD32 as '3.2V'    ,VAHD32 as  '3.2VAH'    " +
                    ",VD35 as '3.5V'    ,VAHD35 as '3.5VAh'    ,FileName    ,Process    ,AnlaysisDayD" +
                    " FROM pfprocess001  " +
                    " where 1=1 ";
            }
            else if (rbOption2.Checked)
            {
                selectedOption = "CC1";
                sSQLTitle = "SELECT ID    ,StartDateD    , EnddateD    , trayID    , parameter    , State   " +
                    ", VDA  as '2.8V'    ,VAHDA as '2.8V AH'    ,VDB as '3.2V'    ,VAHDB as  '3.2VAH'    " +
                    ",VDC as '3.5V'    ,VAHDC as '3.5VAh'    ,FileName    ,Process    ,AnlaysisDayD " +
                    " FROM processcc " +
                    " where Para = 'CC1' ";


            }
            else if (rbOption3.Checked)
            {
                selectedOption = "CC2";
                sSQLTitle = "SELECT ID    ,StartDateD    , EnddateD    , trayID    , parameter    , State   " +
                    ", VDA  as '2.8V'    ,VAHDA as '2.8V AH'    ,VDB as '3.2V'    ,VAHDB as  '3.2VAH'    " +
                    ",VDC as '3.5V'    ,VAHDC as '3.5VAh'    ,FileName    ,Process    ,AnlaysisDayD " +
                    " FROM processcc " +
                    " where Para = 'CC2' ";

            }

            LResult.Text = "Selected option: " + selectedOption;



            



            if ((TextBox1.Text != "") || (TextBox2.Text != ""))
            {
                //sSQLSearch = sSQLSearch + " Where ";



                if (TextBox1.Text != "")
                {
                    sSQLSearch = sSQLSearch + " and  trim(id) = " + "'" + TextBox1.Text + "'";

                }

                if (TextBox2.Text != "")
                {
                    sSQLSearch = sSQLSearch + "  and StartDateD = " + "'" + TextBox2.Text + "'";
                }


            }
            //String tempSql = sSQLTitle + sSQLSearch;
            MySqlDataAdapter myAdapter = new MySqlDataAdapter(sSQLTitle + sSQLSearch, connection);

            DataSet ds = new DataSet();
            try
            {
                myAdapter.Fill(ds, "test");
                GridView1.DataSource = ds;
                GridView1.DataBind();

            }
            catch (Exception ex)
            {
                LResult.Text = "資料錯誤";
            }
        }
    }
}