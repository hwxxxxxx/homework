using System;
using System.Collections.Generic;

public static class EventBus
{
    private static readonly Dictionary<Type, List<Delegate>> SubscribersByType =
        new Dictionary<Type, List<Delegate>>();

    public static IDisposable Subscribe<TEvent>(Action<TEvent> handler) where TEvent : IGameEvent
    {
        Type eventType = typeof(TEvent);
        if (!SubscribersByType.TryGetValue(eventType, out List<Delegate> handlers))
        {
            handlers = new List<Delegate>();
            SubscribersByType[eventType] = handlers;
        }

        handlers.Add(handler);
        return new EventSubscription(() => Unsubscribe(handler));
    }

    public static void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : IGameEvent
    {
        Type eventType = typeof(TEvent);
        if (!SubscribersByType.TryGetValue(eventType, out List<Delegate> handlers))
        {
            return;
        }

        handlers.Remove(handler);
        if (handlers.Count == 0)
        {
            SubscribersByType.Remove(eventType);
        }
    }

    public static void Publish<TEvent>(TEvent gameEvent) where TEvent : IGameEvent
    {
        Type eventType = typeof(TEvent);
        if (!SubscribersByType.TryGetValue(eventType, out List<Delegate> handlers))
        {
            return;
        }

        Delegate[] executionList = handlers.ToArray();
        for (int i = 0; i < executionList.Length; i++)
        {
            Action<TEvent> action = executionList[i] as Action<TEvent>;
            action?.Invoke(gameEvent);
        }
    }

    private sealed class EventSubscription : IDisposable
    {
        private Action disposeAction;

        public EventSubscription(Action disposeAction)
        {
            this.disposeAction = disposeAction;
        }

        public void Dispose()
        {
            if (disposeAction == null)
            {
                return;
            }

            disposeAction.Invoke();
            disposeAction = null;
        }
    }
}
