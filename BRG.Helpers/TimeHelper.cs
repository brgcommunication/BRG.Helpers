using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;

namespace BRG.Helpers
{
    public static partial class TimeHelper
    {
        /// <summary>
        /// Ritorna data e ora corrente nel fuso orario "W. Europe Standard Time" (UTC+01:00) Amsterdam, Berlin, Bern, Rome, Stockholm, Vienna.
        /// </summary>
        /// <returns></returns>
        public static DateTime GetDateNow()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
        }
    }
}
