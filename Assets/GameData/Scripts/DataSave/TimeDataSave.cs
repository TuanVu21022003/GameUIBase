using System;
using UnityEngine;

[Serializable]
public class TimeDataSave : IDataSave<TimeDataSave>
{
    public static TimeDataSave Instance => InGameDataManager.Instance.InGameData.TimeDataSave;
    [field: SerializeField] public Reactive<string> TimeLastDay { get; set; } = new("");
    [field: SerializeField] public string timeInstall { get; set; } = new("");
    public bool IsDirty { get; set; }

    public TimeDataSave DefaultData()
    {
        return this;
    }
}