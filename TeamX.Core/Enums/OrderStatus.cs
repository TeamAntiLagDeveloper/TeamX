namespace TeamX.Core.Enums;

/// <summary>
/// Espelho dos valores de string em OrderStatuses.
/// </summary>
public enum OrderStatus
{
    Pending = 0,
    Paid = 1,
    Cancelled = 2,
    Refunded = 3,
    Failed = 4
}