using CoreData;

public interface IDataSave<out T> where T : class, IDataSave<T>
{
    bool IsDirty { get; set; }
    T DefaultData();
}

public static class DataSaveExtensions
{
    public static void SaveData<T>(this T dataSave) where T : class, IDataSave<T>
    {
        EventSaveManager.AddEventNextFrame(() => SaveDataImmediate(dataSave));
    }

    private static void SaveDataImmediate<T>(this T dataSave) where T : class, IDataSave<T>
    {
        // #if UNITY_EDITOR
        // Debug.Log("Save Data: " + typeof(T).Name + " - " + JsonUtility.ToJson(dataSave, true));
        // #endif
        DataSerializer.SaveDataPrefs(dataSave);
    }
}