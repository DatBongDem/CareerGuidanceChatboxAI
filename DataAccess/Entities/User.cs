using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Entities
{
    public class User
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime DOB { get; set; }

        public DateTime CreateAt { get; set; }

        public DateTime UpdateAt { get; set; }

        public Guid? UpdatedBy { get; set; }

        public string AvatarUrl { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public string? Token { get; set; }
        public DateTime? LastLoginTime { get; set; }

        [ForeignKey("Role")]
        public Guid RoleId { get; set; }
        public Role? Role { get; set; }


        [ForeignKey("Plan")]
        public Guid? PlanId { get; set; }
        public Plan? Plan { get; set; } 

        public DateTime DatePlanRegistration { get; set; }
    }
}