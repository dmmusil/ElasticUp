using Nest;
using System;

namespace ElasticUp.History
{
    public class ElasticUpMigrationHistory
    {
        [Keyword(Index = true)]
        public string ElasticUpMigrationName { get; set; }

        [Keyword(Index = true)]
        public string ElasticUpOperationName { get; set; }

        public DateTime ElasticUpMigrationApplied { get; set; } = DateTime.UtcNow;
    }
}
