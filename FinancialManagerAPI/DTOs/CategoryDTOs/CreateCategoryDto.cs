namespace FinancialManagerAPI.DTOs.CategoryDTOs
{
    public class CreateCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public int UserId { get; set; }
    }
}
