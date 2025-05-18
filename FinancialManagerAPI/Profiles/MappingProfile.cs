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
            CreateMap<CreateExpenseDto, Expense>();
            CreateMap<UpdateExpenseDto, Expense>();

            CreateMap<Debt, DebtDto>().ReverseMap();
            CreateMap<CreateDebtDto, Debt>();
            CreateMap<UpdateDebtDto, Debt>();

            CreateMap<Category, CategoryDto>().ReverseMap();
            CreateMap<CreateCategoryDto, Category>();
            CreateMap<UpdateCategoryDto, Category>();

            CreateMap<Revenue, RevenueDto>().ReverseMap();
            CreateMap<CreateRevenueDto, Revenue>();
            CreateMap<UpdateRevenueDto, Revenue>();
        }
    }
}
