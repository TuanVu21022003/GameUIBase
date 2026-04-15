using UnityEngine;
using Sirenix.Utilities;

[CreateAssetMenu(fileName = "DefaultGlobalConfig", menuName = "GlobalConfigs/DefaultGlobalConfig")]
[GlobalConfig("Assets/GameBase/Resources/GlobalConfig/")]
public class DefaultGlobalConfig : GlobalConfig<DefaultGlobalConfig>
{
    [field: SerializeField] public bool PlayGameAtStart { get; set; }
    [field: SerializeField] public int DefaultBackToMenuLevel { get; set; }
    [field: SerializeField] public int DefaultX2CoinLevel { get; set; }
    [field: SerializeField] public int DefaultLevelPlaying { get; set; }
    [field: SerializeField] public int WinGameReward { get; set; }
    [field: SerializeField] public int[] ReviveCoins { get; set; }
    [field: SerializeField] public int ReviveSlot { get; set; }
    [field: SerializeField] public float TimeDelayLose { get; set; }
}