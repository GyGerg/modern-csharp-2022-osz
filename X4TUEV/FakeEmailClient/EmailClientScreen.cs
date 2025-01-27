﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Serialization;

using Terminal.Gui;

namespace X4TUEV.FakeEmailClient
{
    public class EmailClientScreen : Window
    {

        readonly EmailListView emailListView;


        object? timeoutToken;


        NStack.ustring _queryString = "";
        NStack.ustring queryString 
        { 
            get => _queryString; 
            set 
            {
                _queryString = value;
                var asStr = value.ToString() ?? "";
                if(timeoutToken != null)
                {
                    _ = Application.MainLoop.RemoveTimeout(timeoutToken);
                }
                timeoutToken = Application.MainLoop.AddTimeout(TimeSpan.FromMilliseconds(200), (mainLoop) =>
                {
                    Store.emailsFiltered.Clear();
                    foreach (var email in GenerateFilter())
                    {
                        Store.emailsFiltered.Add(email);
                    }
                    timeoutToken = null;
                    return false;
                });
            } 
        }
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

            Store.emailsFiltered = new (Store.emails);


            Button quitButton = new("Quit")
            {
                X = Pos.Left(this) + 1,
                Y = Pos.Center(),
            };

            FrameView emailsContainer = new("E-mails -- Double-click on a row to open your mail")
            {
                X = Pos.Right(quitButton)+1,
                Y = 1,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };
            Button exportButton = new("Export")
            {
                X = 0,
                Y = Pos.Bottom(quitButton)+1,
                AutoSize = true,
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
                queryString = queryField.Text;
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

            Add(emailsContainer, quitButton, exportButton);

            quitButton.Clicked += () => RequestStop();

            exportButton.Clicked += async () =>
            {
                int res = MessageBox.Query("Export", "Would you like to export your e-mails?\n", "Export all", "Filtered only", "Cancel");
                if(res == 0 || res == 1)
                {
                    bool exportAll = res == 0;

                    SaveDialog saveDialog = new SaveDialog("Export E-mails", "You can pick JSON or JSON (or JSON) for export.", new() {"json"});
                    Application.Run(saveDialog);
                    
                    if(!saveDialog.Canceled)
                    {
                        if (saveDialog.FileName.Length < 1 || saveDialog.FilePath.IsEmpty)
                        {
                            MessageBox.ErrorQuery("Error", "At least pick a name (or empty filepath)", "Damn:(");
                            return;
                        }
                        var pathString = saveDialog.FilePath.ToString()!;
                        /*if(pathString.EndsWith("xml"))
                        {
                            pathString.Insert(pathString.Length-3, ".");
                        }
                        else */if(pathString.EndsWith("json"))
                        {
                            pathString = pathString.Insert(pathString.Length-4, ".");
                        }
                        var fileEnd = pathString.Split('.').LastOrDefault() ?? "";
                        MessageBox.Query($"FileEnd: {saveDialog.FileName}", $"Full path: \n{pathString}", "Close");

                        try
                        {
                            using FileStream fileStream = File.Create(saveDialog.FilePath.ToString()!);
                            switch (fileEnd)
                            {
                                case "json":
                                    await JsonSerializer.SerializeAsync<IEnumerable<Email>>(fileStream, exportAll ? Store.emails : Store.emailsFiltered);
                                    MessageBox.Query("File", "Success!", "Thanks");
                                    break;
                                /*case "xml":
                                    XmlSerializer xmlSerializer = new(typeof(List<Email>));
                                    xmlSerializer.Serialize(fileStream, new List<Email>(exportAll ? Store.emails : Store.emailsFiltered));
                                    break;*/
                                default:
                                    throw new ArgumentException("Invalid file format");
                            }
                        }
                        catch(Exception ex)
                        {
                            MessageBox.ErrorQuery("Error", ex.Message, "OK");
                        }
                    }
                }
            };

        }

        Task<int> GenerateUsers(int count, Random rnd)
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
            return Task<int>.FromResult(0);
        }

        Task<int> GenerateMails(int count, Random rnd)
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
            return Task<int>.FromResult(0);
        }
        IEnumerable<Email> FilterByKeyword(IEnumerable<Email> source, string keyword, string searchString) => keyword.ToLower() switch
        {
            "from" => source.Where(mail => searchString == "*" ? true : mail.From.ToString().Contains(searchString, StringComparison.OrdinalIgnoreCase)),
            "to" => source.Where((mail => searchString == "*" ? true : mail.To.ToString().Contains(searchString, StringComparison.OrdinalIgnoreCase))),
            "first" => int.TryParse(searchString, out int num) && num > 0 ? source.Take(num) : source,
            "last" => int.TryParse(searchString, out int num) && num > 0 ? source.TakeLast(num) : source,
            "before" => DateTime.TryParse(searchString, out var date) ? source.Where(mail => mail.Sent >= date) : source,
            "after" => DateTime.TryParse(searchString, out var date) ? source.Where(mail => mail.Sent <= date) : source,
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
        IEnumerable<Email> FilterByKeyword(IEnumerable<Email> source, string searchString) => searchString switch
        {
            _ => source.Where(email =>
            email.From.ToString().Contains(searchString, StringComparison.OrdinalIgnoreCase)    ||
            email.To.ToString().Contains(searchString, StringComparison.OrdinalIgnoreCase)      ||
            email.Title.Contains(searchString, StringComparison.OrdinalIgnoreCase)              ||
            email.Content.Contains(searchString, StringComparison.OrdinalIgnoreCase))
        };

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
