using System;

namespace BRG.Helpers.Consoles
{
    public class EmailConsoleConfig : StandardConsoleConfig
    {
        public string EmailFromName { get; set; } = string.Empty;                     // Default: StandardConsoleConfig.JobTitle
        public string EmailFromEmail { get; set; } = "no-reply@brg.it";
        public string EmailExecutionNotificationTo { get; set; } = "notifiche@brg.it";
        public string EmailWarningNotificationTo { get; set; } = "notifiche@brg.it";
        public string EmailErrorNotificationTo { get; set; } = "notifiche@brg.it";
        public string EmailNotificationSubject { get; set; } = "Successfully completed";
        public string EmailErrorSubject { get; set; } = "Errors found";
        public string EmailNotificationTags { get; set; } = "[OK]";
        public string EmailErrorTags { get; set; } = "[KO]";
        public string EmailWarningTags { get; set; } = "[WARNING]";
        public bool EmailHtmlFormat { get; set; } = false;
    }
}
