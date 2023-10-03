

namespace Catalog.API.IntegrationEvents;

public class CatalogIntegrationEventService : ICatalogIntegrationEventService, IDisposable
{
    private volatile bool disposedValue;
    private readonly CatalogDbContext _catalogDbContext;
    private readonly IEventBus _eventBus;
    private readonly ILogger<CatalogIntegrationEventService> _logger;


    public CatalogIntegrationEventService(CatalogDbContext catalogDbContext, IEventBus eventBus, ILogger<CatalogIntegrationEventService> logger)
    {
        _catalogDbContext = catalogDbContext ?? throw new ArgumentNullException(nameof(catalogDbContext));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task PublishThroughEventBusAsync(IntegrationEvent evt)
    {
        try
        {
            // TODO: Save event to integration event log
            _logger.LogInformation("----- Publishing integration event: {IntegrationEventId}  - ({@IntegrationEvent})",
                evt.Id, evt);
            _eventBus.Publish(evt);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "ERROR Publishing integration event: {IntegrationEventId}  - ({@IntegrationEvent})", evt.Id, evt);
        }
    }
    
    public async Task SaveEventAndCatalogContextChangesAsync(IntegrationEvent evt)
    {
       _logger.LogInformation("----- CatalogIntegrationEventService - Saving changes and integrationEvent: {IntegrationEventId} - ({@IntegrationEvent})", evt.Id, evt);
       await _catalogDbContext.SaveChangesAsync();
       
       // TODO: Save event to integration event log

    }

    
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: Dispose event log service
            }
            
            disposedValue = true;
        }
    }
    public void Dispose()
    {
        Dispose(disposing:true);
        GC.SuppressFinalize(this);
    }
}