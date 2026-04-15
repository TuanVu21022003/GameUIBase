using System;
using Cysharp.Threading.Tasks;
using MoreMountains.Feedbacks;
using TW.UGUI.MVPPattern;
using UnityEngine;
using R3;
using Sirenix.OdinInspector;
using TMPro;
using TW.UGUI.Core.Activities;

namespace Core.UI.Activities
{
    public class ActivityWarning : Activity
    {
        [field: SerializeField] public ActivityWarningContext.UIPresenter UIPresenter { get; private set; }

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
    public class ActivityWarningContext
    {
        public static class Events
        {
            public static Action SampleEvent { get; set; }
            public static Action<int> UpdateTextTime { get; set; }
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
            [field: SerializeField] public MMF_Player AnimWarning {get; private set; }
            [field: SerializeField] public TextMeshProUGUI TextTimeWarning { get; private set; }

            public UniTask Initialize(Memory<object> args)
            {
                return UniTask.CompletedTask;
            }
        }

        [HideLabel]
        [Serializable]
        public class UIPresenter : IAPresenter, IActivityLifecycleEventSimple
        {
            [field: SerializeField] public UIModel Model { get; private set; } = new();
            [field: SerializeField] public UIView View { get; set; } = new();

            public async UniTask Initialize(Memory<object> args)
            {
                await Model.Initialize(args);
                await View.Initialize(args);
                
                Events.UpdateTextTime += UpdateTextTime;
            }

            public UniTask Cleanup(Memory<object> args)
            {
                Events.UpdateTextTime -= UpdateTextTime;
                return UniTask.CompletedTask;
            }

            public void DidEnter(Memory<object> args)
            {
                View.AnimWarning.PlayFeedbacks();
            }

            private void UpdateTextTime(int value)
            {
                View.TextTimeWarning.text = value.ToString();
            }
        }
    }
}