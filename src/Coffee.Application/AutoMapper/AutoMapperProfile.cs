using AutoMapper;
using Coffee.Application.DTOs;
using Coffee.Domain.Entities;

namespace Coffee.Application.AutoMapper;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<Coffee.Domain.Entities.Coffee, CoffeeDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null));

        CreateMap<CreateCoffeeDto, Coffee.Domain.Entities.Coffee>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.Ingredients, opt => opt.Ignore());

        CreateMap<UpdateCoffeeDto, Coffee.Domain.Entities.Coffee>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.Ingredients, opt => opt.Ignore());

        CreateMap<Category, CategoryDto>();
        CreateMap<CreateCategoryDto, Category>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Coffees, opt => opt.Ignore());
        CreateMap<UpdateCategoryDto, Category>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Coffees, opt => opt.Ignore());

        CreateMap<Ingredient, IngredientDto>();
        CreateMap<CreateIngredientDto, Ingredient>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Coffees, opt => opt.Ignore());
        CreateMap<UpdateIngredientDto, Ingredient>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Coffees, opt => opt.Ignore());
    }
}