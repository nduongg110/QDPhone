namespace QDPhone.Web.Models.Entities;

public class InventoryTransaction
{
    public int Id { get; set; }
    public int ProductVariantId { get; set; }
    public int DeltaQuantity { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

