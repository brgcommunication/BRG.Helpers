using System;
using System.Text;

namespace BRG.Helpers.Consoles
{
    public delegate void EventedConsoleHandler(object sender, EventArgs args);

    public delegate void EventedConsoleFormatHandler(object sender, ConsoleFormatEventArgs args);

    public delegate void EventedConsoleMessageHandler(object sender, ConsoleMessageEventArgs args);

    public class EventedConsole : BufferedConsole, IDisposable
    {
        #region DEFINIZIONE DEGLI EVENTI E METODI DI RISING

        public event EventedConsoleHandler Init;
        public event EventedConsoleHandler Disposing;
        public event EventedConsoleFormatHandler Writing;
        public event EventedConsoleMessageHandler Written;

        protected virtual void OnInit(EventArgs args)
        {
            var handler = Init;  // Evita race-condition: https://www.codeproject.com/Articles/20550/C-Event-Implementation-Fundamentals-Best-Practices#7.EventRaisingCode19
            if (handler != null)
            {
                var eventHandlers = handler.GetInvocationList();
                foreach (var currentHandler in eventHandlers)
                {
                    var currentSubscriber = (EventedConsoleHandler)currentHandler;
                    try
                    {
                        currentSubscriber(this, args);
                    }
                    catch { } // Esplicito il loop per evitate che un subscriber in eccezione blocchi la propogazione dell'evento per gli altri
                }
            }
        }

        protected virtual void OnDisposing(EventArgs args)
        {
            var handler = Disposing;
            if (handler != null)
            {
                var eventHandlers = handler.GetInvocationList();
                foreach (Delegate currentHandler in eventHandlers)
                {
                    var currentSubscriber = (EventedConsoleHandler)currentHandler;
                    try
                    {
                        currentSubscriber(this, args);
                    }
                    catch { } // Esplicito il loop per evitate che un subscriber in eccezione blocchi la propogazione dell'evento per gli altri
                }
            }
        }

        protected virtual void OnWriting(ConsoleFormatEventArgs args)
        {
            var handler = Writing;
            if (handler != null)
            {
                var eventHandlers = handler.GetInvocationList();
                foreach (var currentHandler in eventHandlers)
                {
                    var currentSubscriber = (EventedConsoleFormatHandler)currentHandler;
                    try
                    {
                        currentSubscriber(this, args);
                    }
                    catch { } // Esplicito il loop per evitate che un subscriber in eccezione blocchi la propogazione dell'evento per gli altri
                }
            }
        }

        protected virtual void OnWritten(ConsoleMessageEventArgs args)
        {
            var handler = Written;
            if (handler != null)
            {
                var eventHandlers = handler.GetInvocationList();
                foreach (Delegate currentHandler in eventHandlers)
                {
                    var currentSubscriber = (EventedConsoleMessageHandler)currentHandler;
                    try
                    {
                        currentSubscriber(this, args);
                    }
                    catch { } // Esplicito il loop per evitate che un subscriber in eccezione blocchi la propogazione dell'evento per gli altri
                }
            }
        }

        #endregion

        public EventedConsole() : base()
        {
            OnInit(EventArgs.Empty);
        }

        public EventedConsole(bool disableSystemConsole = false, bool disableBuffer = false, StringBuilder customBuffer = null) : base(disableSystemConsole, disableBuffer, customBuffer)
        {
            OnInit(EventArgs.Empty);
        }

        /// <summary>
        /// Scrive la stringa su System.Console e sui sistemi configurati. 
        /// </summary>
        /// <param name="format"></param>
        /// <param name="arg"></param>
        /// <returns>Ritorna la stringa passata. Permette di definire una volta sola il messaggio evitando variabili temporanee.</returns>
        public override string Write(string format = "", params object[] arg)
        {
            OnWriting(new ConsoleFormatEventArgs(format, arg, false));
            var value = base.Write(format, arg);
            OnWritten(new ConsoleFormatEventArgs(value, arg, false));

            return value;
        }

        /// <summary>
        /// Scrive la stringa su System.Console e sui sistemi configurati. Ritorna la stringa passata senza NewLine aggiuntivi.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="arg"></param>
        /// <returns>Ritorna la stringa passata senza NewLine aggiuntivi. Permette di definire una volta sola il messaggio evitando variabili temporanee.</returns>
        public override string WriteLine(string format = "", params object[] arg)
        {
            OnWriting(new ConsoleFormatEventArgs(format, arg, true));
            var value = base.WriteLine(format, arg);
            OnWritten(new ConsoleMessageEventArgs(value, true));

            return value;
        }

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    OnDisposing(EventArgs.Empty);
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion
    }
}
