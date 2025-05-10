using System.Text.Json.Serialization;

namespace ShowTracker.Core.Models;

public class TvDbToken
{
    [JsonPropertyName("token")]
    public required string Token { get; init; }
}