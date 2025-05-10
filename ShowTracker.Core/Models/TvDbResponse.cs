using System.Text.Json.Serialization;

namespace ShowTracker.Core.Models;

public class TvDbResponse<T>
{
    [JsonPropertyName("status")]
    public required string Status { get; init; }
    
    [JsonPropertyName("message")]
    public string? Message { get; init; }
    
    [JsonPropertyName("data")]
    public required T? Data { get; init; }
}