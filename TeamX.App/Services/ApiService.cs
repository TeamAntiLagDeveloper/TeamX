using System.Text;
using System.Text.Json;
using TeamX.Shared.DTOs;

namespace TeamX.App.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;

    public ApiService()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://localhost:7291/"),
            Timeout = TimeSpan.FromSeconds(10)
        };
    }

    public async Task<ActivateResponse> ActivateLicenseAsync(SecureActivateRequest request)
    {
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("api/license/activate", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<ActivateResponse>(responseContent,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? new ActivateResponse { Success = false, Message = "Erro de comunicação" };
    }

    public async Task<TokenValidationResponse> ValidateTokenAsync(string token, string hardwareFingerprint)
    {
        var request = new TokenValidationRequest
        {
            Token = token,
            HardwareFingerprint = hardwareFingerprint
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("api/license/validate-token", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<TokenValidationResponse>(responseContent,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? new TokenValidationResponse { Success = false };
    }
}