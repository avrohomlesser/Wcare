using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Outlook = Microsoft.Office.Interop.Outlook;
using System.Data;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ChartPractice;
using System.Net.Http;
using Newtonsoft.Json.Linq;


namespace ICR
{
    public partial class frmMain : Form
    {




        SqlConnection connection;
        SqlDataAdapter dataadapter;
        DataSet ds;
        

        public static string searchdate = "servicedt";
        public static  string connectionString = "Data Source=ecw-db-b.lasantehealth.org;Initial Catalog=mobiledoc;Trusted_connection=yes;Connection Timeout=3600";
        public static int caller = 0;
        public int lastX = 0;
        public static bool refreshFilter = false;
        public int gridRecordCount = 0;
        public int currentX = 15;
        Boolean working;
        string tmpFilter;
        string tmprange;
        string xlOpenPath;
        string xlSavePath;



        public static int reportId;
        string defaultJoin = @" left join patients pat  on e.patientID = pat.pid
left join edi_invoice v on e.EncounterId = v.EncounterId  and v.deleteflag = 0  
 left   join insurance i on  v.PrimaryInsId = i.insId
 left join claimstatuscodes sc on v.FileStatus = sc.code left join insurance si on  v.SecondaryInsId = si.insId
left join users u on e.patientid = u.uid
 left join users d ON e.doctorID = d.uid ";

        string sqlTxt = "";
        Boolean insurance = true;
        string defaultSelectDetail = @"select distinct pat.controlno [Patient ID], u.ulname [Last Name],u.ufname [First Name],
 V.ID [Claim ID], concat(d.ufname, ' ' , d.ulname)  Doctor, E.date [Appointment Date], v.SubmittedDate Submitted,
  i.insuranceName [Insurance Name], 
 si.insuranceName [Secondary Insurance Name], 
  v.netpayment - v.ptpayment [Initial Payment] from enc e ";

        string getFacility = @" case when e.facilityid = 70 then 'Rambam' when e.facilityid = 1 then 'Hasc Diagnostic & Treatment Center'  when e.facilityid = 52 then 'Vaccine Center'
                              when e.facilityid = 75 then 'Article 16' when e.facilityid = 77 then 'Rapid Care' when e.facilityid = 78 then '1651 Coney' else 'UnKnown' END Facility";
        string[] speciality = { "Behavioral Health", "Dental", "Vision", "Medical" };
        string[] specialities = { @" and doc.speciality in  ('Clinical social worker','Psychologist','Clinical Psychologist','Licensed Clinical Social Worker','LMHC','LMSW','Mental health worker','Psychiatry','Behavioral Health \/ Developmental Health')",
                                        " and doc.speciality in ('Dental Care','Dental General Practice')",
                                        " and doc.speciality in  ('Ophthalmology','Ophthalmology/Retina Specialist','Optometrist')",
                                        " and doc.speciality not in  ('Clinical social worker','Psychologist','Clinical Psychologist','Licensed Clinical Social Worker','Mental health worker','Psychiatry','Dental Care','Dental General Practice','Ophthalmology','Ophthalmology/Retina Specialist','LMHC','LMSW','Optometrist')"};

        string specialityDetail = @"case when doc.speciality in  ('Clinical social worker','Psychologist','Clinical Psychologist','Licensed Clinical Social Worker','Mental health worker','Psychiatry','Behavioral Health \/ Developmental Health','LMHC','LMSW') then 'Behavioral Health'
                         when doc.speciality in ('Dental Care','Dental General Practice') then 'Dental'
                         when doc.speciality in ('Ophthalmology','Ophthalmology/Retina Specialist','Optometrist') then 'Vision'
                          else 'Medical' end ";
        public static String[,] filterFields;
        public static string sqlFilter;
        public static DateTime from;
        public static DateTime to;
        DataTable detail = new DataTable();
        public static string cptLines;
        int index = 0;
        DataGridView[] reportView;
        string[] colQuery;
        List<object>[] colFilter;
        List<int> tempList;
        List<int>[] colItemSelected;
        string xlSql;
        private string xlTemplate;
        private string xlFileName;
        public static bool refresh = false;
        public static bool developer = false;
        private Dictionary<string, System.Windows.Forms.DataVisualization.Charting.SeriesChartType> chartTypes = new Dictionary<string, System.Windows.Forms.DataVisualization.Charting.SeriesChartType>
        {
{"Column ",System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column },
{"Bar ",System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Bar },
{"StepLine ",System.Windows.Forms.DataVisualization.Charting.SeriesChartType.StepLine },
{"Line ",System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line },
{"Range ",System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Range },
{"RangeBar ",System.Windows.Forms.DataVisualization.Charting.SeriesChartType.RangeBar },
{"Spline ",System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline },
{"SplineArea ",System.Windows.Forms.DataVisualization.Charting.SeriesChartType.SplineArea },
{"StackedColumn ",System.Windows.Forms.DataVisualization.Charting.SeriesChartType.StackedColumn },
{"StackedColumn100 ",System.Windows.Forms.DataVisualization.Charting.SeriesChartType.StackedColumn100 },
        };

        public DataTable getSqlTable(DataTable sqlTable, String sql)
        {
            IFormatProvider culture = new CultureInfo("en-US", true);
            string connstr = connectionString;
            SqlConnection connection = new SqlConnection(connstr);
            connection.Open();
             SqlDataAdapter adapter = new SqlDataAdapter(sql, connection);
            adapter.Fill(sqlTable);
            connection.Close();
            return sqlTable;
        }
        public frmMain()
        {
            InitializeComponent();
           
            reportView = new DataGridView[100];
            colQuery = new string[100];
            colFilter = new List<object>[100];
            colItemSelected = new List<int>[100];
            tempList = new List<int>();

            if (!System.Security.Principal.WindowsIdentity.GetCurrent().Name.Contains("LASANTE\\")){
                connectionString = ICR2.Properties.Settings.Default.rambam_connection;
                xlOpenPath = ICR2.Properties.Settings.Default.rambam_open_xlpath;
                xlSavePath = ICR2.Properties.Settings.Default.rambam_save_xlpath;
            }

            //if (ICR2.Properties.Settings.Default.platform.ToString() == "lasante")
            //{

            //    connectionString = ICR2.Properties.Settings.Default.lasante_connection;
            //    xlOpenPath = ICR2.Properties.Settings.Default.lasante_open_xlpath;
            //    xlSavePath = ICR2.Properties.Settings.Default.lasante_save_xlpath;
            //}
            //else
            //{
            //    connectionString = ICR2.Properties.Settings.Default.rambam_connection;
            //    xlOpenPath = ICR2.Properties.Settings.Default.rambam_open_xlpath;
            //    xlSavePath = ICR2.Properties.Settings.Default.rambam_save_xlpath;
            //}


        }
        string getFacilityNameSql = @" case when e.facilityId = 70 then 'Rambam' when e.facilityId = 77 then 'Rapid Care'  when e.facilityId = 78 then '1651 Coney'
               when e.facilityId = 1 Then 'Hasc Diagnostic & Treatment Center' when e.facilityId = 52 Then 'Vaccine Center' when e.facilityId = 75 Then 'Article 16'
                else CAST(e.facilityId AS varchar)  end ";

        public string getFacilityName(int facilityid)
        {
            string name = "";
            if(facilityid== 70)  name = "Rambam";
                      else if  (facilityid == 1)  name = "[Hasc Diagnostic & Treatment Center]";
                       else if (facilityid == 52)  name = "[Vaccine Center]";
                        else if  (facilityid == 75)   name = "[Article 16]";
                         else if (facilityid == 77) name = "[Rapid Care]";
                          else if (facilityid == 78) name = "[1651 Coney]";
            else  name = "UnKnown"; 
            return name;
        }

        public void setDetail(int table, int col, string header, string query, string where, string xl, string xlWhere)
        {
            DataRow record = detail.NewRow();
            record["Table"] = table;
            record["column"] = col;
            record["header"] = header;
            record["query"] = query;
            record["where"] = where;
            record["xl"] = xl;
            record["xlWhere"] = xlWhere;
            detail.Rows.Add(record);
        }

        public void setGrid()
        {

            reportView[index] = new DataGridView();


            this.reportView[index].Click += new System.EventHandler(this.grid_Click);
            reportView[index].Name = "Text_" + index.ToString();
            reportView[index].EnableHeadersVisualStyles = false;
            reportView[index].ColumnHeadersDefaultCellStyle.Font = new Font(DataGridView.DefaultFont, FontStyle.Bold);
            reportView[index].BackgroundColor = dataGridView1.BackgroundColor;
            reportView[index].BorderStyle = BorderStyle.None;
            reportView[index].MultiSelect = true;
            reportView[index].AutoGenerateColumns = true;
            //reportView[index].ColumnHeadersDefaultCellStyle = dataGridView1.ColumnHeadersDefaultCellStyle;
            reportView[index].ColumnHeadersDefaultCellStyle.BackColor = ColorTranslator.FromHtml("#03a1fc");

            reportView[index].ColumnHeadersDefaultCellStyle.ForeColor = ColorTranslator.FromHtml("#ffffff");
            reportView[index].ColumnHeadersBorderStyle = dataGridView1.ColumnHeadersBorderStyle;
            reportView[index].DefaultCellStyle = dataGridView1.DefaultCellStyle;
            //reportView[index].AlternatingRowsDefaultCellStyle = dataGridView1.AlternatingRowsDefaultCellStyle;
            reportView[index].AlternatingRowsDefaultCellStyle.BackColor = ColorTranslator.FromHtml("#FFC9DEF5");

            reportView[index].AllowUserToAddRows = false;
            //reportView[index].AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            reportView[index].RowHeadersVisible = false;
            // reportView[index].Width = dataGridView1.Width + 200;

            reportView[index].ScrollBars = ScrollBars.Vertical;
            panel.Controls.Add(reportView[index]);
            if (index == 0) panel.Width = reportView[index].Width + 100;
            //   reportView[index].AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
         
            reportView[index].Visible = true;

        }
        public void setTotal(DataTable dt, int gridType)
        {
            if (gridType == 1)
            {




                if (dataGridView1.Rows.Count > 0 && dataGridView1.Visible)
                {
                    gridTotal.DataSource = null;
                    gridTotal.Columns.Clear();
                    gridTotal.Rows.Clear();
                    gridTotal.Refresh();
                    foreach (DataGridViewColumn datagrid in dataGridView1.Columns)
                    {

                        gridTotal.Columns.Add(datagrid.Clone() as DataGridViewColumn);

                    }

                    DataGridViewRow dataRow = (DataGridViewRow)dataGridView1.Rows[0].Clone();


                    dataRow.Cells[0].Value = "Total";
                    for (int ctr = 1; ctr < dt.Columns.Count; ctr++)
                    {
                        if (dataGridView1.Columns[ctr].Visible == true)
                        {
                            if (dt.Columns[ctr].ColumnName.IndexOf(" ID") == -1 && (dt.Columns[ctr].DataType == typeof(Decimal) || dt.Columns[ctr].DataType == typeof(Int16) || dt.Columns[ctr].DataType == typeof(Int32) || dt.Columns[ctr].DataType == typeof(Int64) || dt.Columns[ctr].DataType == typeof(Double)))
                            {
                                dataRow.Cells[ctr].Value = dataGridView1.Rows.Cast<DataGridViewRow>().Sum(t => Convert.ToInt32(t.Cells[ctr].Value == DBNull.Value ? 0 : t.Cells[ctr].Value));
                                //  dt.Compute("Sum([" + dt.Columns[ctr].ColumnName + "])", "");
                            }



                        }
                    }
                    gridTotal.Rows.Add(dataRow);
                    for (int ctr = 0; ctr < gridTotal.Columns.Count; ctr++)
                    {
                        gridTotal.Columns[ctr].Width = dataGridView1.Columns[ctr].Width;
                    }
                    gridTotal.Left = dataGridView1.Left;
                    gridTotal.Width = dataGridView1.Width - 15;


                    if (dataGridView1.Rows.Count < 28)
                    {
                        dataGridView1.Height = (dataGridView1.Rows.Count + 1) * 30;
                    }
                    else
                    {
                        dataGridView1.Height = 810;
                    }

                    gridTotal.Top = dataGridView1.Top + dataGridView1.Height + 5;
                    gridTotal.AllowUserToAddRows = false;
                    gridTotal.Refresh();
                    gridTotal.Visible = true;
                }
                else
                {
                    gridTotal.Visible = false;
                }
            }
            else
            {
                var newRow = dt.NewRow();
                newRow[0] = "Total";
                for (int ctr = 1; ctr < dt.Columns.Count; ctr++)
                {
                    if (dt.Columns[ctr].ColumnName.IndexOf(" ID") == -1 && (dt.Columns[ctr].DataType == typeof(Decimal) || dt.Columns[ctr].DataType == typeof(Int16) || dt.Columns[ctr].DataType == typeof(Int32) || dt.Columns[ctr].DataType == typeof(Int64) || dt.Columns[ctr].DataType == typeof(Double)))
                    {
                        newRow[ctr] = dt.Compute("Sum([" + dt.Columns[ctr].ColumnName + "])", "");
                    }
                }
                dt.Rows.Add(newRow);
            }
        }

        public void setdata(string sql, DataGridView grid, int setType, Boolean total, Boolean reportData)
        {
          
            string query = sql;
            try
            {
                
                connection = new SqlConnection(connectionString);
                dataadapter = new SqlDataAdapter(query, connection);
                dataadapter.SelectCommand.CommandTimeout = 400;
                ds = new DataSet();
                // connection.Open();
                dataadapter.Fill(ds, "enc");
            
                // connection.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

            }

            try {
                if (setType == 0)
                {
                  
                    grid.DataSource = null;
                    grid.DataSource = ds;
                    grid.DataMember = "enc";
                    //MessageBox.Show( grid.Name.ToString() + "|");
                    if (reportData && grid.Name.ToString() != "dataGridView1")
                    {
                        if (grid.RowCount > 0)
                        {
                            grid.ClientSize = new Size(grid.Columns.GetColumnsWidth(DataGridViewElementStates.None) + 75, grid.Name != "Datagridview1" ? (grid.Rows.Count * grid.Rows[0].Height) + 100 : (grid.Rows.Count * grid.Rows[0].Height) + 75);
                        }
                        else
                        {
                            if (index == 0)
                            {
                               
                                MessageBox.Show("There Is no data for selected Range");
                            }
                            index--;
                            grid.Visible = false;
                            return;
                        }
                    }
                }
                if (index >= 0) grid.Columns[0].Width = 160;
                
                if (total) setTotal(ds.Tables[0], grid.Name.ToString() == "dataGridView1" ? 1 : 0);
                grid.ClearSelection();

            } catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

            }
            }
          






            //




            private async void frmMain_Load(object sender, EventArgs e)
        {

            this.Enabled = false;

            bool active = await LicenseCheck.IsClientActiveAsync("client123");

            if (!active)
            {
                MessageBox.Show("Account inactive. Please contact support.");
                Close();
                return;
            }

          
            this.Enabled = true;
            detail.Clear();
            detail.Columns.Add("Table");
            detail.Columns.Add("header");
            detail.Columns.Add("column");
            detail.Columns.Add("query");
            detail.Columns.Add("where");
            detail.Columns.Add("xl");
            detail.Columns.Add("xlWhere");
            dataGridView1.Top = 120; dataGridView1.Left = 50;

            panel.Location = dataGridView1.Location;


        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Add any controls that have been previously added dynamically
            InitializeComponent();

        }



        private void copyAlltoClipboard()
        {
            //reportView[index].SelectionMode = DataGridViewSelectionMode.FullColumnSelect;

            reportView[index].SelectAll();
            DataObject dataObj = reportView[index].GetClipboardContent();
            if (dataObj != null)
                Clipboard.SetDataObject(dataObj);
        }

