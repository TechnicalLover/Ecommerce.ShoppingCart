namespace ShoppingCartService.EventFeed
{
    using System;

    public class Event
    {
        public long SequenceNumber { get; set; }

        public DateTimeOffset OccurredAt { get; set; }

        public string Name { get; set; }

        public object Content { get; set; }

        public Event(
            long sequenceNumber,
            DateTimeOffset occurredAt,
            string name,
            object content)
        {
            SequenceNumber = sequenceNumber;
            OccurredAt = occurredAt;
            Name = name;
            Content = content;
        }
    }
}