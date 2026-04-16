using System;
using UnityEngine;

namespace GameBase
{
    [Serializable]
    public class LevelModel
    {
        [field: SerializeField] public LevelDifficulty LevelDifficulty;
    }
}
