using System;
using System.Collections.Generic;
using UnityEngine;

public static class EventBus
{
    // —ловарь дл€ хранени€ всех событий и их подписчиков
    private static Dictionary<Type, List<Delegate>> _eventListeners = new Dictionary<Type, List<Delegate>>();

    // ѕќƒѕ»—ј“№—я на событие
    public static void Subscribe<T>(Action<T> listener) where T : class
    {
        Type eventType = typeof(T);

        if (!_eventListeners.ContainsKey(eventType))
        {
            _eventListeners[eventType] = new List<Delegate>();
        }

        _eventListeners[eventType].Add(listener);
    }

    // ќ“ѕ»—ј“№—я от событи€
    public static void Unsubscribe<T>(Action<T> listener) where T : class
    {
        Type eventType = typeof(T);

        if (_eventListeners.ContainsKey(eventType))
        {
            _eventListeners[eventType].Remove(listener);
        }
    }

    // ќѕ”ЅЋ» ќ¬ј“№ событие (сообщить всем подписчикам)
    public static void Publish<T>(T eventData) where T : class
    {
        Type eventType = typeof(T);

        if (_eventListeners.ContainsKey(eventType))
        {
            foreach (var listener in _eventListeners[eventType])
            {
                if (listener is Action<T> typedListener)
                {
                    typedListener(eventData);
                }
            }
        }
    }
}