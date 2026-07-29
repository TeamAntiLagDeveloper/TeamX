using TeamX.Core.Entities;

namespace TeamX.Core.Interfaces;

/// <summary>
/// Serviço responsável por operações relacionadas a clientes.
/// </summary>
public interface ICustomerService
{
    /// <summary>
    /// Busca um cliente pelo e-mail. Caso não exista, cria um novo registro.
    /// </summary>
    /// <param name="email">E-mail do cliente (obrigatório).</param>
    /// <param name="cancellationToken">Token de cancelamento opcional.</param>
    /// <returns>O cliente existente ou recém-criado.</returns>
    Task<Customer> GetOrCreateAsync(
        string email,
        CancellationToken cancellationToken = default);
}