using AutoMapper;
using Coffee.Application.DTOs;
using Coffee.Domain.Entities;

namespace Coffee.Application.AutoMapper;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        // Coffee mappings
        CreateMap<Coffee.Domain.Entities.Coffee, CoffeeDto>();

        CreateMap<CreateCoffeeDto, Coffee.Domain.Entities.Coffee>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        CreateMap<UpdateCoffeeDto, Coffee.Domain.Entities.Coffee>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
    }
}