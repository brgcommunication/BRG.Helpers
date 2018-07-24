using System;
using System.Text;

namespace BRG.Helpers.Consoles
{
    /// <summary>
    /// Una EventedConsole personalizzata per realizzare una formattazione consistente dei log generati dai processi schedulati di BRG.
    /// Il log prodotto rispetta il formato di markup di MarkDown (https://en.wikipedia.org/wiki/Markdown).
    /// </summary>
    public class StandardConsole : EventedConsole
    {
        private DateTime consoleCreated = DateTime.Now;
        private StandardConsoleConfig config = null;
        private bool lastCharWasNewLine = true;
        private bool isPreBlockOpen = false;

        private string indentPrefix = String.Empty;
        private int indentLevel = 0;

        private bool postponedInit = true;        // Trick: salta l'esecuzione dell'evento base.OnInit() sul costruttore e poi lo rilancio manualmente. Devo prima impostare BRGConsoleConfig e i suoi handler!

        #region COSTRUTTORI

        /// <summary>
        /// Una EventedConsole personalizzata per realizzare una formattazione consistente dei log generati dai processi schedulati di BRG.
        /// </summary>
        public StandardConsole() : base()
        {
            config = new StandardConsoleConfig();
            OnInit(EventArgs.Empty);
        }

        /// <summary>
        /// Permette di gestire simultaneamente il log su Console e su sistema custom.
        /// </summary>
        public StandardConsole(StandardConsoleConfig config, bool disableSystemConsole = false, bool disableBuffer = false, StringBuilder customBuffer = null) : base(disableSystemConsole, disableBuffer, customBuffer)
        {
            this.config = config;
        }

        #endregion

        #region SETUP DEGLI HANDLER

        protected override void OnInit(EventArgs args)
        {
            if (!postponedInit)
            {
                if (config.OnConsoleInit != null)
                {
                    Init += config.OnConsoleInit;
                }

                base.OnInit(args);
            }

            postponedInit = false;
        }

        protected override void OnDisposing(EventArgs args)
        {
            if (config.OnConsoleDisposing != null)
            {
                Disposing += config.OnConsoleDisposing;
            }

            base.OnDisposing(args);
        }

        protected override void OnWriting(ConsoleFormatEventArgs args)
        {
            if (config.OnConsoleWriting != null)
            {
                Writing += config.OnConsoleWriting;
            }

            args.IndentationLevel = indentLevel;
            args.IdentationChars = config.IndentationChars ?? String.Empty;

            base.OnWriting(args);
        }

        protected override void OnWritten(ConsoleMessageEventArgs args)
        {
            if (config.OnConsoleWritten != null)
            {
                Written += config.OnConsoleWritten;
            }

            args.IndentationLevel = indentLevel;
            args.IdentationChars = config.IndentationChars ?? String.Empty;

            base.OnWritten(args);
        }

        #endregion

        #region METODI Write/WriteLine CON SUPPORTO ALLE INDENTAZIONI

        /// <summary>
        /// Scrive la stringa su System.Console e sui sistemi configurati.
        /// <param name="format"></param>
        /// <param name="arg"></param>
        /// <returns>Ritorna la stringa passata. Permette di definire una volta sola il messaggio evitando variabili temporanee.</returns>
        public override string Write(string format = "", params object[] arg)
        {
            var indentedFormat = ApplyIndentationToFormat(format, false);

            return base.Write(indentedFormat, arg);
        }

        /// <summary>
        /// Scrive la stringa su System.Console e sui sistemi configurati. Ritorna la stringa passata senza NewLine aggiuntivi.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="arg"></param>
        /// <returns>Ritorna la stringa passata senza NewLine aggiuntivi. Permette di definire una volta sola il messaggio evitando variabili temporanee.</returns>
        public override string WriteLine(string format = "", params object[] arg)
        {
            var indentedFormat = ApplyIndentationToFormat(format, true);

            return base.WriteLine(indentedFormat, arg);
        }

        private string ApplyIndentationToFormat(string format = "", bool doingWriteLine = false)
        {
            var sb = new StringBuilder();
            sb.Append((lastCharWasNewLine) ? indentPrefix : String.Empty);
            sb.Append(format);

            // Memorizza se sto eseguendo una WriteLine oppure se la stringa finisce con un NewLine
            lastCharWasNewLine = (doingWriteLine || format.EndsWith(Environment.NewLine));

            // Uniforma le andate a capo e fai un replace con il NewLine dell'ambiente
            sb = sb.Replace("\r\n", "\n");
            sb = sb.Replace('\r', '\n');
            sb = sb.Replace("\n", Environment.NewLine + indentPrefix);

            return sb.ToString();
        }

        /// <summary>
        /// Aggiungi un livello d'indentazione.
        /// </summary>
        public void Indent()
        {
            indentPrefix += config.IndentationChars;
            indentLevel++;
        }

        /// <summary>
        /// Rimuovi un livello d'indentazione.
        /// </summary>
        public void Unindent()
        {
            indentPrefix = indentPrefix.Remove(0, Math.Min(config.IndentationChars.Length, indentPrefix.Length));

            if (indentLevel > 0)
            {
                indentLevel--;
            }
        }

        #endregion

        #region HELPER PER FORMATTAZIONE

        public string TryTo(string format = "", params object[] arg)
        {
            return Write(format + "... ", arg);
        }

        public string TryOK(string format = "", params object[] arg)
        {
            return WriteLine(("[OK] " + format).TrimEnd(), arg);
        }

        public string TryKO(string format = "", params object[] arg)
        {
            return WriteLine(("[KO] " + format).TrimEnd(), arg);
        }

        public string BlockBegin(string format = "", params object[] arg)
        {
            var value = WriteLine(("[BEGIN] " + format).TrimEnd(" :".ToCharArray()) + ":", arg);

            Indent();

            return value;
        }

