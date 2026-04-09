using System;
using System.Collections.Generic;
using UnityEngine;

public class AddOneHeartState : NormalHeartState
{
    private int currentIdEvent = -1;
    public override void InitState()
    {
        timeEndState = HeartDataSave.Instance.timeToAddOneHeart;
    }
    
    public override void CheckTimeState()
    {
        var currentTime = TimeManager.Instance.GetCurrentTime();
        var nextHeartAddTime = timeEndState.Value.ToDateTime();

        if (nextHeartAddTime.Subtract(currentTime).TotalSeconds > 0)
        {
            currentIdEvent = HeartManager.Instance.GetId();
            TimeManager.Instance.RegisterEventTime(nextHeartAddTime, CallBack, currentIdEvent);
            return;
        }

        CallBack(-1, false);
    }

    public override void SaveTime()
    {
        var currentTime = TimeManager.Instance.GetCurrentTime();
        var defaultMinutesForHeart = HeartGlobalConfig.Instance.TimeRefillHeart;
        var timeAddHeartString = timeEndState.Value.Equals("")
            ? currentTime.AddMinutes(defaultMinutesForHeart).ToEnUsString()
            : timeEndState.Value.ToDateTime().AddMinutes(defaultMinutesForHeart).ToEnUsString();
        timeEndState.Value = timeAddHeartString;
        currentIdEvent = HeartManager.Instance.GetId();
        TimeManager.Instance.RegisterEventTime(timeAddHeartString.ToDateTime(), CallBack, currentIdEvent);
        HeartDataSave.Instance.SaveData();
    }

    private void CallBack(int id, bool isAds)
    {
        if (id != currentIdEvent) return;
        actionCallback?.Invoke();
        currentIdEvent = -1;
    }

    public void RemoveEvent()
    {
        if (currentIdEvent == -1) return;
        TimeManager.Instance.RemoveEvent(currentIdEvent);
    }
}