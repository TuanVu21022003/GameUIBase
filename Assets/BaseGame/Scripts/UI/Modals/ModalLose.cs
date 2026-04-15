using System;
using Core.GamePlay.GlobalEnum;
using Core.Manager;
using Core.UI.Activities;
using Core.UI.Screens;
using CoreData;
using Cysharp.Threading.Tasks;
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
using LitMotion;

namespace Core.UI.Modals
{
    public class ModalLose : Modal
    {
        [field: SerializeField] public ModalLoseContext.UIPresenter UIPresenter { get; private set; }

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
    public class ModalLoseContext
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
            public int ReviveSlot { get; private set; }

            public UniTask Initialize(Memory<object> args)
            {
                ReviveCoin = LevelManager.Instance.GetReviveCoin();
                ReviveSlot = DefaultGlobalConfig.Instance.ReviveSlot;
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
            [field: SerializeField] public Button ButtonAds { get; private set; }
            [field: SerializeField] public Button ButtonClose { get; private set; }
            [field: SerializeField] public TextMeshProUGUI reviveCoinTxt { get; private set; }
            [field: SerializeField] public TextMeshProUGUI reviveMovesTxt1 { get; private set; }
            [field: SerializeField] public TextMeshProUGUI reviveMovesTxt2 { get; private set; }
            [field: SerializeField] public HoldToPreviewButton holdToPreviewButton { get; private set; }
            // [field: SerializeField] public TextMeshProUGUI reviveMovesAdsTxt { get; private set; }
            // [field: SerializeField] public List<UIShopPack> UiShopPacks {get; private set;}
            // [field: SerializeField] public List<TextMeshProUGUI> UiShopReviveTime {get; private set;}
            public UniTask Initialize(Memory<object> args)
            {

                // reviveCoinTxt.text = $"{DefaultGlobalConfig.Instance.ReviveCoin}";
                // reviveMovesTxt.text = $"Add {DefaultGlobalConfig.Instance.ReviveMovesTurnCoin} moves to keep playing!";
                return UniTask.CompletedTask;
            }
            public void SetReviveMove(int reviveSlot)
            {
                reviveMovesTxt1.text = $"Add {reviveSlot} slot to keep playing!";
                reviveMovesTxt2.text = $"+{reviveSlot}";
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
                View.SetReviveMove(Model.ReviveSlot);

                View.ButtonKeepPlaying.SetOnClickDestination(OnClickButtonKeepPlaying);
                View.ButtonAds.SetOnClickDestination(OnClickButtonAds);
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
                    await ContinueSuccess(Model.ReviveSlot);
                }
            }

            private async UniTask OnClickButtonAds()
            {
                if (GameManager.Instance.BuildType == BuildType.Cheat)
                {
                    await BuyMovesWithAdsSuccess();
                    return;
                }
                // ADsManager.ShowRewarded(ResourcePos.ingame, ResourcePos.revive,
                //     () => BuyMovesWithAdsSuccess().Forget(), BuyMovesWithAdsFail);
            }

            private async UniTask BuyMovesWithAdsSuccess()
            {
                // await ContinueSuccess(DefaultGlobalConfig.Instance.ReviveMovesTurnAds);
            }
            void BuyMovesWithAdsFail()
            {
            }

            private async UniTask ContinueSuccess(int extraSlot = 0)
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

            // void InitShopPack()
            // {
            //     for (var i = 0; i < View.UiShopPacks.Count; i++)
            //     {
            //         View.UiShopPacks[i].Init(() => ContinueSuccess().Forget(), false);
            //     }
            // }

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