using System;
using Core.UI.Modals;
using CoreData;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using GameUI;
using LitMotion;
using R3;
using TMPro;
using TW.UGUI.Core.Modals;
using TW.UGUI.Core.Views;
using TW.Utility.CustomType;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI.Other
{
    public class UICurrencyResource : MonoBehaviour
    {
        [field: SerializeField] public GameResource.Type CurrencyType { get; private set; } = GameResource.Type.None;
        [field: SerializeField] protected TextMeshProUGUI TextAmount { get; set; }
        [field: SerializeField] protected bool RealValue { get; set; } = true;
        [field: SerializeField] public Button ShowResourceBtn { get; set; }
        [field: SerializeField] protected bool IsClick { get; set; } = false;
        [field: SerializeField] protected GameObject PlusObj { get; set; }

        protected BigNumber Amount { get; set; }
        protected BigNumber AmountChange;

        private void Start()
        {
            GameResource gameResource = PlayerResourceManager.Instance.GetGameResource(CurrencyType);
            gameResource.ReactiveAmount.Subscribe(OnResourceChange).AddTo(this);
            if (ShowResourceBtn != null)
            {
                ShowResourceBtn.SetOnClickDestination(ShowResource);
            }
            OnInit();
        }

        public virtual void OnInit()
        {
            PlusObj.SetActive(IsClick);
        }


        protected virtual void OnResourceChange(BigNumber value)
        {
            
        }

        protected virtual async UniTask ShowResource()
        {
            
        }
        
        protected virtual void ChangeText(BigNumber val)
        {
            Amount = val;
        }
    }
}