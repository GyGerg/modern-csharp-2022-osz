using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Terminal.Gui;

namespace X4TUEV.FakeEmailClient
{
    internal class EmailWindow : Window
    {
        public EmailWindow(Email email)
        {
            Title = email.Title;
            EmailDataFrame topView = new(email)
            {
                X = 0,
                Y = 0,
                AutoSize = true,
                Width = Dim.Fill()
            };
            TextView bottomView = new()
            {
                Width = Dim.Fill(),
                Height = Dim.Fill(1),
                ReadOnly = true,
                Border = new Border()
                {
                    BorderStyle = BorderStyle.Double,
                    Padding = new Thickness(1),
                    BorderThickness = new Thickness(1),
                },
                
                Y = Pos.Bottom(topView)+1,
                WordWrap = true,
                Text = email.Content
            };
            Button quitButton = new("Close")
            {
                Y = Pos.Bottom(bottomView),
                X = Pos.Percent(15),
            };
            quitButton.Clicked += () => Application.RequestStop();
            Add(topView, bottomView, quitButton);
        }
    }

    internal class EmailDataFrame : View
    {
        public EmailDataFrame(Email email) 
        {
            Height = Dim.Function(() => Math.Max(Bounds.Height, 8));
            Width = Dim.Fill();
            Label fromLabel = new("From:")
            {
                Y = 1,
                X = 1,
                Width = Text.Length,
            };
            Label fromLabelData = new(email.From.ToString())
            {
                Y = Pos.Top(fromLabel),
                X = Pos.Right(fromLabel) +1,
                Width = Dim.Fill(1),
                TextAlignment = TextAlignment.Right,
            };
            Label toLabel = new($"To: {email.To}")
            {
                Y = Pos.Bottom(fromLabel)+1,
                X = Pos.Left(fromLabel),
            };
            Label sentLabel = new($"Sent: {email.To}")
            {
                Y = Pos.Bottom(fromLabel) + 1,
                X = Pos.Left(fromLabel),
            };
            Add(fromLabel, fromLabelData, toLabel, sentLabel);
        }
    }
}
