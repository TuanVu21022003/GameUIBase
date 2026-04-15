using System;
using System.Collections.Generic;
using Core.GamePlay;
using Core.GamePlay.GlobalEnum;
using Core.Manager;
using Core.UI.Activities;
using Cysharp.Threading.Tasks;
using TW.UGUI.MVPPattern;
using UnityEngine;
using R3;
using Sirenix.OdinInspector;
using TW.UGUI.Core.Screens;
using Screen = TW.UGUI.Core.Screens.Screen;
using Core.UI.Modals;
using CoreData;
using Cysharp.Text;
using LitMotion;
using Manager;
using MoreMountains.Feedbacks;
using TMPro;
using TW.UGUI.Core.Activities;
using TW.UGUI.Core.Modals;
using TW.UGUI.Core.Views;
using UnityEngine.UI;
using R3;

namespace Core.UI.Screens
{
    public class ScreenInGame : Screen
    {
        [field: SerializeField] public ScreenInGameContext.UIPresenter UIPresenter { get; private set; }

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
    public class ScreenInGameContext
    {
        public static class Events
        {
            public static Action<ResourceType> ShowBoosterHint { get; set; }
            public static Action HideBoosterHint { get; set; }
            public static Action CheckShowAlertDifficult { get; set; }
            public static Action<ResourceType> ClaimBooster { get; set; }
            public static Action<ResourceType> OnUnlockWaitClaimBooster { get; set; }
            public static Action<string> ShowNotification { get; set; }
            public static Action ShowBackGroundBlack { get; set; }
            public static Action HideBackGroundBlack { get; set; }
            public static Action LoadLevel { get; set; }
            public static Action<bool> BoosterGroupSetActive { get; set; }
            public static Action<int> UpdateTxtWarning {get; set; }
            public static Action<bool> SetActiveWarning { get; set; }
            public static Action ShowBotPanel { get; set; }
            public static Action HideBotPanel { get; set; }
            public static Action<bool> SetActiveFirstTut { get; set; }
        }

        [HideLabel]
        [Serializable]
        public class UIModel : IAModel
        {
            [field: Title(nameof(UIModel))]
            [field: SerializeField]
            public SerializableReactiveProperty<int> SampleValue { get; private set; }

            [field: SerializeField] public Reactive<int> CurLevel { get; private set; } = new(0);

