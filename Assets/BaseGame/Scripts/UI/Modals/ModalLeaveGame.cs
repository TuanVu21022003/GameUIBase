using System;
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
using TW.UGUI.Core.Activities;
using TW.UGUI.Core.Modals;
using TW.UGUI.Core.Screens;
using TW.UGUI.Core.Views;
using UnityEngine.UI;

namespace Core.UI.Modals
{
    public class ModalLeaveGame : Modal
    {
        [field: SerializeField] public ModalLeaveGameContext.UIPresenter UIPresenter { get; private set; }

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
    public class ModalLeaveGameContext
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
            [field: SerializeField] public Button ButtonLeave {get; set;}
            [field: SerializeField] public Button ButtonClose {get; set;}

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
                View.ButtonLeave.SetOnClickDestination(OnButtonLeaveClick);
                View.ButtonClose.SetOnClickDestination(OnButtonCloseClick);
            }

            private async UniTask OnButtonLeaveClick()
            {
                View.MainView.interactable = false;
                await BackToMainMenu();

                // ADsManager.ShowInterstitial(ResourcePos.quit_level);
                ADSAction.OnAdsInterEvent?.Invoke(ResourcePos.quit_level.ToString());
                // EventTracking.Instance.LogLevelFail("quitgame");
            }

            public async UniTask BackToMainMenu()
            {
                ViewOptions activityLoading = new ViewOptions(nameof(ActivityLoading));
                Memory<object> args = new Memory<object>(new object[]
                {
                    (Func<UniTask>)(async () =>
                    {
                        //ModalContainer modalContainer = ModalContainer.Find(ContainerKey.Modals);
                        // await UniTask.WaitUntil(() => !modalContainer.IsInTransition);
                        // while (modalContainer.Modals.Count > 0)
                        // {
                        //     await modalContainer.PopAsync(true);
                        // }
                        // await ModalContainer.Find(ContainerKey.Modals).PopAsync(true);
                        await CloseAllModals();
                        await GameManager.Instance.LoadMainMenu();
                    }),
                    null
                });
                await ActivityContainer.Find(ContainerKey.Activities).ShowAsync(activityLoading, args);
            }
            private async UniTask CloseAllModals()
            {
                while (ModalContainer.Find(ContainerKey.Modals).Modals.Count > 0)
                {
                    await ModalContainer.Find(ContainerKey.Modals).PopAsync(false);
                }
            }
            private async UniTask OnButtonCloseClick()
            {
                await ModalContainer.Find(ContainerKey.Modals).PopAsync(true);
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