        public void setReport(DateTime fromDate, DateTime toDate, int id)
        {

            Application.DoEvents();
            reportId = id;
            hide_panels();
            panel.Visible = true;


            if (refresh == false)
            {
                frmCal cal = new frmCal();
                cal.StartPosition = FormStartPosition.CenterScreen;
                from = fromDate;

                to = toDate;
                cboVar.Items.Clear();
                cboVar2.Items.Clear();
                cboVar.Visible = true;
                lblVar.Visible = true;
                cal.ShowDialog();




                if (id == 1)
                {
                    xlTemplate = "template";
                    xlFileName = "Claim Status";
                }
                else if (id == 2)
                {
                    xlTemplate = "notes";
                    xlFileName = "notes";

                }
                else if (id == 3)
                {
                    xlTemplate = "template";
                    xlFileName = "Monthly Report";
                    to = to.AddDays(1);
                }
                else if (id == 5)
                {
                    xlTemplate = "template";
                    xlFileName = "ZocDoc Report";
                }
                else if (id == 6)
                {
                    xlTemplate = "template";
                    xlFileName = "Data View Report";
                }
                else if (id == 7)
                {
                    xlTemplate = "template";
                    xlFileName = "Replaced by 4028";
                }
                else if (id == 8)
                {
                    xlTemplate = "notes";
                    xlFileName = "unlocked Doctors";

                }
                else if (id == 9)
                {
                    xlTemplate = "template";
                    xlFileName = "Unpaid Claims";

                }
                else if (id == 10)
                {
                    xlTemplate = "template";
                    xlFileName = "Wrap Analasys";
                    lblOptions.Enabled = true;
                    if (cboVar.Items.Count == 0)
                    {
                        cboVar.Items.Clear();
                        cboVar2.Items.Clear();
                        cboVar.Items.Add("Summary by Date");
                        cboVar.Items.Add("Summary by File Status");
                        cboVar2.Items.Add("MCD DC");
                        cboVar2.Items.Add("MA");
                        cboVar.SelectedIndex = 0;
                        cboVar2.SelectedIndex = 0;

                    }


                }
                else if (id == 11)
                {
                    xlTemplate = "template";
                    xlFileName = "Patient Liability";
                }
                else if (id == 13)
                {
                    xlTemplate = "template";
                    xlFileName = "Duplicate Visits";
                }
                else if (id == 14)
                {
                    xlTemplate = "template";
                    xlFileName = "ICD Report";
                }
                else if (id == 15)
                {
                    lblVar2.Text = "Group By";
                    xlTemplate = "template";
                    xlFileName = "Visits by Resource";
                    cboVar.Items.Clear();
                    cboVar2.Items.Clear();
                    cboVar.Items.Add("Weekly");
                    cboVar.Items.Add("Monthly");
                    cboVar2.Items.Add("Doctor");
                    cboVar2.Items.Add("Specialty");
                     cboVar2.Items.Add("Specialty Group");
                    cboVar.SelectedIndex = 0;
                    cboVar2.SelectedIndex = 0;
                }
                else if (id == 16)
                {
                    xlTemplate = "template";
                    xlFileName = "Claim Submissions";
                }
                else if (id == 17)
                {
                    xlTemplate = "template";
                    xlFileName = "Received Payments";
                }
                else if (id == 18)
                {
                    xlTemplate = "template";
                    xlFileName = "Cross Over Claims";
                }
                else if (id == 19)
                {
                    xlTemplate = "template";
                    xlFileName = "CPT";
                }
                else if (id == 20)
                {
                    xlTemplate = "template";
                    xlFileName = "CHP Data";
                }
                else if (id == 21)
                {
                    xlTemplate = "template";
                    xlFileName = "CPT CAS";
                }
                else if (id == 22)
                {
                    xlTemplate = "template";
                    xlFileName = "Referral Report";
                }
                else if (id == 23)
                {
                    xlTemplate = "template";
                    xlFileName = "CPT Specific Report";
                }
                else if (id == 24)
                {
                    xlTemplate = "template";
                    xlFileName = txtReportName.Text;
                }
                else if (id == 25)
                {
                    xlTemplate = "template";
                    xlFileName = "Patients By Resource";
                    cboVar.Visible=false;
                    lblVar2.Text = "Group By";
                    cboVar2.Items.Clear();
                    cboVar2.Items.Add("Doctor");
                    cboVar2.Items.Add("Specialty");
                    cboVar2.Items.Add("Specialty Group");
                    cboVar2.SelectedIndex = 0;
                    // panel.Height = 1000;
                }
                else if (id == 26)
                {
                    xlTemplate = "template";
                    xlFileName = "Insurance Payment Reconsiliation";
                }
                else if (id == 27)
                {
                    xlTemplate = "template";
                    xlFileName = "Pharmacy Stats";
                }




                if (id < 4)
                {
                    lblExport.Enabled = true;
                    lblRange.Enabled = true;


                }
            }
            lblTitle.Text = xlFileName.Replace("Report", "") + " Report " + from.ToString("MMM dd yy") + " - " + to.ToString("MMM dd yy");
            if (id == 3) { to = to.AddDays(1); }
            refresh = false;

            panel.Controls.Clear();
            index = 0;
            xlSql = "";

            detail.Clear();

            dataGridView1.Visible = false;
            grpSql.Visible = false;
            Cursor.Current = Cursors.WaitCursor;

        }
        private void releaseObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception ex)
            {
                obj = null;
                MessageBox.Show("Exception Occurred while releasing object " + ex.ToString());
            }
            finally
            {
                GC.Collect();
            }
        }



        private void grid_Click(object sender, EventArgs e)
        {
            DataGridView grid = (DataGridView)sender;

            try
            {


                Cursor.Current = Cursors.WaitCursor;
                if (detail.Rows.Count > 0)
                {


                    //    MessageBox.Show(Array.IndexOf(reportView, grid).ToString());

                    if (grid != null)
                    {

                        string senderId = grid.Name;
                        //MessageBox.Show(Array.IndexOf(reportView, grid).ToString());
                        //MessageBox.Show(grid.SelectedCells[0].Value.ToString());

                        string header = grid.Rows[grid.SelectedCells[0].RowIndex].Cells[0].Value.ToString();

                        if (header == "") return;
                        //  MessageBox.Show(".SHO");


                        DataRow[] results = detail.Select("header = '" + header + "'" + "");

                        if (results.Length == 0)
                        {
                            // MessageBox.Show("header = 'default' and table = " + Array.IndexOf(reportView, grid) + " and column = " + grid.SelectedCells[0].ColumnIndex + "");
                            results = detail.Select("header = 'default' and table = " + Array.IndexOf(reportView, grid) + " and column = " + grid.SelectedCells[0].ColumnIndex + "");


                            if (results.Length == 0) return;
                            setdata(results[0]["query"].ToString() + results[0]["where"].ToString() + "'" + header + "'", dataGridView1, 0, true, true);
                        }
                        else
                        {
                            results = detail.Select("header = '" + header + "' and table = " + Array.IndexOf(reportView, grid) + " and column = " + grid.SelectedCells[0].ColumnIndex + "");
                            if (results.Length == 0) return;
                            //MessageBox.Show("header = '" + header + "' and table = " + Array.IndexOf(reportView, grid) + " and column = " + grid.SelectedCells[0].ColumnIndex + "");
                            setdata(results[0]["query"].ToString() + results[0]["where"].ToString(), dataGridView1, 0, true, false);
                        }
                        panel.Visible = false;
                        dataGridView1.Visible = true;

                        //dataGridView1.ClientSize = new Size(dataGridView1.Columns.GetColumnsWidth(DataGridViewElementStates.None) + 75, (dataGridView1.Rows.Count * grid.Rows[1].Height) + 30);

                        // ShowForm();

                    }

                    //foreach (DataGridViewRow row in dataGridView1.Rows)
                    //{
                    //    row.HeaderCell.Value = (row.Index + 1).ToString();
                    //}
                }// throw new NotImplementedException();
                ShowForm();
            }
            catch
            {
                return;
            }
        }


        private void lblClaimStatus_Click(object sender, EventArgs e)
        {
            setReport(DateTime.Today.AddDays(-30), DateTime.Today.AddDays(0), 1);
            setGrid();

            string sql = @"select  distinct  sc.ShortDesc Description, sum(case when l.date > getdate() - 7 then 1 else 0 end ) [1 week], 
 sum(case when l.date > getdate() - 14 and l.date <= getdate() - 7 then 1 else 0 end ) [2 weeks], 
 sum(case when l.date > getdate() - 21 and l.date <= getdate()- 14  then 1 else 0 end ) [3 weeks], 
sum(case when l.date > getdate() - 28 and l.date <= getdate()- 21 then 1 else 0 end ) [4 weeks], 
 sum(case when l.date > getdate() - 35 and l.date <= getdate()- 28  then 1 else 0 end ) [5 weeks], 
 sum(case when l.date > getdate() - 42 and l.date <= getdate()- 35  then 1 else 0 end ) [6 weeks], 
 sum(case when l.date <= getdate() - 42 then 1 else 0 end ) [> 6 weeks], 
 sum(1) [     Total] from enc e   " + defaultJoin +
 @" left join  (select tostatus stat, invid id, date date ,    ROW_NUMBER() OVER(PARTITION BY invid, tostatus ORDER BY date desc)  rn  from   edi_inv_claimstatus_log ) l on v.id = l.id and v.FileStatus = l.stat and rn = 1 
 where v.SubmittedDate > '2000-01-01' AND V.NETPAYMENT - v.ptpayment <= 0 
  and e.date between '" + from + "' and '" + to + "'  and e.deleteFlag = 0  and (v.deleteflag is null or v.deleteflag = 0 )  group by  sc.ShortDesc order by sc.ShortDesc";
            setdata(sql, reportView[index], 0, true, true);










            string selectSql = defaultSelectDetail.Replace("distinct", "distinct sc.shortdesc Status, l.date [Status Date], ") + defaultJoin + @"  left join  (select tostatus stat, invid id, date date ,    ROW_NUMBER() OVER(PARTITION BY invid, tostatus ORDER BY date desc)  rn  from   edi_inv_claimstatus_log ) l on v.id = l.id and v.FileStatus = l.stat and rn = 1 
 where  v.SubmittedDate > '2000-01-01' AND V.NETPAYMENT - v.ptpayment <= 0 and e.date between '" + from + "' and  '" + to + "'  and e.deleteFlag = 0 and v.deleteflag = 0  ";
            xlSql = selectSql;
            for (int num = 0; num < 3; num++)
            {
                string colHead = "", sqlWhere = " and sc.shortdesc is null ", xlWhere = " and Status is null";
                if (num == 1)
                {
                    colHead = "default";
                    sqlWhere = " and sc.shortdesc = ";
                    xlWhere = " and Status = ";

                }
                else if (num == 2)
                {
                    colHead = "Total";
                    sqlWhere = "";
                    xlWhere = "";
                }

                setDetail(index, 1, colHead, selectSql + " and l.date > DATEADD(DAY, DATEDIFF(DAY, 0, GETDATE()), 0) - 7 ", sqlWhere, "  [Status Date] > #" + DateTime.Today.AddDays(-7).ToString("yyyy-MM-dd") + "#", xlWhere);
                setDetail(index, 2, colHead, selectSql + " and  l.date > DATEADD(DAY, DATEDIFF(DAY, 0, GETDATE()), 0) - 14 and l.date <= DATEADD(DAY, DATEDIFF(DAY, 0, GETDATE()), 0) - 7 ", sqlWhere, "   [Status Date] > #" + DateTime.Today.AddDays(-14).ToString("yyyy-MM-dd") + "# and  [Status Date] <= #" + DateTime.Today.AddDays(-7).ToString("yyyy-MM-dd") + "#", xlWhere);
                setDetail(index, 3, colHead, selectSql + " and l.date > DATEADD(DAY, DATEDIFF(DAY, 0, GETDATE()), 0) - 21 and l.date <= DATEADD(DAY, DATEDIFF(DAY, 0, GETDATE()), 0)- 14  ", sqlWhere, "   [Status Date] > #" + DateTime.Today.AddDays(-21).ToString("yyyy-MM-dd") + "# and  [Status Date] <= #" + DateTime.Today.AddDays(-14).ToString("yyyy-MM-dd") + "# ", xlWhere);
                setDetail(index, 4, colHead, selectSql + " and  l.date > DATEADD(DAY, DATEDIFF(DAY, 0, GETDATE()), 0) - 28 and l.date <= DATEADD(DAY, DATEDIFF(DAY, 0, GETDATE()), 0) - 21 ", sqlWhere, "    [Status Date] > #" + DateTime.Today.AddDays(-28).ToString("yyyy-MM-dd") + "# and  [Status Date] <= #" + DateTime.Today.AddDays(-21).ToString("yyyy-MM-dd") + "#", xlWhere);
                setDetail(index, 5, colHead, selectSql + " and l.date > DATEADD(DAY, DATEDIFF(DAY, 0, GETDATE()), 0) - 35 and l.date <= DATEADD(DAY, DATEDIFF(DAY, 0, GETDATE()), 0) - 28  ", sqlWhere, "   [Status Date] > #" + DateTime.Today.AddDays(-35).ToString("yyyy-MM-dd") + "# and  [Status Date] <= #" + DateTime.Today.AddDays(-28).ToString("yyyy-MM-dd") + "#", xlWhere);
                setDetail(index, 6, colHead, selectSql + " and l.date > DATEADD(DAY, DATEDIFF(DAY, 0, GETDATE()), 0) - 42 and l.date <= DATEADD(DAY, DATEDIFF(DAY, 0, GETDATE()), 0)- 35  ", sqlWhere, "   [Status Date] > #" + DateTime.Today.AddDays(-42).ToString("yyyy-MM-dd") + "# and  [Status Date] <= #" + DateTime.Today.AddDays(-35).ToString("yyyy-MM-dd") + "#", xlWhere);
                setDetail(index, 7, colHead, selectSql + " and l.date > DATEADD(DAY, DATEDIFF(DAY, 0, GETDATE()), 0) - 42  ", sqlWhere, "  [Status Date] <= #" + DateTime.Today.AddDays(-42).ToString("yyyy-MM-dd") + "# ", xlWhere);
                setDetail(index, 8, colHead, selectSql, sqlWhere, "", xlWhere);
            }

            ShowForm();
        }

        private void txtFrom_ValueChanged_1(object sender, EventArgs e)
        {

        }

        private void lblUnlockedNotes_Click(object sender, EventArgs e)
        {

            setReport(new DateTime(DateTime.Today.Year - 1, 1, 1), new DateTime(DateTime.Today.Year - 20, 1, 1), 2);
            string tableName = "[Non Vaccine Visits]";
            String where = @" where  (e.status = 'CHK' and e.encLock = 0 and e.VisitType in (
             'ANN VISIT', 'BH Intake', 'BH Visit45', 'C/P', 'DEN-EM', 'DEN-NEW E', 'DEN-NEW L', 'DEN-RECALL', 'GYN', 'IUDO', 'Lab', 'New PT', 'New PT OV', 'OV', 'POD. O/V', 'TeleVisit', 'URG', 'vascular', 'VISION CON', 'VISION F/U',
             'WALK IN', 'AWV', 'BH Coll', 'BH Family', 'BH Fm w/Pt', 'Den-Short', 'EMP HEALTH', 'IUD', 'IUDR', 'New Vision', 'Newborn', 'Podiatry', 'Pod-New', 'PSY-INT', 'Same Day A', 'UrgGYN')
              and e.deleteFlag = 0 and (d.SS_Deactivated is null or d.SS_Deactivated = 'N') and e.resourceid not in (18354,20540,66093,12586,63816,20228,20455,20359) and e.date between '" + from + "' and getdate()-1";


            string selectSql = @"select concat(u.ulname, ' ', u.ufname)[Doctor], e.patientid [Patient ID] , e.date [Appt Date],   e.VisitType [Visit Type], 
            case when  e.VisitType in ('COVVaccin1','COVVaccine','COVVaccin2','FEMA-1','FEMA-2','FEMA-3','COVVaccin3') then 'Yes' else 'No' end [Vaccine Visit] from enc e 
            left join users u on e.resourceid = u.uid
            left join doctors d on e.resourceid = d.doctorid  ";
            xlSql = selectSql + where;


            int table = 0;
            while (table < 2)
            {
                setGrid();
                string sql = @"select concat(u.ulname, ' ' , u.ufname ) " + tableName + @", 
sum( case when e.date >  getdate()-1 then 1 else 0  end ) [< 3],
  sum( case when e.date between  getdate()-16 and getdate()-3  then 1 else 0  end ) [3-14],
sum( case when e.date between  getdate()-32 and getdate()-16  then 1 else 0  end ) [15-30],
  sum( case when e.date between  getdate()-62 and getdate()-32  then 1 else 0  end ) [31-60],
     sum( case when e.date between  getdate()-91 and getdate()-62  then 1 else 0  end ) [61-90],
	  sum( case when e.date<getdate()-91  then 1 else 0  end ) [> 90],sum(1) [Total]
from enc e
            left join users u on e.resourceid = u.uid
              left join doctors d on e.resourceid = d.doctorid              ";


                sql = sql + where + " group by concat(u.ulname, ' ' , u.ufname ) order by concat(u.ulname, ' ' , u.ufname )";

                setdata(sql, reportView[table], 0, true, true);





                for (int num = 0; num < 3; num++)
                {
                    string colHead = "", sqlWhere = " and concat(u.ulname, ' ', u.ufname) is null ", xlWhere = " and Doctor is null";
                    if (num == 1)
                    {
                        colHead = "default";
                        sqlWhere = " and concat(u.ulname, ' ', u.ufname) = ";
                        xlWhere = table == 0 ? " and [Vaccine Visit] = 'No'" : " and [Vaccine Visit] = 'Yes'";
                        xlWhere += " and Doctor = ";
                    }
                    else if (num == 2)
                    {
                        colHead = "Total";
                        sqlWhere = "";
                        xlWhere = table == 0 ? " and [Vaccine Visit] = 'No'" : " and [Vaccine Visit] = 'Yes'";
                    }

                    setDetail(table, 1, colHead, selectSql + where + " and e.date  between getdate()-3 and getdate()-1 ", sqlWhere, " [Appt Date] between #" + DateTime.Today.AddDays(-2).ToString("yyyy-MM-dd") + "# and  #" + DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd") + "#", xlWhere);
                    setDetail(table, 2, colHead, selectSql + where + " and  e.date   between  getdate()-16 and getdate()-3 ", sqlWhere, "   [Appt Date] between #" + DateTime.Today.AddDays(-15).ToString("yyyy-MM-dd") + "# and  #" + DateTime.Today.AddDays(-3).ToString("yyyy-MM-dd") + "#", xlWhere);
                    setDetail(table, 3, colHead, selectSql + where + " and  e.date   between  getdate()-32 and getdate()-16 ", sqlWhere, "   [Appt Date] between #" + DateTime.Today.AddDays(-31).ToString("yyyy-MM-dd") + "# and  #" + DateTime.Today.AddDays(-16).ToString("yyyy-MM-dd") + "#", xlWhere);
                    setDetail(table, 4, colHead, selectSql + where + " and e.date   between  getdate()-62 and getdate()-32  ", sqlWhere, "   [Appt Date] between #" + DateTime.Today.AddDays(-61).ToString("yyyy-MM-dd") + "# and  #" + DateTime.Today.AddDays(-32).ToString("yyyy-MM-dd") + "# ", xlWhere);
                    setDetail(table, 5, colHead, selectSql + where + " and  e.date between  getdate()-91 and getdate()-62 ", sqlWhere, "    [Appt Date] between #" + DateTime.Today.AddDays(-91).ToString("yyyy-MM-dd") + "# and   #" + DateTime.Today.AddDays(-62).ToString("yyyy-MM-dd") + "#", xlWhere);
                    setDetail(table, 6, colHead, selectSql + where + " and e.date  between '" + from + "' and  getdate()-91 ", sqlWhere, "   [Appt Date] < #" + DateTime.Today.AddDays(-91).ToString("yyyy-MM-dd") + "# ", xlWhere);
                    setDetail(table, 7, colHead, selectSql + where, sqlWhere, "", Regex.Replace(xlWhere, "^ and", " "));


                }
                tableName = "[Vaccine Visits      ]";
                where = @" where  (e.status = 'CHK' or e.status like 'Seen%') and e.encLock = 0 and e.VisitType in ( 'COVVaccin1','COVVaccine','COVVaccin2','FEMA-1','FEMA-2','FEMA-3','COVVaccin3') and e.deleteFlag = 0 and (d.SS_Deactivated is null or d.SS_Deactivated = 'N') and e.resourceid not in (18354,66093,12586,63816,20228,20455,20359) and e.date between '" + from + "' and getdate()-1";

                if (table == 1) break;
                xlSql = xlSql.Replace("'ANN VISIT'", "'ANN VISIT','COVVaccin1','COVVaccine','COVVaccin2','FEMA-1','FEMA-2','FEMA-3','COVVaccin3'");
                table++;
                index++;
            }
            reportView[table].Top = reportView[table - 1].Height + 20;
            ShowForm();

        }



        private void lblReports_MouseEnter(object sender, EventArgs e)
        {
            pnlReports.Visible = true;
        }

        private void hide_panels()
        {
            pnlColumns.Visible = false;
            pnlViews.Visible = false;
            pnlReports.Visible = false;
            pnlExport.Visible = false;
            pnlOptions.Visible = false;
            //  grpCal.Visible = false;
            frmCal cal = new frmCal();
            cal.Hide();



        }

        private void frmMain_MouseEnter(object sender, EventArgs e)
        {
            hide_panels();
        }

        private void pnlReports_Paint(object sender, PaintEventArgs e)
        {

        }

        private void frmMain_MouseHover(object sender, EventArgs e)
        {
            hide_panels();
        }

        private void panel_MouseEnter(object sender, EventArgs e)
        {
            hide_panels();
        }




        private void lblExcel_Click(object sender, EventArgs e)
        {
            hide_panels();
            StringBuilder csvContent = new StringBuilder();

            DataGridView grid;
            int table = 0;
            if (index == -1) table = -1;
            //    dataGridView1.RowHeadersVisible = false;
            while (table <= index)
            {
                if (dataGridView1.Visible)
                {
                    grid = dataGridView1;
                }
                else { grid = reportView[table]; }




                if (dataGridView1.Visible) grid = dataGridView1;




                Cursor.Current = Cursors.WaitCursor;
                //if (dataGridView1.Visible)
                //{


                // Write the header row
                for (int i = 0; i < grid.Columns.Count; i++)
                {
                    csvContent.Append(grid.Columns[i].HeaderText);
                    if (i < grid.Columns.Count - 1)
                    {
                        csvContent.Append(",");
                    }
                }
                csvContent.AppendLine();

                // Write the data rows
                foreach (DataGridViewRow row in grid.Rows)
                {
                    if (!row.IsNewRow) // Skip adding the new row placeholder
                    {
                        for (int i = 0; i < grid.Columns.Count; i++)
                        {
                            csvContent.Append("\"" + row.Cells[i].Value?.ToString() + "\"");
                            if (i < grid.Columns.Count - 1)
                            {
                                csvContent.Append(",");
                            }
                        }
                        csvContent.AppendLine();
                    }
                }
                csvContent.AppendLine();
                table++;
            }
            string fname = xlFileName + "_" + DateTime.Now.ToString("MMddyy_hh_mm") + ".csv";
            // Write to file
            File.WriteAllText(fname, csvContent.ToString());

            // Open the CSV file with Excel
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "excel.exe",
                    Arguments = "\"" + fname + "\"",
                    UseShellExecute = true
                };
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to open Excel: " + ex.Message);
            }
            Cursor.Current = Cursors.Default;


            //else
            //    {




            //        Excel.Application app = null;
            //        Excel.Workbook book = null;
            //        Excel.Sheets sheets = null;
            //        Excel.Worksheet sheet = null;
            //        Excel.Range range = null;
            //        int table = 0, rownum = 4;
            //        object misValue = System.Reflection.Missing.Value;





            //        string tempPath = System.IO.Path.GetTempFileName();


            //        // System.IO.File.WriteAllBytes(tempPath, ICR2.Properties.Resources.template);


            //        app = new Excel.Application();

            //        book = app.Workbooks.Open(xlOpenPath + xlTemplate + ".xlsm");
            //        //  book = app.Workbooks.Open(tempPath);
            //        // book.AutoUpdateSaveChanges = false;
            //        sheets = book.Sheets;
            //        sheet = sheets.get_Item(1);

            //        sheet.Cells.ClearContents();
            //        ////////////if (xlTemplate.Equals("template"))
            //        ////////////{
            //        ////////////    sheet.Cells.ClearContents();
            //        ////////////    //sheet.Cells[1, 3].Value = lblTitle.Text;
            //        ////////////}
            //        // sheet.Cells.Interior.Color = -4142;
            //        DataGridView grid;
            //        //var contents = reportView[table].GetClipboardContent();
            //        if (index == -1) table = -1;
            //        //    dataGridView1.RowHeadersVisible = false;
            //        while (table <= index)
            //        {
            //            int xlctr = 1;

            //            if (dataGridView1.Visible == true)
            //            {
            //                grid = dataGridView1;



            //                for (int ctr = 0; ctr < grid.Columns.Count; ctr++)
            //                {
            //                    if (grid.Columns[ctr].Visible == true)
            //                    {
            //                        if (gridRecordCount == dataGridView1.RowCount && reportId != 14)
            //                        {
            //                            range = (Excel.Range)sheet.Cells[2, xlctr];
            //                        }
            //                        else
            //                        {
            //                            range = (Excel.Range)sheet.Cells[3, xlctr + 1];
            //                        }
            //                        range.Value = "'" + grid.Columns[ctr].Name.ToString();
            //                        xlctr++;
            //                    }
            //                    table = index + 1;
            //                }
            //            }
            //            else
            //            {

            //                grid = reportView[table];
            //                for (int ctr = 0; ctr < grid.Columns.Count; ctr++)
            //                {

            //                    range = (Excel.Range)sheet.Cells[rownum - 1, ctr + 2];
            //                    range.Value = "'" + grid.Columns[ctr].Name.ToString().Trim();

            //                }
            //            }


            //            //for (int i = 0; i < reportView[table].Rows.Count ; i++)
            //            //{
            //            //    for (int j = 0; j < reportView[table].Columns.Count; j++)
            //            //    {
            //            //        sheet.Cells[rownum, j + 2] = reportView[table].Rows[i].Cells[j].Value.ToString()=="0"? "": reportView[table].Rows[i].Cells[j].Value.ToString();

            //            //    }
            //            //    rownum++;
            //            //}
            //            rownum += grid.Rows.Count;



            //            //app.Visible = true;

            //            if (dataGridView1.Visible == true && gridRecordCount == dataGridView1.RowCount && reportId != 14)
            //            {
            //                // range = (Excel.Range)sheet.Range[sheet.Cells[4, 2], sheet.Cells[4, xlctr - 1]];
            //                //  sheet.Paste(range);

            //                sheet.Cells[1, 2] = xlSql;
            //                sheet.Cells[1, 1] = "set";
            //                // range = (Excel.Range)sheet.Range[sheet.Cells[3, 2], sheet.Cells[rownum - 1, xlctr - 1]];
            //            }
            //            else
            //            {
            //                grid.MultiSelect = true;
            //                grid.SelectAll();

            //                var contents = grid.GetClipboardContent();
            //                grid.ClearSelection();
            //                //    reportView[index].MultiSelect = false;
            //                Clipboard.SetDataObject(contents);
            //                range = (Excel.Range)sheet.Range[sheet.Cells[(rownum - grid.Rows.Count), 2], sheet.Cells[rownum - 1, grid.Columns.Count + 1]];
            //                sheet.Paste(range);
            //                sheet.Cells[1, 1] = "set";
            //                range = (Excel.Range)sheet.Range[sheet.Cells[(rownum - grid.Rows.Count) - 1, 2], sheet.Cells[rownum - 1, grid.Columns.Count + 1]];

            //                if (xlTemplate.Equals("template"))
            //                {
            //                    sheet.Application.Goto(range);
            //                    // range.Select();
            //                }
            //                else
            //                {
            //                    sheet.Application.Goto(range);
            //                }
            //                grid.MultiSelect = false;
            //            }
            //            rownum += 2;
            //            table++;


            //        }
            //        //  dataGridView1.RowHeadersVisible = true;
            //        range = sheet.get_Range("C1", "I1000");
            //        range.EntireColumn.AutoFit();
            //        //  sheet.Cells[1, 1] = "";
            //        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            //        string filename = xlSavePath + xlFileName + "_" + timestamp + ".csv";
            //        if (dataGridView1.Visible == true && index > 0)
            //        {
            //            filename = xlSavePath + "Detail " + xlFileName + "_" + timestamp + ".xlsx"; ;
            //        }
            //        //sheet.Range(sheet.Cells[3,3], sheet.Cells[reportView[index].Rows.Count+6 , reportView[index].Columns.Count + 3]).Select();
            //        // sheet.Range(range).Select();
            //        //////////////////if (xlSql != "")
            //        //////////////////{
            //        //////////////////    table = 0; rownum = 4;
            //        //////////////////    sheet = sheets.get_Item("sql");
            //        //////////////////    sheet.Cells.ClearContents();
            //        //////////////////    while (table <= index)
            //        //////////////////    {
            //        //////////////////        string rowHead;
            //        //////////////////        string detailHead;
            //        //////////////////        string where;
            //        //////////////////        for (int i = 0; i < reportView[table].Rows.Count; i++)
            //        //////////////////        {
            //        //////////////////            rowHead = reportView[table].Rows[i].Cells[0].Value.ToString();
            //        //////////////////            if (rowHead != "" && rowHead != "Total")
            //        //////////////////            {
            //        //////////////////                detailHead = "default";
            //        //////////////////            }
            //        //////////////////            else
            //        //////////////////            {
            //        //////////////////                detailHead = rowHead;
            //        //////////////////                if (rowHead == "Total") { rowHead = ""; }
            //        //////////////////            }
            //        //////////////////            if (rowHead != "") rowHead = "'" + rowHead + "'";
            //        //////////////////            for (int j = 1; j < reportView[table].Columns.Count; j++)
            //        //////////////////            {
            //        //////////////////                DataRow[] row = detail.Select("header = '" + detailHead + "' and table = " + table + " and column = '" + j + "'");
            //        //////////////////                if (row.Length.Equals(0) && rowHead != "")
            //        //////////////////                {
            //        //////////////////                    row = detail.Select("header = " + rowHead + " and table = " + table + " and column = '" + j + "'");
            //        //////////////////                }
            //        //////////////////                if (row.Length.Equals(0))
            //        //////////////////                {
            //        //////////////////                }
            //        //////////////////                else
            //        //////////////////                {
            //        //////////////////                    if (row[0][6].ToString() == "")
            //        //////////////////                    {
            //        //////////////////                        where = "";
            //        //////////////////                    }
            //        //////////////////                    else
            //        //////////////////                    {
            //        //////////////////                        where = rowHead;
            //        //////////////////                        if (row[0][6].ToString().Substring(row[0][6].ToString().Length - 1, 1) == "#") { where = where.Replace("'", "#"); }
            //        //////////////////                    }


            //        //////////////////                    sheet.Cells[rownum, j + 2] = "select * from [Data$] where" + row[0][5].ToString() + row[0][6].ToString().Replace("where AND", "where ").Replace("#", "") + where;
            //        //////////////////                    //  MessageBox.Show("select * from [Data$] where" + row[0][5].ToString() + row[0][6].ToString().Replace("where AND", "where ").Replace("#", "") + where);
            //        //////////////////                }
            //        //////////////////            }
            //        //////////////////            rownum++;
            //        //////////////////        }
            //        //////////////////        rownum += 2;
            //        //////////////////        table++;
            //        //////////////////    }
            //        //////////////////    // panel.Controls.Add(xlGrid);
            //        //////////////////    //gridXl.Visible = true;

            //        //////////////////    setdata(xlSql, xlGrid, 0, false, false);

            //        //////////////////    sheet = sheets.get_Item("data");
            //        //////////////////    sheet.Cells.ClearContents();
            //        //////////////////    DataTable d = ds.Tables[0];
            //        //////////////////    for (int col = 0; col < d.Columns.Count; col++)
            //        //////////////////    {
            //        //////////////////        sheet.Cells[1, col + 1].value = d.Columns[col].ColumnName;
            //        //////////////////    }
            //        //////////////////    xlGrid.SelectAll();

            //        //////////////////    // Get the selected rows from the grid
            //        //////////////////    var contents = xlGrid.GetClipboardContent();


            //        //////////////////    // Copy selected rows into clipboard
            //        //////////////////    Clipboard.SetDataObject(contents);


            //        //////////////////    // Set starting cell on Excel worksheet
            //        //////////////////    Excel.Range targetRange = sheet.Cells[2, 1];


            //        //////////////////    // Paste into target cell
            //        //////////////////    sheet.Paste(targetRange);
            //        //////////////////}



            //        book.SaveAs(filename);

            //        //////////////if ((xlTemplate.Equals("template") || dataGridView1.Visible == true) && reportId != 14)
            //        //////////////{

            //        //////////////    book.Close(true, misValue, misValue);
            //        //////////////    app.Quit();
            //        //////////////    MessageBox.Show("File succefully Created as " + filename.ToString());
            //        //////////////}
            //        //////////////else
            //        //////////////{
            //            app.Visible = true;
            //        //////////////}
            //        //   app.Visible = true;


            //        releaseObject(sheet);
            //        releaseObject(sheets);
            //        releaseObject(book);
            //        releaseObject(app);
            //        pnlExport.Visible = false;
            //        ShowForm();
            //    }
        }

        private void lblExport_Click(object sender, EventArgs e)
            {
                //    Cursor.Current = Cursors.WaitCursor;

                //    return;
                //    if (dataGridView1.Visible)
                //    {
                //        StringBuilder csvContent = new StringBuilder();

                //        // Write the header row
                //        for (int i = 0; i < dataGridView1.Columns.Count; i++)
                //        {
                //            csvContent.Append(dataGridView1.Columns[i].HeaderText);
                //            if (i < dataGridView1.Columns.Count - 1)
                //            {
                //                csvContent.Append(",");
                //            }
                //        }
                //        csvContent.AppendLine();

                //        // Write the data rows
                //        foreach (DataGridViewRow row in dataGridView1.Rows)
                //        {
                //            if (!row.IsNewRow) // Skip adding the new row placeholder
                //            {
                //                for (int i = 0; i < dataGridView1.Columns.Count; i++)
                //                {
                //                    csvContent.Append("\""+row.Cells[i].Value?.ToString() +"\"");
                //                    if (i < dataGridView1.Columns.Count - 1)
                //                    {
                //                        csvContent.Append(",");
                //                    }
                //                }
                //                csvContent.AppendLine();
                //            }
                //        }
                //        string fname = xlFileName + "_" + DateTime.Now.ToString("MMddyy_hh_mm") + ".csv";
                //        // Write to file
                //        File.WriteAllText(fname, csvContent.ToString());

                //        // Open the CSV file with Excel
                //        try
                //        {
                //            ProcessStartInfo startInfo = new ProcessStartInfo
                //            {
                //                FileName = "excel.exe",
                //                Arguments = "\"" + fname + "\"",
                //                UseShellExecute = true
                //            };
                //            Process.Start(startInfo);
                //        }
                //        catch (Exception ex)
                //        {
                //            MessageBox.Show("Unable to open Excel: " + ex.Message);
                //        }
                //    }
                //    Cursor.Current = Cursors.Default;
            }
       
            private void lblExport_MouseEnter(object sender, EventArgs e)
            {
                pnlExport.Visible = true;



            }

        private void lblRange_MouseEnter(object sender, EventArgs e)
        {

        }

        private void panel_Paint(object sender, PaintEventArgs e)
        {

        }

        private void lblOutLook_Click(object sender, EventArgs e)
        {
            hide_panels();
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void pnlExport_Paint(object sender, PaintEventArgs e)
        {

        }


        private void lblMonthly_Click(object sender, EventArgs e)
        {
            int ctr, colctr,   monthAmount, fldCtr;
            string caseSql,  detailSql,  shortDesc, sql;
            DateTime endDate, startDate;
            DataTable extracted = new DataTable();


            
            if (!refresh) { 
            var today = DateTime.Today;
            to = new DateTime(today.Year, today.Month, 1);
        }
            setReport(to.AddMonths(-6), to.AddDays(-1), 3);
            from = new DateTime(from.Year, from.Month, 1);
            to = new DateTime(to.Year, to.Month, 1);
            DateTime tmp = from;
            monthAmount = 0;
            while (tmp < to)
            {
                monthAmount++;
                tmp = tmp.AddMonths(1);
            }




            setGrid();



            detailSql = @"select concat(d.ufname, ' ' , d.ulname)  [Doctor], e.patientid [Patient ID], u.ulname [Last Name],u.ufname [First Name], v.id[Claim ID], e.date [Appt Date], v.SubmittedDate Submitted, i.insuranceName [Primary Insurance],case when si.insuranceName is not null then si.insuranceName else '' end[Secondary Insurance],
           v.NetPayment - v.PtPayment[Primary Payment],
            CASE WHEN   v.id is null and e.encLock = 0 and (v.id is null  or  v.SubmittedDate <= '1999-01-01' ) THEN 'Unsubmitted Unlocked'
                    WHEN    v.id is null and e.encLock = 1  and (v.id is null  or  v.SubmittedDate <= '1999-01-01' ) THEN 'Unsubmitted Locked'
                    WHEN  sc.code = 'CMS' and L.DATE <= getdate() - 42 THEN 'Submitted over six Weeks ago'
                   WHEN sc.code = 'CMS' and L.DATE > getdate() - 42 THEN 'Submitted' ELSE sc.ShortDesc END [Description], V.FILESTATUS [File Status], 
           case when  e.claimreq = 1 and  ( v.id is null or v.SubmittedDate <= '1999-01-01') and sc.code != 'CMS' then 'Un Submitted'  
        when  v.SubmittedDate > '1999-01-01' then 'Submitted Unpaid' 
when v.SubmittedDate > '1999-01-01' or sc.code = 'CMS' then 'Submitted' else '' end [Pay Status] 
          from  enc e " + defaultJoin + @" left join  (select tostatus stat, invid id, date date ,    ROW_NUMBER() OVER(PARTITION BY invid, tostatus ORDER BY date desc)  rn 
                   from   edi_inv_claimstatus_log   ) l on v.id = l.id and v.FileStatus = l.stat and rn = 1 
             where  e.claimreq = 1 and (v.deleteflag is null or v.deleteflag = 0) and e.deleteFlag = 0  AND v.primaryinsid !='' ";
            connection = new SqlConnection(connectionString);
            connection.Open();
            extracted.Reset();
            extracted.Columns.Add("Visits");
            for (colctr = monthAmount; colctr >= 1; colctr--)
            {
                extracted.Columns.Add(to.AddMonths(-(colctr )).ToString("MMMM") + " '" + to.AddMonths(-(colctr )).ToString("yy"));
            }
            extracted.Columns.Add("Total");

            string[] fieldName = { "Billable Visits", "Submitted", "Unsubmitted", "Submitted Unpaid" };
            string[] sqlWhere = { @"", " and ( v.SubmittedDate > '1999-01-01' or sc.code = 'CMS')  ", " and (v.id is null  or  v.SubmittedDate <= '1999-01-01' ) and sc.code != 'CMS'  ", @"
                                     and v.SubmittedDate > '1999-01-01' " };

            string[] xlSqlWhere = { @"", " and [Pay Status] LIKE  'Submitted%'  ", " and [Pay Status] =  'Un Submitted'  ", " and [Pay Status] = 'Submitted Unpaid'  " };



            for (int fieldctr = 0; fieldctr < 4; fieldctr++)
            {
                sql = @"select distinct '" + fieldName[fieldctr] + "' Visits,  ";
                for (colctr = monthAmount; colctr >= 1; colctr--)
                {
                    startDate = to.AddMonths(-(colctr ));
                    endDate = to.AddMonths(1 - (colctr )).AddDays(-1);
                    sql += " count(DISTINCT case when ( e.date between '" + startDate + "' and '" + endDate + "') " + sqlWhere[fieldctr] + "  then e.EncounterId End) ,";
                }
                startDate = from;
                endDate = to.AddDays(-1);
                sql += " count(DISTINCT case when ( e.date between '" + startDate + "' and '" + endDate + "') " + sqlWhere[fieldctr] + " then e.EncounterId End) Total";
                sql += " from enc e " + defaultJoin + " where e.claimreq = 1 and  e.deleteFlag = 0  and(v.deleteflag is null or v.deleteflag = 0)   AND (  v.primaryinsid !='') ";


                SqlCommand cmd = new SqlCommand(sql, connection);

                SqlDataReader dr = cmd.ExecuteReader();

                dr.Read();
                DataRow rec = extracted.NewRow();
                for (colctr = 0; colctr <= monthAmount + 1; colctr++)
                {
                    rec[colctr] = dr[colctr];
                }

                dr.Close();
                extracted.Rows.Add(rec);
            }

            DataRow record = extracted.NewRow();
            record[0] = "Submitted Percent";

            for (colctr = 1; colctr <= monthAmount + 1; colctr++)
            {
                int quotient = Convert.ToInt32(extracted.Rows[0][colctr].ToString());
                int number = Convert.ToInt32(extracted.Rows[1][colctr].ToString());
                double product = (double)number / quotient;
                record[colctr] = product.ToString("P0");
            }
            extracted.Rows.Add(record);
            // Percent of Billable Visits Paid = (Submitted – Submitted Unpaid) / Billable Visits
            record = extracted.NewRow();

            record[0] = "Paid Percent";
            for (colctr = 1; colctr <= monthAmount + 1; colctr++)
            {
                int quotient = Convert.ToInt32(extracted.Rows[1][colctr].ToString()) - Convert.ToInt32(extracted.Rows[3][colctr].ToString());
                int number = Convert.ToInt32(extracted.Rows[0][colctr].ToString());
                double product = quotient / (double)number;
                record[colctr] = product.ToString("P0");
            }
            extracted.Rows.Add(record);

            //Percent of Bills Submitted &Paid = (Submitted – Submitted Unpaid) / Submitted
            record = extracted.NewRow();
            record[0] = "Submitted Paid Percent";
            for (colctr = 1; colctr <= monthAmount + 1; colctr++)
            {
                int quotient = Convert.ToInt32(extracted.Rows[1][colctr].ToString()) - Convert.ToInt32(extracted.Rows[3][colctr].ToString());
                int number = Convert.ToInt32(extracted.Rows[1][colctr].ToString());
                double product = quotient / (double)number;
                record[colctr] = product.ToString("P0");
            }
            extracted.Rows.Add(record);
            // Percent of Bills Submitted Unpaid = Submitted Unpaid / Submitted
            record = extracted.NewRow();
            record[0] = "Submitted Un-Paid Percent";
            for (colctr = 1; colctr <= monthAmount + 1; colctr++)
            {
                int quotient = Convert.ToInt32(extracted.Rows[3][colctr].ToString());
                int number = Convert.ToInt32(extracted.Rows[1][colctr].ToString());
                double product = quotient / (double)number;
                record[colctr] = product.ToString("P0");
            }
            extracted.Rows.Add(record);
            reportView[index].AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            reportView[index].DataSource = extracted;
            reportView[index].ClientSize = new Size(reportView[index].Columns.GetColumnsWidth(DataGridViewElementStates.None) + 145, (reportView[index].Rows.Count * reportView[index].Rows[1].Height) + 40);

            for (int strCtr = 0; strCtr < 4; strCtr++)
            {
                fldCtr = 1;
                for (colctr = monthAmount; colctr >= 1; colctr--)
                {
                    startDate = to.AddMonths(-(colctr ));
                    endDate = to.AddMonths(1 - (colctr )).AddDays(-1);
                    setDetail(index, fldCtr, fieldName[strCtr], detailSql + " and e.date between '" + startDate + "' and '" + endDate + "' " + sqlWhere[strCtr], "", xlSqlWhere[strCtr] + " and [Appt Date] between #" + startDate + "# and #" + endDate + "#", "");
                    fldCtr++;
                }
                setDetail(index, fldCtr, fieldName[strCtr], detailSql + " and e.date between '" + from + "' and '" + to.AddDays(-1) + "'" + sqlWhere[strCtr], "", xlSqlWhere[strCtr] + " and [Appt Date] between #" + from + "# and #" + to.AddDays(-1) + "#", "");
            }

            xlSql = detailSql + "  and e.date between '" + from + "' and '" + to + "'";
            reportView[index].ClearSelection();
            //table two  
            shortDesc = " case when  sc.code != 'CMS' then sc.shortdesc WHEN L.DATE >    getdate()- 42   THEN sc.shortdesc ELSE 'Submitted over six Weeks ago' end  ";
            for (index = 1; index < 3; index++)

            {

                if (index == 2)
                {
                    caseSql = "select " + shortDesc + " [Status of Denied Claims], ";
                }
                else
                {
                    caseSql = @"select case when v.id is null and e.encLock = 0 then 'Unsubmitted Unlocked' 
                    when v.id is null and e.encLock = 1 then 'Unsubmitted Locked' 
                     else sc.shortdesc End Unsubmitted,";
                }
                for (colctr = monthAmount; colctr >= 1; colctr--)
                {
                    startDate = to.AddMonths(-(colctr ));
                    endDate = to.AddMonths(1 - (colctr )).AddDays(-1);

                    caseSql += @" sum( CASE WHEN E.DATE between '" + startDate + "' and '" + endDate + "' then 1 else 0 end ) [" + startDate.ToString("MMMM") + " '" + startDate.ToString("yy") + "] ,";
                }

                caseSql += @" sum( CASE WHEN E.DATE between '" + from + "' and '" + to + "' then 1 else 0 end) Total ";
                string fromstr = @"from enc e " + defaultJoin + @" where e.claimreq = 1 and   e.deleteFlag = 0 and (v.deleteflag is null or v.deleteflag = 0)  and (  v.primaryinsid !='' )  and ( v.id is null or v.SubmittedDate <= '1999-01-01') ";
               if (index == 1)
                {
                    sql = caseSql + fromstr + " and sc.code != 'CMS' AND (E.DATE BETWEEN '" + from + "' AND '" + to.AddDays(-1) + @"')  group by case when v.id is null and e.encLock = 0 then 'Unsubmitted Unlocked' 
     when v.id is null and e.encLock = 1 then 'Unsubmitted Locked' 
      else sc.shortdesc End order by case when v.id is null and e.encLock = 0 then 'Unsubmitted Unlocked' 
    when v.id is null and e.encLock = 1 then 'Unsubmitted Locked'   else sc.shortdesc End ";
                }
                else
                {
                    sql = caseSql + @" from enc e " + defaultJoin +
       @" left join  (select tostatus stat, invid id, date date ,    ROW_NUMBER() OVER(PARTITION BY invid, tostatus ORDER BY date desc)  rn 
                   from   edi_inv_claimstatus_log   ) l on v.id = l.id and v.FileStatus = l.stat and rn = 1 
   where    e.deleteFlag = 0  and (v.deleteflag is null or v.deleteflag = 0)  
   and  v.SubmittedDate > '1999-01-01'  AND (E.DATE BETWEEN '" + from + "' AND '" + to.AddDays(-1) + @"')  group by " + shortDesc + " order by " + shortDesc;
                }



                //index++;
                setGrid();
                reportView[index].AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                setdata(sql, reportView[index], 0, true, false);
                connection.Close();

                reportView[index].Width = reportView[index - 1].Width;
                reportView[index].Height = (reportView[index].Rows.Count * reportView[index].Rows[1].Height) + 30;
                if (reportView[index].Height > 550) reportView[index].Height = 550;
                reportView[index].Top = reportView[index - 1].Top + reportView[index - 1].Height + 20;
                string sqlTable1 = index == 1 ? "  and (v.id is null  or  v.SubmittedDate <= '1999-01-01' ) and sc.code != 'CMS'" : "";
                for (ctr = 0; ctr < reportView[0].ColumnCount; ctr++) reportView[0].Columns[ctr].Width = reportView[1].Columns[ctr].Width;
                fldCtr = 1;
                for (colctr = monthAmount; colctr >= 0; colctr--)
                {

                    startDate = to.AddMonths(-(colctr ));
                    endDate = to.AddMonths(1 - (colctr )).AddDays(-1);
                    if (colctr == 0)
                    {
                        startDate = from;
                        endDate = to.AddDays(-1);
                    }

                    if (index == 1)
                    {
                        setDetail(index, fldCtr, "Total", detailSql + sqlTable1 + "  and e.date between '" + startDate + "' and '" + endDate + "' ", " ", " and [Appt Date] between #" + startDate + "# and #" + endDate + "# and [Pay Status] = 'Un Submitted'", " ");
                        setDetail(index, fldCtr, "Default", detailSql + sqlTable1 + "  and e.date between '" + startDate + "' and '" + endDate + "' ", " and sc.shortdesc =  ", " and [Appt Date] between #" + startDate + "# and #" + endDate + "#", " and [Pay Status] = 'Un Submitted' and [Description] = ");
                        setDetail(index, fldCtr, "Unsubmitted Unlocked", detailSql + " and  v.id is null and e.encLock = 0 and (v.id is null  or  v.SubmittedDate <= '1999-01-01' )  and e.date between '" + startDate + "' and '" + endDate + "' ", "", " and [Appt Date] between #" + startDate + "# and #" + endDate + "#", " and [Pay Status] = 'Un Submitted' and [Description] = ");
                        setDetail(index, fldCtr, "Unsubmitted Locked", detailSql + " and v.id is null and e.encLock = 1  and (v.id is null  or  v.SubmittedDate <= '1999-01-01' )  and e.date between '" + startDate + "' and '" + endDate + "' ", "", " and [Appt Date] between #" + startDate + "# and #" + endDate + "#", " and [Pay Status] = 'Un Submitted' and [Description] = ");
                    }
                    else
                    {
                        setDetail(index, fldCtr, "Total", detailSql + sqlTable1 + "  and e.date between '" + startDate + "' and '" + endDate + "' ", " ", " and [Appt Date] between #" + startDate + "# and #" + endDate + "# and [Pay Status] = 'Submitted Unpaid'", " ");
                        setDetail(index, fldCtr, "Default", detailSql + sqlTable1 + "  and e.date between '" + startDate + "' and '" + endDate + "' ", " and sc.shortdesc =  ", " and [Appt Date] between #" + startDate + "# and #" + endDate + "#", " and [Pay Status] = 'Submitted Unpaid' and [Description] = ");
                        setDetail(index, fldCtr, "Submitted over six Weeks ago", detailSql + " and sc.code = 'CMS' and L.DATE <= getdate() - 42  and e.date between '" + startDate + "' and '" + endDate + "' ", "", " and [Appt Date] between #" + startDate + "# and #" + endDate + "#", " and [Pay Status] = 'Submitted Unpaid' and [Description] = ");
                        setDetail(index, fldCtr, "Submitted", detailSql + " and sc.code = 'CMS' and L.DATE > getdate() - 42  and e.date between '" + startDate + "' and '" + endDate + "' ", "", " and [Appt Date] between #" + startDate + "# and #" + endDate + "#", " and [Pay Status] = 'Submitted Unpaid' and [Description] = ");

                    }
                    fldCtr++;
                }

                detailSql = @"select concat(d.ufname, ' ' , d.ulname)  [Doctor], e.patientid [Patient ID],u.ulname [Last Name],u.ufname [First Name],v.id[Claim ID], v.SubmittedDate Submitted, concat(d.ufname, ' ' , d.ulname)  [Doctor],i.insuranceName [Primary Insurance],case when si.insuranceName is not null then si.insuranceName else '' end[Secondary Insurance],
 v.NetPayment - v.PtPayment[Primary Payment], sc.ShortDesc[Description] from  enc e "
  + defaultJoin + @" left join  (select tostatus stat, invid id, date date ,    ROW_NUMBER() OVER(PARTITION BY invid, tostatus ORDER BY date desc)  rn 
                 from   edi_inv_claimstatus_log  ) l on v.id = l.id and v.FileStatus = l.stat and rn = 1 
    where (v.deleteflag is null or v.deleteflag = 0) and e.deleteFlag = 0   and  v.primaryinsid !=''  and v.SubmittedDate > '1999-01-01' ";
                //return;
            }
            to = to.AddDays(-1);
            index = 2;
            extracted.Dispose();
            ShowForm();

        }

        private void lblClose_Click(object sender, EventArgs e)
        {

            panel.Visible = true;
            dataGridView1.Visible = false;
            grpDashBoard.Visible = false;

        }

        private void lblClAttachment_Click(object sender, EventArgs e)
        {
            hide_panels();


            setReport(DateTime.Today.AddDays(-30), DateTime.Today.AddDays(0), 4);
            setGrid();

            string sql = @"select concat(u.ulname, ' ', u.ufname) Name, insd.subscriberNo[Medicaid Number],  e.date Date, i.insuranceName Insurance,
case when v.PrimaryInsId in (4, 5, 7, 9, 25, 26, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 48, 57, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70, 76, 77, 78, 82,
                           83, 84, 85, 88, 92, 93, 94, 95, 96, 97, 98, 99, 100, 102, 106, 107, 108, 109, 110, 113, 114, 115, 116, 117, 118, 120, 121, 124, 125, 128, 129, 134, 135, 136, 138,
                           139, 142, 148, 149, 150, 151, 152, 153, 154, 155, 156, 157, 158, 159, 160, 161, 162, 163, 169, 171, 172, 173, 176, 177, 178, 179, 180, 181, 182, 183, 184, 185,
                           190, 191, 192, 193, 194, 195, 196, 197, 198, 199, 200, 206, 214, 223, 228, 229, 230, 233, 240, 243, 264, 265, 266, 274, 280, 284, 285, 294, 310, 327, 329, 330,
                           333, 336, 338, 341, 351, 352, 353, 355, 356, 357, 358) Then 'Yes' else 'No' end Contracted,
         cpt.code CPT, case when sc.CODE = '45RE' THEN LOGSC.ShortDesc ELSE sc.ShortDesc END  Status,'No' Appealed from edi_invoice v
left join edi_invoice s on v.splitclaimid = s.id
left join insurancedetail insd on v.PatientId = insd.pid  and insd.DentalIns = 0 and insd.DeleteFlag = 0
left join users u on v.PatientId = u.uid
left join enc e on v.encounterid = e.encounterid
left join insurance i on v.PrimaryInsId = i.insId
left join(select min(id) id, invoiceid inv  from edi_inv_cpt where cpt.deleteflag= 0 group by invoiceid) invcpt on v.id = invcpt.inv
left join  edi_inv_cpt cpt on invcpt.id = cpt.id 
left join claimstatuscodes sc on v.FileStatus = sc.code
left join(SELECT max(id) id, invid vid, tostatus tostat from edi_inv_claimstatus_log group by invid, tostatus) sclog on v.id = sclog.vid and sclog.ToStat = v.FileStatus
left join edi_inv_claimstatus_log log on sclog.id = log.id
left join claimstatuscodes logsc on log.FromStatus = logsc.code
where s.id is not null and s.PrimaryInsId = 120 and insd.insid = 78 
  and v.SubmittedDate >= '" + from + "' and v.SubmittedDate <= '" + to + "'  and e.deleteFlag = 0  and (v.deleteflag is null or v.deleteflag = 0 )  ";
            setdata(sql, reportView[index], 0, false, true);
            ShowForm();

        }

        private void lblPharmacyStats_Click(object sender, EventArgs e)
        {
            hide_panels();


            setReport(DateTime.Today.AddDays(-30), DateTime.Today.AddDays(0), 27);
            setGrid();
            string sql = @"select  L.PharmacyName[Pharmacy Name], sum(1) Amount 
 FROM(select i.insuranceName Insurance, e.date Date,  e.encounterid encounter from edi_invoice v 
left join enc e on v.id = e.InvoiceId
left join edi_facilities f on e.facilityId = f.id
left join Insurance i on v.PrimaryInsId = i.insId  WHERE  e.status = 'chk'  and  e.deleteflag = 0 and e.deleteflag = 0) e
  left join rxhub_scriptlog L on E.encounter = l.encounterID
 LEFT JOIN surescript_eprescription P ON L.RxOrderNo = p.RxOrderNo and p.encounterID = e.encounter where l.ID is not null
 and E.DATE BETWEEN '" + from + "' and '" + to + "'  group by L.PharmacyName  ";


            string detailSql = @"SELECT  e.encounter[Encounter ID],  e.facility  Facility, CONCAT(P.doc_FName, ' ', P.doc_LName) Prescriber, L.DrugName[Drug Name], L.NDCCode[NDC Code] ,L.Dosage, P.Strength,
 L.FORMULATION , P.Quantity, L.Refills, E.date Date, L.RxChannel, L.PharmacyName[Pharmacy Name], L.NPI, L.NCPDPID, e.insurance Insurance
 FROM(select i.insuranceName Insurance, e.date Date, f.name facility, e.encounterid encounter from edi_invoice v 
left join enc e on v.id = e.InvoiceId
left join edi_facilities f on e.facilityId = f.id
left join Insurance i on v.PrimaryInsId = i.insId  WHERE  e.status = 'chk' and e.deleteflag = 0 and e.deleteflag = 0) e
  left join rxhub_scriptlog L on E.encounter = l.encounterID

 LEFT JOIN surescript_eprescription P ON L.RxOrderNo = p.RxOrderNo and p.encounterID = e.encounter where l.ID is not null
 and E.DATE BETWEEN '" + from + "' and '" + to + "'";

            

            //sql = detailSql; // @"";

            setDetail(index, 1, "default",  detailSql , " and L.PharmacyName = ", "", "");
            
            setDetail(index, 1, "Total",  detailSql, "", "", "");


            xlSql = detailSql;

            
            setdata(sql, reportView[index], 0, true, true);
            ShowForm();
        }

        private void lblRange_Click(object sender, EventArgs e)
        {

            frmCal cal = new frmCal();

            cal.StartPosition = FormStartPosition.Manual;
            cal.Top = lblRange.Top + 50;
            cal.Left = lblRange.Left;

            cal.ShowDialog();

            if (refresh)
            {
                refreshData(sender, e);


            }
            //setReport(from, to,reportId);
            //grpCal.BringToFront();
            //grpCal.Visible = true;
        }
        public void refreshData(object sender, EventArgs e)
        {
            if (reportId == 1)
            {
                lblClaimStatus_Click(sender, e);
            }
            else if (reportId == 2)
            {
                lblUnlockedNotes_Click(sender, e);
            }
            else if (reportId == 3)
            {
                lblMonthly_Click(sender, e);
            }
            else if (reportId == 4)
            {
                lblClaimStatus_Click(sender, e);
            }
            else if (reportId == 5)
            {
                lblZocDoc_Click(sender, e);
            }
            else if (reportId == 6)
            {
                lblDataView_Click(sender, e);
            }
            else if (reportId == 7)
            {
                lbl4028_Click(sender, e);
            }
            else if (reportId == 9)
            {
                lblUnpaid_Click(sender, e);
            }
            else if (reportId == 10)
            {
                lblWrap_Click(sender, e);
            }
            else if (reportId == 11)
            {
                lblPatient_Click(sender, e);
            }
            else if (reportId == 13)
            {
                lblDuplicateVisits_Click(sender, e);
            }
            else if (reportId == 14)
            {
                lblIcd_Click(sender, e);
            }
            else if (reportId == 15)
            {
                lblVisits_Click(sender, e);
            }
            else if (reportId == 16)
            {
                lblClaimSubmissions_Click(sender, e);
            }
            else if (reportId == 17)
            {
                lblReceivedPayments_Click(sender, e);
            }
            else if (reportId == 18)
            {
                lblCrossOverClaims_Click(sender, e);
            }
            else if (reportId == 19)
            {
                lblCPT_Click(sender, e);
            }
            else if (reportId == 20)
            {
                lblCHPData_Click(sender, e);
            }
            else if (reportId == 21)
            {
                lblCPTCAS_Click(sender, e);
            }
            else if (reportId == 22)
            {
                lblReferral_Click(sender, e);
            }
            else if (reportId == 23)
            {
                lblCPTSpecific_Click(sender, e);
            }
            else if (reportId == 24)
            {
                lblSql_Click(sender, e);
            }
            else if (reportId == 25)
            {
                lblPatients_Click(sender, e);
            }
            else if (reportId == 26)
            {
                lblInsurancePaymentReconsiliation_Click(sender, e);
            }
            else if (reportId == 27)
            {
                lblPharmacyStats_Click(sender, e);
            }

        }
        private void cmdRefresh_Click(object sender, EventArgs e)
        {

         }

        private void pnlExport_Leave(object sender, EventArgs e)
        {
            hide_panels();
        }

        private void lblZocDoc_Click(object sender, EventArgs e)
        {

            setReport(DateTime.Today.AddDays(-30), DateTime.Today.AddDays(-1), 37);



            setGrid();
            string with = @"WITH visits  AS (
     select distinct ins_class,claim_id, Appt_date Appt_date, Patient,[cpt id], rn rn, case when ( [cpt id] like '%G0466%' and ins_class = 'MA') or (ins_class != 'MA' and ( [cpt id] like  '%99202%' or [cpt id] like  '%99203%' or [cpt id] like  '%99204%' or 
		   [cpt id] like  '%99205%' or [cpt id] like  '%99381%' or [cpt id] like  '%99382%' or [cpt id] like  '%99383%' or [cpt id] like  '%99384%' or [cpt id] like  '%99385%' or [cpt id] like  '%99386%' or [cpt id] like  '%99387%')) then
		   'Old submitted as New' else 'New submitted as Old' end patient_status
		   ,  doctor
		        from(
					   					select v.id claim_id,case when p.id in (18,6,11,24) then 'MA' WHEN P.ID = 7 then 'Medicare' else 'Other' end ins_class, e.doctorid doctor,
											v.servicedt Appt_date,v.PatientId Patient, ROW_NUMBER() over (PARTITION BY v.patientid ORDER BY v.servicedt desc)  rn ,
										stuff(( select ',', cpt.Code as [text()] from edi_inv_cpt cpt where cpt.InvoiceId = v.id order by displayindex  for xml path('')), 1, 1, '') [CPT ID]
										from enc e
										left join edi_invoice  v on e.invoiceid = v.id
                                        left join insurance i on v.PrimaryInsId = i.insId
										left join ins_payer_mix p on i.insuranceclass = p.Code and p.deleteflag = 0
										where v.id is not null and  v.DeleteFlag=0  and e.status = 'chk'
						              ) new	where  ((new.Appt_date between '" + from + "' and '" + to + @"') or (rn > 1))
),