            public UniTask Initialize(Memory<object> args)
            {
                CurLevel = GameManager.Instance.Level;
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

            [field: SerializeField] public Button ButtonPause { get; private set; }
            [field: SerializeField] public TextMeshProUGUI TextLevel { get; private set; }
            [field: SerializeField] public Button BtnSpeedUp {get; private set; }
            [field: SerializeField] public TextMeshProUGUI TextSpeedLevel { get; private set; }
            [field: SerializeField] public Transform BoosterBarPos { get; private set; }
            [field: SerializeField] public Transform BoosterBarPosHasBanner { get; private set; }
            [field: SerializeField] public Transform BoosterBarPosNotBanner { get; private set; }
            [field: SerializeField] public CanvasGroup BoosterBG { get; private set; }
            [field: SerializeField] public CanvasGroup BoosterCG { get; private set; }
            [field: SerializeField] public AnimationCurve CurveBoosterBarMoveUp { get; private set; }
            [field: SerializeField] public List<BoosterSelector> BoosterSelectors { get; private set; }
            [field: SerializeField] public AlertLevelDifficult AlertLevelDifficult { get; private set; }
            [field: SerializeField] public Transform BoosterClaimRoot { get; private set; }
            [field: SerializeField] public NotificationItemUI NotificationItemUIPrefab { get; private set; }
            [field: SerializeField] public Transform NotificationContainer { get; private set; }
            [field: SerializeField] public Image BackGroundBlackImage { get; private set; }
            [field: SerializeField] public GameObject BoosterGroup { get; private set; }
            public List<NotificationItemUI> NotificationItems { get; private set; }
            
            
            [field: SerializeField] public GameObject PanelWarning { get; private set; }
            [field: SerializeField] public MMF_Player AnimWarning { get; private set; }
            [field: SerializeField] public TextMeshProUGUI TxtWarning { get; private set; }
            
            [field: SerializeField] public MMF_Player AnimShowBotPanel { get; private set; } = new();
            [field: SerializeField] public MMF_Player AnimHideBotPanel { get; private set; } = new();
            [field: SerializeField] public MMF_Player AnimShowTopPanel { get; private set; } = new();
            [field: SerializeField] public MMF_Player AnimHideTopPanel  { get; private set; } = new();
            
            [field: SerializeField] public GameObject GOTxtFirstTut { get; private set; }
            
            private MotionHandle blackGroundMotionHandle;

            public NotificationItemUI GetNotificationItem()
            {
                if (NotificationItems == null)
                {
                    NotificationItems = new List<NotificationItemUI>();
                }

                foreach (var item in NotificationItems)
                {
                    if (!item.gameObject.activeSelf)
                    {
                        return item;
                    }
                }

                var newItem = GameObject.Instantiate(NotificationItemUIPrefab, NotificationContainer);
                newItem.gameObject.SetActive(false);
                NotificationItems.Add(newItem);
                return newItem;
            }

            public UniTask Initialize(Memory<object> args)
            {
                // BoosterBarPos.position = (ShopData.Instance.RemoveADs ? BoosterBarPosNotBanner : BoosterBarPosHasBanner)
                //     .position;
                BoosterBarPos.position = BoosterBarPosNotBanner.position;
                BackGroundBlackImage.gameObject.SetActive(false);
                
                return UniTask.CompletedTask;
            }

            public void ClaimBooster(ResourceType boosterType)
            {
                ItemGlobalConfig.Instance.ShowBoosterUnlock(boosterType);
                foreach (var selector in BoosterSelectors)
                {
                    if (selector.BoosterType == boosterType)
                    {
                        selector.BoosterCG.alpha = 1;
                        ShowBackGroundBlack();
                        selector.GetBoosterOnTutorial(BoosterClaimRoot.position).Forget();
                        break;
                    }
                }
            }

            public void OnUnlockWaitClaimBooster(ResourceType boosterType)
            {
                foreach (var selector in BoosterSelectors)
                {
                    if (selector.BoosterType == boosterType)
                    {
                        selector.OnUnlockClaim();
                        break;
                    }
                }
            }

            public void ShowNotification(string message)
            {
                var notificationItem = GetNotificationItem();
                notificationItem.ShowNotification(message);
            }

            public void ShowBackGroundBlack()
            {
                BackGroundBlackImage.color = new Color(0, 0, 0, 0);
                BackGroundBlackImage.gameObject.SetActive(true);
                if (blackGroundMotionHandle != null && blackGroundMotionHandle.IsActive())
                {
                    blackGroundMotionHandle.TryCancel();
                }

                blackGroundMotionHandle = LMotion.Create(0f, 0.8f, 0.3f)
                    .WithEase(Ease.OutQuad)
                    .Bind(x => BackGroundBlackImage.color = new Color(0, 0, 0, x));
            }

            public void HideBackGroundBlack()
            {
                if (blackGroundMotionHandle != null && blackGroundMotionHandle.IsActive())
                {
                    blackGroundMotionHandle.TryCancel();
                }

                blackGroundMotionHandle = LMotion.Create(BackGroundBlackImage.color.a, 0f, 0.3f)
                    .WithEase(Ease.InQuad)
                    .WithOnComplete(() => BackGroundBlackImage.gameObject.SetActive(false))
                    .Bind(x => BackGroundBlackImage.color = new Color(0, 0, 0, x));
            }

            public void BoosterGroupSetActive(bool isActive)
            {
                BoosterGroup.SetActive(isActive);
            }

            public void UpdateTextWarning(int obj)
            {
                TxtWarning.text = obj.ToString();
            }

            public void SetActivePanelWarning(bool isActive)
            {
                PanelWarning.gameObject.SetActive(isActive);
                if (isActive)
                {
                    AnimWarning.PlayFeedbacks();
                }
                else
                {
                    AnimWarning.StopFeedbacks();
                }
            }

            public void ShowInteractPanel()
            {
                AnimHideBotPanel.StopFeedbacks();
                AnimShowBotPanel.StopFeedbacks();
                AnimHideTopPanel.StopFeedbacks();
                AnimShowTopPanel.StopFeedbacks();
                AnimShowBotPanel.PlayFeedbacks();
                AnimShowTopPanel.PlayFeedbacks();
            }

            public void HideInteractPanel()
            {
                AnimShowBotPanel.StopFeedbacks();
                AnimHideBotPanel.StopFeedbacks();
                AnimHideTopPanel.StopFeedbacks();
                AnimShowTopPanel.StopFeedbacks();
                AnimHideBotPanel.PlayFeedbacks();
                AnimHideTopPanel.PlayFeedbacks();
            }

            public void SetActiveFirstTut(bool isActive)
            {
                GOTxtFirstTut.SetActive(isActive);
            }

            public void OnLevelChanged(int level)
            {
                TextLevel.SetText($"Level {level}");
            }
        }

