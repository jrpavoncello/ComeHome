using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ComeHome.App.Models;

/// <summary>
/// Bell schedule for a single day of the week.
/// Implements <see cref="INotifyPropertyChanged"/> so it can be used directly
/// as a binding source in the WPF UI.
/// </summary>
public class DaySchedule : INotifyPropertyChanged
{
    private bool _isEnabled = true;
    private TimeSpan _startTime = new(9, 0, 0);
    private TimeSpan _endTime = new(17, 0, 0);
    private int _intervalMinutes = 30;

    public DayOfWeek Day { get; set; }

    /// <summary>Short three-letter display name (Mon, Tue, …).</summary>
    public string DayName => Day switch
    {
        DayOfWeek.Monday    => "Mon",
        DayOfWeek.Tuesday   => "Tue",
        DayOfWeek.Wednesday => "Wed",
        DayOfWeek.Thursday  => "Thu",
        DayOfWeek.Friday    => "Fri",
        DayOfWeek.Saturday  => "Sat",
        DayOfWeek.Sunday    => "Sun",
        _ => Day.ToString()
    };

    public bool IsEnabled
    {
        get => _isEnabled;
        set { if (_isEnabled != value) { _isEnabled = value; OnPropertyChanged(); } }
    }

    public TimeSpan StartTime
    {
        get => _startTime;
        set { if (_startTime != value) { _startTime = value; OnPropertyChanged(); } }
    }

    public TimeSpan EndTime
    {
        get => _endTime;
        set { if (_endTime != value) { _endTime = value; OnPropertyChanged(); } }
    }

    public int IntervalMinutes
    {
        get => _intervalMinutes;
        set { if (_intervalMinutes != value) { _intervalMinutes = value; OnPropertyChanged(); } }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
