using DataAccess.Shares;
using System;

namespace BusinessLogic.DTOs.User
{
    public class UserDto
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Gender { get; set; } = StatusEnum.Other;
        public DateTime DOB { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime UpdateAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public string AvatarUrl { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public Guid RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public Guid? PlanId { get; set; }
        public string? PlanName { get; set; }
        public DateTime DatePlanRegistration { get; set; }
        public string? Token { get; set; } 
        public DateTime? LastLoginTime { get; set; }
    }
}
