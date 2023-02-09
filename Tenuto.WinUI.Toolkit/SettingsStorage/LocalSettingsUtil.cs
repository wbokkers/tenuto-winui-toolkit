using System.Runtime.CompilerServices;
using Windows.Storage;

namespace Tenuto.WinUI.Toolkit.SettingsStorage;

public static class LocalSettingsUtil
{
    /// <summary>
    /// Get the value from the backing field (if set).
    /// If not set, set the backing field from the local settings.
    /// </summary>
    public static bool GetBoolValue(bool defaultValue, [CallerMemberName] string key = null!)
    {
        return LocalSettingsUtil.GetValue<bool>(key, defaultValue);
    }

    /// <summary>
    /// Store the value in the local settings and also set the backing field
    /// </summary>
    public static void SetBoolValue(bool value, [CallerMemberName] string key = null!)
    {
        LocalSettingsUtil.SetValue<bool>(key, value);
    }

    

    public static int GetIntValue(int defaultValue, [CallerMemberName] string key = null!)
    {
        return LocalSettingsUtil.GetValue<int>(key, defaultValue);
    }

    public static void SetIntValue(int value, [CallerMemberName] string key = null!)
    {
        LocalSettingsUtil.SetValue<int>(key, value);
    }

    public static double GetDoubleValue(double defaultValue, [CallerMemberName] string key = null!)
    {
        return LocalSettingsUtil.GetValue<double>(key, defaultValue);
    }

    public static void SetDoubleValue(double value, [CallerMemberName] string key = null!)
    {
        LocalSettingsUtil.SetValue<double>(key, value);
    }

    public static string? GetStringValue(string? defaultValue, [CallerMemberName] string key = null!)
    {
        return LocalSettingsUtil.GetValue<string>(key, defaultValue);
    }


    /// <summary>
    /// Saves the setting in the settings store, and
    /// also in the backing property field
    /// </summary>
    public static void SetStringValue(string? value, [CallerMemberName] string key = null!)
    {
        LocalSettingsUtil.SetValue<string>(key, value);
    }

    public static void RemoveValue(string settingsKey)
    {
        ApplicationData.Current.LocalSettings.Values.Remove(settingsKey);
    }


    public static T? GetValue<T>(string settingsKey, T? defaultValue = default)
    {
        var resultValue = defaultValue;
        try
        {
            if (ApplicationData.Current.LocalSettings.Values.TryGetValue(settingsKey, out var value))
                resultValue = (T)value;
        }
        catch
        {
        }

        return resultValue;
    }

    public static void SetValue<T>(string settingsKey, T? value)
    {
        ApplicationData.Current.LocalSettings.Values[settingsKey] = value;
    }
}

