using System;
using Core.UI.Activities;
using Cysharp.Threading.Tasks;
using TMPro;
using TW.UGUI.Core.Views;
using UnityEngine;
using UnityEngine.UI;

namespace GameUI
{
    public class CheatObject : MonoBehaviour
    {
        public static bool HelperIsActive = false;
        [field: SerializeField] private Button LoadLevelButton { get; set; }
        [field: SerializeField] private Button ChangeBgButton { get; set; }
        [field: SerializeField] private Button HideUIButton { get; set; }
        [field: SerializeField] private Button TestUIButton { get; set; }
        [field: SerializeField] private Button WinButton { get; set; }
        [field: SerializeField] private Button LoseButton { get; set; }
        [field: SerializeField] private TMP_InputField LevelInput { get; set; }

        private void Start()
        {
            // if (GameManager.Instance.BuildType == BuildType.Release)
            // {
            //     gameObject.SetActive(false);
            //     return;
            // }
#if !CHEAT_ONLY
            gameObject.SetActive(false);
            return;
#endif
            LoadLevelButton.SetOnClickDestination(OnLoadLevelButtonClicked);
            HideUIButton.onClick.AddListener(OnHideUIButtonClicked);
            TestUIButton.SetOnClickDestination(TestUIButtonClicked);
            WinButton.SetOnClickDestination(OnWinButtonClicked);
            LoseButton.SetOnClickDestination(OnLoseButtonClicked);
        }

        private async UniTask OnLoadLevelButtonClicked()
        {
            if (int.TryParse(LevelInput.text, out int levelNumber))
            {
                PlayerInfoDataSave.Instance.playerLevel.Value = levelNumber;
                Memory<object> args = new Memory<object>(new object[]
                {
                    (Func<UniTask>)(async () =>
                    {
                        // await GameManager.Instance.LoadCurrentLevel();
                        // LoadLevel
                    }),
                    null
                });
                await UIManager.Instance.OpenActivityAsync<ActivityLoading>(args);
            }

        }

        private void OnHideUIButtonClicked()
        {
            Launcher launcher = FindAnyObjectByType<Launcher>();
            float originalAlpha = launcher.GetComponent<CanvasGroup>().alpha;
            launcher.GetComponent<CanvasGroup>().alpha = originalAlpha > 0.5 ? 0 : 1;
        }

        private async UniTask TestUIButtonClicked()
        {
            // await ScreenContainer.Find(ContainerKey.Screens).PopAsync(false);
            // await UniTask.DelayFrame(1);
            // // GameManager.Instance.LoadCurrentLevel();
            // ScreenOptions viewOption = new ScreenOptions(nameof(ScreenInGame), stack: false);
            // await ScreenContainer.Find(ContainerKey.Screens).PushAsync(viewOption);
        }

        private void OnWinButtonClicked()
        {
            // GameManager.Instance.WinLevel().Forget();
        }

        private void OnLoseButtonClicked()
        {
            // GameManager.Instance.LoseLevelCheat().Forget();
        }
    }
}