using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Manager;
using TW.UGUI.MVPPattern;
using UnityEngine;
using R3;
using Sirenix.OdinInspector;
using TW.UGUI.Core.Modals;
using UnityEngine.UI;
using System.Threading.Tasks;
using Core.Manager;

namespace Core.UI.Modals
{
    public class ModalShop : Modal, IUITabLifecycleEvent
    {
        [field: SerializeField] public ModalShopContext.UIPresenter UIPresenter { get; private set; }

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
    public class ModalShopContext
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

            [field: SerializeField] public bool MarkResumeGame { get; set; }

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

            [field: SerializeField] public Button CloseBtn { get; private set; }

            // [field: SerializeField] public List<UIShopPack> UIShopPacks {get; private set;}  
            [field: SerializeField] public GameObject BlockUI { get; private set; }
            [field: SerializeField] public RectTransform content { get; private set; }

            public UniTask Initialize(Memory<object> args)
            {
                // for (int i = 0; i < UIShopPacks.Count; i++)
                // {
                //     UIShopPacks[i].Init();
                // }

                return UniTask.CompletedTask;
            }
            // public void ReInitContent()
            // {
            //     // for (int i = 0; i < UIShopPacks.Count; i++)
            //     // {
            //     //     UIShopPacks[i].Init();
            //     // }
            // }
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
                await UniTask.WaitForEndOfFrame();
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(View.content);
                View.CloseBtn.SetOnClickDestination(OnCloseModal);
            }

            // public void ReInitContent()
            // {
            //     View.ReInitContent();
            // }

            public async UniTask OnCloseModal()
            {
                Debug.Log("close modal");
                await ModalContainer.Find(ContainerKey.Modals).PopAsync(true);
            }

            public void DidPushEnter(Memory<object> args)
            {
            }

            public void DidPopEnter(Memory<object> args)
            {
            }

            public void DidPushExit(Memory<object> args)
            {
            }

            public void DidPopExit(Memory<object> args)
            {
            }
        }
    }
}