using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ShowTracker.Core.Interfaces;
using ShowTracker.Core.Models;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace ShowTracker.Infrastructure.Clients;

public class TvDbClient(ILogger<TvDbClient> logger, HttpClient httpClient, string apiKey) : ITvDbClient
{
    private string? token;
    
    public async Task<TvDbResponse<T>> GetDataAsync<T>(string endpoint, CancellationToken cancellationToken = default)
    {
        if (!IsTokenValid())
        {
            token = await RefreshTokenAsync(cancellationToken);
        }
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        var response = await httpClient.GetAsync(endpoint, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonSerializer.DeserializeAsync<TvDbResponse<T>>(stream, cancellationToken: cancellationToken) ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    public async Task<string> RefreshTokenAsync(CancellationToken cancellationToken = default)
    {
        var body = JsonConvert.SerializeObject(new { apiKey });
        var response = await httpClient.PostAsync("login", new StringContent(body, Encoding.UTF8, "application/json"), cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Failed to refresh token | {value}", content);
            throw new Exception("Failed to refresh token");
        }

        var parsedResponse = JsonConvert.DeserializeObject<TvDbResponse<TvDbToken>>(content);

        if (parsedResponse is { Status: "success" }) return parsedResponse.Data!.Token;
        
        logger.LogError("Failed to refresh token | {value}", content);
        throw new Exception("Failed to refresh token");
    }

    public bool IsTokenValid()
    {
        if (token == null)
        {
            return false;
        }
        
        var jwtHandler = new JwtSecurityTokenHandler();
        if (!jwtHandler.CanReadToken(token))
        {
            logger.LogDebug("Token is invalid");
            return false;
        }

        var decodedToken = jwtHandler.ReadJwtToken(token);
        var exp = decodedToken.ValidTo;
        
        return exp > DateTime.UtcNow.AddMinutes(2);
    }
}