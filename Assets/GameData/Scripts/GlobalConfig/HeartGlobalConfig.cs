using Sirenix.Utilities;
using UnityEngine;

[CreateAssetMenu(fileName = "HeartGlobalConfig", menuName = "GlobalConfigs/HeartGlobalConfig")]
[GlobalConfig("Assets/GameData/Resources/GlobalConfig/")]
public class HeartGlobalConfig : GlobalConfig<HeartGlobalConfig>
{
    [field: SerializeField] public int MaxHeart { get; private set; } = 5;
    [field: SerializeField] public int TimeRefillHeart { get; private set; } = 30;
}