        public string BlockEnd()
        {
            Unindent();

            return WriteLine("[END]");
        }

        /// <summary>
        /// Se chiuso, apre un blocco preformatted.
        /// </summary>
        /// <param name="lastCharWasNewLine"></param>
        /// <returns></returns>
        private string OpenPreBlock(bool lastCharWasNewLine)
        {
            var enterPreBlock = String.Empty;

            if (isPreBlockOpen)
            {
                return enterPreBlock;
            }

            enterPreBlock += (!lastCharWasNewLine) ? Environment.NewLine : String.Empty;
            enterPreBlock += "```" + Environment.NewLine;

            // Blocco preformatted aperto..
            isPreBlockOpen = true;

            return enterPreBlock;
        }

        /// <summary>
        /// Se aperto, chiude un blocco preformatted.
        /// </summary>
        /// <param name="lastCharWasNewLine"></param>
        /// <returns></returns>
        private string ClosePreBlock(bool lastCharWasNewLine)
        {
            var exitPreBlock = String.Empty;

            if (!isPreBlockOpen)
            {
                return exitPreBlock;
            }

            exitPreBlock += (!lastCharWasNewLine) ? Environment.NewLine : String.Empty;
            exitPreBlock += "```" + Environment.NewLine;

            // Blocco preformatted aperto..
            isPreBlockOpen = false;

            return exitPreBlock;
        }

        /// <summary>
        /// Chiude se necessario il blocco preformattato aperto, apre una sezione inserendo un titolo di livello pari al livello di intendazione attuale, apre un nuovo blocco preformattato.  
        /// </summary>
        /// <param name="format"></param>
        /// <param name="arg"></param>
        /// <returns></returns>
        public string WriteSection(string format = "", params object[] arg)
        {
            // I tag Heading (H1-H6) in MarkDown sono definiti con una sequenza di '#' ma non possono essere indentati.
            // Le indentazioni in MarkDown definiscono dei blocchi di testo preformatted (vengono tradottoti come tag <pre>).
            // Se apro un Heading in un blocco preformattato devo chiuderlo, inserire il titolo e aprirne uno nuovo.

            // Richiamo direttamente il metodo base per non aggiungere le indentazioni
            var value = base.Write(
              ClosePreBlock(lastCharWasNewLine)
              + Environment.NewLine
              + "".PadRight(Math.Min(indentLevel + 1, 6), '#') + (" " + format).TrimEnd() + Environment.NewLine
              + OpenPreBlock(true)
              , arg);

            // Memorizza se sto eseguendo una WriteLine oppure se la stringa finisce con un NewLine
            // Imposto manualmente perché sto richiamando il metodo base.WriteLine senza passare da ApplyIndentationToFormat
            lastCharWasNewLine = true;

            return value;
        }

        /// <summary>
        /// Scrivi intestazione del log.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="arg"></param>
        /// <returns></returns>
        public string WriteHeader()
        {
            // Reset indentazione
            indentPrefix = String.Empty;
            indentLevel = 0;

            var sb = new StringBuilder();

            sb.AppendLine(String.Format("[SUPPORT-GUID: {0}]{1}",
                  config.JobSupportGuid,
                  config.JobTags ?? ""
                  ).ToUpper());

            sb.AppendLine(String.Format("[EXECUTION STARTED AT {0} UTC+00:00]\r\n[EXECUTION STARTED AT {1} (SERVER-TIME)]",
                    config.JobLocalBeginTime.Value.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"),
                    config.JobLocalBeginTime.Value.ToString("yyyy-MM-dd HH:mm:ss UTCK")
                  ).ToUpper());

            if (config.JobTitle != null)
            {
                sb.AppendLine();
                sb.AppendLine("# " + config.JobTitle);
                sb.AppendLine(String.Empty.PadRight(80, '-'));
                sb.Append("- Description: ");
                sb.AppendLine(config.JobDescription);
                sb.Append("- Schedulations: ");
                sb.AppendLine(config.JobSchedulations);
                sb.Append("- Support Notes: ");
                sb.AppendLine(config.JobSupportNotes);
                sb.Append("- Credits: ");
                sb.AppendLine(config.JobCredits);
            }

            sb.AppendLine(String.Empty.PadRight(80, '-'));
            //sb.Append(OpenPreBlock(lastCharWasNewLine));
            indentLevel++;

            return Write(sb.ToString());
        }

        /// <summary>
        /// Scrivi chiusura del log.
        /// </summary>
        /// <param name="jobLocalStartTime"></param>
        /// <param name="jobLocalEndTime"></param>
        /// <returns></returns>
        public string WriteFooter(DateTime? jobLocalStartTime = null, DateTime? jobLocalEndTime = null)
        {
            var sb = new StringBuilder();
            if (isPreBlockOpen)
            {
                // Reset indentazione
                indentPrefix = String.Empty;
                indentLevel = 0;

                sb.Append(ClosePreBlock(lastCharWasNewLine));
            }

            var jlst = jobLocalStartTime ?? consoleCreated;
            var jlet = jobLocalEndTime ?? DateTime.Now;

            sb.AppendLine(String.Empty.PadRight(80, '-'));
            sb.AppendLine(String.Format("[EXECUTION FINISHED AT {0} UTC+00:00]\r\n[EXECUTION FINISHED AT {1} (SERVER-TIME)]\r\n[EXECUTION TIME {2}]",
                jlet.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"),
                jlet.ToString("yyyy-MM-dd HH:mm:ss UTCK"),
                (jlet - jlst).ToString("h'h:'m'm:'s's'")
              ).ToUpper());

            return WriteLine(sb.ToString());
        }

        #endregion

    }
}
