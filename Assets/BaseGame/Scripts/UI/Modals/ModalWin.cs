using System;
using System.Collections.Generic;
using Core.Manager;
using Core.UI.Activities;
using Core.UI.Screens;
using CoreData;
using Cysharp.Threading.Tasks;
using LitMotion;
using Manager;
using TW.UGUI.MVPPattern;
using UnityEngine;
using R3;
using Sirenix.OdinInspector;
using TMPro;
using TW.UGUI.Core.Activities;
using TW.UGUI.Core.Modals;
using TW.UGUI.Core.Screens;
using TW.UGUI.Core.Views;
using UnityEngine.UI;

namespace Core.UI.Modals
{
    public class ModalWin : Modal
    {
        [field: SerializeField] public ModalWinContext.UIPresenter UIPresenter { get; private set; }

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
    public class ModalWinContext
    {
        public static class Events
        {
            public static Action SampleEvent { get; set; }
        }

        [HideLabel]
        [Serializable]
        public class UIModel : IAModel
        {
            [field: Title(nameof(UIModel))]
            [field: SerializeField]
            public SerializableReactiveProperty<int> SampleValue { get; private set; }

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
            [field: SerializeField] public Button ButtonClaimWithAds {get; private set;}
            [field: SerializeField] public Button ButtonClaim {get; private set;}
            [field: SerializeField] public List<StarGroup> StarGroups {get; private set;}
            [field: SerializeField] public Transform TitleCG {get; private set;}
            [field: SerializeField] public TextMeshProUGUI DefaultRewardTxt {get; private set;}
            [field: SerializeField] public TextMeshProUGUI AdsRewardTxt {get; private set;}
            [field: SerializeField] public TextMeshProUGUI LevelTxt {get; private set;}
            [field: SerializeField] public Transform GlowTF {get; private set;}
            
            [field: SerializeField] public Transform CoinGroup {get; private set;}
            [field: SerializeField] public CoinEffectUI CoinEffectUI {get; private set;}
            [field: SerializeField] public UIMoneyResource UIMoneyResource {get; private set;}
            [field: SerializeField] public Transform CoinClaimTF {get; private set;}
            [field: SerializeField] public Transform CoinClaimWithAdsTF {get; private set;}
            
            
            public MotionHandle RotateHandle;
            public UniTask Initialize(Memory<object> args)
            {
                DefaultRewardTxt.text = $"{DefaultGlobalConfig.Instance.WinGameReward}";
                AdsRewardTxt.text = $"{DefaultGlobalConfig.Instance.WinGameReward * 2}";
                LevelTxt.text = $"Level {GameManager.Instance.Level.Value - 1}";
                RunAnim().Forget();
                return UniTask.CompletedTask;
            }
            
            [Button]
            public async UniTask RunAnim()
            {
                ButtonClaimWithAds.transform.localScale = Vector3.zero;
                ButtonClaim.transform.localScale = Vector3.zero;
                TitleCG.localScale = Vector3.zero;
                CoinGroup.localScale = Vector3.zero;

                await PlayAnimTextLevel();

                await PlayAnimCoinOrFeature();
                
                await UniTask.Delay(350);
                // await GameManager.Instance.TryShowRateModal();
                if (GameManager.Instance.Level.Value > DefaultGlobalConfig.Instance.DefaultX2CoinLevel)
                {
                    ButtonClaimWithAds.gameObject.SetActive(true);
                    await LMotion.Create(0f, 1f, 0.2f).WithEase(Ease.OutBack).Bind(x => ButtonClaimWithAds.transform.localScale = x * Vector3.one).AddTo(MainView);
                    await UniTask.Delay(100, cancellationToken: MainView.GetCancellationTokenOnDestroy());
                }
                else
                {
                    ButtonClaimWithAds.gameObject.SetActive(false);
                    // ButtonClaim.transform.position = ButtonClaimWithAds.transform.position;
                }
                await LMotion.Create(0f, 1f, 0.2f).WithEase(Ease.OutBack).Bind(x => ButtonClaim.transform.localScale = x * Vector3.one).AddTo(MainView);
            }

            private async UniTask PlayAnimCoinOrFeature()
            {
                if (NewFeatureGlobalConfig.Instance.TryGetNewFeatureNextUnlock(GameManager.Instance.Level.CurrentValue,
                        out var nextUnlock))
                {
                    await PlayAnimFeature();
                }
                else
                {
                    await PlayAnimCoin();
                }
            }
            
            private async UniTask PlayAnimCoin()
            {
                await LMotion.Create(0f, 1f, 0.3f)
                    .WithEase(Ease.OutBack)
                    .Bind(t =>
                    {
                        CoinGroup.localScale = Vector3.one * t;
                    }).AddTo(MainView);
            } 
            
            private async UniTask PlayAnimFeature()
            {
                ViewOptions activityNewFatureWin = new ViewOptions(nameof(ActivityNewFeatureWin));
                await ActivityContainer.Find(ContainerKey.Activities).ShowAsync(activityNewFatureWin);
            } 
            
            private async UniTask PlayAnimTextLevel()
            {
                PlayRotateLoop();
                await LMotion.Create(0f, 1f, 0.2f)
                    .WithEase(Ease.OutBack)
                    .Bind(t =>
                    {
                        TitleCG.localScale = Vector3.one * t;
                    }).ToUniTask();
            }
            
