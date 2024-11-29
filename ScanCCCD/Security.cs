using MimeKit;
using MailKit.Net.Smtp;
using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Net.NetworkInformation;
using System.Net.Mail;

namespace ScanCCCD
{
    public class Security
    {
        // 1. Lấy mã máy duy nhất (ví dụ: lấy địa chỉ MAC)
        public static string GetMachineCode()
        {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus == OperationalStatus.Up)
                {
                    // Lấy địa chỉ MAC và chuyển thành chuỗi
                    return BitConverter.ToString(ni.GetPhysicalAddress().GetAddressBytes()).Replace("-", "");
                }
            }
            return string.Empty; // Nếu không tìm thấy, trả về chuỗi trống
        }

        // 2. Mã hóa mã máy bằng AES
        private static readonly string key = "1234567890123456";  // 16-byte key (128 bits)
        private static readonly string iv = "1234567890123456";   // 16-byte IV (128 bits)

        public static string EncryptMachineCode(string machineCode)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(key);
                aesAlg.IV = Encoding.UTF8.GetBytes(iv);

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (var msEncrypt = new System.IO.MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new System.IO.StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(machineCode);
                        }
                    }
                    return Convert.ToBase64String(msEncrypt.ToArray());  // Trả về chuỗi mã hóa
                }
            }
        }

        // 3. Giải mã mã máy bằng AES
        public static string DecryptMachineCode(string encryptedMachineCode)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(key);
                aesAlg.IV = Encoding.UTF8.GetBytes(iv);

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (var msDecrypt = new System.IO.MemoryStream(Convert.FromBase64String(encryptedMachineCode)))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new System.IO.StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();  // Trả về chuỗi giải mã
                        }
                    }
                }
            }
        }

        // 4. Gửi mã máy qua email sử dụng MimeKit và MailKit
        public static void SendMachineCodeByEmail(string encryptedMachineCode)
        {
            try
            {
                // Tạo email
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Your App", "your-email@example.com"));
                message.To.Add(new MailboxAddress("", "vuquanganh1802@gmail.com"));
                message.Subject = "License key";

                // Nội dung email
                message.Body = new TextPart("plain")
                {
                    Text = $"License key: {encryptedMachineCode}"
                };

                // Cấu hình SMTP
                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    client.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                    client.Authenticate("anh0180666@huce.edu.vn", "Anh180228@");

                    // Gửi email
                    client.Send(message);
                    client.Disconnect(true);

                    Console.WriteLine("Mã máy đã được gửi qua email.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi gửi email: {ex.Message}");
            }
        }

        // 5. Kiểm tra mã máy khi đăng nhập
        public static bool VerifyMachineCode(string storedEncryptedMachineCode)
        {
            // Mã hóa mã máy nhập vào
            string encryptedEnteredCode = DecryptMachineCode(storedEncryptedMachineCode);

            // So sánh mã máy đã mã hóa với mã máy lưu trữ
            if (encryptedEnteredCode == GetMachineCode())
            {
                Console.WriteLine("Mã máy hợp lệ.");
                return true;
            }
            else
            {
                Console.WriteLine("Mã máy không hợp lệ.");
                return false;
            }
        }

        // Phương thức gửi mã máy khi người dùng đăng ký (được gọi từ bên ngoài)
        public static void SendMachineCodeToUser()
        {
            string machineCode = GetMachineCode();  // Lấy mã máy
            string encryptedCode = EncryptMachineCode(machineCode);  // Mã hóa mã máy
            SendMachineCodeByEmail(encryptedCode);  // Gửi qua email
        }

      
    }
}
