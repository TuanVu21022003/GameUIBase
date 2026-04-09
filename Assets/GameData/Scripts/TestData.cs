using System;
using Cysharp.Text;
using R3;
using Sirenix.OdinInspector;
using TMPro;
using TW.Utility.CustomType;
using UnityEngine;

public class TestData : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textCountHeart;
    [SerializeField] private TextMeshProUGUI textCountdown;
    
    public Reactive<BigNumber> countHeart = new Reactive<BigNumber>();
    public Reactive<string> timeToAddEnergy = new("");
    public Reactive<string> timeToEndInfinite = new("");
    public DateTime dTimeToAddEnergy;
    
    public bool addEvent;
    public bool isOnInfiniteEnergy;

    private void Start()
    {
        
    }

    [Button]
    private void Init()
    {
        timeToAddEnergy = HeartManager.Instance.timeToAddOneHeart;
        timeToEndInfinite = HeartManager.Instance.timeToEndInfiniteHeart;
        countHeart = HeartManager.Instance.heartResource.ReactiveAmount;
        countHeart.Subscribe(OnChangeCountHeart).AddTo(this);
        timeToAddEnergy.Subscribe(ChangeTimeAddOneEnergy).AddTo(this);

        timeToEndInfinite.Subscribe(ChangeTimeEndInfinite).AddTo(this);
    }

    

    private void OnChangeCountHeart(BigNumber value)
    {
        textCountHeart.text = value.ToStringUIFloor();
    }
    
    private void ChangeTime()
    {
        var currentTime = TimeManager.Instance.GetCurrentTime();
        var timeChange = dTimeToAddEnergy.Subtract(currentTime);
        textCountdown.SetTextFormat("{0}", TimeUtil.TimeToString(timeChange.TotalSeconds));
    }

    private void ChangeTimeEndInfinite(string timeChange)
    {
        var activeTime = !timeChange.Equals("");
        if (activeTime)
            dTimeToAddEnergy = timeToEndInfinite.Value.ToDateTime();
        if (activeTime && !addEvent)
        {
           
            addEvent = true;
            isOnInfiniteEnergy = true;
            TimeManager.OnTimeChange += ChangeTime;
        }

        if (!activeTime && isOnInfiniteEnergy)
        {
            isOnInfiniteEnergy = false;
            textCountdown.SetTextFormat("{0}", "Full");
            TimeManager.OnTimeChange -= ChangeTime;
            addEvent = false;
        }
    }

    private void ChangeTimeAddOneEnergy(string timeChange)
    {
        var activeTime = !timeChange.Equals("");
        if (activeTime)
            dTimeToAddEnergy = timeToAddEnergy.Value.ToDateTime();
        
        if (activeTime && !addEvent)
        {
            addEvent = true;
            TimeManager.OnTimeChange += ChangeTime;
        }

        if (!activeTime)
        {
            textCountdown.SetTextFormat("{0}", "Full");
            TimeManager.OnTimeChange -= ChangeTime;
            addEvent = false;
        }
    }
    
    [Button]
    private void TestRemoveOneHeart()
    {
        HeartManager.Instance.UseHeart(1);
    }
    
    [Button]
    private void TestAddOneHeart()
    {
        HeartManager.Instance.UseHeart(1);
    }
}
