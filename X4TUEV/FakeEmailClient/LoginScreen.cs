using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Terminal.Gui;

namespace X4TUEV.FakeEmailClient
{
    internal class LoginScreen : Window
    {

        TextField usernameField;
        TextField passwordField;
        TextField emailField;
        Button loginButton;
        public LoginScreen()
        {
            Modal = true;
            Width = Dim.Percent(80);
            Height = Dim.Percent(80);
            Title = "Login Screen";
            X = Pos.Center();
            Y = Pos.Center();

            Label usernameLabel = new("Username: ")
            {
                Y = Pos.Top(this) + 2,
                X = Pos.Center() - Pos.Percent(25),
            };

            Label passwordLabel = new("Password: ")
            {
                Y = Pos.Bottom(usernameLabel) + 2,
                X = Pos.Left(usernameLabel),
            };

            Label emailLabel = new("Email: ")
            {
                Y = Pos.Bottom(passwordLabel) + 2,
                X = Pos.Left(usernameLabel),
            };

            usernameField = new("admin")
            {
                X = Pos.Center(),
                Y = Pos.Top(usernameLabel),
                Width = Dim.Fill(),
            };

            passwordField = new("admin")
            {
                X = Pos.Left(usernameField),
                Y = Pos.Top(passwordLabel),
                Width = Dim.Fill(),
                Secret = true,
            };

            emailField = new("admin@fake.net")
            {
                X = Pos.Left(usernameField),
                Y = Pos.Top(emailLabel),
                Width = Dim.Fill(),
            };

            loginButton = new("Login")
            {
                X = Pos.Center(),
                Y = Pos.Bottom(emailField) + 5,
            };

            loginButton.Clicked += LoginButton_Clicked;

            Add(usernameLabel, usernameField, passwordLabel, passwordField, emailLabel, emailField, loginButton);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                loginButton.Clicked -= LoginButton_Clicked;
            }
            base.Dispose(disposing);
        }

        private void LoginButton_Clicked()
        {
            if(usernameField.Text != "admin" && passwordField.Text != "admin")
            {
                _ = MessageBox.ErrorQuery("Error", "Invalid username or password (Hint: admin admin)");
                return;
            }
            EmailAddress? email = null;
            try
            {
                email = new(emailField.Text.ToString() ?? "");
            } catch(Exception ex)
            {
                _ = MessageBox.ErrorQuery("Exception", ex.Message);
                return;
            }


            RequestStop();

        }
    }
}
