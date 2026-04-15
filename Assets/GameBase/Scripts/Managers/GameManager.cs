using System;
using Core.UI.Modals;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using TW.UGUI.Core.Modals;
using TW.UGUI.Core.Views;
using TW.Utility.CustomType;
using TW.Utility.DesignPattern;
using UnityEngine;

namespace Core
{
    public enum GameState
    {
        None,
        MainMenu,
        InGame,
        Win,
        Lose,
    }

    public class GameManager : DDOLSingleton<GameManager>
    {
        [field: SerializeField] public GameState GameState { get; set; } = GameState.None;
        private void Start()
        {
            Application.targetFrameRate = 60;
#if !UNITY_EDITOR
            Application.targetFrameRate = 60;
#endif
        }

        public bool IsState(GameState state)
        {
            return GameState == state;
        }

        public void SetState(GameState state)
        {
            GameState = state;
        }

        public async UniTask LoadCurrentLevel()
        {
            // await LevelManager.Instance.LoadLevel();
            PlayerResourceManager.Instance.ChangeResource(GameResource.Type.Heart, new BigNumber(-1));
        }

        public bool TryLoadLevelPlaying()
        {
            return PlayerInfoDataSave.Instance.playerLevel.CurrentValue <= DefaultGlobalConfig.Instance.DefaultLevelPlaying;
        }
    }
}