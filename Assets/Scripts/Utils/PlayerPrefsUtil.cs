using UnityEngine;

public static class PlayerPrefsUtil
{

    public static void SetByEvent(InspectorEvent @event, bool value) => SetBool(@event.Event, value);
    public static void SetByEvent(InspectorEvent @event, int value) => SetInt(@event.Event, value);
    public static void SetByEvent(InspectorEvent @event, float value) => SetFloat(@event.Event, value);
    public static void SetByEvent(InspectorEvent @event, string value) => SetString(@event.Event, value);

    public static bool GetBoolByEvent(InspectorEvent @event, bool defaultValue) => GetBool(@event.Event, defaultValue);
    public static int GetIntByEvent(InspectorEvent @event, int defaultValue) => GetInt(@event.Event, defaultValue);
    public static float GetFloatByEvent(InspectorEvent @event, float defaultValue) => GetFloat(@event.Event, defaultValue);
    public static string GetStringByEvent(InspectorEvent @event, string defaultValue) => GetString(@event.Event, defaultValue);

    #region Basic Operations

    #region Boolean
    private static bool GetBool(string key, bool defaultValue)
    {
        if (PlayerPrefs.HasKey(key))
            return PlayerPrefs.GetInt(key) != 0;
        return defaultValue;
    }

    private static void SetBool(string key, bool value)
    {
        PlayerPrefs.SetInt(key, value ? 1 : 0);
    }
    #endregion

    #region Integer
    private static int GetInt(string key, int defaultValue)
    {
        if (PlayerPrefs.HasKey(key))
            return PlayerPrefs.GetInt(key);
        return defaultValue;
    }

    private static void SetInt(string key, int value)
    {
        PlayerPrefs.SetInt(key, value);
    }
    #endregion

    #region Float
    private static float GetFloat(string key, float defaultValue)
    {
        if (PlayerPrefs.HasKey(key))
            return PlayerPrefs.GetFloat(key);
        return defaultValue;
    }

    private static void SetFloat(string key, float value)
    {
        PlayerPrefs.SetFloat(key, value);
    }
    #endregion

    #region String
    private static string GetString(string key, string defaultValue)
    {
        if (PlayerPrefs.HasKey(key))
            return PlayerPrefs.GetString(key);
        return defaultValue;
    }

    private static void SetString(string key, string value)
    {
        PlayerPrefs.SetString(key, value);
    }
    #endregion

    #endregion

}