current_visits as (select ins_class, claim_id, Appt_date , Patient,[cpt id],  patient_status
		   ,  doctor from visits where visits.rn = 1 ),

prev as ( select * from 
         (select c.Patient,c.Appt_date,c.claim_id, v.Appt_date prevdate, v.claim_id prev_claim_id, ROW_NUMBER() over (partition by v.Patient order by v.Appt_date desc) prn
          from visits v 
		  left join current_visits c on v.Patient = c.Patient   where v.claim_id < c.claim_id  and c.doctor = v.doctor and v.Appt_date > (c.Appt_date - 1095) 
		  )
		  h where h.prn = 1) ,
Medicare as ( select * from (select prev.*, v.Appt_date ma_date, v.claim_id ma_claim_id, ROW_NUMBER() over (partition by v.Patient order by v.Appt_date desc) maprn
          from visits v 
		  left join  prev on v.Patient = prev.Patient   where v.claim_id < prev.prev_claim_id and v.ins_class != 'Other'  and  v.Appt_date > (prev.Appt_date - 1095) 
		  )as h where h.maprn = 1
			  ) ";


            string detailSql = @"select c.ins_class HMO , c.claim_id [Claim ID], c.Appt_date [Appt Date], c.Patient [Patient ID],c.[cpt id][CPT ID],  patient_status [Error],d.printname Doctor, p.prevdate [Previous Date],
        p.prev_claim_id [Prev Claim ID],m.ma_date [Prev Medicaid Date] ,m.claim_id [Prev MDC Claim ID]  from current_visits c 
		left join prev p on c.Patient = p.Patient
		left join medicare m on c.Patient = m.Patient
		left join doctors d on c.doctor = d.doctorid
		where ((c.ins_class = 'Other' AND C.patient_status = 'New submitted as Old' and p.prev_claim_id is null) or 
		     (c.ins_class != 'Other' AND C.patient_status = 'New submitted as Old' and p.prev_claim_id is null and m.ma_date is null) or 
			 (c.ins_class = 'Other' AND C.patient_status = 'Old submitted as New' and p.prev_claim_id is not null ) or 
			 (c.ins_class != 'Other' AND C.patient_status = 'Old submitted as New' and (p.prev_claim_id is not null or  m.ma_date is not null))) ";

            string sql = with + @"select summary.error [Submission Error] ,sum(case when summary.hmo = 'Other' then 0 else 1 end) [Medicare],
                                         sum(case when summary.HMO = 'Other' then 1 else 0 end)[Other], sum(1) [Total] from(" + detailSql + @" ) summary group by summary.Error";

            //sql = detailSql; // @"";

            setDetail(index, 1, "default", with + detailSql + " and c.ins_class != 'Other'", " and c.patient_status = ", "", "");
            setDetail(index, 2, "default", with + detailSql + " and c.ins_class = 'Other'", " and c.patient_status = ", "", "");
            setDetail(index, 3, "default", with + detailSql, " and c.patient_status = ", "", "");

            setDetail(index, 1, "Total", with + detailSql + " and c.ins_class != 'Other'", "", "", "");
            setDetail(index, 2, "Total", with + detailSql + " and c.ins_class = 'Other'", "", "", "");
            setDetail(index, 3, "Total", with + detailSql, "", "", "");


            xlSql = detailSql;

            setdata(sql, reportView[index], 0, true, true);
            reportView[index].Columns[0].Width = 175;



            lblExport.ForeColor = Color.White;

            ShowForm();
        }

        private void pnlViews_MouseEnter(object sender, EventArgs e)
        {
            pnlViews.Visible = true;
        }

        public string PrepareSql(string select)
        {
            string sql = select;
            filterFields[1, 3] = "true";
            for (int i = 0; i < filterFields.GetLength(0); i++)
            {
                if (filterFields[i, 3] == "true")
                {
                    sql += filterFields[i, 0].ToString() + "[" + filterFields[i, 1].ToString() + "], ";
                    if (filterFields[i, 4] != "") { sqlFilter += filterFields[i, 4]; };
                }
            }
            if (developer) MessageBox.Show(sqlFilter + "[]" + sql);
            return sql;
        }
        private void DataView(string action)
        {
            sqlFilter = "";
            if (action == "default")
            {
                filterFields = new string[,]  {


{"pat.controlno ","Patient ID","string","true","",""},
 {"e.date ","Appt Date","date","true","",""},
 {"concat(upat.ulname, ' ', upat.ufname) ","Patient Name","string","true","",""},
 {"upat.ulname [Last Name],upat.ufname ","First Name","string","true","",""},
  {"concat( u.ulname, ' ' , u.ufname ) ","Doctor","filter","true","",""},
 {"d.speciality Speciality, e.visittype ","Visit Type","string","true","",""},
  {"case  when v.id = 0 then '' else v.id end ","Claim ID","int","true","",""},
  {"f.name","Facility","filter","true","",""},
  {"case when v.SubmittedDate <> v.firstSubmittedDate then  v.firstSubmittedDate end ","First Submitted Date","date","true","",""},
 {"v.SubmittedDate ","Last Submitted Date","date","true","",""},
  {"case when v.SubmittedDate > '2000-01-01' then 'Yes' else 'No' end ","Submitted","yesno","true","",""},
 {"case when v.SubmittedDate > '2000-01-01' then 1 else 0 end ","Submitted Count","10","true","",""},
  {"case when e.encLock = 1 then 'Yes' else 'No' end Locked, isnull(v.InvoiceAmount,0) ","Charges","yesno","true","",""},
  {"case when  v.netpayment - v.PtPayment > 0 then 'Yes' else 'No' end ","Insurance Payment","yesno","true","",""},
  {"case when   v.netpayment - v.PtPayment > 0 then 1 else 0 end ","Ins Pmnt Count","10","true","",""},
  {"case when   v.netpayment - v.PtPayment > 0 then 'Yes' else 'No' end ","Primary Insurance Payment","yesno","true","",""},
  {" isnull(v.netpayment,0 ) ","Total Payment","int","true","",""},
  {"isnull(v.netpayment,0) - isnull(v.PtPayment,0)  ","Insurance Amount","int","true","",""},
  {"isnull(  v.netpayment - v.PtPayment ,0) ","Primary insurance Amount","int","true","",""},
 {"sc.shortdesc ","Status","string","true","",""},
 
 {" isnull(v.PtPayment,0) ","Patient Payment","int","true","",""},
 {"isnull(si.insuranceName,'') ","Secondary Insurance","string","true","",""},
 {"isnull(ti.insuranceName,'') ","Tertiary Insurance","string","true","",""},
 {"isnull(i.insurancename ,'') ","Primary Insurance","string","true","",""},
 {"","CPT","string","true","",""},
  {"","CAS","string","true","",""},
 {"cpt.coinsurance ","Co Insurance","int","true","",""},
 {"cpt.Allowed ","Allowed","int","true","",""},
 {"cpt.Deductable ","Deductable","int","true","",""},
             };
                setReport(DateTime.Today.AddDays(-30), DateTime.Today.AddDays(0), 6);
            }
            string sql = PrepareSql(" select * from ( select  ") + @"from enc e
left join edi_facilities f on e.facilityId = f.id
  left join patients pat on e.patientid = pat.pid
left join users upat  on e.patientid = upat.uid
  left join edi_invoice v on e.encounterid = v.encounterid and v.deleteFlag = 0
  left join claimstatuscodes sc on v.FileStatus = sc.code left join doctors d on e.doctorid = d.doctorid
   left join insurance i on v.PrimaryInsId = i.insId  left join users u on e.resourceid = u.uid 
  left join insurance si on v.secondaryInsId = si.insId     left join ins_payer_mix p  on i.insuranceclass = p.Code and p.deleteflag = 0  
   left join insurance ti on v.TertiaryInsId = ti.insId   
  left join (select sum(b.coins) coinsurance, sum(b.Allowed) Allowed, sum(b.deduct) Deductable, v.InvoiceId invid from edi_inv_eob b 
   left join edi_inv_cpt  v on b.InvCptId = v.id and v.deleteflag= 0 group by v.InvoiceId ) cpt on e.invoiceid = cpt.invid where  
     e.deleteFlag = 0  and e.date between '" + from + "' and '" + to + "'   ) dv " + (sqlFilter == "" ? "" : " where" + sqlFilter) + " order by dv.[Appt Date]";
            sql = sql.Replace(", from", " from");
            sql = sql.Replace("where and", "where");
            sql = sql.Replace("where order", "where");

            xlSql = sql;
            //  xlSql = sql.Replace("[CPT],", "");

           // cpt.Code as [text()]  from edi_inv_cpt cpt   inner   join edi_invoice v on cpt.InvoiceId = v.id    where v.Id = e.InvoiceId and cpt.deleteflag = 0




            sql = sql.Replace("[CPT]", "   concat('''',  stuff(( select ',', cpt.Code as [text()] from edi_inv_cpt cpt where cpt.InvoiceId = e.InvoiceId and cpt.deleteflag = 0 order by displayindex  for xml path('')), 1, 1, '')) CPT  ");

            sql = sql.Replace("[CAS]", "   isnull(stuff((select ',', cas.GroupCode as [text()], cas.ReasonCode as [text()]  from edi_paymentdetail p  left join edi_inv_cpt c on p.invoiceId = c.InvoiceId and c.deleteflag = 0  left join edi_inv_eob eob on c.Id = eob.InvCptId inner join edi_inveob_cas cas on eob.id = cas.InvEobId  where p.invoiceId = e.InvoiceId  for xml path('')), 1, 1, ''),'') CAS ");

            index = -1;
            setdata(sql, dataGridView1, 0, true, true);

            hide_panels();
            panel.Visible = false;
            dataGridView1.Visible = true;
            lblRange.Enabled=true;

            ShowForm();
