using System;
using Cysharp.Threading.Tasks;
using TW.UGUI.MVPPattern;
using UnityEngine;
using R3;
using Sirenix.OdinInspector;
using TMPro;
using TW.UGUI.Core.Activities;
using UnityEngine.UI;

namespace Core.UI.Activities
{
    public class ActivityTipBooster : Activity
    {
        [field: SerializeField] public ActivityTipBoosterContext.UIPresenter UIPresenter { get; private set; }

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
    public class ActivityTipBoosterContext
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
            
            [field: SerializeField] public Image ImageIcon { get; private set; }
            [field: SerializeField] public TextMeshProUGUI TextTitle { get; private set; }
            [field: SerializeField] public TextMeshProUGUI TextDesc { get; private set; }
            [field: SerializeField] public Button ButtonClose { get; private set; }
            [field: SerializeField] public Transform TipTransform { get; private set; }
            [field: SerializeField] public Transform PosTipLaser { get; private set; }
            [field: SerializeField] public Transform PosTipPropeller { get; private set; }
            
            public BoosterInfoData Data { get; private set; }

            public UniTask Initialize(Memory<object> args)
            {
                ResourceType type = (ResourceType)args.Span[0];
                Data = ItemGlobalConfig.Instance.GetBoosterData(type);
                ImageIcon.sprite = Data.Sprite;
                TextTitle.text = Data.Name;
                TextDesc.text = Data.Description;
                UpdatePosTip();
                return UniTask.CompletedTask;
            }

            private void UpdatePosTip()
            {
                switch (Data.ResourceType)
                {
                    case ResourceType.Laser:
                        TipTransform.position = PosTipLaser.position;
                        break;
                    case ResourceType.Propeller:
                        TipTransform.position = PosTipPropeller.position;
                        break;
                }
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
                View.ButtonClose.SetOnClickDestination(OnButtonCloseClick);
                
                GameGlobalEvent.OnPlayerUseBoosterMagnetCompleted += OnPlayerUseBoosterMagnetCompleted;
            }

            public UniTask Cleanup(Memory<object> args)
            {
                GameGlobalEvent.OnPlayerUseBoosterMagnetCompleted -= OnPlayerUseBoosterMagnetCompleted;
                return UniTask.CompletedTask;
            }

            private void OnButtonCloseClick()
            {
                
                switch (View.Data.ResourceType)
                {
                    case ResourceType.Laser:
                        // GameGlobalEvent.OnCancelBoosterMagnet.Invoke();
                        BoosterManager.Instance.CancelBooster(BoosterType.Magnet);
                        break;
                    case ResourceType.Propeller:
                        // GameGlobalEvent.OnCancelBooksterBalloon.Invoke();
                        BoosterManager.Instance.CancelBooster(BoosterType.Balloon);
                        break;
                }
                
                
            }
            private void OnPlayerUseBoosterMagnetCompleted(CubeColor cubeColor)
            {
                View.ButtonClose.gameObject.SetActive(false);
            }
        }
    }
}