using System;
using System.Threading;
using Core.UI.Screens;
using Cysharp.Threading.Tasks;
using GameUI;
using LitMotion;
using Sirenix.OdinInspector;
using TMPro;
using TW.UGUI.Core.Activities;
using TW.UGUI.MVPPattern;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class SceneName
{
    public const string MainMenu = "MainMenuScene";
    public const string InGame = "GameplayScene";
}

namespace Core.UI.Activities
{
    public class ActivityLoadingFirstGame : Activity
    {
        [field: SerializeField] public ActivityLoadingFirstGameContext.UIPresenter UIPresenter { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            AddLifecycleEvent(UIPresenter, 1);
        }

        public override async UniTask Initialize(Memory<object> args)
        {
            await base.Initialize(args);
        }
    }


    [Serializable]
    public class ActivityLoadingFirstGameContext
    {
        [HideLabel]
        [Serializable]
        public class UIModel : IAModel
        {
            public UniTask Initialize(Memory<object> args)
            {
                return UniTask.CompletedTask;
            }
        }

        [HideLabel]
        [Serializable]
        public class UIView : IAView
        {
            [field: Title(nameof(UIView))]
            [field: SerializeField]
            public CanvasGroup MainView { get; private set; }

            [field: SerializeField] public Slider Slider { get; private set; }
            [field: SerializeField] public TextMeshProUGUI TxtLoading { get; private set; }
            [field: SerializeField] public TextMeshProUGUI LoadDescriptionText { get; set; }
            
            public UniTask Initialize(Memory<object> args)
            {
                return UniTask.CompletedTask;
            }

            public void OnLoadingProcessUpdate(float value)
            {
                TxtLoading.text = $"{Mathf.RoundToInt(Slider.value * 100)}%";
                Slider.value = value;
            }
        }

        [HideLabel]
        [Serializable]
        public class UIPresenter : IAPresenter, IActivityLifecycleEventSimple
        {
            [field: SerializeField] public UIView View { get; set; } = new();
            [field: SerializeField] public UIModel Model { get; private set; } = new();
            
            [field: SerializeField, ReadOnly] public Reactive<float> LoadProgress { get; set; }
            public bool IsLoadingComplete { get; set; }
            
            private MotionHandle LoadProgressMotion { get; set; }
            public async UniTask Initialize(Memory<object> args)
            {
                await Model.Initialize(args);
                await View.Initialize(args);
            }

            public void DidEnter(Memory<object> args)
            {
                LoadFirstProgressAsync().Forget();
            }
            
            public async UniTask LoadFirstProgressAsync()
            {
                IsLoadingComplete = false;
                View.LoadDescriptionText.text = "Loading Assets...";
                UpdateProgress(0.2f);
                // Loading assets
                // await PoolManager.Instance.FirstInitPool();
                View.LoadDescriptionText.text = "Loading UI...";
                UpdateProgress(0.9f);
                await DoneFirstLoading();
                UpdateProgress(1);
                await WaitLoadingProgressCompleteAsync(View.MainView.GetCancellationTokenOnDestroy());
                IsLoadingComplete = true;
                await CloseActivity();

            }

            public async UniTask CloseActivity()
            {
                await ActivityContainer.Find(ContainerKey.Activities)
                    .HideAsync(nameof(ActivityLoadingFirstGame));
            }
            
            private void UpdateProgress(float targetProgress)
            {
                LoadProgressMotion.TryCancel();
                float currentProgressClamped = LoadProgress.CurrentValue;
                LoadProgressMotion = LMotion
                    .Create(currentProgressClamped, targetProgress, targetProgress - currentProgressClamped)
                    .Bind(OnUpdateProgress)
                    .AddTo(View.MainView);
            }
            
            private void OnUpdateProgress(float progress)
            {
                LoadProgress.Value = progress;
                View.Slider.value = Mathf.Lerp(0.1f, 1f, progress);
                View.TxtLoading.text = $"{(int)(progress * 100)}%";
            }
            
            private async UniTask DoneFirstLoading()
            {
                await LoadSceneAsync(SceneName.InGame);
                if (GameManager.Instance.TryLoadLevelPlaying())
                {
                    await GameManager.Instance.LoadCurrentLevel();
                    await UIManager.Instance.OpenScreenAsync<ScreenInGame>();
                }
                else
                {
                    await UIManager.Instance.OpenScreenAsync<ScreenMainMenu>();
                }
                
            }
            
            private async UniTask WaitLoadingProgressCompleteAsync(CancellationToken ct)
            {
                if (LoadProgress.CurrentValue >= 0.99f) return;
                await UniTask.WaitUntil(IsLoadingProgressComplete, cancellationToken: ct);
            }
            private bool IsLoadingProgressComplete()
            {
                return LoadProgress.CurrentValue >= 0.99f;
            }
            
            public async UniTask LoadSceneAsync(string sceneName)
            {
                var ct = View.MainView.GetCancellationTokenOnDestroy();
    
                try
                {
                    AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
                    operation.allowSceneActivation = true;
                    await operation.ToUniTask(cancellationToken: ct);
                }
                catch (OperationCanceledException)
                {
                    Debug.LogWarning($"LoadSceneAsync cancelled: {sceneName}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"LoadSceneAsync error: {sceneName} - {e}");
                }
            }
        }
    }
}