using MimeKit;
using MailKit.Net.Smtp;
using System.Security.Cryptography;
using System.Text;

namespace EDP_Backend.Helper
{
    public class Helper
    {
        public static Dictionary<string, string> GenerateError(string error)
        {
            return new Dictionary<string, string>
            {
                {"error", error}
            };
        }

        public static void SendMail(string toName, string toEmail, string subject, string body)
        {
            var from = Environment.GetEnvironmentVariable("NET_MAIL_ADDRESS");
            var password = Environment.GetEnvironmentVariable("NET_MAIL_PASSWORD");
            var server = Environment.GetEnvironmentVariable("NET_MAIL_SERVER");
            var port = Convert.ToInt32(Environment.GetEnvironmentVariable("NET_MAIL_PORT"));

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("UPlay No Reply", from));
            email.To.Add(new MailboxAddress(toName, toEmail));

            email.Subject = subject;

            var bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = body;

            email.Body = bodyBuilder.ToMessageBody();

            using (var smtp = new SmtpClient())
            {
                smtp.Connect(server, port, false);
                smtp.Authenticate(from, password);
                smtp.Send(email);
                smtp.Disconnect(true);
            }
        }

        public static string RandomString(int length)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            StringBuilder res = new StringBuilder();
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                byte[] uintBuffer = new byte[sizeof(uint)];

                while (length-- > 0)
                {
                    rng.GetBytes(uintBuffer);
                    uint num = BitConverter.ToUInt32(uintBuffer, 0);
                    res.Append(valid[(int)(num % (uint)valid.Length)]);
                }
            }

            return res.ToString();
        }
    }
}
