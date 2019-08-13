using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BRG.Helpers.Mail
{
    // Vedi: SendGrid.Helpers.Mail.EmailAddress
    public class EmailAddress
    {
        public EmailAddress() { }

        public EmailAddress(string email, string name = null)
        {
            Email = email;
            Name = name;
        }

        public string Email { get; set; }

        public string Name { get; set; }
    }
}
