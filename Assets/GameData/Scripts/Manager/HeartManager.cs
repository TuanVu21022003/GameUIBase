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
    public List<int> listID;
    
    [ShowInInspector]
    public IHeartState currentHeartState;
    
    private IHeartState normalHeartState = new NormalHeartState();
    private IHeartState addInfinityHeartState = new AddInfinityHeartState();
    private IHeartState addOneHeartState = new AddOneHeartState();

    private void Start()
    {
        LoadData();
        InitState();
    }

    private void InitState()
    {
        addInfinityHeartState.SetActionCallback(CallBackInfiniteHeart);
        addInfinityHeartState.InitState();
        
        normalHeartState.SetActionCallback(CallBackAddAllHeart);
        normalHeartState.InitState();
        
        addOneHeartState.SetActionCallback(CallbackAddOneHeart);
        addOneHeartState.InitState();
    }

    private void CallbackAddOneHeart()
    {
        AddOneHeart();
    }

    private void CallBackInfiniteHeart()
    {
        
    }

    private void CallBackAddAllHeart()
    {
        AddHeart((HeartGlobalConfig.Instance.MaxHeart - heartResource.Amount).ToInt());
        normalHeartState.ResetTime();
        addOneHeartState.ResetTime();
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
        //
        // if (!timeToEndInfiniteHeart.Value.Equals(""))
        // {
        //     CheckTimeEndInfiniteHeart();
        // }
        //
        // if (!timeToAddAllHeart.Value.Equals(""))
        // {
        //     CheckTimeAddAllHeart();
        // }
        //
        // if (!timeToAddOneHeart.Value.Equals(""))
        // {
        //     CheckTimeAddOneHeart();
        // }
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
            // isOnInfiniteHeart.Value = true;
            TimeManager.Instance.RegisterEventTime(timeEndInfiniteHeart, ResetTimeInfiniteHeart, GetId());
            return;
        }

        ResetTimeInfiniteHeart(-1);
    }

    private void ResetTimeInfiniteHeart(int idCallBack, bool ads = false)
    {
        timeToEndInfiniteHeart.Value = "";
        //isOnInfiniteHeart.Value = false;
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
            normalHeartState.SaveTime();
            if (!timeToAddOneHeart.Value.Equals(""))
                return;
            addOneHeartState.SaveTime();
        }
    }

    private void AddOneHeart(bool addByAds = false)
    {
        AddHeart(1, addByAds);
        // if (listID.Contains(idCallBack))
        //     listID.Remove(idCallBack);
    }

    private void AddHeart(int amount, bool addByAds = false)
    {
        heartResource.Amount += amount;
        SaveHeartData();
        if (heartResource.Amount >= HeartGlobalConfig.Instance.MaxHeart && timeToAddAllHeart.Value != "")
        {
            normalHeartState.ResetTime();
            addOneHeartState.ResetTime();
        }

        if (addByAds)
        {
            normalHeartState.ReduceTime();
        }

        if (heartResource.Amount < HeartGlobalConfig.Instance.MaxHeart)
        {
            addOneHeartState.SaveTime();
        }

        HeartDataSave.Instance.SaveData();
    }

    public bool IsEnoughHeart()
    {
        return heartResource.Amount > 0;
    }

    public void RefillAddOnHeart()
    {
        (addOneHeartState as AddOneHeartState)?.RemoveEvent();
        AddOneHeart(true);
    }

    public void RefillFullHeart()
    {
        heartResource.Amount = HeartGlobalConfig.Instance.MaxHeart;
        
        addOneHeartState.ResetTime();
        normalHeartState.ResetTime();
        
        TimeManager.Instance.ClearScheduledEvents();
        //listID.Clear();
        SaveHeartData();
    }

    public bool IsCanRefillHeart()
    {
        return heartResource.Amount < HeartGlobalConfig.Instance.MaxHeart;
    }

    public int GetId()
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
        //isOnInfiniteHeart.Value = true;
        var timeInfinite = TimeManager.Instance.GetCurrentTime();
        //Debug.Log(timeInfinite);
        if (!timeToEndInfiniteHeart.Value.Equals(""))
        {
            timeInfinite = timeToEndInfiniteHeart.Value.ToDateTime();
        }
        var timeEnd = timeInfinite.AddHours(rewardAmount.ToFloat());
       // Debug.Log(timeInfinite);
        timeToEndInfiniteHeart.Value = timeEnd.ToEnUsString();
        TimeManager.Instance.RegisterEventTime(timeEnd, ResetTimeInfiniteHeart, GetId());
        HeartDataSave.Instance.SaveData();
    }
}