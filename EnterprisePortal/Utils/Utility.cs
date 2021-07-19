using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Services;

namespace EnterprisePortal.Utils
{
    public class Utility
    {
        public static string UploadImage(HttpPostedFileBase file, string folder)
        {
            string path = "~/Upload/" + folder + "/";
            if (file != null)
            {
                int fileSize = file.ContentLength;
                if (fileSize < 2097152)
                {
                    var allowedExtensions = new[] { ".jpeg", ".jpg", ".png", ".gif" };
                    string ext = Path.GetExtension(file.FileName).ToLower();
                    if (allowedExtensions.Contains(ext))
                    {
                        string renamedFile = DateTime.Now.ToString("yyyyMMdd") + file.FileName;
                        file.SaveAs(HttpContext.Current.Server.MapPath(path) + renamedFile);
                        return path + renamedFile;
                    }
                    else
                    {
                        return "只接受JPEG, JPG, PNG 及 GIF檔";
                    }
                }
                else
                {
                    return "檔案太大,只接受 2MB 以下的圖片";
                }
            }
            Random rnd = new Random();
            int num = rnd.Next(1, 6);
            return path + $"userIcon{num}.jpg";
        }

        public static bool IsValidEmail(string email)
        {
            if (String.IsNullOrEmpty(email))
            {
                return false;
            }
            return Regex.IsMatch(email, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase);
        }

        public static bool IsValidPhoneNumber(string phoneNumber)
        {
            if (String.IsNullOrEmpty(phoneNumber) || phoneNumber.Length != 10)
            {
                return false;
            }
            if (phoneNumber.Substring(0, 2) != "09")
            {
                return false;
            }
            foreach (char chr in phoneNumber)
            {
                if (!Char.IsDigit(chr))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool IsValidCitizenId(string citizenId)
        {
            if (String.IsNullOrEmpty(citizenId) || citizenId.Length != 10)
            {
                return false;
            }
            int county = Encoding.ASCII.GetBytes(citizenId)[0];
            if (county < 65 || county > 90)
            {
                return false;
            }
            int gender = int.Parse(citizenId.Substring(1, 2));
            if (gender != 1 || gender != 2)
            {
                return false;
            }
            string idNum = citizenId.Substring(1);
            foreach (char chr in idNum)
            {
                if (!Char.IsDigit(chr))
                {
                    return false;
                }
            }
            string countyCode = "ABCDEFGHJKLMNPQRSTUVXYWZIO";
            int[] weights = { 1, 9, 8, 7, 6, 5, 4, 3, 2, 1, 1 };
            string convertToNumId = $"{countyCode.IndexOf(citizenId[0]) + 10}{idNum}";
            int checkSum = 0;
            int num, weight;
            for (int i = 0; i < convertToNumId.Length; i++)
            {
                num = int.Parse(convertToNumId[i].ToString());
                weight = weights[i];
                checkSum += num * weight;
            }
            if (checkSum % 10 != 0)
            {
                return false;
            }
            return true;
        }

        public static void SendHtmlFormattedEmail(string recepientEmail, string subject, string body)
        {
            using (MailMessage mailMessage = new MailMessage())
            {
                mailMessage.From = new MailAddress(ConfigurationManager.AppSettings["UserName"], "Tayana", System.Text.Encoding.UTF8);
                mailMessage.BodyEncoding = System.Text.Encoding.UTF8;
                mailMessage.Subject = subject;
                mailMessage.BodyEncoding = System.Text.Encoding.UTF8;
                mailMessage.Body = body;
                mailMessage.IsBodyHtml = true;
                mailMessage.To.Add(new MailAddress(recepientEmail));
                SmtpClient smtp = new SmtpClient();
                smtp.Host = ConfigurationManager.AppSettings["Host"].ToString();
                smtp.EnableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["EnableSsl"]);
                NetworkCredential NetworkCred = new NetworkCredential();
                NetworkCred.UserName = ConfigurationManager.AppSettings["UserName"].ToString();
                NetworkCred.Password = ConfigurationManager.AppSettings["Password"].ToString();
                smtp.UseDefaultCredentials = true;
                smtp.Credentials = NetworkCred;
                smtp.Port = Convert.ToInt16(ConfigurationManager.AppSettings["Port"]);
                smtp.Send(mailMessage);
            }
        }

        public static string PopulateBody(string userName, string content, string link, string description)
        {
            string path = HttpContext.Current.Server.MapPath("~/Utils/Email.html");
            string body = string.Empty;
            using (StreamReader reader = new StreamReader(path))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{UserName}", userName);
            body = body.Replace("{Content}", content);
            body = body.Replace("{Link}", link);
            body = body.Replace("{Description}", description);
            return body;
        }
    }
}