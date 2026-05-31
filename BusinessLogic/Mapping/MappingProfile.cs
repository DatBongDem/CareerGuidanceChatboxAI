using AutoMapper;
using BusinessLogic.DTOs;
using BusinessLogic.DTOs.ChatAI.Question;
using BusinessLogic.DTOs.ChatAI.QuestionCategory;
using BusinessLogic.DTOs.ChatAI.QuestionOption;
using BusinessLogic.DTOs.Plan;
using BusinessLogic.DTOs.Role;
using BusinessLogic.DTOs.User;
using DataAccess.Entities;
using DataAccess.Entities.ChatAI;
using DataAccess.Shares;

namespace BusinessLogic
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // =========================
            // User mappings
            // =========================
            CreateMap<User, UserDto>()
                .ForMember(
                    dest => dest.RoleName,
                    opt => opt.MapFrom(
                        src => src.Role != null
                            ? src.Role.Name
                            : string.Empty
                    )
                );

            CreateMap<User, MeResponseDto>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role != null ? src.Role.Name : "User"));

            CreateMap<CreateUserDto, User>();

            CreateMap<UpdateUserDto, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForAllMembers(opts =>
                    opts.Condition((src, dest, srcMember) => srcMember != null)
                );

            CreateMap<UpdateProfileDto, User>()
                .ForAllMembers(opts =>
                    opts.Condition((src, dest, srcMember) => srcMember != null)
                );

            // =========================
            // Role mappings
            // =========================
            CreateMap<Role, RoleDto>()
                .ForMember(dest => dest.Id,
                    opt => opt.MapFrom(src => src.RoleId))
                .ForMember(dest => dest.UsersCount,
                    opt => opt.MapFrom(src => src.Users.Count));

            CreateMap<CreateRoleDto, Role>();

            CreateMap<UpdateRoleDto, Role>()
                .ForAllMembers(opts =>
                    opts.Condition((src, dest, srcMember) => srcMember != null)
                );

            // =========================
            // Plan mappings
            // =========================
            CreateMap<Plan, PlanDto>()
                .ForMember(dest => dest.Id,
                    opt => opt.MapFrom(src => src.PlanId));

            CreateMap<CreatePlanDto, Plan>();

            CreateMap<UpdatePlanDto, Plan>()
                .ForAllMembers(opts =>
                    opts.Condition((src, dest, srcMember) => srcMember != null)
                );

            CreateMap<PlanHistory, PlanHistoryDto>()
     .ForMember(dest => dest.Status,
         opt => opt.MapFrom(src =>
             DateTime.UtcNow > src.ExpiryDate
                 ? StatusEnum.Expired
                 : StatusEnum.Active
         ));

            // =========================
            // Chat AI mappings
            // =========================
            CreateMap<QuestionCategory, QuestionCategoryDto>();
            CreateMap<CreateQuestionCategoryDto, QuestionCategory>();
            CreateMap<UpdateQuestionCategoryDto, QuestionCategory>();

            CreateMap<Question, QuestionDto>();
            CreateMap<CreateQuestionDto, Question>();
            CreateMap<UpdateQuestionDto, Question>();

            CreateMap<QuestionOption, QuestionOptionDto>();
            CreateMap<CreateQuestionOptionDto, QuestionOption>();
            CreateMap<UpdateQuestionOptionDto, QuestionOption>();
        }
    }
}