namespace EventBus;

public interface IEventBusSubscriptionManager
{
    bool IsEmpty { get; }
    event EventHandler<string> OnEventRemoved;

    void AddDynmaicSubscription<TH>(string eventName)
        where TH : IDynamicIntegrationEventHandler;

    void AddSubscription<T, TH>()
        where T : IntegrationEvent
        where TH : IIntegrationEventHandler<T>;

    void RemoveSubscription<T, TH>()
        where T : IntegrationEvent
        where TH : IIntegrationEventHandler<T>;

    void RemoveDynamicSubscription<TH>(string eventName)
        where TH : IDynamicIntegrationEventHandler;

    bool HasSubscriptionForEvent<T>() where T : IntegrationEvent;
    bool HasSubscriptionForEvent(string eventName);
    Type GetEventTypeByName(string eventName);
    void Clear();

    IEnumerable<InMemoryEventBusSubscriptionManager.SubscriptionInfo> GetHandlersForEvent<T>()
        where T : IntegrationEvent;

    IEnumerable<InMemoryEventBusSubscriptionManager.SubscriptionInfo> GetHandlersForEvent(string eventName);
    
    string GetEventKey<T>();
}