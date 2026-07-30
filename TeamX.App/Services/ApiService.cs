using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using TeamX.Core.Constants;
using TeamX.Shared.DTOs;

namespace TeamX.App.Services;

public sealed class ApiService : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly HttpClient _httpClient;
    private readonly bool _ownsClient;

    public ApiService()
        : this(CreateDefaultClient(), ownsClient: true)
    {
    }

    public ApiService(HttpClient httpClient)
        : this(httpClient, ownsClient: false)
    {
    }

    private ApiService(HttpClient httpClient, bool ownsClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _ownsClient = ownsClient;
    }

    private static HttpClient CreateDefaultClient()
    {
        var client = new HttpClient
        {
            // URL da API em produção
            BaseAddress = new Uri("https://teamx-api.onrender.com/"),
            Timeout = TimeSpan.FromSeconds(45)
        };

        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        client.DefaultRequestHeaders.UserAgent.ParseAdd(
            $"TeamX/{SystemConstants.CurrentVersion}");

        return client;
    }

    public async Task<bool> SendHeartbeatAsync(
        string token,
        string hardwareFingerprint,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(hardwareFingerprint))
            return false;

        try
        {
            var request = new
            {
                Token = token,
                HardwareFingerprint = hardwareFingerprint,
                ComputerName = Environment.MachineName
            };

            using var response = await _httpClient.PostAsJsonAsync(
                "api/license/heartbeat",
                request,
                JsonOptions,
                ct);

            if (!response.IsSuccessStatusCode)
                return false;

            var body = await response.Content.ReadFromJsonAsync<HeartbeatApiResponse>(JsonOptions, ct);
            return body?.Success == true;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return false;
        }
    }

    public async Task<ActivateResponse> ActivateLicenseAsync(
        SecureActivateRequest request,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            using var response = await _httpClient.PostAsJsonAsync(
                "api/license/activate",
                request,
                JsonOptions,
                ct);

            var payload = await ReadJsonOrNullAsync<ActivateResponse>(response, ct);
            if (payload is not null)
                return payload;

            return new ActivateResponse
            {
                Success = false,
                Message = response.IsSuccessStatusCode
                    ? "Resposta inválida do servidor."
                    : FriendlyHttpError(response.StatusCode)
            };
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            return new ActivateResponse
            {
                Success = false,
                Message = "Tempo de conexão esgotado. Tente novamente."
            };
        }
        catch (HttpRequestException)
        {
            return new ActivateResponse
            {
                Success = false,
                Message = "Sem conexão com o servidor. Verifique sua internet."
            };
        }
        catch (Exception)
        {
            return new ActivateResponse
            {
                Success = false,
                Message = "Erro inesperado ao ativar a licença."
            };
        }
    }

    public Task<TokenValidationResponse> ValidateTokenAsync(
        string token,
        string hardwareFingerprint,
        CancellationToken ct = default)
        => ValidateTokenAsync(token, hardwareFingerprint, executableHash: null, ct);

    public async Task<TokenValidationResponse> ValidateTokenAsync(
        string token,
        string hardwareFingerprint,
        string? executableHash,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(hardwareFingerprint))
        {
            return new TokenValidationResponse
            {
                Success = false,
                IsValid = false,
                Message = "Token ou dispositivo inválido."
            };
        }

        try
        {
            var request = new
            {
                Token = token,
                HardwareFingerprint = hardwareFingerprint,
                ExecutableHash = executableHash
            };

            using var response = await _httpClient.PostAsJsonAsync(
                "api/license/validate-token",
                request,
                JsonOptions,
                ct);

            var payload = await ReadJsonOrNullAsync<TokenValidationResponse>(response, ct);
            if (payload is not null)
                return payload;

            return new TokenValidationResponse
            {
                Success = false,
                IsValid = false,
                Message = response.IsSuccessStatusCode
                    ? "Resposta inválida do servidor."
                    : FriendlyHttpError(response.StatusCode)
            };
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            return new TokenValidationResponse
            {
                Success = false,
                IsValid = false,
                Message = "Tempo de conexão esgotado."
            };
        }
        catch (HttpRequestException)
        {
            return new TokenValidationResponse
            {
                Success = false,
                IsValid = false,
                Message = "Sem conexão com o servidor."
            };
        }
        catch (Exception)
        {
            return new TokenValidationResponse
            {
                Success = false,
                IsValid = false,
                Message = "Erro ao validar licença."
            };
        }
    }

    private static async Task<T?> ReadJsonOrNullAsync<T>(
        HttpResponseMessage response,
        CancellationToken ct) where T : class
    {
        try
        {
            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            if (stream is null || stream.Length == 0)
                return null;

            return await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions, ct);
        }
        catch (JsonException)
        {
            return null;
        }
        catch (NotSupportedException)
        {
            // stream sem Length em alguns handlers
            try
            {
                var text = await response.Content.ReadAsStringAsync(ct);
                if (string.IsNullOrWhiteSpace(text))
                    return null;
                return JsonSerializer.Deserialize<T>(text, JsonOptions);
            }
            catch
            {
                return null;
            }
        }
    }

    private static string FriendlyHttpError(System.Net.HttpStatusCode statusCode)
        => statusCode switch
        {
            System.Net.HttpStatusCode.TooManyRequests => "Muitas tentativas. Aguarde um momento.",
            System.Net.HttpStatusCode.Unauthorized => "Não autorizado.",
            System.Net.HttpStatusCode.Forbidden => "Acesso negado.",
            System.Net.HttpStatusCode.NotFound => "Serviço indisponível.",
            >= System.Net.HttpStatusCode.InternalServerError => "Erro no servidor. Tente mais tarde.",
            _ => "Não foi possível concluir a operação."
        };

    public void Dispose()
    {
        if (_ownsClient)
            _httpClient.Dispose();
    }

    private sealed class HeartbeatApiResponse
    {
        public bool Success { get; set; }
    }
}