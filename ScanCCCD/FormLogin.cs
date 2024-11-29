using CsvHelper;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScanCCCD
{
    public partial class FormLogin : Form
    {
        public FormLogin()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                // Vô hiệu hóa các điều khiển trên form
                this.Enabled = false;

                // Gửi email trong hàm send machine code
                Security.SendMachineCodeToUser();

                // Thông báo khi đã gửi yêu cầu thành công
                MessageBox.Show("Đã gửi yêu cầu");
            }
            catch (Exception ex)
            {
                // Thông báo lỗi nếu có vấn đề trong quá trình gửi email
                MessageBox.Show("Lỗi gửi mail: " + ex.Message);
            }
            finally
            {
                // Kích hoạt lại các điều khiển của form
                this.Enabled = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (Security.VerifyMachineCode(textBox1.Text))
                {
                    if (checkBox1.Checked)
                    {
                        Properties.Settings.Default.SavedMachineKey = textBox1.Text;
                        Properties.Settings.Default.Save();
                    }
                    else
                    {
                        Properties.Settings.Default.SavedMachineKey = string.Empty;
                        Properties.Settings.Default.Save();
                    }
                    this.Hide();
                    Form1 frm = new Form1(textBox1.Text, false);
                    frm.ShowDialog();

                }
                else
                {
                    MessageBox.Show("License key không hợp lệ");
                }
            } catch (Exception ex)
            {
                    MessageBox.Show(ex.StackTrace);
            }
        }

        private void FormLogin_Load(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(Properties.Settings.Default.SavedMachineKey))
            {
                textBox1.Text = Properties.Settings.Default.SavedMachineKey;
                checkBox1.Checked = true;
            }
        }


        public async Task<List<string>> CheckCodeAndDateValidity()
        {
            List<string> results = new List<string>();

            // Fetch the CSV content from the URL
            string csvContent = await DownloadCsvFile();

            // Split content by new lines to process each row
            var rows = csvContent.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var row in rows)
            {
                // Split each row by the '|' character to separate the code and date
                var columns = row.Split('|');

                if (columns.Length == 2)
                {
                    string code = columns[0];
                    string dateStr = columns[1];

                    // Check if the code is valid (for example, check if it's a 9-digit number)
                    if (IsValidCode(code))
                    {
                        // Check if the date is valid and if it is in the past compared to the current date
                        if (DateTime.TryParseExact(dateStr, "ddMMyyyy", null, System.Globalization.DateTimeStyles.None, out DateTime expiryDate))
                        {
                            if (expiryDate >= DateTime.Now)
                            {
                                results.Add(code);
                            }
                            
                           
                        }
                        else
                        {
                            results.Add($"Invalid date format: {dateStr}");
                        }
                    }
                    else
                    {
                        results.Add($"Code {code} is invalid.");
                    }
                }
            }

            return results;
        }

        // Async method to download the CSV file from a URL
        public async Task<string> DownloadCsvFile()
        {
            using (HttpClient client = new HttpClient())
            {
                return await client.GetStringAsync("https://docs.google.com/spreadsheets/d/1WlVUMQVmhs6TgK5HY5qETIlg3sULoD2Z9Skuk5Es6EU/export?format=csv");
            }
        }

        // Method to validate the code (e.g., check if it's a 9-digit number)
        public bool IsValidCode(string code)
        {
            return code.Length == 9 && code.All(char.IsDigit);
        }

        // Modify button3_Click to be async and handle validation
        private async void button3_Click(object sender, EventArgs e)
        {
            List<string> results = await CheckCodeAndDateValidity();
            if (results.Count > 0)
            {
                string key = textBox1.Text;
                bool keyExists = results.Any(result => result.Contains(key));
                if (keyExists)
                {
                    this.Hide();
                    Form1 frm = new Form1(textBox1.Text, true);
                    frm.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Key không đúng hoặc đã hết hạn");
                    Application.Exit();
                }
            }
            else
            {
                MessageBox.Show("Không có kết quả hợp lệ.");
            }
        }

    }
}
