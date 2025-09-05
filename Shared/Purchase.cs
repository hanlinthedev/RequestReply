namespace Shared;

public class PurchaseRequest
{
   public string ProductId { get; set; } = string.Empty;
   public int Quantity { get; set; } = 0;
}

public class PurchaseResponse
{
   public bool IsSuccess { get; set; }
   public string StatusMessage { get; set; } = string.Empty;
   public string OrderId { get; set; } = string.Empty;
}