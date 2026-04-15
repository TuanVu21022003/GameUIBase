
using Core.UI.Activities;
using Cysharp.Threading.Tasks;
using GameUI;
using UnityEngine;
using ZBase.UnityScreenNavigator.Core;

namespace GameUI
{
    public class Launcher : UnityScreenNavigatorLauncher
    {
        [SerializeField] private GameObject loadngFake;

        protected override void Awake()
        {
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            base.Awake();
        }


        protected override void Start()
        {
            base.Start();
            StartLoading().Forget();
        }

        private async UniTask StartLoading()
        {
            await UIManager.Instance.OpenActivityAsync<ActivityLoadingFirstGame>();
            loadngFake.gameObject.SetActive(false);
        }
    }
}
