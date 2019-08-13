using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BRG.Helpers.Mail
{

    // Vedi: SendGridMessage
    public class EmailMessage
    {
        // PROPRIETA'

        public List<EmailAddress> Tos { get; private set; }
        public List<EmailAddress> Ccs { get; private set; }
        public List<EmailAddress> Bccs { get; private set; }
        public EmailAddress From { get; set; }
        public EmailAddress ReplyTo { get; set; }
        public string Subject { get; set; }
        public string PlainTextContent { get; set; }
        public string HtmlContent { get; set; }
        //
        // Summary:
        //     Gets or sets a list of category names for this message. Each category name may
        //     not exceed 255 characters. You cannot have more than 10 categories per request.
        public List<string> Categories { get; private set; }
        //
        // Summary:
        //     Gets or sets values that are specific to the entire send that will be carried
        //     along with the email and its activity data. Substitutions will not be made on
        //     custom arguments, so any string that is entered into this parameter will be assumed
        //     to be the custom argument that you would like to be used. This parameter is overridden
        //     by any conflicting personalizations[x].custom_args if that parameter has been
        //     defined. If personalizations[x].custom_args has been defined but does not conflict
        //     with the values defined within this parameter, the two will be merged. The combined
        //     total size of these custom arguments may not exceed 10,000 bytes.
        public Dictionary<string, string> CustomArgs { get; private set; }
        //
        // Summary:
        //     Gets or sets an object containing key/value pairs of header names and the value
        //     to substitute for them. You must ensure these are properly encoded if they contain
        //     unicode characters. Must not be any of the following reserved headers: x-sg-id,
        //     x-sg-eid, received, dkim-signature, Content-Type, Content-Transfer-Encoding,
        //     To, From, Subject, Reply-To, CC, BCC
        public Dictionary<string, string> Headers { get; private set; }

        public List<EmailAttachment> Attachments { get; set; }

        // METODI

        public void AddTo(string email, string name = null)
        {
            Tos.Add(new EmailAddress() { Email = email, Name = name });
        }
        public void AddTo(EmailAddress email)
        {
            Tos.Add(email);
        }
        public void AddTos(List<EmailAddress> emails)
        {
            Tos.AddRange(emails);
        }
        public void AddCc(string email, string name = null)
        {
            Ccs.Add(new EmailAddress() { Email = email, Name = name });
        }
        public void AddCc(EmailAddress email)
        {
            Ccs.Add(email);
        }
        public void AddCcs(List<EmailAddress> emails)
        {
            Ccs.AddRange(emails);
        }
        public void AddBcc(string email, string name = null)
        {
            Bccs.Add(new EmailAddress() { Email = email, Name = name });
        }
        public void AddBcc(EmailAddress email)
        {
            Bccs.Add(email);
        }
        public void AddBccs(List<EmailAddress> emails)
        {
            Bccs.AddRange(emails);
        }
        public void AddCategory(string category)
        {
            Categories.Add(category);
        }
        public void AddCategories(List<string> categories)
        {
            Categories.AddRange(categories);
        }
        public void AddCustomArg(string customArgKey, string customArgValue)
        {
            if (CustomArgs.ContainsKey(customArgKey))
            {
                CustomArgs[customArgKey] = customArgValue;
            }
            else
            {
                CustomArgs.Add(customArgKey, customArgValue);
            }
        }
        public void AddCustomArgs(Dictionary<string, string> customArgs)
        {
            foreach (var c in customArgs)
            {
                AddCustomArg(c.Key, c.Value);
            }
        }
        public void AddHeader(string key, string value)
        {
            if (Headers.ContainsKey(key))
            {
                Headers[key] = value;
            }
            else
            {
                Headers.Add(key, value);
            }
        }
        public void AddHeaders(Dictionary<string, string> headers)
        {
            foreach (var h in headers)
            {
                AddHeader(h.Key, h.Value);
            }
        }
        public void AddAttachment(string filename, string content, string type = null, string disposition = null, string content_id = null)
        {
            Attachments.Add(new EmailAttachment() { Filename = filename, Content = content, Type = type, Disposition = disposition, ContentId = content_id });
        }
        public void AddAttachments(List<EmailAttachment> attachments)
        {
            Attachments.AddRange(attachments);
        }
    }

}