        [HideLabel]
        [Serializable]
        public class UIPresenter : IAPresenter, IScreenLifecycleEventSimple
        {
            [field: SerializeField] public UIModel Model { get; private set; } = new();
            [field: SerializeField] public UIView View { get; set; } = new();
           

            public async UniTask Initialize(Memory<object> args)
            {
                await Model.Initialize(args);
                await View.Initialize(args);
                
                Model.CurLevel.Subscribe(View.OnLevelChanged).AddTo(View.MainView);
                
                AudioManager.Instance.ChangeMusic(AudioKey.BgGamePlay, 0.5f);
                View.ButtonPause.SetOnClickDestination(OnClickButtonPause);
                //View.TextLevel.SetTextFormat("Level {0}", GameManager.Instance.Level.Value);
                View.BtnSpeedUp.SetOnClickDestination(OnClickBtnSpeddUp);
                // if (LevelManager.Instance.CurrentLevel.LevelData.Level > 0)
                // {
                //     View.TextLevel.SetTextFormat("Level {0}", LevelManager.Instance.CurrentLevel.LevelData.Level);
                // }
                // else
                // {
                //     View.TextLevel.SetText("");
                // }
                //
                // LevelManager.Instance.MovesTurn.Subscribe(OnMovesTurnChanged).AddTo(View.MainView);
                Events.ShowBoosterHint += ShowBoosterHint;
                Events.HideBoosterHint += HideBoosterHint;
                Events.CheckShowAlertDifficult += CheckShowAlertDifficult;
                Events.ClaimBooster = View.ClaimBooster;
                Events.OnUnlockWaitClaimBooster = View.OnUnlockWaitClaimBooster;
                Events.ShowNotification = View.ShowNotification;
                Events.ShowBackGroundBlack = View.ShowBackGroundBlack;
                Events.HideBackGroundBlack = View.HideBackGroundBlack;
                Events.LoadLevel = LoadLevel;
                Events.BoosterGroupSetActive = View.BoosterGroupSetActive;
                Events.UpdateTxtWarning = View.UpdateTextWarning;
                Events.SetActiveWarning = View.SetActivePanelWarning;
                Events.ShowBotPanel = View.ShowInteractPanel;
                Events.HideBotPanel = View.HideInteractPanel;
                Events.SetActiveFirstTut = View.SetActiveFirstTut;
                
                GameManager.Instance.IsPauseGame.Subscribe(CheckPauseGame).AddTo(View.MainView);
                SettingData.Instance.SpeedLevel.Subscribe(OnSpeedLevelChanged).AddTo(View.MainView);
                
                GameGlobalEvent.OnPlayerSelectBoosterMagnet += OnPlayerSelectBoosterMagnet;
                GameGlobalEvent.OnPlayerSelectBoosterBalloon += OnPlayerSelectBoosterBalloon;
                GameGlobalEvent.OnCancelBoosterBalloon += OnCancelBooster;
                GameGlobalEvent.OnCancelBoosterMagnet += OnCancelBooster;
                GameGlobalEvent.OnPlayerCameraMoveToInit += OnCompleteBoosterMagnet;
                GameGlobalEvent.OnPlayerUseBoosterBalloonCompleted += OnPlayerUseBoosterBalloonCompleted;
                
                
                LoadLevel();
            }
            private void OnClickBtnSpeddUp()
            {
                int nextLevel = DefaultGlobalConfig.Instance.TryGetRotationSpeedLevel(SettingData.Instance.SpeedLevel.Value);
                SettingData.Instance.SpeedLevel.Value = nextLevel;
                TimeIngameManager.Instance.SetupSpeedImmediately(nextLevel);
            }

