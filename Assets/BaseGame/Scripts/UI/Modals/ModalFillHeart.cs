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
    public class ModalFillHeart : Modal
    {
        [field: SerializeField] public ModalFillHeartContext.UIPresenter UIPresenter { get; private set; }

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
    public class ModalFillHeartContext
    {
        public static class Events
        {
            public static Action SampleEvent { get; set; }
            public static Action FullHeartEvent { get; set; }
        }

        [HideLabel]
        [Serializable]
        public class UIModel : IAModel
        {
            [field: Title(nameof(UIModel))]
            [field: SerializeField]
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
            [field: SerializeField] public TextMeshProUGUI CoinPriceTxt { get; set; }
            [field: SerializeField] public TextMeshProUGUI HeartAddAdsTxt { get; set; }
            [field: SerializeField] public TextMeshProUGUI HeartAddCoinTxt { get; set; }
            [field: SerializeField] public GameObject NotEnoughLifeObj { get; set; }
            [field: SerializeField] public Button BuyHeartWithAdsBtn {get; set;}
            [field: SerializeField] public Button BuyHeartWithCoinBtn {get; set;}
            [field: SerializeField] public Button BuyClose {get; set;}
            [field: SerializeField] public TextMeshProUGUI TxtStateHeart {get; set;}
            [field: SerializeField] public UIMoneyResource UIMoneyResource {get; set;}
            [field: SerializeField] public Transform PosMoneyHome {get; set;}
            [field: SerializeField] public Transform PosMoneyInGame {get; set;}

            public int MaxLife => HeartGlobalConfig.Instance.MaxHeart;
            
            public UniTask Initialize(Memory<object> args)
            {
                // BuyHeartWithCoinBtn.SetInteractable(InGameDataManager.Instance.InGameData.ResourceData.IsEnoughResourceValue(ResourceType.Money, DefaultGlobalConfig.Instance.DefaultFullHeartCoin));
                CoinPriceTxt.text = $"{HeartGlobalConfig.Instance.DefaultFullHeartCoin}";
                HeartAddAdsTxt.text = "+1";
                HeartAddCoinTxt.text = $"+{MaxLife}";
                TxtStateHeart.text = "Next life in:";
                if (GameManager.Instance.IsState(GameState.MainMenu))
                {
                    UIMoneyResource.transform.position = PosMoneyHome.position;
                }
                else
                {
                    UIMoneyResource.transform.position = PosMoneyInGame.position;
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
                View.BuyHeartWithAdsBtn.SetOnClickDestination(BuyHeartWithAds);
                View.BuyHeartWithCoinBtn.SetOnClickDestination(BuyHeartWithCoin);
                View.BuyClose.SetOnClickDestination(OnButtonCloseClick);
                Events.FullHeartEvent = OnFullHeart;
            }

            private void BuyHeartWithAds()
            {
                if (GameManager.Instance.BuildType == BuildType.Cheat)
                {
                    BuyHeartWithAdsSuccess();
                    return;
                }

                // ADsManager.ShowRewarded(ResourcePos.home, ResourcePos.refill_heart,
                //     BuyHeartWithAdsSuccess, BuyHeartWithAdsFail);

                ADSAction.OnAdsRewardEvent?.Invoke(ResourcePos.home.ToString(), ResourcePos.refill_heart.ToString(), BuyHeartWithAdsSuccess, BuyHeartWithAdsFail);
                // InGameAdsController.EventShowAdsReward?.Invoke("AdsRw_BuyHeart", BuyHeartWithAdsSuccess, BuyHeartWithAdsFail);
            }

            private void BuyHeartWithAdsSuccess()
            {
                PlayerResourceManager.Instance.AddResourceValue(ResourceType.Life, 1, ResourcePos.home.ToString(), ResourcePos.ads.ToString());
                if (PlayerResourceManager.Instance.HasFullLife || PlayerResourceManager.Instance.HasInfiniteLife)
                {
                    OnButtonCloseClick().Forget();
                }
            }

            private void BuyHeartWithAdsFail()
            {
            }

            private void BuyHeartWithCoin()
            {
                if (!InGameDataManager.Instance.InGameData.ResourceData.IsEnoughResourceValue(ResourceType.Money,
                        DefaultGlobalConfig.Instance.DefaultFullHeartCoin))
                {
                    ViewOptions viewOptions = new ViewOptions(nameof(ModalShopInGame));
                    ModalContainer.Find(ContainerKey.Modals).PushAsync(viewOptions, true).Forget();
                    return;
                }
                PlayerResourceManager.Instance.SubResourceValue(ResourceType.Money, DefaultGlobalConfig.Instance.DefaultFullHeartCoin, ResourcePos.home.ToString(), ResourcePos.refill_heart.ToString());
                PlayerResourceManager.Instance.AddResourceValue(ResourceType.Life, View.MaxLife - PlayerResourceManager.Instance.CurrentLife.Value, ResourcePos.home.ToString(), ResourcePos.coin.ToString());
                OnButtonCloseClick().Forget();
            }
            private async UniTask OnButtonCloseClick()
            {
                await ModalContainer.Find(ContainerKey.Modals).PopAsync(true);
            }

            public void DidPushEnter(Memory<object> args)
            {
                UpdateState();
            }
            
            public void DidPopEnter(Memory<object> args)
            {
                UpdateState();
            }
            
            private void OnFullHeart()
            {
                View.TxtStateHeart.text = "FULL LIFE";
            }

            private void UpdateState()
            {
                View.TxtStateHeart.text = "Next life in:";
                if (PlayerResourceManager.Instance.HasFullLife)
                {
                    View.TxtStateHeart.text = "FULL LIFE";
                }
                
                if (PlayerResourceManager.Instance.HasInfiniteLife)
                {
                    View.TxtStateHeart.text = "INFINITE LIFE";
                }
                
            }
        }
    }
}