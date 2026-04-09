using Sirenix.OdinInspector;
using TW.Utility.DesignPattern;
using UnityEngine;

public class InGameDataManager : Singleton<InGameDataManager>
{
    [field: SerializeField] public InGameData InGameData { get; private set; } = new();

    protected override void Awake()
    {
        base.Awake();
        LoadData();
        
    }
    
    [Button]
    public void LoadData()
    {
        InGameData.LoadData();
    }
    
    [Button]
    public void SaveAllData()
    {
        InGameData.SaveAllData();
    }
    
    [Button]
    public void ResetData()
    {
        InGameData = new InGameData();  
        InGameData.SaveAllData();
    } 
}
