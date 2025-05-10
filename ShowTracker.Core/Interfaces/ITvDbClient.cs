using ShowTracker.Core.Models;

namespace ShowTracker.Core.Interfaces;

public interface ITvDbClient
{
    Task<TvDbResponse<T>> GetDataAsync<T>(string endpoint, CancellationToken cancellationToken = default);
    
    Task<string> RefreshTokenAsync(CancellationToken cancellationToken = default);

    bool IsTokenValid();
}