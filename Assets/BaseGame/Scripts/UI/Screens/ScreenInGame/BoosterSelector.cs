using System;
using AssetKits.ParticleImage;
using Core;
using Core.GamePlay;
using Core.GamePlay.GlobalEnum;
using Core.Manager;
using Core.UI;
using Core.UI.Modals;
using Core.UI.Screens;
using CoreData;
using Cysharp.Threading.Tasks;
using LitMotion;
using Manager;
using R3;
using TMPro;
using TW.UGUI.Core.Modals;
using TW.UGUI.Core.Views;
using UnityEngine;
using UnityEngine.UI;

public class BoosterSelector : MonoBehaviour
{
    [field: SerializeField] public ResourceType BoosterType { get; private set; } = new();
    [field: SerializeField] public CanvasGroup BoosterCG { get; private set; }
    [field: SerializeField] public Image BoosterIcon { get; private set; }
    [field: SerializeField] public Image BoosterIconWaitClaim { get; private set; }
    [field: SerializeField] public Transform BoosterWrap { get; private set; }
    [field: SerializeField] public Reactive<int> BoosterAmount { get; private set; }
    [field: SerializeField] public TextMeshProUGUI BoosterAmountTxt { get; private set; }
    [field: SerializeField] public TextMeshProUGUI BoosterUnlockLvTxt { get; private set; }
    [field: SerializeField] public TextMeshProUGUI BoosterPriceTxt { get; private set; }
    [field: SerializeField] public GameObject BoosterAmountObj { get; private set; }
    [field: SerializeField] public GameObject AddMoreBoosterObj { get; private set; }
    [field: SerializeField] public GameObject UnlockedObj { get; private set; }
    [field: SerializeField] public GameObject LockedObj { get; private set; }
    [field: SerializeField] public GameObject UnlockWaitClaimObj { get; private set; }
    [field: SerializeField] public Button Button { get; private set; }
    [field: SerializeField] public Transform StateWrapTF { get; private set; }
    [field: SerializeField] public GameObject TutorialHand { get; private set; }
    [field: SerializeField] private AnimationCurve SuggestionCurve;
    [field: SerializeField] private float MultiplierAmplitudeSuggestion = 3f;
    [field: SerializeField] private float TimeLoop = 1f;
    [field: SerializeField] private GameObject EffectLight;
    private int level => GameManager.Instance.Level.Value;
    private BoosterInfoData boosterData;
    private bool isShowOnTutorial;

    private MotionHandle SuggestionHandle;
    private Vector3 originalPos;

    private void Awake()
    {
        BoosterAmount = InGameDataManager.Instance.InGameData.ResourceData.GetResource(BoosterType).Value;
        BoosterAmount.Subscribe(UpdateBoosterAmount).AddTo(this);
        Button.SetOnClickDestination(OnClickBooster);
        boosterData = ItemGlobalConfig.Instance.GetBoosterData(BoosterType);
        BoosterIcon.sprite = boosterData.Sprite;
        BoosterIconWaitClaim.sprite = boosterData.Sprite;
        BoosterUnlockLvTxt.text = $"Lv.{boosterData.LevelUnlock}";
        SetState();
        isShowOnTutorial = false;
        GameManager.Instance.IsPauseGame.Subscribe(CheckPauseGame).AddTo(this);
        EffectLight.SetActive(false);
    }

    private void Start()
    {
        originalPos = transform.localPosition;
    }

    public void OnChangeLevel(int level)
    {
        if (level == boosterData.LevelUnlock)
        {
            // if (InGameDataManager.Instance.InGameData.ResourceData.IsBoosterUsedOnTut(BoosterType)
            //     || boosterData.skeletonDataAsset == null)
            // {
            //     LockedObj.SetActive(false);
            //     UnlockedObj.SetActive(true);
            //     BoosterAmountObj.SetActive(BoosterAmount.Value > 0);
            //     AddMoreBoosterObj.SetActive(BoosterAmount.Value <= 0);
            //     BoosterIcon.gameObject.SetActive(true);
            // }
            LockedObj.SetActive(false);
            UnlockedObj.SetActive(true);
            BoosterAmountObj.SetActive(BoosterAmount.Value > 0);
            AddMoreBoosterObj.SetActive(BoosterAmount.Value <= 0);
            BoosterIcon.gameObject.SetActive(true);
        }
        SetState();
    }

