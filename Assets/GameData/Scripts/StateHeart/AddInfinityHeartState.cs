using UnityEngine;

public class AddInfinityHeartState : NormalHeartState
{
    public int currentId;
    public override void InitState()
    {
        timeEndState = HeartDataSave.Instance.timeToEndInfiniteHeart;
    }

    public override void CheckTimeState()
    {
        var currentTime = TimeManager.Instance.GetCurrentTime();
        var timeEndInfiniteHeart = timeEndState.Value.ToDateTime();
        Debug.Log("str current time: " + currentTime);
        Debug.Log("str time: " + timeEndState.Value);
        Debug.Log("date time: " + timeEndInfiniteHeart);
        if (timeEndInfiniteHeart.Subtract(currentTime).TotalSeconds > 0)
        {
            // isOnInfiniteHeart.Value = true;
            currentId = HeartManager.Instance.GetId();
            TimeManager.Instance.RegisterEventTime(timeEndInfiniteHeart,ResetFilTime , currentId);
            return;
        }

        ResetTime();
    }
    
    private void ResetFilTime(int idCallBack, bool ads = false)
    {
        if (idCallBack != currentId) return;
        currentId = -1;
        ResetTime();
    }
}
