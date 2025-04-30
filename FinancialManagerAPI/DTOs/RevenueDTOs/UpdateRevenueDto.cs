namespace FinancialManagerAPI.DTOs.RevenueDTOs
{
    public class UpdateRevenueDto
    {
        public string? Description { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public int? UserId { get; set; }
    }
}
