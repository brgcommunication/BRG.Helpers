using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;

namespace BRG.Helpers
{
    public static partial class TextHelper
    {
        #region STRING EXTENSION METHODS

        public static string TextToHtml(this string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return string.Empty;
            }

            source = source.Trim();
            source = source.Replace("  ", "&nbsp;&nbsp;");
            source = source.Replace("\n", "<br />");
            source = source.Replace("\r", string.Empty);

            return source;
        }

        public static string StripHtmlTags(this string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return string.Empty;
            }

            var rgxEliminaHTML = new Regex("<[^>]*>", RegexOptions.IgnoreCase | RegexOptions.Multiline);

            return rgxEliminaHTML.Replace(source, string.Empty);
        }

        #endregion

        #region FORMATTAZIONE ECCEZIONI (CON GESTIONE DEL CAMPO Data PER IL PASSAGGIO DI VALORI USER-DEFINED)

        // Extension
        public static string DumpException(this Exception e)
        {
            return FormatException(e);
        }

        public static string FormatException(Exception e)
        {
            var sb = new StringBuilder();
            var customData = String.Empty;

            //// Exception particolari che non richiedono il reference di nuove DLL/PACCHETTI NUGET (il pacchetto BRG.Helpers deve restare leggero non posso ad esempio gestire qui le eccezioni di EntityFramework)
            //var ae = e as AggregateException;
            //if (ae!= null)
            //{
            //    //TODO: CICLA SU ae.InnerExceptions
            //}

            if (e != null)
            {
                // livello 1
                sb.AppendLine();
                sb.AppendLine("------ EXCEPTION NAME ------");
                sb.AppendLine(e.GetType().Name ?? "(null)");
                sb.AppendLine("------ EXCEPTION MESSAGE ------");
                sb.AppendLine(e.Message ?? "(null)");
                sb.AppendLine("------ EXCEPTION STACKTRACE ------");
                sb.AppendLine(e.StackTrace ?? "(null)");
                customData = FormatExceptionData(e);
                if (!String.IsNullOrEmpty(customData))
                {
                    sb.AppendLine("------ EXCEPTION CUSTOM DATA ------");
                    sb.AppendLine(customData);
                }
                sb.AppendLine();
            }

            if (e != null && e.InnerException != null)
            {
                // livello 2
                sb.AppendLine();
                sb.AppendLine("------ INNER EXCEPTION NAME ------");
                sb.AppendLine(e.InnerException.GetType().Name ?? "(null)");
                sb.AppendLine("------ INNER EXCEPTION MESSAGE ------");
                sb.AppendLine(e.InnerException.Message ?? "(null)");
                sb.AppendLine("------ INNER EXCEPTION STACKTRACE ------");
                sb.AppendLine(e.InnerException.StackTrace ?? "(null)");
                customData = FormatExceptionData(e.InnerException);
                if (!String.IsNullOrEmpty(customData))
                {
                    sb.AppendLine("------ INNER EXCEPTION CUSTOM DATA ------");
                    sb.AppendLine(customData);
                }
                sb.AppendLine();
            }

            if (e != null && e.InnerException != null && e.InnerException.InnerException != null)
            {
                // livello 3
                sb.AppendLine();
                sb.AppendLine("------ INNER-INNER EXCEPTION NAME ------");
                sb.AppendLine(e.InnerException.InnerException.GetType().Name ?? "(null)");
                sb.AppendLine("------ INNER-INNER EXCEPTION MESSAGE ------");
                sb.AppendLine(e.InnerException.InnerException.Message ?? "(null)");
                sb.AppendLine("------ INNER-INNER EXCEPTION STACKTRACE ------");
                sb.AppendLine(e.InnerException.InnerException.StackTrace ?? "(null)");
                customData = FormatExceptionData(e.InnerException.InnerException);
                if (!String.IsNullOrEmpty(customData))
                {
                    sb.AppendLine("------ INNER-INNER EXCEPTION CUSTOM DATA ------");
                    sb.AppendLine(customData);
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private static string FormatExceptionData(Exception e)
        {
            var sb = new StringBuilder();

            if (e == null || e.Data == null || e.Data.Count == 0)
            {
                return null;
            }

            foreach (DictionaryEntry p in e.Data)
            {
                sb.Append("[");
                sb.Append((p.Key != null) ? p.Key.ToString() : "");
                sb.Append("] = ");
                sb.AppendLine((p.Value != null) ? p.Value.ToString() : "");
            }

            return sb.ToString();
        }

        #endregion
    }
}
