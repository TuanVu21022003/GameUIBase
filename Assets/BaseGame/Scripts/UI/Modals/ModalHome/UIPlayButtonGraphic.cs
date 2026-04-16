using Core;
using R3;
using TMPro;
using UnityEngine;

public class UIPlayButtonGraphic : MonoBehaviour
{
    [field: SerializeField] private Reactive<int> Level {get; set;}
    [field: SerializeField] private TextMeshProUGUI[] TextLevel {get; set;}
    [field: SerializeField] private GameObject NormalLevel {get; set;}
    [field: SerializeField] private GameObject HardLevel {get; set;}
    [field: SerializeField] private GameObject SuperHardLevel {get; set;}
    private void Start()
    {
        Level = PlayerInfoDataSave.Instance.playerLevel;
        Level.Subscribe(OnLevelChange).AddTo(this);
    }

    private void OnLevelChange(int level)
    {
        LevelConfig levelConfig = LevelGlobalConfig.Instance.GetLevelConfig(level);
        NormalLevel.SetActive(levelConfig.LevelType == LevelDifficulty.Normal);
        HardLevel.SetActive(levelConfig.LevelType == LevelDifficulty.Hard);
        SuperHardLevel.SetActive(levelConfig.LevelType == LevelDifficulty.SuperHard);
        foreach (TextMeshProUGUI textMeshProUGUI in TextLevel)
        {
            textMeshProUGUI.SetText("Level {0}", level);
        }
    }
}
