using System;
using Cysharp.Threading.Tasks;
using TW.UGUI.MVPPattern;
using UnityEngine;
using R3;
using Sirenix.OdinInspector;
using TW.UGUI.Core.Modals;
using Core.Manager;
using UnityEngine.UI;
using TMPro;
using CoreData;
using Manager;
using TW.UGUI.Core.Views;
using Core.UI.Screens;
using LitMotion;

namespace Core.UI.Modals
{
    public class ModalFailByIce : Modal
    {
        [field: SerializeField] public ModalFailByIceContext.UIPresenter UIPresenter { get; private set; }

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
    public class ModalFailByIceContext
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

            public int ReviveCoin { get; private set; }

            public UniTask Initialize(Memory<object> args)
            {
                ReviveCoin = LevelManager.Instance.GetReviveCoin();
                Debug.Log(ReviveCoin);
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

            [field: SerializeField] public Button ButtonKeepPlaying { get; private set; }
            [field: SerializeField] public Button ButtonClose { get; private set; }
            [field: SerializeField] public TextMeshProUGUI reviveCoinTxt { get; private set; }

            [field: SerializeField] public HoldToPreviewButton holdToPreviewButton { get; private set; }

            public UniTask Initialize(Memory<object> args)
            {
                return UniTask.CompletedTask;
            }

            public void SetReviveCoin(int reviveCoin)
            {
                reviveCoinTxt.text = $"{reviveCoin}";
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
                View.SetReviveCoin(Model.ReviveCoin);

                View.ButtonKeepPlaying.SetOnClickDestination(OnClickButtonKeepPlaying);
                View.ButtonClose.SetOnClickDestination(OnClickButtonClose);

                AudioManager.Instance.PlaySoundFx(AudioKey.SfxLose);

                View.holdToPreviewButton.OnHoldStart = HideUI;
                View.holdToPreviewButton.OnHoldEnd = ShowUI;
            }

            private async UniTask OnClickButtonKeepPlaying()
            {
                bool isEnough = InGameDataManager.Instance.InGameData.ResourceData.IsEnoughResourceValue(ResourceType.Money, Model.ReviveCoin);
                // bool isEnough = true;
                if (!isEnough)
                {
                    //LevelManager.Instance.SetPause(true);
                    ViewOptions viewOptions = new ViewOptions(nameof(ModalShopInGame));
                    await ModalContainer.Find(ContainerKey.Modals).PushAsync(viewOptions, true);
                }
                else
                {
                    PlayerResourceManager.Instance.SubResourceValue(ResourceType.Money, Model.ReviveCoin, ResourcePos.ingame.ToString(), ResourcePos.revive.ToString());
                    // LevelManager.Instance.IncreaseReviveCount();
                    await ContinueSuccess();
                }
            }

            private async UniTask ContinueSuccess()
            {
                await ModalContainer.Find(ContainerKey.Modals).PopAsync(true);
                await LevelManager.Instance.CurrentLevel.Revive();
                TimeIngameManager.Instance.ResetTimeDelayLose();
                GameManager.Instance.SetState(GameState.InGame);
                LevelManager.Instance.SetPause(false);
                ScreenInGameContext.Events.ShowBotPanel?.Invoke();
                // GameGlobalEvent.CheckWinCondition?.Invoke(0);
            }

            private async UniTask OnClickButtonClose()
            {
                ViewOptions modalConfirmLose = new ViewOptions(nameof(ModalConfirmLose));
                await ModalContainer.Find(ContainerKey.Modals).PopAsync(true);
                await ModalContainer.Find(ContainerKey.Modals).PushAsync(modalConfirmLose, true);
            }

            [Button]
            private async UniTask HideUI()
            {
                var container = ModalContainer.Find(ContainerKey.Modals);
                var backdrop = container.Backdrops[^1].View.GetComponent<CanvasGroup>();
                await LMotion.Create(1f, 0f, 0.3f)
                    .WithEase(Ease.OutQuad)
                    .Bind(t =>
                    {
                        View.MainView.alpha = t;
                        backdrop.alpha = t;


                    }).AddTo(View.MainView);
            }

            private async UniTask ShowUI()
            {
                var container = ModalContainer.Find(ContainerKey.Modals);
                var backdrop = container.Backdrops[^1].View.GetComponent<CanvasGroup>();
                await LMotion.Create(0f, 1f, 0.3f)
                    .WithEase(Ease.OutQuad)
                    .Bind(t =>
                    {
                        View.MainView.alpha = t;
                        backdrop.alpha = t;


                    }).AddTo(View.MainView);
            }

            public UniTask Cleanup(Memory<object> args)
            {
                View.holdToPreviewButton.OnHoldStart = null;
                View.holdToPreviewButton.OnHoldEnd = null;
                return UniTask.CompletedTask;
            }
        }
    }
}