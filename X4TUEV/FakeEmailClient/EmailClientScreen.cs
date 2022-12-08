using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Terminal.Gui;

namespace X4TUEV.FakeEmailClient
{
    internal class EmailClientScreen : Window
    {
        readonly List<Email> emails;
        readonly List<EmailAddress> userAddresses;
        readonly List<User> users;

        IEnumerable<Email> emailsFiltered;
        public EmailClientScreen()
        {
            Random rnd = new Random();
            var randomUserCount = rnd.Next(50, 250);
            users = new List<User>(randomUserCount + 1) { TopLevelWindow.user! }; 
            userAddresses = new List<EmailAddress>(randomUserCount + 1) { TopLevelWindow.user!.Address }; // leave our mail in it too

            for(int i = 0; i < randomUserCount; i++)
            {
                var fullName = Faker.Name.FullName();
                var emailAddr = new EmailAddress((rnd.Next(0,2) % 1 == 0)
                    ? Faker.Internet.Email(fullName)
                    : Faker.Internet.FreeEmail());

                var user = new User(Faker.Internet.UserName(),
                    Faker.Internet.FreeEmail().Split('@')[0], //now this is ugly
                    emailAddr);

                userAddresses.Add(emailAddr);
                users.Add(user);
            }

            var randomMailCount = rnd.Next(25, 500);
            emails = new List<Email>(randomMailCount);

            int userCnt = users.Count;
            for (int i = 0; i < randomMailCount; i++)
            {
                int userSender = rnd.Next(0, userCnt);
                int userReceiver = rnd.Next(0 ,userCnt);
                emails.Add(new Email(
                    from: users[userSender].Address,
                    to: users[userReceiver].Address,
                    Faker.Lorem.Paragraph(rnd.Next(3, 20))
                ));
            }

            emailsFiltered = emails;
        }
    }
}
