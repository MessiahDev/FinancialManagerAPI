using AutoMapper;
using FinancialManagerAPI.DTOs.CategoryDTOs;
using FinancialManagerAPI.DTOs.DebtDTOs;
using FinancialManagerAPI.DTOs.ExpenseDTOs;
using FinancialManagerAPI.DTOs.RevenueDTOs;
using FinancialManagerAPI.DTOs.UserDTOs;
using FinancialManagerAPI.Models;

namespace FinancialManagerAPI.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserDto>().ReverseMap();
            CreateMap<RegisterUserDto, User>();
            CreateMap<UpdateUserDto, User>();

            CreateMap<Expense, ExpenseDto>().ReverseMap();
            CreateMap<CreateExpenseDto, Expense>().ReverseMap();
            CreateMap<UpdateExpenseDto, Expense>().ReverseMap();

            CreateMap<Debt, DebtDto>().ReverseMap();
            CreateMap<CreateDebtDto, Debt>().ReverseMap();
            CreateMap<UpdateDebtDto, Debt>().ReverseMap();

            CreateMap<Category, CategoryDto>().ReverseMap();
            CreateMap<CreateCategoryDto, Category>().ReverseMap();
            CreateMap<UpdateCategoryDto, Category>().ReverseMap();

            CreateMap<Revenue, RevenueDto>().ReverseMap();
            CreateMap<CreateRevenueDto, Revenue>().ReverseMap();
            CreateMap<UpdateRevenueDto, Revenue>().ReverseMap();
        }
    }
}
