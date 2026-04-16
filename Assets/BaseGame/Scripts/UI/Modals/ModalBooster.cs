using System;
using Cysharp.Threading.Tasks;
using TW.UGUI.MVPPattern;
using UnityEngine;
using R3;
using Sirenix.OdinInspector;
using TMPro;
using TW.UGUI.Core.Modals;
using TW.UGUI.Core.Views;
using UnityEngine.UI;

namespace Core.UI.Modals
{
    public class ModalBooster : Modal
    {
        [field: SerializeField] public ModalBoosterContext.UIPresenter UIPresenter { get; private set; }

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
    public class ModalBoosterContext
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
            [field: SerializeField] public BoosterSelector BoosterSelector { get; private set; }
            [field: SerializeField] public BoosterInfoData BoosterData { get; private set; }

            public UniTask Initialize(Memory<object> args)
            {
                if (args.Length > 0)
                {
                    BoosterSelector = (BoosterSelector)args.Span[0];
                    BoosterData = ItemGlobalConfig.Instance.GetBoosterData(BoosterSelector.BoosterType);
                }
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
            [field: SerializeField] public TextMeshProUGUI BoosterNameTxt { get; private set; }
            [field: SerializeField] public TextMeshProUGUI BoosterDesTxt { get; private set; }
            [field: SerializeField] public TextMeshProUGUI BoosterPriceTxt { get; private set; }
            [field: SerializeField] public Image BoosterIcon { get; private set; }
            [field: SerializeField] public Button CoinBuyBoosterBtn { get; private set; }
            [field: SerializeField] public Button AdsBuyBoosterBtn { get; private set; }
            [field: SerializeField] public Button ExitBtn1 { get; private set; }
            [field: SerializeField] public Button ExitBtn2 { get; private set; }
            public UniTask Initialize(Memory<object> args)
            {
                if (args.Length > 0)
                {
                    BoosterSelector boosterSelector = (BoosterSelector)args.Span[0];
                    BoosterInfoData boosterData = ItemGlobalConfig.Instance.GetBoosterData(boosterSelector.BoosterType);
                    BoosterNameTxt.text = boosterData.Name;
                    BoosterDesTxt.text = boosterData.Description;
                    BoosterPriceTxt.text = $"{boosterData.PriceX3}";
                    BoosterIcon.sprite = boosterData.Sprite;
                    //CoinBuyBoosterBtn.interactable = InGameDataManager.Instance.InGameData.ResourceData.IsEnoughResourceValue(ResourceType.Currency, (int)(CurrencyType.Money), boosterData.Price);
                    // Debug.Log(boosterData.RwLimit > GameplaySaveData.Instance.GetNumbBoosterRwUsed(boosterSelector.BoosterType));
                }
                return UniTask.CompletedTask;
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
                View.ExitBtn1.SetOnClickDestination(OnButtonCloseClick);
                // View.ExitBtn2.SetOnClickDestination(OnButtonCloseClick);
                View.CoinBuyBoosterBtn.SetOnClickDestination(OnButtonBuyBoosterClick);
                View.AdsBuyBoosterBtn.SetOnClickDestination(OnAdsButtonBuyBoosterClick);
                LevelManager.Instance.SetPause(true);
            }

            private async UniTask OnButtonBuyBoosterClick()
            {
                int price = Model.BoosterData.PriceX3;
                bool isEnough = InGameDataManager.Instance.InGameData.ResourceData.IsEnoughResourceValue(ResourceType.Money, price);
                if (!isEnough)
                {
                    //LevelManager.Instance.SetPause(true);
                    ViewOptions viewOptions = new ViewOptions(nameof(ModalShopInGame));
                    await ModalContainer.Find(ContainerKey.Modals).PushAsync(viewOptions, true);
                }
                else
                {
                    PlayerResourceManager.Instance.AddResourceValue(Model.BoosterSelector.BoosterType, 3, ResourcePos.ingame.ToString(), ResourcePos.coin.ToString());
                    var spendType = Model.BoosterSelector.BoosterType - ResourceType.AddSlot + ResourcePos.booster_addslot;
                    PlayerResourceManager.Instance.SubResourceValue(ResourceType.Money, price, ResourcePos.ingame.ToString(), spendType.ToString());
                    await OnButtonCloseClick();
                    //Model.BoosterSelector.SelectBooster("modal_booster");
                }
            }

            private async UniTask OnAdsButtonBuyBoosterClick()
            {
                if (GameManager.Instance.BuildType == BuildType.Cheat)
                {
                    await OnAdsButtonBuyBoosterSuccess();
                    return;
                }

                View.MainView.interactable = false;
                // ADsManager.ShowRewarded(ResourcePos.ingame, ResourcePos.booster_undo + (int)(Model.BoosterSelector.BoosterType - ResourceType.Undo),
                //     () => OnAdsButtonBuyBoosterSuccess().Forget(), OnAdsButtonBuyBoosterFail);

                ADSAction.OnAdsRewardEvent?.Invoke(ResourcePos.ingame.ToString(), ResourcePos.buy_booster.ToString(), () => OnAdsButtonBuyBoosterSuccess().Forget(), OnAdsButtonBuyBoosterFail);

                // InGameAdsController.EventShowAdsReward?.Invoke($"ingame", $"ingame_booster_{Model.BoosterData.BoosterType}", () => OnAdsButtonBuyBoosterSuccess().Forget(), OnAdsButtonBuyBoosterFail);
                // await OnAdsButtonBuyBoosterSuccess();
            }

            private async UniTask OnAdsButtonBuyBoosterSuccess()
            {
                PlayerResourceManager.Instance.AddResourceValue(Model.BoosterSelector.BoosterType, 1, ResourcePos.ingame.ToString(), ResourcePos.ads.ToString());
                await OnButtonCloseClick();
                //Model.BoosterSelector.SelectBooster("modal_booster");
            }

            private void OnAdsButtonBuyBoosterFail()
            {
                View.MainView.interactable = true;
            }

            private async UniTask OnButtonCloseClick()
            {
                if (GameManager.Instance.IsState(GameState.InGame))
                {
                    LevelManager.Instance.SetPause(false);
                }
                await ModalContainer.Find(ContainerKey.Modals).PopAsync(true);
            }
        }
    }
}