public class ErembyWebhookRequest
{
    public string Event { get; set; } = string.Empty;

    public string Store_Id { get; set; } = string.Empty;

    public ErembyData Data { get; set; } = new();
}

public class ErembyData
{
    public string Id { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Currency { get; set; } = string.Empty;

    public ErembyCustomer Customer { get; set; } = new();

    public ErembyProduct Product { get; set; } = new();
}

public class ErembyCustomer
{
    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Document { get; set; } = string.Empty;
}

public class ErembyProduct
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }
}