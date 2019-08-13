using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BRG.Helpers.Mail
{
    public interface ISmtpClient
    {
        /// <summary>
        /// Metodo di invio email tramite il client/servizio SMTP.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="hasToWait"></param>
        /// <returns>String.Empty se ok. Un messaggio di errore altimenti.</returns>
        string Send(EmailMessage email, bool hasToWait = false);
    }
}
