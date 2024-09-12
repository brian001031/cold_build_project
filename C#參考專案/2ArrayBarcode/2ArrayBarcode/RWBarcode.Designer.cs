namespace _2ArrayBarcode
{
    partial class RWBarcode
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RWBarcode));
            this.QRCode = new System.Windows.Forms.Button();
            this.TxtSource = new System.Windows.Forms.TextBox();
            this.pic_Img1 = new System.Windows.Forms.PictureBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btn_Read = new System.Windows.Forms.Button();
            this.tbx_ReadStr = new System.Windows.Forms.TextBox();
            this.lbl_QrCodeImg = new System.Windows.Forms.Label();
            this.webSign = new System.Windows.Forms.WebBrowser();
            this.btn_GetXML = new System.Windows.Forms.Button();
            this.btn_SignOut = new System.Windows.Forms.Button();
            this.tbx_Xml = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.pic_Img1)).BeginInit();
            this.SuspendLayout();
            // 
            // QRCode
            // 
            this.QRCode.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.QRCode.Font = new System.Drawing.Font("PMingLiU", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.QRCode.Location = new System.Drawing.Point(680, 41);
            this.QRCode.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.QRCode.Name = "QRCode";
            this.QRCode.Size = new System.Drawing.Size(173, 62);
            this.QRCode.TabIndex = 0;
            this.QRCode.Text = "QRCreate";
            this.QRCode.UseVisualStyleBackColor = false;
            this.QRCode.Click += new System.EventHandler(this.QRCode_Click);
            // 
            // TxtSource
            // 
            this.TxtSource.Location = new System.Drawing.Point(51, 65);
            this.TxtSource.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.TxtSource.Name = "TxtSource";
            this.TxtSource.Size = new System.Drawing.Size(620, 25);
            this.TxtSource.TabIndex = 1;
            // 
            // pic_Img1
            // 
            this.pic_Img1.Location = new System.Drawing.Point(1219, 15);
            this.pic_Img1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.pic_Img1.Name = "pic_Img1";
            this.pic_Img1.Size = new System.Drawing.Size(284, 255);
            this.pic_Img1.TabIndex = 3;
            this.pic_Img1.TabStop = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("DFKai-SB", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label2.ForeColor = System.Drawing.SystemColors.MenuHighlight;
            this.label2.Location = new System.Drawing.Point(48, 34);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(219, 30);
            this.label2.TabIndex = 5;
            this.label2.Text = "Content(內容)";
            // 
            // btn_Read
            // 
            this.btn_Read.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.btn_Read.Font = new System.Drawing.Font("PMingLiU", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btn_Read.Location = new System.Drawing.Point(769, 504);
            this.btn_Read.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btn_Read.Name = "btn_Read";
            this.btn_Read.Size = new System.Drawing.Size(173, 62);
            this.btn_Read.TabIndex = 6;
            this.btn_Read.Text = "QR READ";
            this.btn_Read.UseVisualStyleBackColor = false;
            this.btn_Read.Click += new System.EventHandler(this.btn_Read_Click);
            // 
            // tbx_ReadStr
            // 
            this.tbx_ReadStr.Location = new System.Drawing.Point(945, 364);
            this.tbx_ReadStr.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tbx_ReadStr.Name = "tbx_ReadStr";
            this.tbx_ReadStr.Size = new System.Drawing.Size(556, 25);
            this.tbx_ReadStr.TabIndex = 7;
            // 
            // lbl_QrCodeImg
            // 
            this.lbl_QrCodeImg.AutoSize = true;
            this.lbl_QrCodeImg.Font = new System.Drawing.Font("DFKai-SB", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_QrCodeImg.Location = new System.Drawing.Point(1316, 292);
            this.lbl_QrCodeImg.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbl_QrCodeImg.Name = "lbl_QrCodeImg";
            this.lbl_QrCodeImg.Size = new System.Drawing.Size(125, 30);
            this.lbl_QrCodeImg.TabIndex = 9;
            this.lbl_QrCodeImg.Text = "2D Code";
            // 
            // webSign
            // 
            this.webSign.Location = new System.Drawing.Point(51, 399);
            this.webSign.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.webSign.MinimumSize = new System.Drawing.Size(27, 25);
            this.webSign.Name = "webSign";
            this.webSign.Size = new System.Drawing.Size(688, 371);
            this.webSign.TabIndex = 10;
            this.webSign.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.webSign_DocumentCompleted);
            // 
            // btn_GetXML
            // 
            this.btn_GetXML.Font = new System.Drawing.Font("DFKai-SB", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btn_GetXML.Location = new System.Drawing.Point(1380, 424);
            this.btn_GetXML.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btn_GetXML.Name = "btn_GetXML";
            this.btn_GetXML.Size = new System.Drawing.Size(123, 42);
            this.btn_GetXML.TabIndex = 11;
            this.btn_GetXML.Text = "GeXML";
            this.btn_GetXML.UseVisualStyleBackColor = true;
            this.btn_GetXML.Click += new System.EventHandler(this.btn_GetXML_Click);
            // 
            // btn_SignOut
            // 
            this.btn_SignOut.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.btn_SignOut.Font = new System.Drawing.Font("PMingLiU", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btn_SignOut.Location = new System.Drawing.Point(769, 652);
            this.btn_SignOut.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btn_SignOut.Name = "btn_SignOut";
            this.btn_SignOut.Size = new System.Drawing.Size(173, 62);
            this.btn_SignOut.TabIndex = 12;
            this.btn_SignOut.Text = "登出網頁";
            this.btn_SignOut.UseVisualStyleBackColor = false;
            this.btn_SignOut.Visible = false;
            this.btn_SignOut.Click += new System.EventHandler(this.btn_SignOut_Click);
            // 
            // tbx_Xml
            // 
            this.tbx_Xml.Location = new System.Drawing.Point(1024, 474);
            this.tbx_Xml.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tbx_Xml.Multiline = true;
            this.tbx_Xml.Name = "tbx_Xml";
            this.tbx_Xml.Size = new System.Drawing.Size(477, 272);
            this.tbx_Xml.TabIndex = 13;
            // 
            // RWBarcode
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackgroundImage = global::_2ArrayBarcode.Properties.Resources.條碼機掃描1;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.ClientSize = new System.Drawing.Size(1274, 785);
            this.Controls.Add(this.tbx_Xml);
            this.Controls.Add(this.btn_SignOut);
            this.Controls.Add(this.btn_GetXML);
            this.Controls.Add(this.webSign);
            this.Controls.Add(this.lbl_QrCodeImg);
            this.Controls.Add(this.tbx_ReadStr);
            this.Controls.Add(this.btn_Read);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.pic_Img1);
            this.Controls.Add(this.TxtSource);
            this.Controls.Add(this.QRCode);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "RWBarcode";
            this.Text = "Barcode-R/W";
            this.Load += new System.EventHandler(this.RWBarcode_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pic_Img1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button QRCode;
        private System.Windows.Forms.TextBox TxtSource;
        private System.Windows.Forms.PictureBox pic_Img1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btn_Read;
        private System.Windows.Forms.TextBox tbx_ReadStr;
        private System.Windows.Forms.Label lbl_QrCodeImg;
        private System.Windows.Forms.WebBrowser webSign;
        private System.Windows.Forms.Button btn_GetXML;
        private System.Windows.Forms.Button btn_SignOut;
        private System.Windows.Forms.TextBox tbx_Xml;
    }
}

