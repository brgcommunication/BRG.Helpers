using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BRG.Helpers.Mail
{
    public class SendGridSmtpClient : ISmtpClient
    {

        public string Send(EmailMessage email, bool hasToWait = false)
        {
            var msg = EmailMessageToSendGridMessage(email);

            //Execute(msg).Wait();

            var task = Execute(msg);

            var timeout = 0;

            while (hasToWait && timeout < 200 && !task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
            {
                timeout++;
                System.Threading.Thread.Sleep(100);
            }

            var r = task.Result;

            // Torna String.Empty se tutto è ok, un messaggio di errore altrimenti
            return (r.StatusCode == System.Net.HttpStatusCode.OK) ? String.Empty : r.StatusCode.ToString();
        }

        private async Task<Response> Execute(SendGridMessage email)
        {
            var providerApiKey = string.Empty;

#if !DEBUG
            // In produzione recupera gli Application Settings dall webapp Azure... 
            providerApiKey = Environment.GetEnvironmentVariable("APPSETTING_SendGridApiKey");
#endif

            if (string.IsNullOrEmpty(providerApiKey))
            {
                providerApiKey = ConfigurationManager.AppSettings["SendGridApiKey"];
            }

            // Crea il servizio SMTP per l'invio mail.
            var client = new SendGridClient(providerApiKey);

            // Invia la mail.
            return await client.SendEmailAsync(email).ConfigureAwait(false);
        }

        /// <summary>
        /// Istanzia un SendGridMessage partendo daòl'EmailMessage passato.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        private SendGridMessage EmailMessageToSendGridMessage(EmailMessage email)
        {
            var from = new SendGrid.Helpers.Mail.EmailAddress(email.From.Email, email.From.Name);
            var tos = email.Tos.Select(a => new SendGrid.Helpers.Mail.EmailAddress(a.Email, a.Name)).ToList();
            var ccs = email.Ccs.Select(a => new SendGrid.Helpers.Mail.EmailAddress(a.Email, a.Name)).ToList();
            var bccs = email.Bccs.Select(a => new SendGrid.Helpers.Mail.EmailAddress(a.Email, a.Name)).ToList();

            var msg = MailHelper.CreateSingleEmailToMultipleRecipients(from, tos, email.Subject, email.PlainTextContent, email.HtmlContent);

            if (email.ReplyTo != null)
            {
                msg.ReplyTo = new SendGrid.Helpers.Mail.EmailAddress() { Email = email.ReplyTo.Email, Name = email.ReplyTo.Name };
            }

            if (ccs != null && ccs.Count > 0)
            {
                msg.AddCcs(ccs);
            }

            if (bccs != null && bccs.Count > 0)
            {
                msg.AddBccs(bccs);
            }

            if (email.Categories.Count > 0)
            {
                msg.AddCategories(email.Categories);
            }

            if (email.CustomArgs.Count > 0)
            {
                msg.AddCustomArgs(email.CustomArgs);
            }

            if (email.Headers.Count > 0)
            {
                msg.AddHeaders(email.Headers);
            }

            if (email.Attachments.Count > 0)
            {
                msg.AddAttachments(email.Attachments.Select(a => new SendGrid.Helpers.Mail.Attachment()
                {
                    Content = a.Content,
                    Disposition = a.Disposition,
                    ContentId = a.ContentId,
                    Filename = a.Filename,
                    Type = a.Type
                }).ToList());
            }

            return msg;
        }

    }
}
