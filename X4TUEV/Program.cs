// See https://aka.ms/new-console-template for more information
using Terminal.Gui;

using X4TUEV;
using X4TUEV.FakeEmailClient;

Console.WriteLine("Hello, Kurzus!");
// TODO async, try/catch, linq, immutable (record), de/serialize, collection


try
{
    new EmailAddress("asd@asd.asd.com");
} 
catch(Exception e)
{
    Console.Error.WriteLine(e.Message);
}
Application.Init();
Application.Run<TopLevelWindow>();

Application.Shutdown();

public class TopLevelWindow : Window
{

    public static User? user = null;

    public TopLevelWindow()
    {
        Title = "X4TUEV - Press Ctrl-Q to exit";
        Width = Dim.Fill();
        Height = Dim.Fill();
        Border.BorderStyle = BorderStyle.None;

        Label infoLabel = new("Sorry if windows are \"jumpy\" on drag, not my fault");

        Button loginBtn = new("Login")
        {
            X = Pos.Center(),
            Y = Pos.Center()
        };
        Add(infoLabel, loginBtn);
        loginBtn.Clicked += () =>
        {
            Application.Run(new LoginScreen());
            if(Store.currentUser == null)
            {
                MessageBox.Query("Oopsie", "Please log in to continue", "OK", "no fuk u");
            }
            else
            {
                Application.Run(new EmailClientScreen());
                Application.RequestStop();
            }
        };



    }

    public Task ShowScreen<T>() where T :  Toplevel, new()
    {
        TaskCompletionSource completionSource = new();

        Task.Run(() => Application.Run(new T())).ContinueWith((_) =>
        {
            completionSource.SetResult();
        });
        return completionSource.Task;
    }
}