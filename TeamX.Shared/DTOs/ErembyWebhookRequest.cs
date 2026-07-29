namespace TeamX.Shared.DTOs;

/// <summary>
/// Payload recebido do webhook da Eremby.
/// </summary>
public class ErembyWebhookRequest
{
    public string Event_Name { get; set; } = string.Empty;
    public ErembyOrder Order { get; set; } = new();
    public List<ErembyItem> Items { get; set; } = new();
}

public class ErembyOrder
{
    public string Id { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Transaction_Id { get; set; } = string.Empty;
    public ErembyCustomer Customer { get; set; } = new();
}

public class ErembyCustomer
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Document { get; set; } = string.Empty;
}

public class ErembyItem
{
    public long Variant_Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Unit_Price { get; set; }
}