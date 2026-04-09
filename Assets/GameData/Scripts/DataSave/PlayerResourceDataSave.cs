using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerResourceDataSave : IDataSave<PlayerResourceDataSave>
{
    public List<GameResourceData> gameResourceData = new();
    public static PlayerResourceDataSave Instance => InGameDataManager.Instance.InGameData.PlayerResourceDataSave;

    public bool IsDirty { get; set; }
    public PlayerResourceDataSave DefaultData()
    {
        return this;
    }

    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }

    public PlayerResourceDataSave FromJson(string json)
    {
        return JsonUtility.FromJson<PlayerResourceDataSave>(json);
    }
    
    
}
