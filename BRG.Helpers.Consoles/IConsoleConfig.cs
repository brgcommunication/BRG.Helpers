using System;

namespace BRG.Helpers.Consoles
{
    public interface IConsoleConfig
    {
        EventedConsoleHandler OnConsoleInit { get; set; }
    }
}
