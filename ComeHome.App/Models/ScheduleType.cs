namespace ComeHome.App.Models;

/// <summary>
/// Determines which scheduling mode is active.
/// </summary>
public enum ScheduleType
{
    /// <summary>Lay-user friendly per-day schedule (Monday–Sunday).</summary>
    Simple,

    /// <summary>Power-user Quartz cron expression.</summary>
    Cron
}
