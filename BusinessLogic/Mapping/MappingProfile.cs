using AutoMapper;
using BusinessLogic.DTOs;
using BusinessLogic.DTOs.Plan;
using BusinessLogic.DTOs.Role;
using BusinessLogic.DTOs.User;
using DataAccess.Entities;

namespace BusinessLogic
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Source -> Target
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role != null ? src.Role.Name : string.Empty))
                .ForMember(dest => dest.PlanName, opt => opt.MapFrom(src => src.Plan != null ? src.Plan.Name : null));

            CreateMap<CreateUserDto, User>();
            CreateMap<UpdateUserDto, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null)); // Ignore null properties during update

            // Role mappings
            CreateMap<Role, RoleDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.RoleId))
                .ForMember(dest => dest.UsersCount, opt => opt.MapFrom(src => src.Users.Count));
            CreateMap<CreateRoleDto, Role>();
            CreateMap<UpdateRoleDto, Role>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Plan mappings
            CreateMap<Plan, PlanDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.PlanId))
                .ForMember(dest => dest.UsersCount, opt => opt.MapFrom(src => src.Users.Count));
            CreateMap<CreatePlanDto, Plan>();
            CreateMap<UpdatePlanDto, Plan>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
