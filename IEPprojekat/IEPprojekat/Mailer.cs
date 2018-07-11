using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace IepWebApp
{
    public static class Mailer
    {
        public static void sendMail(string toUser, string subject, string body)
        {
            MailMessage mail = new MailMessage();
            SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
            SmtpServer.Port = 587;
            SmtpServer.Credentials = new System.Net.NetworkCredential("tjovanovic510@gmail.com", "Necudatikazem.1");
            SmtpServer.EnableSsl = true;
            mail.Subject = subject;
            mail.Body = body;
            mail.From = new MailAddress("tjovanovic510@gmail.com");
            mail.To.Add(toUser);
            SmtpServer.Send(mail);
        }
    }
}