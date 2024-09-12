namespace GetFileInfo
{
    partial class ExcelTask
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExcelTask));
            this.rbn_DataTable1 = new System.Windows.Forms.RadioButton();
            this.rbn_DataLog = new System.Windows.Forms.RadioButton();
            this.gbx_Excel = new System.Windows.Forms.GroupBox();
            this.dgv_dt2 = new System.Windows.Forms.DataGridView();
            this.btn_SaveExcel = new System.Windows.Forms.Button();
            this.gbx_Excel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_dt2)).BeginInit();
            this.SuspendLayout();
            // 
            // rbn_DataTable1
            // 
            this.rbn_DataTable1.AutoSize = true;
            this.rbn_DataTable1.Font = new System.Drawing.Font("標楷體", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.rbn_DataTable1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.rbn_DataTable1.Location = new System.Drawing.Point(293, 29);
            this.rbn_DataTable1.Name = "rbn_DataTable1";
            this.rbn_DataTable1.Size = new System.Drawing.Size(297, 31);
            this.rbn_DataTable1.TabIndex = 0;
            this.rbn_DataTable1.TabStop = true;
            this.rbn_DataTable1.Text = "資料表(DataTable1)";
            this.rbn_DataTable1.UseVisualStyleBackColor = true;
            this.rbn_DataTable1.CheckedChanged += new System.EventHandler(this.rbn_DataTable1_CheckedChanged);
            // 
            // rbn_DataLog
            // 
            this.rbn_DataLog.AutoSize = true;
            this.rbn_DataLog.Font = new System.Drawing.Font("標楷體", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.rbn_DataLog.Location = new System.Drawing.Point(22, 29);
            this.rbn_DataLog.Name = "rbn_DataLog";
            this.rbn_DataLog.Size = new System.Drawing.Size(252, 31);
            this.rbn_DataLog.TabIndex = 1;
            this.rbn_DataLog.TabStop = true;
            this.rbn_DataLog.Text = "資料表(DataLog)";
            this.rbn_DataLog.UseVisualStyleBackColor = true;
            this.rbn_DataLog.CheckedChanged += new System.EventHandler(this.rbn_DataLog_CheckedChanged);
            // 
            // gbx_Excel
            // 
            this.gbx_Excel.Controls.Add(this.rbn_DataTable1);
            this.gbx_Excel.Controls.Add(this.rbn_DataLog);
            this.gbx_Excel.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.gbx_Excel.ForeColor = System.Drawing.Color.Blue;
            this.gbx_Excel.Location = new System.Drawing.Point(63, 12);
            this.gbx_Excel.Name = "gbx_Excel";
            this.gbx_Excel.Size = new System.Drawing.Size(590, 74);
            this.gbx_Excel.TabIndex = 2;
            this.gbx_Excel.TabStop = false;
            this.gbx_Excel.Text = "Excel Select";
            // 
            // dgv_dt2
            // 
            this.dgv_dt2.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_dt2.Location = new System.Drawing.Point(63, 123);
            this.dgv_dt2.Name = "dgv_dt2";
            this.dgv_dt2.RowTemplate.Height = 24;
            this.dgv_dt2.Size = new System.Drawing.Size(590, 253);
            this.dgv_dt2.TabIndex = 3;
            // 
            // btn_SaveExcel
            // 
            this.btn_SaveExcel.Font = new System.Drawing.Font("標楷體", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btn_SaveExcel.Location = new System.Drawing.Point(469, 410);
            this.btn_SaveExcel.Name = "btn_SaveExcel";
            this.btn_SaveExcel.Size = new System.Drawing.Size(184, 54);
            this.btn_SaveExcel.TabIndex = 4;
            this.btn_SaveExcel.Text = "儲存Excel";
            this.btn_SaveExcel.UseVisualStyleBackColor = true;
            // 
            // ExcelTask
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.PaleGreen;
            this.ClientSize = new System.Drawing.Size(722, 508);
            this.Controls.Add(this.btn_SaveExcel);
            this.Controls.Add(this.dgv_dt2);
            this.Controls.Add(this.gbx_Excel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ExcelTask";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Excel-Form";
            this.Load += new System.EventHandler(this.ExcelTask_Load);
            this.gbx_Excel.ResumeLayout(false);
            this.gbx_Excel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_dt2)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RadioButton rbn_DataTable1;
        private System.Windows.Forms.RadioButton rbn_DataLog;
        private System.Windows.Forms.GroupBox gbx_Excel;
        private System.Windows.Forms.DataGridView dgv_dt2;
        private System.Windows.Forms.Button btn_SaveExcel;

    }
}