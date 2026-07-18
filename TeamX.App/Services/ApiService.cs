using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TeamX.Shared.DTOs;

namespace TeamX.App.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        private const string ApiBaseUrl = "https://teamx-api.onrender.com/";

        public ApiService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(ApiBaseUrl),
                Timeout = TimeSpan.FromSeconds(60)
            };
        }

        public async Task<ActivateResponse> ActivateLicenseAsync(SecureActivateRequest request)
        {
            try
            {
                var json = JsonSerializer.Serialize(request);

                using var content = new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync(
                    "api/license/activate",
                    content);

                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new ActivateResponse
                    {
                        Success = false,
                        Message = $"Erro HTTP {(int)response.StatusCode}: {responseContent}"
                    };
                }

                return JsonSerializer.Deserialize<ActivateResponse>(
                           responseContent,
                           new JsonSerializerOptions
                           {
                               PropertyNameCaseInsensitive = true
                           })
                       ?? new ActivateResponse
                       {
                           Success = false,
                           Message = "Resposta inválida da API."
                       };
            }
            catch (TaskCanceledException)
            {
                return new ActivateResponse
                {
                    Success = false,
                    Message = "Tempo de conexão esgotado."
                };
            }
            catch (Exception ex)
            {
                return new ActivateResponse
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<TokenValidationResponse> ValidateTokenAsync(
            string token,
            string hardwareFingerprint)
        {
            try
            {
                var request = new TokenValidationRequest
                {
                    Token = token,
                    HardwareFingerprint = hardwareFingerprint
                };

                var json = JsonSerializer.Serialize(request);

                using var content = new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync(
                    "api/license/validate-token",
                    content);

                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new TokenValidationResponse
                    {
                        Success = false,
                        Message = $"Erro HTTP {(int)response.StatusCode}: {responseContent}"
                    };
                }

                return JsonSerializer.Deserialize<TokenValidationResponse>(
                           responseContent,
                           new JsonSerializerOptions
                           {
                               PropertyNameCaseInsensitive = true
                           })
                       ?? new TokenValidationResponse
                       {
                           Success = false,
                           Message = "Resposta inválida da API."
                       };
            }
            catch (TaskCanceledException)
            {
                return new TokenValidationResponse
                {
                    Success = false,
                    Message = "Tempo de conexão esgotado."
                };
            }
            catch (Exception ex)
            {
                return new TokenValidationResponse
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }
    }
}