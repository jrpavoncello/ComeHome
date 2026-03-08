using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using ComeHome.App.Services;
using Forms = System.Windows.Forms;

namespace ComeHome.App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private Forms.NotifyIcon? _trayIcon;
        private MainWindow? _mainWindow;

        public void ShowBalloonNotification(string title, string text, int timeoutMs = 5000)
        {
            _trayIcon?.ShowBalloonTip(timeoutMs, title, text, Forms.ToolTipIcon.None);
        }

        protected override void OnStartup(System.Windows.StartupEventArgs e)
        {
            base.OnStartup(e);

            ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown;

            var appIcon = LoadTrayIcon() ?? SystemIcons.Application;

            _trayIcon = new Forms.NotifyIcon
            {
                Icon = appIcon,
                Text = "Come Home",
                Visible = true,
                ContextMenuStrip = CreateContextMenu()
            };

            _trayIcon.Click += TrayIcon_Click;

            _mainWindow = new MainWindow();
            _mainWindow.Show();
        }

        private Forms.ContextMenuStrip CreateContextMenu()
        {
            var menu = new Forms.ContextMenuStrip();
            menu.Items.Add("Open", null, (_, _) => ShowMainWindow());
            menu.Items.Add("-");
            menu.Items.Add("Exit", null, (_, _) => ExitApplication());
            return menu;
        }

        private void TrayIcon_Click(object? sender, EventArgs e)
        {
            if (e is Forms.MouseEventArgs mouseArgs && mouseArgs.Button == Forms.MouseButtons.Left)
            {
                ShowMainWindow();
            }
        }

        private void ShowMainWindow()
        {
            if (_mainWindow is null || !_mainWindow.IsLoaded)
            {
                _mainWindow = new MainWindow();
            }

            _mainWindow.Show();
            _mainWindow.WindowState = System.Windows.WindowState.Normal;
            _mainWindow.Activate();
        }

        private void ExitApplication()
        {
            if (_trayIcon is not null)
            {
                _trayIcon.Visible = false;
                _trayIcon.Dispose();
                _trayIcon = null;
            }

            _mainWindow?.Close();
            Shutdown();
        }

        protected override void OnExit(System.Windows.ExitEventArgs e)
        {
            SettingsManager.ClearRunning();

            if (_trayIcon is not null)
            {
                _trayIcon.Visible = false;
                _trayIcon.Dispose();
            }

            base.OnExit(e);
        }

        private static Icon? LoadTrayIcon()
        {
            try
            {
                var sri = GetResourceStream(new Uri("ComeHome.png", UriKind.Relative));
                if (sri is null)
                    return null;

                using var stream = sri.Stream;
                using var original = new Bitmap(stream);
                var size = Forms.SystemInformation.SmallIconSize;
                var resized = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb);
                using (var g = Graphics.FromImage(resized))
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.DrawImage(original, 0, 0, size.Width, size.Height);
                }
                return Icon.FromHandle(resized.GetHicon());
            }
            catch
            {
                return null;
            }
        }
    }
}
