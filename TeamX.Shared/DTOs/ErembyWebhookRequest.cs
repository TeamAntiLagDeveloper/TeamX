using System.Text.Json.Serialization;

namespace TeamX.Shared.DTOs;

public class ErembyWebhookRequest
{
    [JsonPropertyName("event_name")]
    public string Event_Name { get; set; } = string.Empty;

    [JsonPropertyName("order")]
    public ErembyOrder? Order { get; set; }

    [JsonPropertyName("items")]
    public List<ErembyItem> Items { get; set; } = new();
}

public class ErembyOrder
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("transaction_id")]
    public string Transaction_Id { get; set; } = string.Empty;

    [JsonPropertyName("customer")]
    public ErembyCustomer? Customer { get; set; }
}

public class ErembyCustomer
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("document")]
    public string Document { get; set; } = string.Empty;
}

public class ErembyItem
{
    [JsonPropertyName("variant_id")]
    public long Variant_Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("unit_price")]
    public decimal Unit_Price { get; set; }
}