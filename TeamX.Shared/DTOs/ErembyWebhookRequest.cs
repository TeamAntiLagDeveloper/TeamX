namespace TeamX.Shared.DTOs;

public class ErembyWebhookRequest
{
    public string Event_Name { get; set; } = "";

    public ErembyOrder Order { get; set; } = new();

    public List<ErembyItem> Items { get; set; } = new();
}


public class ErembyOrder
{
    public string Id { get; set; } = "";

    public string Status { get; set; } = "";

    public string Transaction_Id { get; set; } = "";

    public ErembyCustomer Customer { get; set; } = new();
}


public class ErembyCustomer
{
    public string Name { get; set; } = "";

    public string Email { get; set; } = "";

    public string Document { get; set; } = "";
}


public class ErembyItem
{
    public long Variant_Id { get; set; }

    public string Name { get; set; } = "";

    public decimal Unit_Price { get; set; }
}