    void CheckPauseGame(bool pause)
    {
        // Debug.Log("Pause Game" + pause);
        if (!pause && BoosterCG.alpha < 1)
        {
            BoosterCG.alpha = 1;
            Button.interactable = true;
        }

        if (pause)
        {
            BoosterCG.alpha = 0.5f;
            Button.interactable = false;
        }
    }

    void SetState()
    {
        // BoosterCG.alpha = 0.5f;
        if (level >= boosterData.LevelUnlock)
        {
            BoosterIcon.gameObject.SetActive(true);
            LockedObj.SetActive(false);
            UnlockedObj.SetActive(true);
            BoosterAmountObj.SetActive(BoosterAmount.Value > 0);
            AddMoreBoosterObj.SetActive(BoosterAmount.Value <= 0);
            Button.interactable = true;
        }
        else if (level < boosterData.LevelUnlock)
        {
            BoosterIcon.gameObject.SetActive(false);
            LockedObj.SetActive(true);
            UnlockedObj.SetActive(false);
            BoosterAmountObj.SetActive(false);
            AddMoreBoosterObj.SetActive(false);
            Button.interactable = false;
        }
        UnlockWaitClaimObj.SetActive(false);
    }

    public async UniTask GetBoosterOnTutorial(Vector3 root)
    {
        EffectLight.SetActive(true);
        Button.interactable = false;
        BoosterWrap.gameObject.SetActive(true);
        BoosterCG.alpha = 1f;
        StateWrapTF.localScale = Vector3.zero;
        // InGameDataManager.Instance.InGameData.ResourceData.AddBoosterUsedOnTut(BoosterType);
        LockedObj.SetActive(false);
        UnlockedObj.SetActive(true);
        BoosterWrap.position = root;
        BoosterWrap.localScale = Vector3.one * 2f;
        await LSequence.Create()
            .AppendInterval(1.5f)
            .Append(LMotion.Create(BoosterWrap.localPosition, Vector3.zero, 0.75f)
                .WithEase(Ease.InBack)
                .Bind(x => BoosterWrap.localPosition = x))
            .Join(LMotion.Create(BoosterWrap.localScale, Vector3.one, 0.75f)
                .WithEase(Ease.InBack)
                .Bind(x => BoosterWrap.localScale = x)).Run().AddTo(this);
        
        LevelManager.Instance.CurrentLevel.SetIsClaimBooster(true);
        ScreenInGameContext.Events.HideBackGroundBlack?.Invoke();
        EffectLight.SetActive(false);
        UnlockWaitClaimObj.SetActive(false);
        // AudioManager.Instance.PlaySoundFx(AudioKey.SfxUIBoosterClaim);
        await LMotion.Create(transform.localScale, Vector3.one * 1.1f, 0.1f)
            .WithEase(Ease.InQuad)
            .Bind(x => transform.localScale = x).AddTo(this);

        await LMotion.Create(transform.localScale, Vector3.one, 0.1f)
            .WithEase(Ease.OutQuad)
            .Bind(x => transform.localScale = x).AddTo(this);
        await LMotion.Create(StateWrapTF.localScale, Vector3.one, 0.1f)
            .WithEase(Ease.OutQuad)
            .Bind(x => StateWrapTF.localScale = x).AddTo(this);
        ActiveBooster();
        Button.interactable = true;
    }

    public void OnUnlockClaim()
    {
        UnlockWaitClaimObj.SetActive(true);
        StateWrapTF.localScale = Vector3.zero;
        BoosterWrap.gameObject.SetActive(false);
    }
    void ActiveBooster()
    {
        BoosterAmountObj.SetActive(true);
        AddMoreBoosterObj.SetActive(BoosterAmount.Value <= 0);
        Button.interactable = true;
        //TutorialHand.SetActive(true);
        isShowOnTutorial = true;
        //DelayHideTut().Forget();
    }

    async UniTask DelayHideTut()
    {
        await UniTask.Delay(15000);
        // InGameDataManager.Instance.InGameData.ResourceData.AddBoosterUsedOnTut(BoosterType);
        if (TutorialHand != null)
        {
            TutorialHand.SetActive(false);
        }
    }

    public void ShowTutorialHand(Transform hint)
    {
        hint.gameObject.SetActive(!LockedObj.activeSelf);
        if (TutorialHand != null)
        {
            hint.position = TutorialHand.transform.position;
            
        }
    }

