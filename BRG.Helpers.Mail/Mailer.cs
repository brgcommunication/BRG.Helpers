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
    /// Configura e invia una email tramite un servizio SMTP (es: SendGrid).
    /// </summary>
    public class Mailer
    {
        private ISmtpClient client;

        public Mailer()
        {
            client = new SendGridSmtpClient();
        }

        public Mailer(ISmtpClient client)
        {
            this.client = client;
        }

        #region RECIPIENT ADDRESS

        /// <summary>
        /// Istanzia un indirizzo email.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        public EmailAddress FormatAddress(string name, string email)
        {
            return new EmailAddress(email, name);
        }

        /// <summary>
        /// Esegue il parsing di una lista di indirizzi email.
        /// Formato CSV: "xxxxxxxxx@brg.it, \"Mario R.\" <xxxxxxxxx@brg.it>, xxxxxxxxx@brgcom.it"
        /// Formato CSV su file .config: "xxxxxxxxx@brg.it, &quot;Mario R.&quot; &lt;xxxxxxxxx@brg.it&gt;, xxxxxxxxx@brgcom.it"
        /// </summary>
        /// <param name="csv"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Istanzia un indirizzo email. Esegue il parsing di una lista di indirizzi email aggiuntivi.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="email"></param>
        /// <param name="csv"></param>
        /// <returns></returns>
        public List<EmailAddress> FormatAddressList(string name, string email, string csv = null)
        {
            var ea = new List<EmailAddress> {
                new EmailAddress(email, name)
            };

            ea.AddRange(FormatAddressList(csv));

            return ea;
        }

        /// <summary>
        /// Istanzia fino a tre indirizzi email.
        /// </summary>
        /// <param name="name1"></param>
        /// <param name="email1"></param>
        /// <param name="name2"></param>
        /// <param name="email2"></param>
        /// <param name="name3"></param>
        /// <param name="email3"></param>
        /// <returns></returns>
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

        public string DumpAddress(EmailAddress address)
        {
            return DumpAddressList(new List<EmailAddress>() { address });
        }

        public string DumpAddressList(List<EmailAddress> addresses)
        {
            if (addresses == null || addresses.Count == 0)
            {
                return string.Empty;
            }

            return addresses.Select(a => String.Format("\"{0}\" <{1}>", a.Name, a.Email)).Aggregate((e, e1) => e + ", " + e);
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
        public EmailMessage CreateHtml(EmailAddress from, EmailAddress recipient, string subject, string body, List<EmailAttachment> attachments = null)
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
        public EmailMessage CreateHtml(EmailAddress from, List<EmailAddress> recipients, string subject, string body, List<EmailAttachment> attachments = null)
        {
            return Create(from, recipients, null, null, subject, body, true, attachments);
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
        public EmailMessage CreatePlainText(EmailAddress from, EmailAddress recipient, string subject, string body, List<EmailAttachment> attachments = null)
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
        public EmailMessage CreatePlainText(EmailAddress from, List<EmailAddress> recipients, string subject, string body, List<EmailAttachment> attachments = null)
        {
            return Create(from, recipients, null, null, subject, body, false, attachments);
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
        public EmailMessage Create(EmailAddress from, List<EmailAddress> to, List<EmailAddress> cc, List<EmailAddress> bcc, string subject, string body, bool isHTML, List<EmailAttachment> attachments = null)
        {
            // Oggetto
            subject = subject.Replace('\r', ' ').Replace('\n', ' ');

            EmailMessage msg;

            #region SEND_TO_DEVELOPER - Per test: se ho un indirizzo email di sviluppo impostato manda tutte le email a questo indirizzo...

            var SEND_TO_DEVELOPER = string.Empty;

#if !DEBUG
            // In produzione recupera gli Application Settings dall webapp Azure... 
            if (string.IsNullOrWhiteSpace(SEND_TO_DEVELOPER))
            {
                SEND_TO_DEVELOPER = Environment.GetEnvironmentVariable("APPSETTING_BRG.Helpers.Mail.SendAllEmailToDeveloper");
            }

            if (string.IsNullOrWhiteSpace(SEND_TO_DEVELOPER))
            {
                SEND_TO_DEVELOPER = Environment.GetEnvironmentVariable("APPSETTING_AzureEmailSendToDeveloper");          // Legacy
            }
#endif

            if (string.IsNullOrWhiteSpace(SEND_TO_DEVELOPER))
            {
                SEND_TO_DEVELOPER = ConfigurationManager.AppSettings["BRG.Helpers.Mail.SendAllEmailToDeveloper"];
            }

            if (string.IsNullOrWhiteSpace(SEND_TO_DEVELOPER))
            {
                SEND_TO_DEVELOPER = ConfigurationManager.AppSettings["AzureEmailSendToDeveloper"];                         // Legacy
            }

            #endregion

            if (String.IsNullOrWhiteSpace(SEND_TO_DEVELOPER))
            {
                if (to.Count.Equals(1))
                {
                    //msg = MailHelper.CreateSingleEmail(from, to.FirstOrDefault(), subject, !isHTML ? body : body.StripHtmlTags(), isHTML ? body : body.TextToHtml());
                    msg = new EmailMessage()
                    {
                        From = from,
                        Subject = subject,
                        PlainTextContent = !isHTML ? body : body.StripHtmlTags(),
                        HtmlContent = isHTML ? body : body.TextToHtml()
                    };
                    msg.AddTo(to.FirstOrDefault());
                }
                else
                {
                    //msg = MailHelper.CreateSingleEmailToMultipleRecipients(from, to, subject, !isHTML ? body : body.StripHtmlTags(), isHTML ? body : body.TextToHtml());
                    msg = new EmailMessage()
                    {
                        From = from,
                        Subject = subject,
                        PlainTextContent = !isHTML ? body : body.StripHtmlTags(),
                        HtmlContent = isHTML ? body : body.TextToHtml()
                    };
                    msg.AddTos(to);
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
                    //msg = MailHelper.CreateSingleEmail(from, devs.FirstOrDefault(), subject, !isHTML ? body : body.StripHtmlTags(), isHTML ? body : body.TextToHtml());
                    msg = new EmailMessage()
                    {
                        From = from,
                        Subject = subject,
                        PlainTextContent = !isHTML ? body : body.StripHtmlTags(),
                        HtmlContent = isHTML ? body : body.TextToHtml()
                    };
                    msg.AddTo(devs.FirstOrDefault());
                }
                else
                {
                    //msg = MailHelper.CreateSingleEmailToMultipleRecipients(from, devs, subject, !isHTML ? body : body.StripHtmlTags(), isHTML ? body : body.TextToHtml());
                    msg = new EmailMessage()
                    {
                        From = from,
                        Subject = subject,
                        PlainTextContent = !isHTML ? body : body.StripHtmlTags(),
                        HtmlContent = isHTML ? body : body.TextToHtml()
                    };
                    msg.AddTos(devs);
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

        /// <summary>
        /// Invia il messaggio tramite il servizio SMTP.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="hasToWait"></param>
        /// <returns>String.Empty tutto ok, messaggio di errore altrimenti</returns>
        public string Send(EmailMessage email, bool hasToWait = false)
        {
            var SEND_COPY_TO_DEVELOPER = string.Empty;      // TODO: manda una copia del messaggio al developer (è una sorta di "inspect mode" da attivare per debug)

            if (client == null || email == null)
            {
                return "Client or EmailMessage not defined";
            }

            try
            {
                var r = client.Send(email, hasToWait);

                return r;

            } catch (Exception e)
            {
                return "SmtpClient exception: " + TextHelper.FormatException(e);
            }
        }

        #endregion
    }

}
