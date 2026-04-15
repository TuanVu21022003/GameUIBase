using System;
using Cysharp.Threading.Tasks;
using LitMotion;
using TW.UGUI.Shared;
using UnityEngine;
using UnityEngine.UI;

namespace BaseGame.Scripts.UI.Anim
{
    public class ModalPushAnim : SimpleTransitionAnimationBehaviour
    {
        public Transform targetTransform;
        public CanvasGroup mainCanvasGroup;
        public CanvasGroup bgCanvasGroup;
        public ScrollRect myScrollRect;
        public override async UniTask PlayAsync(IProgress<float> progress = null)
        {
            if (targetTransform == null)
                targetTransform = transform;
            if (bgCanvasGroup != null)
                bgCanvasGroup.alpha = 0f;
            if (mainCanvasGroup == null)
                mainCanvasGroup = GetComponent<CanvasGroup>();
            mainCanvasGroup.alpha = 0f;
            mainCanvasGroup.interactable = false;
            if (myScrollRect != null)
                myScrollRect.enabled = false;
            
            targetTransform.localScale = Vector3.zero;
            if (bgCanvasGroup != null)
                LMotion.Create(0f, 1f, 0.25f)
                    .Bind(x => bgCanvasGroup.alpha = x)
                    .AddTo(mainCanvasGroup);
            
            LMotion.Create(0f, 1f, 0.25f).WithDelay(0.15f).WithEase(Ease.OutBack).Bind(x => targetTransform.localScale = Vector3.one * x).AddTo(transform);
            if (mainCanvasGroup != null)
                await LMotion.Create(0f, 1f, 0.25f)
                .WithOnComplete(() =>
                {
                    if (myScrollRect != null)
                        myScrollRect.enabled = true;
                    mainCanvasGroup.interactable = true;
                })
                .WithDelay(0.15f)
                .Bind(x => mainCanvasGroup.alpha = x)
                .AddTo(mainCanvasGroup);
        }
    }
}
