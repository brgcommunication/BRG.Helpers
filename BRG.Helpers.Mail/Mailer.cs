using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace BRG.Helpers.Mail
{
    /// <summary>
    /// Configura e invia una email tramite il servizio SendGrid.
    /// </summary>
    public class Mailer
    {
        #region RECIPIENT ADDRESS

        // "John Smith <john@contoso.com>"
        public EmailAddress FormatAddress(string name, string email)
        {
            return new EmailAddress(email, name);
        }

        // ES: FormatAddressList("xxxxxxxxx@brg.it, \"Mario R.\" <xxxxxxxxx@brg.it>, xxxxxxxxx@brgcom.it")
        //     (su web.config): "xxxxxxxxx@brg.it, &quot;Mario R.&quot; &lt;xxxxxxxxx@brg.it&gt;, xxxxxxxxx@brgcom.it"
        public List<EmailAddress> FormatAddressList(string csv)
        {
            if (String.IsNullOrWhiteSpace(csv))
            {
                return new List<EmailAddress>();
            }

            return csv.Split(',')
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Select(x => { var ma = new System.Net.Mail.MailAddress(x); return new EmailAddress(ma.Address, ma.DisplayName); })
                        .Distinct()
                        .ToList();
        }

        public List<EmailAddress> FormatAddressList(string name, string email, string csv = null)
        {
            var ea = new List<EmailAddress> {
                new EmailAddress(email, name)
            };

            ea.AddRange(FormatAddressList(csv));

            return ea;
        }

        public List<EmailAddress> FormatAddressList(string name1, string email1, string name2 = null, string email2 = null, string name3 = null, string email3 = null)
        {
            var ea = new List<EmailAddress> {
                new EmailAddress(email1, name1 ?? email1)
            };

            if (!string.IsNullOrEmpty(email2))
            {
                ea.Add(new EmailAddress(email2, name2 ?? email2));
            }

            if (!string.IsNullOrEmpty(email3))
            {
                ea.Add(new EmailAddress(email3, name3 ?? email3));
            }

            return ea;
        }

        #endregion

        #region CREATE A NEW MESSAGE

        /// <summary>
        /// Crea una email in formato HTML per un singolo destinatario.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="recipient"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="attachments"></param>
        /// <returns></returns>
        public SendGridMessage CreateHtml(EmailAddress from, EmailAddress recipient, string subject, string body, List<Attachment> attachments = null)
        {
            return Create(from, new List<EmailAddress> { recipient }, null, null, subject, body, true, attachments);
        }

        /// <summary>
        /// Crea una email in formato HTML per uno o più destinatario.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="recipients"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="attachments"></param>
        /// <returns></returns>
        public SendGridMessage CreateHtml(EmailAddress from, List<EmailAddress> recipients, string subject, string body, List<Attachment> attachments = null)
        {
            return Create(from, recipients, null, null, subject, body, true, attachments);
        }

        /// <summary>
        /// Crea una email in formato HTML per una serie di indirizzi email associati alla boutique.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="boutique"></param>
        /// <param name="recipient"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="attachments"></param>
        /// <returns></returns>
        public SendGridMessage CreateHtml(EmailAddress from, string boutique, List<string> recipient, string subject, string body, List<Attachment> attachments = null)
        {
            return Create(from, recipient.Select(q => new EmailAddress(q, boutique)).ToList(), null, null, subject, body, true, attachments);
        }

        /// <summary>
        /// Crea una email PLAIN TEXT per un singolo destinatario.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="recipient"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="attachments"></param>
        /// <returns></returns>
        public SendGridMessage CreatePlainText(EmailAddress from, EmailAddress recipient, string subject, string body, List<Attachment> attachments = null)
        {
            return Create(from, new List<EmailAddress> { recipient }, null, null, subject, body, false, attachments);
        }

        /// <summary>
        /// Crea una email PLAIN TEXT per uno o più destinatari.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="recipients"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="attachments"></param>
        /// <returns></returns>
        public SendGridMessage CreatePlainText(EmailAddress from, List<EmailAddress> recipients, string subject, string body, List<Attachment> attachments = null)
        {
            return Create(from, recipients, null, null, subject, body, false, attachments);
        }

        /// <summary>
        /// Crea una email PLAIN TEXT per una serie di indirizzi email associati alla boutique.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="boutique"></param>
        /// <param name="recipient"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="attachments"></param>
        /// <returns></returns>
        public SendGridMessage CreatePlainText(EmailAddress from, string boutique, List<string> recipient, string subject, string body, List<Attachment> attachments = null)
        {
            return Create(from, recipient.Select(q => new EmailAddress(q, boutique)).ToList(), null, null, subject, body, false, attachments);
        }

        /// <summary>
        /// Crea una email per uno o più destinatari.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="isHTML"></param>
        /// <param name="attachments"></param>
        /// <returns></returns>
        public SendGridMessage Create(EmailAddress from, List<EmailAddress> to, List<EmailAddress> cc, List<EmailAddress> bcc, string subject, string body, bool isHTML, List<Attachment> attachments = null)
        {
            // Oggetto
            subject = subject.Replace('\r', ' ').Replace('\n', ' ');

            var msg = new SendGridMessage();

            #region SEND_TO_DEVELOPER - Per test: se ho un indirizzo email di sviluppo impostato manda tutte le email a questo indirizzo...

            var SEND_TO_DEVELOPER = string.Empty;

#if !DEBUG
            // In produzione recupera gli Application Settings dall webapp Azure... 
            SEND_TO_DEVELOPER = Environment.GetEnvironmentVariable("APPSETTING_BRG.Helpers.Mail.SendAllEmailToDeveloper");

            if (string.IsNullOrEmpty(SEND_TO_DEVELOPER))
            {
                SEND_TO_DEVELOPER = Environment.GetEnvironmentVariable("APPSETTING_AzureEmailSendToDeveloper");          // Legacy
            }
#endif

            if (string.IsNullOrEmpty(SEND_TO_DEVELOPER))
            {
                SEND_TO_DEVELOPER = ConfigurationManager.AppSettings["BRG.Helpers.Mail.SendAllEmailToDeveloper"];
            }

            if (string.IsNullOrEmpty(SEND_TO_DEVELOPER))
            {
                SEND_TO_DEVELOPER = ConfigurationManager.AppSettings["AzureEmailSendToDeveloper"];                         // Legacy
            }

            #endregion

            if (String.IsNullOrEmpty(SEND_TO_DEVELOPER))
            {
                if (to.Count.Equals(1))
                {
                    msg = MailHelper.CreateSingleEmail(from, to.FirstOrDefault(), subject, !isHTML ? body : body.StripHtmlTags(), isHTML ? body : body.TextToHtml());
                }
                else
                {
                    msg = MailHelper.CreateSingleEmailToMultipleRecipients(from, to, subject, !isHTML ? body : body.StripHtmlTags(), isHTML ? body : body.TextToHtml());
                }

                if (cc != null && cc.Count > 0)
                {
                    msg.AddCcs(cc);
                }

                if (bcc != null && bcc.Count > 0)
                {
                    msg.AddBccs(bcc);
                }
            }
            else
            {
                // TEST MODE: manda tutto agli indirizzi specificati nel .config
                var devs = FormatAddressList(SEND_TO_DEVELOPER);
                if (devs.Count.Equals(1))
                {
                    msg = MailHelper.CreateSingleEmail(from, devs.FirstOrDefault(), subject, !isHTML ? body : body.StripHtmlTags(), isHTML ? body : body.TextToHtml());
                }
                else
                {
                    msg = MailHelper.CreateSingleEmailToMultipleRecipients(from, devs, subject, !isHTML ? body : body.StripHtmlTags(), isHTML ? body : body.TextToHtml());
                }
            }

            if (attachments != null)
            {
                msg.AddAttachments(attachments);
            }

            return msg;
        }

        #endregion

        #region SEND A MESSAGE

        public void Send(SendGridMessage email, bool haveToAwait = false)
        {
            var task = Execute(email);

            var timeout = 0;

            while (haveToAwait && timeout < 200 && !task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
            {
                timeout++;
                System.Threading.Thread.Sleep(100);
            }
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

        #endregion

        #region MISC

        public string FormatException(Exception e)
        {
            return Text.FormatException(e);
        }

        #endregion
    }

}
