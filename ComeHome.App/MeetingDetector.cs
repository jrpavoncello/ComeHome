using Microsoft.Win32;

namespace ComeHome.App;

/// <summary>
/// Detects whether the user is likely in a meeting by checking if any application
/// is currently using the microphone. Windows tracks microphone access in the
/// CapabilityAccessManager registry hive — an app whose LastUsedTimeStop is 0
/// is actively using the mic right now.
/// </summary>
internal static class MeetingDetector
{
    private const string MicConsentStorePath =
        @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\microphone";

    public static bool IsMicrophoneInUse()
    {
        using var root = Registry.CurrentUser.OpenSubKey(MicConsentStorePath);
        if (root is null)
            return false;

        return HasActiveMicUser(root);
    }

    private static bool HasActiveMicUser(RegistryKey parentKey)
    {
        foreach (var subKeyName in parentKey.GetSubKeyNames())
        {
            using var subKey = parentKey.OpenSubKey(subKeyName);
            if (subKey is null)
                continue;

            // Leaf app entries have a LastUsedTimeStop value.
            // A value of 0 means the mic is currently in use by this app.
            var stopValue = subKey.GetValue("LastUsedTimeStop");
            if (stopValue is long stopTime && stopTime == 0)
                return true;

            // NonPackaged (and other container keys) have nested sub-keys.
            if (HasActiveMicUser(subKey))
                return true;
        }

        return false;
    }
}
