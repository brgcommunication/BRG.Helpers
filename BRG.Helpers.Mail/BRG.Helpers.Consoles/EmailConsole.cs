using BRG.Helpers.Mail;
using System;
using System.Configuration;
using System.Text;

namespace BRG.Helpers.Consoles
{
    /// <summary>
    /// Una StandardConsole che invia il proprio buffer via email.
    /// Convenzioni:
    /// - cerca in appSetting "ExecutionNotificationTo", "WarningNotificationTo" e "ErrorNotificationTo": contengono l'elenco CSV dei destinatari interessati alle notifiche di esecuzione e alle notifiche di errore;
    /// - fallback di ricerca: cerca gli appSetting in ambiente cloud, se non esistenti allora cerca in file app.config o web.config;
    /// - la presenza di un appSetting "BRG.Helpers.Mail.SendAllEmailToDeveloper" forza l'invio di tutte le email all'indirizzo specificato (vedi classe SendGridMailer)
    /// - appSetting in formato CSV: "xxxxxxxxx@brg.it, \"Mario R.\" <xxxxxxxxx@brg.it>, xxxxxxxxx@brgcom.it"
    /// - appSetting in formato CSV su file config: "xxxxxxxxx@brg.it, &quot;Mario R.&quot; &lt;xxxxxxxxx@brg.it&gt;, xxxxxxxxx@brgcom.it"
    /// </summary>
    public class EmailConsole : StandardConsole
    {
        private EmailConsoleConfig config = null;
        private string EXECUTION_NOTIFICATION_TO = string.Empty;
        private string WARNING_NOTIFICATION_TO = string.Empty;
        private string ERROR_NOTIFICATION_TO = string.Empty;

        #region COSTRUTTORI

        /// <summary>
        /// Una StandardConsole che invia via email il proprio buffer.
        /// </summary>
        public EmailConsole(UsageScenarioEnum usageScenario) : base(usageScenario)
        {
            InitSetting();
            config = new EmailConsoleConfig();
            OnInit(EventArgs.Empty);
        }

        /// <summary>
        /// Permette di gestire simultaneamente il log su Console e su sistema custom.
        /// </summary>
        public EmailConsole(UsageScenarioEnum usageScenario, EmailConsoleConfig config,  bool disableSystemConsole = false, bool disableBuffer = false, StringBuilder customBuffer = null) : base(usageScenario, config, disableSystemConsole, disableBuffer, customBuffer)
        {
            InitSetting();
            this.config = config;
        }

        #endregion

