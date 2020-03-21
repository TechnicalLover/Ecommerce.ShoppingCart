using System;

namespace ShoppingCartService.EventFeed
{
    public class EventMetadata
    {
        public DateTimeOffset OccurredAt { get; set; }

        public string EventName { get; set; }
    }
}