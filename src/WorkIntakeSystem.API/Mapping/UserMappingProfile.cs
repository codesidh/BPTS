using AutoMapper;
using WorkIntakeSystem.Core.Entities;
using WorkIntakeSystem.Core.DTOs;

namespace WorkIntakeSystem.API.Mapping;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department.Name))
            .ForMember(dest => dest.BusinessVerticalName, opt => opt.MapFrom(src => src.BusinessVertical.Name))
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()));

        CreateMap<UserDto, User>()
            .ForMember(dest => dest.Department, opt => opt.Ignore())
            .ForMember(dest => dest.BusinessVertical, opt => opt.Ignore())
            .ForMember(dest => dest.SubmittedRequests, opt => opt.Ignore())
            .ForMember(dest => dest.PriorityVotes, opt => opt.Ignore())
            .ForMember(dest => dest.AuditTrails, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedDate, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordSalt, opt => opt.Ignore());
    }
} 