            private void OnSpeedLevelChanged(int speedLevel)
            {
                View.TextSpeedLevel.SetTextFormat(speedLevel.ToString()+"x",MyCacheUI.textFormat);
            }

            public UniTask Cleanup(Memory<object> args)
            {
                Debug.Log("clean up");
                Events.ShowBoosterHint -= ShowBoosterHint;
                Events.HideBoosterHint -= HideBoosterHint;
                Events.CheckShowAlertDifficult -= CheckShowAlertDifficult;
                Events.ClaimBooster = null;
                Events.OnUnlockWaitClaimBooster = null;
                Events.ShowNotification = null;
                Events.ShowBackGroundBlack = null;
                Events.HideBackGroundBlack = null;
                Events.LoadLevel = null;
                Events.BoosterGroupSetActive = null;
                Events.UpdateTxtWarning = null;
                Events.SetActiveWarning = null;
                Events.ShowBotPanel = null;
                Events.HideBotPanel = null;
                Events.SetActiveFirstTut = null;
                
                GameGlobalEvent.OnPlayerSelectBoosterMagnet -= OnPlayerSelectBoosterMagnet;
                GameGlobalEvent.OnPlayerSelectBoosterBalloon -= OnPlayerSelectBoosterBalloon;
                GameGlobalEvent.OnCancelBoosterBalloon -= OnCancelBooster;
                GameGlobalEvent.OnCancelBoosterMagnet -= OnCancelBooster;
                GameGlobalEvent.OnPlayerCameraMoveToInit -= OnCancelBooster;
                GameGlobalEvent.OnPlayerUseBoosterBalloonCompleted -= OnPlayerUseBoosterBalloonCompleted;
                return UniTask.CompletedTask;
            }

            private void CheckShowAlertDifficult()
            {
                if (LevelManager.Instance.LevelConfig.LevelType == LevelDifficulty.Hard ||
                    LevelManager.Instance.LevelConfig.LevelType == LevelDifficulty.SuperHard)
                {
                    Debug.Log("LevelManager.Instance.LevelConfig.LevelType" + LevelManager.Instance.LevelConfig.LevelType);
                    View.AlertLevelDifficult.OnInit(LevelManager.Instance.LevelConfig.LevelType).TryStartMoveAnimation().Forget();
                }
                // else
                // {
                //     View.AlertLevelDifficult.SetActive(false);
                // }
                // if (LevelManager.Instance.LevelConfig.LevelType == LevelDifficulty.SuperHard)
                // {
                //     View.AlertLevelDifficult.OnInit(LevelManager.Instance.LevelConfig.LevelType).TryStartMoveAnimation().Forget();
                // }
                // else
                // {
                //     View.AlertLevelDifficult.SetActive(false);
                // }
                // View.AlertLevelDifficult.SetActive(false);
            }

            private void OnMovesTurnChanged(int moves)
            {
                
            }

            private async UniTask OnClickButtonPause()
            {
                ViewOptions viewOptions = new ViewOptions(nameof(ModalPause));
                await ModalContainer.Find(ContainerKey.Modals).PushAsync(viewOptions);
            }

