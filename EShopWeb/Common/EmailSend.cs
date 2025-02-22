using System.Net;
using System.Net.Mail;

namespace EShopWeb.Common
{
    public class EmailSend
    {
        private readonly static string smtpAddress = "smtp.gmail.com";
        private readonly static int portNumber = 587;
        private readonly static bool enableSSL = true;
        private readonly static string emailFromAddress = "nalinmyid@gmail.com"; //Sender Email Address  
        private readonly static string password = "xxxxxxxxxx"; //Sender Password  

        private readonly static string subject = "Purchase Order";

        public static void SendEmail2(string ToEmailAddrss)
        {    
            try
            {
                using MailMessage mail = new();
                mail.From = new MailAddress(emailFromAddress);
                mail.To.Add(ToEmailAddrss);
                mail.Subject = subject;
                mail.Body = "<html><head><meta charset = \"utf-8\" /><title></title><style type =\"text/css\">.webgrid-table{ margin: 4px;border-collapse: collapse;} .webgrid-header {padding: 0.5em;background-color: #5bc0de;color: #DADADA;}.webgrid-alternating-row {background: #d6e3f2;}.webgrid-table th, .webgrid-table td {border: 1px solid #C0C0C0;padding: 4px 6px 4px 6px;}.webgrid-table th a {color: white;} </style></head><body>";
                mail.Body += "<table>";
                mail.Body += "<tr><td>UnitPrice</td><td>Quantity</td><td>Tax</td><td>LineAmount</td></tr>";                
                    mail.Body += "<tr><td></td></tr>";               
                mail.Body += "<tr class=\"webgrid-alternating-row\"><td colspan =\"3\">Total</td><td class=\"ng-binding\"></td></tr></table>";
                mail.Body += "</body></html>";
                mail.IsBodyHtml = true;

                using SmtpClient smtp = new(smtpAddress, portNumber);
                smtp.Credentials = new NetworkCredential(emailFromAddress, password);
                smtp.EnableSsl = enableSSL;
                smtp.Send(mail);
            }
            catch (System.Exception)
            {

            }
        }
    }   
}