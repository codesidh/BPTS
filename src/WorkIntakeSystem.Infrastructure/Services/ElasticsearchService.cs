using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WorkIntakeSystem.Core.Interfaces;

namespace WorkIntakeSystem.Infrastructure.Services
{
    public class ElasticsearchService : IElasticsearchService
    {
        private readonly ILogger<ElasticsearchService> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly bool _elasticsearchEnabled;

        public ElasticsearchService(
            ILogger<ElasticsearchService> logger,
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _logger = logger;
            _httpClient = httpClient;
            _baseUrl = configuration["Monitoring:Elasticsearch:Url"] ?? "http://localhost:9200";
            _elasticsearchEnabled = configuration.GetValue<bool>("Monitoring:Elasticsearch:Enabled", true);
        }

        #region Index Management

        public async Task<bool> CreateIndexAsync(string indexName, string mapping = null)
        {
            if (!_elasticsearchEnabled) return true;

            try
            {
                var url = $"{_baseUrl}/{indexName}";
                var content = mapping != null 
                    ? new StringContent(mapping, Encoding.UTF8, "application/json")
                    : null;

                var response = await _httpClient.PutAsync(url, content);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully created Elasticsearch index: {IndexName}", indexName);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Failed to create Elasticsearch index {IndexName}. Status: {StatusCode}", 
                        indexName, response.StatusCode);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Elasticsearch index: {IndexName}", indexName);
                return false;
            }
        }

