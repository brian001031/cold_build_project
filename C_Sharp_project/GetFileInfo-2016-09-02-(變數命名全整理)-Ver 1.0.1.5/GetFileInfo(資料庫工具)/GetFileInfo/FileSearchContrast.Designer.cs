namespace GetFileInfo
{
    partial class FileCatch
    {
        /// <summary>
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置 Managed 資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器
        /// 修改這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FileCatch));
            this.lbl_PathFolder = new System.Windows.Forms.Label();
            this.btn_OpenFolder = new System.Windows.Forms.Button();
            this.fbd_OpenPathFolder = new System.Windows.Forms.FolderBrowserDialog();
            this.tbx_FilePath = new System.Windows.Forms.TextBox();
            this.btn_GetFile = new System.Windows.Forms.Button();
            this.lbx_FileName = new System.Windows.Forms.ListBox();
            this.btn_CvtXls = new System.Windows.Forms.Button();
            this.dgv_dt1 = new System.Windows.Forms.DataGridView();
            this.lbl_Status = new System.Windows.Forms.Label();
            this.lbl_RecordInfo = new System.Windows.Forms.Label();
            this.ckx_AllPathFolder = new System.Windows.Forms.CheckBox();
            this.ckx_SinglePathFolder = new System.Windows.Forms.CheckBox();
            this.btn_ClrTable = new System.Windows.Forms.Button();
            this.lbl_CvtXls = new System.Windows.Forms.Label();
            this.lbl_Id = new System.Windows.Forms.Label();
            this.lbl_FileName = new System.Windows.Forms.Label();
            this.lbl_Size = new System.Windows.Forms.Label();
            this.lbl_date = new System.Windows.Forms.Label();
            this.lbl_Version = new System.Windows.Forms.Label();
            this.gbx_File = new System.Windows.Forms.GroupBox();
            this.tbx_SaveDate = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tbx_dirpath = new System.Windows.Forms.TextBox();
            this.lbl_dirpath = new System.Windows.Forms.Label();
            this.tbx_Version = new System.Windows.Forms.TextBox();
            this.tbx_Date = new System.Windows.Forms.TextBox();
            this.tbx_Size = new System.Windows.Forms.TextBox();
            this.tbx_FileName = new System.Windows.Forms.TextBox();
            this.tbx_Id = new System.Windows.Forms.TextBox();
            this.btn_InsertDB = new System.Windows.Forms.Button();
            this.lbl_rdstatus = new System.Windows.Forms.Label();
            this.btn_linkDB = new System.Windows.Forms.Button();
            this.btn_delRow = new System.Windows.Forms.Button();
            this.tbx_SqlCmd = new System.Windows.Forms.TextBox();
            this.lbl_Sqlcmd = new System.Windows.Forms.Label();
            this.rbn_Localside = new System.Windows.Forms.RadioButton();
            this.rbn_Webside = new System.Windows.Forms.RadioButton();
            this.gbx_ReadDelBox = new System.Windows.Forms.GroupBox();
            this.lbl_DataLogall = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.lbl_DataTable1All = new System.Windows.Forms.Label();
            this.btn_DeletLog = new System.Windows.Forms.Button();
            this.btn_readLog = new System.Windows.Forms.Button();
            this.lbl_LogTest = new System.Windows.Forms.Label();
            this.lbx_AddRowInfo = new System.Windows.Forms.ListBox();
            this.ckx_selectAll = new System.Windows.Forms.CheckBox();
            this.lbl_RowSelectCount = new System.Windows.Forms.Label();
            this.lbl_ROwNum = new System.Windows.Forms.Label();
            this.lbl_ProjectTitle = new System.Windows.Forms.Label();
            this.pgb_CentValue = new System.Windows.Forms.ProgressBar();
            this.lbl_CentHundred = new System.Windows.Forms.Label();
            this.lbl_ColorResult = new System.Windows.Forms.Label();
            this.lbl_listName = new System.Windows.Forms.Label();
            this.tbx_Password = new System.Windows.Forms.TextBox();
            this.lbl_Password = new System.Windows.Forms.Label();
            this.btn_Confirm = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_dt1)).BeginInit();
            this.gbx_File.SuspendLayout();
            this.gbx_ReadDelBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // lbl_PathFolder
            // 
            this.lbl_PathFolder.AutoSize = true;
            this.lbl_PathFolder.Font = new System.Drawing.Font("標楷體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_PathFolder.Location = new System.Drawing.Point(22, 98);
            this.lbl_PathFolder.Name = "lbl_PathFolder";
            this.lbl_PathFolder.Size = new System.Drawing.Size(125, 21);
            this.lbl_PathFolder.TabIndex = 0;
            this.lbl_PathFolder.Text = "路徑資料夾";
            // 
            // btn_OpenFolder
            // 
            this.btn_OpenFolder.Font = new System.Drawing.Font("標楷體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btn_OpenFolder.Location = new System.Drawing.Point(22, 45);
            this.btn_OpenFolder.Name = "btn_OpenFolder";
            this.btn_OpenFolder.Size = new System.Drawing.Size(69, 50);
            this.btn_OpenFolder.TabIndex = 1;
            this.btn_OpenFolder.Text = "Open";
            this.btn_OpenFolder.UseVisualStyleBackColor = true;
            this.btn_OpenFolder.Click += new System.EventHandler(this.btn_OpenFolder_Click);
            // 
            // tbx_FilePath
            // 
            this.tbx_FilePath.Location = new System.Drawing.Point(97, 64);
            this.tbx_FilePath.Multiline = true;
            this.tbx_FilePath.Name = "tbx_FilePath";
            this.tbx_FilePath.ReadOnly = true;
            this.tbx_FilePath.Size = new System.Drawing.Size(305, 22);
            this.tbx_FilePath.TabIndex = 2;
            // 
            // btn_GetFile
            // 
            this.btn_GetFile.Cursor = System.Windows.Forms.Cursors.Default;
            this.btn_GetFile.Font = new System.Drawing.Font("標楷體", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btn_GetFile.Location = new System.Drawing.Point(26, 133);
            this.btn_GetFile.Name = "btn_GetFile";
            this.btn_GetFile.Size = new System.Drawing.Size(125, 47);
            this.btn_GetFile.TabIndex = 3;
            this.btn_GetFile.Text = "GetFile";
            this.btn_GetFile.UseVisualStyleBackColor = true;
            this.btn_GetFile.Click += new System.EventHandler(this.btn_GetFile_Click);
            // 
            // lbx_FileName
            // 
            this.lbx_FileName.Font = new System.Drawing.Font("標楷體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbx_FileName.FormattingEnabled = true;
            this.lbx_FileName.ItemHeight = 12;
            this.lbx_FileName.Location = new System.Drawing.Point(1204, 670);
            this.lbx_FileName.Name = "lbx_FileName";
            this.lbx_FileName.Size = new System.Drawing.Size(65, 76);
            this.lbx_FileName.TabIndex = 4;
            this.lbx_FileName.Visible = false;
            this.lbx_FileName.SelectedIndexChanged += new System.EventHandler(this.lbx_FileName_SelectedIndexChanged);
            // 
            // btn_CvtXls
            // 
            this.btn_CvtXls.Font = new System.Drawing.Font("標楷體", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btn_CvtXls.Location = new System.Drawing.Point(1059, 696);
            this.btn_CvtXls.Name = "btn_CvtXls";
            this.btn_CvtXls.Size = new System.Drawing.Size(140, 48);
            this.btn_CvtXls.TabIndex = 6;
            this.btn_CvtXls.Text = "Convert Excel";
            this.btn_CvtXls.UseVisualStyleBackColor = true;
            this.btn_CvtXls.Click += new System.EventHandler(this.btn_CvtXls_Click);
            // 
            // dgv_dt1
            // 
            this.dgv_dt1.AllowUserToAddRows = false;
            this.dgv_dt1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_dt1.Location = new System.Drawing.Point(26, 218);
            this.dgv_dt1.Name = "dgv_dt1";
            this.dgv_dt1.ReadOnly = true;
            this.dgv_dt1.RowTemplate.Height = 24;
            this.dgv_dt1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgv_dt1.Size = new System.Drawing.Size(422, 509);
            this.dgv_dt1.TabIndex = 7;
            this.dgv_dt1.SelectionChanged += new System.EventHandler(this.dgv_dt1_SelectionChanged);
            // 
            // lbl_Status
            // 
            this.lbl_Status.AutoSize = true;
            this.lbl_Status.Font = new System.Drawing.Font("標楷體", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_Status.Location = new System.Drawing.Point(52, 188);
            this.lbl_Status.Name = "lbl_Status";
            this.lbl_Status.Size = new System.Drawing.Size(60, 24);
            this.lbl_Status.TabIndex = 8;
            this.lbl_Status.Text = "狀態";
            // 
            // lbl_RecordInfo
            // 
            this.lbl_RecordInfo.AutoSize = true;
            this.lbl_RecordInfo.Font = new System.Drawing.Font("標楷體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_RecordInfo.Location = new System.Drawing.Point(459, 206);
            this.lbl_RecordInfo.Name = "lbl_RecordInfo";
            this.lbl_RecordInfo.Size = new System.Drawing.Size(102, 16);
            this.lbl_RecordInfo.TabIndex = 9;
            this.lbl_RecordInfo.Text = "紀錄檔資訊:";
            // 
            // ckx_AllPathFolder
            // 
            this.ckx_AllPathFolder.AutoSize = true;
            this.ckx_AllPathFolder.Font = new System.Drawing.Font("標楷體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.ckx_AllPathFolder.Location = new System.Drawing.Point(408, 52);
            this.ckx_AllPathFolder.Name = "ckx_AllPathFolder";
            this.ckx_AllPathFolder.Size = new System.Drawing.Size(167, 25);
            this.ckx_AllPathFolder.TabIndex = 11;
            this.ckx_AllPathFolder.Text = "全路徑資料夾";
            this.ckx_AllPathFolder.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.ckx_AllPathFolder.UseVisualStyleBackColor = true;
            this.ckx_AllPathFolder.CheckedChanged += new System.EventHandler(this.ckx_AllPathFolder_CheckedChanged);
            // 
            // ckx_SinglePathFolder
            // 
            this.ckx_SinglePathFolder.AutoSize = true;
            this.ckx_SinglePathFolder.Font = new System.Drawing.Font("標楷體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.ckx_SinglePathFolder.Location = new System.Drawing.Point(408, 84);
            this.ckx_SinglePathFolder.Name = "ckx_SinglePathFolder";
            this.ckx_SinglePathFolder.Size = new System.Drawing.Size(144, 25);
            this.ckx_SinglePathFolder.TabIndex = 12;
            this.ckx_SinglePathFolder.Text = "單一資料夾";
            this.ckx_SinglePathFolder.UseVisualStyleBackColor = true;
            this.ckx_SinglePathFolder.CheckedChanged += new System.EventHandler(this.ckx_SinglePathFolder_CheckedChanged);
            // 
            // btn_ClrTable
            // 
            this.btn_ClrTable.Font = new System.Drawing.Font("標楷體", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btn_ClrTable.Location = new System.Drawing.Point(26, 738);
            this.btn_ClrTable.Name = "btn_ClrTable";
            this.btn_ClrTable.Size = new System.Drawing.Size(86, 49);
            this.btn_ClrTable.TabIndex = 14;
            this.btn_ClrTable.Text = "清除總表單";
            this.btn_ClrTable.UseVisualStyleBackColor = true;
            this.btn_ClrTable.Click += new System.EventHandler(this.btn_ClrTable_Click);
            // 
            // lbl_CvtXls
            // 
            this.lbl_CvtXls.AutoSize = true;
            this.lbl_CvtXls.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.lbl_CvtXls.Font = new System.Drawing.Font("標楷體", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_CvtXls.ForeColor = System.Drawing.Color.Green;
            this.lbl_CvtXls.Location = new System.Drawing.Point(1073, 672);
            this.lbl_CvtXls.Name = "lbl_CvtXls";
            this.lbl_CvtXls.Size = new System.Drawing.Size(106, 19);
            this.lbl_CvtXls.TabIndex = 16;
            this.lbl_CvtXls.Text = "轉至Excel";
            // 
            // lbl_Id
            // 
            this.lbl_Id.AutoSize = true;
            this.lbl_Id.Font = new System.Drawing.Font("標楷體", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_Id.Location = new System.Drawing.Point(3, 25);
            this.lbl_Id.Name = "lbl_Id";
            this.lbl_Id.Size = new System.Drawing.Size(51, 19);
            this.lbl_Id.TabIndex = 23;
            this.lbl_Id.Text = "序列";
            // 
            // lbl_FileName
            // 
            this.lbl_FileName.AutoSize = true;
            this.lbl_FileName.Font = new System.Drawing.Font("標楷體", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_FileName.Location = new System.Drawing.Point(3, 52);
            this.lbl_FileName.Name = "lbl_FileName";
            this.lbl_FileName.Size = new System.Drawing.Size(51, 19);
            this.lbl_FileName.TabIndex = 24;
            this.lbl_FileName.Text = "檔名";
            // 
            // lbl_Size
            // 
            this.lbl_Size.AutoSize = true;
            this.lbl_Size.Font = new System.Drawing.Font("標楷體", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_Size.Location = new System.Drawing.Point(3, 82);
            this.lbl_Size.Name = "lbl_Size";
            this.lbl_Size.Size = new System.Drawing.Size(95, 19);
            this.lbl_Size.TabIndex = 25;
            this.lbl_Size.Text = "大小(KB)";
            this.lbl_Size.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lbl_date
            // 
            this.lbl_date.AutoSize = true;
            this.lbl_date.Font = new System.Drawing.Font("標楷體", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_date.Location = new System.Drawing.Point(5, 114);
            this.lbl_date.Name = "lbl_date";
            this.lbl_date.Size = new System.Drawing.Size(93, 19);
            this.lbl_date.TabIndex = 26;
            this.lbl_date.Text = "檔案日期";
            // 
            // lbl_Version
            // 
            this.lbl_Version.AutoSize = true;
            this.lbl_Version.Font = new System.Drawing.Font("標楷體", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_Version.Location = new System.Drawing.Point(6, 147);
            this.lbl_Version.Name = "lbl_Version";
            this.lbl_Version.Size = new System.Drawing.Size(42, 19);
            this.lbl_Version.TabIndex = 27;
            this.lbl_Version.Text = "VER";
            // 
            // gbx_File
            // 
            this.gbx_File.Controls.Add(this.tbx_SaveDate);
            this.gbx_File.Controls.Add(this.label1);
            this.gbx_File.Controls.Add(this.tbx_dirpath);
            this.gbx_File.Controls.Add(this.lbl_dirpath);
            this.gbx_File.Controls.Add(this.tbx_Version);
            this.gbx_File.Controls.Add(this.tbx_Date);
            this.gbx_File.Controls.Add(this.tbx_Size);
            this.gbx_File.Controls.Add(this.tbx_FileName);
            this.gbx_File.Controls.Add(this.tbx_Id);
            this.gbx_File.Controls.Add(this.lbl_Id);
            this.gbx_File.Controls.Add(this.lbl_Version);
            this.gbx_File.Controls.Add(this.lbl_FileName);
            this.gbx_File.Controls.Add(this.lbl_date);
            this.gbx_File.Controls.Add(this.lbl_Size);
            this.gbx_File.Location = new System.Drawing.Point(459, 497);
            this.gbx_File.Name = "gbx_File";
            this.gbx_File.Size = new System.Drawing.Size(516, 264);
            this.gbx_File.TabIndex = 28;
            this.gbx_File.TabStop = false;
            this.gbx_File.Text = "gbx_FileInfo(單)";
            // 
            // tbx_SaveDate
            // 
            this.tbx_SaveDate.Location = new System.Drawing.Point(106, 230);
            this.tbx_SaveDate.Multiline = true;
            this.tbx_SaveDate.Name = "tbx_SaveDate";
            this.tbx_SaveDate.ReadOnly = true;
            this.tbx_SaveDate.Size = new System.Drawing.Size(398, 27);
            this.tbx_SaveDate.TabIndex = 35;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("標楷體", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label1.Location = new System.Drawing.Point(3, 230);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(93, 19);
            this.label1.TabIndex = 34;
            this.label1.Text = "存檔日期";
            // 
            // tbx_dirpath
            // 
            this.tbx_dirpath.Location = new System.Drawing.Point(105, 186);
            this.tbx_dirpath.Multiline = true;
            this.tbx_dirpath.Name = "tbx_dirpath";
            this.tbx_dirpath.ReadOnly = true;
            this.tbx_dirpath.Size = new System.Drawing.Size(398, 27);
            this.tbx_dirpath.TabIndex = 33;
            // 
            // lbl_dirpath
            // 
            this.lbl_dirpath.AutoSize = true;
            this.lbl_dirpath.Font = new System.Drawing.Font("標楷體", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_dirpath.Location = new System.Drawing.Point(6, 184);
            this.lbl_dirpath.Name = "lbl_dirpath";
            this.lbl_dirpath.Size = new System.Drawing.Size(93, 19);
            this.lbl_dirpath.TabIndex = 32;
            this.lbl_dirpath.Text = "儲存路徑";
            // 
            // tbx_Version
            // 
            this.tbx_Version.Location = new System.Drawing.Point(106, 149);
            this.tbx_Version.Multiline = true;
            this.tbx_Version.Name = "tbx_Version";
            this.tbx_Version.ReadOnly = true;
            this.tbx_Version.Size = new System.Drawing.Size(399, 24);
            this.tbx_Version.TabIndex = 30;
            // 
            // tbx_Date
            // 
            this.tbx_Date.Location = new System.Drawing.Point(105, 111);
            this.tbx_Date.Multiline = true;
            this.tbx_Date.Name = "tbx_Date";
            this.tbx_Date.ReadOnly = true;
            this.tbx_Date.Size = new System.Drawing.Size(399, 24);
            this.tbx_Date.TabIndex = 30;
            // 
            // tbx_Size
            // 
            this.tbx_Size.Location = new System.Drawing.Point(104, 81);
            this.tbx_Size.Name = "tbx_Size";
            this.tbx_Size.ReadOnly = true;
            this.tbx_Size.Size = new System.Drawing.Size(398, 22);
            this.tbx_Size.TabIndex = 29;
            // 
            // tbx_FileName
            // 
            this.tbx_FileName.Location = new System.Drawing.Point(104, 51);
            this.tbx_FileName.Name = "tbx_FileName";
            this.tbx_FileName.ReadOnly = true;
            this.tbx_FileName.Size = new System.Drawing.Size(399, 22);
            this.tbx_FileName.TabIndex = 29;
            // 
            // tbx_Id
            // 
            this.tbx_Id.Location = new System.Drawing.Point(104, 23);
            this.tbx_Id.Name = "tbx_Id";
            this.tbx_Id.ReadOnly = true;
            this.tbx_Id.Size = new System.Drawing.Size(399, 22);
            this.tbx_Id.TabIndex = 28;
            // 
            // btn_InsertDB
            // 
            this.btn_InsertDB.Font = new System.Drawing.Font("標楷體", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btn_InsertDB.Location = new System.Drawing.Point(265, 133);
            this.btn_InsertDB.Name = "btn_InsertDB";
            this.btn_InsertDB.Size = new System.Drawing.Size(165, 50);
            this.btn_InsertDB.TabIndex = 32;
            this.btn_InsertDB.Text = "寫入資料庫";
            this.btn_InsertDB.UseVisualStyleBackColor = true;
            this.btn_InsertDB.Click += new System.EventHandler(this.btn_InsertDB_Click);
            // 
            // lbl_rdstatus
            // 
            this.lbl_rdstatus.AutoSize = true;
            this.lbl_rdstatus.Font = new System.Drawing.Font("新細明體", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_rdstatus.Location = new System.Drawing.Point(16, 98);
            this.lbl_rdstatus.Name = "lbl_rdstatus";
            this.lbl_rdstatus.Size = new System.Drawing.Size(74, 15);
            this.lbl_rdstatus.TabIndex = 31;
            this.lbl_rdstatus.Text = "狀態.......";
            this.lbl_rdstatus.Visible = false;
            // 
            // btn_linkDB
            // 
            this.btn_linkDB.Font = new System.Drawing.Font("標楷體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btn_linkDB.Location = new System.Drawing.Point(52, 59);
            this.btn_linkDB.Name = "btn_linkDB";
            this.btn_linkDB.Size = new System.Drawing.Size(146, 36);
            this.btn_linkDB.TabIndex = 30;
            this.btn_linkDB.Text = "讀取DataTable1總表";
            this.btn_linkDB.UseVisualStyleBackColor = true;
            this.btn_linkDB.Visible = false;
            this.btn_linkDB.Click += new System.EventHandler(this.btn_linkDB_Click);
            // 
            // btn_delRow
            // 
            this.btn_delRow.Font = new System.Drawing.Font("標楷體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btn_delRow.Location = new System.Drawing.Point(58, 258);
            this.btn_delRow.Name = "btn_delRow";
            this.btn_delRow.Size = new System.Drawing.Size(142, 34);
            this.btn_delRow.TabIndex = 32;
            this.btn_delRow.Text = "刪除DataTable1";
            this.btn_delRow.UseVisualStyleBackColor = true;
            this.btn_delRow.Visible = false;
            this.btn_delRow.Click += new System.EventHandler(this.btn_delRow_Click);
            // 
            // tbx_SqlCmd
            // 
            this.tbx_SqlCmd.Location = new System.Drawing.Point(462, 157);
            this.tbx_SqlCmd.Multiline = true;
            this.tbx_SqlCmd.Name = "tbx_SqlCmd";
            this.tbx_SqlCmd.ReadOnly = true;
            this.tbx_SqlCmd.Size = new System.Drawing.Size(513, 27);
            this.tbx_SqlCmd.TabIndex = 33;
            // 
            // lbl_Sqlcmd
            // 
            this.lbl_Sqlcmd.AutoSize = true;
            this.lbl_Sqlcmd.Font = new System.Drawing.Font("標楷體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_Sqlcmd.Location = new System.Drawing.Point(850, 133);
            this.lbl_Sqlcmd.Name = "lbl_Sqlcmd";
            this.lbl_Sqlcmd.Size = new System.Drawing.Size(125, 21);
            this.lbl_Sqlcmd.TabIndex = 36;
            this.lbl_Sqlcmd.Text = "資料庫指令";
            // 
            // rbn_Localside
            // 
            this.rbn_Localside.AutoSize = true;
            this.rbn_Localside.Font = new System.Drawing.Font("標楷體", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.rbn_Localside.Location = new System.Drawing.Point(147, 10);
            this.rbn_Localside.Name = "rbn_Localside";
            this.rbn_Localside.Size = new System.Drawing.Size(132, 36);
            this.rbn_Localside.TabIndex = 37;
            this.rbn_Localside.TabStop = true;
            this.rbn_Localside.Text = "單機端";
            this.rbn_Localside.UseVisualStyleBackColor = true;
            // 
            // rbn_Webside
            // 
            this.rbn_Webside.AutoSize = true;
            this.rbn_Webside.Font = new System.Drawing.Font("標楷體", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.rbn_Webside.Location = new System.Drawing.Point(285, 10);
            this.rbn_Webside.Name = "rbn_Webside";
            this.rbn_Webside.Size = new System.Drawing.Size(132, 36);
            this.rbn_Webside.TabIndex = 38;
            this.rbn_Webside.TabStop = true;
            this.rbn_Webside.Text = "網路端";
            this.rbn_Webside.UseVisualStyleBackColor = true;
            // 
            // gbx_ReadDelBox
            // 
            this.gbx_ReadDelBox.Controls.Add(this.lbl_DataLogall);
            this.gbx_ReadDelBox.Controls.Add(this.label4);
            this.gbx_ReadDelBox.Controls.Add(this.lbl_DataTable1All);
            this.gbx_ReadDelBox.Controls.Add(this.btn_DeletLog);
            this.gbx_ReadDelBox.Controls.Add(this.btn_readLog);
            this.gbx_ReadDelBox.Controls.Add(this.btn_linkDB);
            this.gbx_ReadDelBox.Controls.Add(this.lbl_rdstatus);
            this.gbx_ReadDelBox.Controls.Add(this.btn_delRow);
            this.gbx_ReadDelBox.Controls.Add(this.lbl_LogTest);
            this.gbx_ReadDelBox.Font = new System.Drawing.Font("標楷體", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.gbx_ReadDelBox.Location = new System.Drawing.Point(993, 93);
            this.gbx_ReadDelBox.Name = "gbx_ReadDelBox";
            this.gbx_ReadDelBox.Size = new System.Drawing.Size(271, 546);
            this.gbx_ReadDelBox.TabIndex = 39;
            this.gbx_ReadDelBox.TabStop = false;
            this.gbx_ReadDelBox.Text = "Read/Delete";
            // 
            // lbl_DataLogall
            // 
            this.lbl_DataLogall.AutoSize = true;
            this.lbl_DataLogall.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.lbl_DataLogall.Font = new System.Drawing.Font("標楷體", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_DataLogall.ForeColor = System.Drawing.Color.Red;
            this.lbl_DataLogall.Location = new System.Drawing.Point(98, 312);
            this.lbl_DataLogall.Name = "lbl_DataLogall";
            this.lbl_DataLogall.Size = new System.Drawing.Size(57, 27);
            this.lbl_DataLogall.TabIndex = 56;
            this.lbl_DataLogall.Text = "Log";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("標楷體", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label4.ForeColor = System.Drawing.Color.OrangeRed;
            this.label4.Location = new System.Drawing.Point(-5, 291);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(327, 27);
            this.label4.TabIndex = 55;
            this.label4.Text = "---------------------";
            // 
            // lbl_DataTable1All
            // 
            this.lbl_DataTable1All.AutoSize = true;
            this.lbl_DataTable1All.BackColor = System.Drawing.Color.LemonChiffon;
            this.lbl_DataTable1All.Font = new System.Drawing.Font("標楷體", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_DataTable1All.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.lbl_DataTable1All.Location = new System.Drawing.Point(73, 30);
            this.lbl_DataTable1All.Name = "lbl_DataTable1All";
            this.lbl_DataTable1All.Size = new System.Drawing.Size(102, 27);
            this.lbl_DataTable1All.TabIndex = 54;
            this.lbl_DataTable1All.Text = "Table1";
            this.lbl_DataTable1All.Visible = false;
            // 
            // btn_DeletLog
            // 
            this.btn_DeletLog.Font = new System.Drawing.Font("標楷體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btn_DeletLog.Location = new System.Drawing.Point(66, 507);
            this.btn_DeletLog.Name = "btn_DeletLog";
            this.btn_DeletLog.Size = new System.Drawing.Size(142, 33);
            this.btn_DeletLog.TabIndex = 53;
            this.btn_DeletLog.Text = "刪除TableLog";
            this.btn_DeletLog.UseVisualStyleBackColor = true;
            this.btn_DeletLog.Visible = false;
            this.btn_DeletLog.Click += new System.EventHandler(this.btn_DeletLog_Click);
            // 
            // btn_readLog
            // 
            this.btn_readLog.Font = new System.Drawing.Font("標楷體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btn_readLog.Location = new System.Drawing.Point(63, 342);
            this.btn_readLog.Name = "btn_readLog";
            this.btn_readLog.Size = new System.Drawing.Size(139, 31);
            this.btn_readLog.TabIndex = 47;
            this.btn_readLog.Text = "讀取TableLog\r\n";
            this.btn_readLog.UseVisualStyleBackColor = true;
            this.btn_readLog.Click += new System.EventHandler(this.btn_readLog_Click);
            // 
            // lbl_LogTest
            // 
            this.lbl_LogTest.AutoSize = true;
            this.lbl_LogTest.Font = new System.Drawing.Font("標楷體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_LogTest.Location = new System.Drawing.Point(15, 380);
            this.lbl_LogTest.Name = "lbl_LogTest";
            this.lbl_LogTest.Size = new System.Drawing.Size(69, 16);
            this.lbl_LogTest.TabIndex = 48;
            this.lbl_LogTest.Text = "log等待";
            // 
            // lbx_AddRowInfo
            // 
            this.lbx_AddRowInfo.Font = new System.Drawing.Font("標楷體", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbx_AddRowInfo.FormattingEnabled = true;
            this.lbx_AddRowInfo.ItemHeight = 15;
            this.lbx_AddRowInfo.Location = new System.Drawing.Point(462, 225);
            this.lbx_AddRowInfo.Name = "lbx_AddRowInfo";
            this.lbx_AddRowInfo.Size = new System.Drawing.Size(513, 259);
            this.lbx_AddRowInfo.TabIndex = 41;
            this.lbx_AddRowInfo.SelectedIndexChanged += new System.EventHandler(this.lbx_AddRowInfo_SelectedIndexChanged);
            // 
            // ckx_selectAll
            // 
            this.ckx_selectAll.AutoSize = true;
            this.ckx_selectAll.Font = new System.Drawing.Font("新細明體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.ckx_selectAll.Location = new System.Drawing.Point(262, 188);
            this.ckx_selectAll.Name = "ckx_selectAll";
            this.ckx_selectAll.Size = new System.Drawing.Size(73, 25);
            this.ckx_selectAll.TabIndex = 42;
            this.ckx_selectAll.Text = "全選";
            this.ckx_selectAll.UseVisualStyleBackColor = true;
            this.ckx_selectAll.CheckedChanged += new System.EventHandler(this.ckx_selectAll_CheckedChanged);
            // 
            // lbl_RowSelectCount
            // 
            this.lbl_RowSelectCount.AutoSize = true;
            this.lbl_RowSelectCount.Font = new System.Drawing.Font("標楷體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_RowSelectCount.Location = new System.Drawing.Point(332, 193);
            this.lbl_RowSelectCount.Name = "lbl_RowSelectCount";
            this.lbl_RowSelectCount.Size = new System.Drawing.Size(85, 16);
            this.lbl_RowSelectCount.TabIndex = 43;
            this.lbl_RowSelectCount.Text = "筆數選取:";
            // 
            // lbl_ROwNum
            // 
            this.lbl_ROwNum.AutoSize = true;
            this.lbl_ROwNum.Location = new System.Drawing.Point(419, 193);
            this.lbl_ROwNum.Name = "lbl_ROwNum";
            this.lbl_ROwNum.Size = new System.Drawing.Size(11, 12);
            this.lbl_ROwNum.TabIndex = 44;
            this.lbl_ROwNum.Text = "0";
            // 
            // lbl_ProjectTitle
            // 
            this.lbl_ProjectTitle.AutoSize = true;
            this.lbl_ProjectTitle.Font = new System.Drawing.Font("標楷體", 48F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_ProjectTitle.ForeColor = System.Drawing.Color.Navy;
            this.lbl_ProjectTitle.Location = new System.Drawing.Point(669, 14);
            this.lbl_ProjectTitle.Name = "lbl_ProjectTitle";
            this.lbl_ProjectTitle.Size = new System.Drawing.Size(418, 64);
            this.lbl_ProjectTitle.TabIndex = 49;
            this.lbl_ProjectTitle.Text = "資料比對工具";
            this.lbl_ProjectTitle.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.lbl_ProjectTitle.DoubleClick += new System.EventHandler(this.lbl_ProjectTitle_DoubleClick);
            // 
            // pgb_CentValue
            // 
            this.pgb_CentValue.Location = new System.Drawing.Point(118, 738);
            this.pgb_CentValue.Name = "pgb_CentValue";
            this.pgb_CentValue.Size = new System.Drawing.Size(330, 23);
            this.pgb_CentValue.TabIndex = 50;
            // 
            // lbl_CentHundred
            // 
            this.lbl_CentHundred.AutoSize = true;
            this.lbl_CentHundred.Location = new System.Drawing.Point(145, 775);
            this.lbl_CentHundred.Name = "lbl_CentHundred";
            this.lbl_CentHundred.Size = new System.Drawing.Size(8, 12);
            this.lbl_CentHundred.TabIndex = 51;
            this.lbl_CentHundred.Text = " ";
            // 
            // lbl_ColorResult
            // 
            this.lbl_ColorResult.AutoSize = true;
            this.lbl_ColorResult.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_ColorResult.Location = new System.Drawing.Point(225, 764);
            this.lbl_ColorResult.Name = "lbl_ColorResult";
            this.lbl_ColorResult.Size = new System.Drawing.Size(57, 16);
            this.lbl_ColorResult.TabIndex = 52;
            this.lbl_ColorResult.Text = "等待...";
            // 
            // lbl_listName
            // 
            this.lbl_listName.AutoSize = true;
            this.lbl_listName.Font = new System.Drawing.Font("新細明體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_listName.Location = new System.Drawing.Point(566, 209);
            this.lbl_listName.Name = "lbl_listName";
            this.lbl_listName.Size = new System.Drawing.Size(52, 12);
            this.lbl_listName.TabIndex = 55;
            this.lbl_listName.Text = "List狀態";
            // 
            // tbx_Password
            // 
            this.tbx_Password.Location = new System.Drawing.Point(1163, 45);
            this.tbx_Password.Name = "tbx_Password";
            this.tbx_Password.PasswordChar = '*';
            this.tbx_Password.Size = new System.Drawing.Size(40, 22);
            this.tbx_Password.TabIndex = 56;
            this.tbx_Password.Visible = false;
            // 
            // lbl_Password
            // 
            this.lbl_Password.AutoSize = true;
            this.lbl_Password.Font = new System.Drawing.Font("標楷體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_Password.ForeColor = System.Drawing.Color.Red;
            this.lbl_Password.Location = new System.Drawing.Point(1078, 21);
            this.lbl_Password.Name = "lbl_Password";
            this.lbl_Password.Size = new System.Drawing.Size(207, 21);
            this.lbl_Password.TabIndex = 57;
            this.lbl_Password.Text = "權限登入密碼(5碼)";
            this.lbl_Password.Visible = false;
            // 
            // btn_Confirm
            // 
            this.btn_Confirm.Location = new System.Drawing.Point(1144, 69);
            this.btn_Confirm.Name = "btn_Confirm";
            this.btn_Confirm.Size = new System.Drawing.Size(75, 23);
            this.btn_Confirm.TabIndex = 58;
            this.btn_Confirm.Text = "登入";
            this.btn_Confirm.UseVisualStyleBackColor = true;
            this.btn_Confirm.Visible = false;
            this.btn_Confirm.Click += new System.EventHandler(this.btn_Confirm_Click);
            // 
            // FileCatch
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.ClientSize = new System.Drawing.Size(1291, 796);
            this.Controls.Add(this.btn_Confirm);
            this.Controls.Add(this.lbl_Password);
            this.Controls.Add(this.tbx_Password);
            this.Controls.Add(this.lbl_listName);
            this.Controls.Add(this.lbl_ColorResult);
            this.Controls.Add(this.lbl_CentHundred);
            this.Controls.Add(this.pgb_CentValue);
            this.Controls.Add(this.lbl_ProjectTitle);
            this.Controls.Add(this.lbl_ROwNum);
            this.Controls.Add(this.lbl_RowSelectCount);
            this.Controls.Add(this.ckx_selectAll);
            this.Controls.Add(this.lbx_AddRowInfo);
            this.Controls.Add(this.gbx_ReadDelBox);
            this.Controls.Add(this.btn_InsertDB);
            this.Controls.Add(this.rbn_Webside);
            this.Controls.Add(this.rbn_Localside);
            this.Controls.Add(this.btn_ClrTable);
            this.Controls.Add(this.lbl_Sqlcmd);
            this.Controls.Add(this.tbx_SqlCmd);
            this.Controls.Add(this.gbx_File);
            this.Controls.Add(this.lbl_CvtXls);
            this.Controls.Add(this.ckx_SinglePathFolder);
            this.Controls.Add(this.ckx_AllPathFolder);
            this.Controls.Add(this.lbl_RecordInfo);
            this.Controls.Add(this.lbl_Status);
            this.Controls.Add(this.dgv_dt1);
            this.Controls.Add(this.btn_CvtXls);
            this.Controls.Add(this.lbx_FileName);
            this.Controls.Add(this.btn_GetFile);
            this.Controls.Add(this.tbx_FilePath);
            this.Controls.Add(this.btn_OpenFolder);
            this.Controls.Add(this.lbl_PathFolder);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FileCatch";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FileSearchContrast";
            this.TransparencyKey = System.Drawing.Color.WhiteSmoke;
            this.Load += new System.EventHandler(this.FileCatch_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_dt1)).EndInit();
            this.gbx_File.ResumeLayout(false);
            this.gbx_File.PerformLayout();
            this.gbx_ReadDelBox.ResumeLayout(false);
            this.gbx_ReadDelBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbl_PathFolder;
        private System.Windows.Forms.Button btn_OpenFolder;
        private System.Windows.Forms.FolderBrowserDialog fbd_OpenPathFolder;
        private System.Windows.Forms.TextBox tbx_FilePath;
        private System.Windows.Forms.Button btn_GetFile;
        private System.Windows.Forms.ListBox lbx_FileName;
        private System.Windows.Forms.Button btn_CvtXls;
        private System.Windows.Forms.DataGridView dgv_dt1;
        private System.Windows.Forms.Label lbl_Status;
        private System.Windows.Forms.Label lbl_RecordInfo;
        private System.Windows.Forms.CheckBox ckx_AllPathFolder;
        private System.Windows.Forms.CheckBox ckx_SinglePathFolder;
        private System.Windows.Forms.Button btn_ClrTable;
        private System.Windows.Forms.Label lbl_CvtXls;
        private System.Windows.Forms.Label lbl_Id;
        private System.Windows.Forms.Label lbl_FileName;
        private System.Windows.Forms.Label lbl_Size;
        private System.Windows.Forms.Label lbl_date;
        private System.Windows.Forms.Label lbl_Version;
        private System.Windows.Forms.GroupBox gbx_File;
        private System.Windows.Forms.TextBox tbx_Id;
        private System.Windows.Forms.TextBox tbx_Version;
        private System.Windows.Forms.TextBox tbx_Date;
        private System.Windows.Forms.TextBox tbx_Size;
        private System.Windows.Forms.TextBox tbx_FileName;
        private System.Windows.Forms.Button btn_linkDB;
        private System.Windows.Forms.Label lbl_rdstatus;
        private System.Windows.Forms.TextBox tbx_dirpath;
        private System.Windows.Forms.Label lbl_dirpath;
        private System.Windows.Forms.Button btn_InsertDB;
        private System.Windows.Forms.Button btn_delRow;
        private System.Windows.Forms.TextBox tbx_SqlCmd;
        private System.Windows.Forms.Label lbl_Sqlcmd;
        private System.Windows.Forms.RadioButton rbn_Localside;
        private System.Windows.Forms.RadioButton rbn_Webside;
        private System.Windows.Forms.GroupBox gbx_ReadDelBox;
        private System.Windows.Forms.ListBox lbx_AddRowInfo;
        private System.Windows.Forms.CheckBox ckx_selectAll;
        private System.Windows.Forms.Label lbl_RowSelectCount;
        private System.Windows.Forms.Label lbl_ROwNum;
        private System.Windows.Forms.Button btn_readLog;
        private System.Windows.Forms.Label lbl_LogTest;
        private System.Windows.Forms.TextBox tbx_SaveDate;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lbl_ProjectTitle;
        private System.Windows.Forms.ProgressBar pgb_CentValue;
        private System.Windows.Forms.Label lbl_CentHundred;
        private System.Windows.Forms.Label lbl_ColorResult;
        private System.Windows.Forms.Button btn_DeletLog;
        private System.Windows.Forms.Label lbl_listName;
        private System.Windows.Forms.Label lbl_DataLogall;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lbl_DataTable1All;
        private System.Windows.Forms.TextBox tbx_Password;
        private System.Windows.Forms.Label lbl_Password;
        private System.Windows.Forms.Button btn_Confirm;
    }
}

