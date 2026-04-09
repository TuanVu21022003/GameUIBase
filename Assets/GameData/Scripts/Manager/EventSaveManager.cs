using System;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class EventSaveManager : MonoBehaviour
{
    private static readonly Stack<Action> EventStack = new();
    private static readonly Stack<Action> CheckStickerDoneStack = new();
    
    private void Start()
    {
        this.UpdateAsObservable().Subscribe(ExecuteNextFrameEvents).AddTo(this);
    }

    private static void ExecuteNextFrameEvents(Unit _)
    {
        while (EventStack.Count > 0)
        {
            Action thisEvent = EventStack.Pop();
            thisEvent?.Invoke();
        }

        if (CheckStickerDoneStack.Count > 0)
        {
            Action thisEvent = CheckStickerDoneStack.Pop();
            thisEvent?.Invoke();
        }
    }

    public static void AddEventNextFrame(Action listener)
    {
        EventStack.Push(listener);
    }

    public static void AddCheckStickerDoneStack(Action listener)
    {
        CheckStickerDoneStack.Push(listener);
    }
}
