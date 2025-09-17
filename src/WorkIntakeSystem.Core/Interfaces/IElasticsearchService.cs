using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WorkIntakeSystem.Core.Interfaces
{
    public interface IElasticsearchService
    {
        // Index Management
        Task<bool> CreateIndexAsync(string indexName, string mapping = null);
        Task<bool> DeleteIndexAsync(string indexName);
        Task<bool> IndexExistsAsync(string indexName);
        
        // Document Operations
        Task<bool> IndexDocumentAsync(string indexName, string id, object document);
        Task<bool> UpdateDocumentAsync(string indexName, string id, object document);
        Task<bool> DeleteDocumentAsync(string indexName, string id);
        Task<T> GetDocumentAsync<T>(string indexName, string id) where T : class;
        
        // Search Operations
        Task<SearchResult<T>> SearchAsync<T>(string indexName, string query) where T : class;
        Task<SearchResult<T>> SearchAsync<T>(string indexName, Dictionary<string, object> searchCriteria) where T : class;
        Task<AggregationResult> AggregateAsync(string indexName, string aggregationQuery);
        
        // Bulk Operations
        Task<bool> BulkIndexAsync(string indexName, IEnumerable<object> documents);
        Task<bool> BulkUpdateAsync(string indexName, IEnumerable<KeyValuePair<string, object>> documents);
        
        // Monitoring
        Task<ClusterHealth> GetClusterHealthAsync();
        Task<IndexStats> GetIndexStatsAsync(string indexName);
        Task<List<string>> GetIndicesAsync();
    }

    public class SearchResult<T>
    {
        public List<T> Documents { get; set; } = new List<T>();
        public long TotalHits { get; set; }
        public double MaxScore { get; set; }
        public TimeSpan Took { get; set; }
        public bool TimedOut { get; set; }
    }

    public class AggregationResult
    {
        public Dictionary<string, object> Aggregations { get; set; } = new Dictionary<string, object>();
        public long TotalHits { get; set; }
        public TimeSpan Took { get; set; }
    }

    public class ClusterHealth
    {
        public required string Status { get; set; }
        public int NumberOfNodes { get; set; }
        public int NumberOfDataNodes { get; set; }
        public int ActivePrimaryShards { get; set; }
        public int ActiveShards { get; set; }
        public int RelocatingShards { get; set; }
        public int InitializingShards { get; set; }
        public int UnassignedShards { get; set; }
        public int DelayedUnassignedShards { get; set; }
        public int NumberOfPendingTasks { get; set; }
        public int NumberOfInFlightFetch { get; set; }
        public TimeSpan TaskMaxWaitTimeInQueue { get; set; }
        public double ActiveShardsPercentAsNumber { get; set; }
    }

    public class IndexStats
    {
        public required string IndexName { get; set; }
        public long DocumentCount { get; set; }
        public long StorageSize { get; set; }
        public long IndexingRate { get; set; }
        public long SearchRate { get; set; }
        public TimeSpan RefreshTime { get; set; }
        public TimeSpan FlushTime { get; set; }
    }
} 