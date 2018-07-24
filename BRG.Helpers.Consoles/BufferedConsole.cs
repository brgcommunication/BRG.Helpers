using System;
using System.Text;

namespace BRG.Helpers.Consoles
{
    /// <summary>
    /// Scrive su System.Console e contestualmente in un buffer di supporto che può essere usato per generare log, notifiche email, ecc.
    /// </summary>
    public class BufferedConsole
    {
        private StringBuilder buffer;
        protected bool IsBufferDisabled { get; private set; }
        protected bool IsSystemConsoleDisabled { get; private set; }

        /// <summary>
        /// Scrive su System.Console e contestualmente in un buffer di supporto che può essere usato per generare log, notifiche email, ecc.
        /// </summary>
        public BufferedConsole()
        {
            buffer = new StringBuilder();
        }

        /// <summary>
        /// Scrive su System.Console e contestualmente in un buffer di supporto che può essere usato per generare log, notifiche email, ecc.
        /// </summary>
        /// <param name="disableSystemConsole">Se true, la scrittura su System.Console è disabilitata</param>
        /// <param name="disableBuffer">Se true, la scrittura sul buffer interno è disabiltita</param>
        /// <param name="customBuffer">Se diverso da null, usa questo buffer al posto di quello predefinito</param>
        public BufferedConsole(bool disableSystemConsole = false, bool disableBuffer = false, StringBuilder customBuffer = null)
        {
            IsSystemConsoleDisabled = disableSystemConsole;
            IsBufferDisabled = disableBuffer;
            buffer = customBuffer ?? new StringBuilder();
        }

        #region WRAPPING DEI METODI System.Console.Write E System.Console.WriteLine

        /// <summary>
        /// Scrive la stringa su System.Console e sul buffer interno.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="arg"></param>
        /// <returns>Ritorna la stringa passata. Permette di definire una volta sola il messaggio evitando variabili temporanee.</returns>
        public virtual string Write(string format = "", params object[] arg)
        {
            var value = ApplyFormat(false, format, arg);

            // Scrivi su System.Console
            if (!IsSystemConsoleDisabled)
            {
                Console.Write(value);
            }

            // Registra nel buffer interno
            if (!IsBufferDisabled)
            {
                buffer.Append(value);
            }

            return value;
        }

        /// <summary>
        /// Scrive la stringa su System.Console e sul buffer interno. Ritorna la stringa passata senza NewLine aggiuntivi.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="arg"></param>
        /// <returns>Ritorna la stringa passata senza NewLine aggiuntivi. Permette di definire una volta sola il messaggio evitando variabili temporanee.</returns>
        public virtual string WriteLine(string format = "", params object[] arg)
        {
            var value = ApplyFormat(true, format, arg);

            // Scrivi su System.Console
            if (!IsBufferDisabled)
            {
                Console.Write(value + Environment.NewLine);
            }

            // Registra nel buffer interno
            if (!IsBufferDisabled)
            {
                buffer.Append(value + Environment.NewLine);
            }

            return value;
        }

        /// <summary>
        /// Applica la formattazione del messaggio finale. I parametri supportati sono quelli di String.Format().
        /// </summary>
        /// <param name="IsWriteLineMethodInvoked">Se true, l'esecuzione di questo ApplyFormat è stata invocata da una WriteLine(). Se false, da una Write().</param>
        /// <param name="format"></param>
        /// <param name="arg"></param>
        /// <returns></returns>
        protected virtual string ApplyFormat(bool IsWriteLineMethodInvoked = false, string format = "", params object[] arg)
        {
            return String.Format(format, arg);
        }

        #endregion

        #region GESTIONE BUFFER

        /// <summary>
        /// Ritorna il contenuto attuale del buffer. Utile per generare log, notifiche email, ecc.
        /// </summary>
        /// <returns></returns>
        public string GetBuffer()
        {
            if (!IsBufferDisabled)
            {
                return buffer.ToString();
            }

            return String.Empty;
        }

        /// <summary>
        /// Cancella il contenuto attuale del buffer. Utile per resettare il buffer dopo aver
        /// </summary>
        public void ResetBuffer()
        {
            if (!IsBufferDisabled && buffer != null)
            {
                buffer.Clear();
            }
        }

        #endregion
    }
}
