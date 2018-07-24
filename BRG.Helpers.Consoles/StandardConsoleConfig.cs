using System;

namespace BRG.Helpers.Consoles
{
    public class StandardConsoleConfig
    {
        public string JobSupportGuid { get; set; } = Guid.NewGuid().ToString().ToUpper();
        public string JobTitle { get; set; }
        public string JobDescription { get; set; }
        public string JobSchedulations { get; set; }
        public string JobTags { get; set; }
        public string JobSupportNotes { get; set; }
        public string JobCredits { get; set; }
        public DateTime? JobLocalBeginTime { get; set; } = DateTime.Now;
        public string IndentationChars { get; set; } = String.Empty.PadRight(4, ' ');
        public EventedConsoleHandler OnConsoleInit { get; set; }
        public EventedConsoleHandler OnConsoleDisposing { get; set; }
        public EventedConsoleFormatHandler OnConsoleWriting { get; set; }
        public EventedConsoleMessageHandler OnConsoleWritten { get; set; }
    }
}
