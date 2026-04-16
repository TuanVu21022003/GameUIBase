using Core.UI.Modals;
using Core.UI.Other;
using Core.UI.Screens;
using CoreData;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using GameUI;
using LitMotion;
using TW.UGUI.Core.Modals;
using TW.UGUI.Core.Screens;
using TW.UGUI.Core.Views;
using TW.Utility.CustomType;
using UnityEngine;

public class UIMoneyResource : UICurrencyResource
{
    [field: SerializeField] public Transform IconTransform {get ; private set; }
    public bool IsAnim { get; set; } = false;
    private MotionHandle motionHandleText;
    private MotionHandle motionHandleIcon;

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
            await modalContainer.PopAsync(true);
        }
    }

    protected override void OnResourceChange(BigNumber value)
    {
        AmountChange = value.ToInt();
        if (!IsAnim)
        {
            ChangeText(AmountChange);
        }
    }
    
    public void PlayAnimation()
    {
        if (motionHandleText.IsActive())
            motionHandleText.TryCancel();
        motionHandleText = LMotion.Create(Amount.ToInt(), AmountChange.ToInt(), 0.25f).Bind(ChangeText);
        
        if (motionHandleIcon.IsActive())
            motionHandleIcon.TryCancel();

        motionHandleIcon = LMotion.Create(Vector3.one, Vector3.one * 1.2f, 0.08f)
            .WithEase(Ease.InOutQuad)
            .WithLoops(-1, LoopType.Yoyo) // loop vô hạn, kiểu ping-pong
            .Bind(x => IconTransform.localScale = x);
    }

    protected override void ChangeText(BigNumber val)
    {
        base.ChangeText(val);
        TextAmount.SetTextFormat(MyCacheUI.textFormat, val.ToStringUI());
    }

    private void ChangeText(int val)
    {
        base.ChangeText(val);
        TextAmount.SetTextFormat(MyCacheUI.textFormat, val);
    }

    private void OnDestroy()
    {
        motionHandleIcon.TryCancel();
        motionHandleText.TryCancel();
    }
}