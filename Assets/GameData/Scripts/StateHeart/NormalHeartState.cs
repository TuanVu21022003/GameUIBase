using System;
using UnityEngine;

public class NormalHeartState : IHeartState
{
    public Action actionCallback;
    public Reactive<string> timeEndState = new Reactive<string>("");
    
    public virtual void SetActionCallback(Action action) => actionCallback = action;
    
    public virtual void InitState()
    {
        timeEndState = HeartDataSave.Instance.timeToAddAllHeart;
        CheckTimeState();
    }

    public virtual void CheckTimeState()
    {
        var currentTime = TimeManager.Instance.GetCurrentTime();
        var nextHeartAddTime = timeEndState.Value.ToDateTime();

        if (nextHeartAddTime.Subtract(currentTime).TotalSeconds > 0) return;

        actionCallback?.Invoke();
    }

    public virtual void ResetTime()
    {
        timeEndState.Value = "";
        HeartDataSave.Instance.SaveData();
    }

    public virtual void SaveTime()
    {
        var currentTime = TimeManager.Instance.GetCurrentTime();
        var defaultMinutesForHeart = HeartGlobalConfig.Instance.TimeRefillHeart;
        var nextHeartAddTime = timeEndState.Value.Equals("")
            ? currentTime.AddMinutes(defaultMinutesForHeart)
            : timeEndState.Value.ToDateTime().AddMinutes(defaultMinutesForHeart);

        var timeToAddHeartString = nextHeartAddTime.ToEnUsString();
        timeEndState.Value = timeToAddHeartString;
        HeartDataSave.Instance.SaveData();
    }

    public void ReduceTime()
    {
        var dateTimeAll = timeEndState.Value.ToDateTime();
        var timeAddOneHeart = HeartGlobalConfig.Instance.TimeRefillHeart;
        dateTimeAll = dateTimeAll.AddMinutes(-timeAddOneHeart);
        var currentTime = TimeManager.Instance.GetCurrentTime();
        if (dateTimeAll < currentTime)
        {
            ResetTime();
            return;
        }
        timeEndState.Value = dateTimeAll.ToEnUsString();
        HeartDataSave.Instance.SaveData();
    }
}
