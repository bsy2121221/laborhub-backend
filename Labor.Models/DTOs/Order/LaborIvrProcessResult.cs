namespace Labor.Models.DTOs.Order;

public class LaborIvrProcessResult
{
    public bool Success { get; set; }
    public int? OrderId { get; set; }
    public int? LaborId { get; set; }
}
