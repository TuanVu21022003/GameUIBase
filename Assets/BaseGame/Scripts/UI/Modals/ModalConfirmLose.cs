using System;
using Core.UI.Activities;
using CoreData;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using TW.UGUI.MVPPattern;
using UnityEngine;
using R3;
using Sirenix.OdinInspector;
using TMPro;
using TW.UGUI.Core.Activities;
using TW.UGUI.Core.Modals;
using TW.UGUI.Core.Views;
using UnityEngine.UI;

namespace Core.UI.Modals
{
    public class ModalConfirmLose : Modal
    {
        [field: SerializeField] public ModalConfirmLoseContext.UIPresenter UIPresenter { get; private set; }

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
    public class ModalConfirmLoseContext
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

            [field: SerializeField]
            public TextMeshProUGUI TextLevel { get; private set; }
            
            [field: SerializeField]
            public Button ButtonClose { get; private set; }
            [field: SerializeField]
            public Button ButtonTryAgain { get; private set; }
            public UniTask Initialize(Memory<object> args)
            {
                TextLevel.SetTextFormat(MyCacheUI.textFormat, "Level " + GameManager.Instance.Level.Value); 
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

                View.ButtonClose.SetOnClickDestination(OnButtonCloseClick);
                View.ButtonTryAgain.SetOnClickDestination(OnButtonTryAgainClick);
            }

            private async UniTask OnButtonCloseClick()
            {
                ViewOptions activityLoading = new ViewOptions(nameof(ActivityLoading));
                Memory<object> args = new Memory<object>(new object[]
                {
                    (Func<UniTask>)(async () =>
                    {
                        await ModalContainer.Find(ContainerKey.Modals).PopAsync(true);
                        GameManager.Instance.LoadMainMenu();

                        // ADsManager.ShowInterstitial(ResourcePos.lose_level);
                        ADSAction.OnAdsInterEvent?.Invoke(ResourcePos.lose_level.ToString());
                        // EventTracking.Instance.LogLevelFail("outofmove");
                    }),
                    null
                });
                await ActivityContainer.Find(ContainerKey.Activities).ShowAsync(activityLoading, args);
            }
            
            private async UniTask OnButtonTryAgainClick()
            {
                if (PlayerResourceManager.Instance.HasLife ||
                    PlayerResourceManager.Instance.HasInfiniteLife)
                {
                    // ADsManager.ShowInterstitial(ResourcePos.replay_level);
                    ADSAction.OnAdsInterEvent?.Invoke(ResourcePos.replay_level.ToString());
                    await OnRetry();
                }
                else
                {
                    ViewOptions viewOptions = new ViewOptions(nameof(ModalFillHeart));
                    await ModalContainer.Find(ContainerKey.Modals).PushAsync(viewOptions);
                }
            }

            private async UniTask OnRetry()
            {
                ViewOptions activityLoading = new ViewOptions(nameof(ActivityLoading));
                Memory<object> args = new Memory<object>(new object[]
                {
                    (Func<UniTask>)(async () =>
                    {
                        await ModalContainer.Find(ContainerKey.Modals).PopAsync(true);
                        await GameManager.Instance.LoadCurrentLevel();
                    }),
                    null
                });
                await ActivityContainer.Find(ContainerKey.Activities).ShowAsync(activityLoading, args);
            }
        }
    }
}