        public async Task<bool> DeleteIndexAsync(string indexName)
        {
            if (!_elasticsearchEnabled) return true;

            try
            {
                var url = $"{_baseUrl}/{indexName}";
                var response = await _httpClient.DeleteAsync(url);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully deleted Elasticsearch index: {IndexName}", indexName);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Failed to delete Elasticsearch index {IndexName}. Status: {StatusCode}", 
                        indexName, response.StatusCode);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Elasticsearch index: {IndexName}", indexName);
                return false;
            }
        }

        public async Task<bool> IndexExistsAsync(string indexName)
        {
            if (!_elasticsearchEnabled) return true;

            try
            {
                var url = $"{_baseUrl}/{indexName}";
                var request = new HttpRequestMessage(HttpMethod.Head, url);
                var response = await _httpClient.SendAsync(request);
                
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if Elasticsearch index exists: {IndexName}", indexName);
                return false;
            }
        }

        #endregion

        #region Document Operations

        public async Task<bool> IndexDocumentAsync(string indexName, string id, object document)
        {
            if (!_elasticsearchEnabled) return true;

            try
            {
                var json = JsonSerializer.Serialize(document);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var url = $"{_baseUrl}/{indexName}/_doc/{id}";
                
                var response = await _httpClient.PutAsync(url, content);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogDebug("Successfully indexed document in {IndexName} with ID: {Id}", indexName, id);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Failed to index document in {IndexName}. Status: {StatusCode}", 
                        indexName, response.StatusCode);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error indexing document in {IndexName} with ID: {Id}", indexName, id);
                return false;
            }
        }

        public async Task<bool> UpdateDocumentAsync(string indexName, string id, object document)
        {
            if (!_elasticsearchEnabled) return true;

            try
            {
                var updateDoc = new { doc = document };
                var json = JsonSerializer.Serialize(updateDoc);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var url = $"{_baseUrl}/{indexName}/_update/{id}";
                
                var response = await _httpClient.PostAsync(url, content);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogDebug("Successfully updated document in {IndexName} with ID: {Id}", indexName, id);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Failed to update document in {IndexName}. Status: {StatusCode}", 
                        indexName, response.StatusCode);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating document in {IndexName} with ID: {Id}", indexName, id);
                return false;
            }
        }

        public async Task<bool> DeleteDocumentAsync(string indexName, string id)
        {
            if (!_elasticsearchEnabled) return true;

            try
            {
                var url = $"{_baseUrl}/{indexName}/_doc/{id}";
                var response = await _httpClient.DeleteAsync(url);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogDebug("Successfully deleted document in {IndexName} with ID: {Id}", indexName, id);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Failed to delete document in {IndexName}. Status: {StatusCode}", 
                        indexName, response.StatusCode);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document in {IndexName} with ID: {Id}", indexName, id);
                return false;
            }
        }

        public async Task<T> GetDocumentAsync<T>(string indexName, string id) where T : class
        {
            if (!_elasticsearchEnabled) return null;

            try
            {
                var url = $"{_baseUrl}/{indexName}/_doc/{id}";
                var response = await _httpClient.GetAsync(url);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var jsonDoc = JsonSerializer.Deserialize<JsonElement>(content);
                    
                    if (jsonDoc.TryGetProperty("_source", out var source))
                    {
                        var sourceJson = source.GetRawText();
                        return JsonSerializer.Deserialize<T>(sourceJson);
                    }
                }
                else
                {
                    _logger.LogWarning("Failed to get document from {IndexName}. Status: {StatusCode}", 
                        indexName, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting document from {IndexName} with ID: {Id}", indexName, id);
            }
            
            return null;
        }

        #endregion

        #region Search Operations

        public async Task<SearchResult<T>> SearchAsync<T>(string indexName, string query) where T : class
        {
            if (!_elasticsearchEnabled) return new SearchResult<T>();

            try
            {
                var content = new StringContent(query, Encoding.UTF8, "application/json");
                var url = $"{_baseUrl}/{indexName}/_search";
                
                var response = await _httpClient.PostAsync(url, content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var searchResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    
                    return ParseSearchResult<T>(searchResponse);
                }
                else
                {
                    _logger.LogWarning("Failed to search in {IndexName}. Status: {StatusCode}", 
                        indexName, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching in {IndexName}", indexName);
            }
            
            return new SearchResult<T>();
        }

        public async Task<SearchResult<T>> SearchAsync<T>(string indexName, Dictionary<string, object> searchCriteria) where T : class
        {
            if (!_elasticsearchEnabled) return new SearchResult<T>();

            try
            {
                var query = JsonSerializer.Serialize(searchCriteria);
                return await SearchAsync<T>(indexName, query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching in {IndexName} with criteria", indexName);
                return new SearchResult<T>();
            }
        }

        public async Task<AggregationResult> AggregateAsync(string indexName, string aggregationQuery)
        {
            if (!_elasticsearchEnabled) return new AggregationResult();

            try
            {
                var content = new StringContent(aggregationQuery, Encoding.UTF8, "application/json");
                var url = $"{_baseUrl}/{indexName}/_search";
                
                var response = await _httpClient.PostAsync(url, content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var searchResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    
                    return ParseAggregationResult(searchResponse);
                }
                else
                {
                    _logger.LogWarning("Failed to aggregate in {IndexName}. Status: {StatusCode}", 
                        indexName, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error aggregating in {IndexName}", indexName);
            }
            
            return new AggregationResult();
        }

        #endregion

        #region Bulk Operations

        public async Task<bool> BulkIndexAsync(string indexName, IEnumerable<object> documents)
        {
            if (!_elasticsearchEnabled) return true;

            try
            {
                var bulkData = new StringBuilder();
                
                foreach (var doc in documents)
                {
                    // Add index action
                    var indexAction = new { index = new { _index = indexName } };
                    bulkData.AppendLine(JsonSerializer.Serialize(indexAction));
                    
                    // Add document
                    bulkData.AppendLine(JsonSerializer.Serialize(doc));
                }
                
                var content = new StringContent(bulkData.ToString(), Encoding.UTF8, "application/x-ndjson");
                var url = $"{_baseUrl}/_bulk";
                
                var response = await _httpClient.PostAsync(url, content);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully bulk indexed documents in {IndexName}", indexName);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Failed to bulk index documents in {IndexName}. Status: {StatusCode}", 
                        indexName, response.StatusCode);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk indexing documents in {IndexName}", indexName);
                return false;
            }
        }

        public async Task<bool> BulkUpdateAsync(string indexName, IEnumerable<KeyValuePair<string, object>> documents)
        {
            if (!_elasticsearchEnabled) return true;

            try
            {
                var bulkData = new StringBuilder();
                
                foreach (var kvp in documents)
                {
                    // Add update action
                    var updateAction = new { update = new { _index = indexName, _id = kvp.Key } };
                    bulkData.AppendLine(JsonSerializer.Serialize(updateAction));
                    
                    // Add document
                    var updateDoc = new { doc = kvp.Value };
                    bulkData.AppendLine(JsonSerializer.Serialize(updateDoc));
                }
                
                var content = new StringContent(bulkData.ToString(), Encoding.UTF8, "application/x-ndjson");
                var url = $"{_baseUrl}/_bulk";
                
                var response = await _httpClient.PostAsync(url, content);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully bulk updated documents in {IndexName}", indexName);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Failed to bulk update documents in {IndexName}. Status: {StatusCode}", 
                        indexName, response.StatusCode);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk updating documents in {IndexName}", indexName);
                return false;
            }
        }

        #endregion

        #region Monitoring

        public async Task<ClusterHealth> GetClusterHealthAsync()
        {
            if (!_elasticsearchEnabled) return new ClusterHealth { Status = "Unknown" };

            try
            {
                var url = $"{_baseUrl}/_cluster/health";
                var response = await _httpClient.GetAsync(url);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<ClusterHealth>(content);
                }
                else
                {
                    _logger.LogWarning("Failed to get cluster health. Status: {StatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cluster health");
            }
            
            return new ClusterHealth { Status = "Unknown" };
        }

        public async Task<IndexStats> GetIndexStatsAsync(string indexName)
        {
            if (!_elasticsearchEnabled) return new IndexStats { IndexName = indexName };

            try
            {
                var url = $"{_baseUrl}/{indexName}/_stats";
                var response = await _httpClient.GetAsync(url);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var statsResponse = JsonSerializer.Deserialize<JsonElement>(content);
                    
                    if (statsResponse.TryGetProperty("indices", out var indices) &&
                        indices.TryGetProperty(indexName, out var indexStats))
                    {
                        return ParseIndexStats(indexName, indexStats);
                    }
                }
                else
                {
                    _logger.LogWarning("Failed to get index stats for {IndexName}. Status: {StatusCode}", 
                        indexName, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting index stats for {IndexName}", indexName);
            }
            
            return new IndexStats { IndexName = indexName };
        }

        public async Task<List<string>> GetIndicesAsync()
        {
            if (!_elasticsearchEnabled) return new List<string>();

            try
            {
                var url = $"{_baseUrl}/_cat/indices?format=json";
                var response = await _httpClient.GetAsync(url);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var indices = JsonSerializer.Deserialize<List<JsonElement>>(content);
                    
                    var indexNames = new List<string>();
                    foreach (var index in indices)
                    {
                        if (index.TryGetProperty("index", out var indexName))
                        {
                            indexNames.Add(indexName.GetString());
                        }
                    }
                    
                    return indexNames;
                }
                else
                {
                    _logger.LogWarning("Failed to get indices. Status: {StatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting indices");
            }
            
            return new List<string>();
        }

        #endregion

        #region Private Methods

        private SearchResult<T> ParseSearchResult<T>(JsonElement searchResponse) where T : class
        {
            var result = new SearchResult<T>();
            
            try
            {
                if (searchResponse.TryGetProperty("took", out var took))
                {
                    result.Took = TimeSpan.FromMilliseconds(took.GetInt64());
                }
                
                if (searchResponse.TryGetProperty("timed_out", out var timedOut))
                {
                    result.TimedOut = timedOut.GetBoolean();
                }
                
                if (searchResponse.TryGetProperty("hits", out var hits))
                {
                    if (hits.TryGetProperty("total", out var total))
                    {
                        if (total.TryGetProperty("value", out var totalValue))
                        {
                            result.TotalHits = totalValue.GetInt64();
                        }
                    }
                    
                    if (hits.TryGetProperty("max_score", out var maxScore))
                    {
                        result.MaxScore = maxScore.GetDouble();
                    }
                    
                    if (hits.TryGetProperty("hits", out var hitsArray))
                    {
                        foreach (var hit in hitsArray.EnumerateArray())
                        {
                            if (hit.TryGetProperty("_source", out var source))
                            {
                                var sourceJson = source.GetRawText();
                                var document = JsonSerializer.Deserialize<T>(sourceJson);
                                if (document != null)
                                {
                                    result.Documents.Add(document);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing search result");
            }
            
            return result;
        }

        private AggregationResult ParseAggregationResult(JsonElement searchResponse)
        {
            var result = new AggregationResult();
            
            try
            {
                if (searchResponse.TryGetProperty("took", out var took))
                {
                    result.Took = TimeSpan.FromMilliseconds(took.GetInt64());
                }
                
                if (searchResponse.TryGetProperty("hits", out var hits) &&
                    hits.TryGetProperty("total", out var total))
                {
                    if (total.TryGetProperty("value", out var totalValue))
                    {
                        result.TotalHits = totalValue.GetInt64();
                    }
                }
                
                if (searchResponse.TryGetProperty("aggregations", out var aggregations))
                {
                    foreach (var aggregation in aggregations.EnumerateObject())
                    {
                        result.Aggregations[aggregation.Name] = aggregation.Value.GetRawText();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing aggregation result");
            }
            
            return result;
        }

        private IndexStats ParseIndexStats(string indexName, JsonElement indexStats)
        {
            var stats = new IndexStats { IndexName = indexName };
            
            try
            {
                if (indexStats.TryGetProperty("total", out var total))
                {
                    if (total.TryGetProperty("docs", out var docs) &&
                        docs.TryGetProperty("count", out var docCount))
                    {
                        stats.DocumentCount = docCount.GetInt64();
                    }
                    
                    if (total.TryGetProperty("store", out var store) &&
                        store.TryGetProperty("size_in_bytes", out var sizeInBytes))
                    {
                        stats.StorageSize = sizeInBytes.GetInt64();
                    }
                }
                
                if (indexStats.TryGetProperty("primaries", out var primaries))
                {
                    if (primaries.TryGetProperty("indexing", out var indexing) &&
                        indexing.TryGetProperty("index_total", out var indexTotal))
                    {
                        stats.IndexingRate = indexTotal.GetInt64();
                    }
                    
                    if (primaries.TryGetProperty("search", out var search) &&
                        search.TryGetProperty("query_total", out var queryTotal))
                    {
                        stats.SearchRate = queryTotal.GetInt64();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing index stats for {IndexName}", indexName);
            }
            
            return stats;
        }

        #endregion
    }
} 