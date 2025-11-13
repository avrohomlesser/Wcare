namespace ICR2
{
    partial class ManageDoctorsEmail
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
            this.grpSettings = new System.Windows.Forms.GroupBox();
            this.txtSubject = new System.Windows.Forms.TextBox();
            this.txtHeader = new System.Windows.Forms.TextBox();
            this.txtGreeting = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.txtEmail = new System.Windows.Forms.TextBox();
            this.txtSMTP = new System.Windows.Forms.TextBox();
            this.chkTest = new System.Windows.Forms.CheckBox();
            this.lblPort = new System.Windows.Forms.Label();
            this.lblPass = new System.Windows.Forms.Label();
            this.lbltext = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.grpTest = new System.Windows.Forms.GroupBox();
            this.txtEmailAmount = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtTestEmail = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cmdTest = new System.Windows.Forms.Button();
            this.cmdEdit = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.cmdSave = new System.Windows.Forms.Button();
            this.grpSettings.SuspendLayout();
            this.grpTest.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpSettings
            // 
            this.grpSettings.Controls.Add(this.txtSubject);
            this.grpSettings.Controls.Add(this.txtHeader);
            this.grpSettings.Controls.Add(this.txtGreeting);
            this.grpSettings.Controls.Add(this.label4);
            this.grpSettings.Controls.Add(this.label5);
            this.grpSettings.Controls.Add(this.label6);
            this.grpSettings.Controls.Add(this.txtPort);
            this.grpSettings.Controls.Add(this.txtPassword);
            this.grpSettings.Controls.Add(this.txtEmail);
            this.grpSettings.Controls.Add(this.txtSMTP);
            this.grpSettings.Controls.Add(this.chkTest);
            this.grpSettings.Controls.Add(this.lblPort);
            this.grpSettings.Controls.Add(this.lblPass);
            this.grpSettings.Controls.Add(this.lbltext);
            this.grpSettings.Controls.Add(this.label1);
            this.grpSettings.Enabled = false;
            this.grpSettings.Location = new System.Drawing.Point(12, 12);
            this.grpSettings.Name = "grpSettings";
            this.grpSettings.Size = new System.Drawing.Size(285, 259);
            this.grpSettings.TabIndex = 0;
            this.grpSettings.TabStop = false;
            // 
            // txtSubject
            // 
            this.txtSubject.Location = new System.Drawing.Point(142, 153);
            this.txtSubject.Name = "txtSubject";
            this.txtSubject.Size = new System.Drawing.Size(128, 20);
            this.txtSubject.TabIndex = 16;
            this.txtSubject.TextChanged += new System.EventHandler(this.validate_text);
            // 
            // txtHeader
            // 
            this.txtHeader.Location = new System.Drawing.Point(142, 180);
            this.txtHeader.Multiline = true;
            this.txtHeader.Name = "txtHeader";
            this.txtHeader.Size = new System.Drawing.Size(128, 46);
            this.txtHeader.TabIndex = 15;
            this.txtHeader.TextChanged += new System.EventHandler(this.validate_text);
            // 
            // txtGreeting
            // 
            this.txtGreeting.Location = new System.Drawing.Point(142, 124);
            this.txtGreeting.Name = "txtGreeting";
            this.txtGreeting.Size = new System.Drawing.Size(128, 20);
            this.txtGreeting.TabIndex = 14;
            this.txtGreeting.TextChanged += new System.EventHandler(this.validate_text);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.label4.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.label4.Location = new System.Drawing.Point(76, 153);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(55, 17);
            this.label4.TabIndex = 13;
            this.label4.Text = "Subject";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.label5.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.label5.Location = new System.Drawing.Point(45, 180);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(86, 17);
            this.label5.TabIndex = 12;
            this.label5.Text = "Header Text";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.label6.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.label6.Location = new System.Drawing.Point(68, 124);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(63, 17);
            this.label6.TabIndex = 11;
            this.label6.Text = "Greeting";
            // 
            // txtPort
            // 
            this.txtPort.Location = new System.Drawing.Point(142, 98);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(29, 20);
            this.txtPort.TabIndex = 9;
            this.txtPort.TextChanged += new System.EventHandler(this.validate_text);
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(142, 72);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new System.Drawing.Size(128, 20);
            this.txtPassword.TabIndex = 8;
            this.txtPassword.TextChanged += new System.EventHandler(this.validate_text);
            // 
            // txtEmail
            // 
            this.txtEmail.Location = new System.Drawing.Point(142, 45);
            this.txtEmail.Name = "txtEmail";
            this.txtEmail.Size = new System.Drawing.Size(128, 20);
            this.txtEmail.TabIndex = 7;
            this.txtEmail.TextChanged += new System.EventHandler(this.validate_text);
            // 
            // txtSMTP
            // 
            this.txtSMTP.Location = new System.Drawing.Point(142, 19);
            this.txtSMTP.Name = "txtSMTP";
            this.txtSMTP.Size = new System.Drawing.Size(128, 20);
            this.txtSMTP.TabIndex = 6;
            this.txtSMTP.TextChanged += new System.EventHandler(this.validate_text);
            // 
            // chkTest
            // 
            this.chkTest.AutoSize = true;
            this.chkTest.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkTest.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.chkTest.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.chkTest.Location = new System.Drawing.Point(55, 232);
            this.chkTest.Name = "chkTest";
            this.chkTest.Size = new System.Drawing.Size(102, 21);
            this.chkTest.TabIndex = 5;
            this.chkTest.Text = "Test Mode  ";
            this.chkTest.UseVisualStyleBackColor = true;
            this.chkTest.CheckedChanged += new System.EventHandler(this.chkTest_CheckedChanged);
            // 
            // lblPort
            // 
            this.lblPort.AutoSize = true;
            this.lblPort.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.lblPort.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.lblPort.Location = new System.Drawing.Point(97, 98);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(34, 17);
            this.lblPort.TabIndex = 3;
            this.lblPort.Text = "Port";
            // 
            // lblPass
            // 
            this.lblPass.AutoSize = true;
            this.lblPass.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.lblPass.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.lblPass.Location = new System.Drawing.Point(62, 72);
            this.lblPass.Name = "lblPass";
            this.lblPass.Size = new System.Drawing.Size(69, 17);
            this.lblPass.TabIndex = 2;
            this.lblPass.Text = "Password";
            // 
            // lbltext
            // 
            this.lbltext.AutoSize = true;
            this.lbltext.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.lbltext.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.lbltext.Location = new System.Drawing.Point(33, 45);
            this.lbltext.Name = "lbltext";
            this.lbltext.Size = new System.Drawing.Size(98, 17);
            this.lbltext.TabIndex = 1;
            this.lbltext.Text = "Email Address";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.label1.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.label1.Location = new System.Drawing.Point(39, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(92, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "SMTP Server";
            // 
            // grpTest
            // 
            this.grpTest.Controls.Add(this.txtEmailAmount);
            this.grpTest.Controls.Add(this.label3);
            this.grpTest.Controls.Add(this.txtTestEmail);
            this.grpTest.Controls.Add(this.label2);
            this.grpTest.Enabled = false;
            this.grpTest.Location = new System.Drawing.Point(12, 277);
            this.grpTest.Name = "grpTest";
            this.grpTest.Size = new System.Drawing.Size(285, 100);
            this.grpTest.TabIndex = 1;
            this.grpTest.TabStop = false;
            // 
            // txtEmailAmount
            // 
            this.txtEmailAmount.Location = new System.Drawing.Point(142, 59);
            this.txtEmailAmount.Name = "txtEmailAmount";
            this.txtEmailAmount.Size = new System.Drawing.Size(29, 20);
            this.txtEmailAmount.TabIndex = 10;
            this.txtEmailAmount.TextChanged += new System.EventHandler(this.validate_text);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.label3.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.label3.Location = new System.Drawing.Point(6, 59);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(117, 17);
            this.label3.TabIndex = 9;
            this.label3.Text = "Amount of Emails";
            // 
            // txtTestEmail
            // 
            this.txtTestEmail.Location = new System.Drawing.Point(142, 25);
            this.txtTestEmail.Name = "txtTestEmail";
            this.txtTestEmail.Size = new System.Drawing.Size(128, 20);
            this.txtTestEmail.TabIndex = 8;
            this.txtTestEmail.TextChanged += new System.EventHandler(this.validate_text);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.label2.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.label2.Location = new System.Drawing.Point(6, 25);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(130, 17);
            this.label2.TabIndex = 2;
            this.label2.Text = "Test Email Address";
            // 
            // cmdTest
            // 
            this.cmdTest.BackColor = System.Drawing.SystemColors.Control;
            this.cmdTest.ForeColor = System.Drawing.SystemColors.ControlText;
            this.cmdTest.Location = new System.Drawing.Point(190, 110);
            this.cmdTest.Name = "cmdTest";
            this.cmdTest.Size = new System.Drawing.Size(93, 20);
            this.cmdTest.TabIndex = 11;
            this.cmdTest.Text = "Test Connection";
            this.cmdTest.UseVisualStyleBackColor = false;
            this.cmdTest.Click += new System.EventHandler(this.cmdTest_Click);
            // 
            // cmdEdit
            // 
            this.cmdEdit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.cmdEdit.ForeColor = System.Drawing.Color.White;
            this.cmdEdit.Location = new System.Drawing.Point(48, 392);
            this.cmdEdit.Name = "cmdEdit";
            this.cmdEdit.Size = new System.Drawing.Size(69, 26);
            this.cmdEdit.TabIndex = 12;
            this.cmdEdit.Text = "Edit";
            this.cmdEdit.UseVisualStyleBackColor = false;
            this.cmdEdit.Click += new System.EventHandler(this.cmdEdit_Click);
            // 
            // cmdCancel
            // 
            this.cmdCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.cmdCancel.Enabled = false;
            this.cmdCancel.ForeColor = System.Drawing.Color.White;
            this.cmdCancel.Location = new System.Drawing.Point(127, 392);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(69, 26);
            this.cmdCancel.TabIndex = 13;
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = false;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // cmdSave
            // 
            this.cmdSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.cmdSave.Enabled = false;
            this.cmdSave.ForeColor = System.Drawing.Color.White;
            this.cmdSave.Location = new System.Drawing.Point(202, 392);
            this.cmdSave.Name = "cmdSave";
            this.cmdSave.Size = new System.Drawing.Size(69, 26);
            this.cmdSave.TabIndex = 14;
            this.cmdSave.Text = "Save";
            this.cmdSave.UseVisualStyleBackColor = false;
            this.cmdSave.Click += new System.EventHandler(this.cmdSave_Click);
            // 
            // ManageDoctorsEmail
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(329, 479);
            this.Controls.Add(this.cmdTest);
            this.Controls.Add(this.cmdSave);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.cmdEdit);
            this.Controls.Add(this.grpTest);
            this.Controls.Add(this.grpSettings);
            this.Name = "ManageDoctorsEmail";
            this.Text = "Manage Doctors Emails";
            this.Load += new System.EventHandler(this.ManageDoctorsEmail_Load);
            this.grpSettings.ResumeLayout(false);
            this.grpSettings.PerformLayout();
            this.grpTest.ResumeLayout(false);
            this.grpTest.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grpSettings;
        private System.Windows.Forms.Label lblPort;
        private System.Windows.Forms.Label lblPass;
        private System.Windows.Forms.Label lbltext;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox grpTest;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.TextBox txtEmail;
        private System.Windows.Forms.TextBox txtSMTP;
        private System.Windows.Forms.CheckBox chkTest;
        private System.Windows.Forms.TextBox txtEmailAmount;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtTestEmail;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtSubject;
        private System.Windows.Forms.TextBox txtHeader;
        private System.Windows.Forms.TextBox txtGreeting;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button cmdTest;
        private System.Windows.Forms.Button cmdEdit;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Button cmdSave;
    }
}