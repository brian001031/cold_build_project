namespace ReadTXT
{
    partial class ReadTxt
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ReadTxt));
            this.btn_ReadTxt = new System.Windows.Forms.Button();
            this.tbx_ReadALL = new System.Windows.Forms.TextBox();
            this.lbl_readTxt = new System.Windows.Forms.Label();
            this.fbd_ReadTXT = new System.Windows.Forms.FolderBrowserDialog();
            this.btn_open = new System.Windows.Forms.Button();
            this.lbx_txtinfo = new System.Windows.Forms.ListBox();
            this.tbx_FilePath = new System.Windows.Forms.TextBox();
            this.pic_Img = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.printPreviewDialog1 = new System.Windows.Forms.PrintPreviewDialog();
            this.ptDoc = new System.Drawing.Printing.PrintDocument();
            this.btn_Print = new System.Windows.Forms.Button();
            this.tbx_allReciver = new System.Windows.Forms.TextBox();
            this.lbl_Reciver = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pic_Img)).BeginInit();
            this.SuspendLayout();
            // 
            // btn_ReadTxt
            // 
            this.btn_ReadTxt.Font = new System.Drawing.Font("Cambria", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_ReadTxt.Location = new System.Drawing.Point(42, 505);
            this.btn_ReadTxt.Name = "btn_ReadTxt";
            this.btn_ReadTxt.Size = new System.Drawing.Size(144, 44);
            this.btn_ReadTxt.TabIndex = 0;
            this.btn_ReadTxt.Text = "ReadTXT檔";
            this.btn_ReadTxt.UseVisualStyleBackColor = true;
            this.btn_ReadTxt.Click += new System.EventHandler(this.btn_ReadTxt_Click);
            // 
            // tbx_ReadALL
            // 
            this.tbx_ReadALL.Font = new System.Drawing.Font("PMingLiU", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.tbx_ReadALL.Location = new System.Drawing.Point(204, 208);
            this.tbx_ReadALL.Multiline = true;
            this.tbx_ReadALL.Name = "tbx_ReadALL";
            this.tbx_ReadALL.Size = new System.Drawing.Size(762, 353);
            this.tbx_ReadALL.TabIndex = 1;
            // 
            // lbl_readTxt
            // 
            this.lbl_readTxt.AutoSize = true;
            this.lbl_readTxt.Font = new System.Drawing.Font("DFKai-SB", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_readTxt.ForeColor = System.Drawing.Color.Red;
            this.lbl_readTxt.Location = new System.Drawing.Point(37, 454);
            this.lbl_readTxt.Name = "lbl_readTxt";
            this.lbl_readTxt.Size = new System.Drawing.Size(149, 24);
            this.lbl_readTxt.TabIndex = 2;
            this.lbl_readTxt.Text = "讀取TXT結果";
            // 
            // btn_open
            // 
            this.btn_open.Font = new System.Drawing.Font("DFKai-SB", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btn_open.Location = new System.Drawing.Point(23, 26);
            this.btn_open.Name = "btn_open";
            this.btn_open.Size = new System.Drawing.Size(150, 56);
            this.btn_open.TabIndex = 3;
            this.btn_open.Text = "OpenTXT";
            this.btn_open.UseVisualStyleBackColor = true;
            this.btn_open.Click += new System.EventHandler(this.btn_open_Click);
            // 
            // lbx_txtinfo
            // 
            this.lbx_txtinfo.FormattingEnabled = true;
            this.lbx_txtinfo.ItemHeight = 15;
            this.lbx_txtinfo.Location = new System.Drawing.Point(23, 112);
            this.lbx_txtinfo.Name = "lbx_txtinfo";
            this.lbx_txtinfo.Size = new System.Drawing.Size(175, 244);
            this.lbx_txtinfo.TabIndex = 4;
            // 
            // tbx_FilePath
            // 
            this.tbx_FilePath.Location = new System.Drawing.Point(191, 38);
            this.tbx_FilePath.Multiline = true;
            this.tbx_FilePath.Name = "tbx_FilePath";
            this.tbx_FilePath.Size = new System.Drawing.Size(384, 39);
            this.tbx_FilePath.TabIndex = 5;
            // 
            // pic_Img
            // 
            this.pic_Img.BackColor = System.Drawing.Color.Blue;
            this.pic_Img.Location = new System.Drawing.Point(668, 139);
            this.pic_Img.Name = "pic_Img";
            this.pic_Img.Size = new System.Drawing.Size(298, 63);
            this.pic_Img.TabIndex = 6;
            this.pic_Img.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("MingLiU", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label1.ForeColor = System.Drawing.Color.Magenta;
            this.label1.Location = new System.Drawing.Point(491, 159);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(158, 28);
            this.label1.TabIndex = 7;
            this.label1.Text = "產生條碼->";
            // 
            // printPreviewDialog1
            // 
            this.printPreviewDialog1.AutoScrollMargin = new System.Drawing.Size(0, 0);
            this.printPreviewDialog1.AutoScrollMinSize = new System.Drawing.Size(0, 0);
            this.printPreviewDialog1.ClientSize = new System.Drawing.Size(400, 300);
            this.printPreviewDialog1.Document = this.ptDoc;
            this.printPreviewDialog1.Enabled = true;
            this.printPreviewDialog1.Icon = ((System.Drawing.Icon)(resources.GetObject("printPreviewDialog1.Icon")));
            this.printPreviewDialog1.Name = "printPreviewDialog1";
            this.printPreviewDialog1.Visible = false;
            // 
            // ptDoc
            // 
            this.ptDoc.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(this.Print_Page);
            // 
            // btn_Print
            // 
            this.btn_Print.Font = new System.Drawing.Font("Microsoft JhengHei", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btn_Print.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.btn_Print.Image = ((System.Drawing.Image)(resources.GetObject("btn_Print.Image")));
            this.btn_Print.Location = new System.Drawing.Point(830, 85);
            this.btn_Print.Name = "btn_Print";
            this.btn_Print.Size = new System.Drawing.Size(136, 48);
            this.btn_Print.TabIndex = 8;
            this.btn_Print.Text = "列印文件←";
            this.btn_Print.UseVisualStyleBackColor = true;
            this.btn_Print.Click += new System.EventHandler(this.btn_Print_Click);
            // 
            // tbx_allReciver
            // 
            this.tbx_allReciver.Location = new System.Drawing.Point(712, 38);
            this.tbx_allReciver.Name = "tbx_allReciver";
            this.tbx_allReciver.Size = new System.Drawing.Size(254, 25);
            this.tbx_allReciver.TabIndex = 9;
            // 
            // lbl_Reciver
            // 
            this.lbl_Reciver.AutoSize = true;
            this.lbl_Reciver.Font = new System.Drawing.Font("MingLiU", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_Reciver.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.lbl_Reciver.Location = new System.Drawing.Point(783, 15);
            this.lbl_Reciver.Name = "lbl_Reciver";
            this.lbl_Reciver.Size = new System.Drawing.Size(126, 20);
            this.lbl_Reciver.TabIndex = 10;
            this.lbl_Reciver.Text = "ERP傳送字串";
            // 
            // ReadTxt
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Green;
            this.ClientSize = new System.Drawing.Size(978, 573);
            this.Controls.Add(this.lbl_Reciver);
            this.Controls.Add(this.tbx_allReciver);
            this.Controls.Add(this.btn_Print);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pic_Img);
            this.Controls.Add(this.tbx_FilePath);
            this.Controls.Add(this.lbx_txtinfo);
            this.Controls.Add(this.btn_open);
            this.Controls.Add(this.lbl_readTxt);
            this.Controls.Add(this.tbx_ReadALL);
            this.Controls.Add(this.btn_ReadTxt);
            this.Name = "ReadTxt";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.Text = "ReadTxtForm";
            this.Load += new System.EventHandler(this.ReadTxt_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pic_Img)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn_ReadTxt;
        private System.Windows.Forms.TextBox tbx_ReadALL;
        private System.Windows.Forms.Label lbl_readTxt;
        private System.Windows.Forms.FolderBrowserDialog fbd_ReadTXT;
        private System.Windows.Forms.Button btn_open;
        private System.Windows.Forms.ListBox lbx_txtinfo;
        private System.Windows.Forms.TextBox tbx_FilePath;
        private System.Windows.Forms.PictureBox pic_Img;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.PrintPreviewDialog printPreviewDialog1;
        private System.Drawing.Printing.PrintDocument ptDoc;
        private System.Windows.Forms.Button btn_Print;
        private System.Windows.Forms.TextBox tbx_allReciver;
        private System.Windows.Forms.Label lbl_Reciver;
    }
}