    public void HideTutorialHand()
    {
        if (TutorialHand != null)
        {
            TutorialHand.SetActive(false);
        }
    }

    void UpdateBoosterAmount(int amount)
    {
        BoosterAmountTxt.text = $"{amount}";
        AddMoreBoosterObj.SetActive(amount <= 0 && !LockedObj.activeSelf);
        BoosterAmountObj.SetActive(amount > 0 && !LockedObj.activeSelf);
    }

    private async UniTask OnClickBooster()
    {
        
        await SelectBooster("ingame");
    }

    public async UniTask SelectBooster(string placement)
    {
         Debug.Log("Click Booster: " + BoosterType);
         if (TutorialHand != null)
         {
             TutorialHand.SetActive(false);
         }
         if ((GameManager.Instance.IsPauseGame.Value && !isShowOnTutorial)
             || LockedObj.activeSelf)
             return;
         if (!GameManager.Instance.IsState(GameState.InGame))
         {
             return;
         }

        if (BoosterAmount.Value > 0)
        {
            if (CheckUseCondition())
            {
                //BoosterManager.Instance.UseBooster(BoosterType);
                BoosterType boosterType = GetBoosterTypeByResourceType(BoosterType);
                BoosterManager.Instance.UseBooster(boosterType);
                Debug.Log("Use Booster: " + BoosterType);
                ScreenInGameContext.Events.HideBoosterHint?.Invoke();
            }
            else
            {
                ScreenInGameContext.Events.ShowNotification?.Invoke(boosterData.MessageNoUse);
                if (BoosterType == ResourceType.Propeller)
                {
                    VibrationManager.Instance.CallHaptic(HapticType.MediumImpact);
                }
            }
        }
        else
        {
            ViewOptions options = new ViewOptions(nameof(ModalBooster));
            await ModalContainer.Find(ContainerKey.Modals).PushAsync(options, this);
        }
    }

    public async UniTask DelayUseBooster()
    {
        await UniTask.Delay(100);
        SelectBooster("ingame");
    }

    private async UniTask ShowModalBooster(ResourceType boosterType)
    {
        GameResource resource = new GameResource(boosterType, 1);
        ViewOptions options = new ViewOptions(nameof(ModalBooster));
        await ModalContainer.Find(ContainerKey.Modals).PushAsync(options, resource);
    }

    bool CheckUseCondition()
    {
        switch (BoosterType)
        {
            case ResourceType.Propeller:
                return BoosterManager.Instance.CanUseBooster(Core.GamePlay.GlobalEnum.BoosterType.Balloon);
            default:
                break;
        }
        return true;
    }

    public void SetSuggestion(bool value)
    {
        if (value)
        {
            SuggestionHandle.TryCancel();
            StartSuggestionCycle();
        }
        else
        {
            SuggestionHandle.TryCancel();
            transform.localPosition = originalPos;
        }
    }

    private void StartSuggestionCycle()
    {
        SuggestionHandle.TryCancel();
        SuggestionHandle = LMotion.Create(0f, 1f, 0.5f)
            .WithEase(Ease.Linear)
            .WithOnComplete(() =>
            {
                transform.localPosition = originalPos;
                SuggestionHandle = LMotion.Create(0f, 0f, TimeLoop)
                    .WithOnComplete(StartSuggestionCycle)
                    .RunWithoutBinding()
                    .AddTo(this);
            })
            .Bind(OnSuggestionUpdate)
            .AddTo(this);
    }

    private void OnSuggestionUpdate(float value)
    {
        var offsetX = SuggestionCurve.Evaluate(value);
        transform.localPosition = originalPos + Vector3.right * offsetX * MultiplierAmplitudeSuggestion;
    }

    private BoosterType GetBoosterTypeByResourceType(ResourceType resourceType)
    {
        switch (resourceType)
        {
            case ResourceType.AddSlot :
                return Core.GamePlay.GlobalEnum.BoosterType.AddSlot;
            case ResourceType.Laser:
                return Core.GamePlay.GlobalEnum.BoosterType.Magnet;
            case ResourceType.Propeller:
                return Core.GamePlay.GlobalEnum.BoosterType.Balloon;
        }
        return Core.GamePlay.GlobalEnum.BoosterType.None;
    }
}