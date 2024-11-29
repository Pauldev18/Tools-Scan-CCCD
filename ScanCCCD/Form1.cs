using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ZXing;
using AForge.Imaging.Filters;
namespace ScanCCCD
{
    public partial class Form1 : Form
    {
        private string licensekey { get; set; }
        private bool dungthu { get; set; }
        public Form1(string key, bool a)
        {
            InitializeComponent();
            this.txt_text.KeyPress += new KeyPressEventHandler(this.txt_text_KeyPress);
            licensekey = key;
            dungthu = a;
            // Đặt StartPosition thành Manual
            this.StartPosition = FormStartPosition.Manual;
            // Tính toán vị trí góc dưới bên phải
            int x = Screen.PrimaryScreen.WorkingArea.Width - this.Width;
            int y = Screen.PrimaryScreen.WorkingArea.Height - this.Height;
            // Đặt vị trí của form
            this.Location = new Point(x, y);

        }


        private void ReadQRCode(Bitmap bitmap)
        {
            try
            {
                // Use the 'using' statement to ensure proper disposal of bitmap.
                using (bitmap)
                {
                    // Initialize BarcodeReader with optimized settings
                    var reader = new BarcodeReader
                    {
                        AutoRotate = true,          // Auto-rotate image if needed
                        TryInverted = true,        // Try reading the QR code in inverted colors (white on black)
                        Options = new ZXing.Common.DecodingOptions
                        {
                            TryHarder = true,       // Try harder to decode, may take more time but increases accuracy
                            PossibleFormats = new List<ZXing.BarcodeFormat> { ZXing.BarcodeFormat.QR_CODE }
                        }
                    };

                    // Decode the QR code
                    var result = reader.Decode(bitmap);

                    if (result != null)
                    {
                        // If decoding successful, display result in TextBox
                        txt_text.Invoke(new Action(() => txt_text.Text = result.Text));

                        KeyPressEventArgs e = new KeyPressEventArgs((char)Keys.Enter);
                        txt_text_KeyPress(txt_text, e);
                    }
                    else
                    {
                        // If no QR code found, display warning
                        MessageBox.Show("Không tìm thấy mã QR trong hình ảnh.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        ClearText();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log and display any errors encountered during decoding
                Console.WriteLine("Lỗi khi đọc mã QR: " + ex.Message);
                MessageBox.Show("Lỗi khi xử lý mã QR: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ClearText();
            }
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            ClearText();
            if(dungthu != true)
            {
                if (!Security.VerifyMachineCode(licensekey))
                {
                    MessageBox.Show("License key không hợp lệ!");
                    Application.Exit();
                }
            }
        }

      

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (picBoxWebcam.Image != null)
            {
                picBoxWebcam.Image.Dispose();
            }
            Application.Exit();

        }
        string[] addressParts = { "Thái Bình", "Đông Hưng", "Đông Động" };
        public void ClearText()
        {
            txt_CCCD.Text = "";
            txt_hoVaTen.Text = "";
            txt_ngaySinh.Text = "";
            txt_gioiTinh.Text = "";
            txt_diaChi.Text = "";
            txt_ngayDK.Text = "";
            if (picBoxWebcam.Image != null)
            {
                picBoxWebcam.Image.Dispose();
                picBoxWebcam.Image = null;
            }
        }
        private void ShowMessage(string message)
        {
            // Create and configure the message box form
            var messageBox = new Form
            {
                Text = "Thông báo",
                Size = new System.Drawing.Size(300, 150),
                StartPosition = FormStartPosition.CenterScreen,
                TopMost = true // Make this form topmost
            };

            // Create and configure the label to display the message
            var lblMessage = new Label
            {
                Text = message,
                AutoSize = true,
                Location = new System.Drawing.Point(20, 20)
            };

            // Create and configure the "OK" button
            var btnOk = new Button
            {
                Text = "OK",
                Location = new System.Drawing.Point(110, 60),
                DialogResult = DialogResult.OK
            };

            // Close the message box when the "OK" button is clicked
            btnOk.Click += (sender, e) => messageBox.Close();

            // Add the label and button to the form's controls
            messageBox.Controls.Add(lblMessage);
            messageBox.Controls.Add(btnOk);

            // Set the "OK" button as the default action for the form (when pressing Enter)
            messageBox.AcceptButton = btnOk;

            // Show the form as a modal dialog
            messageBox.ShowDialog();
        }
        private void fillDataForm()
        {
            var window = WindowAutomation.FindWindowByAutomationId("frmThuNhanHS");

            if (window != null)
            {
                try
                {
                    // Disable the form to prevent user interaction during execution
                    this.Invoke(new Action(() =>
                    {
                        txt_text.Enabled = false;
                        button3.Enabled = false;
                    }));

                    // Chạy các tác vụ tuần tự một cách đồng bộ
                    WindowAutomation.ClickButtonByAutomationId(window, "btnNhapMoi");
                    System.Threading.Thread.Sleep(1000);
                    WindowAutomation.TypeTextOnly(window, "txtHoVaTen", txt_hoVaTen.Text);
                    WindowAutomation.TypeTextDate(window, "mtxDateTime", 4, txt_ngaySinh.Text);
                    if (txt_gioiTinh.Text.Equals("Nam"))
                    {
                        WindowAutomation.SelectComboBoxOptionWithCache(window, "cboGioiTinh",2);
                    }else if (txt_gioiTinh.Text.Equals("Nữ"))
                    {
                        WindowAutomation.SelectComboBoxOptionWithCache(window, "cboGioiTinh", 3);
                    }
                    WindowAutomation.TypeTextOnly(window, "txtCMT", txt_CCCD.Text);
                    WindowAutomation.TypeTextDate(window, "mtxDateTime", 3, txt_ngayDK.Text);
                    WindowAutomation.TypeTextAndSelectFirstSuggestion(window, "atxtTT_HC", GetAddressParts(txt_diaChi.Text));

                    if (cbx_thoiHan.Checked)
                    {
                        WindowAutomation.SelectComboBoxOptionWithCache(window, "cboInGplx", 1);
                    }
                    else
                    {
                        WindowAutomation.SelectComboBoxOptionWithCache(window, "cboInGplx", 0);
                    }

                  
                    this.Invoke(new Action(() =>
                    {
                        ShowMessage("Tất cả các tác vụ đã hoàn thành thành công.");
                        txt_text.Enabled = true;
                        button3.Enabled = true;
                        txt_text.Text = string.Empty;
                    }));
                }
                catch (Exception ex)
                {
                    // Xử lý lỗi nếu có
                    this.Invoke(new Action(() =>
                    {
                        ShowMessage($"Có lỗi xảy ra: {ex.Message}");
                        txt_text.Enabled = true;
                        button3.Enabled = true;
                    }));
                }
                finally
                {
                   ClearText();
                }
            }
            else
            {
                
                this.Invoke(new Action(() =>
                {
                    ShowMessage("Không tìm thấy cửa sổ với tiêu đề đã chọn!");
                }));
            }
        }
        public string[] GetAddressParts(string input)
        {
            // Tách địa chỉ theo dấu phẩy
            string[] addressParts = input.Split(new string[] { ", " }, StringSplitOptions.None);

            // Kiểm tra nếu có ít nhất 3 phần trong địa chỉ
            if (addressParts.Length >= 3)
            {
                // Lấy phần đầu: phần sau dấu phẩy cuối cùng
                string firstPart = addressParts[addressParts.Length - 1].Trim();

                // Lấy phần thứ hai: phần sau dấu phẩy trước dấu phẩy cuối cùng
                string secondPart = addressParts[addressParts.Length - 2].Trim();

                // Lấy phần thứ ba: 2 từ trước dấu phẩy trước dấu phẩy cuối cùng
                string[] thirdPartWords = addressParts[addressParts.Length - 3].Trim().Split(' ');

                // Nếu phần thứ ba có ít nhất 2 từ, lấy 2 từ cuối cùng
                string thirdPart = string.Join(" ", thirdPartWords.Skip(Math.Max(0, thirdPartWords.Length - 2)));

                // Trả về kết quả
                return new string[] { firstPart, secondPart, thirdPart };
            }
            else
            {
                // Nếu có ít hơn 3 phần, trả về toàn bộ phần địa chỉ đã tách
                return addressParts;
            }
        }   
        private Bitmap PreprocessImage(Bitmap originalBitmap)
        {
            try
            {
                Grayscale grayscaleFilter = new Grayscale(0.299, 0.587, 0.114);
                Bitmap grayscaleBitmap = grayscaleFilter.Apply(originalBitmap);  // Kiểm tra bước này

                GaussianBlur blurFilter = new GaussianBlur(0, 1);
                Bitmap blurredBitmap = blurFilter.Apply(grayscaleBitmap);  // Kiểm tra bước này

                Threshold thresholdFilter = new Threshold(95);
                Bitmap thresholdedBitmap = thresholdFilter.Apply(blurredBitmap);  // Kiểm tra bước này

                return thresholdedBitmap;
            }
            catch (Exception ex)
            {
                if (picBoxWebcam.Image != null)
                {
                    picBoxWebcam.Image.Dispose();
                }
                MessageBox.Show("Lỗi trong quá trình xử lý ảnh: " + ex.Message);
                return null;
            }
            finally
            {

                if (picBoxWebcam.Image != null)
                {
                    picBoxWebcam.Image.Dispose();
                }
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif"; // Filter image files
            openFileDialog.Title = "Chọn ảnh có mã QR";

            // If the user selects a file and clicks OK
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;

                try
                {
                    if (picBoxWebcam.Image != null)
                    {
                        picBoxWebcam.Image.Dispose();
                    }

                    Bitmap bitmap = new Bitmap(filePath);
                    Bitmap processedBitmap = PreprocessImage(bitmap);
                    picBoxWebcam.Image = processedBitmap;
                    // Call the ReadQRCode function
                    ReadQRCode(processedBitmap);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi mở ảnh: " + ex.Message);
                }
            }
            }

        private void txt_text_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {

                string data = txt_text.Text;

                /*  // Thay thế tất cả các chuỗi chứa nhiều dấu '|' liên tiếp bằng một dấu '|'
                  data = Regex.Replace(data, @"\|+", "|");*/

                // Tách dữ liệu thành mảng bằng ký tự '|'
                string[] fields = data.Split('|');

                // Kiểm tra nếu dữ liệu đủ các trường
                if (fields.Length >= 6)
                {
                    ClearText();
                    txt_CCCD.Text = fields[0];
                    txt_hoVaTen.Text = fields[2];

                    // Xử lý ngày sinh từ ddMMyyyy sang dd/MM/yyyy
                    if (DateTime.TryParseExact(fields[3], "ddMMyyyy", null, System.Globalization.DateTimeStyles.None, out DateTime ngaySinh))
                    {
                        txt_ngaySinh.Text = ngaySinh.ToString("dd/MM/yyyy");
                    }
                    else
                    {
                        txt_ngaySinh.Text = fields[3];
                    }

                    txt_gioiTinh.Text = fields[4];
                    txt_diaChi.Text = fields[5];

                    // Xử lý ngày đăng ký từ yyyyMMdd sang dd/MM/yyyy
                    if (DateTime.TryParseExact(fields[6], "ddMMyyyy", null, System.Globalization.DateTimeStyles.None, out DateTime ngayDK))
                    {
                        txt_ngayDK.Text = ngayDK.ToString("dd/MM/yyyy");
                    }
                    else
                    {
                        txt_ngayDK.Text = fields[6];
                    }
                }
                else
                {
                    ClearText();
                }
                if (!string.IsNullOrWhiteSpace(txt_CCCD.Text))
                {
                    fillDataForm();
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ClearText();
            txt_text.Clear();
        }
    }
}
       
       