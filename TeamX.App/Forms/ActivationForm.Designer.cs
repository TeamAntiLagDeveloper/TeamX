namespace TeamX.App.Forms
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            lblTitle = new Label();
            lblSubtitle = new Label();
            lblLicense = new Label();
            txtLicenseKey = new TextBox();
            btnActivate = new Button();
            lblStatus = new Label();
            txtHardwareId = new Label();
            txtWindowsVersion = new Label();
            txtComputerName = new Label();
            txtIpAddress = new Label();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI Black", 21.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblTitle.Location = new Point(204, 29);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(130, 40);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "TEAM X";
            // 
            // lblSubtitle
            // 
            lblSubtitle.AutoSize = true;
            lblSubtitle.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblSubtitle.Location = new Point(160, 69);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(219, 20);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "Ative sua licença para continuar";
            // 
            // lblLicense
            // 
            lblLicense.AutoSize = true;
            lblLicense.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblLicense.Location = new Point(8, 118);
            lblLicense.Name = "lblLicense";
            lblLicense.Size = new Size(120, 20);
            lblLicense.TabIndex = 2;
            lblLicense.Text = "Chave da licença";
            // 
            // txtLicenseKey
            // 
            txtLicenseKey.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtLicenseKey.Location = new Point(134, 115);
            txtLicenseKey.Name = "txtLicenseKey";
            txtLicenseKey.Size = new Size(350, 29);
            txtLicenseKey.TabIndex = 3;
            // 
            // btnActivate
            // 
            btnActivate.Location = new Point(490, 115);
            btnActivate.Name = "btnActivate";
            btnActivate.Size = new Size(89, 29);
            btnActivate.TabIndex = 4;
            btnActivate.Text = "Ativar Licença";
            btnActivate.UseVisualStyleBackColor = true;
            btnActivate.Click += btnActivate_Click;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblStatus.ForeColor = Color.Gray;
            lblStatus.Location = new Point(134, 157);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(21, 20);
            lblStatus.TabIndex = 5;
            lblStatus.Text = "**";
            // 
            // txtHardwareId
            // 
            txtHardwareId.AutoSize = true;
            txtHardwareId.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtHardwareId.Location = new Point(12, 198);
            txtHardwareId.Name = "txtHardwareId";
            txtHardwareId.Size = new Size(120, 20);
            txtHardwareId.TabIndex = 7;
            txtHardwareId.Text = "Chave da licença";
            // 
            // txtWindowsVersion
            // 
            txtWindowsVersion.AutoSize = true;
            txtWindowsVersion.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtWindowsVersion.Location = new Point(12, 256);
            txtWindowsVersion.Name = "txtWindowsVersion";
            txtWindowsVersion.Size = new Size(120, 20);
            txtWindowsVersion.TabIndex = 8;
            txtWindowsVersion.Text = "Chave da licença";
            // 
            // txtComputerName
            // 
            txtComputerName.AutoSize = true;
            txtComputerName.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtComputerName.Location = new Point(12, 226);
            txtComputerName.Name = "txtComputerName";
            txtComputerName.Size = new Size(120, 20);
            txtComputerName.TabIndex = 9;
            txtComputerName.Text = "Chave da licença";
            // 
            // txtIpAddress
            // 
            txtIpAddress.AutoSize = true;
            txtIpAddress.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtIpAddress.Location = new Point(8, 282);
            txtIpAddress.Name = "txtIpAddress";
            txtIpAddress.Size = new Size(120, 20);
            txtIpAddress.TabIndex = 10;
            txtIpAddress.Text = "Chave da licença";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(584, 311);
            Controls.Add(txtIpAddress);
            Controls.Add(txtComputerName);
            Controls.Add(txtWindowsVersion);
            Controls.Add(txtHardwareId);
            Controls.Add(lblStatus);
            Controls.Add(btnActivate);
            Controls.Add(txtLicenseKey);
            Controls.Add(lblLicense);
            Controls.Add(lblSubtitle);
            Controls.Add(lblTitle);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "MainForm";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "TeamX Activation";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblTitle;
        private Label lblSubtitle;
        private Label lblLicense;
        private TextBox txtLicenseKey;
        private Button btnActivate;
        private Label lblStatus;
        private Label txtHardwareId;
        private Label txtWindowsVersion;
        private Label txtComputerName;
        private Label txtIpAddress;
    }
}