namespace ShoppingCartService.EventFeed
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Data.SqlClient;
    using Newtonsoft.Json;
    using ShoppingCartService.Models.Configurations;
    using ShoppingCartService.Models.Dto;
    using Dapper;

    public class SqlEventStore : IEventStore
    {
        private readonly string _connectionString;

        public SqlEventStore(EventStoreConfig config)
        {
            _connectionString = config.ConnectionString;
            if (string.IsNullOrWhiteSpace(_connectionString))
                throw new ArgumentException(nameof(config.ConnectionString));
        }

        public async Task<IEnumerable<Event>> GetEvents(
            long firstEventSequenceNumber,
            long lastEventSequenceNumber)
        {
            string readEventsSql = @"select * from EventStore where ID >= @Start and ID <= @End";
            using (var conn = new SqlConnection(_connectionString))
            {
                return (await conn.QueryAsync<dynamic>(
                    readEventsSql,
                    new
                    {
                        Start = firstEventSequenceNumber,
                        End = lastEventSequenceNumber
                    }).ConfigureAwait(false))
                    .Select(row =>
                    {
                        var content = JsonConvert.DeserializeObject(row.Content);
                        return new Event(row.ID, row.OccurredAt, row.Name, content);
                    });
            }
        }

        public async Task Raise(string eventName, object content)
        {
            string writeEventSql = @"insert into EventStore(Name, OccurredAt, Content) values (@Name, @OccurredAt, @Content)";
            var jsonContent = JsonConvert.SerializeObject(content);
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.ExecuteAsync(
                    writeEventSql,
                    new
                    {
                        Name = eventName,
                        OccurredAd = DateTimeOffset.UtcNow,
                        Content = jsonContent
                    });
            }
        }
    }
}