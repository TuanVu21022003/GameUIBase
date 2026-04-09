using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SettingDataSave: IDataSave<SettingDataSave>
{
    public static SettingDataSave Instance => InGameDataManager.Instance.InGameData.SettingDataSave;
    public bool IsDirty { get; set; }
    public SettingDataSave DefaultData()
    {
        return this;
    }
    
    [field: SerializeField] public List<SettingData> settingData { get; set; } = new();
}

[Serializable]
public class SettingData
{
    public SettingKey settingKey = SettingKey.None;
    public Reactive<bool> ableSetting = new(false);
}

public enum SettingKey
{
    None,
    MusicBg,
    Sound,
    Vibrate
}