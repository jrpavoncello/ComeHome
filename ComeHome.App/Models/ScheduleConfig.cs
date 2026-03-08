namespace ComeHome.App.Models;

/// <summary>
/// Complete application configuration that is persisted to disk.
/// </summary>
public class ScheduleConfig
{
    public ScheduleType ScheduleType { get; set; } = ScheduleType.Simple;

    public List<DaySchedule> WeeklySchedule { get; set; } = CreateDefaultWeekly();

    public string CronExpression { get; set; } = "0 */30 9-17 ? * MON-FRI";

    /// <summary>Null means use the built-in default bell sound.</summary>
    public string? BellSoundPath { get; set; }

    public bool MuteDuringMeetings { get; set; } = true;

    /// <summary>Show a Windows notification when the bell rings.</summary>
    public bool ShowNotification { get; set; }

    /// <summary>
    /// Creates the default weekly schedule: Mon–Fri enabled 09:00–17:00 every 30 min,
    /// Sat and Sun disabled.
    /// </summary>
    public static List<DaySchedule> CreateDefaultWeekly()
    {
        DayOfWeek[] ordered =
        [
            DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
            DayOfWeek.Thursday, DayOfWeek.Friday,
            DayOfWeek.Saturday, DayOfWeek.Sunday
        ];

        return ordered.Select(d => new DaySchedule
        {
            Day = d,
            IsEnabled = d is not (DayOfWeek.Saturday or DayOfWeek.Sunday),
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(17, 0, 0),
            IntervalMinutes = 30
        }).ToList();
    }
}
