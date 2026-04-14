using System.Collections.Generic;
using Sirenix.OdinInspector;
using TW.Utility.CustomType;
using TW.Utility.DesignPattern;
using UnityEngine;

public class HeartManager : Singleton<HeartManager>
{
    public GameResource heartResource;
    public Reactive<string> timeToAddAllHeart = new("");
    public Reactive<string> timeToAddOneHeart = new("");
    public Reactive<string> timeToEndInfiniteHeart = new("");
    public Reactive<bool> isOnInfiniteHeart;
    public List<int> listID;

    private void Start()
    {
        LoadData();
    }

    private void LoadData()
    {
        heartResource = new GameResource(GameResource.Type.Heart, HeartDataSave.Instance.currentHeart);
        timeToAddAllHeart = HeartDataSave.Instance.timeToAddAllHeart;
        timeToAddOneHeart = HeartDataSave.Instance.timeToAddOneHeart;
        timeToEndInfiniteHeart = HeartDataSave.Instance.timeToEndInfiniteHeart;
        
        if (heartResource.Amount == -1)
        {
            FirstTimeInit();
        }

        if (!timeToEndInfiniteHeart.Value.Equals(""))
        {
            CheckTimeEndInfiniteHeart();
        }

        if (!timeToAddAllHeart.Value.Equals(""))
        {
            CheckTimeAddAllHeart();
        }

        if (!timeToAddOneHeart.Value.Equals(""))
        {
            CheckTimeAddOneHeart();
        }
    }

    private void CheckTimeEndInfiniteHeart()
    {
        var currentTime = TimeManager.Instance.GetCurrentTime();
        var timeEndInfiniteHeart = timeToEndInfiniteHeart.Value.ToDateTime();
        Debug.Log("str current time: " + currentTime);
        Debug.Log("str time: " + timeToEndInfiniteHeart.Value);
        Debug.Log("date time: " + timeEndInfiniteHeart);
        if (timeEndInfiniteHeart.Subtract(currentTime).TotalSeconds > 0)
        {
            isOnInfiniteHeart.Value = true;
            TimeManager.Instance.RegisterEventTime(timeEndInfiniteHeart, ResetTimeInfiniteHeart, GetId());
            return;
        }

        ResetTimeInfiniteHeart(-1);
    }

    private void CheckTimeAddOneHeart()
    {
        var currentTime = TimeManager.Instance.GetCurrentTime();
        var nextHeartAddTime = timeToAddOneHeart.Value.ToDateTime();

        if (nextHeartAddTime.Subtract(currentTime).TotalSeconds > 0)
        {
            TimeManager.Instance.RegisterEventTime(nextHeartAddTime, AddOneHeart, GetId());
            return;
        }

        AddOneHeart(-1);
    }

    private void CheckTimeAddAllHeart()
    {
        var currentTime = TimeManager.Instance.GetCurrentTime();
        var nextHeartAddTime = timeToAddAllHeart.Value.ToDateTime();

        if (nextHeartAddTime.Subtract(currentTime).TotalSeconds > 0) return;

        AddHeart((HeartGlobalConfig.Instance.MaxHeart - heartResource.Amount).ToInt());
        ResetTimeToAddHeart();
        ResetTimeToAddOneHeart();
    }

    private void ResetTimeInfiniteHeart(int idCallBack, bool ads = false)
    {
        timeToEndInfiniteHeart.Value = "";
        isOnInfiniteHeart.Value = false;
        HeartDataSave.Instance.SaveData();
    }

    private void ResetTimeToAddHeart()
    {
        timeToAddAllHeart.Value = "";
        HeartDataSave.Instance.SaveData();
    }

    private void ResetTimeToAddOneHeart()
    {
        timeToAddOneHeart.Value = "";
        HeartDataSave.Instance.SaveData();
    }

    private void FirstTimeInit()
    {
        heartResource.Amount = HeartGlobalConfig.Instance.MaxHeart;
        SaveHeartData();
    }

    private void SaveHeartData()
    {
        HeartDataSave.Instance.currentHeart = heartResource.Amount.ToInt();
        HeartDataSave.Instance.SaveData();
    }

    [Button]
    public void UseHeart(int amount)
    {
        heartResource.Amount -= amount;
        SaveHeartData();
        if (heartResource.Amount < HeartGlobalConfig.Instance.MaxHeart)
        {
            SaveTimeToAddAllHeart();
            if (!timeToAddOneHeart.Value.Equals(""))
                return;
            SaveTimeToAddOneHeart();
        }
    }

