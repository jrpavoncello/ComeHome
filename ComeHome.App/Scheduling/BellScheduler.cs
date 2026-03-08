using ComeHome.App.Models;
using Quartz;

namespace ComeHome.App.Scheduling;

/// <summary>
/// Calculates the next bell time for both Simple (weekly) and Cron schedule modes.
/// </summary>
public static class BellScheduler
{
    /// <summary>
    /// Returns the next bell time strictly after <paramref name="after"/>,
    /// or <c>null</c> if no future bell is scheduled.
    /// </summary>
    public static DateTime? GetNextBellTime(ScheduleConfig config, DateTime after)
    {
        return config.ScheduleType switch
        {
            ScheduleType.Simple => GetNextSimple(config.WeeklySchedule, after),
            ScheduleType.Cron   => GetNextCron(config.CronExpression, after),
            _ => null
        };
    }

    // ── Simple (weekly) schedule ─────────────────────────────────────────

    private static DateTime? GetNextSimple(List<DaySchedule> schedule, DateTime after)
    {
        // Look up to 8 days ahead to guarantee a full week wrap-around.
        for (var dayOffset = 0; dayOffset < 8; dayOffset++)
        {
            var checkDate = after.Date.AddDays(dayOffset);
            var daySchedule = schedule.FirstOrDefault(s => s.Day == checkDate.DayOfWeek);

            if (daySchedule is not { IsEnabled: true } || daySchedule.IntervalMinutes <= 0)
                continue;

            var start = checkDate + daySchedule.StartTime;
            var end   = checkDate + daySchedule.EndTime;

            // Walk through bell times for this day.
            var bell = start;
            while (bell <= end)
            {
                if (bell > after)
                    return bell;

                bell = bell.AddMinutes(daySchedule.IntervalMinutes);
            }
        }

        return null; // No enabled days at all.
    }

    // ── Cron schedule ────────────────────────────────────────────────────

    private static DateTime? GetNextCron(string cronExpression, DateTime after)
    {
        try
        {
            var cron = new CronExpression(cronExpression);
            var next = cron.GetNextValidTimeAfter(new DateTimeOffset(after));
            return next?.LocalDateTime;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>Validates whether a string is a legal Quartz cron expression.</summary>
    public static bool IsValidCron(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            return false;

        try
        {
            _ = new CronExpression(expression);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Returns the next <paramref name="count"/> fire times for a cron expression
    /// (useful for previewing in the UI).
    /// </summary>
    public static List<DateTime> GetNextFireTimes(string cronExpression, int count = 5)
    {
        var times = new List<DateTime>();

        try
        {
            var cron = new CronExpression(cronExpression);
            var current = DateTimeOffset.Now;

            for (var i = 0; i < count; i++)
            {
                var next = cron.GetNextValidTimeAfter(current);
                if (next is null) break;

                times.Add(next.Value.LocalDateTime);
                current = next.Value;
            }
        }
        catch
        {
            // Invalid expression — return empty list.
        }

        return times;
    }
}
