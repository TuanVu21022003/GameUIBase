using System;
using Sirenix.OdinInspector;
using TW.Utility.CustomType;
using UnityEngine;

[Serializable]
public class GameResource
{
    public enum Type
    {
        None = 0,
        Money = 1,
        Gem = 2,
        Heart = 3,
        NoAds = 4,
    }
    
    [field: HideLabel, HorizontalGroup(nameof(GameResource), 100)]
    [field: SerializeField] public Type ResourceType { get; private set; }
    
    [field: SerializeField, HideLabel, HorizontalGroup(nameof(GameResource))]
    public Reactive<BigNumber> ReactiveAmount { get; private set; } = new();

    public BigNumber Amount
    {
        get => ReactiveAmount.Value;
        set => ReactiveAmount.Value = value;
    }
    
    
    public GameResourceData GameResourceData { get; set; } = new();

    public GameResource()
    {
        
    }

    
    public GameResource(Type resourceType, BigNumber amount)
    {
        ResourceType = resourceType;
        Amount = amount;
    }
    
    public GameResourceData ToGameResourceData()
    {
        return new GameResourceData()
        {
            ResourceType = ResourceType,
            C = Amount.coefficient,
            E = Amount.exponent,
        };
    }
    public GameResource FromGameResourceData(GameResourceData gameResourceData)
    {
        ResourceType = gameResourceData.ResourceType;
        Amount = new BigNumber(gameResourceData.C, gameResourceData.E);
        return this;
    }

    public void Add(BigNumber value)
    {
        Amount += value;
    }
    public void Consume(BigNumber value)
    {
        Amount -= value;
    }
    public bool IsEnough(BigNumber value, float threshold = 0.001f)
    {
        return value <= Amount + threshold;
    }

}

[Serializable]
public class GameResourceData
{
    [field: SerializeField] public GameResource.Type ResourceType { get; set; }
    [field: SerializeField] public double C { get; set; }
    [field: SerializeField] public int E { get; set; }
}