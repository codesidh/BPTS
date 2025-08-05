using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;
using WorkIntakeSystem.Core.Interfaces;

namespace WorkIntakeSystem.Infrastructure.Services;

public class MessageTransformationService : IMessageTransformationService
{
    private readonly ILogger<MessageTransformationService> _logger;
    private readonly Dictionary<string, TransformationRule> _transformationRules;
    private readonly TransformationMetrics _metrics;
    private readonly object _rulesLock = new();
    private readonly object _metricsLock = new();

    public MessageTransformationService(ILogger<MessageTransformationService> logger)
    {
        _logger = logger;
        _transformationRules = new Dictionary<string, TransformationRule>();
        _metrics = new TransformationMetrics();
        InitializeDefaultRules();
    }

    private void InitializeDefaultRules()
    {
        // JSON to XML transformation
        var jsonToXmlRule = new TransformationRule
        {
            SourceFormat = "JSON",
            TargetFormat = "XML",
            TransformationScript = "json-to-xml",
            IsActive = true
        };

        // XML to JSON transformation
        var xmlToJsonRule = new TransformationRule
        {
            SourceFormat = "XML",
            TargetFormat = "JSON",
            TransformationScript = "xml-to-json",
            IsActive = true
        };

        // CSV to JSON transformation
        var csvToJsonRule = new TransformationRule
        {
            SourceFormat = "CSV",
            TargetFormat = "JSON",
            TransformationScript = "csv-to-json",
            IsActive = true
        };

        lock (_rulesLock)
        {
            _transformationRules[$"{jsonToXmlRule.SourceFormat}-{jsonToXmlRule.TargetFormat}"] = jsonToXmlRule;
            _transformationRules[$"{xmlToJsonRule.SourceFormat}-{xmlToJsonRule.TargetFormat}"] = xmlToJsonRule;
            _transformationRules[$"{csvToJsonRule.SourceFormat}-{csvToJsonRule.TargetFormat}"] = csvToJsonRule;
        }
    }

    public async Task<object> TransformMessageAsync(object message, string sourceFormat, string targetFormat)
    {
        var startTime = DateTime.UtcNow;
        var success = false;

        try
        {
            _logger.LogInformation("Transforming message from {SourceFormat} to {TargetFormat}", sourceFormat, targetFormat);

            // If source and target formats are the same, return the message as-is
            if (string.Equals(sourceFormat, targetFormat, StringComparison.OrdinalIgnoreCase))
            {
                return message;
            }

            var transformedMessage = await TransformMessageInternalAsync(message, sourceFormat, targetFormat);
            
            success = true;
            UpdateMetrics(sourceFormat, targetFormat, DateTime.UtcNow - startTime, success);
            
            _logger.LogInformation("Message transformation completed successfully: {SourceFormat} -> {TargetFormat}", 
                sourceFormat, targetFormat);
            
            return transformedMessage;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to transform message from {SourceFormat} to {TargetFormat}", sourceFormat, targetFormat);
            UpdateMetrics(sourceFormat, targetFormat, DateTime.UtcNow - startTime, success);
            throw;
        }
    }

    private async Task<object> TransformMessageInternalAsync(object message, string sourceFormat, string targetFormat)
    {
        var ruleKey = $"{sourceFormat.ToUpper()}-{targetFormat.ToUpper()}";
        
        lock (_rulesLock)
        {
            if (!_transformationRules.TryGetValue(ruleKey, out var rule) || !rule.IsActive)
            {
                throw new InvalidOperationException($"No active transformation rule found for {sourceFormat} to {targetFormat}");
            }
        }

        return await Task.Run(() =>
        {
            return sourceFormat.ToUpper() switch
            {
                "JSON" when targetFormat.ToUpper() == "XML" => TransformJsonToXml(message),
                "XML" when targetFormat.ToUpper() == "JSON" => TransformXmlToJson(message),
                "CSV" when targetFormat.ToUpper() == "JSON" => TransformCsvToJson(message),
                "JSON" when targetFormat.ToUpper() == "CSV" => TransformJsonToCsv(message),
                _ => throw new NotSupportedException($"Transformation from {sourceFormat} to {targetFormat} is not supported")
            };
        });
    }

    private object TransformJsonToXml(object message)
    {
        try
        {
            var jsonString = JsonSerializer.Serialize(message);
            var jsonDoc = JsonDocument.Parse(jsonString);
            var xmlDoc = new XDocument();
            
            var rootElement = new XElement("root");
            ConvertJsonToXml(jsonDoc.RootElement, rootElement);
            xmlDoc.Add(rootElement);
            
            return xmlDoc.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to transform JSON to XML");
            throw;
        }
    }

