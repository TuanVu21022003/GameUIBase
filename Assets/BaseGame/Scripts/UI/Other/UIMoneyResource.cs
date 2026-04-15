using Core.UI.Modals;
using Core.UI.Other;
using Core.UI.Screens;
using CoreData;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using LitMotion;
using TW.UGUI.Core.Modals;
using TW.UGUI.Core.Screens;
using TW.UGUI.Core.Views;
using UnityEngine;

public class UIMoneyResource : UICurrencyResource
{
    [field: SerializeField] public Transform IconTransform {get ; private set; }
    public bool IsAnim { get; set; } = false;
    private MotionHandle motionHandleText;
    private MotionHandle motionHandleIcon;
    protected override void TestAddCurrency()
    {
        GameResource gameResource = ResourceData.Instance.GetResource(CurrencyType);
#if CHEAT_ONLY
        PlayerResourceManager.Instance.AddResourceValue(ResourceType.Money, 1000, "test_add_money_ui", "cheat");    
#endif
    }

    protected override async UniTask ShowResource()
    {
        if (!IsClick)
        {
            return;
        }
        if (ScreenContainer.Find(ContainerKey.Screens).Current.View is ScreenMainMenu)
        {
            await CloseAllModal();
            UIMainMenuTab.ForceOpenTab?.Invoke(0);
        }
        else
        {
            ViewOptions viewOptions = new ViewOptions(nameof(ModalShopInGame));
            await ModalContainer.Find(ContainerKey.Modals).PushAsync(viewOptions);
        }
    }
    private async UniTask CloseAllModal()
    {
        ModalContainer modalContainer = ModalContainer.Find(ContainerKey.Modals);
        while (modalContainer.Modals.Count > 0)
        {
            // if (modalContainer.Current.View is ModalWin or ModalLose)
            // {
            //     return;
            // }
            await modalContainer.PopAsync(true);
        }
    }

    protected override void OnResourceChange(int value)
    {
        AmountChange = value;
        if (!IsAnim)
        {
            ChangeText(value);
        }
    }
    
    public void PlayAnimation()
    {
        if (motionHandleText.IsActive())
            motionHandleText.TryCancel();
        motionHandleText = LMotion.Create(Amount, AmountChange, 0.25f).Bind(ChangeText);
        
        if (motionHandleIcon.IsActive())
            motionHandleIcon.TryCancel();

        motionHandleIcon = LMotion.Create(Vector3.one, Vector3.one * 1.2f, 0.08f)
            .WithEase(Ease.InOutQuad)
            .WithLoops(-1, LoopType.Yoyo) // loop vô hạn, kiểu ping-pong
            .Bind(x => IconTransform.localScale = x);
    }
    
    private string FormatMoney(int value)
    {
        if (value >= 1_000_000_000)
        {
            return FormatWithSuffix(value, 1_000_000_000, "B");
        }

        if (value >= 1_000_000)
        {
            return FormatWithSuffix(value, 1_000_000, "M");
        }

        if (value >= 100_000)
        {
            return FormatWithSuffix(value, 1_000, "K");
        }

        return value.ToString("N0"); // có dấu , cho số thường
    }

    private string FormatWithSuffix(int value, int divisor, string suffix)
    {
        int integerPart = value / divisor;
        int remainder = value % divisor;

        if (remainder == 0)
            return $"{integerPart}{suffix}";

        // lấy 1 chữ số thập phân nhưng KHÔNG làm tròn
        int decimalPart = (remainder * 10) / divisor;

        return $"{integerPart},{decimalPart}{suffix}";
    }

    protected override void ChangeText(int val)
    {
        base.ChangeText(val);
        TextAmount.SetTextFormat(MyCacheUI.textFormat, FormatMoney(val));
    }

    private void OnDestroy()
    {
        motionHandleIcon.TryCancel();
        motionHandleText.TryCancel();
    }
}