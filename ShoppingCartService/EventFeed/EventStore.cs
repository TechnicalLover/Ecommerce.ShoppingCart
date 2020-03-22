namespace ShoppingCartService.EventFeed
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using global::EventStore.ClientAPI;
    using Newtonsoft.Json;
    using ShoppingCartService.Models.Configurations;
    using ShoppingCartService.Models.Dto;

    public class EventStore : IEventStore
    {
        private readonly IEventStoreConnection _connection;

        public EventStore(EventStoreConfig config)
        {
            if (string.IsNullOrWhiteSpace(config.ConnectionString))
                throw new ArgumentException(nameof(config.ConnectionString));

            _connection = EventStoreConnection.Create(config.ConnectionString);
        }

        public async Task<IEnumerable<Event>> GetEvents(long firstEventSequenceNumber, long lastEventSequenceNumber)
        {
            await _connection.ConnectAsync().ConfigureAwait(false);

            var result = await _connection.ReadStreamEventsForwardAsync(
                "ShoppingCart",
                start: (int)firstEventSequenceNumber,
                count: (int)(lastEventSequenceNumber - firstEventSequenceNumber),
                resolveLinkTos: false
            ).ConfigureAwait(false);

            return result.Events.Select(@event => new
            {
                Content = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(@event.Event.Data)),
                Metadata = JsonConvert.DeserializeObject<EventMetadata>(Encoding.UTF8.GetString(@event.Event.Metadata))
            })
            .Select((@event, i) =>
                new Event(
                    i + firstEventSequenceNumber,
                    @event.Metadata.OccurredAt,
                    @event.Metadata.EventName,
                    @event.Content
                ));
        }

        public async Task Raise(string eventName, object content)
        {
            await _connection.ConnectAsync().ConfigureAwait(false);
            var contentJson = JsonConvert.SerializeObject(content);
            var metadataJson = JsonConvert.SerializeObject(new EventMetadata
            {
                OccurredAt = DateTimeOffset.UtcNow,
                EventName = eventName
            });

            var eventData = new EventData(
                Guid.NewGuid(),
                "ShoppingCartEvent",
                isJson: true,
                data: Encoding.UTF8.GetBytes(contentJson),
                metadata: Encoding.UTF8.GetBytes(metadataJson)
            );

            await _connection.AppendToStreamAsync(
                "ShoppingCart",
                ExpectedVersion.Any,
                eventData
            );
        }
    }
}