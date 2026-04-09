using System;
using Cysharp.Text;
using UnityEngine;

public enum TimeFormat
{
    Symbol,
    Keyword
}

public static class TimeUtil
{
    public static string TimeToString(double inputTime, TimeFormat timeFormat = TimeFormat.Symbol)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(inputTime);
        if (timeFormat == TimeFormat.Keyword)
        {
            if (timeSpan.TotalDays >= 1)
            {
                return ZString.Format("{0:D2}d {1:D2}h {2:D2}m", timeSpan.Days, timeSpan.Hours, timeSpan.Minutes);
            }
            else if (timeSpan.TotalHours >= 1)
            {
                if (timeSpan.Seconds == 0)
                {
                    if (timeSpan.Minutes == 0)
                        return ZString.Format("{0:D2}h", timeSpan.Hours);
                    else
                        return ZString.Format("{0:D2}h {1:D2}m", timeSpan.Hours, timeSpan.Minutes);
                }
                return ZString.Format("{0:D2}h {1:D2}m {2:D2}s", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
            }
            else if (timeSpan.TotalMinutes >= 1)
            {
                if (timeSpan.Seconds == 0)
                    return ZString.Format("{0:D2}m", timeSpan.Minutes);
                return ZString.Format("{0:D2}m {1:D2}s", timeSpan.Minutes, timeSpan.Seconds);
            }
            if (timeSpan.TotalSeconds < 10)
            {
                return ZString.Format("{0:D2}s", timeSpan.Seconds);
            }
            return ZString.Format("{0:D2}s", timeSpan.Seconds);
        }
        else if (timeFormat == TimeFormat.Symbol)
        {
            if (timeSpan.TotalDays >= 1)
            {
                return ZString.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Days, timeSpan.Hours, timeSpan.Minutes);
            }
            else if (timeSpan.TotalHours >= 1)
            {
                if (timeSpan.Seconds == 0)
                {
                    if (timeSpan.Minutes == 0)
                        return ZString.Format("{0:D2}:00:00", timeSpan.Hours);
                    else
                        return ZString.Format("{0:D2}:{1:D2}:00", timeSpan.Hours, timeSpan.Minutes);
                }
                return ZString.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
            }
            else if (timeSpan.TotalMinutes >= 1)
            {
                return ZString.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
            }
            if (timeSpan.TotalSeconds < 10)
            {
                return ZString.Format("00:{0:D2}", timeSpan.Seconds);
            }
            return ZString.Format("00:{0:D2}", timeSpan.Seconds);
        }
        return "";
    }
}