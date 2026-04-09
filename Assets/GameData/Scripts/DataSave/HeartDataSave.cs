using System;
using UnityEngine;

[Serializable]
public class HeartDataSave : IDataSave<HeartDataSave>
{
    public static HeartDataSave Instance => InGameDataManager.Instance.InGameData.HeartDataSave;
    public bool IsDirty { get; set; }
    public HeartDataSave DefaultData()
    {
        return this;
    }
    
    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }

    public ShopDataSave FromJson(string json)
    {
        return JsonUtility.FromJson<ShopDataSave>(json);
    }
    
    public int currentHeart = -1;
    public Reactive<string> timeToAddAllHeart = new("");
    public Reactive<string> timeToAddOneHeart = new("");
    public Reactive<string> timeToEndInfiniteHeart = new("");
}
