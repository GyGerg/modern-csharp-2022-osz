using Terminal.Gui;

// See https://aka.ms/new-console-template for more information
namespace X4TUEV;
public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        Console.WriteLine("Hello, Kurzus!");
        Console.ReadKey();
        Application.Init();
        Application.Run<ExampleWindow>();

        return Quit();
    }

    static int Quit(bool hasError = false)
    {
        Application.Shutdown();
        return hasError ? -1 : 0;
    }
}


public class ExampleWindow : Window
{
    public ExampleWindow()
    {
        Title = "Example app (X4TUEV)";

        var nameLabel = new Label(){
            Text = "Username",
        };
        var nameEntry = new TextField(""){
            X = Pos.Right(nameLabel)+1,
            Y = Pos.Top(nameLabel),
            Width = Dim.Fill(),
        };

        var passLabel = new Label(){
            Text = "Password",
            X = Pos.Left(nameLabel),
            Y = Pos.Bottom(nameLabel)+1,
        };
        var passEntry = new TextField(""){
            Secret = true,
            X = Pos.Left(nameEntry),
            Y = Pos.Top(passLabel),
            Width = Dim.Fill(),
        };


        var btnExit = new Button(){
            Text = "Exit",
            Y = Pos.Bottom(passLabel)+1,
            X = Pos.Center(),
            IsDefault = true,

        };

        // btnExit.MouseEnter += (_) => btnExit.SetFocus();
        // btnExit.MouseLeave += (_) => SetFocus();
        btnExit.Clicked += () => {
            var res = MessageBox.Query(50,7,"Please don't leave me I'm scared", "Are you sure you want to quit?", new NStack.ustring[]{"Yes","No"});
            if(res==0)
            {
                Application.RequestStop();
            }
        };

        Add(nameLabel, nameEntry, passLabel, passEntry, btnExit);
    }
}
// TODO async, try/catch, linq, immutable (record), de/serialize, collection

