using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.DTOs.User
{
    public class MeResponseDto
    {
        public Guid UserId { get; set; }

        public string Username { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string Gender { get; set; }

        public string PhoneNumber { get; set; } = string.Empty;

        public DateTime DOB { get; set; }

        public string AvatarUrl { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public DateTime? LastLoginTime { get; set; }

        public string CurrentPlan { get; set; } = string.Empty;
    }
}
