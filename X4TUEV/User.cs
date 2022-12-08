using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using X4TUEV.FakeEmailClient;

namespace X4TUEV
{
    public record class User(string Username, string Password, EmailAddress Address)
    {
    }
}
