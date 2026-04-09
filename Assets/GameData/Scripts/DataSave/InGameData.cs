using System;
using CoreData;
using UnityEngine;

[Serializable]
public class InGameData
{
    [field: SerializeField] public PlayerInfoDataSave PlayerInfoDataSave { get; set; } = new();
    [field: SerializeField] public PlayerResourceDataSave PlayerResourceDataSave { get; set; } = new();
    [field: SerializeField] public SettingDataSave SettingDataSave { get; set; } = new();
    [field: SerializeField] public ShopDataSave ShopDataSave { get; set; } = new();
    [field: SerializeField] public HeartDataSave HeartDataSave { get; set; } = new();
    [field: SerializeField] public TimeDataSave TimeDataSave { get; set; }

    public void LoadData()
    {
        PlayerInfoDataSave = DataSerializer.LoadDataFromPrefs<PlayerInfoDataSave>();
        PlayerResourceDataSave = DataSerializer.LoadDataFromPrefs<PlayerResourceDataSave>();
        SettingDataSave = DataSerializer.LoadDataFromPrefs<SettingDataSave>();
        ShopDataSave = DataSerializer.LoadDataFromPrefs<ShopDataSave>();
        HeartDataSave = DataSerializer.LoadDataFromPrefs<HeartDataSave>();
        TimeDataSave = DataSerializer.LoadDataFromPrefs<TimeDataSave>();
    }

    public void SaveAllData()
    {
        PlayerInfoDataSave.SaveData();
        PlayerResourceDataSave.SaveData();
        SettingDataSave.SaveData();
        ShopDataSave.SaveData();
        HeartDataSave.SaveData();
        TimeDataSave.SaveData();
    }
}