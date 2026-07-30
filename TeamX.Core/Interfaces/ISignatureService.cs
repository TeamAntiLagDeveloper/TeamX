using TeamX.Shared.DTOs;

namespace TeamX.Core.Interfaces;

public interface ISignatureService
{
    string GenerateSignature(SecureActivateRequest request, string secret);

    bool ValidateSignature(SecureActivateRequest request, string secret);
}