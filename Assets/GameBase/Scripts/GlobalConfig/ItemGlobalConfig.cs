using System;
using System.Collections.Generic;
using GameBase;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemGlobalConfig", menuName = "GlobalConfigs/ItemGlobalConfig")]
[GlobalConfig("Assets/Resources/GlobalConfig/")]
public class ItemGlobalConfig : GlobalConfig<ItemGlobalConfig>
{
    public List<ResourcesData> ResourceDatas;
    public List<BoosterInfoData> BooosterInfoDatas;
    public List<LevelDifficultyData> LevelDifficultyDatas;

    public Sprite GetItemSprite(GameResource.Type resourceType)
    {
        for (int i = 0; i < ResourceDatas.Count; i++)
        {
            if (ResourceDatas[i].ResourceType == resourceType)
            {
                return ResourceDatas[i].Sprite;
            }
        }
        return null;
    }

    public BoosterInfoData GetBoosterData(GameResource.Type resourceType)
    {
        for (int i = 0; i < BooosterInfoDatas.Count; i++)
        {
            if (BooosterInfoDatas[i].ResourceType == resourceType)
            {
                return BooosterInfoDatas[i];
            }
        }
        return null;
    }

    public string GetNewBoosterUnlock(int level)
    {
        string boosterName = string.Empty;
        for (var i = 0; i < BooosterInfoDatas.Count; i++)
        {
            if (BooosterInfoDatas[i].LevelUnlock == level)
            {
                boosterName = $"Booster_{BooosterInfoDatas[i].ResourceType}";
                break;
            }
        }
        return boosterName;
    }
    public bool IsNeedShowBoosterUnlock(int level)
    {
        string boosterName = string.Empty;
        for (var i = 0; i < BooosterInfoDatas.Count; i++)
        {
            if (BooosterInfoDatas[i].LevelUnlock == level)
            {
                boosterName = $"Booster_{BooosterInfoDatas[i].ResourceType}";
                int isShown = PlayerPrefs.GetInt(boosterName, 0);
                return isShown != 1;
            }
        }

        return false;
    }
    public void ShowBoosterUnlock(GameResource.Type boosterType)
    {
       string boosterName = $"Booster_{boosterType}";
        int isShown = PlayerPrefs.GetInt(boosterName, 0);
        if (isShown != 1)
        {
            PlayerPrefs.SetInt(boosterName, 1);
        }
    }
    public void FetchRemote()
    {
        
    }
    
    public LevelDifficultyData GetLevelDifficultyData(LevelDifficulty type)
    {
        for (int i = 0; i < LevelDifficultyDatas.Count; i++)
        {
            if (LevelDifficultyDatas[i].Type == type)
            {
                return LevelDifficultyDatas[i];
            }
        }
        return null;
    }
}

[System.Serializable]
public class ResourcesData
{
    public GameResource.Type ResourceType;
    [PreviewField] public Sprite Sprite;
}

[System.Serializable]
public class BoosterInfoData
{
    public GameResource.Type ResourceType;
    public string Name;
    public string Description;
    public int LevelUnlock;
    public string MessageNoUse;
    [PreviewField] public Sprite Sprite;
    public int Price;
    public int PriceX3;
    public int RwLimit = 1;
}

[Serializable]
public class LevelDifficultyData
{
    public LevelDifficulty Type;
    public AlertLevelDifficultyData AlertLevelDifficultyData;
}

[Serializable]
public class AlertLevelDifficultyData
{ 
    [PreviewField] public Sprite LineTopAlertSprite;
    [PreviewField] public Sprite LineBottomAlertSprite;
    [PreviewField] public Sprite WarningAlertSprite;
    [PreviewField] public Sprite BodySprite;
    public string LevelDifficultyName;
    public Material LevelDifficultyColor;
}

