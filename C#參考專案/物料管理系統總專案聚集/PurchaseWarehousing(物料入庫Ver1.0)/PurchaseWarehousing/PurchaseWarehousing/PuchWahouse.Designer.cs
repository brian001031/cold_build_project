namespace PurchaseWarehousing
{
    partial class PuchWarehousing
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PuchWarehousing));
            this.dgv_SqlTableData = new System.Windows.Forms.DataGridView();
            this.lbl_SqlStatus = new System.Windows.Forms.Label();
            this.rbn_AutoScan = new System.Windows.Forms.RadioButton();
            this.rbn_ManualScan = new System.Windows.Forms.RadioButton();
            this.gbx_DataTable = new System.Windows.Forms.GroupBox();
            this.lbl_WareScan = new System.Windows.Forms.Label();
            this.btn_WareSingConfirm = new System.Windows.Forms.Button();
            this.tbx_WareSingScan = new System.Windows.Forms.TextBox();
            this.lbl_Flag1 = new System.Windows.Forms.Label();
            this.lbl_Run1 = new System.Windows.Forms.Label();
            this.lbl_Run2 = new System.Windows.Forms.Label();
            this.lbl_TitleName = new System.Windows.Forms.Label();
            this.tbx_ColumnLongChar = new System.Windows.Forms.TextBox();
            this.btn_PreviewCheck = new System.Windows.Forms.Button();
            this.btn_Readpublic = new System.Windows.Forms.Button();
            this.tbx_SourceText = new System.Windows.Forms.TextBox();
            this.gbx_AutoMode = new System.Windows.Forms.GroupBox();
            this.lbl_WaitRun = new System.Windows.Forms.Label();
            this.pgb_Status = new System.Windows.Forms.ProgressBar();
            this.lbx_ItemNum = new System.Windows.Forms.ListBox();
            this.btn_ClearItem = new System.Windows.Forms.Button();
            this.btn_addItem = new System.Windows.Forms.Button();
            this.btn_AutoConfirm = new System.Windows.Forms.Button();
            this.cbx_ItemNum = new System.Windows.Forms.ComboBox();
            this.lbl_ObjectBarcode = new System.Windows.Forms.Label();
            this.gbx_MannulMode = new System.Windows.Forms.GroupBox();
            this.btn_addItem2 = new System.Windows.Forms.Button();
            this.lbl_Flag2 = new System.Windows.Forms.Label();
            this.cbx_ItemNum2 = new System.Windows.Forms.ComboBox();
            this.lbl_PuchWarePaper = new System.Windows.Forms.Label();
            this.tbx_MannulSigNum = new System.Windows.Forms.TextBox();
            this.tbx_PuchWarePaper = new System.Windows.Forms.TextBox();
            this.btn_ManualConfirm = new System.Windows.Forms.Button();
            this.lbl_SourceNum = new System.Windows.Forms.Label();
            this.btn_ReadSourceTable = new System.Windows.Forms.Button();
            this.tbx_Reason = new System.Windows.Forms.TextBox();
            this.lbl_ReasonReport = new System.Windows.Forms.Label();
            this.btn_Reset = new System.Windows.Forms.Button();
            this.btn_ResonConfirm = new System.Windows.Forms.Button();
            this.gbx_MannulCheck = new System.Windows.Forms.GroupBox();
            this.lbl_SelectRowItem = new System.Windows.Forms.Label();
            this.lbl_ID = new System.Windows.Forms.Label();
            this.lbl_Department = new System.Windows.Forms.Label();
            this.tbx_IDCheck = new System.Windows.Forms.TextBox();
            this.tbx_DepartCheck = new System.Windows.Forms.TextBox();
            this.lbl_TestTime = new System.Windows.Forms.Label();
            this.lbl_ChangeClass = new System.Windows.Forms.Label();
            this.lbl_ChangeType = new System.Windows.Forms.Label();
            this.btn_DelPuchExist = new System.Windows.Forms.Button();
            this.lbl_Invalid = new System.Windows.Forms.Label();
            this.lbl_dgvSelectStatus = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_SqlTableData)).BeginInit();
            this.gbx_DataTable.SuspendLayout();
            this.gbx_AutoMode.SuspendLayout();
            this.gbx_MannulMode.SuspendLayout();
            this.gbx_MannulCheck.SuspendLayout();
            this.SuspendLayout();
            // 
            // dgv_SqlTableData
            // 
            this.dgv_SqlTableData.AllowDrop = true;
            this.dgv_SqlTableData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_SqlTableData.Location = new System.Drawing.Point(11, 53);
            this.dgv_SqlTableData.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.dgv_SqlTableData.MultiSelect = false;
            this.dgv_SqlTableData.Name = "dgv_SqlTableData";
            this.dgv_SqlTableData.ReadOnly = true;
            this.dgv_SqlTableData.RowTemplate.Height = 27;
            this.dgv_SqlTableData.Size = new System.Drawing.Size(907, 213);
            this.dgv_SqlTableData.TabIndex = 0;
            this.dgv_SqlTableData.SelectionChanged += new System.EventHandler(this.dgv_SqlTableData_SelectionChanged);
            // 
            // lbl_SqlStatus
            // 
            this.lbl_SqlStatus.AutoSize = true;
            this.lbl_SqlStatus.Font = new System.Drawing.Font("DFKai-SB", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_SqlStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.lbl_SqlStatus.Location = new System.Drawing.Point(382, 18);
            this.lbl_SqlStatus.Name = "lbl_SqlStatus";
            this.lbl_SqlStatus.Size = new System.Drawing.Size(173, 28);
            this.lbl_SqlStatus.TabIndex = 4;
            this.lbl_SqlStatus.Text = "連結狀態---";
            // 
            // rbn_AutoScan
            // 
            this.rbn_AutoScan.AutoSize = true;
            this.rbn_AutoScan.Font = new System.Drawing.Font("DFKai-SB", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.rbn_AutoScan.ForeColor = System.Drawing.Color.Green;
            this.rbn_AutoScan.Location = new System.Drawing.Point(175, 427);
            this.rbn_AutoScan.Margin = new System.Windows.Forms.Padding(4);
            this.rbn_AutoScan.Name = "rbn_AutoScan";
            this.rbn_AutoScan.Size = new System.Drawing.Size(149, 31);
            this.rbn_AutoScan.TabIndex = 11;
            this.rbn_AutoScan.TabStop = true;
            this.rbn_AutoScan.Text = "掃描條碼";
            this.rbn_AutoScan.UseVisualStyleBackColor = true;
            this.rbn_AutoScan.CheckedChanged += new System.EventHandler(this.rbn_AutoScan_CheckedChanged);
            // 
            // rbn_ManualScan
            // 
            this.rbn_ManualScan.AutoSize = true;
            this.rbn_ManualScan.Font = new System.Drawing.Font("DFKai-SB", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.rbn_ManualScan.ForeColor = System.Drawing.Color.Red;
            this.rbn_ManualScan.Location = new System.Drawing.Point(640, 738);
            this.rbn_ManualScan.Margin = new System.Windows.Forms.Padding(4);
            this.rbn_ManualScan.Name = "rbn_ManualScan";
            this.rbn_ManualScan.Size = new System.Drawing.Size(149, 31);
            this.rbn_ManualScan.TabIndex = 12;
            this.rbn_ManualScan.TabStop = true;
            this.rbn_ManualScan.Text = "手動輸入";
            this.rbn_ManualScan.UseVisualStyleBackColor = true;
            this.rbn_ManualScan.CheckedChanged += new System.EventHandler(this.rbn_ManualScan_CheckedChanged);
            // 
            // gbx_DataTable
            // 
            this.gbx_DataTable.BackColor = System.Drawing.Color.Olive;
            this.gbx_DataTable.Controls.Add(this.lbl_WareScan);
            this.gbx_DataTable.Controls.Add(this.btn_WareSingConfirm);
            this.gbx_DataTable.Controls.Add(this.dgv_SqlTableData);
            this.gbx_DataTable.Controls.Add(this.tbx_WareSingScan);
            this.gbx_DataTable.Controls.Add(this.lbl_SqlStatus);
            this.gbx_DataTable.Font = new System.Drawing.Font("DFKai-SB", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.gbx_DataTable.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.gbx_DataTable.Location = new System.Drawing.Point(6, 89);
            this.gbx_DataTable.Margin = new System.Windows.Forms.Padding(4);
            this.gbx_DataTable.Name = "gbx_DataTable";
            this.gbx_DataTable.Padding = new System.Windows.Forms.Padding(4);
            this.gbx_DataTable.Size = new System.Drawing.Size(1243, 271);
            this.gbx_DataTable.TabIndex = 13;
            this.gbx_DataTable.TabStop = false;
            this.gbx_DataTable.Text = "資料表呈現";
            // 
            // lbl_WareScan
            // 
            this.lbl_WareScan.AutoSize = true;
            this.lbl_WareScan.Font = new System.Drawing.Font("DFKai-SB", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_WareScan.ForeColor = System.Drawing.Color.Plum;
            this.lbl_WareScan.Location = new System.Drawing.Point(958, 48);
            this.lbl_WareScan.Name = "lbl_WareScan";
            this.lbl_WareScan.Size = new System.Drawing.Size(273, 28);
            this.lbl_WareScan.TabIndex = 14;
            this.lbl_WareScan.Text = "異動單據掃描欄位↓";
            // 
            // btn_WareSingConfirm
            // 
            this.btn_WareSingConfirm.BackColor = System.Drawing.Color.Turquoise;
            this.btn_WareSingConfirm.Font = new System.Drawing.Font("DFKai-SB", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btn_WareSingConfirm.ForeColor = System.Drawing.Color.Black;
            this.btn_WareSingConfirm.Location = new System.Drawing.Point(989, 169);
            this.btn_WareSingConfirm.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btn_WareSingConfirm.Name = "btn_WareSingConfirm";
            this.btn_WareSingConfirm.Size = new System.Drawing.Size(184, 81);
            this.btn_WareSingConfirm.TabIndex = 15;
            this.btn_WareSingConfirm.Text = "入庫異動庫確認";
            this.btn_WareSingConfirm.UseVisualStyleBackColor = false;
            this.btn_WareSingConfirm.Click += new System.EventHandler(this.btn_WareSingConfirm_Click);
            // 
            // tbx_WareSingScan
            // 
            this.tbx_WareSingScan.Font = new System.Drawing.Font("PMingLiU", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.tbx_WareSingScan.Location = new System.Drawing.Point(924, 93);
            this.tbx_WareSingScan.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tbx_WareSingScan.Name = "tbx_WareSingScan";
            this.tbx_WareSingScan.Size = new System.Drawing.Size(309, 25);
            this.tbx_WareSingScan.TabIndex = 14;
            // 
            // lbl_Flag1
            // 
            this.lbl_Flag1.AutoSize = true;
            this.lbl_Flag1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lbl_Flag1.Font = new System.Drawing.Font("Microsoft JhengHei", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_Flag1.ForeColor = System.Drawing.Color.Black;
            this.lbl_Flag1.Location = new System.Drawing.Point(2, 364);
            this.lbl_Flag1.Name = "lbl_Flag1";
            this.lbl_Flag1.Size = new System.Drawing.Size(1214, 22);
            this.lbl_Flag1.TabIndex = 303;
            this.lbl_Flag1.Text = "---------------------------------------------------------------------------------" +
    "--------------------------------------------------------------------------------" +
    "-----------";
            // 
            // lbl_Run1
            // 
            this.lbl_Run1.AutoSize = true;
            this.lbl_Run1.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.lbl_Run1.Font = new System.Drawing.Font("DFKai-SB", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_Run1.ForeColor = System.Drawing.Color.White;
            this.lbl_Run1.Location = new System.Drawing.Point(13, 57);
            this.lbl_Run1.Name = "lbl_Run1";
            this.lbl_Run1.Size = new System.Drawing.Size(104, 20);
            this.lbl_Run1.TabIndex = 304;
            this.lbl_Run1.Text = "執行順序1";
            // 
            // lbl_Run2
            // 
            this.lbl_Run2.AutoSize = true;
            this.lbl_Run2.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.lbl_Run2.Font = new System.Drawing.Font("DFKai-SB", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_Run2.ForeColor = System.Drawing.Color.White;
            this.lbl_Run2.Location = new System.Drawing.Point(13, 430);
            this.lbl_Run2.Name = "lbl_Run2";
            this.lbl_Run2.Size = new System.Drawing.Size(104, 20);
            this.lbl_Run2.TabIndex = 305;
            this.lbl_Run2.Text = "執行順序2";
            // 
            // lbl_TitleName
            // 
            this.lbl_TitleName.AutoSize = true;
            this.lbl_TitleName.BackColor = System.Drawing.Color.Blue;
            this.lbl_TitleName.Font = new System.Drawing.Font("DFKai-SB", 36F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_TitleName.ForeColor = System.Drawing.Color.Yellow;
            this.lbl_TitleName.Location = new System.Drawing.Point(383, 11);
            this.lbl_TitleName.Name = "lbl_TitleName";
            this.lbl_TitleName.Size = new System.Drawing.Size(391, 60);
            this.lbl_TitleName.TabIndex = 307;
            this.lbl_TitleName.Text = "物料入庫作業";
            // 
            // tbx_ColumnLongChar
            // 
            this.tbx_ColumnLongChar.Location = new System.Drawing.Point(120, 52);
            this.tbx_ColumnLongChar.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tbx_ColumnLongChar.Name = "tbx_ColumnLongChar";
            this.tbx_ColumnLongChar.Size = new System.Drawing.Size(314, 31);
            this.tbx_ColumnLongChar.TabIndex = 9;
            // 
            // btn_PreviewCheck
            // 
            this.btn_PreviewCheck.Font = new System.Drawing.Font("DFKai-SB", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btn_PreviewCheck.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.btn_PreviewCheck.Location = new System.Drawing.Point(456, 68);
            this.btn_PreviewCheck.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btn_PreviewCheck.Name = "btn_PreviewCheck";
            this.btn_PreviewCheck.Size = new System.Drawing.Size(129, 42);
            this.btn_PreviewCheck.TabIndex = 3;
            this.btn_PreviewCheck.Text = "<-前置確認";
            this.btn_PreviewCheck.UseVisualStyleBackColor = true;
            this.btn_PreviewCheck.Click += new System.EventHandler(this.btn_Updateprivate_Click);
            // 
            // btn_Readpublic
            // 
            this.btn_Readpublic.Font = new System.Drawing.Font("DFKai-SB", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btn_Readpublic.ForeColor = System.Drawing.Color.Purple;
            this.btn_Readpublic.Location = new System.Drawing.Point(1108, 31);
            this.btn_Readpublic.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btn_Readpublic.Name = "btn_Readpublic";
            this.btn_Readpublic.Size = new System.Drawing.Size(137, 42);
            this.btn_Readpublic.TabIndex = 1;
            this.btn_Readpublic.Text = "讀取公用表";
            this.btn_Readpublic.UseVisualStyleBackColor = true;
            this.btn_Readpublic.Click += new System.EventHandler(this.btn_Readpublic_Click);
            // 
            // tbx_SourceText
            // 
            this.tbx_SourceText.Location = new System.Drawing.Point(17, 80);
            this.tbx_SourceText.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tbx_SourceText.Name = "tbx_SourceText";
            this.tbx_SourceText.Size = new System.Drawing.Size(451, 25);
            this.tbx_SourceText.TabIndex = 307;
            this.tbx_SourceText.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tbx_SourceText_KeyPress);
            // 
            // gbx_AutoMode
            // 
            this.gbx_AutoMode.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.gbx_AutoMode.Controls.Add(this.lbl_WaitRun);
            this.gbx_AutoMode.Controls.Add(this.pgb_Status);
            this.gbx_AutoMode.Controls.Add(this.lbx_ItemNum);
            this.gbx_AutoMode.Controls.Add(this.btn_ClearItem);
            this.gbx_AutoMode.Controls.Add(this.btn_addItem);
            this.gbx_AutoMode.Controls.Add(this.btn_AutoConfirm);
            this.gbx_AutoMode.Controls.Add(this.cbx_ItemNum);
            this.gbx_AutoMode.Controls.Add(this.lbl_ObjectBarcode);
            this.gbx_AutoMode.Controls.Add(this.tbx_SourceText);
            this.gbx_AutoMode.Font = new System.Drawing.Font("PMingLiU", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.gbx_AutoMode.ForeColor = System.Drawing.Color.Blue;
            this.gbx_AutoMode.Location = new System.Drawing.Point(17, 474);
            this.gbx_AutoMode.Margin = new System.Windows.Forms.Padding(4);
            this.gbx_AutoMode.Name = "gbx_AutoMode";
            this.gbx_AutoMode.Padding = new System.Windows.Forms.Padding(4);
            this.gbx_AutoMode.Size = new System.Drawing.Size(615, 359);
            this.gbx_AutoMode.TabIndex = 306;
            this.gbx_AutoMode.TabStop = false;
            this.gbx_AutoMode.Text = "條碼模式";
            // 
            // lbl_WaitRun
            // 
            this.lbl_WaitRun.AutoSize = true;
            this.lbl_WaitRun.Font = new System.Drawing.Font("DFKai-SB", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_WaitRun.ForeColor = System.Drawing.Color.Blue;
            this.lbl_WaitRun.Location = new System.Drawing.Point(487, 333);
            this.lbl_WaitRun.Name = "lbl_WaitRun";
            this.lbl_WaitRun.Size = new System.Drawing.Size(72, 19);
            this.lbl_WaitRun.TabIndex = 325;
            this.lbl_WaitRun.Text = "等待中";
            // 
            // pgb_Status
            // 
            this.pgb_Status.Location = new System.Drawing.Point(17, 329);
            this.pgb_Status.Name = "pgb_Status";
            this.pgb_Status.Size = new System.Drawing.Size(451, 23);
            this.pgb_Status.TabIndex = 324;
            // 
            // lbx_ItemNum
            // 
            this.lbx_ItemNum.Font = new System.Drawing.Font("PMingLiU", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbx_ItemNum.FormattingEnabled = true;
            this.lbx_ItemNum.ItemHeight = 15;
            this.lbx_ItemNum.Location = new System.Drawing.Point(17, 133);
            this.lbx_ItemNum.Name = "lbx_ItemNum";
            this.lbx_ItemNum.Size = new System.Drawing.Size(451, 184);
            this.lbx_ItemNum.TabIndex = 313;
            this.lbx_ItemNum.SelectedIndexChanged += new System.EventHandler(this.lbx_ItemNum_SelectedIndexChanged);
            // 
            // btn_ClearItem
            // 
            this.btn_ClearItem.Font = new System.Drawing.Font("PMingLiU", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btn_ClearItem.Location = new System.Drawing.Point(487, 133);
            this.btn_ClearItem.Name = "btn_ClearItem";
            this.btn_ClearItem.Size = new System.Drawing.Size(108, 37);
            this.btn_ClearItem.TabIndex = 312;
            this.btn_ClearItem.Text = "清除項目";
            this.btn_ClearItem.UseVisualStyleBackColor = true;
            this.btn_ClearItem.Click += new System.EventHandler(this.btn_ClearItem_Click);
            // 
            // btn_addItem
            // 
            this.btn_addItem.Font = new System.Drawing.Font("DFKai-SB", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btn_addItem.ForeColor = System.Drawing.Color.Fuchsia;
            this.btn_addItem.Location = new System.Drawing.Point(490, 79);
            this.btn_addItem.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btn_addItem.Name = "btn_addItem";
            this.btn_addItem.Size = new System.Drawing.Size(101, 42);
            this.btn_addItem.TabIndex = 311;
            this.btn_addItem.Text = "加入批號";
            this.btn_addItem.UseVisualStyleBackColor = true;
            this.btn_addItem.Click += new System.EventHandler(this.btn_addItem_Click);
            // 
            // btn_AutoConfirm
            // 
            this.btn_AutoConfirm.Font = new System.Drawing.Font("DFKai-SB", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btn_AutoConfirm.ForeColor = System.Drawing.Color.Green;
            this.btn_AutoConfirm.Location = new System.Drawing.Point(490, 254);
            this.btn_AutoConfirm.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btn_AutoConfirm.Name = "btn_AutoConfirm";
            this.btn_AutoConfirm.Size = new System.Drawing.Size(108, 56);
            this.btn_AutoConfirm.TabIndex = 310;
            this.btn_AutoConfirm.Text = "總確認入庫";
            this.btn_AutoConfirm.UseVisualStyleBackColor = true;
            this.btn_AutoConfirm.Click += new System.EventHandler(this.btn_AutoConfirm_Click);
            // 
            // cbx_ItemNum
            // 
            this.cbx_ItemNum.BackColor = System.Drawing.SystemColors.HighlightText;
            this.cbx_ItemNum.ForeColor = System.Drawing.SystemColors.InfoText;
            this.cbx_ItemNum.FormattingEnabled = true;
            this.cbx_ItemNum.Location = new System.Drawing.Point(443, 36);
            this.cbx_ItemNum.Margin = new System.Windows.Forms.Padding(4);
            this.cbx_ItemNum.Name = "cbx_ItemNum";
            this.cbx_ItemNum.Size = new System.Drawing.Size(165, 23);
            this.cbx_ItemNum.TabIndex = 309;
            this.cbx_ItemNum.SelectedIndexChanged += new System.EventHandler(this.cbx_ItemNum_SelectedIndexChanged);
            // 
            // lbl_ObjectBarcode
            // 
            this.lbl_ObjectBarcode.AutoSize = true;
            this.lbl_ObjectBarcode.Font = new System.Drawing.Font("DFKai-SB", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_ObjectBarcode.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.lbl_ObjectBarcode.Location = new System.Drawing.Point(13, 44);
            this.lbl_ObjectBarcode.Name = "lbl_ObjectBarcode";
            this.lbl_ObjectBarcode.Size = new System.Drawing.Size(156, 20);
            this.lbl_ObjectBarcode.TabIndex = 308;
            this.lbl_ObjectBarcode.Text = "物料掃描欄位↓";
            // 
            // gbx_MannulMode
            // 
            this.gbx_MannulMode.BackColor = System.Drawing.Color.Yellow;
            this.gbx_MannulMode.Controls.Add(this.btn_addItem2);
            this.gbx_MannulMode.Controls.Add(this.lbl_Flag2);
            this.gbx_MannulMode.Controls.Add(this.cbx_ItemNum2);
            this.gbx_MannulMode.Controls.Add(this.lbl_PuchWarePaper);
            this.gbx_MannulMode.Controls.Add(this.tbx_MannulSigNum);
            this.gbx_MannulMode.Controls.Add(this.tbx_PuchWarePaper);
            this.gbx_MannulMode.Controls.Add(this.btn_ManualConfirm);
            this.gbx_MannulMode.Controls.Add(this.lbl_SourceNum);
            this.gbx_MannulMode.Controls.Add(this.tbx_ColumnLongChar);
            this.gbx_MannulMode.Controls.Add(this.btn_PreviewCheck);
            this.gbx_MannulMode.Font = new System.Drawing.Font("PMingLiU", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.gbx_MannulMode.ForeColor = System.Drawing.Color.Blue;
            this.gbx_MannulMode.Location = new System.Drawing.Point(640, 787);
            this.gbx_MannulMode.Margin = new System.Windows.Forms.Padding(4);
            this.gbx_MannulMode.Name = "gbx_MannulMode";
            this.gbx_MannulMode.Padding = new System.Windows.Forms.Padding(4);
            this.gbx_MannulMode.Size = new System.Drawing.Size(609, 39);
            this.gbx_MannulMode.TabIndex = 308;
            this.gbx_MannulMode.TabStop = false;
            this.gbx_MannulMode.Text = "手鍵模式";
            // 
            // btn_addItem2
            // 
            this.btn_addItem2.Font = new System.Drawing.Font("DFKai-SB", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btn_addItem2.ForeColor = System.Drawing.Color.RoyalBlue;
            this.btn_addItem2.Location = new System.Drawing.Point(37, 278);
            this.btn_addItem2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btn_addItem2.Name = "btn_addItem2";
            this.btn_addItem2.Size = new System.Drawing.Size(101, 42);
            this.btn_addItem2.TabIndex = 314;
            this.btn_addItem2.Text = "加入批號";
            this.btn_addItem2.UseVisualStyleBackColor = true;
            // 
            // lbl_Flag2
            // 
            this.lbl_Flag2.AutoSize = true;
            this.lbl_Flag2.BackColor = System.Drawing.Color.DarkGreen;
            this.lbl_Flag2.Font = new System.Drawing.Font("Microsoft JhengHei", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_Flag2.ForeColor = System.Drawing.Color.Black;
            this.lbl_Flag2.Location = new System.Drawing.Point(4, 163);
            this.lbl_Flag2.Name = "lbl_Flag2";
            this.lbl_Flag2.Size = new System.Drawing.Size(605, 22);
            this.lbl_Flag2.TabIndex = 309;
            this.lbl_Flag2.Text = "---------------------------------------------------------------------------------" +
    "----";
            // 
            // cbx_ItemNum2
            // 
            this.cbx_ItemNum2.BackColor = System.Drawing.SystemColors.InfoText;
            this.cbx_ItemNum2.ForeColor = System.Drawing.SystemColors.Info;
            this.cbx_ItemNum2.FormattingEnabled = true;
            this.cbx_ItemNum2.Location = new System.Drawing.Point(149, 286);
            this.cbx_ItemNum2.Margin = new System.Windows.Forms.Padding(4);
            this.cbx_ItemNum2.Name = "cbx_ItemNum2";
            this.cbx_ItemNum2.Size = new System.Drawing.Size(100, 28);
            this.cbx_ItemNum2.TabIndex = 313;
            // 
            // lbl_PuchWarePaper
            // 
            this.lbl_PuchWarePaper.AutoSize = true;
            this.lbl_PuchWarePaper.Font = new System.Drawing.Font("DFKai-SB", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_PuchWarePaper.ForeColor = System.Drawing.Color.Maroon;
            this.lbl_PuchWarePaper.Location = new System.Drawing.Point(13, 101);
            this.lbl_PuchWarePaper.Name = "lbl_PuchWarePaper";
            this.lbl_PuchWarePaper.Size = new System.Drawing.Size(93, 20);
            this.lbl_PuchWarePaper.TabIndex = 312;
            this.lbl_PuchWarePaper.Text = "採購單號";
            // 
            // tbx_MannulSigNum
            // 
            this.tbx_MannulSigNum.Location = new System.Drawing.Point(37, 223);
            this.tbx_MannulSigNum.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tbx_MannulSigNum.Multiline = true;
            this.tbx_MannulSigNum.Name = "tbx_MannulSigNum";
            this.tbx_MannulSigNum.Size = new System.Drawing.Size(212, 34);
            this.tbx_MannulSigNum.TabIndex = 312;
            // 
            // tbx_PuchWarePaper
            // 
            this.tbx_PuchWarePaper.Location = new System.Drawing.Point(120, 97);
            this.tbx_PuchWarePaper.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tbx_PuchWarePaper.Name = "tbx_PuchWarePaper";
            this.tbx_PuchWarePaper.Size = new System.Drawing.Size(314, 31);
            this.tbx_PuchWarePaper.TabIndex = 311;
            // 
            // btn_ManualConfirm
            // 
            this.btn_ManualConfirm.Font = new System.Drawing.Font("DFKai-SB", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btn_ManualConfirm.ForeColor = System.Drawing.Color.Red;
            this.btn_ManualConfirm.Location = new System.Drawing.Point(339, 256);
            this.btn_ManualConfirm.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btn_ManualConfirm.Name = "btn_ManualConfirm";
            this.btn_ManualConfirm.Size = new System.Drawing.Size(236, 56);
            this.btn_ManualConfirm.TabIndex = 310;
            this.btn_ManualConfirm.Text = "總確認入庫";
            this.btn_ManualConfirm.UseVisualStyleBackColor = true;
            // 
            // lbl_SourceNum
            // 
            this.lbl_SourceNum.AutoSize = true;
            this.lbl_SourceNum.Font = new System.Drawing.Font("DFKai-SB", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_SourceNum.ForeColor = System.Drawing.Color.Maroon;
            this.lbl_SourceNum.Location = new System.Drawing.Point(33, 65);
            this.lbl_SourceNum.Name = "lbl_SourceNum";
            this.lbl_SourceNum.Size = new System.Drawing.Size(51, 20);
            this.lbl_SourceNum.TabIndex = 308;
            this.lbl_SourceNum.Text = "料號";
            // 
            // btn_ReadSourceTable
            // 
            this.btn_ReadSourceTable.Font = new System.Drawing.Font("DFKai-SB", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btn_ReadSourceTable.ForeColor = System.Drawing.Color.Green;
            this.btn_ReadSourceTable.Location = new System.Drawing.Point(980, 31);
            this.btn_ReadSourceTable.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btn_ReadSourceTable.Name = "btn_ReadSourceTable";
            this.btn_ReadSourceTable.Size = new System.Drawing.Size(123, 42);
            this.btn_ReadSourceTable.TabIndex = 309;
            this.btn_ReadSourceTable.Text = "讀取物料表";
            this.btn_ReadSourceTable.UseVisualStyleBackColor = true;
            // 
            // tbx_Reason
            // 
            this.tbx_Reason.Location = new System.Drawing.Point(67, 36);
            this.tbx_Reason.Multiline = true;
            this.tbx_Reason.Name = "tbx_Reason";
            this.tbx_Reason.Size = new System.Drawing.Size(262, 25);
            this.tbx_Reason.TabIndex = 311;
            // 
            // lbl_ReasonReport
            // 
            this.lbl_ReasonReport.AutoSize = true;
            this.lbl_ReasonReport.Font = new System.Drawing.Font("PMingLiU", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_ReasonReport.Location = new System.Drawing.Point(6, 37);
            this.lbl_ReasonReport.Name = "lbl_ReasonReport";
            this.lbl_ReasonReport.Size = new System.Drawing.Size(55, 19);
            this.lbl_ReasonReport.TabIndex = 312;
            this.lbl_ReasonReport.Text = "理由:";
            // 
            // btn_Reset
            // 
            this.btn_Reset.Font = new System.Drawing.Font("DFKai-SB", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btn_Reset.ForeColor = System.Drawing.Color.Navy;
            this.btn_Reset.Location = new System.Drawing.Point(813, 31);
            this.btn_Reset.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btn_Reset.Name = "btn_Reset";
            this.btn_Reset.Size = new System.Drawing.Size(111, 40);
            this.btn_Reset.TabIndex = 313;
            this.btn_Reset.Text = "重新啟動";
            this.btn_Reset.UseVisualStyleBackColor = true;
            this.btn_Reset.Click += new System.EventHandler(this.btn_Reset_Click);
            // 
            // btn_ResonConfirm
            // 
            this.btn_ResonConfirm.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.btn_ResonConfirm.Font = new System.Drawing.Font("PMingLiU", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btn_ResonConfirm.ForeColor = System.Drawing.Color.Teal;
            this.btn_ResonConfirm.Location = new System.Drawing.Point(342, 33);
            this.btn_ResonConfirm.Name = "btn_ResonConfirm";
            this.btn_ResonConfirm.Size = new System.Drawing.Size(75, 31);
            this.btn_ResonConfirm.TabIndex = 315;
            this.btn_ResonConfirm.Text = "確認";
            this.btn_ResonConfirm.UseVisualStyleBackColor = false;
            this.btn_ResonConfirm.Click += new System.EventHandler(this.btn_ResonConfirm_Click);
            // 
            // gbx_MannulCheck
            // 
            this.gbx_MannulCheck.BackColor = System.Drawing.Color.DarkGray;
            this.gbx_MannulCheck.Controls.Add(this.tbx_Reason);
            this.gbx_MannulCheck.Controls.Add(this.btn_ResonConfirm);
            this.gbx_MannulCheck.Controls.Add(this.lbl_ReasonReport);
            this.gbx_MannulCheck.Font = new System.Drawing.Font("PMingLiU", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.gbx_MannulCheck.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.gbx_MannulCheck.Location = new System.Drawing.Point(821, 703);
            this.gbx_MannulCheck.Name = "gbx_MannulCheck";
            this.gbx_MannulCheck.Size = new System.Drawing.Size(428, 77);
            this.gbx_MannulCheck.TabIndex = 316;
            this.gbx_MannulCheck.TabStop = false;
            this.gbx_MannulCheck.Text = "手動確認";
            // 
            // lbl_SelectRowItem
            // 
            this.lbl_SelectRowItem.AutoSize = true;
            this.lbl_SelectRowItem.BackColor = System.Drawing.Color.Black;
            this.lbl_SelectRowItem.Font = new System.Drawing.Font("DFKai-SB", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_SelectRowItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.lbl_SelectRowItem.Location = new System.Drawing.Point(456, 400);
            this.lbl_SelectRowItem.Name = "lbl_SelectRowItem";
            this.lbl_SelectRowItem.Size = new System.Drawing.Size(93, 20);
            this.lbl_SelectRowItem.TabIndex = 318;
            this.lbl_SelectRowItem.Text = "等待選擇";
            // 
            // lbl_ID
            // 
            this.lbl_ID.AutoSize = true;
            this.lbl_ID.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.lbl_ID.Location = new System.Drawing.Point(120, 16);
            this.lbl_ID.Name = "lbl_ID";
            this.lbl_ID.Size = new System.Drawing.Size(37, 15);
            this.lbl_ID.TabIndex = 319;
            this.lbl_ID.Text = "帳號";
            // 
            // lbl_Department
            // 
            this.lbl_Department.AutoSize = true;
            this.lbl_Department.ForeColor = System.Drawing.Color.Red;
            this.lbl_Department.Location = new System.Drawing.Point(119, 37);
            this.lbl_Department.Name = "lbl_Department";
            this.lbl_Department.Size = new System.Drawing.Size(37, 15);
            this.lbl_Department.TabIndex = 320;
            this.lbl_Department.Text = "部門";
            // 
            // tbx_IDCheck
            // 
            this.tbx_IDCheck.Location = new System.Drawing.Point(180, 6);
            this.tbx_IDCheck.Name = "tbx_IDCheck";
            this.tbx_IDCheck.Size = new System.Drawing.Size(178, 25);
            this.tbx_IDCheck.TabIndex = 321;
            // 
            // tbx_DepartCheck
            // 
            this.tbx_DepartCheck.Location = new System.Drawing.Point(180, 37);
            this.tbx_DepartCheck.Name = "tbx_DepartCheck";
            this.tbx_DepartCheck.Size = new System.Drawing.Size(178, 25);
            this.tbx_DepartCheck.TabIndex = 322;
            // 
            // lbl_TestTime
            // 
            this.lbl_TestTime.AutoSize = true;
            this.lbl_TestTime.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
            this.lbl_TestTime.Location = new System.Drawing.Point(50, 404);
            this.lbl_TestTime.Name = "lbl_TestTime";
            this.lbl_TestTime.Size = new System.Drawing.Size(19, 15);
            this.lbl_TestTime.TabIndex = 323;
            this.lbl_TestTime.Text = ": :";
            // 
            // lbl_ChangeClass
            // 
            this.lbl_ChangeClass.AutoSize = true;
            this.lbl_ChangeClass.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.lbl_ChangeClass.Location = new System.Drawing.Point(408, 441);
            this.lbl_ChangeClass.Name = "lbl_ChangeClass";
            this.lbl_ChangeClass.Size = new System.Drawing.Size(41, 15);
            this.lbl_ChangeClass.TabIndex = 324;
            this.lbl_ChangeClass.Text = "種類:";
            // 
            // lbl_ChangeType
            // 
            this.lbl_ChangeType.AutoSize = true;
            this.lbl_ChangeType.ForeColor = System.Drawing.Color.Maroon;
            this.lbl_ChangeType.Location = new System.Drawing.Point(480, 439);
            this.lbl_ChangeType.Name = "lbl_ChangeType";
            this.lbl_ChangeType.Size = new System.Drawing.Size(23, 15);
            this.lbl_ChangeType.TabIndex = 325;
            this.lbl_ChangeType.Text = "==";
            // 
            // btn_DelPuchExist
            // 
            this.btn_DelPuchExist.BackColor = System.Drawing.Color.Yellow;
            this.btn_DelPuchExist.Font = new System.Drawing.Font("DFKai-SB", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btn_DelPuchExist.ForeColor = System.Drawing.Color.Navy;
            this.btn_DelPuchExist.Location = new System.Drawing.Point(776, 476);
            this.btn_DelPuchExist.Name = "btn_DelPuchExist";
            this.btn_DelPuchExist.Size = new System.Drawing.Size(191, 57);
            this.btn_DelPuchExist.TabIndex = 326;
            this.btn_DelPuchExist.Text = "啟動作廢";
            this.btn_DelPuchExist.UseVisualStyleBackColor = false;
            this.btn_DelPuchExist.Click += new System.EventHandler(this.btn_DelPuchExist_Click);
            // 
            // lbl_Invalid
            // 
            this.lbl_Invalid.AutoSize = true;
            this.lbl_Invalid.Font = new System.Drawing.Font("DFKai-SB", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_Invalid.ForeColor = System.Drawing.Color.Blue;
            this.lbl_Invalid.Location = new System.Drawing.Point(663, 549);
            this.lbl_Invalid.Name = "lbl_Invalid";
            this.lbl_Invalid.Size = new System.Drawing.Size(432, 30);
            this.lbl_Invalid.TabIndex = 327;
            this.lbl_Invalid.Text = "如需作廢請按上方按鈕,謝謝←";
            // 
            // lbl_dgvSelectStatus
            // 
            this.lbl_dgvSelectStatus.AutoSize = true;
            this.lbl_dgvSelectStatus.Font = new System.Drawing.Font("DFKai-SB", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_dgvSelectStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.lbl_dgvSelectStatus.Location = new System.Drawing.Point(755, 608);
            this.lbl_dgvSelectStatus.Name = "lbl_dgvSelectStatus";
            this.lbl_dgvSelectStatus.Size = new System.Drawing.Size(57, 28);
            this.lbl_dgvSelectStatus.TabIndex = 328;
            this.lbl_dgvSelectStatus.Text = "===";
            // 
            // PuchWarehousing
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.ClientSize = new System.Drawing.Size(1261, 846);
            this.Controls.Add(this.lbl_dgvSelectStatus);
            this.Controls.Add(this.lbl_Invalid);
            this.Controls.Add(this.btn_DelPuchExist);
            this.Controls.Add(this.lbl_ChangeType);
            this.Controls.Add(this.lbl_ChangeClass);
            this.Controls.Add(this.lbl_TestTime);
            this.Controls.Add(this.tbx_DepartCheck);
            this.Controls.Add(this.tbx_IDCheck);
            this.Controls.Add(this.lbl_Department);
            this.Controls.Add(this.lbl_ID);
            this.Controls.Add(this.lbl_SelectRowItem);
            this.Controls.Add(this.gbx_MannulCheck);
            this.Controls.Add(this.btn_Reset);
            this.Controls.Add(this.btn_ReadSourceTable);
            this.Controls.Add(this.rbn_ManualScan);
            this.Controls.Add(this.gbx_MannulMode);
            this.Controls.Add(this.lbl_TitleName);
            this.Controls.Add(this.gbx_AutoMode);
            this.Controls.Add(this.lbl_Run2);
            this.Controls.Add(this.btn_Readpublic);
            this.Controls.Add(this.lbl_Run1);
            this.Controls.Add(this.lbl_Flag1);
            this.Controls.Add(this.gbx_DataTable);
            this.Controls.Add(this.rbn_AutoScan);
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "PuchWarehousing";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.Text = "刪除前次資料";
            this.Load += new System.EventHandler(this.PuchaseWarehousing_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_SqlTableData)).EndInit();
            this.gbx_DataTable.ResumeLayout(false);
            this.gbx_DataTable.PerformLayout();
            this.gbx_AutoMode.ResumeLayout(false);
            this.gbx_AutoMode.PerformLayout();
            this.gbx_MannulMode.ResumeLayout(false);
            this.gbx_MannulMode.PerformLayout();
            this.gbx_MannulCheck.ResumeLayout(false);
            this.gbx_MannulCheck.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dgv_SqlTableData;
        private System.Windows.Forms.Label lbl_SqlStatus;
        private System.Windows.Forms.RadioButton rbn_AutoScan;
        private System.Windows.Forms.RadioButton rbn_ManualScan;
        private System.Windows.Forms.GroupBox gbx_DataTable;
        private System.Windows.Forms.Button btn_WareSingConfirm;
        private System.Windows.Forms.TextBox tbx_WareSingScan;
        private System.Windows.Forms.Label lbl_WareScan;
        private System.Windows.Forms.Label lbl_Flag1;
        private System.Windows.Forms.Label lbl_Run1;
        private System.Windows.Forms.Label lbl_Run2;
        private System.Windows.Forms.Label lbl_TitleName;
        private System.Windows.Forms.TextBox tbx_ColumnLongChar;
        private System.Windows.Forms.Button btn_PreviewCheck;
        private System.Windows.Forms.Button btn_Readpublic;
        private System.Windows.Forms.TextBox tbx_SourceText;
        private System.Windows.Forms.GroupBox gbx_AutoMode;
        private System.Windows.Forms.Label lbl_ObjectBarcode;
        private System.Windows.Forms.ComboBox cbx_ItemNum;
        private System.Windows.Forms.Button btn_addItem;
        private System.Windows.Forms.Button btn_AutoConfirm;
        private System.Windows.Forms.GroupBox gbx_MannulMode;
        private System.Windows.Forms.Button btn_ManualConfirm;
        private System.Windows.Forms.Label lbl_SourceNum;
        private System.Windows.Forms.TextBox tbx_PuchWarePaper;
        private System.Windows.Forms.Label lbl_PuchWarePaper;
        private System.Windows.Forms.Button btn_addItem2;
        private System.Windows.Forms.Label lbl_Flag2;
        private System.Windows.Forms.ComboBox cbx_ItemNum2;
        private System.Windows.Forms.TextBox tbx_MannulSigNum;
        private System.Windows.Forms.Button btn_ReadSourceTable;
        private System.Windows.Forms.TextBox tbx_Reason;
        private System.Windows.Forms.Label lbl_ReasonReport;
        private System.Windows.Forms.Button btn_Reset;
        private System.Windows.Forms.Button btn_ResonConfirm;
        private System.Windows.Forms.GroupBox gbx_MannulCheck;
        private System.Windows.Forms.Label lbl_SelectRowItem;
        private System.Windows.Forms.Button btn_ClearItem;
        private System.Windows.Forms.ListBox lbx_ItemNum;
        private System.Windows.Forms.Label lbl_ID;
        private System.Windows.Forms.Label lbl_Department;
        private System.Windows.Forms.TextBox tbx_IDCheck;
        private System.Windows.Forms.TextBox tbx_DepartCheck;
        private System.Windows.Forms.Label lbl_TestTime;
        private System.Windows.Forms.ProgressBar pgb_Status;
        private System.Windows.Forms.Label lbl_WaitRun;
        private System.Windows.Forms.Label lbl_ChangeClass;
        private System.Windows.Forms.Label lbl_ChangeType;
        private System.Windows.Forms.Button btn_DelPuchExist;
        private System.Windows.Forms.Label lbl_Invalid;
        private System.Windows.Forms.Label lbl_dgvSelectStatus;
    }
}

