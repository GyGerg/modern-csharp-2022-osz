using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace X4TUEV.FakeEmailClient
{
    internal static class Store // I'm gonna get killed for this
    {
        public static User? currentUser;
        public static readonly ObservableCollection<User> users = new();
        public static readonly ObservableCollection<EmailAddress> userAddresses = new();
        public static readonly ObservableCollection<Email> emails = new();
        public static ObservableCollection<Email> emailsFiltered = new();
    }
}
