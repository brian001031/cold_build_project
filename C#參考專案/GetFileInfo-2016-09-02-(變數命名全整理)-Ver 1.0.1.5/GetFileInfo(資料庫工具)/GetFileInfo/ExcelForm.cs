using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
//(+) 2016.07.26 sql lib include
using System.Data.SqlClient;
using System.Data.OleDb;        //for Microsoft SQL 3.5
//using Microsoft.Office.Interop.Excel;   //匯入Export Excel.dll ,2016.08.31,Brian 
using Excel = Microsoft.Office.Interop.Excel;


namespace GetFileInfo
{
    public partial class ExcelTask : Form
    {
        /**************************Declare Global Variable ***************************/                 
    


        /********************************Declare End ***********************************/
               
        public ExcelTask()
        {
            InitializeComponent();
        }

        private void ExcelTask_Load(object sender, EventArgs e)
        {            
          //  rbn_DataLog.Select();
         //   rbn_DataLog.Checked = false;    
            rbn_DataTable1.Checked = false;
            rbn_DataLog.Checked = false;
        }
        

        //啟動讀取資料表(DataLog),2016.08.31,Brian
        private void rbn_DataLog_CheckedChanged(object sender, EventArgs e)
        {
            //開啟讀取
            if (rbn_DataLog.Checked == true)
            {
               // MessageBox.Show("rbn_DataLog 已被選取 OK!");
               
            }          
  
        }

        //啟動讀取資料表(DataTable1),2016.08.31,Brian        
        private void rbn_DataTable1_CheckedChanged(object sender, EventArgs e)
        {
            //開啟讀取
            if (rbn_DataTable1.Checked == true)
            {
              //  MessageBox.Show("rbn_DataTable1 已被選取 OK!");
            
            }       

        }

      

      
    }

}
