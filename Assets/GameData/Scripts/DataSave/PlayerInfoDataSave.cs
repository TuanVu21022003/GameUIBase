using System;
using UnityEngine;

[Serializable]
public class PlayerInfoDataSave : IDataSave<PlayerInfoDataSave>
{
    public Reactive<int> playerLevel = new(0);  
    public static PlayerInfoDataSave Instance => InGameDataManager.Instance.InGameData.PlayerInfoDataSave;
    public bool IsDirty { get; set; }
    public PlayerInfoDataSave DefaultData()
    {
        return this;
    }

    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }

    public PlayerInfoDataSave FromJson(string json)
    {
        return JsonUtility.FromJson<PlayerInfoDataSave>(json);
    }
}
