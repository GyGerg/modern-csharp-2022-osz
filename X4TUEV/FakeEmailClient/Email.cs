using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace X4TUEV.FakeEmailClient
{
    public record class Email(EmailAddress from, EmailAddress to, string content)
    {

    }

    public record class EmailAddress(string name, string provider)
    {
        public EmailAddress(string input) : this("","")
        {
            if(input.Count(x => x == '@') != 1)
            {
                throw new ArgumentException($"{input} is not a valid email address!");
            }

            var split = input.Split('@');
            if(split.Length != 2 || split[1].Count(x => x == '.') != 1) { 
                throw new ArgumentException($"{input} is not a valid email address!");
            }
            name = split[0];
            provider = split[1];
        }
        public override string ToString()
        {
            return $"{name}@{provider}";
        }
    }
}