            public void PlayRotateLoop()
            {
                RotateHandle = LMotion.Create(0, 360, 2f)
                    .WithEase(Ease.Linear)
                    .WithLoops(-1)
                    .Bind(t =>
                    {
                        GlowTF.localRotation = Quaternion.Euler(0, 0, t);
                    }).AddTo(MainView); 
            }
        }

        [HideLabel]
        [Serializable]
        public class UIPresenter : IAPresenter, IModalLifecycleEventSimple
        {
            [field: SerializeField] public UIModel Model { get; private set; } = new();
            [field: SerializeField] public UIView View { get; set; } = new();

            public async UniTask Initialize(Memory<object> args)
            {

                await Model.Initialize(args);
                await View.Initialize(args);
                View.ButtonClaim.SetOnClickDestination(OnClickButtonClaim);
                View.ButtonClaimWithAds.SetOnClickDestination(OnClickButtonClaimWithAds);
                AudioManager.Instance.PlaySoundFx(AudioKey.SfxWin);
            }
            private void OnClickButtonClaimWithAds()
            {
                View.MainView.interactable = false;
                if (GameManager.Instance.BuildType == BuildType.Cheat)
                {
                    OnClaimWithAdsSuccess();
                    return;
                }
                //ADsManager.ShowRewarded(ResourcePos.ingame, ResourcePos.ads,
                //    OnClaimWithAdsSuccess, OnClaimWithAdsFail);

                ADSAction.OnAdsRewardEvent?.Invoke(ResourcePos.ingame.ToString(), ResourcePos.ads.ToString(), OnClaimWithAdsSuccess, OnClaimWithAdsFail);

                // InGameAdsController.EventShowAdsReward?.Invoke("AdsRw_GetWinReward", OnClaimWithAdsSuccess, OnClaimWithAdsFail);

            }

            private void OnClaimWithAdsSuccess()
            {
                View.UIMoneyResource.IsAnim = true;
                PlayerResourceManager.Instance.AddResourceValue(ResourceType.Money, DefaultGlobalConfig.Instance.WinGameReward * 2, "gameplay", "win_level_x2_reward");
                OnEffectCoin(View.CoinClaimWithAdsTF.position);
            }
            private void OnClaimWithAdsFail()
            {
                View.MainView.interactable = true;
            }
            
            private void OnClickButtonClaim()
            {
                View.MainView.interactable = false;
                View.UIMoneyResource.IsAnim = true;
                PlayerResourceManager.Instance.AddResourceValue(ResourceType.Money, DefaultGlobalConfig.Instance.WinGameReward,"gameplay", "win_level");
                OnEffectCoin(View.CoinClaimTF.position);
            }

            private void OnEffectCoin(Vector3 posStart)
            {
                View.CoinEffectUI.OnInit(posStart, View.UIMoneyResource.IconTransform,
                    View.UIMoneyResource.PlayAnimation, () => DoneGetReward().Forget());
            }

            private async UniTask DoneGetReward()
            {
                if (NewFeatureGlobalConfig.Instance.TryGetNewFeatureNextUnlock(GameManager.Instance.Level.CurrentValue,
                        out var nextUnlock))
                {
                    await ActivityContainer.Find(ContainerKey.Activities).HideAsync(nameof(ActivityNewFeatureWin));
                }
                if (GameManager.Instance.Level.Value <= DefaultGlobalConfig.Instance.DefaultBackToMenuLevel)
                {
                    await LoadNextLevel();
                    // TrackerAnalyticsManager.Instance.TrackLevelStart(GameManager.Instance.Level.Value);
                }
                else
                {
                    await BackToMainMenu();
                }
                //
                // ADsManager.ShowInterstitial(ResourcePos.win_level);
                ADSAction.OnAdsInterEvent?.Invoke(ResourcePos.win_level.ToString());
            }
            
            public async UniTask BackToMainMenu()
            {
                ViewOptions activityLoading = new ViewOptions(nameof(ActivityLoading));
                Memory<object> args = new Memory<object>(new object[]
                {
                    (Func<UniTask>)(async () =>
                    {
                        LevelManager.Instance.ClearLevel().Forget();
                        await ModalContainer.Find(ContainerKey.Modals).PopAsync(true);
                        GameManager.Instance.LoadMainMenu();
                        // LevelManager.Instance.ClearLevel();
                    }),
                    null
                });
                await ActivityContainer.Find(ContainerKey.Activities).ShowAsync(activityLoading, args);
            }
            
            public async UniTask LoadNextLevel()
            {
                ViewOptions activityLoading = new ViewOptions(nameof(ActivityLoading));
                Memory<object> args = new Memory<object>(new object[]
                {
                    (Func<UniTask>)(async () =>
                    {
                        await LevelManager.Instance.ClearLevel();
                        await GameManager.Instance.LoadCurrentLevel();
                        await ModalContainer.Find(ContainerKey.Modals).PopAsync(true);
                    }),
                    null
                });
                await ActivityContainer.Find(ContainerKey.Activities).ShowAsync(activityLoading, args);
            }

            public void DidPopExit(Memory<object> args)
            {
                View.RotateHandle.Cancel();
            }
        }
    }
}