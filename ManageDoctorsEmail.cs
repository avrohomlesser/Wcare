using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MailKit.Net.Smtp;
using MailKit.Security;
using System.Data.SqlClient;
using MimeKit;
using MimeKit.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
namespace ICR2
{
    public partial class ManageDoctorsEmail : Form
    {
        private JObject settings;
      //  private EmailSettings esettings;
        public ManageDoctorsEmail()
        {
            InitializeComponent();
          
         //   LoadSettings();
        }

        
        
           

 
public void setText() {

    

            txtSMTP.Text = settings["SmtpServer"].ToString();
            txtPort.Text = settings["Port"].ToString();
            txtEmail.Text = settings["EmailAddress"].ToString();
            txtPassword.Text = settings["Password"].ToString();
            txtGreeting.Text = settings["Greeting"].ToString();
            txtHeader.Text = settings["HeaderText"].ToString();
            txtSubject.Text = settings["EmailSubject"].ToString();
            txtTestEmail.Text = settings["TestAddress"].ToString();
            chkTest.Checked = settings["TestMode"].ToObject<bool>();
            txtEmailAmount.Text = settings["TestCtr"].ToString();

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }
 public  string jsonPath ="";
        public void LoadSettings()
        {
         



        }

        private void cmdEdit_Click(object sender, EventArgs e)
        {
            cmdEdit.Enabled = false;
            cmdCancel.Enabled = true;
            cmdSave.Enabled = true;
            grpTest.Enabled = chkTest.Checked;
            grpSettings.Enabled = true;
            cmdTest.Enabled = false;
            validate_text(sender,e);
            
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            setText();
            cmdEdit.Enabled = true;
            cmdCancel.Enabled = false;
            cmdSave.Enabled = false;
            grpTest.Enabled = false;
            grpSettings.Enabled = false;
            cmdTest.Enabled = true;

        }

        private void cmdSave_Click(object sender, EventArgs e)
        {
            settings["SmtpServer"] = txtSMTP.Text;
            settings["Port"] = int.Parse(txtPort.Text);
            settings["EmailAddress"] = txtEmail.Text;
            settings["Password"] = txtPassword.Text;
            settings["Greeting"] = txtGreeting.Text;
            settings["HeaderText"] = txtHeader.Text;
            settings["EmailSubject"] = txtSubject.Text;
            settings["TestAddress"] = txtTestEmail.Text;
            settings["TestMode"] = chkTest.Checked;
            settings["TestCtr"] =  int.Parse(txtEmailAmount.Text);
            ;
            //= Save updated settings to appsettings.json
            string settingsPath = jsonPath + "settings.json";
            File.WriteAllText(settingsPath, settings.ToString());

            MessageBox.Show("Settings saved successfully!");
            cmdEdit.Enabled = true;
            cmdCancel.Enabled = false;
            cmdSave.Enabled = false;
            grpTest.Enabled = false;
            grpSettings.Enabled = false;
            cmdTest.Enabled = true;
        }

        private void chkTest_CheckedChanged(object sender, EventArgs e)
        {
            grpTest.Enabled = chkTest.Checked && grpSettings.Enabled;
            validate_text(sender,e);
        }

        private void ManageDoctorsEmail_Load(object sender, EventArgs e)
        {
            if (ICR2.Properties.Settings.Default.platform.ToString() == "lasante")
            {
                jsonPath = "C:\\ecw\\";
            }
            else
            {
                jsonPath = "C:\\Program Files\\EZw\\Application\\";
            }
            string json = File.ReadAllText(jsonPath + "settings.json"); // Load settings from file
            settings = JObject.Parse(json);
            //  settings = JsonConvert.DeserializeObject<settings>(json);

            setText();

        }

        private void cmdTest_Click(object sender, EventArgs e)
        {
            try
            {
                SmtpClient smtp = new SmtpClient();
                          
                smtp.Connect(txtSMTP.Text.ToString(), int.Parse(txtPort.Text.ToString()), SecureSocketOptions.StartTls);
                smtp.Authenticate(txtEmail.Text.ToString(), txtPassword.Text.ToString());
                MessageBox.Show("Connection Succeeded!");
              
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                
            }
           
        }

        private void validate_text(object sender, EventArgs e)
        {
            if (grpSettings.Enabled)
            {
                cmdSave.Enabled = txtEmail.Text != "" && txtGreeting.Text != "" && txtPassword.Text != "" && txtHeader.Text != "" && txtPort.Text != "" && txtSMTP.Text != "" && txtSubject.Text != "";
                if (cmdSave.Enabled && chkTest.Checked) cmdSave.Enabled = txtTestEmail.Text != "" && txtEmailAmount.Text != "";
            }
        }
    }
    //public class EmailSettings
    //{
    //    [JsonProperty("SmtpServer")]
    //    public string SmtpServer { get; set; }

    //    [JsonProperty("SmtpPort")]
    //    public int SmtpPort { get; set; }

    //    [JsonProperty("EmailAddress")]
    //    public string EmailAddress { get; set; }

    //    [JsonProperty("Password")]
    //    public string Password { get; set; }

    //    [JsonProperty("Greeting")]
    //    public string Greeting { get; set; }

    //    [JsonProperty("HeaderText")]
    //    public string HeaderText { get; set; }

    //    [JsonProperty("EmailSubject")]
    //    public string EmailSubject { get; set; }

    //    [JsonProperty("TestMode")]
    //    public Boolean TestMode { get; set; }

    //    [JsonProperty("TestAddress")]
    //    public string TestAddress { get; set; }

    //    [JsonProperty("TestCtr")]
    //    public int TestCtr { get; set; }
    //}

}
