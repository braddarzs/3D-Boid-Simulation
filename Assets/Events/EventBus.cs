using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public static class EventBus
{
    private static readonly Dictionary<GameEventType, Action<GameEventData>> eventTable =
        new Dictionary<GameEventType, Action<GameEventData>>();

    public static void Subscribe(GameEventType eventType, Action<GameEventData> handler)
    {
        if (!eventTable.ContainsKey(eventType))
        {
            eventTable[eventType] = delegate { };
        }

        eventTable[eventType] += handler;
    }

    public static void Unsubscribe(GameEventType eventType, Action<GameEventData> handler)
    {
        if (eventTable.ContainsKey(eventType))
        {
            eventTable[eventType] -= handler;
        }
    }

    public static void Raise(GameEventType eventType, GameEventData eventData)
    {
        if (eventTable.ContainsKey(eventType))
        {
            eventTable[eventType].Invoke(eventData);
        }
        else
        {
            UnityEngine.Debug.LogWarning($"No listeners for event: {eventType}");
        }
    }
}