using System;
using Core.Manager;
using Cysharp.Threading.Tasks;
using TW.UGUI.MVPPattern;
using UnityEngine;
using R3;
using Sirenix.OdinInspector;
using TW.UGUI.Core.Modals;
using UnityEngine.UI;
using Core.UI.Activities;
using CoreData;
using TW.UGUI.Core.Activities;
using TW.UGUI.Core.Views;

namespace Core.UI.Modals
{
    public class ModalRetryGame : Modal
    {
        [field: SerializeField] public ModalRetryGameContext.UIPresenter UIPresenter { get; private set; }

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
    public class ModalRetryGameContext
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

            [field: SerializeField] public Button ButtonRetry { get; set; }
            [field: SerializeField] public Button ButtonClose { get; set; }

            public UniTask Initialize(Memory<object> args)
            {
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
                View.ButtonRetry.SetOnClickDestination(OnButtonRetryClick);
                View.ButtonClose.SetOnClickDestination(OnButtonCloseClick);
            }

            private async UniTask OnButtonRetryClick()
            {
                //View.MainView.interactable = false;
                if (PlayerResourceManager.Instance.HasLife ||
                    PlayerResourceManager.Instance.HasInfiniteLife)
                {
                    // ADsManager.ShowInterstitial(ResourcePos.replay_level);
                    ADSAction.OnAdsInterEvent?.Invoke(ResourcePos.replay_level.ToString());
                    await RetryLevelCurrent();
                }
                else
                {
                    ViewOptions viewOptions = new ViewOptions(nameof(ModalFillHeart));
                    await ModalContainer.Find(ContainerKey.Modals).PushAsync(viewOptions);
                }
            }

            public async UniTask RetryLevelCurrent()
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
            private async UniTask OnButtonCloseClick()
            {
                await ModalContainer.Find(ContainerKey.Modals).PopAsync(true);
                ViewOptions viewOptions = new ViewOptions(nameof(ModalPause));
                await ModalContainer.Find(ContainerKey.Modals).PushAsync(viewOptions);
            }

            public void DidPushEnter(Memory<object> args)
            {
                Debug.Log("DidPushEnter");
                LevelManager.Instance.SetPause(true);
            }
            public void DidPopEnter(Memory<object> args)
            {
                Debug.Log("DidPopEnter");
            }

            public void DidPushExit(Memory<object> args)
            {
                Debug.Log("DidPushExit");
            }
            public void DidPopExit(Memory<object> args)
            {
                Debug.Log("DidPopExit");
                LevelManager.Instance.SetPause(false);
            }
        }
    }
}