using System;
using Cysharp.Threading.Tasks;
using TW.UGUI.Core.Activities;
using TW.UGUI.Core.Modals;
using TW.UGUI.Core.Screens;
using TW.UGUI.Core.Views;
using TW.UGUI.Shared;
using TW.Utility.DesignPattern;
using UnityEngine;

namespace GameUI
{
    public class UIManager : Singleton<UIManager>
    {
        private void Start()
        {
            Application.targetFrameRate = 60;
        }

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(this);
        }

        #region Screens

        public async UniTask OpenScreenAsync<T>(bool stackChange, params object[] args)
        {
            var option = new ScreenOptions(typeof(T).Name, stack: stackChange);
            await ScreenContainer.Find(ContainerKey.Screens).PushAsync(option, args);
        }

        public async UniTask OpenScreenAsync<T>(params object[] args)
        {
            var option = new ScreenOptions(typeof(T).Name, stack: false);
            await ScreenContainer.Find(ContainerKey.Screens).PushAsync(option, args);
        }

        public void OpenScreen<T>(params object[] args)
        {
            ScreenOptions option = new ScreenOptions(typeof(T).Name, stack: false);
            ScreenContainer.Find(ContainerKey.Screens).Push(option, args);
        }

        public async UniTask CloseScreenAsync(bool playAnimation = true)
        {
            await ScreenContainer.Find(ContainerKey.Screens).PopAsync(playAnimation);
        }

        public void CloseScreen(bool playAnimation = true)
        {
            ScreenContainer.Find(ContainerKey.Screens).Pop(playAnimation);
        }

        #endregion

        #region Modal

        public async UniTask OpenModalAsync<T>(bool anim, params object[] args)
        {
            //AudioManager.Instance.PlaySoundFx(AudioType.SfxUIModalOpen);
            var option = new ViewOptions(typeof(T).Name, playAnimation: anim);
            await ModalContainer.Find(ContainerKey.Modals).PushAsync(option, args);
        }

        public async UniTask OpenModalAsync<T>(params object[] args)
        {
            //AudioManager.Instance.PlaySoundFx(AudioType.SfxUIModalOpen);
            var option = new ViewOptions(typeof(T).Name, playAnimation: true);
            await ModalContainer.Find(ContainerKey.Modals).PushAsync(option, args);
        }

        public void OpenModal<T>(bool anim = true, params object[] args)
        {
            //AudioManager.Instance.PlaySoundFx(AudioType.SfxUIModalOpen);
            var option = new ViewOptions(typeof(T).Name, playAnimation: anim);
            ModalContainer.Find(ContainerKey.Modals).Push(option, args);
        }

        public async UniTask CloseModalAsync(bool anim = true)
        {
            //AudioManager.Instance.PlaySoundFx(AudioType.SfxUIModalClose);
            await ModalContainer.Find(ContainerKey.Modals).PopAsync(anim);
        }

        public void CloseModal(bool anim = true)
        {
            //AudioManager.Instance.PlaySoundFx(AudioType.SfxUIModalClose);
            ModalContainer.Find(ContainerKey.Modals).Pop(anim);
        }

        public bool IsHaveModalOpen()
        {
            return ModalContainer.Find(ContainerKey.Modals).Modals.Count > 0;
        }

        public ViewRef<ModalBackdrop> GetModalBackdrop()
        {
            return ModalContainer.Find(ContainerKey.Modals).Backdrops[^1];
        }

        #endregion

        #region Activity

        public async UniTask OpenActivityAsync<T>(params object[] args)
        {
            ViewOptions option = new ViewOptions(typeof(T).Name, playAnimation: true);
            await ActivityContainer.Find(ContainerKey.Activities).ShowAsync(option, args);
        }

        public void OpenActivity<T>(params object[] args)
        {
            ViewOptions option = new ViewOptions(typeof(T).Name, playAnimation: true);
            ActivityContainer.Find(ContainerKey.Activities).Show(option, args);
        }

        public async UniTask CloseActivityAsync<T>()
        {
            await ActivityContainer.Find(ContainerKey.Activities).HideAsync(typeof(T).Name, true);
        }

        public void CloseActivity<T>()
        {
            ActivityContainer.Find(ContainerKey.Activities).Hide(typeof(T).Name, true);
        }

        public bool IsHaveActivityOpen()
        {
            return ActivityContainer.Find(ContainerKey.Activities).Activities.Count > 0;
        }

        public async UniTask CloseAllActivity()
        {
            await ActivityContainer.Find(ContainerKey.Activities).HideAllAsync(true);
        }

        #endregion
    }
}