using System.Text;

using Terminal.Gui;

namespace X4TUEV
{
    internal class SidebarStuff : Toplevel
    {
        FrameView sidebar;
        Button sidebarButton;

        Label stuffLabel;
        bool _isSidebarShowing = false;
        bool IsSidebarShowing
        {
            get => _isSidebarShowing;
            set
            {
                if (_isSidebarShowing != value)
                {
                    _isSidebarShowing = value;
                    if (value) Add(sidebar);
                    else Remove(sidebar);
                    sidebarButton.Text = btnText;
                    SetNeedsDisplay(sidebar.Bounds);
                    sidebarButton.SetNeedsDisplay();
                }
            }
        }
        string btnText => $"{(_isSidebarShowing ? "Close" : "Show")} sidebar";
        int percent = 35;
        public SidebarStuff()
        {
            CanFocus = true;
            //var tokenFunc = Application.MainLoop.AddIdle(() =>
            //{
            //    SetNeedsDisplay();
            //    return true;
            //});

            sidebar = new FrameView("Sidebar")
            {
                Height = Dim.Fill(),
                Width = Dim.Percent(percent),
                X = -1,
                Y = 0,

            };


            sidebarButton = new Button(btnText)
            {
                X = Pos.Left(this) + 2,
                Y = Pos.Top(this) + 2,
            };

            var msgButton = new Button("MessageBox")
            {
                X = Pos.Center(),
                Y = Pos.Center()
            };
            var scrollFrame = new FrameView("Scroll frame")
            {
                X = Pos.Left(this) + Pos.Function(GetWidth),
                Y = 0,
                Width = Dim.Percent(100 - percent, true),
                Height = Dim.Fill() - 5,
            };

            var sw = new ScrollView()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };
            //scrollFrame.Add(sw);
            sw.ContentSize = new Size(200, 100);
            sw.ContentOffset = new Point(0, 0);
            sw.Add(new FrameView("Asd")
            {
                X = 5,
                Y = 10,
                Width = 80,
                Height = 60,
                CanFocus = true,
            });


            //msgButton.Clicked += () =>  MessageBox.Query("Title", "Message", "OK");
            msgButton.Clicked += () =>
            {
                FileDialog fd = new FileDialog()
                {
                    AllowedFileTypes = new[] { "json", "csv", "xml" },
                    Title = "get file mate",
                };
                Application.Run(fd);
                if (!fd.Canceled && fd?.FilePath is { } fp && !string.IsNullOrWhiteSpace(fp.ToString()))
                {
                    MessageBox.Query("Success", "File selected", "OK");
                }
            };

            stuffLabel = new Label("")
            {
                X = Pos.Center(),
                Y = Pos.Top(this) + 2
            };

            var asdLabel = new Label("proba szoveg")
            {
                X = Pos.Left(this) + 5,
                Y = Pos.Center()
            };

            sidebarButton.Clicked += SidebarSwitch;

            Add(sidebarButton, msgButton);
            Add(stuffLabel, asdLabel);
            Add(scrollFrame);
        }

        private async void SidebarSwitch()
        {
            //IsSidebarShowing = !IsSidebarShowing;
            await SidebarAnim(!IsSidebarShowing);
        }

        private void MoveButtonTo(View view)
        {
            if (view == null || view == sidebarButton.SuperView)
                return;

            if (sidebarButton.SuperView is { } parent && view != parent)
            {
                parent.Remove(sidebarButton);
                view.Add(sidebarButton);

                sidebarButton.X = Pos.Left(view) + 2;
                sidebarButton.Y = Pos.Top(view) + 2;
                sidebarButton.SetNeedsDisplay();
            }
            else
            {
                MessageBox.ErrorQuery("Error", "Button has no SuperView", "OK", "fuck");
            }
        }

        private int GetWidth() => (int)(Application.Top.Bounds.Width * (double)(percent * 0.01));

        private Task<bool> SidebarAnim(bool showSidebar, CancellationTokenSource cancellationTokenSource = default)
        {
            TaskCompletionSource<bool> completionSource = new();

            int width = GetWidth();

            Func<int, bool> condition = showSidebar ? (i) => i <= -1 : (i) => i >= -width;
            int increment = (width / 6) * (showSidebar ? 1 : -1);

            Application.MainLoop.Invoke(async () =>
            {
                if (showSidebar)
                {
                    IsSidebarShowing = true;
                    MoveButtonTo(sidebar);
                }

                for (int i = showSidebar ? -width : -1; showSidebar ? i <= -1 : i >= -width; i += increment)
                {
                    if (cancellationTokenSource?.IsCancellationRequested ?? false)
                    {
                        completionSource.SetCanceled();
                        return;
                    }
                    MoveSidebarTo(i);
                    await Task.Delay(1);
                }
                MoveSidebarTo(showSidebar ? -1 : -width);
                if (!showSidebar)
                {
                    IsSidebarShowing = false;
                    MoveButtonTo(this);
                }
                _ = completionSource.TrySetResult(true);
            });

            return completionSource.Task;

        }

        private void MoveSidebarTo(int x)
        {
            sidebar.X = x;
            sidebar.SetNeedsDisplay();
            stuffLabel.Text = $"{sidebar.X}";
        }
    }
}