    private void ConvertJsonToXml(JsonElement jsonElement, XElement parentElement)
    {
        switch (jsonElement.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in jsonElement.EnumerateObject())
                {
                    var childElement = new XElement(property.Name);
                    ConvertJsonToXml(property.Value, childElement);
                    parentElement.Add(childElement);
                }
                break;
            case JsonValueKind.Array:
                foreach (var item in jsonElement.EnumerateArray())
                {
                    var childElement = new XElement("item");
                    ConvertJsonToXml(item, childElement);
                    parentElement.Add(childElement);
                }
                break;
            case JsonValueKind.String:
                parentElement.Value = jsonElement.GetString() ?? string.Empty;
                break;
            case JsonValueKind.Number:
                parentElement.Value = jsonElement.GetDecimal().ToString();
                break;
            case JsonValueKind.True:
                parentElement.Value = "true";
                break;
            case JsonValueKind.False:
                parentElement.Value = "false";
                break;
            case JsonValueKind.Null:
                parentElement.Value = string.Empty;
                break;
        }
    }

    private object TransformXmlToJson(object message)
    {
        try
        {
            var xmlString = message.ToString() ?? string.Empty;
            var xmlDoc = XDocument.Parse(xmlString);
            var jsonObject = ConvertXmlToJson(xmlDoc.Root);
            return jsonObject;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to transform XML to JSON");
            throw;
        }
    }

    private object ConvertXmlToJson(XElement element)
    {
        if (!element.HasElements)
        {
            return element.Value ?? string.Empty;
        }

        var result = new Dictionary<string, object>();
        var groups = element.Elements().GroupBy(e => e.Name.LocalName);

        foreach (var group in groups)
        {
            if (group.Count() == 1)
            {
                result[group.Key] = ConvertXmlToJson(group.First());
            }
            else
            {
                result[group.Key] = group.Select(e => ConvertXmlToJson(e)).ToArray();
            }
        }

        return result;
    }

    private object TransformCsvToJson(object message)
    {
        try
        {
            var csvString = message.ToString() ?? string.Empty;
            var lines = csvString.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            
            if (lines.Length < 2)
            {
                return new { data = new object[0] };
            }

            var headers = lines[0].Split(',').Select(h => h.Trim().Trim('"')).ToArray();
            var data = new List<Dictionary<string, string>>();

            for (int i = 1; i < lines.Length; i++)
            {
                var values = ParseCsvLine(lines[i]);
                var row = new Dictionary<string, string>();
                
                for (int j = 0; j < Math.Min(headers.Length, values.Length); j++)
                {
                    row[headers[j]] = values[j];
                }
                
                data.Add(row);
            }

            return new { data = data.ToArray() };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to transform CSV to JSON");
            throw;
        }
    }

    private string[] ParseCsvLine(string line)
    {
        var result = new List<string>();
        var current = "";
        var inQuotes = false;
        
        for (int i = 0; i < line.Length; i++)
        {
            var c = line[i];
            
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(current.Trim());
                current = "";
            }
            else
            {
                current += c;
            }
        }
        
        result.Add(current.Trim());
        return result.ToArray();
    }

    private object TransformJsonToCsv(object message)
    {
        try
        {
            var jsonString = JsonSerializer.Serialize(message);
            var jsonDoc = JsonDocument.Parse(jsonString);
            
            if (jsonDoc.RootElement.ValueKind != JsonValueKind.Array)
            {
                throw new InvalidOperationException("JSON must be an array for CSV transformation");
            }

            var rows = new List<string>();
            var headers = new HashSet<string>();

            // First pass: collect all possible headers
            foreach (var item in jsonDoc.RootElement.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.Object)
                {
                    foreach (var property in item.EnumerateObject())
                    {
                        headers.Add(property.Name);
                    }
                }
            }

            var headerArray = headers.ToArray();
            rows.Add(string.Join(",", headerArray.Select(h => $"\"{h}\"")));

            // Second pass: create rows
            foreach (var item in jsonDoc.RootElement.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.Object)
                {
                    var row = new List<string>();
                    foreach (var header in headerArray)
                    {
                        var value = item.TryGetProperty(header, out var prop) ? prop.GetString() ?? "" : "";
                        row.Add($"\"{value.Replace("\"", "\"\"")}\"");
                    }
                    rows.Add(string.Join(",", row));
                }
            }

            return string.Join("\n", rows);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to transform JSON to CSV");
            throw;
        }
    }

    public async Task<bool> ValidateMessageFormatAsync(object message, string expectedFormat)
    {
        try
        {
            return await Task.Run(() =>
            {
                return expectedFormat.ToUpper() switch
                {
                    "JSON" => ValidateJsonFormat(message),
                    "XML" => ValidateXmlFormat(message),
                    "CSV" => ValidateCsvFormat(message),
                    _ => false
                };
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate message format: {ExpectedFormat}", expectedFormat);
            return false;
        }
    }

    private bool ValidateJsonFormat(object message)
    {
        try
        {
            var jsonString = message.ToString() ?? string.Empty;
            JsonDocument.Parse(jsonString);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private bool ValidateXmlFormat(object message)
    {
        try
        {
            var xmlString = message.ToString() ?? string.Empty;
            XDocument.Parse(xmlString);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private bool ValidateCsvFormat(object message)
    {
        try
        {
            var csvString = message.ToString() ?? string.Empty;
            var lines = csvString.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            return lines.Length > 0 && lines[0].Contains(',');
        }
        catch
        {
            return false;
        }
    }

    public async Task<object> TransformRequestAsync(object request, string sourceFormat, string targetFormat)
    {
        return await TransformMessageAsync(request, sourceFormat, targetFormat);
    }

    public async Task<object> TransformResponseAsync(object response, string sourceFormat, string targetFormat)
    {
        return await TransformMessageAsync(response, sourceFormat, targetFormat);
    }

    public async Task<TransformationRule> GetTransformationRuleAsync(string sourceFormat, string targetFormat)
    {
        var ruleKey = $"{sourceFormat.ToUpper()}-{targetFormat.ToUpper()}";
        
        lock (_rulesLock)
        {
            if (_transformationRules.TryGetValue(ruleKey, out var rule))
            {
                return rule;
            }
        }
        
        return new TransformationRule
        {
            SourceFormat = sourceFormat,
            TargetFormat = targetFormat,
            IsActive = false
        };
    }

    public async Task<bool> AddTransformationRuleAsync(TransformationRule rule)
    {
        try
        {
            var ruleKey = $"{rule.SourceFormat.ToUpper()}-{rule.TargetFormat.ToUpper()}";
            
            lock (_rulesLock)
            {
                _transformationRules[ruleKey] = rule;
            }

            _logger.LogInformation("Added transformation rule: {SourceFormat} -> {TargetFormat}", 
                rule.SourceFormat, rule.TargetFormat);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add transformation rule");
            return false;
        }
    }

    public async Task<IEnumerable<TransformationRule>> GetAllTransformationRulesAsync()
    {
        lock (_rulesLock)
        {
            return _transformationRules.Values.ToList();
        }
    }

    public async Task<TransformationMetrics> GetTransformationMetricsAsync()
    {
        lock (_metricsLock)
        {
            return new TransformationMetrics
            {
                TotalTransformations = _metrics.TotalTransformations,
                SuccessfulTransformations = _metrics.SuccessfulTransformations,
                FailedTransformations = _metrics.FailedTransformations,
                AverageTransformationTime = _metrics.AverageTransformationTime,
                TransformationsByFormat = new Dictionary<string, int>(_metrics.TransformationsByFormat),
                LastReset = _metrics.LastReset
            };
        }
    }

    private void UpdateMetrics(string sourceFormat, string targetFormat, TimeSpan duration, bool success)
    {
        lock (_metricsLock)
        {
            _metrics.TotalTransformations++;
            
            if (success)
            {
                _metrics.SuccessfulTransformations++;
            }
            else
            {
                _metrics.FailedTransformations++;
            }

            var formatKey = $"{sourceFormat}-{targetFormat}";
            if (_metrics.TransformationsByFormat.ContainsKey(formatKey))
            {
                _metrics.TransformationsByFormat[formatKey]++;
            }
            else
            {
                _metrics.TransformationsByFormat[formatKey] = 1;
            }

            // Update average transformation time
            var totalTime = _metrics.AverageTransformationTime * (_metrics.SuccessfulTransformations - 1) + duration.TotalMilliseconds;
            _metrics.AverageTransformationTime = totalTime / _metrics.SuccessfulTransformations;
        }
    }
} 