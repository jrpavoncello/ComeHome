using ComeHome.App.Models;
using ComeHome.App.Scheduling;
using ComeHome.App.Services;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ComeHome.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const string DefaultNextBellMessage =
            "Press Start to begin. Your session will continue in the background even when you close the UI.";

        private readonly DispatcherTimer _bellTimer = new();
        private readonly System.Windows.Media.MediaPlayer _mediaPlayer = new();
        private ScheduleConfig _config = null!;
        private bool _isRunning;
        private DateTime? _nextBellTime;

        private static string DefaultBellPath =>
            Path.Combine(AppContext.BaseDirectory, "Sounds", "default_bell.mp3");

        private string BellSoundPath => _config.BellSoundPath ?? DefaultBellPath;

        public MainWindow()
        {
            InitializeComponent();

            var savedConfig = SettingsManager.Load();
            var runningConfig = SettingsManager.LoadRunning();

            _config = runningConfig ?? savedConfig;

            if (runningConfig is not null)
            {
                _config.BellSoundPath = savedConfig.BellSoundPath;
                _config.MuteDuringMeetings = savedConfig.MuteDuringMeetings;
            }

            _bellTimer.Tick += BellTimer_Tick;

            // Populate UI from config
            DayScheduleList.ItemsSource = _config.WeeklySchedule;
            CronExpressionBox.Text = _config.CronExpression;
            ScheduleTabControl.SelectedIndex = _config.ScheduleType == ScheduleType.Cron ? 1 : 0;
            MuteDuringMeetingsCheckBox.IsChecked = _config.MuteDuringMeetings;

            if (_config.BellSoundPath is not null)
            {
                SoundFileText.Text = Path.GetFileName(_config.BellSoundPath);
                ResetSoundButton.Visibility = Visibility.Visible;
            }

            // Subscribe to mute checkbox changes for auto-save
            MuteDuringMeetingsCheckBox.Checked += MuteDuringMeetings_Changed;
            MuteDuringMeetingsCheckBox.Unchecked += MuteDuringMeetings_Changed;

            ShowNotificationCheckBox.IsChecked = _config.ShowNotification;
            ShowNotificationCheckBox.Checked += ShowNotification_Changed;
            ShowNotificationCheckBox.Unchecked += ShowNotification_Changed;

            if (runningConfig is not null)
            {
                Start();
            }
        }

        // ── Toggle Start / Stop ──────────────────────────────────────────

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isRunning)
                Stop();
            else
                Start();
        }

        private void Start()
        {
            _nextBellTime = BellScheduler.GetNextBellTime(_config, DateTime.Now);

            if (_nextBellTime is null)
            {
                StatusText.Text = "No upcoming bell";
                NextBellText.Text = "Check your schedule configuration";
                SettingsManager.ClearRunning();
                return;
            }

            _isRunning = true;
            SettingsManager.SaveRunning(_config);
            ScheduleNextTick();

            ToggleButton.Content = "Stop";
            ToggleButton.Background = new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#C0392B"));
            StatusText.Text = "Running";
            UpdateNextBellText();
        }

        private void Stop()
        {
            _isRunning = false;
            _bellTimer.Stop();
            _nextBellTime = null;

            ToggleButton.Content = "Start";
            ToggleButton.Background = new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#4A8C6F"));
            StatusText.Text = "Ready";
            NextBellText.Text = DefaultNextBellMessage;

            SettingsManager.ClearRunning();
        }

        private void ScheduleNextTick()
        {
            if (_nextBellTime is null)
            {
                Stop();
                return;
            }

            var delay = _nextBellTime.Value - DateTime.Now;

            // Guard against negative or zero intervals
            if (delay <= TimeSpan.Zero)
                delay = TimeSpan.FromMilliseconds(50);

            _bellTimer.Stop();
            _bellTimer.Interval = delay;
            _bellTimer.Start();
        }

        private void BellTimer_Tick(object? sender, EventArgs e)
        {
            _bellTimer.Stop();

            if (MuteDuringMeetingsCheckBox.IsChecked == true && MeetingDetector.IsMicrophoneInUse())
            {
                MeetingStatusText.Text = $"Bell skipped — mic in use ({DateTime.Now:h:mm tt})";
            }
            else
            {
                PlayBell();
                MeetingStatusText.Text = "";

                if (_config.ShowNotification)
                {
                    ((App)System.Windows.Application.Current).ShowBalloonNotification(
                        "Come Home",
                        "Gently put aside what you are working on, take 3 deep breaths, and release your tension.");
                }
            }

            // Schedule the next bell
            _nextBellTime = BellScheduler.GetNextBellTime(_config, DateTime.Now);

            if (_nextBellTime is not null)
            {
                ScheduleNextTick();
                UpdateNextBellText();
            }
            else
            {
                Stop();
                NextBellText.Text = "No more bells scheduled today";
            }
        }

        // ── Bell sound ───────────────────────────────────────────────────

        private void PlayBell()
        {
            _mediaPlayer.Stop();
            _mediaPlayer.Open(new Uri(BellSoundPath, UriKind.Absolute));
            _mediaPlayer.Play();
        }

        private void PreviewSound_Click(object sender, RoutedEventArgs e)
        {
            PlayBell();
        }

        private void BrowseSound_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Select a bell sound",
                Filter = "Audio files (*.mp3;*.wav;*.wma)|*.mp3;*.wav;*.wma|All files (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                _config.BellSoundPath = dialog.FileName;
                SoundFileText.Text = Path.GetFileName(dialog.FileName);
                ResetSoundButton.Visibility = Visibility.Visible;
                SettingsManager.SaveBellSound(_config.BellSoundPath);
            }
        }

        private void ResetSound_Click(object sender, RoutedEventArgs e)
        {
            _config.BellSoundPath = null;
            SoundFileText.Text = "Default bell";
            ResetSoundButton.Visibility = Visibility.Collapsed;
            SettingsManager.SaveBellSound(null);
        }

        // ── Schedule tab ─────────────────────────────────────────────────

        private void ScheduleTab_Changed(object sender, SelectionChangedEventArgs e)
        {
            // Guard against early fires before _config is initialized
            if (_config is null) return;

            _config.ScheduleType = ScheduleTabControl.SelectedIndex == 1
                ? ScheduleType.Cron
                : ScheduleType.Simple;
        }

        private void CronExpression_Changed(object sender, TextChangedEventArgs e)
        {
            if (_config is null) return;

            var expression = CronExpressionBox.Text;

            if (BellScheduler.IsValidCron(expression))
            {
                CronValidationText.Text = "✓ Valid expression";
                CronValidationText.Foreground = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#4A8C6F"));

                var nextTimes = BellScheduler.GetNextFireTimes(expression);
                CronNextTimesList.ItemsSource = nextTimes
                    .Select(t => t.ToString("ddd MMM dd, h:mm:ss tt"))
                    .ToList();
            }
            else
            {
                CronValidationText.Text = "✗ Invalid expression";
                CronValidationText.Foreground = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#C0392B"));
                CronNextTimesList.ItemsSource = null;
            }

            _config.CronExpression = expression;
        }

        // ── Meeting detection ────────────────────────────────────────────

        private void MuteDuringMeetings_Changed(object sender, RoutedEventArgs e)
        {
            _config.MuteDuringMeetings = MuteDuringMeetingsCheckBox.IsChecked == true;
            SettingsManager.SaveMuteDuringMeetings(_config.MuteDuringMeetings);
        }

        // ── Notifications ─────────────────────────────────────────────

        private void ShowNotification_Changed(object sender, RoutedEventArgs e)
        {
            _config.ShowNotification = ShowNotificationCheckBox.IsChecked == true;
            SettingsManager.SaveShowNotification(_config.ShowNotification);
        }

        // ── Helpers ──────────────────────────────────────────────────

        private void UpdateNextBellText()
        {
            var next = BellScheduler.GetNextBellTime(_config, DateTime.Now);
            NextBellText.Text = next is not null
                ? $"Next bell at {next.Value:h:mm tt}"
                : "No upcoming bell";
        }

        private async void SaveSchedule_Click(object sender, RoutedEventArgs e)
        {
            SettingsManager.SaveSchedule(_config);

            SaveScheduleButton.Content = "✓ Saved";
            SaveScheduleButton.IsEnabled = false;

            await Task.Delay(1500);

            SaveScheduleButton.Content = "💾 Save Schedule";
            SaveScheduleButton.IsEnabled = true;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
            base.OnClosing(e);
        }
    }
}