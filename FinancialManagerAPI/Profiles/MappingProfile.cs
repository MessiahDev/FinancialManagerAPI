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
            CreateMap<Debt, DebtDto>().ReverseMap();
            CreateMap<Category, CategoryDto>().ReverseMap();
            CreateMap<Revenue, RevenueDto>().ReverseMap();
        }
    }
}
