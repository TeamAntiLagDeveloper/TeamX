using TeamX.Data.Context;
using TeamX.Core.Interfaces;

namespace TeamX.Core.Services;

public class HeartbeatService : IHeartbeatService
{
    private readonly ApplicationDbContext _context;
    private readonly ITokenService _tokenService;

    public HeartbeatService(
        ApplicationDbContext context,
        ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    public async Task<bool> RecordHeartbeatAsync(string token, string hardwareFingerprint)
    {
        var validation = await _tokenService.ValidateTokenAsync(token, hardwareFingerprint);
        if (!validation.IsValid)
            return false;

        // Atualização futura do LastSeen pode ser implementada aqui
        return true;
    }

    public Task<bool> IsDeviceActiveAsync(string token, string hardwareFingerprint)
    {
        // Implementação básica por enquanto
        return Task.FromResult(true);
    }
}