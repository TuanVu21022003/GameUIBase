using System;
using Core.UI.Activities;
using Core.UI.Screens;
using CoreData;
using Cysharp.Threading.Tasks;
using TW.UGUI.MVPPattern;
using UnityEngine;
using R3;
using Sirenix.OdinInspector;
using TW.UGUI.Core.Activities;
using TW.UGUI.Core.Modals;
using TW.UGUI.Core.Screens;
using TW.UGUI.Core.Views;
using UnityEngine.UI;
using GameUI;

namespace Core.UI.Modals
{
    public class ModalHome : Modal, IUITabLifecycleEvent
    {
        [field: SerializeField] public ModalHomeContext.UIPresenter UIPresenter { get; private set; }

        public override async UniTask Initialize(Memory<object> args)
        {
            await UIPresenter.Initialize(args);
        }

        public void OnTabEnter(Memory<object> args)
        {
            UIPresenter.DidPushEnter(args);
        }

        public void OnTabExit(Memory<object> args)
        {
            UIPresenter.DidPushExit(args);
        }
    }


    [Serializable]
    public class ModalHomeContext
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
            [field: SerializeField] public Button ButtonPlay {get; private set;}
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
                View.ButtonPlay.SetOnClickDestination(OnClickButtonPlay);
            }

            private async UniTask OnClickButtonPlay()
            {
                if (InGameDataManager.Instance.InGameData.ResourceData.IsEnoughResourceValue(ResourceType.Life, 1) || PlayerResourceManager.Instance.HasInfiniteLife)
                {
                    View.ButtonPlay.interactable = false;
                    await LoadGame();
                    // TrackerAnalyticsManager.Instance.TrackLevelStart(GameManager.Instance.Level.Value);
                }
                else
                {
                    ViewOptions viewOptions = new ViewOptions(nameof(ModalFillHeart));
                    await ModalContainer.Find(ContainerKey.Modals).PushAsync(viewOptions);
                }
            }

            async UniTask LoadGame()
            {
                Debug.Log("Load Game");
                ViewOptions activityLoading = new ViewOptions(nameof(ActivityLoading));
                Memory<object> args = new Memory<object>(new object[]
                {
                    (Func<UniTask>)(async () =>
                    {
                        await GameManager.Instance.LoadCurrentLevel();
                        ScreenOptions viewOption = new ScreenOptions(nameof(ScreenInGame), stack: false);
                        await ScreenContainer.Find(ContainerKey.Screens).PushAsync(viewOption);
                    }),
                    null
                });
                await ActivityContainer.Find(ContainerKey.Activities).ShowAsync(activityLoading, args);
            }

            public void DidPushEnter(Memory<object> args)
            {
            }
            
            public void DidPushExit(Memory<object> args)
            {
            }
            
        }
    }
}