using System.ComponentModel.DataAnnotations;

namespace ShowTracker.Core.Configuration;

public class TvDbOptions
{
    public const string Name = "TvDb";

    [Required] public required string BaseUrl { get; init; } = null!;

    [Required] public required string ApiKey { get; init; } = null!;
}