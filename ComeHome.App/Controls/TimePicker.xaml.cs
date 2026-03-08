using System.Windows;
using System.Windows.Controls;

namespace ComeHome.App.Controls;

/// <summary>
/// A compact hour : minute picker backed by two <see cref="ComboBox"/> controls.
/// Hours range 00-23; minutes in 5-minute increments (00, 05, 10, … 55).
/// </summary>
public partial class TimePicker : System.Windows.Controls.UserControl
{
    public static readonly DependencyProperty SelectedTimeProperty =
        DependencyProperty.Register(
            nameof(SelectedTime),
            typeof(TimeSpan),
            typeof(TimePicker),
            new FrameworkPropertyMetadata(
                new TimeSpan(9, 0, 0),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedTimePropertyChanged));

    public TimeSpan SelectedTime
    {
        get => (TimeSpan)GetValue(SelectedTimeProperty);
        set => SetValue(SelectedTimeProperty, value);
    }

    private bool _suppressUpdate;

    public TimePicker()
    {
        InitializeComponent();

        for (var h = 0; h < 24; h++)
            HourCombo.Items.Add(h.ToString("D2"));

        for (var m = 0; m < 60; m += 5)
            MinuteCombo.Items.Add(m.ToString("D2"));

        SyncComboBoxes();
    }

    private static void OnSelectedTimePropertyChanged(
        DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TimePicker picker && !picker._suppressUpdate)
            picker.SyncComboBoxes();
    }

    /// <summary>Push the current <see cref="SelectedTime"/> into the combo boxes.</summary>
    private void SyncComboBoxes()
    {
        _suppressUpdate = true;
        HourCombo.SelectedIndex = Math.Clamp(SelectedTime.Hours, 0, 23);
        MinuteCombo.SelectedIndex = Math.Clamp(SelectedTime.Minutes / 5, 0, 11);
        _suppressUpdate = false;
    }

    /// <summary>Pull the combo box selections back into <see cref="SelectedTime"/>.</summary>
    private void OnTimeChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_suppressUpdate || HourCombo.SelectedIndex < 0 || MinuteCombo.SelectedIndex < 0)
            return;

        _suppressUpdate = true;
        SelectedTime = new TimeSpan(HourCombo.SelectedIndex, MinuteCombo.SelectedIndex * 5, 0);
        _suppressUpdate = false;
    }
}
