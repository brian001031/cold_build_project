namespace MMS_BrowseDatabase
{
    partial class Form_MMSBrowse
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_MMSBrowse));
            this.rbn_MaterialPuchWahsing = new System.Windows.Forms.RadioButton();
            this.rbn_WorkPicking = new System.Windows.Forms.RadioButton();
            this.rbn_ReturnMaterial = new System.Windows.Forms.RadioButton();
            this.gbx_ClassItemSelect = new System.Windows.Forms.GroupBox();
            this.tbx_InputSearch = new System.Windows.Forms.TextBox();
            this.lbl_UserRecord = new System.Windows.Forms.Label();
            this.cbx_SelectItem = new System.Windows.Forms.ComboBox();
            this.cbx_statusItem1 = new System.Windows.Forms.ComboBox();
            this.cbx_statusItem2 = new System.Windows.Forms.ComboBox();
            this.cbx_statusItem3 = new System.Windows.Forms.ComboBox();
            this.btn_ConfirmSearch = new System.Windows.Forms.Button();
            this.lbl_datetimeRun = new System.Windows.Forms.Label();
            this.lbl_OPdatetime = new System.Windows.Forms.Label();
            this.lbl_FixiedRun = new System.Windows.Forms.Label();
            this.lbl_SignInBody = new System.Windows.Forms.Label();
            this.dgv_MMSData = new System.Windows.Forms.DataGridView();
            this.lbl_cbxItem1 = new System.Windows.Forms.Label();
            this.lbl_cbxItem2 = new System.Windows.Forms.Label();
            this.lbl_cbxItem3 = new System.Windows.Forms.Label();
            this.tm_SysTime = new System.Windows.Forms.Timer(this.components);
            this.gbx_ClassItemSelect.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_MMSData)).BeginInit();
            this.SuspendLayout();
            // 
            // rbn_MaterialPuchWahsing
            // 
            this.rbn_MaterialPuchWahsing.AutoSize = true;
            this.rbn_MaterialPuchWahsing.Font = new System.Drawing.Font("PMingLiU", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.rbn_MaterialPuchWahsing.Location = new System.Drawing.Point(38, 24);
            this.rbn_MaterialPuchWahsing.Name = "rbn_MaterialPuchWahsing";
            this.rbn_MaterialPuchWahsing.Size = new System.Drawing.Size(114, 24);
            this.rbn_MaterialPuchWahsing.TabIndex = 0;
            this.rbn_MaterialPuchWahsing.TabStop = true;
            this.rbn_MaterialPuchWahsing.Text = "物料入庫";
            this.rbn_MaterialPuchWahsing.UseVisualStyleBackColor = true;
            this.rbn_MaterialPuchWahsing.CheckedChanged += new System.EventHandler(this.rbn_MaterialPuchWahsing_CheckedChanged);
            // 
            // rbn_WorkPicking
            // 
            this.rbn_WorkPicking.AutoSize = true;
            this.rbn_WorkPicking.Font = new System.Drawing.Font("PMingLiU", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.rbn_WorkPicking.Location = new System.Drawing.Point(221, 25);
            this.rbn_WorkPicking.Name = "rbn_WorkPicking";
            this.rbn_WorkPicking.Size = new System.Drawing.Size(114, 24);
            this.rbn_WorkPicking.TabIndex = 1;
            this.rbn_WorkPicking.TabStop = true;
            this.rbn_WorkPicking.Text = "工單領料";
            this.rbn_WorkPicking.UseVisualStyleBackColor = true;
            this.rbn_WorkPicking.CheckedChanged += new System.EventHandler(this.rbn_WorkPicking_CheckedChanged);
            // 
            // rbn_ReturnMaterial
            // 
            this.rbn_ReturnMaterial.AutoSize = true;
            this.rbn_ReturnMaterial.Font = new System.Drawing.Font("PMingLiU", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.rbn_ReturnMaterial.Location = new System.Drawing.Point(407, 25);
            this.rbn_ReturnMaterial.Name = "rbn_ReturnMaterial";
            this.rbn_ReturnMaterial.Size = new System.Drawing.Size(114, 24);
            this.rbn_ReturnMaterial.TabIndex = 2;
            this.rbn_ReturnMaterial.TabStop = true;
            this.rbn_ReturnMaterial.Text = "退料作業";
            this.rbn_ReturnMaterial.UseVisualStyleBackColor = true;
            this.rbn_ReturnMaterial.CheckedChanged += new System.EventHandler(this.rbn_ReturnMaterial_CheckedChanged);
            // 
            // gbx_ClassItemSelect
            // 
            this.gbx_ClassItemSelect.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.gbx_ClassItemSelect.Controls.Add(this.rbn_MaterialPuchWahsing);
            this.gbx_ClassItemSelect.Controls.Add(this.rbn_ReturnMaterial);
            this.gbx_ClassItemSelect.Controls.Add(this.rbn_WorkPicking);
            this.gbx_ClassItemSelect.Font = new System.Drawing.Font("PMingLiU", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.gbx_ClassItemSelect.Location = new System.Drawing.Point(29, 12);
            this.gbx_ClassItemSelect.Name = "gbx_ClassItemSelect";
            this.gbx_ClassItemSelect.Size = new System.Drawing.Size(565, 68);
            this.gbx_ClassItemSelect.TabIndex = 3;
            this.gbx_ClassItemSelect.TabStop = false;
            this.gbx_ClassItemSelect.Text = "類別開啟";
            // 
            // tbx_InputSearch
            // 
            this.tbx_InputSearch.Location = new System.Drawing.Point(162, 157);
            this.tbx_InputSearch.Name = "tbx_InputSearch";
            this.tbx_InputSearch.Size = new System.Drawing.Size(265, 25);
            this.tbx_InputSearch.TabIndex = 4;
            this.tbx_InputSearch.TextChanged += new System.EventHandler(this.tbx_InputSearch_TextChanged);
            this.tbx_InputSearch.MouseDown += new System.Windows.Forms.MouseEventHandler(this.tbx_InputSearch_MouseDown);
            // 
            // lbl_UserRecord
            // 
            this.lbl_UserRecord.AutoSize = true;
            this.lbl_UserRecord.BackColor = System.Drawing.Color.DarkTurquoise;
            this.lbl_UserRecord.Font = new System.Drawing.Font("PMingLiU", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_UserRecord.ForeColor = System.Drawing.Color.Black;
            this.lbl_UserRecord.Location = new System.Drawing.Point(24, 106);
            this.lbl_UserRecord.Name = "lbl_UserRecord";
            this.lbl_UserRecord.Size = new System.Drawing.Size(223, 28);
            this.lbl_UserRecord.TabIndex = 5;
            this.lbl_UserRecord.Text = "帳號使用紀錄:↓";
            // 
            // cbx_SelectItem
            // 
            this.cbx_SelectItem.FormattingEnabled = true;
            this.cbx_SelectItem.Location = new System.Drawing.Point(29, 157);
            this.cbx_SelectItem.Name = "cbx_SelectItem";
            this.cbx_SelectItem.Size = new System.Drawing.Size(115, 23);
            this.cbx_SelectItem.TabIndex = 6;
            this.cbx_SelectItem.SelectedIndexChanged += new System.EventHandler(this.cbx_SelectItem_SelectedIndexChanged);
            // 
            // cbx_statusItem1
            // 
            this.cbx_statusItem1.FormattingEnabled = true;
            this.cbx_statusItem1.Location = new System.Drawing.Point(481, 159);
            this.cbx_statusItem1.Name = "cbx_statusItem1";
            this.cbx_statusItem1.Size = new System.Drawing.Size(113, 23);
            this.cbx_statusItem1.TabIndex = 7;
            this.cbx_statusItem1.SelectedIndexChanged += new System.EventHandler(this.cbx_statusItem1_SelectedIndexChanged);
            // 
            // cbx_statusItem2
            // 
            this.cbx_statusItem2.FormattingEnabled = true;
            this.cbx_statusItem2.Location = new System.Drawing.Point(614, 159);
            this.cbx_statusItem2.Name = "cbx_statusItem2";
            this.cbx_statusItem2.Size = new System.Drawing.Size(113, 23);
            this.cbx_statusItem2.TabIndex = 8;
            this.cbx_statusItem2.SelectedIndexChanged += new System.EventHandler(this.cbx_statusItem2_SelectedIndexChanged);
            // 
            // cbx_statusItem3
            // 
            this.cbx_statusItem3.FormattingEnabled = true;
            this.cbx_statusItem3.Location = new System.Drawing.Point(748, 159);
            this.cbx_statusItem3.Name = "cbx_statusItem3";
            this.cbx_statusItem3.Size = new System.Drawing.Size(113, 23);
            this.cbx_statusItem3.TabIndex = 9;
            // 
            // btn_ConfirmSearch
            // 
            this.btn_ConfirmSearch.BackColor = System.Drawing.Color.DarkGreen;
            this.btn_ConfirmSearch.Font = new System.Drawing.Font("PMingLiU", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btn_ConfirmSearch.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btn_ConfirmSearch.Location = new System.Drawing.Point(899, 213);
            this.btn_ConfirmSearch.Name = "btn_ConfirmSearch";
            this.btn_ConfirmSearch.Size = new System.Drawing.Size(205, 50);
            this.btn_ConfirmSearch.TabIndex = 334;
            this.btn_ConfirmSearch.Text = "篩選";
            this.btn_ConfirmSearch.UseVisualStyleBackColor = false;
            this.btn_ConfirmSearch.Click += new System.EventHandler(this.btn_ConfirmSearch_Click);
            // 
            // lbl_datetimeRun
            // 
            this.lbl_datetimeRun.AutoSize = true;
            this.lbl_datetimeRun.Font = new System.Drawing.Font("PMingLiU", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_datetimeRun.ForeColor = System.Drawing.Color.Yellow;
            this.lbl_datetimeRun.Location = new System.Drawing.Point(993, 74);
            this.lbl_datetimeRun.Name = "lbl_datetimeRun";
            this.lbl_datetimeRun.Size = new System.Drawing.Size(91, 30);
            this.lbl_datetimeRun.TabIndex = 338;
            this.lbl_datetimeRun.Text = "--:--:--";
            // 
            // lbl_OPdatetime
            // 
            this.lbl_OPdatetime.AutoSize = true;
            this.lbl_OPdatetime.BackColor = System.Drawing.Color.Red;
            this.lbl_OPdatetime.Font = new System.Drawing.Font("Microsoft JhengHei", 19.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_OPdatetime.Location = new System.Drawing.Point(958, 20);
            this.lbl_OPdatetime.Name = "lbl_OPdatetime";
            this.lbl_OPdatetime.Size = new System.Drawing.Size(150, 42);
            this.lbl_OPdatetime.TabIndex = 337;
            this.lbl_OPdatetime.Text = "系統時間";
            // 
            // lbl_FixiedRun
            // 
            this.lbl_FixiedRun.AutoSize = true;
            this.lbl_FixiedRun.Font = new System.Drawing.Font("PMingLiU", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_FixiedRun.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.lbl_FixiedRun.Location = new System.Drawing.Point(781, 74);
            this.lbl_FixiedRun.Name = "lbl_FixiedRun";
            this.lbl_FixiedRun.Size = new System.Drawing.Size(130, 30);
            this.lbl_FixiedRun.TabIndex = 336;
            this.lbl_FixiedRun.Text = "維護中...";
            // 
            // lbl_SignInBody
            // 
            this.lbl_SignInBody.AutoSize = true;
            this.lbl_SignInBody.BackColor = System.Drawing.Color.Red;
            this.lbl_SignInBody.Font = new System.Drawing.Font("Microsoft JhengHei", 19.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_SignInBody.Location = new System.Drawing.Point(761, 21);
            this.lbl_SignInBody.Name = "lbl_SignInBody";
            this.lbl_SignInBody.Size = new System.Drawing.Size(150, 42);
            this.lbl_SignInBody.TabIndex = 335;
            this.lbl_SignInBody.Text = "登入人員";
            // 
            // dgv_MMSData
            // 
            this.dgv_MMSData.AllowDrop = true;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("PMingLiU", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgv_MMSData.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgv_MMSData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("PMingLiU", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgv_MMSData.DefaultCellStyle = dataGridViewCellStyle2;
            this.dgv_MMSData.Location = new System.Drawing.Point(29, 268);
            this.dgv_MMSData.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.dgv_MMSData.MultiSelect = false;
            this.dgv_MMSData.Name = "dgv_MMSData";
            this.dgv_MMSData.ReadOnly = true;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("PMingLiU", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgv_MMSData.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.dgv_MMSData.RowTemplate.Height = 27;
            this.dgv_MMSData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgv_MMSData.Size = new System.Drawing.Size(1075, 518);
            this.dgv_MMSData.TabIndex = 339;
            // 
            // lbl_cbxItem1
            // 
            this.lbl_cbxItem1.AutoSize = true;
            this.lbl_cbxItem1.Font = new System.Drawing.Font("PMingLiU", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_cbxItem1.Location = new System.Drawing.Point(513, 185);
            this.lbl_cbxItem1.Name = "lbl_cbxItem1";
            this.lbl_cbxItem1.Size = new System.Drawing.Size(38, 24);
            this.lbl_cbxItem1.TabIndex = 340;
            this.lbl_cbxItem1.Text = "==";
            // 
            // lbl_cbxItem2
            // 
            this.lbl_cbxItem2.AutoSize = true;
            this.lbl_cbxItem2.Font = new System.Drawing.Font("PMingLiU", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_cbxItem2.Location = new System.Drawing.Point(656, 185);
            this.lbl_cbxItem2.Name = "lbl_cbxItem2";
            this.lbl_cbxItem2.Size = new System.Drawing.Size(38, 24);
            this.lbl_cbxItem2.TabIndex = 341;
            this.lbl_cbxItem2.Text = "==";
            // 
            // lbl_cbxItem3
            // 
            this.lbl_cbxItem3.AutoSize = true;
            this.lbl_cbxItem3.Font = new System.Drawing.Font("PMingLiU", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_cbxItem3.Location = new System.Drawing.Point(796, 185);
            this.lbl_cbxItem3.Name = "lbl_cbxItem3";
            this.lbl_cbxItem3.Size = new System.Drawing.Size(38, 24);
            this.lbl_cbxItem3.TabIndex = 342;
            this.lbl_cbxItem3.Text = "==";
            // 
            // tm_SysTime
            // 
            this.tm_SysTime.Interval = 10;
            this.tm_SysTime.Tick += new System.EventHandler(this.tm_SysTime_Tick);
            // 
            // Form_MMSBrowse
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.SkyBlue;
            this.ClientSize = new System.Drawing.Size(1131, 808);
            this.Controls.Add(this.lbl_cbxItem3);
            this.Controls.Add(this.lbl_cbxItem2);
            this.Controls.Add(this.lbl_cbxItem1);
            this.Controls.Add(this.dgv_MMSData);
            this.Controls.Add(this.lbl_datetimeRun);
            this.Controls.Add(this.lbl_OPdatetime);
            this.Controls.Add(this.lbl_FixiedRun);
            this.Controls.Add(this.lbl_SignInBody);
            this.Controls.Add(this.btn_ConfirmSearch);
            this.Controls.Add(this.cbx_statusItem3);
            this.Controls.Add(this.cbx_statusItem2);
            this.Controls.Add(this.cbx_statusItem1);
            this.Controls.Add(this.cbx_SelectItem);
            this.Controls.Add(this.lbl_UserRecord);
            this.Controls.Add(this.tbx_InputSearch);
            this.Controls.Add(this.gbx_ClassItemSelect);
            this.ForeColor = System.Drawing.Color.Black;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form_MMSBrowse";
            this.Text = "MMS_BrowseInfo";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.gbx_ClassItemSelect.ResumeLayout(false);
            this.gbx_ClassItemSelect.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_MMSData)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton rbn_MaterialPuchWahsing;
        private System.Windows.Forms.RadioButton rbn_WorkPicking;
        private System.Windows.Forms.RadioButton rbn_ReturnMaterial;
        private System.Windows.Forms.GroupBox gbx_ClassItemSelect;
        private System.Windows.Forms.TextBox tbx_InputSearch;
        private System.Windows.Forms.Label lbl_UserRecord;
        private System.Windows.Forms.ComboBox cbx_SelectItem;
        private System.Windows.Forms.ComboBox cbx_statusItem1;
        private System.Windows.Forms.ComboBox cbx_statusItem2;
        private System.Windows.Forms.ComboBox cbx_statusItem3;
        private System.Windows.Forms.Button btn_ConfirmSearch;
        private System.Windows.Forms.Label lbl_datetimeRun;
        private System.Windows.Forms.Label lbl_OPdatetime;
        private System.Windows.Forms.Label lbl_FixiedRun;
        private System.Windows.Forms.Label lbl_SignInBody;
        private System.Windows.Forms.DataGridView dgv_MMSData;
        private System.Windows.Forms.Label lbl_cbxItem1;
        private System.Windows.Forms.Label lbl_cbxItem2;
        private System.Windows.Forms.Label lbl_cbxItem3;
        private System.Windows.Forms.Timer tm_SysTime;
    }
}

