using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Terminal.Gui;

namespace X4TUEV.FakeEmailClient
{
    public class EmailClientScreen : Window
    {

        readonly EmailListView emailListView;

        IEnumerable<Email> emailsFiltered;

        NStack.ustring _queryString = "";
        NStack.ustring queryString 
        { 
            get => _queryString; 
            set 
            {
                _queryString = value;
                var asStr = value.ToString() ?? "";
                Store.emailsFiltered.Clear();
                var res = GenerateFilter();
                //var res = Store.emails.Where(email => email.From.ToString().Contains(asStr, StringComparison.OrdinalIgnoreCase)
                //|| email.To.ToString().Contains(asStr, StringComparison.OrdinalIgnoreCase)
                //|| email.Title.Contains(asStr, StringComparison.InvariantCultureIgnoreCase)
                //|| email.Content.Contains(asStr, StringComparison.OrdinalIgnoreCase)
                //);
                foreach(var email in res)
                {
                    Store.emailsFiltered.Add(email);
                }
                //countLabel.Text = $"Count: {Store.emailsFiltered.Count}";
                //countLabel.Text = asStr;
                countLabel.Text = Store.emails.First().From.ToString();
            } 
        }
        Label countLabel;
        public EmailClientScreen()
        {
            Random rnd = new();
            var randomUserCount = rnd.Next(50, 250);
            if(Store.currentUser is null)
            {
                throw new NullReferenceException("Current user is null, how did you even get to this screen?");
            }
            Store.users.Add(Store.currentUser);
            Store.userAddresses.Add(Store.currentUser.Address);

            GenerateUsers(randomUserCount, rnd);

            var randomMailCount = rnd.Next(25, 500);

            GenerateMails(randomMailCount, rnd);

            emailsFiltered = Store.emails.AsEnumerable();
            Store.emailsFiltered = new (Store.emails);


            Button quitButton = new("Quit")
            {
                X = Pos.Left(this) + 1,
                Y = Pos.Center(),
            };
            countLabel = new("Count: pending")
            {
                X = Pos.Left(quitButton),
                Y = Pos.Bottom(quitButton) + 5
            };
            FrameView emailsContainer = new("E-mails")
            {
                X = Pos.Right(quitButton)+1,
                Y = 1,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };

            Label queryLabel = new("Query: ")
            {
                X = 1,
                Y = Pos.Top(emailsContainer)
            };
            TextField queryField = new()
            {
                Y = Pos.Top(queryLabel),
                X = Pos.Right(queryLabel)+1,
                Width = Dim.Fill(1)
            };
            queryField.TextChanged += (newText) =>
            {
                queryString = newText;
            };
            emailsContainer.Add(queryLabel, queryField);
            emailListView = new()
            {
                X = 0,
                Y = Pos.Top(emailsContainer)+1,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
            };
            emailsContainer.Add(emailListView);
            Add(emailsContainer, quitButton, countLabel);
            quitButton.Clicked += () => RequestStop();

        }

        void GenerateUsers(int count, Random rnd)
        {
            for (int i = 0; i < count; i++)
            {
                var fullName = Faker.Name.FullName();
                var emailAddr = new EmailAddress((rnd.Next(0, 2) % 1 == 0)
                    ? Faker.Internet.Email(fullName)
                    : Faker.Internet.FreeEmail());

                var user = new User(Faker.Internet.UserName(),
                    Faker.Internet.FreeEmail().Split('@')[0], //now this is extra ugly
                    emailAddr);

                Store.userAddresses.Add(emailAddr);
                Store.users.Add(user);
            }
        }

        void GenerateMails(int count, Random rnd)
        {
            int userCnt = Store.users.Count;
            for (int i = 0; i < count; i++)
            {
                int userSender = rnd.Next(0, userCnt);
                int userReceiver = rnd.Next(0, userCnt);
                Store.emails.Add(new Email(
                    From: Store.users[userSender].Address,
                    To: Store.users[userReceiver].Address,
                    Content: Faker.Lorem.Paragraph(rnd.Next(3, 20)),
                    Sent: Faker.Finance.Maturity(0, 180), // f it, closest one I could find to a date generator
                    Title: string.Join(' ', Faker.Lorem.Words(rnd.Next(1, 5)))
                ));
            }
        }
        IEnumerable<Email> FilterByKeyword(IEnumerable<Email> source, string keyword, string searchString) => keyword.ToLower() switch
        {
            "from" => source.Where(mail => searchString == "*" ? true : mail.From.ToString().Contains(searchString, StringComparison.OrdinalIgnoreCase)),
            "to" => source.Where((mail => searchString == "*" ? true : mail.To.ToString().Contains(searchString, StringComparison.OrdinalIgnoreCase))),
            "order_by_desc" => source.OrderByDescending(mail => searchString switch
            {
                "title" => mail.Title,
                "content" => mail.Content,
                "sender" => mail.From.ToString(),
                "receiver" => mail.To.ToString(),
                _ => mail.ToString()
            }).AsEnumerable(),
            "order_by" => source.OrderBy(mail => searchString switch
            {
                "title" => mail.Title,
                "content" => mail.Content,
                "sender" => mail.From.ToString(),
                "receiver" => mail.To.ToString(),
                _ => mail.ToString()
            }).AsEnumerable(),
            _ => source,
        };
        IEnumerable<Email> FilterByKeyword(IEnumerable<Email> source, string searchString) =>
            source.Where(email =>
            email.Title.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
            email.Content.Contains(searchString, StringComparison.OrdinalIgnoreCase));

        IEnumerable<Email> GenerateFilter()
        {
            if(queryString is null or {IsEmpty: true })
            {
                return Store.emails;
            }
            IEnumerable<Email> filter = Store.emails;
            if(queryString.Contains(' '))
            {
                string[] splitBySpace = queryString.ToString()!.Split(' ');
                foreach (string s in splitBySpace)
                {
                    if (queryString.Contains(':'))
                    {
                        var split = s.Split(':');
                        if (split.Length == 1) filter = FilterByKeyword(filter, split[0]);
                        else filter = FilterByKeyword(filter, split[0], split[1]);
                    }
                    else
                    {
                        filter = FilterByKeyword(filter, s);
                    }
                }
            }
            else
            {
                string casted = queryString.ToString() ?? "";
                if (queryString.Contains(':'))
                {
                    var split = casted.Split(':');
                    if (split.Length == 1) return filter;
                    filter = FilterByKeyword(filter, split[0], split[1]);
                }
                else
                {
                    filter = FilterByKeyword(filter, casted);
                }
            }
            

            return filter;
        }

        void GenerateButtons()
        {

        }
    }
}
