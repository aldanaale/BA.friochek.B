namespace BA.Backend.Application.Cliente.DTOs;

public class ClientOrderDto
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DispatchDate { get; set; }
    public List<ClientOrderItemDto> Items { get; set; } = new();
}

public class ClientOrderSummaryDto
{
    public Guid OrderId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DispatchDate { get; set; }
}

public class ClientOrderItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }
}
