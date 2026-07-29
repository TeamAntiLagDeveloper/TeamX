namespace TeamX.Core.Enums;

/// <summary>
/// Representa o status de um pedido (Order).
/// </summary>
public enum OrderStatus
{
    /// <summary>
    /// Pedido criado, aguardando pagamento.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Pagamento confirmado com sucesso.
    /// </summary>
    Paid = 1,

    /// <summary>
    /// Pedido cancelado (antes ou depois do pagamento).
    /// </summary>
    Cancelled = 2,

    /// <summary>
    /// Valor do pedido foi reembolsado.
    /// </summary>
    Refunded = 3
}