        /// <summary>
        /// Invia via email il buffer della console. Usa EmailConsoleConfig per definire le caratteristiche della comunicazione.
        /// </summary>
        /// <param name="errorsFoundDuringExecution">Flag che indica se trattare l'email come una comunicazione di esecuzione avvenuta con successo o come una comunicazione di errore. Nel primo caso verranno notificati i destinatati interessati alle notifiche di esecuzione, nell'altro quelli interessati esclusivamente alle notifiche di errore.</param>
        /// <param name="warningsFoundDuringExecution">Flag che indica che la comunicazione deve essere inviata anche ai destinatari interessati a riceve le comunicazione di "allerta"</param>
        /// <param name="subjectTags">Elenco di tag extra da inserire nell'oggetto della email</param>
        /// <param name="bodyFirstLine">Prima riga del body della email (opzionale). Il contenuto del buffer sarà accodato di seguito. Utile per forzare la preview del messaggio nei client di posta passando dati come ID di oggetti o parametri utili a identificare il contenuto della comunicazione.</param>
        /// <returns></returns>
        public bool SendBufferByEmail(bool errorsFoundDuringExecution, bool warningsFoundDuringExecution = false, string[] subjectTags = null, string bodyFirstLine = null)
        {
            WriteLine(String.Empty.PadRight(80, '-'));

            TryTo("SendBufferByEmail - Check config");
            if (config == null)
            {
                TryKO("Missing configuration! (EmailConsoleConfig)");
                return false;
            }
            TryOK();

            // Classe helper per connessione a servizio SendGrid
            var mailer = new Mailer();

            #region DESTINATARI

            // Destinatari a cui notificare ogni esecuzione da appSetting
            var executionNotificationTo = mailer.FormatAddressList(EXECUTION_NOTIFICATION_TO ?? "");
            // Destinatari a cui notificare ogni esecuzione da EmailConsoleConfig
            executionNotificationTo.AddRange(mailer.FormatAddressList(config.EmailExecutionNotificationTo ?? ""));

            if (errorsFoundDuringExecution)
            {
                // In caso di errore aggiungere anche i destinatari interessati solo agli errori da appSetting
                executionNotificationTo.AddRange(mailer.FormatAddressList(ERROR_NOTIFICATION_TO ?? ""));
                // In caso di errore aggiungere anche i destinatari interessati solo agli errori da EmailConsoleConfig
                executionNotificationTo.AddRange(mailer.FormatAddressList(config.EmailErrorNotificationTo ?? ""));
            }

            if (warningsFoundDuringExecution)
            {
                // In caso di allerta aggiungere anche i destinatari interessati solo agli allerta da appSetting
                executionNotificationTo.AddRange(mailer.FormatAddressList(WARNING_NOTIFICATION_TO ?? ""));
                // In caso di allerta aggiungere anche i destinatari interessati solo agli errori da EmailConsoleConfig
                executionNotificationTo.AddRange(mailer.FormatAddressList(config.EmailWarningNotificationTo ?? ""));
            }

            #endregion

            TryTo("SendBufferByEmail - Load recipients");

            // Se ho almeno un destinatario invia la mail...
            if (executionNotificationTo.Count > 0 && config != null)
            {
                TryOK();

                #region CREA MESSAGGIO E SCRIVI PREVIEW SUL LOG

                WriteLine("SendBufferByEmail - Email notification preview:");

                var from = mailer.FormatAddress(String.IsNullOrEmpty(config.EmailFromName) ? config.JobTitle : config.EmailFromName, config.EmailFromEmail);
                WriteLine("  FROM: " + mailer.DumpAddress(from));
                WriteLine("  TO: " + mailer.DumpAddressList(executionNotificationTo));

                // Esempio formato subject: "[<EMAILERRORTAGS_OR_EMAILNOTIFICATIONTAGS>][<WARNINGTAGS>][<SUBJECTTAGS>] <JobTitle> - <EmailErrorSubject_or_EmailNotificationSubject>"

                var subject = String.Empty;
                if (errorsFoundDuringExecution)
                {
                    subject += config.EmailErrorTags ?? string.Empty;
                    subject += (warningsFoundDuringExecution && config.EmailWarningTags != null) ? config.EmailWarningTags : string.Empty;
                    subject += FormatTags(subjectTags);
                    subject += (String.IsNullOrEmpty(config.JobTitle)) ? string.Empty : $" {config.JobTitle}";
                    subject += (String.IsNullOrEmpty(config.EmailErrorSubject)) ? string.Empty : $" - {config.EmailErrorSubject}";
                }
                else
                {
                    subject += config.EmailNotificationTags ?? string.Empty;
                    subject += (warningsFoundDuringExecution && config.EmailWarningTags != null) ? config.EmailWarningTags : string.Empty;
                    subject += FormatTags(subjectTags);
                    subject += (String.IsNullOrEmpty(config.JobTitle)) ? string.Empty : $" {config.JobTitle}";
                    subject += (String.IsNullOrEmpty(config.EmailNotificationSubject)) ? string.Empty : $" - {config.EmailNotificationSubject}";
                }
                subject = subject.Trim(" -".ToCharArray());
                WriteLine("  SUBJECT: " + subject);

                // Anticipo questi write prima della GetBuffer in modo da inviare tutto via email!
                WriteLine("  CATEGORY: " + config.JobTitle.ToLowerInvariant());
                TryTo("SendBufferByEmail - Sending");
                TryOK();
                WriteLine(String.Empty.PadRight(80, '-'));        
                if (!String.IsNullOrWhiteSpace(bodyFirstLine))
                {
                    WriteLine(bodyFirstLine);
                    WriteLine();
                }

                var body =  GetBuffer();

                var email = mailer.Create(from, executionNotificationTo, null, null, subject, body, config.EmailHtmlFormat, null);

                if (!String.IsNullOrEmpty(config.JobTitle))
                {
                    email.AddCategory(config.JobTitle.ToLowerInvariant());      // Categorizzazione su SendGrid
                }

                #endregion


                bool asyncSupportDisabled = (UsageScenario == UsageScenarioEnum.CONSOLE_APP || UsageScenario == UsageScenarioEnum.WEB_JOB);

                var smtpErrors = mailer.Send(email, asyncSupportDisabled);           // Invio Async ma aspetta!
            
                if (String.IsNullOrEmpty(smtpErrors))
                {
                    return true;
                }
                else
                {
                    WriteLine("SEND ERRORS: " + smtpErrors);
                }
            }
            else
            {
                TryKO("No recipients found!");
            }

            return false;
        }

        private void InitSetting()
        {
            // Destinatari di default: se in ambiente cloud (Azure - vedi Setting della WebApp)
            EXECUTION_NOTIFICATION_TO = Environment.GetEnvironmentVariable("APPSETTING_ExecutionNotificationTo") ?? string.Empty;
            WARNING_NOTIFICATION_TO = Environment.GetEnvironmentVariable("APPSETTING_WarningNotificationTo") ?? string.Empty;
            ERROR_NOTIFICATION_TO = Environment.GetEnvironmentVariable("APPSETTING_ErrorNotificationTo") ?? string.Empty;

            // Se non ho trovato setting in ambiente cloud provo sul config dell'applicazione
            if (String.IsNullOrEmpty(EXECUTION_NOTIFICATION_TO) && String.IsNullOrEmpty(ERROR_NOTIFICATION_TO) && String.IsNullOrEmpty(WARNING_NOTIFICATION_TO))
            {
                EXECUTION_NOTIFICATION_TO = ConfigurationManager.AppSettings["ExecutionNotificationTo"] ?? string.Empty;
                WARNING_NOTIFICATION_TO = ConfigurationManager.AppSettings["WarningNotificationTo"] ?? string.Empty;
                ERROR_NOTIFICATION_TO = ConfigurationManager.AppSettings["ErrorNotificationTo"] ?? string.Empty;
            };
        }

        private string FormatTags(string[] tags)
        {
            if (tags == null)
            {
                return string.Empty;
            }

            var ts = string.Empty;
            foreach (var t in tags)
            {
                if (!String.IsNullOrEmpty(t))
                {
                    ts += $"[{t.ToUpperInvariant()}]";
                }
            }
            return ts;
        }

    }

}
