namespace EventBus.Abstractions;

public interface IEventBus
{
    void publish(IntegrationEvent @event);
    
    void subscribe<T, TH>()
        where T : IntegrationEvent
        where TH : IIntegrationEventHandler<T>;
    
    void unsubscribe<T, TH>()
        where T : IntegrationEvent
        where TH : IIntegrationEventHandler<T>;
}