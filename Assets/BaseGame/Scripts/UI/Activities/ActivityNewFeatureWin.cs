using System;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using LitMotion;
using TW.UGUI.MVPPattern;
using UnityEngine;
using R3;
using Sirenix.OdinInspector;
using TMPro;
using TW.UGUI.Core.Activities;
using UnityEngine.UI;

namespace Core.UI.Activities
{
    public class ActivityNewFeatureWin : Activity
    {
        [field: SerializeField] public ActivityNewFeatureWinContext.UIPresenter UIPresenter { get; private set; }

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
    public class ActivityNewFeatureWinContext
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
            
            [field: SerializeField] public Transform FeatureGroup {get; private set;}
            [field: SerializeField] public Transform FeatureNoticeGroup {get; private set;}
            [field: SerializeField] public Image FeatureProgress {get; private set;}
            [field: SerializeField] public AnimationCurve CurveFeatureNoticeGroup {get; private set;}
            [field: SerializeField] public Transform GlowTF {get; private set;}
            [field: SerializeField] public TextMeshProUGUI TextProgress {get; private set;}
            [field: SerializeField] public Image ImageIcon {get; private set;}
            [field: SerializeField] public TextMeshProUGUI TextName {get; private set;}
            public MotionHandle RotateHandle;
            public UniTask Initialize(Memory<object> args)
            {
                PlayRotateLoop();
                if (NewFeatureGlobalConfig.Instance.TryGetNewFeatureNextUnlock(GameManager.Instance.Level.CurrentValue,
                        out NewFeatureData newFeatureDataNext))
                {
                    ImageIcon.sprite = newFeatureDataNext.FeatureIcon;
                    TextName.text = newFeatureDataNext.FeatureName;
                    FeatureProgress.sprite = newFeatureDataNext.FeatureIcon;
                }
                return UniTask.CompletedTask;
            }
            
            public async UniTask PlayAnimFeature(float featureProgressPrevious, float featureProgressNext)
            {
                FeatureGroup.transform.localScale = Vector3.zero;
                FeatureNoticeGroup.gameObject.SetActive(false);
                
                int progressPrevious = Mathf.RoundToInt(featureProgressPrevious * 100);
                int progressNext = Mathf.RoundToInt(featureProgressNext * 100);
                if (progressNext >= 100)
                {
                    TextProgress.gameObject.SetActive(false);   
                }
                
                FeatureProgress.fillAmount = 1 - featureProgressPrevious;
                TextProgress.SetTextFormat(MyCacheUI.textFormatPercent, progressPrevious);
                await LMotion.Create(0f, 1f, 0.3f)
                    .WithEase(Ease.OutBack)
                    .Bind(t =>
                    {
                        FeatureGroup.localScale = Vector3.one * t;
                    }).AddTo(MainView);
                
                FeatureNoticeGroup.gameObject.SetActive(true);
                await LMotion.Create(0f, 1f, 0.3f)
                    .WithEase(CurveFeatureNoticeGroup)
                    .Bind(t =>
                    {
                        FeatureNoticeGroup.localScale = Vector3.one * t;
                    }).AddTo(MainView);
                LMotion.Create(progressPrevious, progressNext, 0.5f)
                    .WithEase(Ease.OutQuad)
                    .Bind(t =>
                    {
                        TextProgress.SetTextFormat(MyCacheUI.textFormatPercent, t);
                    }).AddTo(MainView);
                await LMotion.Create(1 - featureProgressPrevious, 1 - featureProgressNext, 0.5f)
                    .WithEase(Ease.OutQuad)
                    .Bind(t =>
                    {
                        FeatureProgress.fillAmount = t;
                    }).AddTo(MainView);
            }
            
            public void PlayRotateLoop()
            {
                RotateHandle = LMotion.Create(0, 360, 2f)
                    .WithEase(Ease.Linear)
                    .WithLoops(-1)
                    .Bind(t =>
                    {
                        GlowTF.localRotation = Quaternion.Euler(0, 0, t);
                    }).AddTo(MainView); 
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
            }

            [Button]
            private async UniTask UpdateState()
            {
                NewFeatureData newFeatureDataNext = null;
                NewFeatureData newFeatureDataPrevious = null;

                int levelUnlockNext = 0;
                int levelUnlockPrevious = 1;
                int levelCurrent = GameManager.Instance.Level.CurrentValue;
                
                if (NewFeatureGlobalConfig.Instance.TryGetNewFeatureNextUnlock(levelCurrent,
                        out newFeatureDataNext))
                {
                    levelUnlockNext = newFeatureDataNext.UnlockLevel;
                    if (NewFeatureGlobalConfig.Instance.TryGetNewFeaturePreviousUnlock(
                            levelCurrent,
                            out newFeatureDataPrevious))
                    {
                        levelUnlockPrevious = newFeatureDataPrevious.UnlockLevel;
                    }
                    
                    float countLevel = levelUnlockNext - levelUnlockPrevious;
                    float countLevelCurrent = GameManager.Instance.Level.CurrentValue - levelUnlockPrevious - 1;
                    float countLevelNext = countLevelCurrent + 1;
                    
                    Debug.Log("countLevel: " + countLevel);
                    Debug.Log("countLevelCurrent: " + countLevelCurrent);
                    Debug.Log("countLevelNext: " + countLevelNext);
                    
                    float featureProgressPrevious = (float)countLevelCurrent/countLevel;
                    float featureProgressNext = (float)countLevelNext/countLevel;
                    
                    await View.PlayAnimFeature(featureProgressPrevious, featureProgressNext);
                }
            }

            public void DidEnter(Memory<object> args)
            {
                UpdateState().Forget();
            }

            public UniTask Cleanup(Memory<object> args)
            {
                View.RotateHandle.Cancel();
                return UniTask.CompletedTask;
            }
        }
    }
}