    private void SaveTimeToAddOneHeart(bool addByAds = false)
    {
        var currentTime = TimeManager.Instance.GetCurrentTime();
        var defaultMinutesForHeart = HeartGlobalConfig.Instance.TimeRefillHeart;
        var timeAddHeartString = timeToAddOneHeart.Value.Equals("") || addByAds
            ? currentTime.AddMinutes(defaultMinutesForHeart).ToEnUsString()
            : timeToAddOneHeart.Value.ToDateTime().AddMinutes(defaultMinutesForHeart).ToEnUsString();
        timeToAddOneHeart.Value = timeAddHeartString;
        TimeManager.Instance.RegisterEventTime(timeAddHeartString.ToDateTime(), AddOneHeart, GetId());
        HeartDataSave.Instance.SaveData();
    }

    private void SaveTimeToAddAllHeart()
    {
        var currentTime = TimeManager.Instance.GetCurrentTime();
        var defaultMinutesForHeart = HeartGlobalConfig.Instance.TimeRefillHeart;
        var nextHeartAddTime = timeToAddAllHeart.Value.Equals("")
            ? currentTime.AddMinutes(defaultMinutesForHeart)
            : timeToAddAllHeart.Value.ToDateTime().AddMinutes(defaultMinutesForHeart);

        var timeToAddHeartString = nextHeartAddTime.ToEnUsString();
        timeToAddAllHeart.Value = timeToAddHeartString;
        HeartDataSave.Instance.SaveData();
    }

    private void AddOneHeart(int idCallBack, bool addByAds = false)
    {
        AddHeart(1, addByAds);
        if (listID.Contains(idCallBack))
            listID.Remove(idCallBack);
    }

    private void AddHeart(int amount, bool addByAds = false)
    {
        heartResource.Amount += amount;
        SaveHeartData();
        if (heartResource.Amount >= HeartGlobalConfig.Instance.MaxHeart && timeToAddAllHeart.Value != "")
        {
            ResetTimeToAddHeart();
            ResetTimeToAddOneHeart();
        }

        if (heartResource.Amount < HeartGlobalConfig.Instance.MaxHeart)
        {
            SaveTimeToAddOneHeart(addByAds);
        }

        HeartDataSave.Instance.SaveData();
    }

    public bool IsEnoughHeart()
    {
        return heartResource.Amount > 0;
    }

    public void RefillAddOnHeart()
    {
        for (var i = listID.Count - 1; i >= 0; i--)
        {
            TimeManager.Instance.RemoveEvent(listID[i]);
            listID.RemoveAt(i);
        }
        AddOneHeart(-1, true);
    }

    public void RefillFullHeart()
    {
        heartResource.Amount = HeartGlobalConfig.Instance.MaxHeart;
        ResetTimeToAddHeart();
        ResetTimeToAddOneHeart();
        TimeManager.Instance.ClearScheduledEvents();
        //listID.Clear();
        SaveHeartData();
    }

    public bool IsCanRefillHeart()
    {
        return heartResource.Amount < HeartGlobalConfig.Instance.MaxHeart;
    }

    private int GetId()
    {
        var idReturn = 0;
        while (listID.Contains(idReturn))
        {
            idReturn++;
        }

        listID.Add(idReturn);
        return idReturn;
    }

    public void AddHeartInfiniteTime(BigNumber rewardAmount)
    {
        RefillFullHeart();
        SetTimeInfinite(rewardAmount);
    }

    private void SetTimeInfinite(BigNumber rewardAmount)
    {
        isOnInfiniteHeart.Value = true;
        var timeInfinite = TimeManager.Instance.GetCurrentTime();
        //Debug.Log(timeInfinite);
        if (!timeToEndInfiniteHeart.Value.Equals(""))
        {
            timeInfinite = timeToEndInfiniteHeart.Value.ToDateTime();
        }
        var timeEnd = timeInfinite.AddSeconds(rewardAmount.ToFloat());
       // Debug.Log(timeInfinite);
        timeToEndInfiniteHeart.Value = timeEnd.ToEnUsString();
        TimeManager.Instance.RegisterEventTime(timeEnd, ResetTimeInfiniteHeart, GetId());
        HeartDataSave.Instance.SaveData();
    }
}