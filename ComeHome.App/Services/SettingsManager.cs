using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using ComeHome.App.Models;

namespace ComeHome.App.Services;

/// <summary>
/// Loads and saves <see cref="ScheduleConfig"/> as JSON in the user's AppData folder.
/// </summary>
public static class SettingsManager
{
    private static readonly string SettingsDir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ComeHome");

    private static string SettingsPath => Path.Combine(SettingsDir, "settings.json");
    private static string RunningPath => Path.Combine(SettingsDir, "running.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new TimeSpanJsonConverter() },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static ScheduleConfig Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                return JsonSerializer.Deserialize<ScheduleConfig>(json, JsonOptions)
                       ?? new ScheduleConfig();
            }
        }
        catch
        {
            // Corrupted file — fall back to defaults.
        }

        return new ScheduleConfig();
    }

    public static void Save(ScheduleConfig config)
    {
        try
        {
            Directory.CreateDirectory(SettingsDir);
            var json = JsonSerializer.Serialize(config, JsonOptions);
            File.WriteAllText(SettingsPath, json);
        }
        catch
        {
            // Best-effort — swallow write errors.
        }
    }

    public static ScheduleConfig? LoadRunning()
    {
        try
        {
            if (File.Exists(RunningPath))
            {
                var json = File.ReadAllText(RunningPath);
                return JsonSerializer.Deserialize<ScheduleConfig>(json, JsonOptions);
            }
        }
        catch
        {
            // Corrupted file — fall back.
        }

        return null;
    }

    public static void SaveRunning(ScheduleConfig config)
    {
        try
        {
            Directory.CreateDirectory(SettingsDir);
            var json = JsonSerializer.Serialize(config, JsonOptions);
            File.WriteAllText(RunningPath, json);
        }
        catch
        {
            // Best-effort — swallow write errors.
        }
    }

    public static void ClearRunning()
    {
        try
        {
            if (File.Exists(RunningPath))
                File.Delete(RunningPath);
        }
        catch
        {
            // Best-effort — swallow delete errors.
        }
    }

    public static void SaveBellSound(string? bellSoundPath)
    {
        var config = Load();
        config.BellSoundPath = bellSoundPath;
        Save(config);
    }

    public static void SaveMuteDuringMeetings(bool mute)
    {
        var config = Load();
        config.MuteDuringMeetings = mute;
        Save(config);
    }

    public static void SaveShowNotification(bool show)
    {
        var config = Load();
        config.ShowNotification = show;
        Save(config);
    }

    public static void SaveSchedule(ScheduleConfig uiConfig)
    {
        var config = Load();
        config.ScheduleType = uiConfig.ScheduleType;
        config.WeeklySchedule = uiConfig.WeeklySchedule;
        config.CronExpression = uiConfig.CronExpression;
        Save(config);
    }

    /// <summary>
    /// Serializes <see cref="TimeSpan"/> as "HH:mm" strings for human-readable JSON.
    /// </summary>
    private sealed class TimeSpanJsonConverter : JsonConverter<TimeSpan>
    {
        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => TimeSpan.Parse(reader.GetString()!);

        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString(@"hh\:mm"));
    }
}
