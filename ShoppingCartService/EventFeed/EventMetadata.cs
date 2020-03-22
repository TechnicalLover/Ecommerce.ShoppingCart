namespace ShoppingCartService.EventFeed
{
    using System;

    public class EventMetadata
    {
        public DateTimeOffset OccurredAt { get; set; }

        public string EventName { get; set; }
    }
}