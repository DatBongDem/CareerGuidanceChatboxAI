using System;
using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs.User
{
    public class UpdateUserDto
    {
        [StringLength(50)]
        public string? Username { get; set; }

        [StringLength(255)]
        public string? Address { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        public DateTime? DOB { get; set; }

        //public string? AvatarUrl { get; set; }

        public string Gender { get; set; }

        public bool? IsActive { get; set; }

        public Guid? RoleId { get; set; }

    }
}
