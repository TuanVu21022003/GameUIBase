using System;
using AssetKits.ParticleImage;
using Manager;
using UnityEngine;
using UnityEngine.Events;

public class CoinEffectUI : MonoBehaviour
{
    [field: SerializeField] public ParticleImage ParticleImage { get; set; }

    private void Start()
    {
        gameObject.SetActive(false);
    }

    public CoinEffectUI OnInit(Vector3 posStart, Transform target, Action callbackOnFirst, Action callbackOnLast)
    {
        transform.position = posStart;
        ParticleImage.attractorTarget = target;
        ParticleImage.onFirstParticleFinishAction = callbackOnFirst;
        ParticleImage.onLastParticleFinishAction = callbackOnLast;
        ParticleImage.onParticleFinish.AddListener(OnParticleFinish);
        gameObject.SetActive(true);
        PlayAnimation();
        return this;
    }
    
    private void PlayAnimation()
    {
        ParticleImage.Play();
    }

    private void OnParticleFinish()
    {
        AudioManager.OnPlayCoinEffect?.Invoke();
    }
}
