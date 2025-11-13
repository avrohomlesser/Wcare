using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ICR
{
    public partial class frmCal : Form
    {
        //frmMain frm = new frmMain();
        public frmCal()
        {
            InitializeComponent();
        }

        private void frmCal_Load(object sender, EventArgs e)
        {
            cboRange.Visible = frmMain.reportId == 17;
            cboRange.SelectedIndex = frmMain.searchdate == "servicedt" ? 0 : 1;
             txtFrom.Value= frmMain.from;
            if (frmMain.to == new DateTime(DateTime.Now.Year - 20, 1, 1)) { 
                txtTo.Enabled = false;
               lblTo.Enabled = false;
            }
            else
            {
                txtTo.Enabled = true;
                lblTo.Enabled = true;
                txtTo.Value = frmMain.to;
            }
            if (frmMain.reportId == 2)
            {
                grpSpan.Enabled = false;
            }
            else
            {
                grpSpan.Enabled = true;
            }

            if (frmMain.reportId == 6)
            {
                if (!frmMain.refreshFilter)
                {
                    chkColumns.Items.Clear();
                    for (int i = 0; i < frmMain.filterFields.GetLength(0); i++)
                    {

                        if (i != 1)
                        {
                            chkColumns.Items.Add(frmMain.filterFields[i, 1].ToString(), true);
                            frmMain.filterFields[i, 5] = "";
                            frmMain.filterFields[i, 4] = "";
                        }
                    }
                    txtQuery.Text = "";
                    pnlColumns.Visible = false;
                    pnlQuery.Visible = false;
                }
                pnlFilter.Visible = true;
                lblFilter.Top = lblColumns.Top + 17;

            }





            if (frmMain.reportId == 23)
            {
                lblCptLines.Visible = true;
                txtcptcodes.Visible = true;
                cmdOk.Enabled = false;
                Height = 407;
            }
            else
            {
                lblCptLines.Visible = false;
                txtcptcodes.Visible = false;
                cmdOk.Enabled = true;
                Height = 407;
            }

        }

        private void txtFrom_ValueChanged(object sender, EventArgs e)
        {
            frmMain.from =txtFrom.Value  ;
        }

        private void txtTo_ValueChanged(object sender, EventArgs e)
        {
            frmMain.to=txtTo.Value ;
        }

        private void cmdOk_Click(object sender, EventArgs e)
        {
            frmMain.cptLines = txtcptcodes.Text;
            frmMain.refresh = true;
            this.Hide();
        }

        private void frmCal_Activated(object sender, EventArgs e)
        {
            cmdOk.Focus();
        }

        private void optToday_CheckedChanged(object sender, EventArgs e)
        {
           
        }

        private void cboDays_SelectedIndexChanged(object sender, EventArgs e)
        {
         
        }

        private void optPreviousDays_CheckedChanged(object sender, EventArgs e)
        {
            
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
           
        }

        private void optPreviousWeek_CheckedChanged(object sender, EventArgs e)
        {
       
        }

        private void cboSpan_SelectedIndexChanged(object sender, EventArgs e)
        {
            refreshDate();

        }
        private void refreshDate()
        {
            int[] span = new int[] { 8, 5, 13, 5, 6 };
            cboAmount.Items.Clear();
            cboAmount.Text = "";
            chkSubsequent.Enabled = false;
            for (int spanner = 0; spanner < 5; spanner++)
            {

                if (spanner == cboSpan.SelectedIndex)
                {

                    if (spanner == 0) { cboAmount.Items.Add("Today"); } else { cboAmount.Items.Add("Current"); };
                    for (int amt = 1; amt < span[spanner]; amt++) {
                        cboAmount.Items.Add(amt.ToString());
                    }
                    cboAmount.SelectedIndex = -1;
                }
            }    
        }

        private void cboAmount_SelectedIndexChanged(object sender, EventArgs e)
        {
         
        }

        private void cboAmount_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (chkToDate.Checked) chkSubsequent.Checked = true;
            chkToDate.Enabled = chkSubsequent.Checked;
            if (cboAmount.SelectedIndex < 0) return;
            if (cboSpan.SelectedIndex == 0)
            {
                if (cboAmount.SelectedIndex == 0)
                {
                    txtFrom.Value = DateTime.Today;
                    txtTo.Value = DateTime.Today;
                }
                else
                {
                    int dayAmount = Int32.Parse(cboAmount.SelectedItem.ToString());
                    dayAmount = 0 - dayAmount;
                    txtFrom.Value = DateTime.Today.AddDays(dayAmount);
                    if (chkSubsequent.Checked)
                    {
                       
                            txtTo.Value = DateTime.Today.AddDays(-1);
                        
                    }
                    else
                    {
                        txtTo.Value = DateTime.Today.AddDays(dayAmount );
                    }
                }
            }
            else if (cboSpan.SelectedIndex == 1)
            {
                if (cboAmount.SelectedIndex == 0)
                {
                    txtFrom.Value = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
                    txtTo.Value = DateTime.Today.AddDays(-1);
                }
                else
                {
                    int weekAmount = Int32.Parse(cboAmount.SelectedItem.ToString()) * 7;
                    //weekAmount = 0 - weekAmount  ;
                    txtFrom.Value = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek - weekAmount);

                    if (chkSubsequent.Checked)
                    {
                        if (chkToDate.Checked)
                        {
                            txtTo.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddDays(-1);
                        }
                        else
                        {
                            txtTo.Value = txtFrom.Value.AddDays( weekAmount-1);
                        }


                    }
                    else
                    {
                       txtTo.Value = txtFrom.Value.AddDays(6);
                    }
                    
                }
            }
            else if (cboSpan.SelectedIndex == 2)
            {
                if (cboAmount.SelectedIndex == 0)
                {
                    txtFrom.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                    txtTo.Value = DateTime.Today.AddDays(-1);
                    if (txtTo.Value < txtFrom.Value) { txtTo.Value = txtFrom.Value; }
                }
                else
                {
                    int monthAmount = Int32.Parse(cboAmount.SelectedItem.ToString());
                    //weekAmount = 0 - weekAmount  ;
                    txtFrom.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-monthAmount);
                    if (chkSubsequent.Checked)
                    {
                        if (chkToDate.Checked)
                        {
                            txtTo.Value = DateTime.Today.AddDays(-1);
                        }
                        else
                        {
                            txtTo.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddDays(-1);
                        }
                    }
                    else
                    {
                        txtTo.Value = txtFrom.Value.AddMonths(1).AddDays(-1);
                    }
                }
            }
            else if (cboSpan.SelectedIndex == 3)
            {
                if (cboAmount.SelectedIndex == 0)
                {
                    txtFrom.Value = new DateTime(DateTime.Today.Year,1, 1);
                    txtTo.Value = DateTime.Today.AddDays(-1);
                }
                else
                {
                    int yearAmount = Int32.Parse(cboAmount.SelectedItem.ToString());
                    //weekAmount = 0 - weekAmount  ;
                    txtFrom.Value = new DateTime(DateTime.Today.Year- yearAmount, 1 , 1);
                    if (chkSubsequent.Checked)
                    {
                        if (chkToDate.Checked)
                        {
                            txtTo.Value =  DateTime.Today.AddDays(-1);
                        }
                        else
                        {
                            txtTo.Value = txtFrom.Value.AddYears(yearAmount).AddDays(-1);
                        }
                    }
                    else
                    {
                        txtTo.Value = new DateTime((DateTime.Today.Year - yearAmount)+1, 1, 1).AddDays(-1);
                    }
                }
            }
            chkSubsequent.Enabled = true;
        }

        private void frmCal_MouseLeave(object sender, EventArgs e)
        {
        
        }

        private void txtcptcodes_TextChanged(object sender, EventArgs e)
        {
            cmdOk.Enabled = txtcptcodes.Text != "";
        }

      
        private void lblColumns_Click(object sender, EventArgs e)
        {
            pnlColumns.Visible = !pnlColumns.Visible;
            if (pnlColumns.Visible) pnlQuery.Visible = false;
            resize_form();
            //  MessageBox.Show(pnlColumns.Top.ToString() + " " + pnlColumns.Height.ToString());
        }
        private void resize_form()
        {
            int size = 270 + grpSpan.Height;
            size += pnlColumns.Visible == true ? pnlColumns.Height : 0;
            size += pnlQuery.Visible == true ? pnlQuery.Height : 0;
            pnlFilter.Top = grpSpan.Top + grpSpan.Height + 15;
            lblFilter.Top = pnlColumns.Visible == true ? 281 : 20;
            pnlQuery.Top = lblFilter.Top + 20;
            pnlFilter.Height = pnlColumns.Height + pnlQuery.Height + 20;
            //  pnlQuery.Top = lblFilter.Top + 10;
            this.Height = size;
        }

        private void calFilter_ValueChanged(object sender, EventArgs e)
        {
            txtCompare.Text = calFilter.Value.ToShortDateString();
        }

 

        private void chkFilter_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (chkFilter.GetItemChecked(e.Index) && chkFilter.CheckedItems.Count < 2)
            {
                cmdAdd.Enabled = false;
            }
            else
            {
                cmdAdd.Enabled = true;
            }
        }


        private void frmCal_VisibleChanged(object sender, EventArgs e)
        {
            resize_form();
        }
        private void chkColumns_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            int ind = e.Index == 0 ? 0 : e.Index + 1;
            //  cmdOk.Enabled = !chkColumns.GetItemChecked(e.Index) && chkColumns.CheckedItems.Count < 2;
            if (chkColumns.GetItemChecked(e.Index) && chkColumns.CheckedItems.Count < 2)
            {
                cmdOk.Enabled = false;

            }
            else
            {
                cmdOk.Enabled = true;
            }

            if (chkColumns.GetItemChecked(e.Index))
            {
                frmMain.filterFields[ind, 3] = "false";
                frmMain.filterFields[e.Index, 5] = "";
                reset_query_text();
                chkSelectAll.Checked = false;
                chkSelectAll.Enabled = true;
            }
            else
            {
                frmMain.filterFields[ind, 3] = "true";
                chkDeselectAll.Checked = false;
                chkDeselectAll.Enabled = true;

            }


        }

        private void lblFilter_Click(object sender, EventArgs e)
        {
            pnlQuery.Visible = !pnlQuery.Visible;
            if (pnlQuery.Visible) pnlColumns.Visible = false;
            Dictionary<string, string> comboSource = new Dictionary<string, string>();


            cboFields.DataSource = null;
            cboFields.Items.Clear();
            for (int i = 0; i < frmMain.filterFields.GetLength(0); i++)
            {
                if (frmMain.filterFields[i, 3] == "true" && i != 1)
                {
                    comboSource.Add(frmMain.filterFields[i, 1], frmMain.filterFields[i, 2]);
                }


            }






            cboFields.DataSource = new BindingSource(comboSource, null);
            cboFields.DisplayMember = "Key";
            cboFields.ValueMember = "Value";
            cboFields.SelectedIndex = -1;
            cboFields.Text = "Select Field";
            cmdAdd.Enabled = false;
            reset_query_text();
            resize_form();


        }

        private void cboFields_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (cboFields.SelectedIndex >= 0)
            {
                optEquals.Checked = true;
                optEquals.Text = "Equals";
                optLess.Text = "Less than or Equals";
                optGreater.Text = "Greater than or Equals";
                optEquals.Enabled = true;
                optGreater.Enabled = true;
                optLess.Enabled = true;
                optLess.Visible = true;
                cmdAdd.Enabled = true;
                txtCompare.Enabled = true;
                txtCompare.Text = "";
                calFilter.Visible = false;

                chkFilter.Visible = false;
                string type = cboFields.SelectedValue.ToString();
                if (type == "string")
                {
                    optEquals.Text = "Contains";
                    // optEquals.Enabled = false;
                    optGreater.Enabled = false;
                    optLess.Enabled = false;
                    cmdAdd.Enabled = false;
                }
                else if (type != "int" && type != "date" && type != "filter")
                {
                    if (type == "yesno")
                    {
                        optEquals.Text = "Yes";
                        optGreater.Text = "No";
                    }
                    else if (type == "10")
                    {
                        optEquals.Text = "One";
                        optGreater.Text = "Zero";

                    }
                    txtCompare.Enabled = false;
                    optEquals.Checked = true;
                    optEquals.Enabled = true;
                    optGreater.Enabled = true;
                    optLess.Visible = false;
                }
                else if (type == "date")
                {
                    calFilter.Visible = true;
                    txtCompare.Text = calFilter.Value.ToShortDateString();
                }
                else if (type == "filter")
                {
                    cmdAdd.Enabled = false;
                    chkFilter.Visible = true;
                    chkFilter.DataSource = null;
                    chkFilter.Items.Clear();
                    if (cboFields.Text == "Doctor" || cboFields.Text == "Speciality")
                    {

                        string connstr = frmMain.connectionString;
                        SqlConnection connection = new SqlConnection(connstr);
                        connection.Open();

                        DataTable filter = new DataTable();
                        string sql;
                        if (cboFields.Text == "Doctor")
                            sql = "select concat(u.ulname, ' ' , u.ufname) Doctor from doctors d LEFT JOIN USERS U ON D.DOCTORID =  u.uid where d.printname > '' and (d.SS_Deactivated is null or d.SS_Deactivated = 'N') AND U.STATUS = 0 order by concat(u.ulname, ' ' , u.ufname) ";
                        else sql = "select distinct speciality  from doctors d where d.speciality > '' and  printname > '' and (d.SS_Deactivated is null or d.SS_Deactivated = 'N') order by d.speciality ";

                        SqlDataAdapter adapter = new SqlDataAdapter(sql, connection);
                        adapter.Fill(filter);
                        connection.Close();
                        chkFilter.DataSource = filter;
                        chkFilter.Refresh();
                        chkFilter.DisplayMember = cboFields.Text == "Doctor" ? "Doctor" : "speciality";
                        txtCompare.Focus();
                    }
                    else if (cboFields.Text == "Facility")
                    {
                        chkFilter.Items.AddRange("Rambam,Hasc Diagnostic & Treatment Center,Vaccine Center,Article 16".Split(','));


                    }

                }
                else if (type == "int")
                {

                    cmdAdd.Enabled = false;
                }




            }


        }

        private void txtCompare_TextChanged(object sender, EventArgs e)
        {
            cmdAdd.Enabled = txtCompare.Text != "";
            if (cboFields.SelectedValue.ToString() == "int")
            {
                foreach (char c in txtCompare.Text)
                {
                    //MessageBox.Show(c.ToString());
                    if (!Char.IsDigit(c) && !System.Text.RegularExpressions.Regex.IsMatch(c.ToString(), "[\r\n]"))
                    {

                        txtCompare.Text = txtCompare.Text.Remove(txtCompare.Text.Length - 1);
                    }
                }
            }
            else if (cboFields.SelectedValue.ToString() == "filter")
            {
                cmdAdd.Enabled = false;
                if (txtCompare.Text == "")
                {
                    chkFilter.TopIndex = 0;
                }
                else
                {
                    for (int ctr = 0; ctr < chkFilter.Items.Count; ctr++)
                    {
                        if (chkFilter.GetItemText(chkFilter.Items[ctr]).ToString().IndexOf(txtCompare.Text, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            chkFilter.TopIndex = ctr;
                            return;
                        }
                    }
                }
            }




            if (txtCompare.Lines.Length > 1)
            {
                optEquals.Text = "Range";
                optLess.Enabled = false;
                optGreater.Enabled = false;
                optEquals.Checked = true;
            }
            else
            {
                optEquals.Text = "Equals";
                if (cboFields.SelectedValue.ToString() == "int")
                {
                    optLess.Enabled = true;
                    optGreater.Enabled = true;
                }
            }
            txtCompare.ScrollBars = ScrollBars.None;
            if (txtCompare.Lines.Length < 2)
            {
                txtCompare.Height = 20;
            }
            else if (txtCompare.Lines.Length < 8)
            {
                txtCompare.Height = 15 * txtCompare.Lines.Length;
            }
            else
            {
                txtCompare.Height = 100;
                txtCompare.ScrollBars = ScrollBars.Vertical;
            }

        }

        private void txtQuery_TextChanged(object sender, EventArgs e)
        {
            cmdClear.Enabled = txtQuery.Text != "";
            txtQuery.ScrollBars = txtQuery.Lines.Length > 5 ? ScrollBars.Vertical : ScrollBars.None;

        }

        private void cmdAdd_Click(object sender, EventArgs e)
        {
            string type = cboFields.SelectedValue.ToString();
            int i = 0;
            for (int index = 0; index < frmMain.filterFields.GetLength(0); index++)
            {
                if (frmMain.filterFields[index, 1].ToString() == cboFields.GetItemText(cboFields.SelectedItem)) i = index;
            }
            if (type == "int")
            {
                if (optEquals.Text == "Range")
                {


                    frmMain.filterFields[i, 5] = cboFields.Text + " Contains Range ";
                    frmMain.filterFields[i, 4] = " and dv.[" + cboFields.Text + "] in (";
                    string[] lines = txtCompare.Text.Replace("'", "").Split(new[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string txt in lines)
                    {
                        frmMain.filterFields[i, 4] += txt + ",";
                    }
                    frmMain.filterFields[i, 4] += ")";
                    frmMain.filterFields[i, 4] = frmMain.filterFields[i, 4].Replace(",)", ")");

                }
                else
                {
                    string operand = optEquals.Checked ? " = " : optGreater.Checked ? " >= " : " <= ";
                    frmMain.filterFields[i, 5] = cboFields.Text + operand + txtCompare.Text;
                    frmMain.filterFields[i, 4] = " and dv.[" + cboFields.Text + "] " + operand + txtCompare.Text;
                    txtCompare.Text = "";
                }
            }
            else if (type == "date")
            {
                string operand = optEquals.Checked ? " = " : optGreater.Checked ? " >= " : " <= ";
                frmMain.filterFields[i, 5] = cboFields.Text + operand + txtCompare.Text;
                frmMain.filterFields[i, 4] = " and dv.[" + cboFields.Text + "] " + operand + "'" + txtCompare.Text + "'";
                txtCompare.Text = "";
            }
            else if (type == "string")
            {
                if (optEquals.Text == "Range")
                {


                    frmMain.filterFields[i, 5] = cboFields.Text + " Contains Range ";
                    frmMain.filterFields[i, 4] = " and  (";
                    string[] lines = txtCompare.Text.Replace("'", "").Split(new[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string txt in lines)
                    {
                        frmMain.filterFields[i, 4] += "  dv.[" + cboFields.Text + "] like '%" + txt + "%' or ";
                    }
                    frmMain.filterFields[i, 4] += ")";
                    frmMain.filterFields[i, 4] = frmMain.filterFields[i, 4].Replace("or )", ")");

                }
                else
                {
                    frmMain.filterFields[i, 5] = cboFields.Text + " Contains " + txtCompare.Text;
                    frmMain.filterFields[i, 4] = " and dv.[" + cboFields.Text + "] like '%" + txtCompare.Text + "%'";
                }

            }
            else if (type == "yesno")
            {
                frmMain.filterFields[i, 5] = cboFields.Text + " = " + (optEquals.Checked ? "Yes" : "No");
                frmMain.filterFields[i, 4] = " and dv.[" + cboFields.Text + "] = " + (optEquals.Checked ? "'Yes'" : "'No'");

            }
            else if (type == "10")
            {
                frmMain.filterFields[i, 5] = cboFields.Text + " = " + (optEquals.Checked ? "1" : "0");
                frmMain.filterFields[i, 4] = " and dv.[" + cboFields.Text + "] = " + (optEquals.Checked ? "1'" : "0");

            }
            else if (type == "filter")
            {

                frmMain.filterFields[i, 5] = cboFields.Text + " = ( ";
                frmMain.filterFields[i, 4] = " and dv.[" + cboFields.Text + "] in (";
                for (int cols = 0; cols < chkFilter.Items.Count; cols++)
                {


                    if (chkFilter.GetItemChecked(cols))
                    {
                        frmMain.filterFields[i, 5] += chkFilter.GetItemText(chkFilter.Items[cols]) + "\r\n or ";
                        frmMain.filterFields[i, 4] += "'" + chkFilter.GetItemText(chkFilter.Items[cols]) + "',";
                    }


                }
                frmMain.filterFields[i, 5] += ")";
                frmMain.filterFields[i, 4] += ")";
                frmMain.filterFields[i, 4] = frmMain.filterFields[i, 4].Replace(",)", ")");
                frmMain.filterFields[i, 5] = frmMain.filterFields[i, 5].Replace("\r\n or )", ")");

                chkFilter.DataSource = null;
                chkFilter.Refresh();
                chkFilter.Items.Clear();


            }
            cmdAdd.Enabled = false;
            reset_query_text();
            txtCompare.Text = "";
        }

        private void reset_query_text()
        {
            txtQuery.Text = "";
            for (int index = 0; index < frmMain.filterFields.GetLength(0); index++)
            {
                if (frmMain.filterFields[index, 5] != "")
                {
                    txtQuery.Text += txtQuery.Text == "" ? frmMain.filterFields[index, 5] : "\r\n and " + frmMain.filterFields[index, 5];
                };
            }
        }

        private void cmdClear_Click(object sender, EventArgs e)
        {
            txtQuery.Text = "";
            for (int i = 0; i < frmMain.filterFields.GetLength(0); i++)
            {
                frmMain.filterFields[i, 5] = "";
                frmMain.filterFields[i, 4] = "";
            }
        }

        private void chkDeselectAll_CheckedChanged(object sender, EventArgs e)
        {
            if (chkDeselectAll.Checked == true)
            {
                for (int cols = 0; cols < chkColumns.Items.Count; cols++)
                {
                    chkColumns.SetItemChecked(cols, false);
                }
                chkSelectAll.Checked = false;
                chkSelectAll.Enabled = true;
                chkDeselectAll.Enabled = false;
            }
        }

        private void chkSelectAll_CheckedChanged(object sender, EventArgs e)
        {
            if (chkSelectAll.Checked == true)
            {
                for (int cols = 0; cols < chkColumns.Items.Count; cols++)
                {
                    chkColumns.SetItemChecked(cols, true);
                }
                chkDeselectAll.Checked = false;
                chkDeselectAll.Enabled = true;
                chkSelectAll.Enabled = false;
            }
        }

        private void pnlFilter_Paint(object sender, PaintEventArgs e)
        {

        }

        private void cboRange_SelectedIndexChanged(object sender, EventArgs e)
        {
            
                frmMain.searchdate = cboRange.SelectedIndex == 1? "checkdate":"servicedt";
        }
    }
}
