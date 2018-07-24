using System;

namespace BRG.Helpers.Consoles
{
    public class ConsoleMessageEventArgs : EventArgs
    {
        public string Message { get; }
        public bool IsWriteLineMethodInvoked { get; }
        public int IndentationLevel { get; protected internal set; }
        public string IdentationChars { get; protected internal set; }

        public ConsoleMessageEventArgs(string message, bool isWriteLineMethodInvoked = false, int indentationLevel = 0, string indentationChars = "")
        {
            Message = message;
            IsWriteLineMethodInvoked = isWriteLineMethodInvoked;
            IndentationLevel = (indentationLevel > 0) ? indentationLevel : 0;
            IdentationChars = indentationChars ?? String.Empty;
        }
    }
}
