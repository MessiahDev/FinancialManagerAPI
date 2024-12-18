namespace FinancialManagerAPI.DTOs.RevenueDTOs
{
    public class CreateRevenueDto
    {
        public string? Description { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string? Category { get; set; }
    }
}