            public void ShowBoosterHint(ResourceType boosterType)
            {
                BoosterSelector selector = GetBoosterSelector(boosterType);
                if (selector != null)
                {
                    selector.SetSuggestion(true);
                }
            }

            public void HideBoosterHint()
            {
                for (int i = 0; i < View.BoosterSelectors.Count; i++)
                {
                    View.BoosterSelectors[i].SetSuggestion(false);
                }
            }

            private BoosterSelector GetBoosterSelector(ResourceType boosterType)
            {
                for (int i = 0; i < View.BoosterSelectors.Count; i++)
                {
                    if (View.BoosterSelectors[i].BoosterType == boosterType)
                    {
                        return View.BoosterSelectors[i];
                    }
                }

                return null;
            }

            public void HideBooster(ResourceType boosterType)
            {
                BoosterSelector selector = GetBoosterSelector(boosterType);
                if (selector != null)
                {
                    selector.BoosterCG.alpha = 0;
                }
            }

            public void LoadLevel()
            {
                View.ShowInteractPanel();
                InitBoosterSelectors();
                if (!TutorialManager.Instance.TutData.IsPassFirstTut)
                {
                    View.SetActiveFirstTut(true);
                }
            }

            private void InitBoosterSelectors()
            {
                for (int i = 0; i < View.BoosterSelectors.Count; i++)
                {
                    BoosterSelector boosterSelector = View.BoosterSelectors[i];
                    boosterSelector.OnChangeLevel(GameManager.Instance.Level.Value);
                }
            }
            
            void CheckPauseGame(bool pause)
            {
                // Debug.Log("Pause Game" + pause);
                if (!pause)
                {
                    View.BoosterBG.alpha = 1;
                }

                if (pause)
                {
                    View.BoosterBG.alpha = 0.5f;
                }
            }

            private void OnPlayerSelectBoosterMagnet()
            {
                OpenTipBooster(ResourceType.Laser).Forget();
            }
            
            private void OnPlayerSelectBoosterBalloon()
            {
                OpenTipBooster(ResourceType.Propeller).Forget();
            }

            private async UniTask OpenTipBooster(ResourceType boosterType)
            {
                View.BoosterCG.interactable = false;
                ViewOptions activityTipBooster = new ViewOptions(nameof(ActivityTipBooster));
                //await EffectBoosterHide();
                await ActivityContainer.Find(ContainerKey.Activities).ShowAsync(activityTipBooster, boosterType);
                
            }
            
            private async UniTask CloseTipBooster()
            {
                await ActivityContainer.Find(ContainerKey.Activities).HideAsync(nameof(ActivityTipBooster));
                //await EffectBoosterShow();
                View.BoosterCG.interactable = true;
            }

            private void OnCancelBooster()
            {
                CloseTipBooster().Forget();
            }
            
            private void OnCompleteBoosterMagnet()
            {
                CloseTipBooster().Forget();
            }
            
            

            private async UniTask EffectBoosterHide()
            {
                Vector3 startPosition = View.BoosterBarPos.transform.position;
                    Vector3 endPosition = startPosition + new Vector3(0, -250, 0);
                await LMotion.Create(startPosition, endPosition, 0.3f)
                    .WithEase(Ease.InQuad)
                    .Bind(x => View.BoosterBarPos.transform.position = x)
                    .AddTo(View.MainView);
            }

            private async UniTask EffectBoosterShow()
            {
                Debug.Log("EffectBoosterShow");
                Vector3 startPosition = View.BoosterBarPos.transform.position;
                Vector3 endPosition = View.BoosterBarPosNotBanner.transform.position;
                await LMotion.Create(startPosition, endPosition, 0.3f)
                    .WithEase(View.CurveBoosterBarMoveUp)
                    .Bind(x => View.BoosterBarPos.transform.position = x)
                    .AddTo(View.MainView);
            }
            
            private void OnPlayerUseBoosterBalloonCompleted(Gun gun)
            {
                CloseTipBooster().Forget();
            }

            
        }
    }
}