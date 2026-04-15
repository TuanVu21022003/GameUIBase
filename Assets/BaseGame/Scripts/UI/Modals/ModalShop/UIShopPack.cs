using System.Collections.Generic;
using Core;
using Core.GamePlay;
using Core.UI;
using CoreData;
using Manager;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using R3;

public class UIShopPack : MonoBehaviour
{
    [field: SerializeField] public PackageName PackageName { get; set; }
    [field: SerializeField] public TextMeshProUGUI PackageNameTxt { get; set; }
    [field: SerializeField] public List<UIItemInfo> UIItemInfos { get; set; } = new();
    [field: SerializeField] public ShopPackageDataConfig ShopPackageDataConfig { get; set; } = new();
    [field: SerializeField] public Button PurchaseBtn { get; set; }
    [field: SerializeField] public TextMeshProUGUI TxtPackPrice { get; set; }
    [BoxGroup("Editor Only")]
    [SerializeField]
    private Image bgImage;
    [BoxGroup("Editor Only")]
    [SerializeField]
    private List<Image> bgItem;
    [BoxGroup("Editor Only")]
    [SerializeField]
    private Image mainItems;
    [BoxGroup("Editor Only")]
    [SerializeField]
    private Sprite bgSprite;
    [BoxGroup("Editor Only")]
    [SerializeField]
    private Sprite bgItemSprite;
    [BoxGroup("Editor Only")]
    [SerializeField]
    private Sprite mainItemsSprite;
    

    private void Start()
    {
        Init();
        PurchaseBtn.SetOnClickDestination(OnPurchase);
        InGameDataManager.Instance.InGameData.SettingData.LanguageCode
            .Subscribe(OnLanguageChanged).AddTo(this);
    }

    public void Init()
    {
        if (!CheckPackPurchaseAvailable())
        {
            gameObject.SetActive(false);
            return;
        }
        gameObject.SetActive(true);
        // this.callBackFromParent = callBackFromParent;
        // this.showReward = showReward;
        // this.reInit = reInit;
        InitUI();
    }

    [Button]
    public void InitDefault()
    {
        if (bgImage != null)
            bgImage.sprite = bgSprite;
        for (int i = 0; i < bgItem.Count; i++)
        {
            if (bgItem[i] != null)
            {
                bgItem[i].sprite = bgItemSprite;
            }
        }
        if (mainItems != null)
            mainItems.sprite = mainItemsSprite;
        ShopPackageDataConfig = ShopGlobalConfig.Instance.GetShopPackageDataConfig(PackageName);
        SetPackageName();
    }

    [Button]
    public void InitUI()
    {
        ShopPackageDataConfig = ShopGlobalConfig.Instance.GetShopPackageDataConfig(PackageName);
        if (ShopPackageDataConfig == null)
        {
            // Debug.LogError($"ShopPackageDataConfig Null {PackageName}");
            gameObject.SetActive(false);
            return;
        }
        SetPackPrice();
        SetPackageName();
        int itemCount = 0;
        for (int i = 0; i < UIItemInfos.Count; i++)
        {
            if (i < ShopPackageDataConfig.GameResources.Count)
            {
                if (ShopPackageDataConfig.GameResources[i] != null
                    && ShopPackageDataConfig.GameResources[i].Value.Value > 0)
                {
                    UIItemInfos[i].Init(ShopPackageDataConfig.GameResources[i]);
                    UIItemInfos[i].gameObject.SetActive(true);
                }
                else
                {
                    UIItemInfos[i].gameObject.SetActive(false);
                }
            }
            else
            {
                UIItemInfos[i].gameObject.SetActive(false);
            }
        }
    }

    private bool CheckPackPurchaseAvailable()
    {
        // if(PackageName == PackageName.removeads)
        //     return !ShopData.Instance.IsPackPurchased(PackageName.ToString());
        return true;
    }

    private void SetPackPrice()
    {
        if(GameManager.Instance.BuildType == BuildType.Cheat)
        {
            TxtPackPrice.text = $"${ShopPackageDataConfig.price}";
            PurchaseBtn.interactable = true;
            return;
        }
        switch (ShopPackageDataConfig.priceType)
        {
            case PriceType.IAP:
                // TxtPackPrice.text = $"{IAPManager.Instance.GetIAPPackage(ShopPackageDataConfig.packageId).GetPrice()}";
                PurchaseBtn.interactable = true;
                break;
            case PriceType.Money:
                TxtPackPrice.text = $"{ShopPackageDataConfig.price}";
                PurchaseBtn.interactable = true;
                break;
        }
    }

    private void OnPurchase()
    {
#if CHEAT_ONLY
        PurchaseSuccess();
        return;
#endif
        if (GameManager.Instance.BuildType == BuildType.Cheat)
        {
            PurchaseSuccess();
            return;
        }
        Debug.Log($"OnPurchase {ShopPackageDataConfig.packageName} - {ShopPackageDataConfig.priceType}");
        switch (ShopPackageDataConfig.priceType)
        {
            case PriceType.IAP:
                OnIAPPurchase();
                break;
            case PriceType.Ads:
                OnAdsPurchase();
                break;
            case PriceType.Money:
                OnMoneyPurchase();
                break;
        }
    }

    private void OnTestPurchase()
    {
        PurchaseSuccess();
    }

    private void OnIAPPurchase()
    {
        // IAPManager.Instance.PurchasePackage(ShopPackageDataConfig.packageId, PurchaseSuccess);

        // InGameIAPController.EventPurchaseIAPProduct?.Invoke(ShopPackageDataConfig.packageId, PurchaseSuccess);
    }

    private void OnAdsPurchase()
    {
        // InGameAdsController.EventShowAdsReward?.Invoke($"AdsRw_GetResource_{ShopPackageDataConfig.packageName}", OnAdsPurchaseSuccess, null);
    }

    private void OnAdsPurchaseSuccess()
    {
        // InGameDataManager.Instance.InGameData.ResourceDataSave.SetPackageCondition(ShopPackageDataConfig.packageName, DateTime.Now.AddMinutes(10).ToString(CultureInfo.InvariantCulture));
        //         adsWaitingTime = InGameDataManager.Instance.InGameData.ResourceDataSave.GetPackageCondion(ShopPackageDataConfig.packageName);
        //         CheckPurchaseByAds();
        //         PurchaseSuccess();
    }

    private void OnMoneyPurchase()
    {
        PurchaseSuccess();
    }

    public void PurchaseSuccess()
    {
        ResourcePos placement = GameManager.Instance.GameState == GameState.MainMenu ? ResourcePos.home : ResourcePos.ingame;
        RewardManager.Instance.AddReward(ShopPackageDataConfig.GameResources, placement.ToString(), ResourcePos.iap.ToString());
        RewardManager.Instance.ShowReward();
        ShopData.Instance.AddPurchasedPack(ShopPackageDataConfig.packageName.ToString());
        GameGlobalEvent.OnPurchaseIapSuccess?.Invoke();
    }

    private void OnLanguageChanged(string language)
    {
        SetPackageName();
    }

    private void SetPackageName()
    {
        if (PackageNameTxt != null)
        {
            PackageNameTxt.text = $"{ShopPackageDataConfig.packageNameToUI}";
        }
    }
}