using System;
using Cysharp.Threading.Tasks;
using GameEnum;
using LitMotion;
using Manager;
using TW.UGUI.MVPPattern;
using UnityEngine;
using R3;
using Sirenix.OdinInspector;
using TW.UGUI.Core.Modals;
using UnityEngine.UI;

namespace Core.UI.Modals
{
    public class ModalSetting : Modal, IUITabLifecycleEvent
    {
        [field: SerializeField] public ModalSettingContext.UIPresenter UIPresenter { get; private set; }

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
    public class ModalSettingContext
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

            [field: SerializeField] public bool ActionSelected { get; set; }

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

            [field: SerializeField] public CanvasGroup LanguagePopup { get; private set; }
            
            public UniTask Initialize(Memory<object> args)
            {
                return UniTask.CompletedTask;
            }

            void SetupLanguageDropdown()
            {
                // if(languageInited)
                //     return;
                // languageInited = true;
                // languageBtnPool.OnInit(LanguageBtnPrefab, 1, languageBtnParrent);
                // listLanguage = LanguageGlobalConfig.Instance.listLanguage;
                // for (var i = 0; i < listLanguage.Count; i++)
                // {
                //     LanguageBtn languageBtn = languageBtnPool.Spawn();
                //     languageBtn.Init(listLanguage[i].ToString(), HideLanguagePopup);
                // }
            }

            void HideLanguagePopup()
            {
                LanguagePopup.alpha = 0f;
                LanguagePopup.interactable = false;
                LanguagePopup.blocksRaycasts = false;
            }

            public void ShowLanguagePopup()
            {
                SetupLanguageDropdown();
                LanguagePopup.alpha = 1f;
                LanguagePopup.interactable = true;
                LanguagePopup.blocksRaycasts = true;
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
#if UNITY_IOS
                View.RestoreBtn.gameObject.SetActive(true);
                View.RestoreBtn.SetOnClickDestination(RestorePurchase);
#endif
                // View.LanguageBtn.AddListener(View.ShowLanguagePopup);
            }

            private async UniTask OnClickButtonClose()
            {
                await ModalContainer.Find(ContainerKey.Modals).PopAsync(true);
            }

            public void SetSettingValue(SettingType type, bool value)
            {
                InGameDataManager.Instance.InGameData.SettingData.SetSettingValue(type, value);
            }

            public void SetActionSelected()
            {
                Model.ActionSelected = true;
                LMotion.Create(0, 1f, 1.5f).WithOnComplete(() => { Model.ActionSelected = false; }).RunWithoutBinding()
                    .AddTo(View.MainView);
            }

            void RestorePurchase()
            {
                // Purchaser.Instance.RestorePurchases();
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