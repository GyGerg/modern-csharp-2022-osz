using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Terminal.Gui;

namespace X4TUEV.FakeEmailClient
{
    public class EmailListView : TableView
    {
        readonly DataTable dataTable;


        public EmailListView()
        {
            dataTable = new DataTable();
            dataTable.Columns.Add(new DataColumn(nameof(Email.From), typeof(string)) { ReadOnly = true });
            dataTable.Columns.Add(new DataColumn(nameof(Email.Title), typeof(string)) { ReadOnly = true });
            dataTable.Columns.Add(new DataColumn(nameof(Email.To), typeof(string)) { ReadOnly = true });
            dataTable.Columns.Add(new DataColumn("hash", typeof(int)));

            CellActivated += (cellEventArgs) =>
            {
                
                Application.Run(new EmailWindow(Store.emailsFiltered[cellEventArgs.Row]));
            };
            //dataTable.Columns["hash"]!.MaxLength = 1;




            Store.emailsFiltered.CollectionChanged += EmailsFiltered_CollectionChanged;

            Table = dataTable;
            Application.MainLoop.Invoke(Init);
        }

        async void Init()
        {
            Application.MainLoop.MainIteration();
            

            await Task.Run(async () =>
            {
                foreach (var email in Store.emailsFiltered)
                {
                    var newRow = dataTable.NewRow();
                    newRow[nameof(Email.From)] = email.From;
                    newRow[nameof(Email.Title)] = email.Title;
                    newRow[nameof(Email.To)] = email.To;
                    newRow["hash"] = email.GetHashCode();
                    Application.MainLoop.Invoke(() =>
                    {
                        dataTable.Rows.Add(newRow);
                        SetNeedsDisplay();
                    }); // idk if it does anything fancy
                }
                await Task.Delay(100);
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Store.emails.CollectionChanged -= EmailsFiltered_CollectionChanged;
            }
            base.Dispose(disposing);
        }

        private void EmailsFiltered_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var table = Table;
            table.Rows.Clear();
            SetNeedsDisplay();
            foreach (var email in Store.emailsFiltered)
            {
                var newRow = table.NewRow();
                newRow[nameof(Email.From)] = email.From;
                newRow[nameof(Email.Title)] = email.Title;
                newRow[nameof(Email.To)] = email.To;
                newRow["hash"] = email.GetHashCode();
                table.Rows.Add(newRow);
            }
            Console.WriteLine($"Count: {Store.emailsFiltered.Count}");
            Table = table;
            SetNeedsDisplay();
        }
    }
}
