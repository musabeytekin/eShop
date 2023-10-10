namespace Ordering.Infrastructure;

public static class MediatorExtension
{
    public static async Task DispatchDomainEventsAsync(this IMediator mediator, OrderingDbContext ctx)
    {
        
        // get all domain entity entries that have domain events
        var domainEntities = ctx.ChangeTracker
            .Entries<Entity>()
            .Where(x => x.Entity.DomainEvents != null && x.Entity.DomainEvents.Any());

        // get all domain events from all domain entity entries
        var domainEvents = domainEntities
            .SelectMany(x => x.Entity.DomainEvents)
            .ToList();

        // clear domain events from all domain entity entries
        domainEntities.ToList()
            .ForEach(entity => entity.Entity.ClearDomainEvents());

        // publish all domain events
        foreach (var domainEvent in domainEvents)
        {
            await mediator.Publish(domainEvent);
        }
    }
}