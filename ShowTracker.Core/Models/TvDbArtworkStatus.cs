using System.Text.Json.Serialization;

namespace ShowTracker.Core.Models;

public class TvDbArtworkStatus
{
    [JsonPropertyName("id")]
    public required int Id { get; init; }
    
    [JsonPropertyName("name")]
    public required string Name { get; init; }
}