namespace BRG.Helpers.Consoles
{
    public class ConsoleFormatEventArgs : ConsoleMessageEventArgs
    {
        public object[] Args { get; }

        public ConsoleFormatEventArgs(string format, object[] args, bool isWriteLineMethodInvoked = false, int indentationLevel = 0, string indentationChars = "") : base(format, isWriteLineMethodInvoked, indentationLevel, indentationChars)
        {
            Args = args;
        }
    }
}