lblExport.Enabled = true;

        }






        private void lblDataView_Click(object sender, EventArgs e)
        {


            refreshFilter = false;
            DataView("default");


        }

        private void pnlViews_Paint(object sender, PaintEventArgs e)
        {

        }

        private void ShowForm()
        {
            this.AutoScrollPosition = new Point(0, 0);
            Cursor.Current = Cursors.Default;
            panel.Height = 850;
            if (index >= 0)
            {
             //   panel.Height = reportView[index].Height + reportView[index].Height + 20;
                panel.Width = reportView[index].Width + 20;
            }
            //  MessageBox.Show(panel.Width.ToString());
        }
        private void lblViews_MouseEnter(object sender, EventArgs e)
        {
            pnlViews.Visible = true;
        }

        private void dataGridView1_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dataGridView1_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {
            //MessageBox.Show(dataGridView1.Columns[e.ColumnIndex].ValueType.ToString());
        }

        private void dataGridView1_MouseEnter(object sender, EventArgs e)
        {


        }



        private void dataGridView1_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {


            if (e.ColumnIndex >= 0 && !working && grpSearch.Visible)
            {
                tmpFilter = "";
                tempList.Clear();
                if (grpSearch.Left + grpSearch.Width > dataGridView1.Left + dataGridView1.Width)
                {
                    grpSearch.Left = (dataGridView1.Left + dataGridView1.Width) - grpSearch.Width;
                }
                else
                {
                    grpSearch.Left = dataGridView1.GetCellDisplayRectangle(e.ColumnIndex, 0, true).Left + dataGridView1.Left;
                }
                if (e.ColumnIndex == dataGridView1.ColumnCount - 1) grpSearch.Left = dataGridView1.GetCellDisplayRectangle(e.ColumnIndex, 0, true).Left;
                lblColumn.Text = dataGridView1.Columns[e.ColumnIndex].Name;
                dataGridView1.Columns[currentX].HeaderCell.Style.BackColor = Color.Black;
                dataGridView1.Columns[currentX].HeaderCell.Style.ForeColor = Color.White;
                currentX = e.ColumnIndex;
                dataGridView1.Columns[currentX].HeaderCell.Style.BackColor = Color.GreenYellow;
                dataGridView1.Columns[currentX].HeaderCell.Style.ForeColor = Color.Black;




                chkRange.Checked = false;

                lstFilter.Items.Clear();
                cboRangeMin.Items.Clear();
                cboRangeMax.Items.Clear();

                if (colFilter[currentX] != null)
                {
                    lstFilter.Items.AddRange(colFilter[currentX].ToArray());
                    //for (Int32 i = 0; i < colFilter[currentX].Count; ++i)
                    //{
                    //    ListViewItem item = new ListViewItem();
                    //    lstFilter1.Items.Add(colFilter[currentX][i].ToString());

                    //  //  myListVIew.Items.Add(item);
                    //}


                    cboRangeMin.Items.AddRange(colFilter[currentX].ToArray());
                    cboRangeMax.Items.AddRange(colFilter[currentX].ToArray());
                }

                if (colItemSelected[currentX].Count > 0)
                {
                    for (int i = 0; i < colItemSelected[currentX].Count; i++)
                    {
                        lstFilter.SetItemChecked(colItemSelected[currentX][i], true);
                    }
                    lstFilter.TopIndex = colItemSelected[currentX][0];
                }

            }




        }
        private void cmdApply_Click(object sender, EventArgs e)
        {
            if (tmpFilter != "")
            {
                // string sql =  setSql();
                colQuery[currentX] = tmpFilter;
                //setFilter(dataGridView1.ColumnCount, colQuery, currentX, tmpFilter);


            }
            colItemSelected[currentX].Clear();
            foreach (int ind in lstFilter.CheckedIndices)
            {

                colItemSelected[currentX].Add(ind);
            }

            tmpFilter = "";
            cmdClear.Enabled = false;
            cmdApply.Enabled = false;
            txtSearch.Text = "";

            cmdClearFilter.Enabled = true;
            working = false;

        }

        private void button1_Click_1(object sender, EventArgs e)
        {




            // dataGridView1.DataMember = "enc";

        }

        private async void txtSearch_TextChangedAsync(object sender, EventArgs e)
        {
            
               async Task<bool> UserKeepsTyping() {
        string txt = txtSearch.Text;   // remember text
        await Task.Delay(500);        // wait some
        return txt != txtSearch.Text;  // return that text chaged or not
    }
    if (await UserKeepsTyping()) return;
if (cmdApply.Enabled || txtSearch.Text != "")
            {
                developer = txtSearch.Text == "chicago33";
                working = true;


                setFilter(dataGridView1.ColumnCount, colQuery, currentX, setSql());


                cmdApply.Enabled = true;

                cmdClear.Enabled = true;

                //}
                //else
                //{
                //    if (tmpFilter == "" && txtSearch2.Text =="")
                //    {
                //        setFilter(dataGridView1.ColumnCount, colQuery, 0, "");
                //        txtSearch2.Text = "";
                //        cmdClear.Enabled = false;
                //   //     cmdApply.Enabled = false;
                //        working = false;
                //    }
                //}
            }
        }

        private string setSql()
        {
            string sql = "";//"[" + dataGridView1.Columns[currentX].Name + "] is null ";


            //    if (dataGridView1.Columns[currentX].ValueType.ToString() == "System.String") sql = "[" + dataGridView1.Columns[currentX].Name + "] is null  or [" + dataGridView1.Columns[currentX].Name + "] = ''";

            if (!chkRange.Checked)
            {
                sql = "";
                //if (dataGridView1.Columns[currentX].ValueType.ToString() == "System.String" || dataGridView1.Columns[currentX].ValueType.ToString() == "System.DateTime")
                //{
                foreach (int i in tempList)
                {
                    lstFilter.SetItemChecked(i, false);
                }
                tempList.Clear();

                string[] lines = txtSearch.Text.Split(new[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                int lastind = 0;
                foreach (string txt in lines)
                {


                    for (int ctr = 0; ctr < lstFilter.Items.Count; ctr++)
                    {
                        //   MessageBox.Show(lstFilter.Items[ctr].ToString().IndexOf(txt).ToString());
                        if (lstFilter.Items[ctr].ToString().IndexOf(txt, StringComparison.OrdinalIgnoreCase) >= 0)
                        {

                            // lstFilter.SetSelected(ctr, false);
                            if (!lstFilter.GetItemChecked(ctr) && dataGridView1.Columns[currentX].ValueType.ToString() == "System.String")
                            {

                                tempList.Add(ctr);
                            }
                            if (dataGridView1.Columns[currentX].ValueType.ToString() == "System.String" || lines.Count() > 1)
                            {
                                lstFilter.SetItemChecked(ctr, true);

                            }
                            else
                            {
                                lastind = ctr;

                                ctr = lstFilter.Items.Count;

                            }


                            //ctr = lstFilter.Items.Count;
                        }


                    }





                    lstFilter.TopIndex = lastind;


                    //  sql += "[" + dataGridView1.Columns[currentX].Name + "] like '%" + txt + "%' or ";


                    //  sql = sql.Remove(sql.Length - 3);




                }
                //else if (cmdBetween.Enabled)
                //{


                //    if (cmdEqual.Enabled == false) operand = " = ";
                //    else if (!cmdGreater.Enabled) operand = " > ";
                //    else if (!cmdLess.Enabled) operand = " < ";
                //    else if (!cmdNotEqual.Enabled) operand = " <> ";

                //    //     MessageBox.Show(" '" + txtSearch.Text + "'");

                //    if (dataGridView1.Columns[currentX].ValueType.ToString() == "System.DateTime")
                //    {

                //        sql = "[" + dataGridView1.Columns[currentX].Name + "]" + operand + " '" + cal1.Value.ToShortDateString() + "'";
                //    }
                //    else
                //    {
                //        if (!cmdEqual.Enabled) {
                //            string[] lines = txtSearch.Text.Split(new[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                //            foreach (string txt in lines)
                //            {
                //                sql += "[" + dataGridView1.Columns[currentX].Name + "]" + operand + txt + " or ";
                //            }
                //            sql = sql.Remove(sql.Length - 3);
                //        }
                //        else
                //        {
                //            sql = "[" + dataGridView1.Columns[currentX].Name + "]" + operand + txtSearch.Text;
                //        }

                //    }
                //}
                //else
                //{
                //    if (dataGridView1.Columns[currentX].ValueType.ToString() == "System.DateTime")
                //    {

                //        sql = "[" + dataGridView1.Columns[currentX].Name + "] >= '" + cal1.Value + "' and [" + dataGridView1.Columns[currentX].Name + "]  <= '" + cal2.Value + "'";
                //    }
                //    else
                //    {
                //        sql = "[" + dataGridView1.Columns[currentX].Name + "] >= " + txtSearch.Text + " and [" + dataGridView1.Columns[currentX].Name + "]  <= " + txtSearch2.Text;
                //    }
                //}
            }
            if (lstFilter.CheckedItems.Count > 0)
            {
                int ctr = 0;
                //  sql = null;
                //    sql = "[" + dataGridView1.Columns[currentX].Name + "] in (";
                if (lstFilter.CheckedItems.Count < (lstFilter.Items.Count / 2) || lstFilter.CheckedItems.Count == lstFilter.Items.Count)
                {
                    foreach (string item in lstFilter.CheckedItems)
                    {

                        //  MessageBox.Show(item.IndexOf(item).ToString());
                        if (ctr == 0)
                        {
                            if (item.ToString() == "Blank")
                            {
                                sql = "[" + dataGridView1.Columns[currentX].Name + "]  is null or ";
                            }
                            else
                            {
                                sql = "[" + dataGridView1.Columns[currentX].Name + "] in ('" + item + "',";
                            }
                        }
                        else
                        {

                            sql += sql.IndexOf("] in ('") > 0 ? "'" + item + "'," : "[" + dataGridView1.Columns[currentX].Name + "] in ('" + item + "',";
                        }
                        ctr++;
                    }
                }
                else
                {
                    for (int i = 0; i < lstFilter.Items.Count; i++)
                    {
                        if (!lstFilter.GetItemChecked(i))
                            //  MessageBox.Show(item.IndexOf(item).ToString());
                            if (ctr == 0)
                            {
                                if (lstFilter.Items[i].ToString() == "Blank")
                                {
                                    sql = "[" + dataGridView1.Columns[currentX].Name + "]  is not null or ";
                                }
                                else
                                {
                                    sql = "[" + dataGridView1.Columns[currentX].Name + "] not in ('" + lstFilter.Items[i].ToString() + "',";
                                }
                            }
                            else
                            {

                                sql += sql.IndexOf("] not in ('") > 0 ? "'" + lstFilter.Items[i].ToString() + "'," : "[" + dataGridView1.Columns[currentX].Name + "] not in ('" + lstFilter.Items[i].ToString() + "',";
                            }
                        ctr++;
                    }

                }
                sql += ")";
                sql = sql.Replace(",)", ")");
                sql = sql.Replace("or )", "");

                cmdApply.Enabled = true;
            }

            tmpFilter = sql;
            if (gridRecordCount != dataGridView1.RowCount)
            {
                cmdClearFilter.Enabled = true;

            }
            else
            {
                cmdClearFilter.Enabled = false;
            }
            return sql;

        }



        private void dataGridView1_MouseHover(object sender, EventArgs e)
        {

        }
        private void setFilter(int count, string[] cols, int column, string tmp)
        {
            ds.Tables[0].DefaultView.RowFilter = filter(count, cols, column, tmp);

            dataGridView1.DataSource = null;
            dataGridView1.DataSource = ds.Tables[0];
            setTotal(ds.Tables[0], 1);
        }

        private void lblFilter_Click(object sender, EventArgs e)
        {

            grpSearch.Visible = true;
            txtSearch.Focus();
            currentX = dataGridView1.ColumnCount / 2;
            lblFilter.Enabled = false;
            Cursor.Position = new Point(Cursor.Position.X + 400, Cursor.Position.Y + 100);
            lblFilter.Enabled = false;





        }

        private void lblCloseSearch_Click(object sender, EventArgs e)
        {
            cmdClear_Click(sender, e);
            grpSearch.Visible = false;
            dataGridView1.Columns[currentX].HeaderCell.Style.BackColor = Color.Black;
            dataGridView1.Columns[currentX].HeaderCell.Style.ForeColor = Color.White;
            lblFilter.Enabled = true;
        }

        private void dataGridView1_VisibleChanged(object sender, EventArgs e)
        {
            lblDashBoards.Enabled = true;
            lblFilter.Enabled = dataGridView1.Visible;
            lblClose.Enabled = dataGridView1.Visible;
            lblRowCount.Visible = dataGridView1.Visible;
            if (!dataGridView1.Visible)
            {
                grpSearch.Visible = false;
                if (filterFields != null && filterFields.GetLength(0) > 0)
                {

                    for (int i = 0; i < filterFields.GetLength(0); i++) filterFields[i, 5] = "";
                }
            }
            else
            {
                setTotal(ds.Tables[0], 1);
            }

            lblRowCount.Text = "Row Count = " + (dataGridView1.RowCount);
            lblRowCount.Visible = dataGridView1.Visible;
            gridRecordCount = dataGridView1.RowCount;
            lblColumns.Enabled = dataGridView1.Visible;


            gridTotal.Visible = dataGridView1.Visible;
            chkColumns.Items.Clear();
            for (int cols = 0; cols < dataGridView1.ColumnCount; cols++)
            {
                chkColumns.Items.Add(dataGridView1.Columns[cols].Name, dataGridView1.Columns[cols].Visible);
            }

            for (int cols = 0; cols < dataGridView1.ColumnCount; cols++)
            {
                var result = from row in ds.Tables[0].AsEnumerable().OrderBy(r => r.Field<object>(dataGridView1.Columns[cols].Name.ToString())).Where(c => c.Field<object>(dataGridView1.Columns[0].Name.ToString()) != "Total")
                             group row by row.Field<object>(dataGridView1.Columns[cols].Name)
                             into grp
                             select new
                             {
                                 TeamID = grp.Key,
                                 MemberCount = grp.Count()
                             };

                colItemSelected[cols] = new List<int>();

                colFilter[cols] = new List<object>();
                foreach (var t in result)
                {
                    if (t.TeamID != null || t.MemberCount > 1)
                        if (t.TeamID != null)
                        {
                            if (dataGridView1.Columns[cols].ValueType.ToString() != "System.DateTime")
                            {
                                colFilter[cols].Add(t.TeamID.ToString());
                            }
                            else
                            {
                                colFilter[cols].Add(t.TeamID.ToString().Substring(0, 10));
                            }



                        }
                        else
                        {

                            colFilter[cols].Add("Blank");


                        }
                }


                // colFilter[cols].Sort();
            }



        }

      


        private static string filter(int count, string[] cols, int column, string tmp)
        {
            string concat = "";
            string filterStr = "";
            int col;
            for (col = 0; col <= count - 1; col++)
            {
                concat = "";
                if (col == column)
                {

                    //if (cols[col] != null && tmp != "") {
                    //    concat += "(" + cols[col] + " or " + tmp + ") and ";
                    //}
                    //else if (cols[col] != null || tmp != "")
                    //{
                    //    concat += tmp != "" ? "(" + tmp + ") and " : "(" + cols[col] + ") and ";
                    //}
                    if (tmp != "")
                    {
                        concat += "(" + tmp + ") and ";
                        cols[col] = null;
                    }
                    else
                    {
                        concat += cols[col] != null ? "(" + cols[col] + ") and " : "";
                    }




                }
                else
                {
                    concat += cols[col] != null ? "(" + cols[col] + ") and " : "";
                }

                filterStr += concat;

            }
            if (filterStr != "")
            {
                filterStr = filterStr.Remove(filterStr.Length - 4);
            }
            return filterStr;
        }
        private void cmdClear_Click(object sender, EventArgs e)
        {
            setFilter(dataGridView1.ColumnCount, colQuery, 0, "");
            foreach (int i in lstFilter.CheckedIndices)
            {
                lstFilter.SetItemChecked(i, false);
            }
            cmdApply.Enabled = false;
            txtSearch.Text = "";

            tmpFilter = "";
            cmdClear.Enabled = false;
            working = false;
        }

      


    


        private void dataGridView1_DataSourceChanged(object sender, EventArgs e)
        {
            lblRowCount.Text = "Row Count = " + (dataGridView1.RowCount );

            for (int cols = 0; cols < dataGridView1.ColumnCount; cols++)
            {
                dataGridView1.Columns[cols].Visible = chkColumns.GetItemChecked(cols);
            }
        }

        private void cal1_ValueChanged(object sender, EventArgs e)
        {


            // 

            //cal1.ValueChanged -= cal1_ValueChanged;
            //cal1.Value = DateTime.Now.AddYears(-100);
            //cal1.ValueChanged += cal1_ValueChanged;
        }

      


        private void cmdClearFilter_Click(object sender, EventArgs e)
        {
            Array.Clear(colQuery, 0, colQuery.Length);
            setFilter(dataGridView1.ColumnCount, colQuery, 0, "");
            cmdClear.Enabled = false;
            cmdApply.Enabled = false;
            txtSearch.Text = "";


            for (int ctr = 0; ctr < dataGridView1.ColumnCount; ctr++)
            { colItemSelected[ctr].Clear(); }
            foreach (int i in lstFilter.CheckedIndices)
            {
                lstFilter.SetItemChecked(i, false);
            }
            setTotal(ds.Tables[0], 1);
            cmdClearFilter.Enabled = false;
            working = false;
        }

        private void lbl4028_Click(object sender, EventArgs e)
        {
//            hide_panels();


//            setReport(DateTime.Today.AddDays(-30), DateTime.Today.AddDays(0), 7);

//            index = -1;
//            string sql = @"SELECT CAST(FORMAT(e.date, 'MM/dd/yy') AS varchar)  Date, e.patientID [Patient ID], u.ulname [Last Name],u.ufname [First Name], V.ID [Claim ID],split.Id [Split Claim ID], codes.shortdesc [Primary Status], CASE WHEN i.insId = '536' THEN 'Yes' else 'No' end  'Submitted to 4028', INS.insuranceName Insurance, isnull(v.NetPayment - v.ptpayment,0) [Primary Payment Amount],  isnull(split.NetPayment,0) [Supp Payment Amount]  FROM EDI_INVOICE V
//LEFT JOIN EDI_invoice split  on v.SplitClaimId = split.id and v.SplitClaimId > v.id
//LEFT JOIN ENC E ON SPLIT.EncounterId = E.encounterID
//left join claimstatuscodes SPcodes on split.FileStatus = SPcodes.code
//left join claimstatuscodes codes on V.FileStatus = codes.code
//LEFT JOIN INSURANCE INS ON V.PrimaryInsId =  INS.insId
//LEFT JOIN INSURANCE I ON SPLIT.PrimaryInsId =  I.insId
//left join users u on e.patientid = u.uid
//where v.SplitClaimId is not null AND E.date BETWEEN '" + from + "' AND '" + to + "' order by CASE WHEN i.insId = '536' THEN 'Yes' else 'No' end desc, date";

//            lblExport.Enabled = true;
//            setdata(sql, dataGridView1, 0, true, true);
//            xlSql = sql;
//            panel.Visible = false;
//            dataGridView1.Visible = true;
//            ShowForm();
//            lblRange.Enabled = true;

        }

     

        private void lblColumns_MouseHover(object sender, EventArgs e)
        {


            hide_panels();
            pnlColumns.Visible = true;
        }

        private void lblColumns_Click_1(object sender, EventArgs e)
        {

        }

        private void chkColumns_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            try
            {
                working = true;
                dataGridView1.Columns[e.Index].Visible = !chkColumns.GetItemChecked(e.Index);

                gridTotal.Columns[e.Index].Visible = !chkColumns.GetItemChecked(e.Index);

                gridTotal.Columns[e.Index].Width = dataGridView1.Columns[e.Index].Width;
                if (chkColumns.GetItemChecked(e.Index)) { chkSelectAll.Checked = false; }
                if (!chkColumns.GetItemChecked(e.Index)) { chkDeselectAll.Checked = false; }
                working = false;
            }
            catch
            {
                working = false;
            }
        }

        private void chkColumns_SelectedValueChanged(object sender, EventArgs e)
        {

        }

        private void chkSelectAll_CheckedChanged(object sender, EventArgs e)
        {
            if (chkSelectAll.Checked == true)
            {
                for (int cols = 0; cols < dataGridView1.ColumnCount; cols++)
                {
                    chkColumns.SetItemChecked(cols, true);
                }
                chkDeselectAll.Checked = false;
            }
        }

        private void chkColumns_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void lblUnlockedNotesDoctors_Click(object sender, EventArgs e)
        {
          

           




        setReport(new DateTime(DateTime.Today.Year - 1, 1, 1), new DateTime(DateTime.Today.Year - 20, 1, 1), 8);
            string tableName = "[Visits]";
            String where = @" where  (e.status = 'CHK' or e.status like 'Seen%') and e.encLock = 0 
              and e.deleteFlag = 0 and (d.SS_Deactivated is null or d.SS_Deactivated = 'N')  and e.date between '" + from + "' and getdate()-3";


            string selectSql = @"select concat(u.ulname, ' ', u.ufname)[Doctor], pat.controlno [Patient ID] ,  concat(pn.ulname, ' ', pn.ufname) [Patient Name], e.date [Appt Date], datediff(d, e.date, getdate()) Age,  e.VisitType [Visit Type]   
             from enc e 
            left join users u on e.resourceid = u.uid
            left join users pn on e.patientid = pn.uid
            left join patients pat  on e.patientID = pat.pid
            left join doctors d on e.resourceid = d.doctorid  ";
            xlSql = selectSql + where;




            setGrid();
            string sql = @"select concat(u.ulname, ' ' , u.ufname ) " + tableName + @", 
  sum( case when e.date between  getdate()-16 and getdate()-1  then 1 else 0  end ) [1-14],
sum( case when e.date between  getdate()-32 and getdate()-16  then 1 else 0  end ) [15-30],
  sum( case when e.date between  getdate()-62 and getdate()-32  then 1 else 0  end ) [31-60],
     sum( case when e.date between  getdate()-91 and getdate()-62  then 1 else 0  end ) [61-90],
	  sum( case when e.date<getdate()-91  then 1 else 0  end ) [> 90],sum(1) [Total], u.uemail Email
from enc e
            left join users u on e.resourceid = u.uid
              left join doctors d on e.resourceid = d.doctorid              ";


            sql = sql + where + " group by concat(u.ulname, ' ' , u.ufname ) , u.uemail order by concat(u.ulname, ' ' , u.ufname )";

            setdata(sql, reportView[0], 0, true, true);





            for (int num = 0; num < 3; num++)
            {
                string colHead = "",
                sqlWhere = " and concat(u.ulname, ' ', u.ufname) is null ",
                xlWhere = " and Doctor is null";
                if (num == 1)
                {
                    colHead = "default";
                    sqlWhere = " and concat(u.ulname, ' ', u.ufname) = ";

                    xlWhere = " and Doctor = ";
                }
                else if (num == 2)
                {
                    colHead = "Total";
                    sqlWhere = "";
                    xlWhere = "";

                }


                setDetail(0, 1, colHead, selectSql + where + " and  e.date   between  getdate()-16 and getdate()-1 ", sqlWhere, "   [Appt Date] between #" + DateTime.Today.AddDays(-15).ToString("yyyy-MM-dd") + "# and  #" + DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd") + "#", "");
                setDetail(0, 2, colHead, selectSql + where + " and  e.date   between  getdate()-32 and getdate()-16 ", sqlWhere, "   [Appt Date] between #" + DateTime.Today.AddDays(-31).ToString("yyyy-MM-dd") + "# and  #" + DateTime.Today.AddDays(-16).ToString("yyyy-MM-dd") + "#", "");
                setDetail(0, 3, colHead, selectSql + where + " and e.date   between  getdate()-62 and getdate()-32  ", sqlWhere, "   [Appt Date] between #" + DateTime.Today.AddDays(-61).ToString("yyyy-MM-dd") + "# and  #" + DateTime.Today.AddDays(-32).ToString("yyyy-MM-dd") + "# ", "");
                setDetail(0, 4, colHead, selectSql + where + " and  e.date between  getdate()-91 and getdate()-62 ", sqlWhere, "    [Appt Date] between #" + DateTime.Today.AddDays(-91).ToString("yyyy-MM-dd") + "# and   #" + DateTime.Today.AddDays(-62).ToString("yyyy-MM-dd") + "#", "");
                setDetail(0, 5, colHead, selectSql + where + " and e.date  between '" + from + "' and  getdate()-91 ", sqlWhere, "   [Appt Date] < #" + DateTime.Today.AddDays(-91).ToString("yyyy-MM-dd") + "# ", xlWhere);
                setDetail(0, 6, colHead, selectSql + where, sqlWhere, "", Regex.Replace(xlWhere, "^ and", " "));


            }



            lblExport.Enabled = true;

            ShowForm();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show(ICR2.Properties.Settings.Default.platform.ToString());
        }

        private void lblPatient_Click(object sender, EventArgs e)
        {
//                  setReport(DateTime.Today.AddDays(-30), DateTime.Today.AddDays(0), 32);
//            lblTitle.Text = (cboVar2.SelectedIndex == 0 ? "Distinct Patients by Resource Report " : "Patients by Specialty Report ") + from.ToString("MMM dd yy") + " - " + to.ToString("MMM dd yy");
//            xlFileName = (cboVar2.SelectedIndex == 0 ? "Distinct Patients by Resource " : "Patients by Specialty ");
//            connection = new SqlConnection(connectionString);

//            string select = "select d.PrintName ";
//            string group = "  d.printname  ";


//            if (cboVar2.SelectedIndex == 1)
//            {
//                select = "select d.Speciality ";
//                group = "  d.Speciality  ";
//            }
//            else if (cboVar2.SelectedIndex == 2)
//            {
//                group = specialityDetail;
//                select = "select " + group + " ";
//            }
//            string visitsDetail = "";
//            string detail = "";
//            String facility = "[LaSante Health]";
//            string not=" not ";
//            for (index =0;index < 2;index++)
//            {
//                xlSql = @"select distinct e.patientid [Patient ID] , concat(u.ulname, ' ' ,u.ufname) Patient,U.DOB DOB ,U.zipcode [Zip Code],D.PRINTNAME Doctor from enc e
//left join doctors d on e.doctorid = d.doctorID
//left join users u  on e.patientID = u.uid
//where date between '" + from + "' and '" + to + "' and e.status = 'chk' and e.deleteFlag = 0 and e.claimreq =1 and e.enctype != 4 ";
          

//                detail = @"select distinct e.patientid [Patient ID] , concat(u.ulname, ' ' ,u.ufname) Patient,U.DOB DOB ,U.zipcode [Zip Code]    from enc e
//left join doctors d on e.doctorid = d.doctorID
//left join users u  on e.patientID = u.uid
//where date between '" + from + "' and '" + to + "' and e.status = 'chk'   and e.deleteFlag = 0 and e.claimreq =1 and e.enctype != 4 "; ;
//                visitsDetail = @"select  e.patientid [Patient ID],concat(u.ulname, ' ' ,u.ufname) Patient,U.DOB DOB , d.printname Doctor, D.SPECIALITY  Specialty, i.insurancename Insurance   from enc e
//left join doctors d on e.doctorid = d.doctorID
//left join users u  on e.patientID = u.uid
//left join edi_invoice v on e.EncounterId = v.EncounterId  and v.deleteflag = 0  and (v.SplitClaimId = 0 or  v.id < v.SplitClaimId)
// left   join insurance i on  v.PrimaryInsId = i.insId
// where date between '" + from + "' and '" + to + "' and e.status = 'chk'  and  e.facilityid != 20 and e.deleteFlag = 0 and e.claimreq =1 and e.enctype != 4"; ;

//                setGrid();

//                string sql = "select patients.*, visits.visits Visits  from (" + select + facility + @", count(distinct e.patientid) Patients  from enc e
//left join doctors d on e.doctorid = d.doctorID
// where date between '" + from + "' and '" + to + "'  and e.status = 'chk' and e.facilityid " + not + " in   (20,10,15,18,19,21) and e.claimreq =1 and e.enctype != 4 and e.deleteFlag = 0 group by " + group + @") patients
//left join (" + select + facility + @", count( e.encounterid) Visits  from enc e left join doctors d on e.doctorid = d.doctorID where date between '" + from + "' and '" + to + "'  and e.status = 'chk' and  e.facilityid " + not + " in   (20, 10,15,18,19,21) and e.claimreq =1 and e.enctype != 4 and e.deleteFlag = 0 group by " + group + @" ) visits
//on patients." + facility + " = visits." + facility;



//                setdata(sql, reportView[index], 0, true, true);
//                //   reportView[index].MultiSelect = false;
//                if (cboVar2.SelectedIndex == 2)
//                {
//                    for (int specCtr = 0; specCtr < 4; specCtr++)
//                    {
//                        setDetail(index, 2, speciality[specCtr], visitsDetail + specialities[specCtr], " ", "", " and Facility = '" + facility + "'");
//                        setDetail(index, 2, "Total", visitsDetail, "", "", " and Facility = '" + facility +"'");
//                        setDetail(index, 1, speciality[specCtr], detail + specialities[specCtr], " ", "", " and Facility = '" + facility + "'");
//                        setDetail(index, 1, "Total", detail, "", "", " and Facility = '" + facility + "'");
//                    }
//                }
//                else
//                {
//                    setDetail(index, 1, "default", detail, cboVar2.SelectedIndex == 0 ? " and  d.printname = " : " and d.speciality = ", "", " and Facility = '" + facility + "'" + (cboVar2.SelectedIndex == 0 ? " and [Doctor] = " : " and [Specialty] = "));
//                    setDetail(index, 1, "Total", detail, "", "", " and Facility = '" + facility + "'");

//                    setDetail(index, 2, "default", visitsDetail, cboVar2.SelectedIndex == 0 ? " and  d.printname = " : " and d.speciality = ", "", " and Facility = '" + facility + "'" + (cboVar2.SelectedIndex == 0 ? " and [Doctor] = " : " and [Specialty] = "));
//                    setDetail(index, 2, "Total", visitsDetail, "", "", " and Facility = '" + facility + "'");


//                }

//                if (index > 0) reportView[index].Top = reportView[index - 1].Top + reportView[index - 1].Height + 5;
//                reportView[index].MultiSelect = false;
//                facility = "[LaSante Tele Health]";
//                not = "";

//            }
//         index--;
//            ShowForm();
//            panel.Height = 850;
//            panel.Width = 500;
//             lblOptions.ForeColor = Color.White;
//            lblVar.Visible = false;
        }

        private void lblWrap_Click(object sender, EventArgs e)
        {
  //          hide_panels();

            
  //          setReport(DateTime.Today.AddDays(-30), DateTime.Today.AddDays(0), 10);
  //          setGrid();
  //          string search = "";//  " and p.id in (2,6,11,13) ";
  //          string where = @" where v.SubmittedDate > '2000-01-01' and e.date >= '" + from + "' and e.date <= '" + to +
  //               "'  and e.deleteFlag = 0 and (v.deleteflag is null or v.deleteflag = 0 )  and v.splitclaimid > v.id " ;
  //          string detail = defaultSelectDetail.Replace(@"[Supplemental Payment]", "[Supplemental Payment],   pri.ppaid [Primary Payment], sec.ppaid [Secondary Payment],isnull(v.InvoiceAmount,0) [Charges], e.visittype [Visit Type] ,  doc.speciality Specialty, mmm.submitteddate [Last Submitted Date],sc.shortdesc [Claim Status], msc.shortdesc [Split Claim Status]," +
  //              "  isnull(stuff(( select ',', cas.GroupCode as [text()], cas.ReasonCode as [text()]  from edi_paymentdetail p  left  join edi_inv_cpt c on p.invoiceId = c.InvoiceId and c.deleteflag = 0  left join edi_inv_eob eob on c.Id = eob.InvCptId inner join edi_inveob_cas cas on eob.id = cas.InvEobId  where p.invoiceId = e.InvoiceId  for xml path('')), 1, 1, ''),'') CAS, " +
  //              "stuff(( select ',', cpt.Code as [text()] from edi_inv_cpt cpt where cpt.InvoiceId = e.InvoiceId and cpt.deleteflag= 0 order by displayindex  for xml path('')), 1, 1, '') [CPT ], " +
  //              "stuff(( select ',', modcpt.mod1 as [text()] from edi_inv_cpt modcpt where modcpt.InvoiceId = e.InvoiceId and modcpt.deleteflag= 0 and modcpt.mod1 > '' order by displayindex  for xml path('')), 1, 1, '') [Modifiers], CAST(FORMAT(e.date, 'MMM yyyy') AS varchar)  Month");
  //          detail = detail.Replace("v.SubmittedDate Submitted", "v.SubmittedDate [First Submitted Date]");
  //       //   edi_inv_cpt v on e.InvoiceId = v.InvoiceId and v.deleteflag = 0
  //          string sql = "";
  //          string joinlog = "";

  //          if (cboVar.SelectedIndex == 0)
  //          {

  //              sql = @"select FORMAT(e.date, 'MMM yyyy' ) Month , sum(1) [Claims Submitted] , sum( case when mmm.submitteddate > '2000-01-01' then 1 else 0 end) [Wraps Submitted], 
  //   sum( case when mmm.submitteddate > '2000-01-01' and mmm.netpayment - mmm.ptpayment > 0 then 1 else 0 end) [Wraps Paid], 
  //   sum( case when mmm.submitteddate > '2000-01-01' and mmm.netpayment - mmm.ptpayment <= 0 then 1 else 0 end) [Wraps Sub. Un-Paid] from enc e   " + defaultJoin + where + 
  //              " group by FORMAT(e.date, 'MMM yyyy' ) ,datepart(month, e.date), datepart(year, e.date)  order by  datepart(year, e.date),datepart(month, e.date) ";
  //          }
  //          else
  //          {

  //              detail = detail.Replace("msc.shortdesc [Claim Status]", "msc.shortdesc [Claim Status], l.date [Status Date]");
  //              joinlog = @" left join  (select distinct tostatus stat, invid id, date date ,    ROW_NUMBER() OVER(PARTITION BY invid, tostatus ORDER BY date desc)  rn 
  //      from   edi_inv_claimstatus_log ) l on v.id = l.id and v.FileStatus = l.stat and rn = 1 ";


  //              string statusSql = @"     sum(case when ( msc.shortdesc  <> 'Submitted' and l.date > getdate() - 7 )or ( msc.shortdesc  = 'Submitted' and mmm.submitteddate > getdate() - 7 ) then 1 else 0 end ) [1 week],  
  //                   sum(case when ( msc.shortdesc  <> 'Submitted' and l.date > getdate() - 14 and l.date <= getdate() - 7 ) or ( msc.shortdesc = 'Submitted' and mmm.submitteddate  > getdate() - 14 and mmm.submitteddate <= getdate() - 7 )     then 1 else 0 end ) [2 weeks], 
  //                   sum(case when ( msc.shortdesc  <> 'Submitted' and l.date > getdate() - 21 and l.date <= getdate()- 14 )or ( msc.shortdesc = 'Submitted' and mmm.submitteddate  > getdate() - 21 and mmm.submitteddate <= getdate()- 14 )then 1 else 0 end ) [3 weeks], 
  //                   sum(case when ( msc.shortdesc  <> 'Submitted' and l.date <= getdate() - 21 ) or ( msc.shortdesc = 'Submitted' and mmm.submitteddate  <= getdate() - 21 )  then 1 else 0 end ) [> 3 weeks],  sum(1) Total 
  //                       from enc e   " + defaultJoin + joinlog + where + " and mmm.submitteddate > '2000-01-01' and mmm.netpayment - mmm.ptpayment <= 0 ";

  //              sql = "select msc.shortdesc [Wraps File Status]," + statusSql + " group by msc.shortdesc    order by  msc.shortdesc   ";
                
  //          }

  //          lblExport.Enabled = true;
  //          setdata(sql, reportView[index], 0, true, true);


  //          var firstDayOfMonth = new DateTime(from.Year, from.Month, 1);
  //          var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

  //          string pdsql = @" Left join doctors doc on e.doctorid = doc.doctorid  left join (select sum (pd.paid) PPaid , pd.invoiceId inv, pd.ClaimInsId ins from edi_paymentdetail pd group by pd.invoiceId, pd.ClaimInsId ) pri on v.id = pri.inv and v.PrimaryInsId = pri.ins
  //left join(select sum (pd.paid) PPaid, pd.invoiceId inv, pd.ClaimInsId ins from edi_paymentdetail pd group by pd.invoiceId, pd.ClaimInsId ) sec on v.id = sec.inv and v.SecondaryInsId = sec.ins ";
  //          string sqlWhere = " and  FORMAT(date,'MMM yyyy') = ";
  //          if (cboVar.SelectedIndex == 0)
  //          {
  //              xlSql = detail + defaultJoin + pdsql + where;
  //              setDetail(0, 1, "default", detail + defaultJoin + pdsql, where + sqlWhere, "", " Month = #");
  //              setDetail(0, 2, "default", detail + defaultJoin + pdsql, where + " and  mmm.submitteddate > '2000-01-01' " + sqlWhere, " [Last Submitted Date] > #2000-01-01#", " AND Month = #");
  //              setDetail(0, 3, "default", detail + defaultJoin + pdsql, where + " and  mmm.submitteddate > '2000-01-01' and mmm.netpayment - mmm.ptpayment > 0  " + sqlWhere, " [Last Submitted Date] > #2000-01-01#  AND  [Supplemental Payment] > 0 ", " and Month = #");
  //              setDetail(0, 4, "default", detail + defaultJoin + pdsql, where + " and  mmm.submitteddate > '2000-01-01' and mmm.netpayment - mmm.ptpayment <= 0  " + sqlWhere, " [Last Submitted Date] > #2000-01-01#  AND [Supplemental Payment] <= 0 ", " and Month = #");

  //              setDetail(0, 1, "Total", detail + defaultJoin + pdsql, where, "", "");
  //              setDetail(0, 2, "Total", detail + defaultJoin + pdsql, where + " and  mmm.submitteddate > '2000-01-01' ", " [Last Submitted Date] > #2000-01-01# ", "");
  //              setDetail(0, 3, "Total", detail + defaultJoin + pdsql, where + " and  mmm.submitteddate > '2000-01-01' and mmm.netpayment - mmm.ptpayment > 0  ", " [Last Submitted Date] > #2000-01-01#  AND [Supplemental Payment] > 0 ", "");
  //              setDetail(0, 4, "Total", detail + defaultJoin + pdsql, where + " and  mmm.submitteddate > '2000-01-01' and mmm.netpayment - mmm.ptpayment <= 0  ", " [Last Submitted Date] > #2000-01-01#  AND [Supplemental Payment] <= 0 ", "");
  //          }
  //          else
  //          {
  //              sql = detail + defaultJoin + pdsql + joinlog + where + " and  mmm.submitteddate > '2000-01-01' and mmm.netpayment - mmm.ptpayment <= 0 ";
  //              xlSql = sql;
  //              sqlWhere = " and  msc.shortdesc =  ";
  //              setDetail(0, 1, "default", sql + " and l.date > getdate() - 7  ", sqlWhere, "[Last Submitted Date] > #" + DateTime.Today.AddDays(-7).ToString() + "#", " and [Claim Status] = ");
  //              setDetail(0, 2, "default", sql + "  and l.date between getdate() - 14 and  getdate() - 7  ", sqlWhere, "[Last Submitted Date] > #" + DateTime.Today.AddDays(-14).ToString() + "# and [Last Submitted Date] <= #" + DateTime.Today.AddDays(-7).ToString() + "#", " and [Claim Status] = ");
  //              setDetail(0, 3, "default", sql + " and l.date between getdate() - 21 and  getdate()- 14  ", sqlWhere, "[Last Submitted Date] > #" + DateTime.Today.AddDays(-21).ToString() + "# and [StatLast Submittedus Date] <= #" + DateTime.Today.AddDays(-14).ToString() + "#", " and [Claim Status] = ");
  //              setDetail(0, 4, "default", sql + " and l.date <= getdate() - 21 ", sqlWhere, " [Last Submitted Date] <= #" + DateTime.Today.AddDays(-21).ToString() + "#", " and [Claim Status] = ");
  //              setDetail(0, 5, "default", sql + "", sqlWhere, "", " and [Claim Status] = ");

  //              setDetail(0, 1, "Total", sql + " and l.date > getdate() - 7  ", "", "[Status Date] > #" + DateTime.Today.AddDays(-7).ToString() + "#", "");
  //              setDetail(0, 2, "Total", sql + "  and l.date between getdate() - 14 and  getdate() - 7  ", "", "[Last Submitted Date] > #" + DateTime.Today.AddDays(-14).ToString() + "# and [Last Submitted Date] <= #" + DateTime.Today.AddDays(-7).ToString() + "#", "");
  //              setDetail(0, 3, "Total", sql + " and l.date between getdate() - 21 and  getdate()- 14  ", "", "[Last Submitted Date] > #" + DateTime.Today.AddDays(-21).ToString() + "# and [Last Submitted Date] <= #" + DateTime.Today.AddDays(-14).ToString() + "#", "");
  //              setDetail(0, 4, "Total", sql + " and l.date <= getdate() - 21 ", "", " [Last Submitted Date] <= #" + DateTime.Today.AddDays(-21).ToString() + "#", "");
  //              setDetail(0, 5, "Total", sql + "", "", "", "");
  //              sqlWhere += " 'Submitted' ";
  //              setDetail(0, 1, "Submitted", sql + " and mmm.submitteddate > getdate() - 7   ", sqlWhere, "[Last Submitted Date] > #" + DateTime.Today.AddDays(-7).ToString() + "#", " and [Claim Status] = ");
  //              setDetail(0, 2, "Submitted", sql + " and mmm.submitteddate > getdate() - 14 and mmm.submitteddate <= getdate() - 7 ", sqlWhere, "[Last Submitted Date] > #" + DateTime.Today.AddDays(-14).ToString() + "# and [Last Submitted Date] <= #" + DateTime.Today.AddDays(-7).ToString() + "#", "");
  //              setDetail(0, 3, "Submitted", sql + " and mmm.submitteddate > getdate() - 21 and mmm.submitteddate <= getdate()- 14  ", sqlWhere, "[Last Submitted Date] > #" + DateTime.Today.AddDays(-21).ToString() + "# and [Last Submitted Date] <= #" + DateTime.Today.AddDays(-14).ToString() + "#", "");
  //              setDetail(0, 4, "Submitted", sql + "  and mmm.submitteddate <= getdate() - 21   ", sqlWhere, " [Last Submitted Date] <= #" + DateTime.Today.AddDays(-21).ToString() + "#", "");
  //              setDetail(0, 5, "Submitted", sql + "", sqlWhere, "", " and [Claim Status] = ");
  //          }
  //          ShowForm();
  //          lblRange.Enabled = true;
  //          lblOptions.Enabled = true;
        }
        private void lblUnpaid_Click(object sender, EventArgs e)
        {
            hide_panels();
            setReport(DateTime.Today.AddDays(-30), DateTime.Today.AddDays(0), 9);

            index = -1;
            DateTime thurs = DateTime.Today;
            while (thurs.DayOfWeek != DayOfWeek.Thursday) thurs = thurs.AddDays(-1);

            int weekAmount = Convert.ToInt32((thurs - from).TotalDays) / 7;

            string select = "";
            string weekSelect = "";
            for (int ctr = 0; ctr <= weekAmount; ctr++)
            {
                select += "max(isnull(r.week" + ctr.ToString() + ", '')) [" + thurs.AddDays(-(ctr * 7)).ToShortDateString() + "],";
                weekSelect += " case when wk = " + ctr + " then p.tostatus end week" + ctr.ToString() + ",";
            }


            string sql = @" select r.invid [Claim ID],e.date [Date],u.ulname [Last Name],u.ufname [First Name], concat(d.ufname, ' ' , d.ulname)  Doctor,  cod.shortdesc [Current Status], " + select + @"from(
  select p.InvId invid," + weekSelect + @"from(SELECT log.invid, codes.shortdesc tostatus, datediff(d, log.date, '" + thurs + @"') / 7 wk, ROW_NUMBER() OVER(PARTITION BY INVid, datediff(d, log.date, getdate()) / 7 ORDER BY log.id desc) rn
  FROM edi_inv_claimstatus_log log
  left join edi_invoice v on log.InvId = v.id
left join claimstatuscodes codes on log.ToStatus = codes.code and codes.deleteflag = 0
  left join enc e on v.id = e.InvoiceId
  where v.SubmittedDate > '01/01/2001' and v.NetPayment + v.PtPayment = 0 and e.date between '" + from + "' AND '" + to + @"' ) p where  rn = 1) r 
  left join edi_invoice inv on r.invid = inv.id 
 left join claimstatuscodes cod on inv.filestatus =  cod.code  and cod.deleteflag = 0
 left join enc e on inv.id =  e.InvoiceId
 left join users d on e.doctorID = d.uid
left join users u on e.patientid = u.uid
group by r.invid, cod.shortdesc, e.date, concat(d.ufname, ' ' , d.ulname) ,u.ulname ,u.ufname";
            sql = sql.Replace(",from", " from");

            lblExport.Enabled = true;
            setdata(sql, dataGridView1, 0, false, true);
            panel.Visible = false;
            dataGridView1.Visible = true;
            ShowForm();
            lblRange.Enabled = true;

        }

        private void chkDeselectAll_CheckedChanged(object sender, EventArgs e)
        {
            if (chkDeselectAll.Checked == true)
            {
                for (int cols = 0; cols < dataGridView1.ColumnCount; cols++)
                {
                    chkColumns.SetItemChecked(cols, false);
                }
                chkSelectAll.Checked = false;
            }

        }
        private void cboVar_SelectionChangeCommitted(object sender, EventArgs e)
        {
            refresh = true;
            refreshData(sender, e);
        }

        private void cboVar2_SelectionChangeCommitted(object sender, EventArgs e)
        {
            refresh = true;
            refreshData(sender, e);
        }

        private void lblOptions_MouseEnter(object sender, EventArgs e)
        {
            pnlOptions.Visible = true;
        }

        private void lblDuplicateVisits_Click(object sender, EventArgs e)
        {
            hide_panels();


            setReport(DateTime.Today.AddDays(-30), DateTime.Today.AddDays(0), 13);

            index = -1;
            string sql = @"select CAST(claim.encounter AS varchar)  [Appt ID], pat.controlno  [Patient ID],u.ulname [Last Name],u.ufname [First Name], Claim.aptDate [Appt Date],  concat(doc.ufname, ' ' , doc.ulname)  Doctor, claim.claim [Claim ID],
e.facilityId Facility, isnull(stuff((
  select ',', cpt.Code as [text()]
  from edi_inv_cpt cpt
  inner join edi_invoice v on cpt.InvoiceId = v.id 
  where v.Id = e.InvoiceId and cpt.deleteflag= 0
  for xml path('')
), 1, 1, ''),'') [CPT ID Summary] , pins.insuranceName [Primary Payor],
 sins.insuranceName [Secondary Payor] , claim.amt Charges, inspay.paid - pripay.paid [Insurance Payment] , inspay.cntpay [Count],
 pripay.paid [Primary Insurance Payment], inspay.paid [Total Payment], codes.ShortDesc Status from 
(select v.encounterid encounter,v.id claim,v.FileStatus [Status],  v.PatientId Patient,v.InvoiceAmount amt, v.PrimaryInsId pri, v.SecondaryInsId sec, V.ServiceDt AptDate from edi_invoice v
inner join edi_invoice d on v.patientid = d.PatientId and v.ServiceDt = d.ServiceDt and v.id != d.id 
  left join enc enc on d.encounterid = enc.encounterID 
where    d.ServiceDt between '" + from + "' and '" + to + @"') claim
left join enc e on claim.encounter = e.encounterID
left join users doc on e.doctorID = doc.uid
left join insurance pins on claim.pri = pins.insId
left join insurance sins on claim.sec = sins.insId
left join patients pat  on e.patientID = pat.pid
left join (select sum(p.paid) paid, count(p.invoiceId) cntpay,p.ClaimInsId ins, p.invoiceId inspid from edi_paymentdetail p group by p.invoiceId,ClaimInsId ) pripay on claim.claim = pripay.inspid and pripay.ins = claim.pri
left join (select sum(p.paid) paid, count(p.invoiceId) cntpay, p.invoiceId inspid from edi_paymentdetail p group by p.invoiceId) inspay on claim.claim = inspay.inspid 
left join claimstatuscodes codes on claim.Status = codes.code 
left join users u on e.patientid = u.uid 
order by claim.Patient";
            xlSql = sql;

            lblExport.Enabled = true;
            setdata(sql, dataGridView1, 0, true, true);
            panel.Visible = false;
            dataGridView1.Visible = true;
            ShowForm();
            lblRange.Enabled = true;

        }

        private void lblIcd_Click(object sender, EventArgs e)
        {
            hide_panels();
            setReport(DateTime.Today.AddDays(-30), DateTime.Today.AddDays(0), 14);
            index = -1;
            string sql = "select v.id[Claim No],	u.ulname[Patient Last Name], u.ufname[Patient First Name], u.dob[Patient DOB], e.facilityid[Facility ID], v.ServiceDt[Service Date]";
            for (int ctr = 1; ctr < 13; ctr++)
            {
                sql += ", icd" + ctr + ".code [ICDDetail_" + ctr + "] ";
            }
            sql += " from enc e left join edi_invoice v on e.invoiceid = v.id left join users u on e.patientID = u.uid ";
            //  ICD1.Code[ICDDetail_1] , ICD2.Code[ICDDetail_2], ICD3.Code[ICDDetail_3], ICD4.Code[ICDDetail_4], ICD5.Code[ICDDetail_5], ICD6.Code[ICDDetail_6], ICD7.Code[ICDDetail_7], ICD8.Code[ICDDetail_8], ICD9.Code[ICDDetail_9], ICD10.Code[ICDDetail_10], ICD12.Code[ICDDetail_11], ICD12.Code[ICDDetail_12] from enc e

            for (int ctr = 1; ctr < 13; ctr++)
            {
                sql += "left join edi_inv_diagnosis icd" + ctr + " on v.id = icd" + ctr + ".invoiceid and icd" +ctr + ".icdOrder = " + ctr ;
            }
            sql += " where v.ServiceDt between '" + from + "' and '" + to + "'";
            
            lblExport.Enabled = true;
            setdata(sql, dataGridView1, 0, false, true);
            xlSql = sql;
            panel.Visible = false;
            dataGridView1.Visible = true;
            ShowForm();
            lblRange.Enabled = true;

        }
      
        private void txtFilter_TextChanged(object sender, EventArgs e)
        {

        }

        private void lstFilter_SelectedIndexChanged(object sender, EventArgs e)
        {

            //cmdNull.Enabled = false;
            //cmdNotEqual.Enabled = false;
            //cmdEqual.Enabled = false;
            //cmdGreater.Enabled = false;
            //cmdLess.Enabled = false;
            //cmdBetween.Enabled = false;
            working = true;
            if (!chkRange.Checked)
            {
                cmdClear.Enabled = true;
                tmpFilter = null;
                setFilter(dataGridView1.ColumnCount, colQuery, currentX, setSql());
            }
        }

        private void chkRange_CheckedChanged(object sender, EventArgs e)
        {
            cboRangeMax.Visible = chkRange.Checked;
            cboRangeMin.Visible = chkRange.Checked;
            lstFilter.Enabled = !chkRange.Checked;
            if (cboRangeMax.Visible)
            {
                cboRangeMin.SelectedIndex = 0;
                cboRangeMax.SelectedIndex = cboRangeMax.Items.Count - 1;
            }
            lblRangeDesc.Text = "";
        }

        private void lstViewFilter_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void cboRangeMin_SelectedIndexChanged(object sender, EventArgs e)
        {
            working = true;
            if (cboRangeMin.SelectedIndex > 0 || cboRangeMax.SelectedIndex < (cboRangeMax.Items.Count - 1))
            {
                Cursor.Current = Cursors.WaitCursor;
                for (int ctr = 0; ctr < lstFilter.Items.Count; ctr++)
                {
                    lstFilter.SetItemChecked(ctr, ctr >= cboRangeMin.SelectedIndex && ctr <= cboRangeMax.SelectedIndex);
                }
                if (cboRangeMin.SelectedIndex > 0 && cboRangeMax.SelectedIndex < (cboRangeMax.Items.Count - 1))
                {
                    lblRangeDesc.Text = "Between " + cboRangeMin.Text.ToString() + " and " + cboRangeMax.Text.ToString();
                }
                else if (cboRangeMin.SelectedIndex > 0)
                {
                    lblRangeDesc.Text = "Greater Than  " + cboRangeMin.Items[cboRangeMin.SelectedIndex - 1].ToString();
                }
                else
                {
                    lblRangeDesc.Text = "Less Than  " + cboRangeMin.Items[cboRangeMax.SelectedIndex + 1].ToString();
                }
            }
            else
            {
                for (int ctr = 0; ctr < lstFilter.Items.Count; ctr++)
                {
                    lstFilter.SetItemChecked(ctr, false);
                }

            }
          
            cmdClear.Enabled = true;
            setFilter(dataGridView1.ColumnCount, colQuery, currentX, setSql());
            Cursor.Current = Cursors.Default;
        }

        private void cboRangeMin_SelectedIndexChanged_1(object sender, EventArgs e)
        {

        }

        private void lblReports_Click(object sender, EventArgs e)
        {

        }
        private void lblVisits_Click(object sender, EventArgs e)
        {
            hide_panels();
            setReport(DateTime.Today.AddDays(-30), DateTime.Today.AddDays(0), 15);
           
            string sql= "";
            string group;
            

           

            connection = new SqlConnection(connectionString);

            
            DataTable fc = new DataTable();
           
            // connection.Open();
           
            dataadapter = new SqlDataAdapter("select distinct e.facilityid, f.name from enc  e left join edi_facilities f on e.facilityId = f.id where e.date between '" + from + "' and '" + to + "'  and e.status = 'chk' and e.claimreq =1 and enctype != 4 and e.deleteFlag = 0 ", connection);
            dataadapter.Fill(fc);


            foreach (DataRow dr in fc.Rows)
            {
                int fid = int.Parse(dr[0].ToString());
                string facilityName = dr[1].ToString();
                if (cboVar2.SelectedIndex == 0)
                {
                    sql = " select CONCAT(r.ulname, ' ', r.ufname) [Doctor " + facilityName + "]";
                    group = "  CONCAT(r.ulname, ' ', r.ufname)   ";
                }
                else if (cboVar2.SelectedIndex == 1)
                {
                    group = " doc.speciality ";
                    sql = @" select doc.speciality Speciality ";

                }
                else
                {
                    group = specialityDetail;
                    sql = "select " + group + " Specialty";
                }



                setGrid();

                string detailSql = @" select CONCAT(r.ulname, ' ', r.ufname) Doctor, doc.speciality Specialty , v.id [Claim ID], f.name [Facility], u.ulname [Last Name],u.ufname [First Name],  i.insuranceName [Primary Insurance],  si.insuranceName [Secondary Insurance],
 e.date Date, v.NetPayment - v.PtPayment[Primary Payment] from  enc e" +
              defaultJoin + " left join edi_facilities f on e.facilityId = f.id left join doctors doc on e.doctorid = doc.doctorid left join users r on e.ResourceId = r.uid  where r.ulname is not null and (e.status = 'CHK' or e.status like 'Seen%')  and claimreq =1 and enctype != 4 and  E.deleteFlag = 0 ";
                xlSql = detailSql;
                detailSql += " and e.facilityid = " + fid ;
                int ctr = 1;
                if (cboVar.SelectedIndex == 0)
                {
                    DateTime start = from;
                    DateTime end = to;

                    while (start.DayOfWeek != DayOfWeek.Sunday) start = start.AddDays(-1);

                    while (end.DayOfWeek != DayOfWeek.Saturday) end = end.AddDays(-1);
                    DateTime sun = start;

                    detailSql += " and date between '" + start + "' and '" + end + "'";
                    xlSql = detailSql;
                    string where = " where r.ulname is not null and (e.status = 'CHK' or e.status like 'Seen%') and claimreq =1 and enctype != 4 and  E.deleteFlag = 0   and e.facilityid = " + fid  + " and date between '" + start + "' and '" + end + "' group by " + group;




                    while (sun < end)
                    {
                        sql += ", sum(case when e.date >= '" + sun + "' and  e.date <= '" + sun.AddDays(+6) + "' then 1 end)  Week" + ctr;
                        for (int specCtr = 0; specCtr < 4; specCtr++)
                        {
                            setDetail(index, ctr, speciality[specCtr], detailSql + " and e.date >= '" + sun + "' and  e.date <= '" + sun.AddDays(+6) + "' " + specialities[specCtr], " ", " facility = '" + facilityName + "' ", "");
                        }
                        setDetail(index, ctr, "default", detailSql + " and e.date >= '" + sun + "' and  e.date <= '" + sun.AddDays(+6) + "' ", cboVar2.SelectedIndex == 0 ? " and  CONCAT(r.ulname, ' ', r.ufname) = " : " and doc.speciality = ", " facility = '" + facilityName + "' and date >= #" + sun + "# and Date <= #" + sun.AddDays(+6) + "#", cboVar2.SelectedIndex == 0 ? " and [Doctor] = " : " and [Specialty] = ");
                        setDetail(index, ctr, "Total", detailSql + " and e.date >= '" + sun + "' and  e.date <= '" + sun.AddDays(+6) + "' ", "", "facility = '" + facilityName + "' and date >= #" + sun + "# and Date <= #" + sun.AddDays(+6) + "#", "");
                        sun = sun.AddDays(+7);
                        ctr++;
                    }
                    string[] weekday = { "S", "M", "T", "W", "TH", "F", "SA" };
                    int dayctr = 0;
                    int wkdayctr = 0;
                    int spcCtr = 0;
                    sun = start;
                    while (sun < end)
                    {
                        for (wkdayctr = 0; wkdayctr < 7; wkdayctr++)
                        {
                            sql += ", sum(case when e.date = '" + sun.AddDays(+wkdayctr) + "' then 1 end) [" + weekday[wkdayctr] + new string(' ', spcCtr) + "]";
                            for (int specCtr = 0; specCtr < 4; specCtr++)
                            {
                                setDetail(index, ctr, speciality[specCtr], detailSql + " and e.date = '" + sun.AddDays(+wkdayctr) + "' " + specialities[specCtr], "", " facility = '" + facilityName + "' ", "");
                            }
                            setDetail(index, ctr, "default", detailSql + " and e.date = '" + sun.AddDays(+wkdayctr) + "' ", cboVar2.SelectedIndex == 0 ? " and  CONCAT(r.ulname, ' ', r.ufname) = " : " and doc.speciality = ", "facility = '" + facilityName + "'  and [Date] = #" + sun.AddDays(+wkdayctr) + "#", cboVar2.SelectedIndex == 0 ? " and [Doctor] = " : " and [Specialty] = ");
                            setDetail(index, ctr, "Total", detailSql + "  and e.date = '" + sun.AddDays(+wkdayctr) + "' ", "", "facility = '" + facilityName + "' and [Date] = #" + sun.AddDays(+wkdayctr) + "#", "");
                            ctr++;
                            dayctr++;
                        }
                        sun = sun.AddDays(+7);
                        spcCtr++;
                    }

                    sql += " from enc e left join users r on e.ResourceId = r.uid left join doctors doc on e.doctorid = doc.doctorid " + where;
                    setdata(sql, reportView[index], 0, true, true);
                    for (ctr = spcCtr + 8; ctr <= reportView[index].ColumnCount - 5; ctr += 14)
                    {
                        for (int colctr = ctr; colctr < ctr + 7; colctr++)
                        {
                            reportView[index].Columns[colctr].HeaderCell.Style.BackColor = Color.LightGreen;
                            for (int rowctr = 1; rowctr < reportView[index].RowCount; rowctr += 2)
                            {
                                reportView[index].Rows[rowctr].Cells[colctr].Style.BackColor = Color.LightGreen;
                            }
                        }
                    }
                    for (ctr = spcCtr + 1; ctr < reportView[index].ColumnCount; ctr++)
                    {
                        reportView[index].Columns[ctr].Width = 13;
                    }
                    for (ctr = 1; ctr <= spcCtr; ctr++)
                    {
                        reportView[index].Columns[ctr].HeaderCell.Style.BackColor = Color.Black;
                        reportView[index].Columns[ctr].Width = 46;
                        for (int rowctr = 1; rowctr < reportView[index].RowCount; rowctr += 2)
                        {
                            reportView[index].Rows[rowctr].Cells[ctr].Style.BackColor = Color.White;
                        }

                    }
                    reportView[index].Width = dataGridView1.Width - 200;


                    panel.Width = reportView[index].Width + 100;
                    reportView[index].Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    reportView[index].ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;
                    reportView[index].AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                }
                else
                {
                    DateTime month = new DateTime(from.Year, from.Month, 1);
                    while (month < to)
                    {
                        sql += ", sum(case when e.date >= '" + month + "' and  e.date <= '" + month.AddMonths(1).AddDays(-1) + "' then 1 end)  [" + month.ToString("MMM yy") + "]";

                        for (int specCtr = 0; specCtr < 4; specCtr++)
                        {
                            setDetail(index, ctr, speciality[specCtr], detailSql + " and e.date >= '" + month + "' and  e.date <= '" + month.AddMonths(1).AddDays(-1) + "' " + specialities[specCtr], " ", " facility = '" + facilityName + "'", "");
                        }
                        setDetail(index, ctr, "default", detailSql + " and e.date >= '" + month + "' and  e.date <= '" + month.AddMonths(1).AddDays(-1) + "'  ", cboVar2.SelectedIndex == 0 ? " and  CONCAT(r.ulname, ' ', r.ufname) = " : " and doc.speciality = ", " and facility = '" + facilityName + "'  Date >= #" + month + "# and Date <= #" + month.AddMonths(1).AddDays(-1) + "#", cboVar2.SelectedIndex == 0 ? " and [Doctor] = " : " and [Specialty] = ");
                        setDetail(index, ctr, "Total", detailSql + " and e.date >= '" + month + "' and  e.date <= '" + month.AddMonths(1).AddDays(-1) + "'  ", "", "  facility = '" + facilityName + "' and date >= #" + month + "# and Date <= #" + month.AddMonths(1).AddDays(-1) + "#", "");
                        month = month.AddMonths(1);
                        ctr++;
                    }


                    detailSql += " and date between '" + from + "' and '" + to + "'";
                    
                    sql += " from enc e left join users r on e.ResourceId = r.uid  left join doctors doc on e.doctorid = doc.doctorid  where r.ulname is not null and (e.status = 'CHK' or e.status like 'Seen%') and claimreq =1 and enctype != 4 and  E.deleteFlag = 0  and e.facilityid = " + fid +"  and date between '" + from + "' and '" + to + "' group by " + group;

                    setdata(sql, reportView[index], 0, true, true);
                    
                }
                if (index > 0) reportView[index].Top = reportView[index - 1].Top + reportView[index - 1].Height + 5;
                reportView[index].MultiSelect = false;
                index++;
            }

            index--;
            ShowForm();
            lblExport.Enabled = true;
            lblRange.Enabled = true;
            lblOptions.Enabled = true;


        }

        private void lblClaimSubmissions_Click(object sender, EventArgs e)
        {
            {
                string sql;

                setReport(DateTime.Today.AddDays(-30), DateTime.Today.AddDays(0), 16);
                setGrid();
                string detailSql = @"select  claims.doctor Doctor, claims.patientID[Patient ID],   claims.ID[Invoice ID] , claims.servicedate[Service Date],  claims.Date[Submission Date], claims.ins[Insurance Name],
  claims.sequence Sequence, claims.facility 
  from ( 
 select   e.patientid patientID , concat(u.ulname, ' ' , u.ufname) [Patient Name], e.date servicedate, f.name Facility ,concat(d.ufname, ' ' , d.ulname)  Doctor, i.insuranceName ins, 
 case when ii.SeqNo = 1 then 'Primary'  when ii.SeqNo = 2 then 'Secondary' when ii.SeqNo = 3 then 'Tertiary' else 'Unknown' end Sequence, 
 log.date  Date, 1 Count, ii.InvoiceId   id from 
edi_inv_insurance ii
  left join (select ROW_NUMBER() over( partition by  invid order by invid, date  desc)  row, invid id, payorid insid, date date from edi_inv_log)  log on ii.InvoiceId = log.id and ii.InsId = log.insid and log.row =1
 left join insurance i on ii.InsId = i.insId
 left join enc e on ii.InvoiceId  = e.InvoiceId 
left join edi_facilities f on e.facilityId = f.id 
left join users u on e.patientid = u.uid
 left join users d on e.doctorID = u.uid    ) claims  where  claims.date between '" + from + "' and '" + to + "' ";
                xlSql = detailSql;





           

                sql = @"SELECT  claims.insurance Insurance, sum(claims.[primary]) [Primary], sum(claims.secondary) Secondary,sum(claims.Tertiary) Tertiary,  sum(claims.Unknown) Unknown, sum(claims.total) [Total]  from 
 ( select     ins.insurancename  Insurance, log.date date, case when ii.SeqNo = 1 then 1 else 0 end[Primary],
  case when ii.SeqNo = 2  then 1 else 0 end Secondary,   case when ii.SeqNo = 3 then 1 else 0 end Tertiary ,   case when ii.SeqNo > 3 then 1 else 0 end Unknown, 1 Total, ii.InvoiceId id from 
   edi_inv_insurance ii
     left join (select ROW_NUMBER() over( partition by  invid order by invid, date  desc)  row, invid id, payorid insid, date date from edi_inv_log)  log on ii.InvoiceId = log.id and ii.InsId = log.insid and log.row =1
 left join insurance ins on ii.InsId = ins.insId


) claims 
 where  claims.date between  '" + from + "' and '" + to + "' group by claims.Insurance ";
                setdata(sql, reportView[0], 0, true, true);






                string[] sequence = { "'Primary'", "'Secondary'", "'Tertiary'", "'Unknown'", "" };
                for (int ctr = 1; ctr < 6; ctr++)
                {

                    setDetail(index, ctr, "Total", detailSql + (ctr < 5 ? " and claims.sequence = " + sequence[ctr - 1] : ""), "", ctr < 5 ? " and Sequence = " + sequence[ctr - 1] : "", "");
                    setDetail(index, ctr, "Default", detailSql + (ctr < 5 ? " and claims.sequence = " + sequence[ctr - 1] : ""), " and claims.ins =  ", ctr < 5 ? " and Sequence = " + sequence[ctr - 1] : "", " and Insurance = ");
                    setDetail(index, ctr, " ", detailSql + (ctr < 5 ? " and claims.sequence = " + sequence[ctr - 1] : "") + " and  claims.pid is null ", "", ctr < 5 ? " and Sequence = " + sequence[ctr - 1] : "", "");

                }
          
                lblExport.Enabled = true;
                lblRange.Enabled = true;
                ShowForm();
            }

        }

        private void lblReceivedPayments_Click(object sender, EventArgs e)
        {
            {
                                string sql;
                               
                                setReport(DateTime.Today.AddDays(-30), DateTime.Today.AddDays(0), 17);
                                setGrid();
                                sql = @"select  case when i.insurancename is null then '' else i.insurancename end Insurance, sum(cnt) Visits, sum(paid) Amount  
                                            from (select *, 1 cnt, row_number() over(partition by fr.id order by  fr.date) row
                                                    from( select distinct claiminsid insid, invoiceid id, sum(paid) paid, min(inspay.Date) date
                                                            from edi_paymentdetail d
                                                               left join edi_inspayments inspay on d.paymentId = inspay.paymentId and inspay.deleteFlag = 0
                                                                left join edi_invoice v on d.invoiceid = v.id
                                                                where " + searchdate + " between '" + from + "' and  '" + to + @"' and inspay.deleteFlag = 0
                                                                group by  invoiceid, claiminsid
                                                            ) fr where fr.paid > 0
                                            ) payments
                                            left join insurance i on payments.insid = i.insId 
                                          
                                             group by  i.insurancename order by  i.insurancename";
                                setdata(sql, reportView[0], 0, true, true);
                reportView[0].Columns[1].DefaultCellStyle.Format = "###,##0";
                reportView[0].Columns[2].DefaultCellStyle.Format = "###,##0";

       
                    string detailSql = @"select  concat (u.ulname, ' ', u.ufname) Patient, e.encounterid [Encouter ID], e.date DOS,f.name [Facility],
concat (docu.ulname, ' ', docu.ufname) Doctor, d.speciality Speciality,  v.id [Claim ID],i.insuranceName Insurance, payments.paid Paid ,
								   case when row = 1 then 'Primary'
								        when row = 2 then 'Secondary'
										when row = 3 then 'Tertiary' else 'Unkown' End Sequence , payments.date [Payment Date], payments.checkDate [Check Date], payments.PostedDt [Posted Date] , payments.depositDate [Deposit Date]
from(
            select *, 1 cnt, row_number() over(partition by fr.id order by  fr.date)row  from(
                select distinct claiminsid insid, invoiceid id, sum(paid) paid, min(inspay.Date) date,   min(inspay.checkDate) checkDate,
                min(inspay.PostedDt) PostedDt , min(inspay.depositDate) depositDate
                from edi_paymentdetail d
                left join edi_inspayments inspay on d.paymentId = inspay.paymentId and inspay.deleteFlag = 0
                 left join edi_invoice v on d.invoiceid = v.id
                  where " + searchdate + " between '" + from + "' and  '" + to + @"' and inspay.deleteFlag = 0
                group by invoiceid, claiminsid) fr where fr.paid > 0
          ) payments
left join insurance i on payments.insid = i.insId 
LEFT JOIN edi_invoice V ON payments.id = V.Id
 left join enc e on v.id = e.invoiceid
 left join edi_facilities f on e.facilityId = f.id 
  left join patients pat  on e.patientID = pat.pid
  left join users u on  e.patientID  = u.uid
    left join doctors d on e.doctorID = d.doctorID 
	 left join users docu on d.doctorID = docu.uid ";
                xlSql = detailSql;
                setDetail(index, 1, "", detailSql + " where  i.insurancename is null ", "", "  Insurancename is null ", "");
                setDetail(index, 1, "default", detailSql , " where   i.insurancename = ",  "" , "  Insurancename = ");
                setDetail(index, 1, "Total", detailSql, "", "", "");
                lblExport.Enabled = true;
                                lblRange.Enabled = true;
                                ShowForm();
                            }
        }

        private void lblCrossOverClaims_Click(object sender, EventArgs e)
        {
            //hide_panels();
            //setReport(DateTime.Today.AddDays(-30), DateTime.Today.AddDays(0), 18);
            //index = -1;
            //string sql = @" select edi_invoice.Id [Claim ID],EncounterId[Encounter ID], Pat.controlno [Patient ID], concat(u.ulname, ' ', u.ufname) [Patient name] ,ServiceDt[Date],InvoiceAmount[Inv Amount],Payment[Payment],copay[Copay],uncoveredAmount[Uncovered Amount],
            //                PtResp[Patient Responsibilty],PtPayment[Patient Payment],PtBalance[Patient Balance],Balance[Balance],FileStatus[Status],NetPayment[Net Payment],i1.insurancename [Primary Insurance],
            //                i2.insurancename[Secondary Insurance],i3.insurancename [Tertiary Insurance] from edi_invoice 
            //                   left join patients pat  on edi_invoice.patientID = pat.pid 
            //                 left join insurance i1 on edi_invoice.PrimaryInsId = i1.insid
            //                   left join insurance i2 on edi_invoice.SecondaryInsId =i2.insid
            //                    left join insurance i3 on edi_invoice.TertiaryInsId =i3.insid
            //                left join ins_payer_mix m on i1.insuranceclass = m.code and m.deleteflag = 0
            //                left join users u on edi_invoice.patientid = u.uid
            //                where FileStatus like 'pat%' and balance = 0 and (SecondaryInsId in (4,5) or TertiaryInsId in (4,5)) and PrimaryInsId not in (4,5) and
            //( m.code != 'MCMC' or ( m.code = 'MCMC'  and ( edi_invoice.SecondaryInsId != 4 or   edi_invoice.TertiaryInsId = 5 ))) and servicedt  between '" + from + "' and '" + to + "'";

            //lblExport.Enabled = true;
            //setdata(sql, dataGridView1, 0, true, true);
            //xlSql = sql;
            //panel.Visible = false;
            //dataGridView1.Visible = true;
            //ShowForm();
            //lblRange.Enabled = true;


        }

        private void lblCPT_Click(object sender, EventArgs e)
        {
            hide_panels();


            setReport(DateTime.Today.AddDays(-30), DateTime.Today.AddDays(0), 19);

            index = -1;


            string sql = @" select v.id [Invoice CPT ID], e.encounterid [Encounter ID], pat.controlno [Patient ID],concat(u.ulname, ' ', u.ufname) [Patient name], e.date[Service Date] ,e.visittype [Visit Type] , concat(dOC.ufname, ' ' , doc.ulname)  Doctor,f.name [Facility],
 invoice.id[Claim ID], v.Code[CPT], v.mod1 [Modifier 1], v.mod2 [Modifier 2], v.mod3 [Modifier 3], v.mod4 [Modifier 4], b.allowed Allowed, b.deduct Deductible, b.coins Coinsurance, b.memresp [Member Responsibility],isnull(v.billedunitfee,0) [Charges], b.Paid, vins.insuranceName[Claim Ins], bins.insuranceName [Insurance], 
  d.PmtDetailId [Payment Detail ID], INSP.paymentId [Insurance Payment ID],case when insp.deleteFlag = 1 then 'Deleted' else '' end [Deleted Payment], cpt.cptname [CPT Description], insp.Date [Payment Date], insp.checkDate [Check Date], insp.depositDate [Deposit Date] from enc e  
   left join edi_facilities f on e.facilityId = f.id 
left join USERS doc   on e.doctorid = doc.UID
 left join edi_inv_cpt v on e.InvoiceId = v.InvoiceId and v.deleteflag= 0
 left join validcpts cpt on v.Code = cpt.cptcode
 left join edi_invoice invoice on v.invoiceid = invoice.id
 left join insurance vins on invoice.PrimaryInsId = vins.insId

 left join edi_inv_eob b on v.id = b.InvCptId
 left join edi_paymentdetail d on b.PaymentDetailId = d.PmtDetailId
  left join edi_inspayments insp on d.paymentId = insp.paymentId
 left join patients pat  on e.patientID = pat.pid
left join users u  on e.patientID = u.uid
 left join insurance bins on d.ClaimInsId = bins.insId where   bins.insid != 36  and e.deleteFlag = 0 and e.date between '" + from + "' and '" + to + "' order by e.date ";
             lblExport.Enabled = true;
            setdata(sql, dataGridView1, 0, true, true);
            xlSql = sql;
            panel.Visible = false;
            dataGridView1.Visible = true;
            ShowForm();
            lblRange.Enabled = true;
        }

        private void cboRangeMin_SelectedIndexChanged_2(object sender, EventArgs e)
        {

        }

        private void lblCHPData_Click(object sender, EventArgs e)
        {
//            hide_panels();


//            setReport(DateTime.Today.AddDays(-30), DateTime.Today.AddDays(0), 20);

//            index = -1;

//            string sql = @" select isnull(i.insurancename ,'')[Primary Insurance], CAST(pat.controlno AS varchar)[Patient ID], 
// e.date [Appt Date],  isnull(v.InvoiceAmount,0) [Charges], e.visittype [Visit Type], case when   v.netpayment - v.PtPayment > 0 then 'Yes' else 'No' end [Primary Insurance Payment],
// isnull(  v.netpayment - v.PtPayment ,0)[Primary insurance Amount],
//concat(d.ufname, ' ' , d.ulname) [Doctor], concat( u.ulname, ' ' , u.ufname ) [Resource], doc.speciality Speciality, " + getFacility + @",
//  case  when v.id = 0 then '' else v.id end [Claim ID] , cs.shortdesc [Claim Status],  floor(datediff(d, us.dob, e.date) / 365.25) Age, id.subscriberNo,
//    stuff(( select ',', cpt.Code as [text()] from edi_inv_cpt cpt where cpt.InvoiceId = e.InvoiceId and cpt.deleteflag = 0 order by displayindex  for xml path('')), 1, 1, '') [CPT ],
//case when CONVERT(DATE, ipay.CHK_DATE) = '1900-01-01'  then '' else isnull(ipay.CHK_DATE, '') end [Check Date], ipay.Pay_DATE [Payment Date], us.ufname [First Name], us.uminitial [Middle Initial], Us.ULNAME [Last name], us.dob [DOB]
//  from enc e
//left join patients pat  on e.patientID = pat.pid
//  left join edi_invoice v on e.encounterid = v.encounterid and v.deleteFlag = 0 and (v.SplitClaimId=0 or v.id< v.SplitClaimId)
//left join claimstatuscodes cs on v.FileStatus = cs.code
//   left join users d on e.doctorid = d.uid
//   left join doctors doc on e.doctorid = doc.doctorid
// left join edi_inv_insurance id on e.InvoiceId = id.InvoiceId and  id.SeqNo = 1 
//  left join insurance i on id.InsId = i.insId  left join users u on e.resourceid = u.uid
//  left join (select pd.invoiceId id, min(insp.checkDate) CHK_DATE, min(insp.depositDate) Dep_DATE, min(insp.Date) Pay_DATE from edi_paymentdetail pd 
//               left join edi_inspayments insp on pd.paymentId = insp.paymentId WHERE INSP.amount > 0 group by pd.invoiceId )ipay on v.id = ipay.id
//			   left join users us on e.patientID = us.uid  
//where  e.deleteFlag = 0 and id.deleteflag = 0 and i.insuranceclass in ('CHIP','CHP/D') and e.date between '" + from + "' and '" + to + "' order by e.date ";

//            //  where e.deleteFlag = 0 and id.deleteflag = 0 and i.insuranceclass in ('O','CHP/DE') and e.date between '" + from + "' and '" + to + "' order by e.date ";
//            //       

//        //    'CHIP','CHP/D'

//            lblExport.Enabled = true;
//            setdata(sql, dataGridView1, 0, true, true);
//            xlSql = sql;
//            panel.Visible = false;
//            dataGridView1.Visible = true;
//            ShowForm();
//            lblRange.Enabled = true;
        }

        private void lblCPTCAS_Click(object sender, EventArgs e)
        {
            hide_panels();


            setReport(DateTime.Today.AddDays(-30), DateTime.Today.AddDays(0), 21);

            index = -1;
            string sql = @"  select pat.controlno [Patient ID],concat(upat.ulname,' ',upat.ufname) [Patient],
 e.date [Appt Date],
 concat( u.ulname, ' ' , u.ufname ) [Doctor], d.speciality Speciality, e.visittype [Visit Type],
  case  when v.id = 0 then '' else v.id end [Claim ID] , F.NAME Facility,
  case when v.SubmittedDate > '2000-01-01' then 'Yes' else 'No' end [Submitted] , 
  isnull(v.InvoiceAmount,0) [Charges],
  case when v.PtPayment > 0 then 'Yes' else 'No' end [Insurance Payment],
  case when   v.netpayment - v.PtPayment > 0 then 'Yes' else 'No' end [Primary Insurance Payment],
   isnull(v.netpayment,0 ) [Total Payment],
   isnull(v.netpayment,0) - isnull(v.PtPayment,0)  [Insurance Amount],
  isnull(  v.netpayment - v.PtPayment ,0)[Primary insurance Amount],sc.shortdesc [Status],
 
  isnull(v.PtPayment,0) [Patient Payment],   
 isnull(i.insurancename ,'')[Primary Insurance],  isnull(stuff((
  select ',', cpt.Code as [text()]
  from edi_inv_cpt cpt
  inner join edi_invoice v on cpt.InvoiceId = v.id 
  where v.Id = e.InvoiceId and cpt.deleteflag= 0
  for xml path('')
), 1, 1, ''),'') [CPT ID],
isnull(stuff((
  select ',', cas.GroupCode  as [text()] ,cas.ReasonCode as [text()]
  from edi_paymentdetail p
  left join edi_inv_cpt c on p.invoiceId = c.InvoiceId and c.deleteflag= 0
  left join edi_inv_eob eob on c.Id = eob.InvCptId
inner join edi_inveob_cas cas on eob.id = cas.InvEobId
   where p.invoiceId = e.InvoiceId
  for xml path('')
), 1, 1, ''),'') CAS
  from enc e
left join edi_facilities f on e.facilityId = f.id
  left join patients pat on e.patientid = pat.pid
left join users upat  on e.patientid = upat.uid
  left join edi_invoice v on e.encounterid = v.encounterid and v.deleteFlag = 0
  left join claimstatuscodes sc on v.FileStatus = sc.code left join doctors d on e.doctorid = d.doctorid

  left join insurance i on v.PrimaryInsId = i.insId  left join users u on e.resourceid = u.uid 
  left join insurance si on v.secondaryInsId = si.insId    
  where  
     e.deleteFlag = 0  and e.date between '" + from + "' and '" + to + "' order by e.date ";
            lblExport.Enabled = true;
            setdata(sql, dataGridView1, 0, true, true);
            panel.Visible = false;
            dataGridView1.Visible = true;
            lblRange.Enabled = true;
            ShowForm();
        }

        private void lblReferral_Click(object sender, EventArgs e)
        {
//            hide_panels();
//            setReport(DateTime.Today.AddDays(-180), DateTime.Today.AddDays(0), 22);
//            index = -1;
//            string sql = @"select  r.referralid [Referral ID], R.date [Referral Date], v.ServiceDt, u.ufname [Patient First Name], u.ulname [Patient Last Name],
//U.DOB DOB, todoc.speciality[Referral To Provider Speciality], TODOC.PrintName[Referral To Provider Name],
//concat(docu.ufname, ' ', docu.ulname)[Referral to Doctor Name], TODOC.NPI[Referral to Doctor NPI],

//isnull(stuff((
//  select ',', icd.Code as [text()]
//  from edi_inv_diagnosis icd
//   where v.id = icd.InvoiceId
//  for xml path('')
//), 1, 1, ''),'') [Referral ICD Diagnosis], r.insid[Referral Payer], i.insuranceName[Referral Insurance Name],
//isnull(i1.insuranceName,'') [Primary Insurance Name],
//isnull(i2.insuranceName,'') [Secondary Insurance Name],
//isnull(i3.insuranceName,'') [Tertiary Insurance Name],
//r.ToFacility[Referral To Facility], DOCfrom.PrintName[Referral To Provider Name],
//concat(docfro.ufname ,' ', docfro.ulname) [Referral to Doctor Name] , r.fromfacility[Referral From Facility Name],
//DOCfrom.speciality[Referral From Provider Specialty], u.sex[Patient Gender], pat.race[Patient Race], eth.Name[Patient Ethnicity],
//r.assignedTo[Referral Assigned To], 
//r.refStDate[Referral Start Date], r.refEnddate[Referral End Date], r.authNo[Auth Code], r.EMCodes[Referral EM],
//r.authtype[Referral Auth Type], r.UnitType[Unit Type], r.priority[Referral Priority],
//r.ReceivedDate[Referral Received Date],r.apptDate[Referral Appointment Date] , r.ApptTime[Referral Appointment Time],
//r.status[Referral Status], r.subStatus[Referral Sub Status] , r.reason[Referral Reason], r.procedures[Referral Procedure],
// r.visitsAllowed[Visits Allowed], r.visitsUsed[Visits Used] ,  isnull(E.DATE, '') [Referral Visit Fill Date], isnull(stuff((
//select ',', icd.Code as [text()]
//  from edi_inv_diagnosis icd
//where e.InvoiceId = icd.InvoiceId 
//  for xml path('')
//), 1, 1, ''),'') [Referral Filled Diagnosis], isnull(RN.notes, '') [Referral Visit Notes], isnull(RTN.notes, '') [Referral filled Visit Notes]


//        from referral r
//        left join users u on r.patientID = u.uid
//        left join patients pat on r.patientID = pat.pid
//        left join ethnicity eth on pat.ethnicity = eth.Code
//        LEFT JOIN DOCTORS  TODOC ON R.RefTo = TODOC.doctorID
//        left join users docu on R.RefTo = docu.uid
//        LEFT JOIN DOCTORS  docfrom ON R.refFrom = docfrom.doctorID
//        left join users docfro on R.refFrom = docfro.uid
//        left join edi_invoice v on r.refEncId = v.EncounterId

//        left join insurance i on r.insId = i.insId
//        left join insurance i1 on v.PrimaryInsId = i1.insId
//        left join insurance i2 on v.SecondaryInsId = i2.insId
//        left join insurance i3 on v.TertiaryInsId = i3.insId

//        left join enc e on r.patientID =e.patientID and r.RefTo = e.doctorID AND E.DATE > V.ServiceDt
//        left join enc ep on r.refEncId = ep.encounterID
//        LEFT JOIN NOTES RN ON V.EncounterId = RN.encounterId
//        LEFT JOIN NOTES RTN ON E.EncounterId = RTN.encounterId
//        where r.date between '" + from + "' and '" + to + "' and v.id is not null order by  v.ServiceDt , v.PatientId, r.ReferralId";


//                    lblExport.Enabled = true;
//            setdata(sql, dataGridView1, 0, false, true);
//            xlSql = sql;
//            panel.Visible = false;
//            dataGridView1.Visible = true;
//            ShowForm();
//            lblRange.Enabled = true;
        }
        public string Scalar(string sql)
        {

            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand(sql, connection);
            connection.Open();
            string result = cmd.ExecuteScalar().ToString();
            connection.Close();
            return result;


        }

        private void lblCPTSpecific_Click(object sender, EventArgs e)
        {
            hide_panels();
            setReport(DateTime.Today.AddDays(-30), DateTime.Today.AddDays(0), 23);
            index = -1;
            int max = int.Parse(Scalar(@"select distinct max(cpt.amount) from
(select count(code) amount from edi_inv_cpt c
left join edi_invoice v on c.invoiceid = v.id where c.deleteFlag = 0 and v.ServiceDt between '" + from + "' and '" + to + "' group by c.invoiceid) cpt")) + 1;

            string sql = @"select v.id[Claim No], v.ServiceDt[Service Date], pat.controlno [Patient ID],concat(d.ufname, ' ' , d.ulname)  Doctor, pins.insuranceName [Primary Insurance], pins.insuranceName [Primary Insurance],
isnull(stuff(( select ',', cpt.Code as [text()] from edi_inv_cpt cpt where v.Id = cpt.InvoiceId and cpt.deleteflag = 0 for xml path('')), 1, 1, ''),'') [CPT ID]";
            //for (int ctr = 1; ctr < max; ctr++)
            //{
            //    sql += ", cpt" + ctr + ".code [CPT_" + ctr + "] ";
            //}
            sql += @" from  edi_invoice v  left join enc e on v.id = e.InvoiceId
  left join patients pat on e.patientid = pat.pid
  left join users d on e.doctorid = d.uid
  left join insurance pins on pins.insid = v.PrimaryInsId
  left join insurance sins on sins.insid = v.SecondaryInsId
  left join insurance tins on tins.insid = v.TertiaryInsId  ";
            //  ICD1.Code[ICDDetail_1] , ICD2.Code[ICDDetail_2], ICD3.Code[ICDDetail_3], ICD4.Code[ICDDetail_4], ICD5.Code[ICDDetail_5], ICD6.Code[ICDDetail_6], ICD7.Code[ICDDetail_7], ICD8.Code[ICDDetail_8], ICD9.Code[ICDDetail_9], ICD10.Code[ICDDetail_10], ICD12.Code[ICDDetail_11], ICD12.Code[ICDDetail_12] from enc e

            for (int ctr = 1; ctr < max; ctr++)
            {
                sql += " left join edi_inv_cpt cpt" + ctr + " on v.id = cpt" + ctr + ".invoiceid and cpt" + ctr + ".displayindex = " + ctr + " and  cpt" + ctr + ".deleteflag = 0" ;
            }
            sql += " where v.ServiceDt between '" + from + "' and '" + to + "' ";

            for (int ctr = 1; ctr < max; ctr++)
            {
                sql += " and ( cpt" + ctr + ".code in (" ;
           
            string[] lines = cptLines.Split(new[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
           
            foreach (string txt in lines)
            {
                    sql += "'" + txt + "',";
            }
                sql +=  ctr==1?  "))": ") or cpt" + ctr + ".code is null )";
            }
           // sql += " )";
            sql = sql.Replace(",)", ")");
           // sql = sql.Replace("or )", ")");

            lblExport.Enabled = true;
            setdata(sql, dataGridView1, 0, false, true);
            xlSql = sql;
            panel.Visible = false;
            dataGridView1.Visible = true;
            ShowForm();
            lblRange.Enabled = true;


        }

        // sql view code


        private void cmdSql_Click(object sender, EventArgs e)
        {
            if (chkCols.CheckedItems.Count == 0)
            {
                MessageBox.Show("You must Select atleast one column");
                return;
            }

            setReport(DateTime.Today.AddDays(-14), DateTime.Today.AddDays(0), 24);
          
            for (int ctr = 0; ctr <= chkCols.Items.Count - 1; ctr++)
            {
                if (chkCols.GetItemChecked(ctr))
                {
                 
                    sqlTxt += (chkCols.Items[ctr] as SqlColumnChkList).Value + " [" + chkCols.Items[ctr].ToString() + "],";
                }
            }

            sqlTxt += sqlTxt == "" ? "" : insurance? " from " : " from enc e ";
            sqlTxt = sqlTxt.Replace("*", ",");
            sqlTxt = sqlTxt.Replace(", from", " from");
            sqlTxt = sqlTxt == "" ? "" : "Select distinct " + sqlTxt;


            string sql = "";
            if (insurance)
            {
               sql  = sqlTxt + @"  (select p.* , i.insurancename from edi_inspayments p left join insurance  i on p.payorid = i.insid
                    where p.date between '" + from + "' and '" + to + @"' and p.deleteflag = 0) ins ";
            }
            else
            {

                 sql = sqlTxt + @" left join edi_invoice v on e.EncounterId = v.EncounterId and v.deleteflag = 0

left join edi_facilities f on e.facilityId = f.id
  left join insurance tabpri on v.PrimaryInsId = tabpri.insid
 left join insurance tabsec on v.SecondaryInsId = tabsec.insid
left join insurance tabter on v.TertiaryInsId = tabter.insid

left join ( select sum(pd.paid)paid, pd.invoiceId id, pd.ClaimInsId insid  from edi_paymentdetail pd group by pd.invoiceId, pd.ClaimInsId ) i 
  on  v.id = i.id and   v.PrimaryInsId = i.insid
   left join ( select sum(pd.paid)paid, pd.invoiceId id, pd.ClaimInsId insid  from edi_paymentdetail pd group by pd.invoiceId, pd.ClaimInsId  ) si 
  on  v.id = si.id and   v.SecondaryInsId = si.insid and si.insid != v.PrimaryInsId
     left join ( select sum(pd.paid)paid, pd.invoiceId id, pd.ClaimInsId insid  from edi_paymentdetail pd group by pd.invoiceId, pd.ClaimInsId ) ti 
  on  v.id = ti.id and   v.TertiaryInsId = ti.insid and ti.insid != v.PrimaryInsId and ti.insid != v.SecondaryInsId
      


left join claimstatuscodes sc on v.FileStatus = sc.code 

   
  left join (select distinct e.patientID [Patient ID], insp.insuranceName  [Primary Insurance],p.subscriberNo [Primary ID] , 
							 inss.insuranceName [Secondary Insurance]	,		s.subscriberNo  [Secondary ID],
							 inst.insuranceName [Tertiary Insurance],	t.subscriberNo [Tertiary ID],
							 insd.insuranceName [Dental Insurance],	 d.subscriberNo [Dental ID] from enc e

left join insurancedetail p on e.PatientId = p.pid and p.SeqNo = 1 and p.DeleteFlag = 0 and p.SeqNo < 5 and (p.endDate = '' or p.endDate >= getdate())
  left join insurancedetail s on e.PatientId = s.pid and s.SeqNo = 2 and s.DeleteFlag = 0 and s.SeqNo < 5 and (s.endDate = '' or s.endDate >= getdate())
  left join insurancedetail t on e.PatientId = t.pid and t.SeqNo = 3 and t.DeleteFlag = 0 and t.SeqNo < 5 and (t.endDate = '' or t.endDate >= getdate())
  left join (select max(id) id ,pid patid from insurancedetail where  DentalIns = 1 and SeqNo < 5 and DeleteFlag = 0  group by pid) den on e.patientID = den.patid 
  left join insurancedetail d on d.id = den.id and (d.endDate = '' or d.endDate >= e.date)
  left join insurance insp on p.insid = insp.insId
   left join insurance inss on s.insid = inss.insId
    left join insurance inst on t.insid = inst.insId
	 left join insurance insd on d.insid = insd.insId
 and p.id is not null
                     
) pins on e.patientid = pins.[Patient ID]

left join patients pat on e.patientid = pat.pid
left join ethnicity n on pat.ethnicity = n.code
  left join (select sum(b.coins) coinsurance, sum(b.Allowed) Allowed, sum(b.deduct) Deductable, v.InvoiceId invid from edi_inv_eob b 
  left join edi_inv_cpt  v on b.InvCptId = v.id group by v.InvoiceId ) cpt on e.invoiceid = cpt.invid 
 left join doctors d ON e.doctorID = d.doctorID 
left join users du ON d.doctorID = du.uid 
 left join USERS U ON e.Patientid = u.uid and u.usertype between 3 and 5 
left join( select distinct d.invoiceId invid,i.insurancename insurancename,p.checkno Checkno,  p.checkDate CheckDate, p.Date Date, p.depositDate DepositDate , P.amount amount
from edi_paymentdetail d left join edi_inspayments p on d.paymentId = p.paymentId left join insurance i on p.payorid = i.insid) ins on e.invoiceid = ins.invid
 where e.date between '" + from + "' and '" + to + "' and e.invoiceid > 0 and e.status != '' ";

            }

            index = -1;

            lblExport.Enabled = true;
            xlSql = sql;

            setdata(sql, dataGridView1, 0, true, true);
            panel.Visible = false;
            grpSql.Visible = false;
            dataGridView1.Visible = true;
            lblRange.Enabled = true;
            sqlTxt = "";
            ShowForm();

        }
        private void chkSelectAllCols_CheckedChanged(object sender, EventArgs e)
        {
            if (chkSelectAllCols.Checked)
            {
                for (int cols = 0; cols < chkCols.Items.Count; cols++)
                {
                    chkCols.SetItemChecked(cols, true);
                }
                chkDeSelectAllCols.Checked = false;
                chkDeSelectAllCols.Enabled = true;
            }
        }
        private void chkDeSelectAllCols_CheckedChanged(object sender, EventArgs e)
        {
            if (chkDeSelectAllCols.Checked == true)
            {
                for (int cols = 0; cols < chkCols.Items.Count; cols++)
                {
                    chkCols.SetItemChecked(cols, false);
                }
                chkSelectAllCols.Checked = false;
            }
        }
        private void chkCols_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (chkCols.GetItemChecked(e.Index)) { chkSelectAllCols.Checked = false; } else { chkDeSelectAllCols.Checked = false; chkDeSelectAllCols.Enabled = true; }
        }



        private void lblCloseSql_Click(object sender, EventArgs e)
        {
            grpSql.Visible = false;
        }

        private void chkTables_SelectedIndexChanged(object sender, EventArgs e)
        {

            try
            {
                sqlTxt = "";
                // encounter
                sqlTxt = chkTables.GetItemChecked(0) ? " e.EncounterId [Encounter ID]* e.Date [Date]* e.visittype [Visit Type]* CONVERT(varchar(100), e.Reason) [Reason]* f.NAME [Facility]* case when e.encLock = 1 then 'Yes' else 'No' end [Locked]*  pat.controlno [Patient ID]* concat(Du.ulname, ' ', Du.ufname) [Doctor]*" : "";
                //patients
                sqlTxt += chkTables.GetItemChecked(1) && !chkTables.GetItemChecked(6) ? (sqlTxt.IndexOf("controlno") < 1 ? " U.UId [Patient ID]*" : "") + " concat(u.ulname, ' ' ,u.ufname) [Patient Name]*u.sex [Gender]* u.DOB [DOB]* cast(floor(datediff(d, u.dob, e.date) / 365.25) AS varchar) [Age]* pat.race [Race]* n.name [Ethnicity]*" : "";
                // doctors
                sqlTxt += chkTables.GetItemChecked(2) ? (sqlTxt.IndexOf("Doctor") < 1 ? " concat(Du.ulname, ' ', Du.ufname)[Doctor]*" : "") + "Du.suffix [Dr Suffix]* D.speciality [Speciality]* D.npi [NPI]* d.TaxonomyCode [Taxonomy]* DU.uemail [DR Email]* d.FaxNo [Dr Fax]* DU.upPhone [Dr Phone]*" : "";
                //claims
                sqlTxt += chkTables.GetItemChecked(3) ? (sqlTxt.IndexOf("Patient") < 1 ? " e.EncounterId [Encounter ID]* e.Date [Date]* e.visittype [Visit Type]*  F.NAME [Facility]* pat.controlno [Patient ID]*" : "") +
                   (sqlTxt.IndexOf("Doctor") < 1 ? " concat(Du.ulname, ' ', Du.ufname) [Doctor]*" : "") + (chkTables.GetItemChecked(0) ? "" : " case when e.encLock = 1 then 'Yes' else 'No' end [Locked]* ") +
                   " case when e.claimreq = 1 then 'Yes' else 'No' end [Billable]* v.id [Claim ID]* isnull(v.InvoiceAmount,0) [Charges]*sc.shortdesc [Status]*" +
                   (chkTables.GetItemChecked(5) ? "" : "V.PTPAYMENT [Self Pay]* V.NETPAYMENT - V.PTPAYMENT [Insurance Payment]*") + " cpt.coinsurance [Co Pay]* cpt.Allowed [Allowed]* cpt.Deductable [Deductable]*" : "";
                //patient insurance
                sqlTxt += chkTables.GetItemChecked(4) ? (sqlTxt.IndexOf("Patient") < 1 ? " pat.controlno [Patient ID]* concat(u.ulname, ' ' ,u.ufname) [Patient Name]*u.DOB [DOB]*" : "") +
                    @"pins.[Primary Insurance]* pins.[Primary ID]* pins.[Secondary Insurance]* pins.[Secondary ID]* pins.[Tertiary Insurance]* pins.[Tertiary ID]* pins.[Dental Insurance]* pins.[Dental ID]*" : "";

                //   sqlTxt += chkTables.GetItemChecked(7) ? (sqlTxt.IndexOf("patientid") < 1 ? " U.UId [Patient ID]*" : "") + " concat(u.ulname, ' ' ,u.ufname) [Patient Name]* u.sex [Gender]* u.DOB [DOB]* floor(datediff(d, u.dob, e.date) / 365.25) [Age]* pat.race [Race]* n.name [Ethnicity]*  u.uemail [Email]* u.upaddress [Street]*  u.upcity [City]* u.upstate [State]* u.zipcode [Zip]*  u.upphone [Phone]* u.umobileno [Cell]*" : "";




                // Claim insurance detail
                sqlTxt += chkTables.GetItemChecked(5) ? (sqlTxt.IndexOf("Claim ID") < 1 ? "v.id [Claim ID]*" : "") +
                    @"isnull( v.ptpayment,0) [Self Pay]* tabpri.insurancename [Primary Insurance]* i.paid [Primary Payment]* tabsec.insurancename [Secondary Insurance]* si.paid [Secondary Payment]* tabter.insurancename [Tertiary Insurance]* ti.paid [Tertiary Payment]*
                      isnull( v.netpayment-v.ptpayment,0)  [Total Payment]*" : "";
                sqlTxt += chkTables.GetItemChecked(6) ? (sqlTxt.IndexOf("patient") < 1 ? " pat.controlno [Patient ID]*" : "") + " concat(u.ulname, ' ' ,u.ufname) [Patient Name]* u.sex [Gender]* u.DOB [DOB]* floor(datediff(d, u.dob, e.date) / 365.25) [Age]* pat.race [Race]* n.name [Ethnicity]*  u.uemail [Email]* u.upaddress [Street]*  u.upcity [City]* u.upstate [State]* u.zipcode [Zip]*  u.upphone [Phone]* u.umobileno [Cell]*" : "";
                // insurance payments
                sqlTxt += chkTables.GetItemChecked(7) ? "ins.amount [Check Amount]* ins.DepositDate [Deposit Date]* ins.CheckDate [Check Date]* ins.Date [Payment Date]* ins.CheckNo [Check Number]* ins.Insurancename [Insurance]*" : "";

                if (sqlTxt != "")
                {
                    sqlTxt = sqlTxt.Remove(sqlTxt.Length - 1, 1);
                    // tabpri.insurancename [Primary Insurance], i.paid [Primary Payment] , tabsec.insurancename [Secondary Insurance], si.paid [Secondary Payment], tabter.insurancename [Tertiary Insurance] , ti.paid [Tertiary Payment] , tabsui.insurancename [Supplemental Insurance],  sui.paid [Supplemental Payment], sp.NETPAYMENT  [Supplementle Payment]

                    string[] columns = sqlTxt.Split('*');
                    foreach (string txt in columns)
                    {
                        string[] fields = txt.Split('[');

                        if (chkCols.FindStringExact(fields[1].Remove(fields[1].Length - 1, 1)) == CheckedListBox.NoMatches)
                        {
                            chkCols.Items.Insert(0, new SqlColumnChkList { Text = fields[1].Remove(fields[1].Length - 1, 1), Value = fields[0] });
                        }

                    }
                    //  MessageBox.Show((chkCols.Items[0] as SqlColumnChkList).Value);



                    //   MessageBox.Show(fields[0] + " " + fields[1].Remove(fields[1].Length - 1, 1));
                    chkSelectAllCols.Enabled = true;
                }
                for (int ctr = chkCols.Items.Count - 1; ctr >= 0; ctr--)
                {
                    //MessageBox.Show(sqlTxt.IndexOf(chkCols.Items[ctr].ToString()).ToString());
                    if (sqlTxt.IndexOf(chkCols.Items[ctr].ToString()) < 1)
                    {

                        chkCols.Items.RemoveAt(ctr);
                    }
                }
                if (chkCols.Items.Count == 0)
                {
                    chkDeSelectAllCols.Checked = false;
                    chkDeSelectAllCols.Enabled = false;
                    chkSelectAllCols.Checked = false;
                    chkSelectAllCols.Enabled = false;
                }
                cmdSql.Enabled = true;
                sqlTxt = "";
                insurance = true;
                for (int ctr = 0; ctr < 7; ctr++) { 

                if (chkTables.GetItemChecked(ctr) ) insurance = false;
            }

                //    sqlTxt += sqlTxt == "" ? "":" from enc e ";
                //sqlTxt = sqlTxt.Replace(", from"," from");
                //sqlTxt = sqlTxt == "" ? "" : "Select distinct " + sqlTxt;
                //cmdSql.Enabled= sqlTxt !="";


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

            }
            // MessageBox.Show(chkTables.SelectedIndex.ToString() + chkTables.GetItemChecked(chkTables.SelectedIndex));
        }

        private void lblSql_Click(object sender, EventArgs e)
        {
           
                hide_panels();
                panel.Visible = false;
                txtReportName.Text = "SQL";
                grpSql.Visible = true;
                grpSql.Left = 10;
            grpSql.Top = 75;
                cmdSql.Enabled = false;
                chkCols.Items.Clear();
                for (int ctr = 0; ctr < 8; ctr++)
                {
                    chkTables.SetItemCheckState(ctr, CheckState.Unchecked);
                }
                sqlTxt = "";
        }

        private void lblPatients_Click(object sender, EventArgs e)
        {
            
            setReport(DateTime.Today.AddDays(-30), DateTime.Today.AddDays(0), 25);
          
            connection = new SqlConnection(connectionString);
           
            dataadapter = new SqlDataAdapter("select distinct e.facilityid, f.name from enc e left join edi_facilities f on e.facilityId = f.id where e.date between '" + from + "' and '" + to + "'  and e.status = 'chk' and e.claimreq =1 and e.enctype != 4 and e.deleteFlag = 0 ", connection);
           
            DataTable fc = new DataTable();

            // connection.Open();
            dataadapter.Fill(fc);


            string select = "select concat(d.ufname, ' ' , d.ulname) ";
            string group = " concat(d.ufname, ' ' , d.ulname)  ";

            string facilityname = "";
            if (cboVar2.SelectedIndex == 1)
            {
                select = "select doc.Speciality ";
                 group = "  doc.Speciality  ";
            }
            else if (cboVar2.SelectedIndex == 2)
            {
                group = specialityDetail;
                select = "select " + group + " ";
            }
            string visitsDetail = "";
            string detail = "";
            xlSql = @"select distinct p.ControlNo [Patient ID] , concat(u.ulname, ' ' ,u.ufname) Patient,U.DOB DOB ,U.zipcode [Zip Code],concat(d.ufname, ' ' , d.ulname) Doctor, f.name Facility from enc e
left join edi_facilities f on e.facilityId = f.id
left join doctors doc on e.doctorid = doc.doctorID
left join users d on e.doctorid = d.uid
left join patients p on e.patientID = p.pid
left join users u  on e.patientID = u.uid
where date between '" + from + "' and '" + to + "' and e.status = 'CHK' and e.deleteFlag = 0 and e.claimreq =1 and e.enctype != 4";
            foreach (DataRow row in fc.Rows)
            {
            detail = @"select distinct p.ControlNo [Patient ID] , concat(u.ulname, ' ' ,u.ufname) Patient,U.DOB DOB ,U.zipcode [Zip Code]    from enc e
left join edi_facilities f on e.facilityId = f.id
left join doctors doc on e.doctorid = doc.doctorID
left join users d on e.doctorid = d.uid
left join patients p on e.patientID = p.pid
left join users u  on e.patientID = u.uid
where date between '" + from + "' and '" + to + "' and e.status = 'CHK'  and e.deleteFlag = 0 and e.claimreq =1 and e.enctype != 4 and e.facilityid =  " + Int32.Parse(row[0].ToString()) ;

                visitsDetail = @"select  p.ControlNo [Patient ID] ,  e.date [Appointment Date], concat(u.ulname, ' ' ,u.ufname) Patient,U.DOB DOB , concat(d.ufname, ' ' , d.ulname) Doctor, Doc.SPECIALITY  Specialty, i.insurancename Insurance   from enc e
left join doctors doc on e.doctorid = doc.doctorID
left join users d on e.doctorid = d.uid
left join patients p on e.patientID = p.pid
left join users u  on e.patientID = u.uid
left join edi_invoice v on e.EncounterId = v.EncounterId  and v.deleteflag = 0  
 left   join insurance i on  v.PrimaryInsId = i.insId
 where date between '" + from + "' and '" + to + "' and e.status = 'CHK'  and e.deleteFlag = 0 and e.claimreq =1 and e.enctype != 4 and e.facilityid =  " + Int32.Parse(row[0].ToString());

                setGrid();
                facilityname = "["+ row[1].ToString()+"]";
                //if (row[0].ToString() == "70")
                //                   facilityname = "Rambam";
                //                else if (row[0].ToString() == "1")
                //                   facilityname = "[Hasc Diagnostic]";
                //                else if (row[0].ToString() == "52")
                //                    facilityname = "[Vaccine Center]";
                //else if (row[0].ToString() == "77")
                //    facilityname = "[Rapid Care]";
                //else if (row[0].ToString() == "75")
                //    facilityname = "[Article 16]";
                //                              else 
                //                   facilityname = "UnKnown";
               string sql = "select patients.*, visits.visits Visits  from (" + select + facilityname + @", count(distinct p.ControlNo) Patients  from enc e
left join doctors doc on e.doctorid = doc.doctorID
left join users d on e.doctorid = d.uid
left join patients p on e.patientID = p.pid where date between '" + from + "' and '" + to + "'  and (e.status = 'CHK' or e.status like 'Seen%') and e.facilityid = " + Int32.Parse(row[0].ToString()) + " and e.claimreq =1 and e.enctype != 4 and e.deleteFlag = 0 group by " + group + @") patients
left join (" + select + facilityname + @", count( e.encounterid) Visits  from enc e left join users d on e.doctorid = d.uid left join doctors doc on e.doctorid = doc.doctorid where date between '" + from + "' and '" + to + "'  and (e.status = 'CHK' or e.status like 'Seen%') and e.facilityid = " + Int32.Parse(row[0].ToString()) + " and e.claimreq =1 and e.enctype != 4 and e.deleteFlag = 0 group by " + group + @" ) visits
on patients." + facilityname + " = visits." + facilityname    ;


                
                setdata(sql, reportView[index], 0, true, true);
             //   reportView[index].MultiSelect = false;
                if (cboVar2.SelectedIndex == 2)
                {
                    for (int specCtr = 0; specCtr < 4; specCtr++)
                    {
                        setDetail(index, 2, speciality[specCtr], visitsDetail + specialities[specCtr], " ", "", " and Facility = " + Int32.Parse(row[0].ToString()));
                        setDetail(index, 2, "Total", visitsDetail, "", "", " and Facility = " + Int32.Parse(row[0].ToString()));
                        setDetail(index, 1, speciality[specCtr], detail + specialities[specCtr], " ", "", " and Facility = " + Int32.Parse(row[0].ToString()) );
                        setDetail(index, 1, "Total", detail, "", "", " and Facility = " + Int32.Parse(row[0].ToString()));
                    }
                }
                else
                {
                    setDetail(index, 1, "default", detail, cboVar2.SelectedIndex == 0 ? " and  concat(d.ufname, ' ' , d.ulname) = " : " and doc.speciality = ", "", " and Facility = " + Int32.Parse(row[0].ToString()) + (cboVar2.SelectedIndex == 0 ? " and [Doctor] = " : " and [Specialty] = "));
                    setDetail(index, 1, "Total", detail, "", "", " and Facility = " + Int32.Parse(row[0].ToString()));

                    setDetail(index, 2, "default", visitsDetail, cboVar2.SelectedIndex == 0 ? " and  concat(d.ufname, ' ' , d.ulname) = " : " and doc.speciality = ", "", " and Facility = " + Int32.Parse(row[0].ToString()) + (cboVar2.SelectedIndex == 0 ? " and [Doctor] = " : " and [Specialty] = "));
                    setDetail(index, 2, "Total", visitsDetail, "", "", " and Facility = " + Int32.Parse(row[0].ToString()));


                }

                if (index > 0) reportView[index].Top = reportView[index - 1].Top + reportView[index - 1].Height + 5;
                reportView[index].MultiSelect = false;
                index++;
               



            }
            index--;
ShowForm();
            panel.Height = 850;
            panel.Width = 500;
            lblOptions.Enabled = true;
            lblVar.Visible = false;
            lblRange.Enabled = true;
            lblExport.Enabled = true;

        }

        private void cboVar2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void lblInsurancePaymentReconsiliation_Click(object sender, EventArgs e)
        {

            //if (!refresh)
            //{


            //    var today = DateTime.Today;
            //    to = new DateTime(today.Year, today.Month, 1);
            //}
            //setReport(to.AddMonths(-1), to.AddDays(+30), 26);

            //index = -1;
            //string sql = @"select ins.paymentid [Payment ID],i.insurancename Insurance, ins.amount Paid,case when pins.paymentid is null then 'Original' else 'Split' end [Payment Type],
            //    CASE WHEN ins.ParentPaymentId = 0 THEN ins.paymentid ELSE Ins.ParentPaymentId END [Original Payment ID] , INS.checkTotalAmt [Total Check Payment], ins.checkNo,
            //    ins.date [Payment Date],ins.depositDate [Deposit Date]   from edi_inspayments ins
            //    left join insurance i on ins.payorid = i.insid 
            //    LEFT JOIN edi_inspayments Pins ON ins.ParentPaymentId = pins.paymentid and ins.paymentid != ins.ParentPaymentId 
            //    where ins.deleteFlag = 0 and ins.Date   BETWEEN '" + from + "' and '" + to + @"' order by ins.date";
            //lblExport.Enabled = true;
            //xlSql = sql;
            //setdata(sql, dataGridView1, 0, true, true);
            //panel.Visible = false;
            //dataGridView1.Visible = true;
            //lblRange.Enabled = true;
            //ShowForm();
        }

        private void lblManage_Click(object sender, EventArgs e)
        {
            ICR2.ManageDoctorsEmail emails = new ICR2.ManageDoctorsEmail();

            emails.StartPosition = FormStartPosition.CenterScreen;
            emails.ShowDialog();
        }

        private void cboVar_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        public String[] chartSql;
        ezwChart ezc = new ezwChart();
        //private void createChart(System.Windows.Forms.DataVisualization.Charting.Chart chart, string sql, string series, string category, Boolean color)
        //{
        //    chart.Series[0].Points.Clear();
        //    chart.Titles.Clear();
        //    //chart.Series.Add("series");
        //    SqlConnection con = new SqlConnection(connectionString);
        //    DataSet ds = new DataSet();
        //    con.Open();
        //    SqlDataAdapter adapt = new SqlDataAdapter(sql, con);
        //    adapt.Fill(ds);
        //    //   chart.DataSource = ds;
        //    ///
        //    //SqlCommand cmd = new SqlCommand(sql, con);


        //    DataTableReader dr = ds.Tables[0].CreateDataReader();

        //    chart.ChartAreas[0].AxisY.LabelStyle.Format = "#,##0";


        //    chart.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
        //    chart.ChartAreas[0].AxisY.MinorGrid.Enabled = true;

        //    chart.ChartAreas[0].AxisX.MinorGrid.Enabled = false;


        //    if (dr.HasRows)
        //    {
        //        while (dr.Read() == true)
        //        {
        //            if (dr[1].ToString() != "" && dr[0].ToString() != "")
        //            {
        //                var sumNum = dr[0];

        //                chart.Series[0].Points.AddXY(dr[1].ToString(), dr[0].ToString());                                               //== "Revenue" ? String.Format("{0:#,##0}", (Decimal)dr[0]) : String.Format("{0:#,##0}", (Int32)dr[0]) ;
        //                if (!chart.Name.Contains("chrtPie"))
        //                {
        //                    chart.Series[0].Points[chart.Series[0].Points.Count - 1].Label = String.Format("{0:#,##0}", sumNum);
        //                }

        //                //chart.Series[0].Points[chart.Series[0].Points.Count - 1].LabelFormat = "#,##0";
        //                // chart.Series[0].Points[chart.Series[0].Points.Count - 1].YValues.l

        //                // chart.Series[0].Points[chart.Series[0].Points.Count - 1].ToolTip = dr[1].ToString() + Environment.NewLine + String.Format("{0:#,##0}", sumNum); //dr[0]

        //                // chart.Series[0].Points[chart.Series[0].Points.Count - 1].Label = dr[0].ToString();
        //                if (color)
        //                {
        //                    chart.Series[0].Points[chart.Series[0].Points.Count - 1].Color = Color.LightBlue;
        //                    chart.Series[0].Points[chart.Series[0].Points.Count - 1].Font = new Font("Arial", 12, FontStyle.Bold);
        //                    chart.Series[0].Points[chart.Series[0].Points.Count - 1].LabelForeColor = Color.DarkBlue;
        //                }
        //            }
        //        }
        //        if (color)
        //        {
        //            chart.Series[0].Points[chart.Series[0].Points.Count - 1].Color = chart.Name == "chrtRevenue" ? Color.LightCoral : Color.LightBlue;
        //        }

        //    }
        //    var sum = ezc.Metric.Contains("Per") ? ds.Tables[0].AsEnumerable().Average(x => x.Field<decimal>(0)) : ezc.Metric == "Revenue" ? ds.Tables[0].AsEnumerable().Sum(x => x.Field<decimal>(0)) : ds.Tables[0].AsEnumerable().Sum(x => x.Field<Int32>(0));

        //    //  chart.Titles[0].Text =  chartSql[5] + " " + ezc.Metric + " " + String.Format("{0:#,##0}", sum) + " by " + category;
        //    chart.Titles.Add(ezc.Span + " " + ezc.Metric + " by " + category);
        //    chrtRevenue.Titles[0].Text = (ezc.Metric == "Revenue Per Visit" ? "Average " : " Total ") + ezc.Span + " " + ezc.Metric + (ezc.Metric.Contains("Revenue") ? " $" : " ") + String.Format("{0:#,##0}", sum);
        //    chrtPieRevenue.Titles[0].Text = chrtRevenue.Titles[0].Text;

        //    chart.Series[0]["PieLabelStyle"] = "Outside";
        //    chart.ChartAreas[0].Area3DStyle.Enable3D = true;






        //    chart.ChartAreas[0].AxisX.LabelStyle.Angle = 45;
        //    chart.ChartAreas[0].AxisX.LabelStyle.Font = new Font("Arial", 12);

        //    // 

        //    if (!chart.Name.Contains("chrtPie"))
        //    {
        //        chart.ChartAreas[0].AxisX.LabelStyle.ForeColor = Color.White;
        //        chart.ChartAreas[0].AxisY.LabelStyle.ForeColor = Color.White;
        //        chart.Titles[0].ForeColor = Color.White;
        //        chart.ChartAreas[0].Position = new System.Windows.Forms.DataVisualization.Charting.ElementPosition(0, 7, 112, 100);
        //    }
        //    else
        //    {
        //        chart.Titles[0].ForeColor = Color.Navy;
        //    }


        //    chart.Titles[0].Font = new Font("Arial", 12, FontStyle.Bold);

        //    ////////chart.PerformLayout();
        //    ////////chart.Update();
        //    con.Close();

        //    //chrtDoctors.Height = 400;



        //}

        private void tabControl1_Click(object sender, EventArgs e)
        {

            // chrtRevenue.Titles.Add("Revenue");



        }
        private void refreshCharts(object sender, EventArgs e)
        {
            
////////////            if (ezc.Metric == null)
////////////            {
                
////////////                    ezc.Metric = "Revenue";
////////////                    ezc.Select = " isnull(floor(SUM(v.netpayment)),0) ";
////////////                    ezc.SqlDate = "v.ServiceDt ";
              
////////////                ezc.From = " from enc e ";
////////////                ezc.Join = " left join edi_invoice v on  e.invoiceid = v.id  ";
////////////                ezc.Span = " Past Year";
////////////                ezc.Range = " between '" + DateTime.Now.AddDays(-365).ToString("yyyy-MM-dd") + "' and '" + new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddDays(-1).ToString("yyyy-MM-dd") + "'";
////////////                ezc.DateFormat = "'MMM yy'";
////////////            }
////////////            // 3 and 4 is from and join
////////////            string where = ezc.Select.Contains("v.") ? " and v.deleteflag = 0" : " and e.deleteflag = 0 and e.status = 'chk' and e.claimreq = 1";
////////////            if (ezc.Metric == "Revenue Per Visit") where += " and (v.splitclaimid > v.id or v.splitclaimid = 0) ";
////////////            string sql = "select " + ezc.Select + "[" + ezc.Metric + "], " + ezc.GetFormattedDate() + "  span " + ezc.From + ezc.Join + " where  " + ezc.SqlDate + ezc.Range + where + " group by " + ezc.GetFormattedDate() + " order by min(" + ezc.SqlDate + ")";
////////////            createChart(chrtRevenue, sql, ezc.Metric, "span", true);
////////////            createChart(chrtPieRevenue, sql, ezc.Metric, "span", false);

////////////            sql = "select " + ezc.Select + "[" + ezc.Metric + "], " + specialityDetail + @" Specialty " + ezc.From + ezc.Join + (ezc.Join.Contains("doctor") ? "" : "  left join doctors doc on e.doctorid = doc.doctorid = d.doctorID") + " where " + ezc.SqlDate + ezc.Range + where + " group by " + specialityDetail + @"  order by" + ezc.Select + " desc ";
////////////            createChart(chrtSpecialties, sql, ezc.Metric, "Specialty", true);


////////////            string age = @" case  
////////////when floor(datediff(d, u.dob, e.date) / 365.25) <= 10 then '0 to 10'
////////////when floor(datediff(d, u.dob, e.date) / 365.25) <= 20 then '11 to  20'
////////////when floor(datediff(d, u.dob, e.date) / 365.25) <= 30 then '21 to 30'
////////////when floor(datediff(d, u.dob, e.date) / 365.25) <= 40 then '31 to 40'
////////////when floor(datediff(d, u.dob, e.date) / 365.25) <= 50 then '41 to 50'
////////////when floor(datediff(d, u.dob, e.date) / 365.25) <= 60 then '51 to 60'
////////////when floor(datediff(d, u.dob, e.date) / 365.25) <= 70 then '61 to 70'
////////////when floor(datediff(d, u.dob, e.date) / 365.25) <= 80 then '71 to 80'
////////////when floor(datediff(d, u.dob, e.date) / 365.25) <= 90 then '81 to 90'
////////////when floor(datediff(d, u.dob, e.date) / 365.25) <= 100 then '91 to 100'
////////////else 'Over 100' END ";
////////////            sql = "select " + ezc.Select + "[" + ezc.Metric + @"], " + age + " Age " + ezc.From + ezc.Join + (ezc.Join.Contains("doctor") ? "" : "  left join doctors doc on e.doctorid = doc.doctorid ") + (ezc.Join.Contains("user") ? "" : " left join users u on  e.patientid = u.uid") + " where " + ezc.SqlDate + ezc.Range + where + @" group by " + age + "order by" + ezc.Select + " desc ";
////////////            createChart(chrtPieSpecialty, sql, ezc.Metric, "Age", false);

////////////            sql = "select " + ezc.Select + "[" + ezc.Metric + "], " + getFacilityNameSql + " Facility " +ezc.From + ezc.Join + " where  " + ezc.SqlDate + ezc.Range + where + " group by " + getFacilityNameSql +  @"  order by" + ezc.Select + " desc ";
////////////            createChart(chrtFacilities, sql, ezc.Metric, "Facility", true);


////////////            sql = "select " + ezc.Select + "[" + ezc.Metric + "], " + " case when pat.race = '' or pat.race = 'Declined to Specify' or pat.race = 'Other Race' or pat.race like '%report%' then 'NA' when pat.race like '%black%'  or pat.race like '%African%'  then 'Black' else pat.race end" + ezc.From + ezc.Join + "   left join patients pat on e.patientid = pat.pid where  " + ezc.SqlDate + ezc.Range + where +
////////////                @" group by  case when pat.race = '' or pat.race = 'Declined to Specify' or pat.race = 'Other Race' or pat.race like '%report%' then 'NA' when pat.race like '%black%'  or pat.race like '%African%'  then 'Black' else pat.race end order by" + ezc.Select + " desc ";



////////////            createChart(chrtPieFacilities, sql, ezc.Metric, "Race", false);

////////////            sql = "select top(20) " + ezc.Select + "[" + ezc.Metric + "], concat(du.ufname, ' ' , du.ulname) doctor " + ezc.From + ezc.Join + "  left join users du on   e.doctorid = du.uid   where " + ezc.SqlDate + ezc.Range + where + " group by concat(du.ufname, ' ' , du.ulname)  order by" + ezc.Select + " desc ";

////////////            createChart(chrtDoctors, sql, ezc.Metric, "Doctor", true);



////////////            sql = "select top(20) " + ezc.Select + "[" + ezc.Metric + "],  case when U.zipcode = '' then 'na' else SUBSTRING(U.zipcode, 1, 5) end " + ezc.From + ezc.Join + (ezc.Join.Contains("users") ? "" : "  left join users u on  e.patientid = u.uid ") + "   where " + ezc.SqlDate + ezc.Range + where + " group by  case when U.zipcode = '' then 'na' else SUBSTRING(U.zipcode, 1, 5) end  order by" + ezc.Select + " desc ";

////////////            createChart(chrtPieDoctors, sql, ezc.Metric, "ZIP Code", false);

////////////            sql = "select " + ezc.Select + "[" + ezc.Metric + "], p.name Class " + ezc.From + ezc.Join + (!ezc.Join.Contains("mix") ? @" 
//////////// left join insurance i on v.PrimaryInsId = i.insId left join ins_payer_mix p on i.insuranceclass = p.Code and p.id != 24 left join insurance si on v.SecondaryInsId = si.insId
//////////// left join ins_payer_mix sp on si.insuranceclass = sp.Code and sp.id != 24" : "") + @" where "
////////////  + ezc.SqlDate + ezc.Range + where + " group by  p.name  order by" + ezc.Select + " desc ";

////////////            createChart(chrtClass, sql, ezc.Metric, "Class", true);



////////////            chrtFacilities.ChartAreas[0].AxisX.LabelStyle.Angle = 35;
////////////            chrtDoctors.ChartAreas[0].AxisX.LabelStyle.Angle = 50;
////////////            //chrtRevenue.ChartAreas[0].AxisX.LabelStyle.Angle = -25;
////////////            //chrtDoctors.ChartAreas[0].Area3DStyle.Enable3D = false;

////////////            //chrtDoctors.ChartAreas[0].AxisX.ScrollBar.Enabled = true; // Enable vertical scrollbar
////////////            //chrtDoctors.ChartAreas[0].AxisX.ScrollBar.IsPositionedInside = true; // Position the scrollbar inside the chart area
////////////            //chrtDoctors.ChartAreas[0].AxisX.ScrollBar.ButtonStyle = System.Windows.Forms.DataVisualization.Charting.ScrollBarButtonStyles.ResetZoom;
////////////            // chrtDoctors.ChartAreas[0].AxisX.ScaleView.Size = 20;
////////////            //chrtFacilities.ChartAreas[0].AxisX.ScaleView.Size = 10;
////////////            //chrtDoctors.ChartAreas[0].AxisX.ScrollBar.Size = 20;
////////////            // chrtDoctors.ChartAreas[0].Position.Width = 100;
////////////            // chrtDoctors.ChartAreas[0].AxisX.ScaleView.MinSize = 10;
////////////            //chrtSpecialties.ChartAreas[0].AxisX.LabelStyle.Angle = -25;
////////////            chrtSpecialties.ChartAreas[0].AxisX.ScrollBar.Enabled = true; // Enable vertical scrollbar
////////////            chrtSpecialties.ChartAreas[0].AxisX.ScrollBar.IsPositionedInside = true; // Position the scrollbar inside the chart area
////////////            chrtSpecialties.ChartAreas[0].AxisX.ScrollBar.ButtonStyle = System.Windows.Forms.DataVisualization.Charting.ScrollBarButtonStyles.ResetZoom;
////////////            //chrtSpecialties.ChartAreas[0].AxisX.ScaleView.Size = 10;
////////////            //chrtSpecialties.ChartAreas[0].AxisX.ScrollBar.Size = 20;
////////////            //chrtSpecialties.ChartAreas[0].AxisX.ScaleView.MinSize = 10;
////////////            //chrtDoctors.Width = 1900;  // Adjust the width as needed



        }
        private void tabPage1_Click(object sender, EventArgs e)
        {


        }
        private void enableCategoryLabels()
        {
            lblRevenueStat.ForeColor = Color.White;
            lblPatientsStat.ForeColor = Color.White;
            lblVisitsStat.ForeColor = Color.White;
            lblRevenuePerVisit.ForeColor = Color.White;
            lblVisitsPerDay.ForeColor = Color.White;
            if (lblMinutesPerVisit.Visible) lblMinutesPerVisit.ForeColor = Color.White;

        }

        private void enableSpanLabels()
        {
            lblPastYearStat.ForeColor = Color.White;
            lblLastYearStat.ForeColor = Color.White;
            lblYearToDateStat.ForeColor = Color.White;
            lblAlltimeStat.ForeColor = Color.White;
        }


        private void lblRevenueStat_Click(object sender, EventArgs e)
        {
            if (lblRevenueStat.ForeColor == Color.White)
            {
                enableCategoryLabels();
                lblRevenueStat.ForeColor = Color.DimGray;
                ezc.Metric = "Revenue";
                ezc.Select = " isnull( floor( SUM(v.netpayment)),0) ";
                ezc.SqlDate = "v.ServiceDt ";
                ezc.From = " from enc e   ";
                ezc.Join = " left join edi_invoice v on e.invoiceid = v.id   ";
                ezc.Where = "";
                //     lblAlltimeStat_Click(sender, e);

             
                    if (cboCategory.SelectedIndex == 0 && !ezc.Join.Contains("doctor"))
                    {
                        ezc.DateGroup = "'yy MM'";
                        ezc.Join += " left join doctors doc on e.doctorid = doc.doctorid ";
                    }
                    createPerfomanceChart();
                
            }
        }

        private void lblPatientsStat_Click(object sender, EventArgs e)
        {
            if (lblPatientsStat.ForeColor == Color.White)
            {
                enableCategoryLabels();
                lblPatientsStat.ForeColor = Color.DimGray;
                ezc.Metric = "Patients";
                ezc.Select = " count( distinct e.patientid) ";
                ezc.SqlDate = "e.date ";
                ezc.From = " from enc e ";
                ezc.Join = "left join edi_invoice v on  e.invoiceid  = v.id   ";
                ezc.Where = " and e.deleteflag = 0 and e.status = 'chk' and e.claimreq = 1 ";
                //  lblAlltimeStat_Click(sender, e);
              
                    if (cboCategory.SelectedIndex == 0 && !ezc.Join.Contains("doctor")) { ezc.Join += " left join doctors doc on e.doctorid = doc.doctorid "; }
                    createPerfomanceChart();
                
            }
        }

        private void lblVisitsStat_Click(object sender, EventArgs e)
        {
            if (lblVisitsStat.ForeColor == Color.White)
            {
                enableCategoryLabels();
                lblVisitsStat.ForeColor = Color.DimGray;
                ezc.Metric = "Visits";
                ezc.Select = " count( distinct e.encounterid) ";
                ezc.SqlDate = "e.date ";
                ezc.From = " from enc e ";
                ezc.Join = "left join edi_invoice v on  e.invoiceid  = v.id   ";
                ezc.Where = " and e.deleteflag = 0 and e.status = 'chk' and e.claimreq = 1 ";
                //  lblAlltimeStat_Click(sender, e);
             
                    if (cboCategory.SelectedIndex == 0 && !ezc.Join.Contains("doctor")) { ezc.Join += "  left join doctors doc on e.doctorid = doc.doctorid "; }
                    createPerfomanceChart();
                
            }
        }

        private void lblRevenuePerVisit_Click(object sender, EventArgs e)
        {
            if (lblRevenuePerVisit.ForeColor == Color.White)
            {
                enableCategoryLabels();
                lblRevenuePerVisit.ForeColor = Color.DimGray;
                ezc.Metric = "Revenue Per Visit";
                ezc.Select = " isnull(    floor(  Avg(v.netpayment)  )   ,0)";
                ezc.SqlDate = "e.date ";
                ezc.From = " from enc e ";
                ezc.Join = " left join edi_invoice v on  e.invoiceid  = v.id ";
                ezc.Where = " and e.deleteflag = 0 and e.status = 'chk' and e.claimreq = 1 and v.filestatus !='INA' and v.netpayment > 0 ";



                
                    if (cboCategory.SelectedIndex == 0 && !ezc.Join.Contains("doctors")) { ezc.Join += "  left join doctors doc on e.doctorid = doc.doctorid "; }
                    createPerfomanceChart();
               
            }

        }
        private void lblVisitsPerDay_Click(object sender, EventArgs e)
        {
            if (lblVisitsPerDay.ForeColor == Color.White)
            {
                enableCategoryLabels();
                lblVisitsPerDay.ForeColor = Color.DimGray;
                ezc.Metric = "Visits Per Day";
                ezc.Select = " isnull(    floor( COUNT(e.encounterID) * 1.0 / COUNT(DISTINCT date)  )   ,0)";
                ezc.SqlDate = "e.date ";
                ezc.From = " from enc e ";
                ezc.Join = " left join edi_invoice v on  e.invoiceid  = v.id  ";
                ezc.Where = " and e.deleteflag = 0 and e.status = 'chk' and e.claimreq = 1  ";
            
                    if (cboCategory.SelectedIndex == 0) { ezc.Join += "  left join doctors doc on e.doctorid = doc.doctorid "; }
                    createPerfomanceChart();
                
            }

        }

        private void lblMinutesPerVisit_Click(object sender, EventArgs e)
        {
            if (lblVisitsPerDay.ForeColor == Color.White)
            {
                enableCategoryLabels();
                lblMinutesPerVisit.ForeColor = Color.DimGray;
                ezc.Metric = "Minutes Per Visit";
                ezc.Select = " isnull(    floor( COUNT(e.encounterID) * 1.0 / COUNT(DISTINCT date)  )   ,0)";
                ezc.SqlDate = "e.date ";
                ezc.From = " from enc e ";
                ezc.Join = " left join edi_invoice v on  e.invoiceid  = v.id  ";
                ezc.Where = " and e.deleteflag = 0 and e.status = 'chk' and e.claimreq = 1  ";

                if (cboCategory.SelectedIndex == 0) { ezc.Join += " left join doctors doc on e.doctorid = doc.doctorid "; }
                createPerfomanceChart();

            }
        }

        private void chart1_Click_1(object sender, EventArgs e)
        {

        }

        private void lblAlltimeStat_Click(object sender, EventArgs e)
        {
            if (lblAlltimeStat.ForeColor == Color.White)
            {
                enableSpanLabels();
                lblAlltimeStat.ForeColor = Color.DimGray;
                ezc.Span = " All Time ";
                ezc.Range = " > '2017-12-31' ";
                ezc.DateFormat = "'yyyy'";
                ezc.DateGroup = "'yyyy'";
                createPerfomanceChart();
            }
        }

        private void lblYearToDateStat_Click(object sender, EventArgs e)
        {
            if (lblYearToDateStat.ForeColor == Color.White)
            {
                enableSpanLabels();
                lblYearToDateStat.ForeColor = Color.DimGray;
                ezc.Span = " Year To Date ";
                ezc.Range = " between '" + new DateTime(DateTime.Now.Year, 1, 1).ToString("yyyy-MM-dd") + "'  and '" + new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddDays(-1).ToString("yyyy-MM-dd") + "'";
                ezc.DateFormat = "'MMM'";
                ezc.DateGroup = "'MM'";
                createPerfomanceChart();
            }
        }

        private void lblPastYearStat_Click(object sender, EventArgs e)
        {
            if (lblPastYearStat.ForeColor == Color.White)
            {
                enableSpanLabels();
                lblPastYearStat.ForeColor = Color.DimGray;
                ezc.Span = " Past Year ";
                ezc.Range = " between '" + DateTime.Now.AddDays(-365).ToString("yyyy-MM-dd") + "' and '" + new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddDays(-1).ToString("yyyy-MM-dd") + "'";
                ezc.DateFormat = "'MMM yyyy'";
                ezc.DateGroup = "'yy MM'";
                createPerfomanceChart();
            }
        }

        private void lblLastYearStat_Click(object sender, EventArgs e)
        {
            if (lblLastYearStat.ForeColor == Color.White)
            {
                enableSpanLabels();
                lblLastYearStat.ForeColor = Color.DimGray;
                ezc.Span = " Last Year ";
                ezc.Range = " between '" + new DateTime(DateTime.Now.Year - 1, 1, 1).ToString("yyyy-MM-dd") + "' and '" + new DateTime(DateTime.Now.Year - 1, 12, 31).ToString("yyyy-MM-dd") + "'";
                ezc.DateFormat = "'MMM'";
                ezc.DateGroup = "'MM'";
                createPerfomanceChart();
            }
        }



        int ChartIndex = 0;
       

        private void chrtRevenue_MouseMove(object sender, MouseEventArgs e)
        {


        }


        private void chrtDoctors_MouseMove(object sender, MouseEventArgs e)
        {

        }

        private void chrtDoctors_MouseHover(object sender, EventArgs e)
        {

        }



        private void chrtDoctors_Click_1(object sender, EventArgs e)
        {

        }

        private void chrtRevenue_Click_1(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            ezc.mouseHover = true;
        }


        private string thousands(double amount)
        {
            string format = "#,##0";

            if(amount > 999)
            {
                format = "#,##0,K";
            }

            return format;
        }


        private void createPerfomanceChart()
        {


if (ezc.DateGroup == null)
            {

            lblCategory.Visible = true;
            cboCategory.Visible = true;
            pnlConditions.Visible = true;
            trkIncline.Value = chrtPerformance.ChartAreas[0].Area3DStyle.Inclination;
            trkRotation.Value = chrtPerformance.ChartAreas[0].Area3DStyle.Rotation;

            

                ezc.Metric = "Revenue";
                ezc.Select = " isnull(floor(SUM(v.netpayment)),0) ";

                ezc.SqlDate = " e.date ";
                ezc.Span = " Past Year";
                ezc.From = " from enc e ";
                ezc.Range = " between '" + DateTime.Now.AddDays(-365).ToString("yyyy-MM-dd") + "' and '" + new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddDays(-1).ToString("yyyy-MM-dd") + "'";
                ezc.DateFormat = "'MMM yy'";
                ezc.DateGroup = "'yy MM'";
                ezc.Join = " left join edi_invoice v on e.encounterid = v.encounterid  left join doctors doc on e.doctorid = doc.doctorid";
                ezc.Where = " and e.deleteflag = 0 and e.status = 'chk' and e.claimreq = 1 ";
           
            comboBoxChartTypes.Items.Clear();
            foreach (var chartType in chartTypes)
            {
                comboBoxChartTypes.Items.Add(chartType.Key);
            }
 
            // Set the default selected item
            comboBoxChartTypes.SelectedIndexChanged -= comboBoxChartTypes_SelectedIndexChanged;
            comboBoxChartTypes.SelectedIndex = 0;
            comboBoxChartTypes.SelectedIndexChanged += comboBoxChartTypes_SelectedIndexChanged;
            cboCategory.SelectedIndexChanged -= cboCategory_SelectedIndexChanged;
            cboCategory.SelectedIndex = 0;
            cboCategory.SelectedIndexChanged += cboCategory_SelectedIndexChanged;

            chrtPerformance.ChartAreas[0].AxisY.LabelStyle.Format = "#,##0";
           }
       
            if (chkConditions.Items.Count > 0)
            {
                chkConditions.Items.Clear();
                //ezc.Conditions.Clear();

            }
            chkConditions.ItemCheck -= chkConditions_ItemCheck;

            DataTable dt = new DataTable();
            if (cboCategory.Text == "Doctors")
            {
                ezc.SeriesField = " doc.printname ";
                if (!ezc.Join.Contains("doctors")) ezc.Join += @"  left join doctors doc on e.doctorid = doc.doctorid ";
            }
            else if (cboCategory.Text == "Facilities")
            {
                ezc.SeriesField = "f.name";
                if (!ezc.Join.Contains("edi_facilities")) ezc.Join += @" left join edi_facilities f on e.facilityId = f.id";
            }
            else if (cboCategory.Text == "Insurance")
            {
                ezc.SeriesField = " case when ins.insurancename is null then 'NA' else ins.insurancename end ";
                if (!ezc.Join.Contains("ins.insId")) ezc.Join += @"  left join insurance ins on v.PrimaryInsId = ins.insId  left join insurance si on v.SecondaryInsId = si.insId
 ";
            }
            else if (cboCategory.Text == "Specialty")
            {
                ezc.SeriesField = specialityDetail;
                if (!ezc.Join.Contains("doctors")) ezc.Join += @"  left join doctors doc on e.doctorid = doc.doctorid ";
            }
            else if (cboCategory.Text == "Race")
            {
                ezc.SeriesField = " case when pat.race = '' or pat.race = 'Declined to Specify' or pat.race = 'Other Race' or pat.race like '%report%' then 'NA' when pat.race like '%black%'  or pat.race like '%African%'  then 'Black' else pat.race end ";
                if (!ezc.Join.Contains("pat.")) ezc.Join += @"   left join patients pat on e.patientid = pat.pid ";
            }
            else if (cboCategory.Text == "Location")
            {
                ezc.SeriesField = " case when U.zipcode = '' then 'na' else SUBSTRING(U.zipcode, 1, 5) end ";
                if (!ezc.Join.Contains("user")) ezc.Join += @"   left join users u on  e.patientid = u.uid ";
            }
            else if (cboCategory.Text == "Age")
            {

                ezc.SeriesField = @" case  
when floor(datediff(d, u.dob, e.date) / 365.25) <= 10 then '0 to 10'
when floor(datediff(d, u.dob, e.date) / 365.25) <= 20 then '11 to  20'
when floor(datediff(d, u.dob, e.date) / 365.25) <= 30 then '21 to 30'
when floor(datediff(d, u.dob, e.date) / 365.25) <= 40 then '31 to 40'
when floor(datediff(d, u.dob, e.date) / 365.25) <= 50 then '41 to 50'
when floor(datediff(d, u.dob, e.date) / 365.25) <= 60 then '51 to 60'
when floor(datediff(d, u.dob, e.date) / 365.25) <= 70 then '61 to 70'
when floor(datediff(d, u.dob, e.date) / 365.25) <= 80 then '71 to 80'
when floor(datediff(d, u.dob, e.date) / 365.25) <= 90 then '81 to 90'
when floor(datediff(d, u.dob, e.date) / 365.25) <= 100 then '91 to 100'
else 'Over 100' END";
                if (!ezc.Join.Contains("user")) ezc.Join += @"   left join users u on  e.patientid = u.uid ";
            }
            else if (cboCategory.Text == "General")
            {
                ezc.SeriesField = @" case   when e.deleteFlag = 0 then 'Amount' end";
              }
            if (lblMinutesPerVisit.ForeColor == Color.DimGray && lblMinutesPerVisit.Visible)
            {
                dt = getSqlTable(dt, @"  select avg(minutes) [Minutes Per Visit],Category Category from
(select " + ezc.SeriesField + " Category, datediff(minute, min(e.startTime), max(e.endtime)) /count(" + ezc.SeriesField + @") minutes ,e.date date
from enc e " + ezc.Join + " where  " + ezc.SqlDate + ezc.Range + ezc.Where + " group by  " + ezc.SeriesField + " , Date ) l group by l.Category order by avg(minutes)");
            }
            else
            {
                dt = getSqlTable(dt, @"  select * from (select " + ezc.Select + "[" + ezc.Metric + "], " + ezc.SeriesField + " category from enc e " + ezc.Join + " where " + ezc.SqlDate + ezc.Range
                                + ezc.Where + " group by " + ezc.SeriesField + ") sql where sql.[" + ezc.Metric + "] > 0 order by [" + ezc.Metric + "] desc ");
            }


            int ctr = 0;
            List<ezwChart.chartSeries> chartseries = new List<ezwChart.chartSeries>(0);


            foreach (DataRow row in dt.Rows)
            {
                ezwChart.chartSeries category = new ezwChart.chartSeries();
                category.Category = row[1].ToString();
                category.Enabeled = ctr <= 10;
                category.Amount = Convert.ToInt32(row[0].ToString());
                chkConditions.Items.Add(row[1], ctr <= 10);

                ctr++;

                chartseries.Add(category);


            }
            ezc.series = chartseries;

            //    ezc.Conditions = conditions;
            chkConditions.ItemCheck += chkConditions_ItemCheck;

            string sql = "";









            sql =
          @"  select * from (  select " + ezc.Select + "[" + ezc.Metric + "]," + ezc.SeriesField + " Category, format(" + ezc.SqlDate + ", " + ezc.DateFormat + ")  Month, format(" + ezc.SqlDate + ", " + ezc.DateGroup + ")  DD from enc e " + ezc.Join + " where " + ezc.SqlDate + ezc.Range
              + ezc.Where + "  group by " + ezc.SeriesField + " , format(" + ezc.SqlDate + ", " + ezc.DateFormat + "),format(" + ezc.SqlDate + "," + ezc.DateGroup + ") ) sql where sql.[" + ezc.Metric + "] > 0  order by sql.DD  , sql.Category";

            if (lblMinutesPerVisit.ForeColor == Color.DimGray && lblMinutesPerVisit.Visible)
                sql = @"  select avg(minutes) [Minutes Per Visit],Category Category, format(date , " + ezc.DateFormat + ")  Month, format(l.date , " + ezc.DateGroup + @")  DD from
(select " + ezc.SeriesField + " Category, datediff(minute, min(e.startTime), max(e.endtime)) /count(" + ezc.SeriesField + @") minutes ,e.date date
from enc e " + ezc.Join + " where  " + ezc.SqlDate + ezc.Range + ezc.Where + " group by  " + ezc.SeriesField + " , Date ) l group by l.Category, format(l.date, " + ezc.DateFormat + ")  , format(l.date, " + ezc.DateGroup + ") order by DD , l.Category";


            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand(sql, connection);
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            DataTable dataTable = new DataTable();
            connection.Open();
            adapter.Fill(dataTable);


            connection.Close();

            chrtPerformance.Series.Clear();
            chrtPerformance.ChartAreas[0].AxisX.CustomLabels.Clear();
            chrtPerformance.ChartAreas[0].AxisX.Title = ezc.Span;
            chrtPerformance.ChartAreas[0].AxisY.Title = ezc.Metric;
            chrtPerformance.ChartAreas[0].AxisX.Interval = 1;
            //   chrtPerformance.ChartAreas[0].AxisX.Minimum = 1;
            chrtPerformance.ChartAreas[0].AxisX.LabelStyle.Angle = 25;
            //   chrtPerformance.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
            chrtPerformance.ChartAreas[0].Area3DStyle.Enable3D = true;

            chrtPerformance.ChartAreas[0].AxisX.LabelStyle.ForeColor = Color.White;
            chrtPerformance.ChartAreas[0].AxisY.LabelStyle.ForeColor = Color.White;
            chrtPerformance.ChartAreas[0].AxisY.LabelStyle.Font = new Font("Arial", 12, FontStyle.Bold);
            chrtPerformance.ChartAreas[0].AxisX.LabelStyle.Font = new Font("Arial", 12, FontStyle.Bold);
            // Add series for each doctor
            List<string> doctors = new List<string>();
            ctr = 0;


            string span = dataTable.Rows[0]["month"].ToString();
            chrtPerformance.ChartAreas[0].AxisX.CustomLabels.Add(ctr, ctr + 0.5, span);
            foreach (DataRow row in dataTable.Rows)

            {
                if (span != row["month"].ToString())
                {
                    //    MessageBox.Show(span);
                    span = row["month"].ToString();

                    ctr++;
                    chrtPerformance.ChartAreas[0].AxisX.CustomLabels.Add(ctr, ctr + 0.5, span);

                }
                string category = row["category"].ToString();
                if (!doctors.Contains(category))
                {

                    doctors.Add(category);

                    System.Windows.Forms.DataVisualization.Charting.Series series = new System.Windows.Forms.DataVisualization.Charting.Series(category);


                    if (comboBoxChartTypes.Items.Count > 0)
                    {
                        string selectedChartType = comboBoxChartTypes.SelectedItem.ToString();
                        System.Windows.Forms.DataVisualization.Charting.SeriesChartType chartType = chartTypes[selectedChartType];
                        series.ChartType = chartType;
                        series.Enabled = false;
                    }
                    series.Enabled = false;

                    chrtPerformance.Series.Add(series);





                }

                for (int i = chrtPerformance.Series[category].Points.Count; i < ctr; i++)
                {
                    chrtPerformance.Series[category].Points.AddXY(ctr, 0);
                }

                int patientsCount = Convert.ToInt32(row[0]);
                chrtPerformance.Series[category].Points.AddXY(ctr, patientsCount);
                //  chrtPerformance.Series[category].Points[ctr].ToolTip = chrtPerformance.Series[category].Name + Environment.NewLine + chrtPerformance.ChartAreas[0].AxisX.CustomLabels[ctr].Text.ToString() + Environment.NewLine + chrtPerformance.Series[category].Points[ctr].YValues[0];
                chrtPerformance.Series[category]["LabelStyle"] = "Right";

                //    if (ctr == 10) break;
            }

            // Bind data to series

            int conditionCtr = 0;
            for (int i = 0; i < chkConditions.Items.Count; i++)
            {
                if (chkConditions.GetItemChecked(i))
                {
                    chrtPerformance.Series[chkConditions.Items[i].ToString()].Enabled = true;
                    conditionCtr++;
                    if (conditionCtr > 11) break;
                }

            }

            foreach (System.Windows.Forms.DataVisualization.Charting.Series s in chrtPerformance.Series)
            {
                //  MessageBox.Show(s["PointWidth"]);// = "10";
                //s["PointWidth"] = ".3";
                //  s.MarkerSize = 15;


                int k = 0;
                string lbl;

                for(int pctr =0;pctr< s.Points.Count;pctr++)
                {

 if (chrtPerformance.Series.Count > 1)
                {
                    k = s.Points.Count > 1 ? 1 : 0;
                        lbl = s.Name;
                }
                else
                {
                        k = pctr;
                        lbl = s.Points[pctr].YValues[0].ToString(thousands(double.Parse(s.Points[pctr].YValues[0].ToString())));
                }
 s.Points[k].Label = lbl;
                s.Points[0].LabelAngle = 25;
                //  s.Points[0].LabelToolTip = "lblERAAnalysis";

                s.Points[k].Font = new Font("Arial", 12, FontStyle.Bold);
                s.Points[k].LabelForeColor = Color.White;
                s.Points[k].LabelBackColor = Color.FromArgb(50, Color.Black);

                    if (chrtPerformance.Series.Count > 1) break;
                }


               
               
                // System.Windows.Forms.DataVisualization.Charting.DataPoint p = s.Points[s.Points.Count / 2];

               
                //s.Points[s.Points.Count / 2] = p;
                //   p.LabelBackColor = Color.White;
            }


            foreach (System.Windows.Forms.DataVisualization.Charting.Series s in chrtPerformance.Series)
            {
                // s.MarkerSize = 10;

                foreach (System.Windows.Forms.DataVisualization.Charting.DataPoint p in s.Points)
                {

                    //   p.ToolTip = s.Name +Environment.NewLine + p.AxisLabel + Environment.NewLine + p.YValues[0]   ;
                    //   chart.Series[0].Points[chart.Series[0].Points.Count - 1].ToolTip = dr[1].ToString() + Environment.NewLine + String.Format("{0:#,##0}", sumNum); //dr[0]
                }
            }

            chrtPerformance.Titles[0].Text =  (ezc.Span + " " + ezc.Metric + " By " + cboCategory.Text).Replace("By General","");

            chrtPerformance.ResetAutoValues();
            // chrtPerformance.ChartAreas[0].RecalculateAxesScale();


            chrtPerformance.Update();

            //  s[0].Points.Count - 1].YValues[0];
            //int ctr = 0;
            //foreach (DataRow row in ds.Tables[0].Rows)
            //{




            //    // Add data points to the series
            //    chart1.Series[0].Points.AddXY(row[series], row[category]);


            //    ctr++;
            //}


        }



        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {

            ////////////////// MessageBox.Show(tabControl1.SelectedTab.Text);
            ////////////////grpStats.Visible = true;
            ////////////////grpStats.Top = tabControl1.Top + 30;
            ////////////////lblCategory.Visible = false;
            ////////////////cboCategory.Visible = false;
            ////////////////pnlConditions.Visible = false;
          
            ////////////////if (tabControl1.SelectedTab.Text == "Performance")
            ////////////////{

            ////////////////    lblCategory.Visible = true;
            ////////////////    cboCategory.Visible = true;
            ////////////////    pnlConditions.Visible = true;
            ////////////////    trkIncline.Value = chrtPerformance.ChartAreas[0].Area3DStyle.Inclination;
            ////////////////    trkRotation.Value = chrtPerformance.ChartAreas[0].Area3DStyle.Rotation;

            ////////////////    if (ezc.DateGroup == null)
            ////////////////    {
                  
            ////////////////            ezc.Metric = "Revenue";
            ////////////////            ezc.Select = " isnull(floor(SUM(v.netpayment)),0) ";
                  
            ////////////////        ezc.SqlDate = " e.date ";
            ////////////////        ezc.Span = " Past Year";
            ////////////////        ezc.From = " from enc e ";
            ////////////////        ezc.Range = " between '" + DateTime.Now.AddDays(-365).ToString("yyyy-MM-dd") + "' and '" + new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddDays(-1).ToString("yyyy-MM-dd") + "'";
            ////////////////        ezc.DateFormat = "'MMM yy'";
            ////////////////        ezc.DateGroup = "'yy MM'";
            ////////////////        ezc.Join = " left join edi_invoice v on e.encounterid = v.encounterid  left join doctors doc on e.doctorid = doc.doctorid";
            ////////////////        ezc.Where = " and e.deleteflag = 0 and e.status = 'chk' and e.claimreq = 1 ";
            ////////////////    }
            ////////////////    comboBoxChartTypes.Items.Clear();
            ////////////////    foreach (var chartType in chartTypes)
            ////////////////    {
            ////////////////        comboBoxChartTypes.Items.Add(chartType.Key);
            ////////////////    }

            ////////////////    // Set the default selected item
            ////////////////    comboBoxChartTypes.SelectedIndexChanged -= comboBoxChartTypes_SelectedIndexChanged;
            ////////////////    comboBoxChartTypes.SelectedIndex = 0;
            ////////////////    comboBoxChartTypes.SelectedIndexChanged += comboBoxChartTypes_SelectedIndexChanged;
            ////////////////    cboCategory.SelectedIndexChanged -= cboCategory_SelectedIndexChanged;
            ////////////////    cboCategory.SelectedIndex = 0;
            ////////////////    cboCategory.SelectedIndexChanged += cboCategory_SelectedIndexChanged;
            ////////////////    createPerfomanceChart();

            ////////////////}
            ////////////////else
            ////////////////{
            ////////////////    refreshCharts(null, null);
            ////////////////}
        }
        private void gridAppointments_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            DataGridView dataGridView = sender as DataGridView;

            e.PaintBackground(e.CellBounds, true);
            if (e.RowIndex < 0)
            {
                e.PaintContent(e.ClipBounds);
                dataGridView.ColumnHeadersDefaultCellStyle.Font = new Font(dataGridView.DefaultCellStyle.Font.FontFamily, 14, FontStyle.Bold);
                dataGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.Yellow;
                dataGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.Black;
            }
            if (e.RowIndex >= 0 && e.RowIndex < dataGridView.Rows.Count)
            {


                dataGridView.Rows[e.RowIndex].DefaultCellStyle.Font = new Font(dataGridView.DefaultCellStyle.Font, FontStyle.Bold);

                if (((e.ColumnIndex == 4 || e.ColumnIndex == 5) && dataGridView.Name == "gridAppointments" && dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString() != "now") || (e.ColumnIndex == 3 && dataGridView.Name != "gridAppointments"))    // Assuming the 'Label' column is at index 2
                {
                    e.Handled = true;
                    // dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.ForeColor = dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.BackColor;
                    var t = dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                    int widthValue = Int32.Parse(t.ToString());

                    using (Label label = new Label())
                    {
                        //label.Text = (widthValue).ToString();
                        label.BackColor = e.ColumnIndex == 4 ? Color.OrangeRed : e.ColumnIndex == 5 ? Color.Green : Color.DarkOrange;
                        label.TextAlign = ContentAlignment.MiddleLeft;
                        label.ForeColor = Color.White;
                        label.Font = new Font(label.Font.FontFamily, 12, FontStyle.Bold);
                        // Adjust the size of the label based on the widthValue
                        int adjustedWidth = Math.Min(widthValue / 2, e.CellBounds.Width);
                        label.Size = new Size(adjustedWidth, e.CellBounds.Height);

                        // Draw the label within the cell bounds
                        e.Graphics.FillRectangle(new SolidBrush(label.BackColor), e.CellBounds.X + 5, e.CellBounds.Y + 10, label.Width, label.Height - 15);
                        TextRenderer.DrawText(e.Graphics, FormatTimeFromInt(widthValue), label.Font, new Point(e.CellBounds.Location.X + 10, e.CellBounds.Location.Y + 10), label.ForeColor);

                    }

                }
                else
                {
                    e.PaintContent(e.ClipBounds);
                }


                if (dataGridView.Name == "gridAppointments" && dataGridView.Rows[e.RowIndex].Cells[5].Value.ToString() == "now")
                {
                    dataGridView.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.Yellow;
                }
                else { dataGridView.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.White; }
            }
            e.Handled = true;
        }


        static string FormatTimeFromInt(int time)
        {
            // Separate the integer into hours and minutes
            int hours = time / 60;
            int minutes = time % 60;

            // Format the hours and minutes as a time string
            return string.Format("{0}:{1:00}", hours, minutes);
        }

        private void trkIncline_Scroll(object sender, EventArgs e)
        {
            chrtPerformance.ChartAreas[0].Area3DStyle.Inclination = trkIncline.Value;
        }

        private void trkRotation_Scroll(object sender, EventArgs e)
        {

            //  lblCondition.Text = trkRotation.Value.ToString();
            chrtPerformance.ChartAreas[0].Area3DStyle.Rotation = trkRotation.Value;
        }



        private void cboCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            createPerfomanceChart();
            if (cboCategory.Text == "Doctors")
            {
                lblMinutesPerVisit.Visible = true;

            }
            else if (lblMinutesPerVisit.ForeColor == Color.DimGray)
            {
                lblMinutesPerVisit.ForeColor = Color.White;
                lblMinutesPerVisit.Visible = false;
                lblVisitsStat_Click(sender, e);
            }
            else { lblMinutesPerVisit.Visible = false; }
            lblCondition.Text = cboCategory.Text;
        }

        private void grpStats_Enter(object sender, EventArgs e)
        {

        }

        private void chkConditions_SelectedIndexChanged(object sender, EventArgs e)
        {


        }

        private void lblPatientsStat_Click_1(object sender, EventArgs e)
        {

        }

        private void chkConditions_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            ezc.series.Find(s => s.Category == chkConditions.Items[e.Index].ToString()).Enabeled = !chkConditions.GetItemChecked(chkConditions.SelectedIndex);
            chrtPerformance.Series[chkConditions.Items[e.Index].ToString()].Enabled = !chkConditions.GetItemChecked(chkConditions.SelectedIndex);
            chrtPerformance.ResetAutoValues();
            //  chrtPerformance.ChartAreas[0].RecalculateAxesScale();
            //   createPerfomanceChart();
        }

        private void chkConditions_SelectedIndexChanged_1(object sender, EventArgs e)
        {

        }

        private void comboBoxChartTypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedChartType = comboBoxChartTypes.SelectedItem.ToString();

            System.Windows.Forms.DataVisualization.Charting.SeriesChartType chartType = chartTypes[selectedChartType];
            for (int i = 0; i < chrtPerformance.Series.Count - 1; i++)
                chrtPerformance.Series[i].ChartType = chartType;
        }

        private Point previousLocation;
        private void chrtPerformance_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (previousLocation.Y == 0)
                {
                    previousLocation.Y = e.Y;
                    previousLocation.X = e.X;
                    return;
                }

                if (previousLocation.Y < e.Y && chrtPerformance.ChartAreas[0].Area3DStyle.Inclination < 90)
                {
                    chrtPerformance.ChartAreas[0].Area3DStyle.Inclination += 1;
                    previousLocation.Y = e.Y;
                }
                if (previousLocation.Y > e.Y && chrtPerformance.ChartAreas[0].Area3DStyle.Inclination > -90)
                {
                    chrtPerformance.ChartAreas[0].Area3DStyle.Inclination -= 1;
                    previousLocation.Y = e.Y;
                    trkIncline.Value = chrtPerformance.ChartAreas[0].Area3DStyle.Inclination;
                }

                // 


                if (previousLocation.X < e.X && chrtPerformance.ChartAreas[0].Area3DStyle.Rotation > 15)
                {
                    chrtPerformance.ChartAreas[0].Area3DStyle.Rotation -= 1;
                    previousLocation.X = e.X;
                }
                if (previousLocation.X > e.X && chrtPerformance.ChartAreas[0].Area3DStyle.Rotation < 135)
                {
                    chrtPerformance.ChartAreas[0].Area3DStyle.Rotation += 1;
                    previousLocation.X = e.X;

                    trkRotation.Value = chrtPerformance.ChartAreas[0].Area3DStyle.Rotation;

                }







            }
            System.Windows.Forms.DataVisualization.Charting.HitTestResult h = chrtPerformance.HitTest(e.X, e.Y);
            if (h.ChartElementType.ToString() == "DataPoint")
            {
                System.Windows.Forms.DataVisualization.Charting.DataPoint p = h.Series.Points[chrtPerformance.HitTest(e.X, e.Y).PointIndex];
                lblChrtData.Text = h.Series.Name + Environment.NewLine + chrtPerformance.ChartAreas[0].AxisX.CustomLabels[chrtPerformance.HitTest(e.X, e.Y).PointIndex].Text.ToString() + Environment.NewLine + (ezc.Metric.Contains("Revenue") ? "$" : "") + String.Format("{0:#,##0}", p.YValues[0]);
                lblChrtData.Top = e.Y - 75;
                lblChrtData.Left = e.X + 230;

            }

            lblChrtData.Visible = h.ChartElementType.ToString().Contains("DataPoint");

            //    MessageBox.Show(chrtPerformance.HitTest(e.X, e.Y).ChartElementType.ToString());


            //  System.Windows.Forms.DataVisualization.Charting.HitTestResult hitTestResult = chrtPerformance.HitTest(e.X, e.Y);

            //  // if this is a data point (pie slice) = detach it
            //  System.Windows.Forms.DataVisualization.Charting.DataPoint p = chrtPerformance.HitTest(e.X, e.Y).Series.Points[chrtPerformance.HitTest(e.X, e.Y).PointIndex];

            //MessageBox.Show(  );
        }

        private void lblCondition_Click(object sender, EventArgs e)
        {

        }

        private void lblSortAmount_Click(object sender, EventArgs e)
        {
            if (lblSortAmount.ForeColor == Color.DimGray) return;
            lblSortAmount.ForeColor = Color.DimGray;
            lblSortCategory.ForeColor = Color.White;

            chkConditions.ItemCheck -= chkConditions_ItemCheck;
            chkConditions.Items.Clear();
            chkConditions.Sorted = false;
            foreach (ezwChart.chartSeries cs in ezc.series)
            {
                chkConditions.Items.Add(cs.Category, cs.Enabeled);
            }
            chkConditions.ItemCheck += chkConditions_ItemCheck;
        }

        private void lblSortCategory_Click(object sender, EventArgs e)
        {
            if (lblSortCategory.ForeColor == Color.DimGray) return;
            lblSortAmount.ForeColor = Color.White;
            lblSortCategory.ForeColor = Color.DimGray;
            chkConditions.Sorted = true;

        }

        private void chkConditions_SelectedIndexChanged_2(object sender, EventArgs e)
        {

        }

        private void chrtPerformance_CustomizeLegend(object sender, System.Windows.Forms.DataVisualization.Charting.CustomizeLegendEventArgs e)
        {

        }


        private void chart_MouseLeave(object sender, EventArgs e)
        {
            if (!ezc.mouseHover) return;
            timer1.Stop();
            ezc.mouseHover = false;
            System.Windows.Forms.DataVisualization.Charting.Chart chart = sender as System.Windows.Forms.DataVisualization.Charting.Chart;

            if (chart != null)
            {
                //Point chartPoint = chart.PointToClient(Control.MousePosition);

                //// Check if the mouse is within the bounds of the chart
                //Rectangle chartBounds = chart.ClientRectangle;
                // ezc.isMouseOverChart = chartBounds.Contains(chartPoint);

                //// If the mouse is still within the chart bounds, ignore the event
                //if (ezc.isMouseOverChart)
                //{
                //        chart.MouseEnter -= chart_MouseEnter;
                //    return;
                //}

                //    ezc.isMouseOverChart = false;




                Cursor.Current = Cursors.WaitCursor;

                var chartLeft = chart.Name == "chrtPieSpecialty" || chart.Name == "chrtPieDoctors" ? chart.Left + ((chart.Width / 3) * 2) : chart.Left;
                chartLeft = chart.Name == "chrtFacilities" || chart.Name == "chrtClass" ? chart.Left + 820 : chartLeft;
                string where = ezc.Select.Contains("v.") ? " and v.deleteflag = 0" : " and e.deleteflag = 0 and e.status = 'chk' and e.claimreq = 1";



                chart.Width = chart.Name.Contains("Pie") ? chart.Width / 3 : chart.Name == "chrtDoctors" ? chart.Width : chart.Width / 2;
                chart.Height = chart.Name.Contains("Pie") ? chart.Height / 3 : chart.Height / 2;
                chart.BorderlineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.NotSet;
                chart.Left = chartLeft;

                chart.Top = chart.Name == "chrtSpecialties" || chart.Name == "chrtClass" || chart.Name == "chrtDoctors" ? chart.Top + 300 : chart.Top;
                //  if   (chart.Name.Contains("Doctors") )  createChart(chart, "select top(20) " + ezc.Select + "[" + ezc.Metric + "], concat(u.ufname, ' ' , u.ulname) doctor " + ezc.From + ezc.Join + " left join users u on e.doctorID = u.uid where " + ezc.SqlDate + ezc.Range + where + " group by concat(u.ufname, ' ' , u.ulname)  order by" + ezc.Select + " desc ", ezc.Metric, "doctor", !chart.Name.Contains("Pie"));
                //  chart.ChartAreas[0].Area3DStyle.Enable3D =   chart.Name.Contains("Pie") ?  true:false;
                //  chart.MouseEnter += chart_MouseEnter;
                Cursor.Current = Cursors.Default;
            }

        }


        private void chart_MouseMove(object sender, MouseEventArgs e)
        {

            System.Windows.Forms.DataVisualization.Charting.Chart chart = sender as System.Windows.Forms.DataVisualization.Charting.Chart;
            if (ezc.mouseHover)
            {



                if (chart != null && (chart.Height < 330))
                {

                    Cursor.Current = Cursors.WaitCursor;
                    string where = ezc.Select.Contains("v.") ? " and v.deleteflag = 0" : " and e.deleteflag = 0 and e.status = 'chk' and e.claimreq = 1";


                    var chartLeft = chart.Name == "chrtPieSpecialty" || chart.Name == "chrtPieDoctors" ? chart.Left - (chart.Width * 2) : chart.Left;
                    chartLeft = chart.Name == "chrtFacilities" || chart.Name == "chrtClass" ? chart.Left - 820 : chartLeft;

                    chart.Width = chart.Name.Contains("Pie") ? chart.Width * 3 : chart.Name == "chrtDoctors" ? chart.Width : chart.Width * 2;
                    chart.Height = chart.Name.Contains("Pie") ? chart.Height * 3 : chart.Height * 2;
                    chart.BringToFront();
                    chart.BorderlineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Solid;
                    chart.Left = chartLeft;
                    chart.Top = chart.Name == "chrtSpecialties" || chart.Name == "chrtClass" || chart.Name == "chrtDoctors" ? chart.Top - 300 : chart.Top;
                    // if (chart.Name.Contains("Doctors")) createChart(chart, "select  " + ezc.Select + "[" + ezc.Metric + "],concat(u.ufname, ' ' , u.ulname) doctor " + ezc.From + ezc.Join + " left join users u on e.doctorID = u.uid where " + ezc.SqlDate + ezc.Range + where + " group by concat(u.ufname, ' ' , u.ulname)  order by" + ezc.Select + " desc ", ezc.Metric, "doctor", !chart.Name.Contains("Pie"));
                    //if (chart.Name.ToString() != "chrtDoctors")
                    chart.ChartAreas[0].Area3DStyle.Enable3D = true;
                    Cursor.Current = Cursors.Default;
                }



            }


            System.Windows.Forms.DataVisualization.Charting.HitTestResult h = chart.HitTest(e.X, e.Y);
            // lblChrtData.Text = h.ChartElementType.ToString();
            if (h.ChartElementType.ToString() == "DataPoint" || h.ChartElementType.ToString() == "DataPointLabel" || h.ChartElementType.ToString() == "LegendItem")
            {

                //  MessageBox.Show("yes");
                ChartIndex = chart.HitTest(e.X, e.Y).PointIndex;
                System.Windows.Forms.DataVisualization.Charting.DataPoint p = h.Series.Points[chart.HitTest(e.X, e.Y).PointIndex];
                lblChrtData.Text = p.AxisLabel + Environment.NewLine + String.Format("{0:#,##0}", p.YValues[0]);
              //  lblChrtData.Top = (chart.Top + e.Y + 60) - this.VerticalScroll.Value;
                lblChrtData.Left = chart.Left + e.X + 20;


            }



            lblChrtData.Visible = h.ChartElementType.ToString().Contains("DataPoint") || h.ChartElementType.ToString().Contains("LegendItem");


        }

        private void chart_MouseEnter(object sender, EventArgs e)
        {
            timer1.Start();
        }



            private void gridAppointments_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void gridAppointments_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {


        }

        private void chkTasks_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void tabControl1_VisibleChanged(object sender, EventArgs e)
        {
          
        }

        private void lblDashBoards_Click(object sender, EventArgs e)
        {
            lblDashBoards.Enabled = false;
            Cursor.Current = Cursors.WaitCursor;
 //tabControl1.Visible = true;
            lblClose.Enabled = true;
            createPerfomanceChart();
            panel.Visible = false;
            grpDashBoard.Visible = true;
            grpDashBoard.Left = 0;
            grpDashBoard.Top = 77;
            Cursor.Current = Cursors.Default;
        }

        private void grpDashBoard_VisibleChanged(object sender, EventArgs e)
        {
            lblDashBoards.Enabled = !grpDashBoard.Visible;
        }

        private void lblViews_Click(object sender, EventArgs e)
        {

        }

        private void chrtPerformance_Click(object sender, EventArgs e)
        {

        }

        
    }
    public class SqlColumnChkList
    {
        public string Text { get; set; }
        public string Value { get; set; }
        public override string ToString()
        {
            if (this.Text != "") return this.Text;
            else
                return this.Value;
        }


    }


class LicenseCheck
    {
        private static readonly string url =
            "https://avrohomlesser.github.io/EZW/client_status.json";

        public static async Task<bool> IsClientActiveAsync(string clientId)
        {
            using (var http = new HttpClient())
            {
                try
                {
                    string json = await http.GetStringAsync(url);
                    var data = JObject.Parse(json);
                    var status = data[clientId]?.ToString()?.ToLower();
                    return status == "active";
                }
                catch
                {
                    // server unreachable — decide your default
                    return true; // allow run or block depending on policy
                }
            }
        }
    }

}