using System;
using UnityEngine;

public interface IHeartState
{
    void SetActionCallback(Action action);
    void InitState();
    void CheckTimeState();
    void ResetTime();
    void SaveTime();
    void ReduceTime();
}
