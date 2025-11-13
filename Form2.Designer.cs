namespace ICR
{
    partial class frmCal
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
            this.label1 = new System.Windows.Forms.Label();
            this.lblTo = new System.Windows.Forms.Label();
            this.txtFrom = new System.Windows.Forms.DateTimePicker();
            this.txtTo = new System.Windows.Forms.DateTimePicker();
            this.cmdOk = new System.Windows.Forms.Button();
            this.grpSpan = new System.Windows.Forms.GroupBox();
            this.chkToDate = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.chkSubsequent = new System.Windows.Forms.CheckBox();
            this.cboAmount = new System.Windows.Forms.ComboBox();
            this.cboSpan = new System.Windows.Forms.ComboBox();
            this.pnlFilter = new System.Windows.Forms.Panel();
            this.pnlColumns = new System.Windows.Forms.FlowLayoutPanel();
            this.chkSelectAll = new System.Windows.Forms.CheckBox();
            this.chkDeselectAll = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.chkColumns = new System.Windows.Forms.CheckedListBox();
            this.lblFilter = new System.Windows.Forms.Label();
            this.lblColumns = new System.Windows.Forms.Label();
            this.pnlQuery = new System.Windows.Forms.Panel();
            this.chkFilter = new System.Windows.Forms.CheckedListBox();
            this.calFilter = new System.Windows.Forms.DateTimePicker();
            this.grpOperator = new System.Windows.Forms.GroupBox();
            this.optLess = new System.Windows.Forms.RadioButton();
            this.optGreater = new System.Windows.Forms.RadioButton();
            this.optEquals = new System.Windows.Forms.RadioButton();
            this.cmdAdd = new System.Windows.Forms.Button();
            this.cmdClear = new System.Windows.Forms.Button();
            this.txtCompare = new System.Windows.Forms.TextBox();
            this.cboFields = new System.Windows.Forms.ComboBox();
            this.txtQuery = new System.Windows.Forms.TextBox();
            this.txtcptcodes = new System.Windows.Forms.TextBox();
            this.lblCptLines = new System.Windows.Forms.Label();
            this.cboRange = new System.Windows.Forms.ComboBox();
            this.grpSpan.SuspendLayout();
            this.pnlFilter.SuspendLayout();
            this.pnlColumns.SuspendLayout();
            this.pnlQuery.SuspendLayout();
            this.grpOperator.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.label1.Location = new System.Drawing.Point(21, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(40, 17);
            this.label1.TabIndex = 19;
            this.label1.Text = "From";
            // 
            // lblTo
            // 
            this.lblTo.AutoSize = true;
            this.lblTo.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.lblTo.Location = new System.Drawing.Point(31, 50);
            this.lblTo.Name = "lblTo";
            this.lblTo.Size = new System.Drawing.Size(25, 17);
            this.lblTo.TabIndex = 20;
            this.lblTo.Text = "To";
            // 
            // txtFrom
            // 
            this.txtFrom.AllowDrop = true;
            this.txtFrom.Checked = false;
            this.txtFrom.CustomFormat = "MM/dd/yyyy";
            this.txtFrom.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.txtFrom.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.txtFrom.Location = new System.Drawing.Point(78, 21);
            this.txtFrom.Name = "txtFrom";
            this.txtFrom.Size = new System.Drawing.Size(100, 23);
            this.txtFrom.TabIndex = 17;
            this.txtFrom.ValueChanged += new System.EventHandler(this.txtFrom_ValueChanged);
            // 
            // txtTo
            // 
            this.txtTo.AllowDrop = true;
            this.txtTo.Checked = false;
            this.txtTo.CustomFormat = "MM/dd/yyyy";
            this.txtTo.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.txtTo.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.txtTo.Location = new System.Drawing.Point(79, 50);
            this.txtTo.Name = "txtTo";
            this.txtTo.Size = new System.Drawing.Size(100, 23);
            this.txtTo.TabIndex = 18;
            this.txtTo.ValueChanged += new System.EventHandler(this.txtTo_ValueChanged);
            // 
            // cmdOk
            // 
            this.cmdOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.cmdOk.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.cmdOk.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdOk.ForeColor = System.Drawing.Color.White;
            this.cmdOk.Location = new System.Drawing.Point(63, 815);
            this.cmdOk.Name = "cmdOk";
            this.cmdOk.Size = new System.Drawing.Size(64, 30);
            this.cmdOk.TabIndex = 21;
            this.cmdOk.Text = "OK";
            this.cmdOk.UseVisualStyleBackColor = false;
            this.cmdOk.Click += new System.EventHandler(this.cmdOk_Click);
            // 
            // grpSpan
            // 
            this.grpSpan.Controls.Add(this.chkToDate);
            this.grpSpan.Controls.Add(this.label3);
            this.grpSpan.Controls.Add(this.label2);
            this.grpSpan.Controls.Add(this.chkSubsequent);
            this.grpSpan.Controls.Add(this.cboAmount);
            this.grpSpan.Controls.Add(this.cboSpan);
            this.grpSpan.Location = new System.Drawing.Point(15, 108);
            this.grpSpan.Name = "grpSpan";
            this.grpSpan.Size = new System.Drawing.Size(173, 103);
            this.grpSpan.TabIndex = 35;
            this.grpSpan.TabStop = false;
            // 
            // chkToDate
            // 
            this.chkToDate.AutoSize = true;
            this.chkToDate.Enabled = false;
            this.chkToDate.Location = new System.Drawing.Point(9, 80);
            this.chkToDate.Name = "chkToDate";
            this.chkToDate.Size = new System.Drawing.Size(60, 17);
            this.chkToDate.TabIndex = 40;
            this.chkToDate.Text = "Todate";
            this.chkToDate.UseVisualStyleBackColor = true;
            this.chkToDate.CheckedChanged += new System.EventHandler(this.cboAmount_SelectionChangeCommitted);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.label3.Location = new System.Drawing.Point(88, 16);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(56, 17);
            this.label3.TabIndex = 39;
            this.label3.Text = "Amount";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.label2.Location = new System.Drawing.Point(6, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 17);
            this.label2.TabIndex = 38;
            this.label2.Text = "Span";
            // 
            // chkSubsequent
            // 
            this.chkSubsequent.AutoSize = true;
            this.chkSubsequent.Location = new System.Drawing.Point(9, 63);
            this.chkSubsequent.Name = "chkSubsequent";
            this.chkSubsequent.Size = new System.Drawing.Size(121, 17);
            this.chkSubsequent.TabIndex = 37;
            this.chkSubsequent.Text = "Include Subsequent";
            this.chkSubsequent.UseVisualStyleBackColor = true;
            this.chkSubsequent.CheckedChanged += new System.EventHandler(this.cboAmount_SelectionChangeCommitted);
            // 
            // cboAmount
            // 
            this.cboAmount.FormattingEnabled = true;
            this.cboAmount.Items.AddRange(new object[] {
            "Days",
            "Weeks",
            "Months",
            "Quaters",
            "Years"});
            this.cboAmount.Location = new System.Drawing.Point(91, 36);
            this.cboAmount.Name = "cboAmount";
            this.cboAmount.Size = new System.Drawing.Size(72, 21);
            this.cboAmount.TabIndex = 36;
            this.cboAmount.SelectedIndexChanged += new System.EventHandler(this.cboAmount_SelectionChangeCommitted);
            // 
            // cboSpan
            // 
            this.cboSpan.FormattingEnabled = true;
            this.cboSpan.Items.AddRange(new object[] {
            "Days",
            "Weeks",
            "Months",
            "Years"});
            this.cboSpan.Location = new System.Drawing.Point(9, 36);
            this.cboSpan.Name = "cboSpan";
            this.cboSpan.Size = new System.Drawing.Size(59, 21);
            this.cboSpan.TabIndex = 35;
            this.cboSpan.SelectionChangeCommitted += new System.EventHandler(this.cboSpan_SelectedIndexChanged);
            // 
            // pnlFilter
            // 
            this.pnlFilter.AutoSize = true;
            this.pnlFilter.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlFilter.Controls.Add(this.pnlColumns);
            this.pnlFilter.Controls.Add(this.lblFilter);
            this.pnlFilter.Controls.Add(this.lblColumns);
            this.pnlFilter.Controls.Add(this.pnlQuery);
            this.pnlFilter.Location = new System.Drawing.Point(6, 217);
            this.pnlFilter.Name = "pnlFilter";
            this.pnlFilter.Size = new System.Drawing.Size(188, 599);
            this.pnlFilter.TabIndex = 38;
            this.pnlFilter.Visible = false;
            this.pnlFilter.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlFilter_Paint);
            // 
            // pnlColumns
            // 
            this.pnlColumns.AutoSize = true;
            this.pnlColumns.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlColumns.Controls.Add(this.chkSelectAll);
            this.pnlColumns.Controls.Add(this.chkDeselectAll);
            this.pnlColumns.Controls.Add(this.label4);
            this.pnlColumns.Controls.Add(this.chkColumns);
            this.pnlColumns.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.pnlColumns.Location = new System.Drawing.Point(12, 28);
            this.pnlColumns.Name = "pnlColumns";
            this.pnlColumns.Size = new System.Drawing.Size(164, 259);
            this.pnlColumns.TabIndex = 54;
            this.pnlColumns.Visible = false;
            // 
            // chkSelectAll
            // 
            this.chkSelectAll.AutoSize = true;
            this.chkSelectAll.Location = new System.Drawing.Point(3, 3);
            this.chkSelectAll.Name = "chkSelectAll";
            this.chkSelectAll.Size = new System.Drawing.Size(70, 17);
            this.chkSelectAll.TabIndex = 49;
            this.chkSelectAll.Text = "Select All";
            this.chkSelectAll.UseVisualStyleBackColor = true;
            this.chkSelectAll.CheckedChanged += new System.EventHandler(this.chkSelectAll_CheckedChanged);
            // 
            // chkDeselectAll
            // 
            this.chkDeselectAll.AutoSize = true;
            this.chkDeselectAll.Location = new System.Drawing.Point(3, 26);
            this.chkDeselectAll.Name = "chkDeselectAll";
            this.chkDeselectAll.Size = new System.Drawing.Size(87, 17);
            this.chkDeselectAll.TabIndex = 50;
            this.chkDeselectAll.Text = "De Select All";
            this.chkDeselectAll.UseVisualStyleBackColor = true;
            this.chkDeselectAll.CheckedChanged += new System.EventHandler(this.chkDeselectAll_CheckedChanged);
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(3, 46);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(86, 8);
            this.label4.TabIndex = 47;
            // 
            // chkColumns
            // 
            this.chkColumns.CheckOnClick = true;
            this.chkColumns.FormattingEnabled = true;
            this.chkColumns.Location = new System.Drawing.Point(3, 57);
            this.chkColumns.Name = "chkColumns";
            this.chkColumns.Size = new System.Drawing.Size(158, 199);
            this.chkColumns.TabIndex = 48;
            this.chkColumns.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.chkColumns_ItemCheck);
            // 
            // lblFilter
            // 
            this.lblFilter.AutoSize = true;
            this.lblFilter.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblFilter.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.lblFilter.ForeColor = System.Drawing.Color.Navy;
            this.lblFilter.Location = new System.Drawing.Point(11, 290);
            this.lblFilter.Name = "lblFilter";
            this.lblFilter.Size = new System.Drawing.Size(39, 17);
            this.lblFilter.TabIndex = 56;
            this.lblFilter.Text = "Filter";
            this.lblFilter.Click += new System.EventHandler(this.lblFilter_Click);
            // 
            // lblColumns
            // 
            this.lblColumns.AutoSize = true;
            this.lblColumns.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblColumns.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.lblColumns.ForeColor = System.Drawing.Color.Navy;
            this.lblColumns.Location = new System.Drawing.Point(12, 6);
            this.lblColumns.Name = "lblColumns";
            this.lblColumns.Size = new System.Drawing.Size(62, 17);
            this.lblColumns.TabIndex = 59;
            this.lblColumns.Text = "Columns";
            this.lblColumns.Click += new System.EventHandler(this.lblColumns_Click);
            // 
            // pnlQuery
            // 
            this.pnlQuery.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlQuery.Controls.Add(this.chkFilter);
            this.pnlQuery.Controls.Add(this.calFilter);
            this.pnlQuery.Controls.Add(this.grpOperator);
            this.pnlQuery.Controls.Add(this.cmdAdd);
            this.pnlQuery.Controls.Add(this.cmdClear);
            this.pnlQuery.Controls.Add(this.txtCompare);
            this.pnlQuery.Controls.Add(this.cboFields);
            this.pnlQuery.Controls.Add(this.txtQuery);
            this.pnlQuery.Location = new System.Drawing.Point(9, 310);
            this.pnlQuery.Name = "pnlQuery";
            this.pnlQuery.Size = new System.Drawing.Size(176, 286);
            this.pnlQuery.TabIndex = 58;
            this.pnlQuery.Visible = false;
            // 
            // chkFilter
            // 
            this.chkFilter.CheckOnClick = true;
            this.chkFilter.FormattingEnabled = true;
            this.chkFilter.Location = new System.Drawing.Point(6, 53);
            this.chkFilter.Name = "chkFilter";
            this.chkFilter.Size = new System.Drawing.Size(160, 94);
            this.chkFilter.TabIndex = 68;
            this.chkFilter.Visible = false;
            this.chkFilter.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.chkFilter_ItemCheck);
            // 
            // calFilter
            // 
            this.calFilter.AllowDrop = true;
            this.calFilter.Checked = false;
            this.calFilter.CustomFormat = "MM/dd/yyyy";
            this.calFilter.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.calFilter.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.calFilter.Location = new System.Drawing.Point(6, 155);
            this.calFilter.Name = "calFilter";
            this.calFilter.Size = new System.Drawing.Size(161, 23);
            this.calFilter.TabIndex = 67;
            this.calFilter.Visible = false;
            this.calFilter.ValueChanged += new System.EventHandler(this.calFilter_ValueChanged);
            // 
            // grpOperator
            // 
            this.grpOperator.Controls.Add(this.optLess);
            this.grpOperator.Controls.Add(this.optGreater);
            this.grpOperator.Controls.Add(this.optEquals);
            this.grpOperator.Location = new System.Drawing.Point(6, 47);
            this.grpOperator.Name = "grpOperator";
            this.grpOperator.Size = new System.Drawing.Size(161, 102);
            this.grpOperator.TabIndex = 66;
            this.grpOperator.TabStop = false;
            // 
            // optLess
            // 
            this.optLess.AutoSize = true;
            this.optLess.Location = new System.Drawing.Point(5, 62);
            this.optLess.Name = "optLess";
            this.optLess.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.optLess.Size = new System.Drawing.Size(121, 17);
            this.optLess.TabIndex = 64;
            this.optLess.Text = "Less then or Equals";
            this.optLess.UseVisualStyleBackColor = true;
            // 
            // optGreater
            // 
            this.optGreater.AutoSize = true;
            this.optGreater.Location = new System.Drawing.Point(5, 39);
            this.optGreater.Name = "optGreater";
            this.optGreater.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.optGreater.Size = new System.Drawing.Size(134, 17);
            this.optGreater.TabIndex = 63;
            this.optGreater.Text = "Greater then or Equals";
            this.optGreater.UseVisualStyleBackColor = true;
            // 
            // optEquals
            // 
            this.optEquals.AutoSize = true;
            this.optEquals.Checked = true;
            this.optEquals.Location = new System.Drawing.Point(5, 6);
            this.optEquals.Name = "optEquals";
            this.optEquals.Padding = new System.Windows.Forms.Padding(3, 10, 0, 0);
            this.optEquals.Size = new System.Drawing.Size(60, 27);
            this.optEquals.TabIndex = 62;
            this.optEquals.TabStop = true;
            this.optEquals.Text = "Equals";
            this.optEquals.UseVisualStyleBackColor = true;
            // 
            // cmdAdd
            // 
            this.cmdAdd.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdAdd.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.cmdAdd.Enabled = false;
            this.cmdAdd.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdAdd.ForeColor = System.Drawing.Color.White;
            this.cmdAdd.Location = new System.Drawing.Point(24, 262);
            this.cmdAdd.Name = "cmdAdd";
            this.cmdAdd.Size = new System.Drawing.Size(52, 21);
            this.cmdAdd.TabIndex = 65;
            this.cmdAdd.Text = "Add";
            this.cmdAdd.UseVisualStyleBackColor = false;
            this.cmdAdd.Click += new System.EventHandler(this.cmdAdd_Click);
            // 
            // cmdClear
            // 
            this.cmdClear.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdClear.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.cmdClear.Enabled = false;
            this.cmdClear.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdClear.ForeColor = System.Drawing.Color.White;
            this.cmdClear.Location = new System.Drawing.Point(89, 262);
            this.cmdClear.Name = "cmdClear";
            this.cmdClear.Size = new System.Drawing.Size(52, 21);
            this.cmdClear.TabIndex = 64;
            this.cmdClear.Text = "Clear";
            this.cmdClear.UseVisualStyleBackColor = false;
            this.cmdClear.Click += new System.EventHandler(this.cmdClear_Click);
            // 
            // txtCompare
            // 
            this.txtCompare.Location = new System.Drawing.Point(4, 155);
            this.txtCompare.Multiline = true;
            this.txtCompare.Name = "txtCompare";
            this.txtCompare.Size = new System.Drawing.Size(163, 20);
            this.txtCompare.TabIndex = 62;
            this.txtCompare.TextChanged += new System.EventHandler(this.txtCompare_TextChanged);
            // 
            // cboFields
            // 
            this.cboFields.FormattingEnabled = true;
            this.cboFields.Location = new System.Drawing.Point(6, 22);
            this.cboFields.Name = "cboFields";
            this.cboFields.Size = new System.Drawing.Size(161, 21);
            this.cboFields.TabIndex = 58;
            this.cboFields.SelectedIndexChanged += new System.EventHandler(this.cboFields_SelectedIndexChanged);
            // 
            // txtQuery
            // 
            this.txtQuery.Location = new System.Drawing.Point(6, 184);
            this.txtQuery.Multiline = true;
            this.txtQuery.Name = "txtQuery";
            this.txtQuery.ReadOnly = true;
            this.txtQuery.Size = new System.Drawing.Size(161, 77);
            this.txtQuery.TabIndex = 63;
            this.txtQuery.TextChanged += new System.EventHandler(this.txtQuery_TextChanged);
            // 
            // txtcptcodes
            // 
            this.txtcptcodes.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.txtcptcodes.Location = new System.Drawing.Point(46, 717);
            this.txtcptcodes.Multiline = true;
            this.txtcptcodes.Name = "txtcptcodes";
            this.txtcptcodes.Size = new System.Drawing.Size(100, 91);
            this.txtcptcodes.TabIndex = 36;
            this.txtcptcodes.TextChanged += new System.EventHandler(this.txtcptcodes_TextChanged);
            // 
            // lblCptLines
            // 
            this.lblCptLines.AutoSize = true;
            this.lblCptLines.Location = new System.Drawing.Point(46, 737);
            this.lblCptLines.Name = "lblCptLines";
            this.lblCptLines.Size = new System.Drawing.Size(90, 13);
            this.lblCptLines.TabIndex = 37;
            this.lblCptLines.Text = "Insert CPT Codes";
            // 
            // cboRange
            // 
            this.cboRange.FormattingEnabled = true;
            this.cboRange.Items.AddRange(new object[] {
            "Appointment Date",
            "Check Date"});
            this.cboRange.Location = new System.Drawing.Point(19, 81);
            this.cboRange.Name = "cboRange";
            this.cboRange.Size = new System.Drawing.Size(159, 21);
            this.cboRange.TabIndex = 39;
            this.cboRange.Text = "Appointment Date";
            this.cboRange.SelectedIndexChanged += new System.EventHandler(this.cboRange_SelectedIndexChanged);
            // 
            // frmCal
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(206, 851);
            this.Controls.Add(this.cboRange);
            this.Controls.Add(this.pnlFilter);
            this.Controls.Add(this.lblCptLines);
            this.Controls.Add(this.txtcptcodes);
            this.Controls.Add(this.grpSpan);
            this.Controls.Add(this.cmdOk);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblTo);
            this.Controls.Add(this.txtFrom);
            this.Controls.Add(this.txtTo);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmCal";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Calendar";
            this.Activated += new System.EventHandler(this.frmCal_Activated);
            this.Load += new System.EventHandler(this.frmCal_Load);
            this.MouseLeave += new System.EventHandler(this.frmCal_MouseLeave);
            this.grpSpan.ResumeLayout(false);
            this.grpSpan.PerformLayout();
            this.pnlFilter.ResumeLayout(false);
            this.pnlFilter.PerformLayout();
            this.pnlColumns.ResumeLayout(false);
            this.pnlColumns.PerformLayout();
            this.pnlQuery.ResumeLayout(false);
            this.pnlQuery.PerformLayout();
            this.grpOperator.ResumeLayout(false);
            this.grpOperator.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblTo;
        private System.Windows.Forms.DateTimePicker txtFrom;
        private System.Windows.Forms.DateTimePicker txtTo;
        private System.Windows.Forms.Button cmdOk;
        private System.Windows.Forms.GroupBox grpSpan;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox chkSubsequent;
        private System.Windows.Forms.ComboBox cboAmount;
        private System.Windows.Forms.ComboBox cboSpan;
        private System.Windows.Forms.CheckBox chkToDate;
        private System.Windows.Forms.TextBox txtcptcodes;
        private System.Windows.Forms.Label lblCptLines;
        private System.Windows.Forms.Panel pnlFilter;
        private System.Windows.Forms.Label lblColumns;
        private System.Windows.Forms.Panel pnlQuery;
        private System.Windows.Forms.CheckedListBox chkFilter;
        private System.Windows.Forms.DateTimePicker calFilter;
        private System.Windows.Forms.GroupBox grpOperator;
        private System.Windows.Forms.RadioButton optLess;
        private System.Windows.Forms.RadioButton optGreater;
        private System.Windows.Forms.RadioButton optEquals;
        private System.Windows.Forms.Button cmdAdd;
        private System.Windows.Forms.Button cmdClear;
        private System.Windows.Forms.TextBox txtCompare;
        private System.Windows.Forms.ComboBox cboFields;
        private System.Windows.Forms.TextBox txtQuery;
        private System.Windows.Forms.FlowLayoutPanel pnlColumns;
        private System.Windows.Forms.CheckBox chkSelectAll;
        private System.Windows.Forms.CheckBox chkDeselectAll;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckedListBox chkColumns;
        private System.Windows.Forms.Label lblFilter;
        private System.Windows.Forms.ComboBox cboRange;
    }
}