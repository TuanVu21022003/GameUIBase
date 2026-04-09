using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using TW.Utility.DesignPattern;
using UnityEngine;

public class TimeManager : Singleton<TimeManager>
{
    private DateTime currentTime;
    private DateTime nextMidnight;
    [SerializeField] private double timeToEndDay;
    public Reactive<string> lastDay;
    public string timeInstall;
    public static Reactive<bool> LoadDone = new (false);
    
    public static Action OnDayEnd;
    public static Action<double> OnTimeChangeWithValue;
    public static Action OnTimeChange;
    
    public DateTime GetCurrentTime() => currentTime;
    
    [ShowInInspector, ReadOnly] private List<ScheduledEvent> scheduledEvents = new();
    
    protected override void Awake()
    {
        base.Awake();
        UpdateTime();
    }
    
    private void Start()
    {
        LoadData();
    }
    
    private void LoadData()
    {
        lastDay = TimeDataSave.Instance.TimeLastDay;
        timeInstall = TimeDataSave.Instance.timeInstall;
        if (lastDay.Value.Equals(""))
        {
            lastDay.Value = currentTime.ToEnUsString();
            TimeDataSave.Instance.SaveData();
        }
        var dateOfLastDay = lastDay.Value.ToDateTime();
        if (dateOfLastDay.Date < currentTime.Date)
        {
            _ = CallResetDailyData();
        }

        if (timeInstall.Equals(""))
        {
            timeInstall = currentTime.ToEnUsString();
            PlayerInfoDataSave.Instance.SaveData();
        }
      
        _ = CallSetUserProperty();
    }

    private async UniTask CallSetUserProperty()
    {
        await UniTask.WaitForSeconds(5f);
        // IngameFirebaseAnalystic.Instance.SetUserRetention();
    }

    public double GetDayRetention()
    {
        return currentTime.Subtract(timeInstall.ToDateTime()).TotalDays;
    }
    
    private async UniTask CallResetDailyData()
    {
        await UniTask.WaitForEndOfFrame();
        OnDayEnd?.Invoke();
        SaveLastDay();
        LoadDone.Value = true;
    }
    
    private void Update()
    {
        currentTime = DateTime.UtcNow;
        ProcessDueEvents();
        if (timeToEndDay > 0)
        {
            timeToEndDay = (nextMidnight - currentTime).TotalSeconds;
            if (timeToEndDay <= 0)
            {
                //Debug.Log("Call end day");
                OnDayEnd?.Invoke();
                SaveLastDay();
                UpdateTime();
            }

            OnTimeChangeWithValue?.Invoke(timeToEndDay);
            OnTimeChange?.Invoke();
        }
    }
    
    private void UpdateTime()
    {
        currentTime = DateTime.UtcNow;
        nextMidnight = currentTime.Date.AddDays(1);
        timeToEndDay = (nextMidnight - currentTime).TotalSeconds;
    }
    
    private void FetchServerTime()
    {
        // try
        // {
        //     WebRequest request = WebRequest.Create($"http://www.google.com");
        //     request.Timeout = 5000;
        //     using WebResponse response = request.GetResponse();
        //     currentTime = DateTime.Parse(response.Headers["date"]);
        //     //Debug.Log($"Google time: {currentTime}");
        // }
        // catch
        // {
        //     currentTime = DateTime.UtcNow;
        // }
    }
    
    private void SaveLastDay()
    {
        lastDay.Value = DateTime.UtcNow.ToEnUsString();
        TimeDataSave.Instance.SaveData();
    }
    
    public void RegisterEventTime(DateTime eventTime, Action<int, bool> action, int id)
    {
        var se = new ScheduledEvent(eventTime, action, id);
        var idx = scheduledEvents.BinarySearch(se, ScheduledEventComparer.Instance);
        if (idx < 0) idx = ~idx;
        scheduledEvents.Insert(idx, se);
    }
    
    private void ProcessDueEvents()
    {
        if (scheduledEvents.Count == 0) return;

        for (var i = scheduledEvents.Count - 1; i >= 0; i--)
        {
            if (scheduledEvents[i].Time > currentTime) continue;
            scheduledEvents[i].actionCallBack?.Invoke(scheduledEvents[i].id, false);
            scheduledEvents.RemoveAt(i);
        }
    }

    public void ClearScheduledEvents()
    {
        scheduledEvents.Clear();
    }

    public void RemoveEvent(int id)
    {
        for (var i = scheduledEvents.Count - 1; i >= 0; i--)
        {
            if (id == scheduledEvents[i].id)
            {
                scheduledEvents.RemoveAt(i);
            }
        }
    }
}

public static class TimeExtension
{
    private static readonly CultureInfo EnUsCulture = CultureInfo.GetCultureInfo("en-US");

    private const string StandardDateTimeFormat = "MM/dd/yyyy HH:mm:ss";
    private const string StandardDateFormat = "MM/dd/yyyy";

    public static string ToEnUsString(this DateTime dateTime)
    {
        return dateTime.ToString(StandardDateTimeFormat, EnUsCulture);
    }

    public static DateTime ToDateTime(this string dateString, DateTime defaultValue = default)
    {
        if (string.IsNullOrWhiteSpace(dateString))
            return defaultValue;

        if (DateTime.TryParse(dateString, EnUsCulture, DateTimeStyles.None, out DateTime result))
            return result;

        return defaultValue;
    }
}

// small holder for a scheduled action
public class ScheduledEvent
{
    public int id;
    public DateTime Time;
    public Action<int, bool> actionCallBack;
    public string strTime;

    public ScheduledEvent(DateTime time, Action<int, bool> actionCallBack, int id)
    {
        Time = time; 
        this.actionCallBack = actionCallBack;
        strTime = time.ToEnUsString();
        this.id = id;
    }
}

public class ScheduledEventComparer : IComparer<ScheduledEvent>
{
    public static readonly ScheduledEventComparer Instance = new();
    public int Compare(ScheduledEvent x, ScheduledEvent y) => x.Time.CompareTo(y.Time);
}