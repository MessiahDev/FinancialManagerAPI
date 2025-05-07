namespace FinancialManagerAPI.DTOs.DebtDTOs
{
    public class DebtDto
    {
        public int Id { get; set; }
        public string? Description { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public string? Creditor { get; set; }
        public bool IsPaid { get; set; }
    }
}
